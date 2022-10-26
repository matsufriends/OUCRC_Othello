using System.Text;
using Cysharp.Threading.Tasks;
using LitJson;
using UnityEngine;
using UnityEngine.Networking;
namespace oucrcNet {
    public static class OucrcNetUtility {
        private static string s_url;
        public static void Init(string url) {
            s_url = url;
        }
        public static async UniTask Send(string url,string roomId,Vector2Int pos,string userId) {
            var sendInfo = new SendInfo {
                column  = pos.x
               ,row     = pos.y
               ,user_id = userId
            };
            var postData = Encoding.UTF8.GetBytes(JsonMapper.ToJson(sendInfo));
            var request = new UnityWebRequest($"{url}/{roomId}",UnityWebRequest.kHttpVerbPOST) {
                uploadHandler   = new UploadHandlerRaw(postData)
               ,downloadHandler = new DownloadHandlerBuffer()
            };
            request.SetRequestHeader("Content-Type","application/json");
            await request.SendWebRequest();
            request.Dispose();
        }
        public static async UniTask<ReceiveInfo> Get(string url,string roomId) {
            using var request = UnityWebRequest.Get($"{url}/{roomId}");
            await request.SendWebRequest();
            var receiveInfo = JsonMapper.ToObject<ReceiveInfo>(request.downloadHandler.text);
            receiveInfo.Log();
            return receiveInfo;
        }
    }
}