namespace MiniTemplateEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

public class HtmlTemplateRenderer:IHtmlTemplateRenderer
{
    private static readonly Regex Token =
        new(@"\$\{(?<kind>if|foreach)\((?<head>[^)]*)\)" +
            @"(?<body>(?:[^{}]|\{(?<d>)|\}(?<-d>))*?)(?(d)(?!))\}" +
            @"|\$\{(?<path>[\w.]+)\}",
            RegexOptions.Compiled | RegexOptions.Singleline);

    private static readonly Regex Branch =
        new(@"[{}]|elseif\((?<c>[^)]*)\)|else\b", RegexOptions.Compiled);

    private static readonly Regex Each =
        new(@"^\s*(?<var>\w+)\s+in\s+(?<path>[\w.]+)\s*$", RegexOptions.Compiled);

    private static readonly Regex Condition =
        new(@"^(?<path>[\w.]+)\s*(?<op>!=|>=|<=|=|>|<)\s*(?<val>.+)$", RegexOptions.Compiled);
    public string RenderFromString(string htmlTemplate, object datamodel)
    {
        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        // первый сегмент пути: либо переменная foreach из scope, либо сама модель
        object Resolve(string path, Dictionary<string, object> scope)
        {
            var parts = path.Split('.');
            object current = scope.TryGetValue(parts[0], out var v) ? v : datamodel;

            foreach (var name in parts.Skip(1))
            {
                var type = current.GetType();

                current = type.GetProperty(name, flags)?.GetValue(current)
                       ?? type.GetField(name, flags)?.GetValue(current);

                if (current == null) break;
            }

            return current;
        }

        bool Atom(string expr, Dictionary<string, object> scope)
        {
            var c = Condition.Match(expr.Trim());
            var op = c.Groups["op"].Value;
            var left = Convert.ToString(Resolve(c.Groups["path"].Value, scope), CultureInfo.InvariantCulture);
            var right = c.Groups["val"].Value.Trim();

            if (op == "=" || op == "!=")
            {
                var equal = string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
                return op == "=" ? equal : !equal;
            }

            var cmp = double.Parse(left, CultureInfo.InvariantCulture)
                            .CompareTo(double.Parse(right, CultureInfo.InvariantCulture));

            return op switch
            {
                ">"  => cmp > 0,
                "<"  => cmp < 0,
                ">=" => cmp >= 0,
                _    => cmp <= 0
            };
        }

        bool Eval(string cond, Dictionary<string, object> scope) =>
            cond.Split("||").Any(orPart => orPart.Split("&&").All(a => Atom(a, scope)));

        string Render(string template, Dictionary<string, object> scope)
        {
            return Token.Replace(template, m =>
            {
                if (m.Groups["path"].Success)
                    return Resolve(m.Groups["path"].Value, scope)?.ToString() ?? string.Empty;

                var head = m.Groups["head"].Value;
                var body = m.Groups["body"].Value;

                if (m.Groups["kind"].Value == "foreach")
                {
                    var e = Each.Match(head);
                    var items = (IEnumerable)Resolve(e.Groups["path"].Value, scope);

                    return string.Concat(items.Cast<object>().Select(item =>
                        Render(body, new Dictionary<string, object>(scope) { [e.Groups["var"].Value] = item })));
                }

                // if: режем тело по веткам только на нулевой глубине (вложенные блоки не трогаем)
                var conds = new List<string> { head };
                var texts = new List<string>();
                int pos = 0, depth = 0;

                foreach (Match t in Branch.Matches(body))
                {
                    if (t.Value == "{") depth++;
                    else if (t.Value == "}") depth--;
                    else if (depth == 0)
                    {
                        texts.Add(body[pos..t.Index]);
                        conds.Add(t.Groups["c"].Success ? t.Groups["c"].Value : null); // null = else
                        pos = t.Index + t.Length;
                    }
                }
                texts.Add(body[pos..]);

                for (var i = 0; i < conds.Count; i++)
                    if (conds[i] == null || Eval(conds[i], scope))
                        return Render(texts[i], scope);

                return string.Empty;
            });
        }

        return Render(htmlTemplate, new Dictionary<string, object>());
    }
    public string RenderFromFile(string filePath, object dataModel)
    {
        throw new NotImplementedException();
    }

    public string RenderToFile(string inputFilePath, string outputFilePath, object dataModel)
    {
        throw new NotImplementedException();
    }
}