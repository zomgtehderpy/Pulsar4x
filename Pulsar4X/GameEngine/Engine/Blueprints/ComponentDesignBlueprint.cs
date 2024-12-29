using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Pulsar4X.Blueprints;

public class ComponentDesignBlueprint : Blueprint
{
    public struct Property
    {
        public string Key { get; set; }

        private JToken _value;
        public JToken Value
        {
            get => _value;
            set => _value = value;
        }

        public T? GetValue<T>() => _value.ToObject<T>();

        public int AsInt => _value.Value<int>();
        public double AsDouble => _value.Value<double>();
        public string? AsString => _value.Value<string>();
    }

    public string Name { get; set; }
    public string TemplateId { get; set; }
    public List<Property>? Properties { get; set; }

}