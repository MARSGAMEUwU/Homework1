using System.Net;
using System.Text;
using Server.Framework.attribute;
using Server.Framework.Handlers;
 
namespace Server.Controllers
{
    [HttpController("Auth")]
    internal class AuthController
    {
        private static readonly string[] Activities = { "buy game", "play game", "comment game" };
 
        [Get("profile")]
        public Task GetProfile(HandlerContext context, string id, string password)
            => SendHtmlAsync(context, BuildPage("Profile", id, password, Array.Empty<string>()));
 
        [Post("activities")]
        public Task PostActivities(HandlerContext context, string id, string password)
            => SendHtmlAsync(context, BuildPage("Activities", id, password, Activities));
 
        private static string BuildPage(string title, string id, string password, IEnumerable<string> lines)
        {
            var sb = new StringBuilder();
 
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang=\"ru\">");
            sb.AppendLine("<head><meta charset=\"utf-8\"><title>" + title + "</title></head>");
            sb.AppendLine("<body>");
            sb.AppendLine("<h1>" + title + "</h1>");
 
            sb.AppendLine("<p>id: " + WebUtility.HtmlEncode(id) + "</p>");
            sb.AppendLine("<p>password: " + WebUtility.HtmlEncode(password) + "</p>");
 
            foreach (string line in lines)
                sb.AppendLine("<p>" + line + "</p>");
 
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");
 
            return sb.ToString();
        }
 
        private static Task SendHtmlAsync(HandlerContext context, string html)
            => context.SendTextAsync(html, 200, "text/html; charset=utf-8");
    }
}