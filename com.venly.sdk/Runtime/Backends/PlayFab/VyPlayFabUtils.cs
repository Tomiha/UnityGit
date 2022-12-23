using PlayFab;
using VenlySDK.Backends.PlayFab;
using VenlySDK.Core;

public static class VyPlayFabUtils
{
    public static VyException ToVyException(this PlayFabError pfError)
    {
        return new VyException($"[PlayFab Error] {pfError.ErrorMessage}");
    }

    ////Exceptions Wrappers
    //public static VyException WrapException(PlayFabError error)
    //{
    //    //todo: add more info
    //    return new VyException(error.ErrorMessage);
    //}

    ////Exception Handlers
    //public static void HandleReject(PlayFabError error, Promise.Deferred? deferred = null)
    //{
    //    if (deferred.HasValue) deferred.Value.Reject(error);
    //    else HandleException(error);
    //}

    //public static void HandleReject<T>(PlayFabError error, Promise<T>.Deferred? deferred = null)
    //{
    //    if (deferred.HasValue) deferred.Value.Reject(error);
    //    else HandleException(error);
    //}

    //public static void HandleException(PlayFabError error)
    //{
    //    //todo: add more information to exception
    //    VenlyUtils.HandleException(new Exception($"[PlayFab Error] {error.ErrorMessage}"));
    //}
}
