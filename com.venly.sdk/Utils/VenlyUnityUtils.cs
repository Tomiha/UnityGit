#if !ENABLE_VENLY_AZURE
using Proto.Promises;
using Venly.Utils;
using UnityEngine;
using UnityEngine.Networking;

public static class VenlyUnityUtils
{
    public static Promise<Texture2D> DownloadImage(string uri)
    {
        var result = Promise<Texture2D>.NewDeferred();

        var webRequest = UnityWebRequestTexture.GetTexture(uri);
        webRequest.SendWebRequest().completed += (asyncResult) =>
        {
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                result.Reject(VenlyUtils.WrapException($"Failed to download image from uri\nError = {webRequest.error}\nUri = {uri}"));
            }
            else
            {
                var texture = ((DownloadHandlerTexture) webRequest.downloadHandler).texture;
                result.Resolve(texture);
            }
        };

        return result.Promise;
    }
}
#endif