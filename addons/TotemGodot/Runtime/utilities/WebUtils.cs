//using UnityEngine.Networking;
using GodotUtils;

namespace TotemUtils
{
    public static class WebUtils
    {
        public static UnityWebRequest CreateRequestJson(string url, string json, string method = "POST")
        {
            var request = new UnityWebRequest(url, method);
            //byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            //request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            //request.downloadHandler = new DownloadHandlerBuffer();

            request.uploadHandler = new UploadHandler(json);
            request.SetRequestHeader("Content-Type", "application/json");

            return request;
        }
    }
}
