using System;
using Newtonsoft.Json;

namespace VenlySDK.Models.Wallet
{
    [Serializable]
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public abstract class VyExecuteBase<T> where T : new()
    {
        [JsonProperty("pincode")] public string Pincode { get; set; }
        [JsonProperty("transactionRequest")] public T Request { get; set; } = new T();
    }
}