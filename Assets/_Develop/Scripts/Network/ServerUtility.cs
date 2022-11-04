using System;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitJson;
using MornLib.Singletons;
using UnityEngine;
using UnityEngine.Networking;
namespace OucrcReversi.Network {
    public class ServerUtility : Singleton<ServerUtility> {
        private string _battleUrl;
        private string _watchUrl;
        public const float WatchInterval = 0.1f;
        private const int c_timeOutTime = 1;
        protected override void Instanced() { }
        public void SetUrl(OucrcNetType netType,string url) {
            switch(netType) {
                case OucrcNetType.Watch:
                    _watchUrl = url;
                    break;
                case OucrcNetType.Battle:
                    _battleUrl = url;
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(netType),netType,null);
            }
        }
        private bool TryGetUrl(OucrcNetType oucrcNetType,out string url) {
            url = oucrcNetType switch {
                OucrcNetType.Watch  => _watchUrl
               ,OucrcNetType.Battle => _battleUrl
               ,_                   => throw new ArgumentOutOfRangeException(nameof(oucrcNetType),oucrcNetType,null)
            };
            return url is { Length: > 0 };
        }
        public async UniTask PostRefreshRoom(OucrcNetType oucrcNetType,CancellationToken token) {
            if(TryGetUrl(oucrcNetType,out var url) == false) return;
            Debug.Log("PostRefresh");
            using var request = new UnityWebRequest($"{url}/rooms",UnityWebRequest.kHttpVerbPOST) {
                uploadHandler   = new UploadHandlerRaw(null)
               ,downloadHandler = new DownloadHandlerBuffer()
            };
            await GetRequestTask(request,token);
        }
        public async UniTask<RoomIdUsersAndBoard> PostMakeBattleRoom(OucrcNetType oucrcNetType,MakeBattleRoomData makeBattleRoomData,CancellationToken token) {
            if(TryGetUrl(oucrcNetType,out var url) == false) return null;
            var postData = Encoding.UTF8.GetBytes(JsonMapper.ToJson(makeBattleRoomData));
            Debug.Log("MakeBattleRoom\n" + JsonMapper.ToJson(makeBattleRoomData));
            using var request = new UnityWebRequest($"{url}/rooms",UnityWebRequest.kHttpVerbPOST) {
                uploadHandler   = new UploadHandlerRaw(postData)
               ,downloadHandler = new DownloadHandlerBuffer()
            };
            request.SetRequestHeader("Content-Type","application/json");
            return await GetRequestTask(request,token) == false ? null : JsonMapper.ToObject<RoomIdUsersAndBoard>(request.downloadHandler.text);
        }
        public async UniTask<UserInfo> PostRegisterUserOrAi(OucrcNetType         oucrcNetType
                                                           ,bool                 isAi
                                                           ,RegisterUserPostData registerUserPostData
                                                           ,CancellationToken    token) {
            if(TryGetUrl(oucrcNetType,out var url) == false) return null;
            var postData = Encoding.UTF8.GetBytes(JsonMapper.ToJson(registerUserPostData));
            Debug.Log("PostRegisterUserOrAi\n" + JsonMapper.ToJson(registerUserPostData));
            using var request = new UnityWebRequest($"{url}/{(isAi ? "ai" : "users")}",UnityWebRequest.kHttpVerbPOST) {
                uploadHandler   = new UploadHandlerRaw(postData)
               ,downloadHandler = new DownloadHandlerBuffer()
            };
            request.SetRequestHeader("Content-Type","application/json");
            return await GetRequestTask(request,token) == false ? null : JsonMapper.ToObject<UserInfo>(request.downloadHandler.text);
        }
        public async UniTask PostAiPutData(OucrcNetType oucrcNetType,string roomId,AIPutPostData aiPutPostData,CancellationToken token) {
            if(roomId == null) return;
            if(TryGetUrl(oucrcNetType,out var url) == false) return;
            var postData = Encoding.UTF8.GetBytes(JsonMapper.ToJson(aiPutPostData));
            Debug.Log("PostAiPutData\n" + JsonMapper.ToJson(aiPutPostData));
            using var request = new UnityWebRequest($"{url}/rooms/{roomId}",UnityWebRequest.kHttpVerbPOST) {
                uploadHandler   = new UploadHandlerRaw(postData)
               ,downloadHandler = new DownloadHandlerBuffer()
            };
            request.SetRequestHeader("Content-Type","application/json");
            await GetRequestTask(request,token);
        }
        public async UniTask PostPlayerPutData(OucrcNetType oucrcNetType,string roomId,PlayerPutPostData aiPutPostData,CancellationToken token) {
            if(roomId == null) return;
            if(TryGetUrl(oucrcNetType,out var url) == false) return;
            var postData = Encoding.UTF8.GetBytes(JsonMapper.ToJson(aiPutPostData));
            Debug.Log("PostPlayerPutData\n" + JsonMapper.ToJson(aiPutPostData));
            using var request = new UnityWebRequest($"{url}/rooms/{roomId}",UnityWebRequest.kHttpVerbPOST) {
                uploadHandler   = new UploadHandlerRaw(postData)
               ,downloadHandler = new DownloadHandlerBuffer()
            };
            request.SetRequestHeader("Content-Type","application/json");
            Debug.Log("AAA : " + request.url);
            await GetRequestTask(request,token);
        }
        public async UniTask<UserInfo[]> GetAllUsers(OucrcNetType oucrcNetType,bool isAi,CancellationToken token) {
            if(TryGetUrl(oucrcNetType,out var url) == false) return null;
            using var request = UnityWebRequest.Get($"{url}/{(isAi ? "ai" : "users")}");
            if(await GetRequestTask(request,token) == false) return null;
            var tex = $"{{\"users\":{request.downloadHandler.text}}}";
            var allRoomInfo = JsonMapper.ToObject<AllUserInfo>(tex);
            return allRoomInfo.users;
        }
        public async UniTask<RoomIdUsersAndBoard[]> GetAllRooms(OucrcNetType oucrcNetType,CancellationToken token) {
            if(TryGetUrl(oucrcNetType,out var url) == false) return null;
            using var request = UnityWebRequest.Get($"{url}/rooms");
            if(await GetRequestTask(request,token) == false) return null;
            var tex = $"{{\"rooms\":{request.downloadHandler.text}}}";
            var allRoomInfo = JsonMapper.ToObject<AllRoomInfo>(tex);
            return allRoomInfo.rooms;
        }
        private static async UniTask<bool> GetRequestTask(UnityWebRequest request,CancellationToken token) {
            try {
                await request.SendWebRequest().ToUniTask(cancellationToken: token);
                return request.result == UnityWebRequest.Result.Success;
            } catch(Exception e) {
                //Debug.Log(request.url + "\n" + e);
                return false;
            }
        }
    }
}