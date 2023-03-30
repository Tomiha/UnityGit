using VenlySDK.Core;
using VenlySDK.Models.Shared;

namespace VenlySDK
{
    public static partial class Venly
    {
        public static class MarketAPI
        {
            private static readonly eVyApiEndpoint _apiEndpoint = eVyApiEndpoint.Market;

            public static class Client
            {
                //public VyTask<VyOfferDto[]> GetOffers()
                //{

                //}
            }

#if ((UNITY_EDITOR || UNITY_SERVER) || ENABLE_VENLY_API_SERVER) && !DISABLE_VENLY_API_SERVER
            public static class Server
            {
               
            }
#endif
        }
    }
}
