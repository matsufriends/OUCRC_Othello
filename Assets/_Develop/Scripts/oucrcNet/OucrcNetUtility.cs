using System.Text;
using Cysharp.Threading.Tasks;
using LitJson;
using UnityEngine;
using UnityEngine.Networking;
namespace oucrcNet {
    public static class OucrcNetUtility {
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
            Debug.Log(JsonMapper.ToJson(sendInfo));
            Debug.Log(postData);
            request.SetRequestHeader("Content-Type","application/json");
            await request.SendWebRequest();
            
            request.Dispose();
            /*
            var req = new UnityWebRequest($"{url}/{roomId}", "POST");
            req.uploadHandler   = new UploadHandlerRaw(postData);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.Send();
*/
            return;
            /*
            using var request = new UnityWebRequest($"{url}/{roomId}",UnityWebRequest.kHttpVerbPOST)
            {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonMapper.ToJson(sendInfo)))
            };
            request.SetRequestHeader("Content-Type","application/json");
            //Debug.Log($"SEND\nURL:{request.url}\nmessage:{JsonMapper.ToJson(sendInfo)}");
            request.SendWebRequest().completed += x => { Debug.Log($"Send End {x}");};
            */
        }
        public static async UniTask<ReceiveInfo> Get(string url,string roomId) {
            using var request = UnityWebRequest.Get($"{url}/{roomId}");
            await request.SendWebRequest();
            //Debug.Log($"GET\nURL:{request.url}\nmessage:{request.downloadHandler.text}");
            var receiveInfo = JsonMapper.ToObject<ReceiveInfo>(request.downloadHandler.text);
            receiveInfo.Log();
            //Debug.Log("END:GET");
            return receiveInfo;
        }
    }
}