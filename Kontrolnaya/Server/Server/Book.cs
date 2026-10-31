internal class Book
{
    public string Title { get; set; }
    public string Author { get; set; }
    public string Readed { get; set; }

    public Book(string title, string author)
    {
        Title = title;
        Author = author;
        Readed = "Unread";
    }
}