namespace Server.Framework;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class CommandAttribute : Attribute
{
    public CommandAttribute(string route, string httpMethod = "GET")
    {
        Route = route;
        HttpMethod = httpMethod.ToUpperInvariant();
    }

    public string Route { get; }
    public string HttpMethod { get; }
}