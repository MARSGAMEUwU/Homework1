namespace Server.Framework.Handlers;


public sealed class HandlerPipeline
{
    private readonly List<Handler> _handlers = new();
    private readonly Handler _terminal;
    private Handler _head;

    public HandlerPipeline(Handler terminal)
    {
        _terminal = terminal;
        _head = terminal;
    }

    public HandlerPipeline Use(Handler handler)
    {
        _handlers.Add(handler);
        Relink();
        return this;
    }

    private void Relink()
    {
        var all = new List<Handler>(_handlers) { _terminal };

        for (int i = 0; i < all.Count; i++)
            all[i].Successor = i + 1 < all.Count ? all[i + 1] : null;

        _head = all[0];
    }

    public async Task ExecuteAsync(HandlerContext context)
    {
        try
        {
            await _head.HandleAsync(context);

            if (!context.IsResponded)
                await context.SendStatusAsync(404, "404 Not Found");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при обработке {context.Method} {context.Path}: {ex}");

            if (!context.IsResponded)
            {
                try { await context.SendStatusAsync(500, "500 Internal Server Error"); }
                catch {}
            }
        }
        finally
        {
            context.ForceClose();
        }
    }
}
