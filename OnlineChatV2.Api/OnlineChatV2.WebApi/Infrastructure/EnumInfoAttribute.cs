namespace OnlineChatV2.WebApi.Infrastructure;

public class EnumInfoAttribute : Attribute
{
    public string Name { get; }
    public EnumInfoAttribute(string name)
    {
        Name = name;
    }
}