using System.Collections.Generic;
using VenlySDK.Core;
using VenlySDK.Models.Shared;
using VenlySDK.Utils;

namespace VenlySDK
{
    public static partial class Venly
    {
#if ((UNITY_EDITOR || UNITY_SERVER) || ENABLE_VENLY_API_SERVER) && !DISABLE_VENLY_API_SERVER
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

                VyTaskNotifier<VyAccessTokenDto> _notifier = VyTask<VyAccessTokenDto>.Create();

                var reqData = VyRequestData
                    .Post("/auth/realms/Arkane/protocol/openid-connect/token", eVyApiEndpoint.Auth)
                    .AddFormContent(formData);

                Request<VyAccessTokenDto>(reqData)
                    .OnComplete(result =>
                    {
                        if (result.Success)
                        {
                            VenlyUtils.JWT.UpdateBearerToken(result.Data);
                            SetEnvironment(result.Data.Environment);
                        }

                        _notifier.Notify(result);
                    });

                return _notifier.Task;
            }
        }
#endif
    }
}
