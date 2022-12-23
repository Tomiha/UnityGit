#if !ENABLE_VENLY_AZURE
using Proto.Promises;
using UnityEngine;
using UnityEngine.Networking;
using VenlySDK.Core;
using VenlySDK.Utils;

public static class VenlyUnityUtils
{
    public static VyTask<Texture2D> DownloadImage(string uri)
    {
        var taskNotifier = VyTask<Texture2D>.Create();

        var webRequest = UnityWebRequestTexture.GetTexture(uri);
        webRequest.SendWebRequest().completed += (asyncResult) =>
        {
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                taskNotifier.NotifyFail($"Failed to download image from uri\nError = {webRequest.error}\nUri = {uri}");
            }
            else
            {
                var texture = ((DownloadHandlerTexture) webRequest.downloadHandler).texture;
                taskNotifier.NotifySuccess(texture);
            }
        };

        return taskNotifier.Task;
    }
}
#endif