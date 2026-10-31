namespace Server.Framework.Handlers;

public sealed class NotFoundHandler : Handler
{
    private readonly string _notFoundPage;

    public NotFoundHandler(string root, string pageName = "404.html")
    {
        _notFoundPage = Path.Combine(Path.GetFullPath(root), pageName);
    }

    public override async Task HandleAsync(HandlerContext context)
    {
        Console.WriteLine($"404: {context.Method} {context.Path}");

        if (File.Exists(_notFoundPage))
        {
            await context.SendFileAsync(_notFoundPage, 404);
            return;
        }

        await context.SendTextAsync("404 Not Found", 404);
    }
}