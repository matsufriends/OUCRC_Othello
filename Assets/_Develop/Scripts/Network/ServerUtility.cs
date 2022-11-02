using System;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitJson;
using MornLib.Singletons;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;
namespace OucrcReversi.Network {
    public class ServerUtility : Singleton<ServerUtility> {
        private readonly Subject<OucrcNetType> _urlUpdateSubject = new();
        private string _battleUrl;
        private string _watchUrl;
        public IObservable<OucrcNetType> OnUrlUpdated => _urlUpdateSubject;
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
            _urlUpdateSubject.OnNext(netType);
        }
        public async UniTask Post(string url,string roomId,Vector2Int pos,string userId) {
            var sendInfo = new CellPositionAndUserId {
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
        private string GetUrl(OucrcNetType oucrcNetType) => oucrcNetType switch {
            OucrcNetType.Watch  => _watchUrl
           ,OucrcNetType.Battle => _battleUrl
           ,_                   => throw new ArgumentOutOfRangeException(nameof(oucrcNetType),oucrcNetType,null)
        };
        public async UniTask<RoomIdUsersAndBoards> GetRoom(OucrcNetType oucrcNetType,string roomId,CancellationToken token) {
            var url = GetUrl(oucrcNetType);
            using var request = UnityWebRequest.Get($"{url}/rooms/{roomId}");
            await request.SendWebRequest().ToUniTask(cancellationToken: token);
            if(request.result != UnityWebRequest.Result.Success) return null;
            var room = JsonMapper.ToObject<RoomIdUsersAndBoards>(request.downloadHandler.text);
            return room;
        }
        public async UniTask<RoomIdUsersAndBoards[]> GetAllRooms(OucrcNetType oucrcNetType,CancellationToken token) {
            var url = GetUrl(oucrcNetType);
            using var request = UnityWebRequest.Get($"{url}/rooms");
            await request.SendWebRequest().ToUniTask(cancellationToken: token);
            if(request.result != UnityWebRequest.Result.Success) return null;
            var tex = $"{{\"rooms\":{request.downloadHandler.text}}}";
            var allRoomInfo = JsonMapper.ToObject<AllRoomInfo>(tex);
            return allRoomInfo.rooms;
        }
        [Serializable]
        private sealed class AllRoomInfo {
            public RoomIdUsersAndBoards[] rooms = new RoomIdUsersAndBoards[2];
        }
    }
}