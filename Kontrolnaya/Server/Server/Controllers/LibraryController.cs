using System.Net;
using System.Text;
using MiniTemplateEngine;
using Server.Framework.attribute;
using Server.Framework.Handlers;
namespace Server;

[HttpController("books")]
internal class LibraryController
{
    private const int MaxTitle = 100;
    private const int MinTitle = 2;
    private const int MaxAuthor = 60;
    private const int MinAuthor = 2;
    public List<Book> books = new List<Book>();
    HtmlTemplateRenderer htmlTemplateRenderer = new HtmlTemplateRenderer();

    [Get("")]
    public Task List(HandlerContext context, string filter)
    {
        List<Book> filteredlist = new List<Book>();
        if (books.Count == 0)
        {
            return SendHtmlAsync(context,"<!DOCTYPE html><html lang=\"ru\"><head><meta charset=\"utf-8\"><title></title></head><body><p>Кирдык книг нет</p></body><html>");
        }

        foreach (var item in books)
        {
            if (item.Author.Contains(filter, StringComparison.OrdinalIgnoreCase))
            {
                filteredlist.Add(item);
            }
        }
        string htmlContext ="<!DOCTYPE html><html lang=\"ru\"><head><meta charset=\"utf-8\"><title></title></head>"+
                            "<body>"+
                            "<ul>" +
                            "${foreach(b in book)" +
                            "<li><p>Title: ${b.Author} (${b.Title}, группа ${b.Year})</p></li>}" +
                            "</ul>"+
                            "</body></html>";
        string html = htmlTemplateRenderer.RenderFromString(htmlContext, filteredlist);
        return SendHtmlAsync(context,html);
    }

    [Get("details")]
    public Task Details(HandlerContext context, List<Book> list, int id)
    {
        return Task.CompletedTask;
    }

    [Post("create")]
    public Task Create(HandlerContext context, string title, string author)
    {
    }

    [Put("update")]
    public Task Update(HandlerContext context, List<Book> list, string title, string author)
    {
        return Task.CompletedTask;
    }
    private static Task SendHtmlAsync(HandlerContext context, string html)
        => context.SendTextAsync(html, 200, "text/html; charset=utf-8");
}