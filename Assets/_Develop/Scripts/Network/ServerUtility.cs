using System;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitJson;
using MornLib.Singletons;
using UnityEngine.Networking;
namespace OucrcReversi.Network {
    public class ServerUtility : Singleton<ServerUtility> {
        private string _battleUrl;
        private string _watchUrl;
        public const float WatchInterval = 0.1f;
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
        private string GetUrl(OucrcNetType oucrcNetType) {
            return oucrcNetType switch {
                OucrcNetType.Watch  => _watchUrl
               ,OucrcNetType.Battle => _battleUrl
               ,_                   => throw new ArgumentOutOfRangeException(nameof(oucrcNetType),oucrcNetType,null)
            };
        }
        public async UniTask PostRefreshRoom(OucrcNetType oucrcNetType,CancellationToken token) {
            var url = GetUrl(oucrcNetType);
            using var request = new UnityWebRequest($"{url}/rooms",UnityWebRequest.kHttpVerbPOST) {
                uploadHandler   = new UploadHandlerRaw(null)
               ,downloadHandler = new DownloadHandlerBuffer()
            };
            await request.SendWebRequest().ToUniTask(cancellationToken: token);
        }
        public async UniTask<UserInfo> PostRegisterUser(OucrcNetType oucrcNetType,RegisterUserPostData registerUserPostData,CancellationToken token) {
            var url = GetUrl(oucrcNetType);
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
            var url = GetUrl(oucrcNetType);
            using var request = new UnityWebRequest($"{url}/ai",UnityWebRequest.kHttpVerbPOST) {
                uploadHandler   = new UploadHandlerRaw(null)
               ,downloadHandler = new DownloadHandlerBuffer()
            };
            await request.SendWebRequest().ToUniTask(cancellationToken: token);
        }
        public async UniTask PostPutData(OucrcNetType oucrcNetType,string roomId,PutPostData putPostData,CancellationToken token) {
            var url = GetUrl(oucrcNetType);
            var postData = Encoding.UTF8.GetBytes(JsonMapper.ToJson(putPostData));
            using var request = new UnityWebRequest($"{url}/{roomId}",UnityWebRequest.kHttpVerbPOST) {
                uploadHandler   = new UploadHandlerRaw(postData)
               ,downloadHandler = new DownloadHandlerBuffer()
            };
            request.SetRequestHeader("Content-Type","application/json");
            await request.SendWebRequest().ToUniTask(cancellationToken: token);
        }
        public async UniTask<UserInfo> GetUser(OucrcNetType oucrcNetType,string userId,CancellationToken token) {
            var url = GetUrl(oucrcNetType);
            using var request = UnityWebRequest.Get($"{url}/users/{userId}");
            try {
                await request.SendWebRequest().ToUniTask(cancellationToken: token).Timeout(TimeSpan.FromSeconds(3));
            } catch(Exception) {
                return null;
            }
            return JsonMapper.ToObject<UserInfo>(request.downloadHandler.text);
        }
        public async UniTask<UserInfo[]> GetAllUsers(OucrcNetType oucrcNetType,CancellationToken token) {
            var url = GetUrl(oucrcNetType);
            using var request = UnityWebRequest.Get($"{url}/users");
            try {
                await request.SendWebRequest().ToUniTask(cancellationToken: token).Timeout(TimeSpan.FromSeconds(3));
            } catch(Exception) {
                return null;
            }
            if(request.result != UnityWebRequest.Result.Success) return null;
            var tex = $"{{\"users\":{request.downloadHandler.text}}}";
            var allRoomInfo = JsonMapper.ToObject<AllUserInfo>(tex);
            return allRoomInfo.users;
        }
        public async UniTask<AiInfo[]> GetAllAIs(OucrcNetType oucrcNetType,CancellationToken token) {
            var url = GetUrl(oucrcNetType);
            using var request = UnityWebRequest.Get($"{url}/ai");
            try {
                await request.SendWebRequest().ToUniTask(cancellationToken: token).Timeout(TimeSpan.FromSeconds(3));
            } catch(Exception) {
                return null;
            }
            if(request.result != UnityWebRequest.Result.Success) return null;
            var tex = $"{{\"ais\":{request.downloadHandler.text}}}";
            var allRoomInfo = JsonMapper.ToObject<AllAiInfo>(tex);
            return allRoomInfo.ais;
        }
        public async UniTask<RoomIdUsersAndBoard> GetRoom(OucrcNetType oucrcNetType,string roomId,CancellationToken token) {
            var url = GetUrl(oucrcNetType);
            using var request = UnityWebRequest.Get($"{url}/rooms/{roomId}");
            try {
                await request.SendWebRequest().ToUniTask(cancellationToken: token).Timeout(TimeSpan.FromSeconds(3));
            } catch(Exception) {
                return null;
            }
            return JsonMapper.ToObject<RoomIdUsersAndBoard>(request.downloadHandler.text);
            ;
        }
        public async UniTask<RoomIdUsersAndBoard[]> GetAllRooms(OucrcNetType oucrcNetType,CancellationToken token) {
            var url = GetUrl(oucrcNetType);
            using var request = UnityWebRequest.Get($"{url}/rooms");
            try {
                await request.SendWebRequest().ToUniTask(cancellationToken: token).Timeout(TimeSpan.FromSeconds(3));
            } catch(Exception) {
                return null;
            }
            if(request.result != UnityWebRequest.Result.Success) return null;
            var tex = $"{{\"rooms\":{request.downloadHandler.text}}}";
            var allRoomInfo = JsonMapper.ToObject<AllRoomInfo>(tex);
            return allRoomInfo.rooms;
        }
    }
}