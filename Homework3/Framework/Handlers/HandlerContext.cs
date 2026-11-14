using System.Collections.Specialized;
using System.Net;
using System.Text;

namespace Server.Framework.Handlers;

public sealed class HandlerContext
{
    public HandlerContext(HttpListenerContext listenerContext)
    {
        Listener = listenerContext;
    }

    public HttpListenerContext Listener { get; }
    public HttpListenerRequest Request => Listener.Request;
    public HttpListenerResponse Response => Listener.Response;

    public string Path => Request.Url?.LocalPath ?? "/";

    public string Method => Request.HttpMethod.ToUpperInvariant();

    public NameValueCollection Query { get; internal set; } = new();

    public NameValueCollection Form { get; internal set; } = new();

    public Dictionary<string, object?> Items { get; } = new();

    public bool IsResponded { get; private set; }


    public async Task SendBytesAsync(byte[] body, int statusCode, string contentType, bool writeBody = true)
    {
        EnsureNotResponded();
        IsResponded = true;

        Response.StatusCode = statusCode;
        Response.ContentType = contentType;
        Response.ContentLength64 = body.Length;

        if (writeBody)
            await Response.OutputStream.WriteAsync(body);

        Response.Close();
    }

    public Task SendTextAsync(string text, int statusCode = 200, string contentType = "text/plain; charset=utf-8")
        => SendBytesAsync(Encoding.UTF8.GetBytes(text), statusCode, contentType);

    public async Task SendFileAsync(string filePath, int statusCode = 200)
    {
        byte[] buffer = await File.ReadAllBytesAsync(filePath);
        await SendBytesAsync(buffer, statusCode, Contentypes.Get(filePath), writeBody: Method != "HEAD");
    }

    public Task SendStatusAsync(int statusCode, string? message = null)
        => SendTextAsync(message ?? $"{statusCode}", statusCode);

    public Task RedirectAsync(string location, int statusCode = 302)
    {
        EnsureNotResponded();
        IsResponded = true;

        Response.StatusCode = statusCode;
        Response.RedirectLocation = location;
        Response.ContentLength64 = 0;
        Response.Close();
        return Task.CompletedTask;
    }

    private void EnsureNotResponded()
    {
        if (IsResponded)
            throw new InvalidOperationException("На этот запрос ответ уже был отправлен.");
    }

    internal void ForceClose()
    {
        IsResponded = true;
        try { Response.Close(); }
        catch { try { Response.Abort(); } catch { } }
    }
}
