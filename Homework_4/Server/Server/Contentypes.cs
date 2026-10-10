namespace Server;

public class Contentypes
{
    private const string Default = "application/octet-stream";
 
    private static readonly Dictionary<string, string> Map = new(StringComparer.OrdinalIgnoreCase)
    {
        [".html"] = "text/html; charset=utf-8",
        [".htm"] = "text/html; charset=utf-8",
        [".css"] = "text/css; charset=utf-8",
        [".js"] = "text/javascript; charset=utf-8",
        [".json"] = "application/json; charset=utf-8",
        [".txt"] = "text/plain; charset=utf-8",
        [".xml"] = "application/xml; charset=utf-8",
 
        [".png"] = "image/png",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".gif"] = "image/gif",
        [".webp"] = "image/webp",
        [".svg"] = "image/svg+xml",
        [".ico"] = "image/x-icon",
 
        [".woff"] = "font/woff",
        [".woff2"] = "font/woff2",
        [".ttf"] = "font/ttf",
 
        [".mp3"] = "audio/mpeg",
        [".mp4"] = "video/mp4",
        [".webm"] = "video/webm",
        [".pdf"] = "application/pdf",
    };
 
    public static string Get(string filePath)
    {
        string extension = Path.GetExtension(filePath);
        return Map.TryGetValue(extension, out string? type) ? type : Default;
    }
}