using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Server.Framework.attribute;

namespace Server.Framework.Handlers;

internal sealed record CommandParameter(
    string Name, Type Type, bool IsContext, bool IsRequired, bool HasDefault, object? DefaultValue);

internal sealed class CommandDescriptor
{
    public required string Route { get; init; }
    public required string HttpMethod { get; init; }
    public required MethodInfo Method { get; init; }
    public required Type Owner { get; init; }
    public required CommandParameter[] Parameters { get; init; }

    public async Task InvokeAsync(HandlerContext context)
    {
        NameValueCollection source = HttpMethod is "GET" or "HEAD" ? context.Query : context.Form;

        if (!TryBind(context, source, out object?[] args, out var errors))
        {
            await context.SendTextAsync("400 Bad Request: " + string.Join("; ", errors), 400);
            return;
        }

        object? instance = Method.IsStatic ? null : Activator.CreateInstance(Owner);
        object? result;

        try
        {
            result = Method.Invoke(instance, args);
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            throw;
        }

        string? text = null;
        if (result is Task<string> taskOfString) text = await taskOfString;
        else if (result is Task task) await task;
        else if (result is string s) text = s;

        if (context.IsResponded)
            return;

        if (text is not null)
            await context.SendTextAsync(text);
        else
            await context.SendBytesAsync(Array.Empty<byte>(), 204, "text/plain; charset=utf-8");
    }

    private bool TryBind(HandlerContext context, NameValueCollection source, out object?[] args, out List<string> errors)
    {
        args = new object?[Parameters.Length];
        errors = new List<string>();

        for (int i = 0; i < Parameters.Length; i++)
        {
            var p = Parameters[i];

            if (p.IsContext)
            {
                args[i] = context;
                continue;
            }

            string? raw = source[p.Name];
            Type target = Nullable.GetUnderlyingType(p.Type) ?? p.Type;

            if (string.IsNullOrEmpty(raw) && target != typeof(string))
                raw = null;

            if (raw is null)
            {
                if (p.HasDefault) args[i] = p.DefaultValue;
                else if (p.IsRequired) errors.Add($"не передан параметр '{p.Name}'");
                else args[i] = null;
                continue;
            }

            if (TryConvert(raw, target, out object? value))
                args[i] = value;
            else
                errors.Add($"параметр '{p.Name}' имеет неверный формат");
        }

        return errors.Count == 0;
    }

    private static bool TryConvert(string raw, Type target, out object? value)
    {
        value = null;

        try
        {
            if (target == typeof(string)) { value = raw; return true; }

            if (target == typeof(bool))
            {
                if (raw.Equals("on", StringComparison.OrdinalIgnoreCase) || raw == "1") { value = true; return true; }
                if (raw == "0") { value = false; return true; }
                if (bool.TryParse(raw, out bool b)) { value = b; return true; }
                return false;
            }

            if (target.IsEnum)
            {
                if (Enum.TryParse(target, raw, ignoreCase: true, out object? e) && Enum.IsDefined(target, e!))
                {
                    value = e;
                    return true;
                }
                return false;
            }

            var converter = TypeDescriptor.GetConverter(target);
            if (!converter.CanConvertFrom(typeof(string)))
                return false;

            value = converter.ConvertFromString(null, CultureInfo.InvariantCulture, raw);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}

internal sealed class CommandRegistry
{
    private static readonly NullabilityInfoContext Nullability = new();

    private readonly Dictionary<string, Dictionary<string, CommandDescriptor>> _routes = new();

    public CommandRegistry(Assembly assembly)
    {
        foreach (Type type in SafeGetTypes(assembly))
        {
            if (!type.IsClass || type.IsAbstract)
                continue;

            foreach (MethodInfo method in type.GetMethods(
                         BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                var command = method.GetCustomAttribute<CommandAttribute>();
                if (command is not null)
                    Register(type, method, command.Route, command.HttpMethod);

                var controller = type.GetCustomAttribute<HttpControllerAttribute>();
                if (controller is null)
                    continue;

                foreach (var verb in method.GetCustomAttributes<HttpVerbAttribute>())
                    Register(type, method, $"/{controller.Name}/{verb.Route}", verb.HttpMethod);
            }
        }
    }

    public int Count => _routes.Values.Sum(v => v.Count);

    public bool TryFind(string route, string httpMethod, out CommandDescriptor? command)
    {
        command = null;
        return _routes.TryGetValue(NormalizeRoute(route), out var byMethod)
               && byMethod.TryGetValue(httpMethod, out command);
    }

    public static string NormalizeRoute(string route)
    {
        route = route.Trim().ToLowerInvariant();
        if (!route.StartsWith('/')) route = "/" + route;
        return route.Length > 1 ? route.TrimEnd('/') : route;
    }

    private void Register(Type type, MethodInfo method, string routeTemplate, string httpMethod)
    {
        string where = $"{type.Name}.{method.Name}";

        bool returnOk = method.ReturnType == typeof(void) || method.ReturnType == typeof(string)
                        || method.ReturnType == typeof(Task) || method.ReturnType == typeof(Task<string>);
        if (!returnOk)
            throw new InvalidOperationException($"Команда {where}: допустимый тип результата void, string, Task или Task<string>.");

        if (!method.IsStatic && type.GetConstructor(Type.EmptyTypes) is null)
            throw new InvalidOperationException($"Команда {where}: у класса нужен публичный конструктор без параметров (или сделайте метод static).");

        var parameters = method.GetParameters().Select(p =>
        {
            if (p.ParameterType.IsByRef)
                throw new InvalidOperationException($"Команда {where}: параметры ref/out не поддерживаются.");

            bool isContext = p.ParameterType == typeof(HandlerContext);
            bool nullable = Nullable.GetUnderlyingType(p.ParameterType) is not null
                            || (!p.ParameterType.IsValueType
                                && Nullability.Create(p).WriteState == NullabilityState.Nullable);

            return new CommandParameter(p.Name!, p.ParameterType, isContext,
                IsRequired: !nullable && !p.HasDefaultValue,
                HasDefault: p.HasDefaultValue,
                DefaultValue: p.HasDefaultValue ? p.DefaultValue : null);
        }).ToArray();

        string route = NormalizeRoute(routeTemplate);

        if (!_routes.TryGetValue(route, out var byMethod))
            _routes[route] = byMethod = new Dictionary<string, CommandDescriptor>();

        if (byMethod.ContainsKey(httpMethod))
            throw new InvalidOperationException($"Дубликат команды {httpMethod} {route} ({where}).");

        byMethod[httpMethod] = new CommandDescriptor
        {
            Route = route,
            HttpMethod = httpMethod,
            Method = method,
            Owner = type,
            Parameters = parameters
        };
    }

    private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
    {
        try { return assembly.GetTypes(); }
        catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t => t is not null)!; }
    }
}
