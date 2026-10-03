using System.Net;
using System.Text;

namespace Server;

public class HttpServer
{
    private readonly ServerConfig _config;
    private readonly HttpListener _listener = new();

    private readonly string _root = Path.GetFullPath("static");

    public HttpServer(ServerConfig config)
    {
        _config = config;
    }

    public string Address => $"http://{_config.Host}:{_config.Port}/";

    public bool Start()
    {
        if (!Directory.Exists(_root))
        {
            Directory.CreateDirectory(_root);
            Console.WriteLine($"Папка {_root} не найдена, создана пустая.");
        }
        if (!File.Exists(Path.Combine(_root, "index.html")))
        try
        {
            _listener.Prefixes.Add(Address);
            _listener.Start();
        }
        catch (Exception ex) when (ex is HttpListenerException or ArgumentException)
        {
            Console.WriteLine($"Не удалось запустить сервер на {Address}: {ex.Message}");
            return false;
        }

        Console.WriteLine("Сервер начал свою работу");
        Receive();
        return true;
    }

    public void Stop()
    {
        _listener.Stop();
        Console.WriteLine("Сервер завершил свою работу");
    }

    private void Receive()
    {
        try
        {
            _listener.BeginGetContext(ListenerCallback, null);
        }
        catch (Exception ex) when (ex is ObjectDisposedException or InvalidOperationException or HttpListenerException)
        {
        }
    }

    private async void ListenerCallback(IAsyncResult result)
    {
        HttpListenerContext? context = null;

        try
        {
            context = _listener.EndGetContext(result);
        }
        catch (Exception ex) when (ex is ObjectDisposedException or InvalidOperationException or HttpListenerException)
        {
        }

        if (!_listener.IsListening)
            return;

        Receive();

        if (context == null)
            return;

        try
        {
            await HandleRequestAsync(context);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при обработке запроса: {ex.Message}");
            try
            {
                context.Response.StatusCode = 500;
                context.Response.Close();
            }
            catch
            {
                context.Response.Abort();
            }
        }
    }

    private async Task HandleRequestAsync(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        string urlPath = request.Url?.LocalPath ?? "/";
        Console.WriteLine($"Пришел запрос: {urlPath}");

        string target = Path.GetFullPath(Path.Combine(_root, urlPath.TrimStart('/')));

        if (!IsInsideRoot(target))
        {
            await SendNotFoundAsync(response);
            return;
        }

        if (Directory.Exists(target))
        {
            if (!urlPath.EndsWith('/'))
            {
                response.StatusCode = 301;
                response.RedirectLocation = request.Url!.AbsolutePath + "/" + request.Url.Query;
                response.Close();
                return;
            }

            target = Path.Combine(target, "index.html");
        }

        if (!File.Exists(target))
        {
            await SendNotFoundAsync(response);
            return;
        }

        await SendFileAsync(response, target, 200);
        Console.WriteLine("Запрос обработан");
    }

    private bool IsInsideRoot(string fullPath)
    {
        return fullPath.Equals(_root, StringComparison.OrdinalIgnoreCase)
               || fullPath.StartsWith(_root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }

    private static async Task SendFileAsync(HttpListenerResponse response, string filePath, int statusCode)
    {
        byte[] buffer = await File.ReadAllBytesAsync(filePath);

        response.StatusCode = statusCode;
        response.ContentType = Contentypes.Get(filePath);
        response.ContentLength64 = buffer.Length;

        await response.OutputStream.WriteAsync(buffer);
        response.Close();
    }

    private async Task SendNotFoundAsync(HttpListenerResponse response)
    {
        string notFoundPage = Path.Combine(_root, "404.html");

        if (File.Exists(notFoundPage))
        {
            await SendFileAsync(response, notFoundPage, 404);
            return;
        }

        byte[] buffer = Encoding.UTF8.GetBytes("404 Not Found");
        response.StatusCode = 404;
        response.ContentType = "text/plain; charset=utf-8";
        response.ContentLength64 = buffer.Length;

        await response.OutputStream.WriteAsync(buffer);
        response.Close();
    }
}