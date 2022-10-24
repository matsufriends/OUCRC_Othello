using Cysharp.Threading.Tasks;
using LitJson;
using UnityEngine;
using UnityEngine.Networking;
namespace oucrcNet {
    public static class OucrcNetUtility {
        public static async void Send(string url,string roomId,Vector2Int pos,string userId) {
            var sendInfo = new SendInfo {
                column  = pos.x
               ,row     = pos.y
               ,user_id = userId
            };
            var request = UnityWebRequest.Post($"{url}/{roomId}","POST");
            request.uploadHandler   = new UploadHandlerRaw(System.Text.Encoding.ASCII.GetBytes(JsonMapper.ToJson(sendInfo)));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type","application/json");
            Debug.Log($"SEND\nURL:{request.url}\nmessage:{JsonMapper.ToJson(sendInfo)}");
            await request.SendWebRequest();
        }
        public static async UniTask<ReceiveInfo> Get(string url,string roomId) {
            var request = UnityWebRequest.Get($"{url}/{roomId}");
            await request.SendWebRequest();
            Debug.Log($"GET\nURL:{request.url}\nmessage:{request.downloadHandler.text}");
            var receiveInfo = JsonMapper.ToObject<ReceiveInfo>(request.downloadHandler.text);
            receiveInfo.Log();
            return receiveInfo;
        }
    }
}