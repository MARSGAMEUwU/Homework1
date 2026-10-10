using System.Collections.Specialized;
using System.Reflection;
using System.Web;

namespace Server.Framework.Handlers;

public sealed class RouterHandler : Handler
{
    private const long MaxFormBytes = 1024 * 1024; // 1 МБ

    private readonly CommandRegistry _commands;
    private readonly Dictionary<string, (string Location, int Status)> _redirects = new();

    public RouterHandler(Assembly? commandsAssembly = null)
    {
        _commands = new CommandRegistry(commandsAssembly ?? Assembly.GetExecutingAssembly());
        Console.WriteLine($"Зарегистрировано команд: {_commands.Count}");
    }

    public RouterHandler Redirect(string from, string to, int statusCode = 302)
    {
        _redirects[CommandRegistry.NormalizeRoute(from)] = (to, statusCode);
        return this;
    }

    public override async Task HandleAsync(HandlerContext context)
    {
        context.Query = HttpUtility.ParseQueryString(context.Request.Url?.Query ?? string.Empty);

        if (!await TryReadFormAsync(context))
        {
            await context.SendStatusAsync(413, "413 Payload Too Large");
            return;
        }

        string route = CommandRegistry.NormalizeRoute(context.Path);

        if (_redirects.TryGetValue(route, out var redirect))
        {
            await context.RedirectAsync(redirect.Location, redirect.Status);
            return;
        }

        if (_commands.TryFind(route, context.Method, out var command))
        {
            await command!.InvokeAsync(context);
            return;
        }

        await NextAsync(context);
    }

    private static async Task<bool> TryReadFormAsync(HandlerContext context)
    {
        var request = context.Request;

        if (context.Method is "GET" or "HEAD" || !request.HasEntityBody)
            return true;

        string? contentType = request.ContentType;
        if (contentType is null || !contentType.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase))
            return true;

        if (request.ContentLength64 > MaxFormBytes)
            return false;

        using var memory = new MemoryStream();
        var buffer = new byte[8192];
        int read;

        while ((read = await request.InputStream.ReadAsync(buffer)) > 0)
        {
            if (memory.Length + read > MaxFormBytes)
                return false;
            memory.Write(buffer, 0, read);
        }

        string body = request.ContentEncoding.GetString(memory.ToArray());
        context.Form = HttpUtility.ParseQueryString(body, request.ContentEncoding);
        return true;
    }
}
