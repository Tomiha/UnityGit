using System;
using Newtonsoft.Json;

namespace Venly.Utils
{
    #region Extensions
    public static class EnumExtensions
    {
        public static string GetMemberName(this Enum e)
        {
            return JsonConvert.SerializeObject(e).Trim('"');
        }
    }
    #endregion

    #region JSON Converters
    public class ExpireConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            DateTime expireDate = (DateTime)value;
            var timeLeft = expireDate - DateTime.Now;

            writer.WriteValue(timeLeft.Seconds);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var expireSpan = (Int64)reader.Value;
            var expireDate = DateTime.Now.AddSeconds(expireSpan);

            return expireDate;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime);
        }
    }
    #endregion
}