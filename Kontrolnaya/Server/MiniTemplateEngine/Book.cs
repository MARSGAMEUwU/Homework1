internal class Book
{
    string Title { get; set; }
    string Author { get; set; }
    string Readed { get; set; }

    public Book(string title, string author)
    {
        Title = title;
        Author = author;
        Readed = "Unread";
    }
}