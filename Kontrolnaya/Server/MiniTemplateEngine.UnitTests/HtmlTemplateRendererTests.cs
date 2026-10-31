namespace MiniTemplateEngine.UnitTests;

[TestClass]
public sealed class HtmlTemplateRendererTests
{
    [TestMethod]
    public void RenderFromString_Possitive_ReturnValid()
    {   
        //Arrange
        var serv = new HtmlTemplateRenderer();
        string template = "<p>Имя:${user.Name}</p>" +
                          "<p>Возраст:${user.Age}</p>" +
                          "<p>Группа:${user.Group}</p>" +
                          "<p>Пол:${user.Sex}</p>" +
                          "$if(user.Sex=true)<p>С тебя 5к</p>$else<p>Ты девочка</p>$endif" +
                          "$if(user.Name!=Vasya)<p>С тебя 5к</p>$else<p>Ты девочка</p>$endif";
        User user = new User("Vasya", "11-541", 22, true);
        string expected = "<p>Имя:Vasya</p>" +
                          "<p>Возраст:22</p>" +
                          "<p>Группа:11-541</p>" +
                          "<p>Пол:True</p>" +
                          "<p>С тебя 5к</p>" +
                          "<p>Ты девочка</p>";
        //Act
        string result = serv.RenderFromString(template,user);
        //Assert
        Assert.AreEqual(expected, result);
    }
    [TestMethod]
    public void RenderFromString_NullExceptions()
    {   
        //Arrange
        var serv = new HtmlTemplateRenderer();
        string template = "<p>Имя:${user.Name}</p>" +
                          "<p>Возраст:${user.Age}</p>" +
                          "<p>Группа:${user.Group}</p>" +
                          "<p>Пол:${user.Sex}</p>" +
                          "$if(user.Sex=true)<p>С тебя 5к</p>$else<p>Ты девочка</p>$endif" +
                          "$if(user.Name!=Vasya)<p>С тебя 5к</p>$else<p>Ты девочка</p>$endif";
        User user = new User("Vasya", "11-541", 22, true);
        string expected = "<p>Имя:Vasya</p>" +
                          "<p>Возраст:22</p>" +
                          "<p>Группа:11-541</p>" +
                          "<p>Пол:True</p>" +
                          "<p>С тебя 5к</p>" +
                          "<p>Ты девочка</p>";
        //Act
        //Assert
        Assert.Throws<ArgumentNullException>(() => serv.RenderFromString(template,null));
        Assert.Throws<ArgumentNullException>(() => serv.RenderFromString(null, user));
        Assert.Throws<ArgumentNullException>(() => serv.RenderFromString(null, null));
    }
}