namespace MiniTemplateEngine;

public class User
{
    string Name{get;set;}
    string Group{get;set;}
    int Age{get;set;}
    bool Sex{get;set;}

    public User(string name, string group, int age, bool sex)
    {
        Name = name;
        Group = group;
        Age = age;
        Sex = sex;
    }
}