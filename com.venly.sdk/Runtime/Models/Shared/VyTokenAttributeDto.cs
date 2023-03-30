using System;
using Newtonsoft.Json;

namespace VenlySDK.Models.Shared
{
    [Serializable]
    public class VyTokenAttributeDto
    {
        [JsonProperty("type")] public eVyTokenAttributeType Type { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("value")] public object Value { get; set; }

        public T As<T>()
        {
            return (T) Convert.ChangeType(Value, typeof(T));
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }


    

}