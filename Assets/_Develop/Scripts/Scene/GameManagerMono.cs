using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MornLib.Singletons;
using OucrcReversi.Network;
using UniRx;
namespace OucrcReversi.Scene {
    public class GameManagerMono : SingletonMono<GameManagerMono> {
        private readonly Subject<(OucrcNetType,UserInfo[])> _getAllUserSubject = new();
        private readonly Subject<(OucrcNetType,UserInfo[])> _getAllAISubject = new();
        private readonly Subject<(OucrcNetType,RoomIdUsersAndBoard[])> _getAllRoomSubject = new();
        public IObservable<(OucrcNetType,UserInfo[])> OnGetAllUser => _getAllUserSubject;
        public IObservable<(OucrcNetType,UserInfo[])> OnGetAllAI => _getAllAISubject;
        public IObservable<(OucrcNetType,RoomIdUsersAndBoard[])> OnGetAllRoom => _getAllRoomSubject;
        protected override void MyAwake() { }
        private void Start() {
            var a = new BoardStatusPoller(OucrcNetType.Watch);
            var token = gameObject.GetCancellationTokenOnDestroy();
            ServerGetLoop(token).Forget();
        }
        private async UniTask ServerGetLoop(CancellationToken token) {
            while(true) {
                GetUserTask(OucrcNetType.Watch,token).Forget();
                GetUserTask(OucrcNetType.Battle,token).Forget();
                GetAITask(OucrcNetType.Battle,token).Forget();
                GetRoomTask(OucrcNetType.Watch,token).Forget();
                GetRoomTask(OucrcNetType.Battle,token).Forget();
                await UniTask.Yield();
            }
        }
        private async UniTask GetUserTask(OucrcNetType oucrcNetType,CancellationToken token) {
            _getAllUserSubject.OnNext((oucrcNetType,await ServerUtility.Instance.GetAllUsers(oucrcNetType,false,token)));
        }
        private async UniTask GetAITask(OucrcNetType oucrcNetType,CancellationToken token) {
            _getAllAISubject.OnNext((oucrcNetType,await ServerUtility.Instance.GetAllUsers(oucrcNetType,true,token)));
        }
        private async UniTask GetRoomTask(OucrcNetType oucrcNetType,CancellationToken token) {
            _getAllRoomSubject.OnNext((oucrcNetType,await ServerUtility.Instance.GetAllRooms(oucrcNetType,token)));
        }
    }
}