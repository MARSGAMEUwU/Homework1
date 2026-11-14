using System.Net;
using Server.Framework.Handlers;
namespace Server;

public class HttpServer
{
    private readonly ServerConfig _config;
    private readonly HttpListener _listener = new();

    private readonly string _root = Server.Framework.ServerPaths.StaticRoot;
    private readonly HandlerPipeline _pipeline;

    public HttpServer(ServerConfig config)
    {
        _config = config;
        _pipeline = new HandlerPipeline(new NotFoundHandler(_root))
            .Use(new RouterHandler())
            .Use(new StaticFileHandler(_root));
    }

    public string Address => $"http://{_config.Host}:{_config.Port}/";

    public bool Start()
    {
        if (!Directory.Exists(_root))
        {
            Directory.CreateDirectory(_root);
            Console.WriteLine($"Папка {_root} не найдена, создана пустая.");
        }

        if (!File.Exists(Path.Combine(_root, "login.html")))
            Console.WriteLine("Предупреждение: static/login.html не найден.");

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
            await _pipeline.ExecuteAsync(new HandlerContext(context));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Критическая ошибка при обработке запроса: {ex.Message}");
            try { context.Response.Abort(); } catch { }
        }
    }
}
