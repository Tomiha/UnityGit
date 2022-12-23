using System.Collections.Generic;
using VenlySDK.Core;
using VenlySDK.Models;
using VenlySDK.Models.Internal;

namespace VenlySDK
{
    public static partial class Venly
    {
#if UNITY_EDITOR || UNITY_SERVER || ENABLE_VENLY_API_SERVER
        public static class AuthAPI
        {
            public static VyTask<VyAccessTokenDto> GetAccessToken(string clientId, string clientSecret)
            {
                if (string.IsNullOrWhiteSpace(clientId))
                    return VyTask<VyAccessTokenDto>.Failed(VyException.Argument("Client-ID cannot be NULL or empty", nameof(clientId)));

                if (string.IsNullOrWhiteSpace(clientSecret))
                    return VyTask<VyAccessTokenDto>.Failed(VyException.Argument("Client-SECRET cannot be NULL or empty", nameof(clientSecret)));

                var formData = new Dictionary<string, string>
                {
                    {"grant_type", "client_credentials"},
                    {"client_id", clientId},
                    {"client_secret", clientSecret}
                };

                var reqData = VyRequestData
                    .Post("/auth/realms/Arkane/protocol/openid-connect/token", eVyApiEndpoint.Auth)
                    .AddFormContent(formData);
                return Request<VyAccessTokenDto>(reqData);
            }
        }
#endif
    }
}
