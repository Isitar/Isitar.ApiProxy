namespace Api.Models
{
    public class Property
    {
        public const string ListPropertyType = "list";
        public const string IntPropertyType = "int";
        public const string FloatPropertyType = "float";
        public const string ObjectPropertyType = "obj";
        public const string StringObjectType = "string";
        
        
        public string Name { get; set; }
        public string Type { get; set; }
        public object Value { get; set; }
    }
}