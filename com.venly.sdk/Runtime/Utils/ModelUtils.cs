using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using VenlySDK.Core;
using VenlySDK.Models;

namespace VenlySDK.Utils
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
            DateTime expireDate = (DateTime) value;
            var timeLeft = expireDate - DateTime.Now;

            writer.WriteValue(timeLeft.Seconds);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var expireSpan = (Int64) reader.Value;
            var expireDate = DateTime.Now.AddSeconds(expireSpan);

            return expireDate;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime);
        }
    }

    public class SupportedChainConverter : JsonConverter
    {
        private static Dictionary<eVyChain, string> _chainNameLUT = null;

        private static void CacheChainNames()
        {
            _chainNameLUT = new Dictionary<eVyChain, string>();
            var fields = typeof(eVyChain).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);
            foreach (var fieldInfo in fields)
            {
                var att = fieldInfo.GetCustomAttribute<EnumMemberAttribute>();
                if (att != null)
                {
                    var enumVal = (eVyChain)fieldInfo.GetValue(null);
                    var enumName = att.Value;

                    _chainNameLUT.Add(enumVal, enumName);
                }
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                if(_chainNameLUT==null)
                    CacheChainNames();

                eVyChain chain = (eVyChain)value;
                writer.WriteValue(_chainNameLUT[chain]);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var chainString = reader.Value as string;

            if (Enum.TryParse<eVyChain>(chainString, true, out var result))
            {
                return result;
            }
            
            return eVyChain.NotSupported;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(eVyChain);
        }
    }

    //internal sealed class SupportedChainConverter : JsonConverter
    //{
    //    public override bool CanConvert(Type objectType)
    //    {
    //        return objectType.IsArray && objectType.GetElementType() == typeof(eVyChain);
    //    }

    //    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    //    {
    //        if (existingValue is null)
    //        {
    //            // Returning empty array???
    //            return Array.Empty<eVyChain>();
    //        }

    //        return VenlyUtils.TrimUnsupportedChains((eVyChain[]) existingValue);
    //    }

    //    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    //    {
    //        writer.WriteValue(value);
    //    }
    //}

    public class HttpContentConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if(value == null)
                writer.WriteValue((byte[])null);

            HttpContent content = (HttpContent)value;
            var byteContent = content.ReadAsByteArrayAsync().Result;
            writer.WriteValue(byteContent);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            if (reader.Value == null) return null;

            var byteContent = Encoding.UTF8.GetBytes(reader.Value as string);
            //var byteContent = reader.ReadAsBytesAsync().Result;
            //if (byteContent == null) return null;
            return new ByteArrayContent(byteContent);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(HttpContent);
        }
    }

    public class HttpMethodConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            HttpMethod method = (HttpMethod)value;
            writer.WriteValue(method.Method);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            if (reader.Value == null)
                throw new Exception("[HttpMethodConverter] Method String is NULL");

            var method = (string)reader.Value;
            return new HttpMethod(method);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(HttpMethod);
        }
    }

    public class RequestDataConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
                writer.WriteValue((byte[])null);

            VyRequestData content = (VyRequestData)value;
            var strData = JsonConvert.SerializeObject(content);
            var byteData = Encoding.UTF8.GetBytes(strData);

            writer.WriteValue(byteData);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var byteContent = reader.ReadAsBytes();
            if (byteContent == null) return null;

            var stringContent = Encoding.UTF8.GetString(byteContent);
            return JsonConvert.DeserializeObject<VyRequestData>(stringContent);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(VyRequestData);
        }
    }

    #endregion
}