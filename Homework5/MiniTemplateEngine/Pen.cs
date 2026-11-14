internal class Pen
{
    string Name{get;set;}
    string Color{get;set;}
    int Width{get;set;}
    decimal Price{get;set;}

    public Pen(string name, string color, int width, decimal price)
    {
        Name = name;
        Color = color;
        Width = width;
        Price = price;
    }
}