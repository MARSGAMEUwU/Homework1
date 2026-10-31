using System.Drawing;
using MiniTemplateEngine;
using Size = MiniTemplateEngine.Size;

HtmlTemplateRenderer htmlTemplateRenderer = new HtmlTemplateRenderer();
// string template1 = "<p>Имя:${user.Name}</p>" +
//                    "<p>Возраст:${user.Age}</p>" +
//                    "<p>Группа:${user.Group}</p>" +
//                    "<p>Пол:${user.Sex}</p>"+
//                    "${if(user.Sex=true)<p>С тебя 5к</p>$else<p>Ты девочка</p>$endif" +
//                    "$if(user.Name!=Vasya)<p>С тебя 5к</p>$else<p>Ты девочка</p>$endif";
// User user = new User("Vasya", "11-541", 22, true);
//
// string result1 = htmlTemplateRenderer.RenderFromString(template1, user);
// Console.WriteLine(result1);
//
// string template2 = "<p>Name:${book.Name}</p>" +
//                    "<p>Год:${book.Year}</p>" +
//                    "<p>Автор:${book.Author}</p>";
// Book book = new Book("Преступление и Наказание", 1867, "Ф.М.Достоевский");
// string result2 = htmlTemplateRenderer.RenderFromString(template2, book);
// Console.WriteLine(result2);
//
// string template3 = "<p>Name:${book.Name}</p>" +
//                    "<p>Цвет:${book.Year}</p>" +
//                    "<p>Толщина:${book.Author} мм</p>" +
//                    "<p>Цена${book.Price}</p>";
// Pen pen = new Pen("Berlingo", "Blue", 1,20);
// string result3 = htmlTemplateRenderer.RenderFromString(template3, pen);
// Console.WriteLine(result3);

// Bag bag = new Bag("Boarini Milanesi Parva Mea", new Size(10, 10, 10), Color.Aqua ,new Maden(1, "Italy", "Bologna"));
// string templateBag = "<h1>${bag.Name}(${bag.Price})<p>Страна: ${bag.Maden.City}</p></h1>";
// Console.WriteLine(htmlTemplateRenderer.RenderFromString(templateBag, bag));

var users = new List<User>
{
    new User("Саша", "11-507", 20, true),
    new User("Маша",   "11-541", 17, false),
    new User("Vasya",    "11-506", 15, true),
    new User("Ольга",   "11-505", 22, false)
};

string template =
    "<ul>" +
    "${foreach(User in user)" +
    "<li><p>Name: ${User.Name} (${User.Age}, группа ${User.Group})</p></li>" +
    "${if(User.Sex=true && User.Age>=18)<li><p>С тебя 5к</p></li>" +
    "elseif(User.Age<18)<li><p>У тебя нет денег</p></li>" +
    "else<li><p>ОК</p></li>}" +
    "}" +
    "</ul>";

Console.WriteLine(htmlTemplateRenderer.RenderFromString(template, users));