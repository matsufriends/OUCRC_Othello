using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MornLib.Singletons;
using OucrcReversi.Cell;
using OucrcReversi.Network;
using UniRx;
using UnityEngine;
namespace OucrcReversi.Scene {
    public class GameManagerMono : SingletonMono<GameManagerMono> {
        private readonly Subject<(OucrcNetType,UserInfo[])> _getAllAISubject = new();
        private readonly Subject<(OucrcNetType,RoomIdUsersAndBoard[])> _getAllRoomSubject = new();
        private readonly Subject<(OucrcNetType,UserInfo[])> _getAllUserSubject = new();
        private readonly Dictionary<string,BattleResult> _resultDictionary = new();
        public IObservable<(OucrcNetType,UserInfo[])> OnGetAllUser => _getAllUserSubject;
        public IObservable<(OucrcNetType,UserInfo[])> OnGetAllAI => _getAllAISubject;
        public IObservable<(OucrcNetType,RoomIdUsersAndBoard[])> OnGetAllRoom => _getAllRoomSubject;
        private void Start() {
            var a = new BoardStatusPoller(OucrcNetType.Watch,9,new Vector3(-20,0,-12),"");
            var token = gameObject.GetCancellationTokenOnDestroy();
            ServerGetLoop(token).Forget();
        }
        protected override void MyAwake() { }
        public bool TryGetBattleResult(string userId,out BattleResult battleResult) {
            return _resultDictionary.TryGetValue(userId,out battleResult);
        }
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
            if(rooms != null) {
                _resultDictionary.Clear();
                foreach(var room in rooms) {
                    if(room.next != null) continue;
                    if(_resultDictionary.ContainsKey(room.black.id) == false) _resultDictionary.Add(room.black.id,new BattleResult());
                    if(_resultDictionary.ContainsKey(room.white.id) == false) _resultDictionary.Add(room.white.id,new BattleResult());
                    _resultDictionary[room.black.id].Battles++;
                    _resultDictionary[room.white.id].Battles++;
                    var black = room.GetCellCount(CellColor.Black);
                    var white = room.GetCellCount(CellColor.White);
                    if(black > white) _resultDictionary[room.black.id].Wins++;
                    if(white > black) _resultDictionary[room.white.id].Wins++;
                }
                _getAllRoomSubject.OnNext((oucrcNetType,rooms));
            }
        }
    }
    public class BattleResult {
        public int Battles;
        public int Wins;
    }
}