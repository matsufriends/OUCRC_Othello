using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MornLib.Singletons;
using OucrcReversi.Network;
using UniRx;
using UnityEngine;
namespace OucrcReversi.Scene {
    public class GameManagerMono : SingletonMono<GameManagerMono> {
        private readonly Subject<(OucrcNetType,UserInfo[])> _getAllAISubject = new();
        private readonly Subject<(OucrcNetType,RoomIdUsersAndBoard[])> _getAllRoomSubject = new();
        private readonly Subject<(OucrcNetType,UserInfo[])> _getAllUserSubject = new();
        public IObservable<(OucrcNetType,UserInfo[])> OnGetAllUser => _getAllUserSubject;
        public IObservable<(OucrcNetType,UserInfo[])> OnGetAllAI => _getAllAISubject;
        public IObservable<(OucrcNetType,RoomIdUsersAndBoard[])> OnGetAllRoom => _getAllRoomSubject;
        private void Start() {
            var a = new BoardStatusPoller(OucrcNetType.Watch,9,new Vector3(-20,0,-12),"");
            var token = gameObject.GetCancellationTokenOnDestroy();
            ServerGetLoop(token).Forget();
        }
        protected override void MyAwake() { }
        private async UniTask ServerGetLoop(CancellationToken token) {
            while(true) {
                GetUserTask(OucrcNetType.Watch,token).Forget();
                GetUserTask(OucrcNetType.Battle,token).Forget();
                GetAITask(OucrcNetType.Battle,token).Forget();
                GetRoomTask(OucrcNetType.Watch,token).Forget();
                GetRoomTask(OucrcNetType.Battle,token).Forget();
                await UniTask.Delay(TimeSpan.FromSeconds(ServerUtility.WatchInterval),cancellationToken: token);
            }
        }
        private async UniTask GetUserTask(OucrcNetType oucrcNetType,CancellationToken token) {
            var users = await ServerUtility.Instance.GetAllUsers(oucrcNetType,false,token);
            if(users != null) {
                _getAllUserSubject.OnNext((oucrcNetType,users));
                if(users.All(x => x.status == null)) ServerUtility.Instance.PostRefreshRoom(oucrcNetType,token).Forget();
            }
        }
        private async UniTask GetAITask(OucrcNetType oucrcNetType,CancellationToken token) {
            var ais = await ServerUtility.Instance.GetAllUsers(oucrcNetType,true,token);
            if(ais != null) _getAllAISubject.OnNext((oucrcNetType,ais));
        }
        private async UniTask GetRoomTask(OucrcNetType oucrcNetType,CancellationToken token) {
            var rooms = await ServerUtility.Instance.GetAllRooms(oucrcNetType,token);
            if(rooms != null) _getAllRoomSubject.OnNext((oucrcNetType,rooms));
        }
    }
}