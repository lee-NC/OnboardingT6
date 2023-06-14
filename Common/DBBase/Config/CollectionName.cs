namespace Demo.Common.DBBase.Config
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class CollectionName : Attribute
    {
        
        public CollectionName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Empty collection name not allowed", "value");
            }

            this.Name = value;
        }
        
        public virtual string Name { get; private set; }
    }
}
