using System;
using System.Text;
using Cysharp.Threading.Tasks;
using LitJson;
using MornLib.Singletons;
using UnityEngine;
using UnityEngine.Networking;
using CancellationToken = System.Threading.CancellationToken;
namespace OucrcReversi.Network {
    public class ServerUtility : Singleton<ServerUtility> {
        private string _battleUrl;
        private string _watchUrl;
        public const float WatchInterval = 0.1f;
        protected override void Instanced() {
            _watchUrl  = PlayerPrefs.GetString(OucrcNetTypeEx.ToString(OucrcNetType.Watch),"");
            _battleUrl = PlayerPrefs.GetString(OucrcNetTypeEx.ToString(OucrcNetType.Battle),"");
        }
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
            PlayerPrefs.SetString(OucrcNetTypeEx.ToString(netType),url);
            PlayerPrefs.Save();
        }
        private string GetUrl(OucrcNetType oucrcNetType) {
            return oucrcNetType switch {
                OucrcNetType.Watch  => _watchUrl
               ,OucrcNetType.Battle => _battleUrl
               ,_                   => throw new ArgumentOutOfRangeException(nameof(oucrcNetType),oucrcNetType,null)
            };
        }
        public async UniTask<UserInfo> PostRegisterUser(OucrcNetType oucrcNetType,RegisterUserPostData registerUserPostData) {
            var url = GetUrl(oucrcNetType);
            var postData = Encoding.UTF8.GetBytes(JsonMapper.ToJson(registerUserPostData));
            using var request = new UnityWebRequest($"{url}/users",UnityWebRequest.kHttpVerbPOST) {
                uploadHandler   = new UploadHandlerRaw(postData)
               ,downloadHandler = new DownloadHandlerBuffer()
            };
            request.SetRequestHeader("Content-Type","application/json");
            await request.SendWebRequest();
            return JsonMapper.ToObject<UserInfo>(request.downloadHandler.text);
        }
        public async UniTask PostPutData(OucrcNetType oucrcNetType,string roomId,PutPostData putPostData) {
            var url = GetUrl(oucrcNetType);
            var postData = Encoding.UTF8.GetBytes(JsonMapper.ToJson(putPostData));
            using var request = new UnityWebRequest($"{url}/{roomId}",UnityWebRequest.kHttpVerbPOST) {
                uploadHandler   = new UploadHandlerRaw(postData)
               ,downloadHandler = new DownloadHandlerBuffer()
            };
            request.SetRequestHeader("Content-Type","application/json");
            await request.SendWebRequest();
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