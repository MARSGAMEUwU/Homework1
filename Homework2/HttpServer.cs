using System.Net;
using System.Text;

namespace ConsoleApp1
{
    public class HttpServer
    {
        private readonly HttpListener server;
        private readonly ServerConfig config;

        private bool isRunning;

        public string Address =>
            $"http://{config.Host}:{config.Port}/{config.Path}/";

        public HttpServer(ServerConfig config)
        {
            this.config = config;

            server = new HttpListener();
            server.Prefixes.Add(Address);
        }

        public void Start()
        {
            server.Start();
            isRunning = true;

            Console.WriteLine($"HTTP сервер запущен:");
            Console.WriteLine(Address);

            _ = ListenAsync();
        }

        private async Task ListenAsync()
        {
            while (isRunning)
            {
                try
                {
                    HttpListenerContext context = await server.GetContextAsync();

                    _ = HandleRequestAsync(context);
                }
                catch (HttpListenerException)
                {
                    if (isRunning)
                    {
                        Console.WriteLine("Ошибка при получении запроса.");
                    }
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
            }
        }

        private async Task HandleRequestAsync(HttpListenerContext context)
        {
            HttpListenerResponse response = context.Response;

            string htmlFile = "search-engine.html";

            string responseText;

            if (File.Exists(htmlFile))
            {
                responseText = await File.ReadAllTextAsync(htmlFile);
                response.StatusCode = 200;
                response.ContentType = "text/html; charset=utf-8";
            }
            else
            {
                responseText = "Not Found 404";

                response.StatusCode = 404;
                response.ContentType = "text/plain; charset=utf-8";
            }

            byte[] buffer = Encoding.UTF8.GetBytes(responseText);

            response.ContentLength64 = buffer.Length;

            try
            {
                using Stream output = response.OutputStream;

                await output.WriteAsync(buffer);
                await output.FlushAsync();
                Console.WriteLine("Response отпрален полностью");
            }
            catch
            {
                Console.WriteLine("Response недотпрален");
            }
            finally
            {
                response.Close();
            }
        }

        public void Stop()
        {
            if (!isRunning)
                return;

            isRunning = false;

            server.Stop();
            server.Close();

            Console.WriteLine("HTTP сервер остановлен.");
        }
    }
}