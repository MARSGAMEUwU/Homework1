namespace Server.Framework.Handlers;

public sealed class StaticFileHandler : Handler
{
    private readonly string _root;
    private readonly string _indexFile;

    public StaticFileHandler(string root, string indexFile = "login.html")
    {
        _root = Path.GetFullPath(root);
        _indexFile = indexFile;
    }

    public override async Task HandleAsync(HandlerContext context)
    {
        if (context.Method is not ("GET" or "HEAD"))
        {
            await NextAsync(context);
            return;
        }

        string urlPath = context.Path;
        string target;

        try
        {
            target = Path.GetFullPath(Path.Combine(_root, urlPath.TrimStart('/', '\\')));
        }
        catch (ArgumentException)
        {
            await NextAsync(context);
            return;
        }

        if (!IsInsideRoot(target))
        {
            await NextAsync(context);
            return;
        }

        if (Directory.Exists(target))
        {
            if (!urlPath.EndsWith('/'))
            {
                var url = context.Request.Url!;
                await context.RedirectAsync(url.AbsolutePath + "/" + url.Query, 301);
                return;
            }

            target = Path.Combine(target, _indexFile);
        }

        if (!File.Exists(target))
        {
            await NextAsync(context);
            return;
        }

        await context.SendFileAsync(target);
        Console.WriteLine($"Отдан файл: {urlPath}");
    }

    private bool IsInsideRoot(string fullPath)
    {
        return fullPath.Equals(_root, StringComparison.OrdinalIgnoreCase)
               || fullPath.StartsWith(_root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }
}
