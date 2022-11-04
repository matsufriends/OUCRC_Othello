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
        private const float c_timeOutTime = 1;
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
            using var request = new UnityWebRequest($"{url}/rooms",UnityWebRequest.kHttpVerbPOST) {
                uploadHandler   = new UploadHandlerRaw(null)
               ,downloadHandler = new DownloadHandlerBuffer()
            };
            await request.SendWebRequest().ToUniTask(cancellationToken: token);
        }
        public async UniTask<UserInfo> PostRegisterUser(OucrcNetType oucrcNetType,RegisterUserPostData registerUserPostData,CancellationToken token) {
            if(TryGetUrl(oucrcNetType,out var url) == false) return null;
            var postData = Encoding.UTF8.GetBytes(JsonMapper.ToJson(registerUserPostData));
            using var request = new UnityWebRequest($"{url}/users",UnityWebRequest.kHttpVerbPOST) {
                uploadHandler   = new UploadHandlerRaw(postData)
               ,downloadHandler = new DownloadHandlerBuffer()
            };
            request.SetRequestHeader("Content-Type","application/json");
            await request.SendWebRequest().ToUniTask(cancellationToken: token);
            return JsonMapper.ToObject<UserInfo>(request.downloadHandler.text);
        }
        public async UniTask PostRegisterAi(OucrcNetType oucrcNetType,CancellationToken token) {
            if(TryGetUrl(oucrcNetType,out var url) == false) return;
            using var request = new UnityWebRequest($"{url}/ai",UnityWebRequest.kHttpVerbPOST) {
                uploadHandler   = new UploadHandlerRaw(null)
               ,downloadHandler = new DownloadHandlerBuffer()
            };
            await request.SendWebRequest().ToUniTask(cancellationToken: token);
        }
        public async UniTask PostAiPutData(OucrcNetType oucrcNetType,string roomId,AIPutPostData aiPutPostData,CancellationToken token) {
            if(roomId == null) return;
            if(TryGetUrl(oucrcNetType,out var url) == false) return;
            var postData = Encoding.UTF8.GetBytes(JsonMapper.ToJson(aiPutPostData));
            Debug.Log($"{url}/rooms/{roomId} \n {JsonMapper.ToJson(aiPutPostData)}");
            using var request = new UnityWebRequest($"{url}/rooms/{roomId}",UnityWebRequest.kHttpVerbPOST) {
                uploadHandler   = new UploadHandlerRaw(postData)
               ,downloadHandler = new DownloadHandlerBuffer()
            };
            request.SetRequestHeader("Content-Type","application/json");
            await request.SendWebRequest().ToUniTask(cancellationToken: token);
        }
        public async UniTask PostPlayerPutData(OucrcNetType oucrcNetType,string roomId,PlayerPutPostData aiPutPostData,CancellationToken token) {
            if(roomId == null) return;
            if(TryGetUrl(oucrcNetType,out var url) == false) return;
            var postData = Encoding.UTF8.GetBytes(JsonMapper.ToJson(aiPutPostData));
            Debug.Log($"{url}/rooms/{roomId} \n {JsonMapper.ToJson(aiPutPostData)}");
            using var request = new UnityWebRequest($"{url}/rooms/{roomId}",UnityWebRequest.kHttpVerbPOST) {
                uploadHandler   = new UploadHandlerRaw(postData)
               ,downloadHandler = new DownloadHandlerBuffer()
            };
            request.SetRequestHeader("Content-Type","application/json");
            await request.SendWebRequest().ToUniTask(cancellationToken: token);
        }
        public async UniTask<UserInfo> GetUser(OucrcNetType oucrcNetType,string userId,CancellationToken token) {
            if(userId == null) return null;
            if(TryGetUrl(oucrcNetType,out var url) == false) return null;
            using var request = UnityWebRequest.Get($"{url}/users/{userId}");
            try {
                await request.SendWebRequest().ToUniTask(cancellationToken: token).Timeout(TimeSpan.FromSeconds(c_timeOutTime));
            } catch(Exception e) {
                Debug.Log(e);
                return null;
            }
            return JsonMapper.ToObject<UserInfo>(request.downloadHandler.text);
        }
        public async UniTask<UserInfo[]> GetAllUsers(OucrcNetType oucrcNetType,CancellationToken token) {
            if(TryGetUrl(oucrcNetType,out var url) == false) return null;
            using var request = UnityWebRequest.Get($"{url}/users");
            try {
                await request.SendWebRequest().ToUniTask(cancellationToken: token).Timeout(TimeSpan.FromSeconds(c_timeOutTime));
            } catch(Exception e) {
                Debug.Log(e);
                return null;
            }
            if(request.result != UnityWebRequest.Result.Success) return null;
            var tex = $"{{\"users\":{request.downloadHandler.text}}}";
            var allRoomInfo = JsonMapper.ToObject<AllUserInfo>(tex);
            return allRoomInfo.users;
        }
        public async UniTask<AiInfo[]> GetAllAIs(OucrcNetType oucrcNetType,CancellationToken token) {
            if(TryGetUrl(oucrcNetType,out var url) == false) return null;
            using var request = UnityWebRequest.Get($"{url}/ai");
            try {
                await request.SendWebRequest().ToUniTask(cancellationToken: token).Timeout(TimeSpan.FromSeconds(c_timeOutTime));
            } catch(TimeoutException e) {
                request.Dispose();
                return null;
            } catch(Exception e) {
                Debug.Log(e);
                return null;
            }
            if(request.result != UnityWebRequest.Result.Success) return null;
            var tex = $"{{\"ais\":{request.downloadHandler.text}}}";
            var allRoomInfo = JsonMapper.ToObject<AllAiInfo>(tex);
            return allRoomInfo.ais;
        }
        public async UniTask<RoomIdUsersAndBoard> GetRoom(OucrcNetType oucrcNetType,string roomId,CancellationToken token) {
            if(roomId == null) return null;
            if(TryGetUrl(oucrcNetType,out var url) == false) return null;
            using var request = UnityWebRequest.Get($"{url}/rooms/{roomId}");
            Debug.Log($"{url}/rooms/{roomId}");
            try {
                await request.SendWebRequest().ToUniTask(cancellationToken: token).Timeout(TimeSpan.FromSeconds(c_timeOutTime));
            } catch(Exception e) {
                Debug.Log(e);
                return null;
            }
            return JsonMapper.ToObject<RoomIdUsersAndBoard>(request.downloadHandler.text);
            ;
        }
        public async UniTask<RoomIdUsersAndBoard[]> GetAllRooms(OucrcNetType oucrcNetType,CancellationToken token) {
            if(TryGetUrl(oucrcNetType,out var url) == false) return null;
            using var request = UnityWebRequest.Get($"{url}/rooms");
            try {
                await request.SendWebRequest().ToUniTask(cancellationToken: token).Timeout(TimeSpan.FromSeconds(c_timeOutTime));
            } catch(Exception e) {
                Debug.Log(e);
                return null;
            }
            if(request.result != UnityWebRequest.Result.Success) return null;
            var tex = $"{{\"rooms\":{request.downloadHandler.text}}}";
            var allRoomInfo = JsonMapper.ToObject<AllRoomInfo>(tex);
            return allRoomInfo.rooms;
        }
    }
}