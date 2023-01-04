using System;
using Newtonsoft.Json;
using VenlySDK.Utils;

namespace VenlySDK.Models
{
    public class VyAccessTokenDto
    {
        [JsonProperty("access_token")] public string Token { get; private set; }

        [JsonProperty("expires_in")]
        [JsonConverter(typeof(ExpireConverter))]
        public DateTime TokenExpire { get; private set; }

        [JsonProperty("refresh_token")] public string RefreshToken { get; private set; }

        [JsonProperty("refresh_expires_in")]
        [JsonConverter(typeof(ExpireConverter))]
        public DateTime RefreshTokenExpire { get; private set; }

        [JsonProperty("token_type")] public string TokenType { get; private set; }
        [JsonProperty("not-before-policy")] public int NotBeforePolicy { get; private set; }
        [JsonProperty("session_state")] public string SessionState { get; private set; }
        [JsonProperty("scope")] public string Scope { get; private set; }

        public bool IsValid => TokenExpire > DateTime.Now;
    }
}