namespace Api.Parser
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Text.Json;
    using Models;

    public class PropertyParser
    {
        public static Property FromJson(JsonElement jel)
        {
            return new Property
            {
                Name = jel.EnumerateObject().Where(jprop => jprop.Name.ToLower().Equals("name")).Select(jprop => jprop.Value.GetString() ?? string.Empty).FirstOrDefault(),
                Type = jel.EnumerateObject().Where(jprop => jprop.Name.ToLower().Equals("type")).Select(jprop => jprop.Value.GetString() ?? string.Empty).FirstOrDefault(),
                Value = jel.EnumerateObject().Where(jprop => jprop.Name.ToLower().Equals("value")).Select(jprop => jprop.Value).FirstOrDefault(),
            };
        }

        public static dynamic ParsePropertyValue(Property p)
        {
            switch (p.Type.ToLower())
            {
                case Property.IntPropertyType:
                    if (!long.TryParse(p.Value.ToString(), out var retInt))
                    {
                        throw new ArgumentException($"value is not an int: {p.Value}", p.Name);
                    }

                    return retInt;
                case Property.FloatPropertyType:
                    if (!decimal.TryParse(p.Value.ToString(), out var retFloat))
                    {
                        throw new ArgumentException($"value is not a float: {p.Value}", p.Name);
                    }

                    return retFloat;
                case Property.StringObjectType:
                    return p.Value.ToString();
                case Property.ListPropertyType:
                    var listProperties = p.Value is JsonElement ? (JsonElement) p.Value : default;
                    return listProperties.EnumerateArray().ToList()
                        .Select(jel =>
                        {
                            var prop = FromJson(jel);

                            return ParsePropertyValue(prop);
                        })
                        .ToList();
                case Property.ObjectPropertyType:
                    var properties = p.Value is JsonElement ? (JsonElement) p.Value : default;
                    var res = new ExpandoObject() as IDictionary<string, dynamic>;
                    ;
                    foreach (var jel in properties.EnumerateArray())
                    {
                        var property = FromJson(jel);
                        var name = property.Name;
                        var evaluatedVal = ParsePropertyValue(property);
                        res.Add(name, evaluatedVal);
                    }

                    return res;
                default: throw new ArgumentException("unable to parse.. found type: " + p.Type, nameof(p));
            }
        }
    }
}