using System;
using PlayFab;
using Proto.Promises;
using Venly.Utils;

public static class VyPlayFabUtils
{
    //Exceptions Wrappers
    public static VenlyException WrapException(PlayFabError error)
    {
        //todo: add more info
        return new VenlyException(error.ErrorMessage);
    }

    //Exception Handlers
    public static void HandleReject(PlayFabError error, Promise.Deferred? deferred = null)
    {
        if (deferred.HasValue) deferred.Value.Reject(error);
        else HandleException(error);
    }

    public static void HandleReject<T>(PlayFabError error, Promise<T>.Deferred? deferred = null)
    {
        if (deferred.HasValue) deferred.Value.Reject(error);
        else HandleException(error);
    }

    public static void HandleException(PlayFabError error)
    {
        //todo: add more information to exception
        VenlyUtils.HandleException(new Exception($"[PlayFab Error] {error.ErrorMessage}"));
    }
}
