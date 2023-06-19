namespace Demo.Common.DBBase.Config;

[AttributeUsage(AttributeTargets.Class)]
public class CollectionName : Attribute
{
    public CollectionName(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Empty collection name not allowed", "value");

        Name = value;
    }

    public virtual string Name { get; private set; }
}