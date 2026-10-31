namespace Server.Framework.attribute;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class HttpControllerAttribute : Attribute
{
    public HttpControllerAttribute(string name) => Name = name.Trim('/');

    public string Name { get; }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public abstract class HttpVerbAttribute : Attribute
{
    protected HttpVerbAttribute(string httpMethod, string route)
    {
        HttpMethod = httpMethod;
        Route = route.Trim('/');
    }

    public string HttpMethod { get; }
    public string Route { get; }
}

public sealed class GetAttribute : HttpVerbAttribute
{
    public GetAttribute(string route) : base("GET", route) { }
}

public sealed class PostAttribute : HttpVerbAttribute
{
    public PostAttribute(string route) : base("POST", route) { }
}