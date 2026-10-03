using System.Text.Json;

namespace Server
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string configFile = "adress.json";
            ServerConfig config;

            
            if (!File.Exists(configFile))
            {
                Console.WriteLine("Файл adress.json не найден.");
                Console.WriteLine("Введите настройки сервера.");

                Console.Write("Host (например, 127.0.0.1): ");
                string? host = Console.ReadLine();

                Console.Write("Port (например, 8888): ");
                string? port = Console.ReadLine();

                Console.Write("Path (например, connection): ");
                string? path = Console.ReadLine();

                config = new ServerConfig
                {
                    Host = string.IsNullOrWhiteSpace(host) ? "127.0.0.1" : host,
                    Port = string.IsNullOrWhiteSpace(port) ? "8888" : port,
                    Path = string.IsNullOrWhiteSpace(path) ? "connection" : path
                };

                string json = JsonSerializer.Serialize(
                    config,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

                await File.WriteAllTextAsync(configFile, json);

                Console.WriteLine("Файл adress.json создан.");
            }
            else
            {
                string json = await File.ReadAllTextAsync(configFile);

                config = JsonSerializer.Deserialize<ServerConfig>(json)
                         ?? new ServerConfig();
            }

            HttpServer server = new HttpServer(config);

            if (!server.Start())
            {
                Console.WriteLine("Нажмите Enter для выхода.");
                Console.ReadLine();
                return;
            }

            Console.WriteLine();
            Console.WriteLine("Сервер запущен.");
            Console.WriteLine($"Адрес: {server.Address}");
            Console.WriteLine();
            Console.WriteLine("Доступные команды:");
            Console.WriteLine("stop - остановить сервер");
            Console.WriteLine("address - изменить адрес сервера");
            Console.WriteLine();

            while (true)
            {
                string? command = Console.ReadLine();

                if (command == null)
                    continue;

                command = command.ToLower().Trim();

                if (command == "stop")
                {
                    server.Stop();
                    break;
                }

                if (command == "address")
                {
                    Console.WriteLine("Изменение адреса сервера.");

                    Console.Write($"Host [{config.Host}]: ");
                    string? host = Console.ReadLine();

                    Console.Write($"Port [{config.Port}]: ");
                    string? port = Console.ReadLine();

                    Console.Write($"Path [{config.Path}]: ");
                    string? path = Console.ReadLine();

                    if (!string.IsNullOrWhiteSpace(host))
                        config.Host = host;

                    if (!string.IsNullOrWhiteSpace(port))
                        config.Port = port;

                    if (!string.IsNullOrWhiteSpace(path))
                        config.Path = path;

                    string json = JsonSerializer.Serialize(
                        config,
                        new JsonSerializerOptions
                        {
                            WriteIndented = true
                        });

                    await File.WriteAllTextAsync(configFile, json);

                    Console.WriteLine();
                    Console.WriteLine("Настройки сохранены.");
                    Console.WriteLine("Для применения нового адреса перезапустите сервер.");
                    Console.WriteLine();
                }
            }

            Console.WriteLine("Сервер остановлен.");
        }
    }

    public class ServerConfig
    {
        public string Host { get; set; } = "127.0.0.1";
        public string Port { get; set; } = "8888";
        public string Path { get; set; } = "connection";
    }
}   