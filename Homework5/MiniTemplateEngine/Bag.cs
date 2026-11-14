using System.Drawing;

namespace MiniTemplateEngine;

internal class Bag
{
    string Name;
    Maden Maden;
    Size Size;
    Color Color;

    public Bag(string name, Size size, Color color, Maden maden ){
        Name = name;
        Maden = maden;
        Size = size;
        Color = color;
    }
}