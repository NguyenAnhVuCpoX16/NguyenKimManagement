namespace NKCManagement
{
    public class AiFieldAttribute : Attribute
    {
        public string Description { get; }
        public AiFieldAttribute(string description = "")
        {
            Description = description;
        }
    }
}
