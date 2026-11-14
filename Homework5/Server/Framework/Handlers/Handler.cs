namespace Server.Framework.Handlers;

public abstract class Handler
{
    public Handler? Successor { get; internal set; }

    public abstract Task HandleAsync(HandlerContext context);

    protected Task NextAsync(HandlerContext context)
        => Successor is null ? Task.CompletedTask : Successor.HandleAsync(context);
}