using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using OucrcReversi.Board;
using UnityEngine;
namespace OucrcReversi.Network {
    public sealed class BoardStatusPoller : IDisposable {
        private const int c_maxRoomCount = 5;
        private const float c_watchInterval = 0.1f;
        private readonly OucrcNetType _oucrcNetType;
        private readonly ValueTuple<int,BoardPresenter>[] _presenters;
        private readonly CancellationTokenSource _tokenSource = new();
        public BoardStatusPoller(OucrcNetType oucrcNetType) {
            _oucrcNetType = oucrcNetType;
            _presenters   = new ValueTuple<int,BoardPresenter>[c_maxRoomCount];
            for(var i = 0;i < c_maxRoomCount;i++) _presenters[i] = new ValueTuple<int,BoardPresenter>(-1,null);
            WatchLoopTask().Forget();
        }
        public void Dispose() {
            _tokenSource.Cancel();
        }
        private async UniTask WatchLoopTask() {
            while(true) {
                var rooms = await ServerUtility.Instance.GetAllRooms(_oucrcNetType,_tokenSource.Token);
                if(rooms != null) {
                    var endRoomIndex = rooms.Length - 1;
                    var startRoomIndex = Mathf.Max(endRoomIndex - 5,0);
                    for(var i = startRoomIndex;i < endRoomIndex;i++) UpdateRoom(rooms[i],i);
                }
                await UniTask.Delay(TimeSpan.FromSeconds(c_watchInterval),cancellationToken: _tokenSource.Token);
            }
        }
        private BoardPresenter GenerateBoard(int arrayIndex,RoomIdUsersAndBoard room) {
            var offset = new Vector3(0.5f - room.BoardSize.x / 2f,0,-0.5f + room.BoardSize.y / 2f);
            offset.x += (room.BoardSize.x + 4) * arrayIndex;
            var view = BoardView3dObjectPoolMono.Instance.Rent();
            view.Init(offset,room.BoardSize,_oucrcNetType);
            return new BoardPresenter(new BoardModel(room.BoardSize,room.NextCellColor),view);
        }
        private void UpdateRoom(RoomIdUsersAndBoard room,int roomIndex) {
            var arrayIndex = roomIndex % c_maxRoomCount;
            if(_presenters[arrayIndex].Item1 != roomIndex) {
                _presenters[arrayIndex].Item2?.Dispose();
                _presenters[arrayIndex] = new ValueTuple<int,BoardPresenter>(roomIndex,GenerateBoard(arrayIndex,room));
            }
            var presenter = _presenters[arrayIndex].Item2;
            var roomCellCount = room.GetCellCount();
            var presenterCellCount = presenter.GetCellCount();
            if(presenterCellCount == roomCellCount) return;
            if(presenterCellCount + 1 == roomCellCount) ApplyOnePut(room,presenter);
            else presenter.InitializeBoard(room.GetGrid(),room.NextCellColor);
        }
        private static void ApplyOnePut(RoomIdUsersAndBoard room,BoardPresenter presenter) {
            var roomGrid = room.GetGrid();
            for(var y = 0;y < room.BoardSize.y;y++) {
                for(var x = 0;x < room.BoardSize.x;x++) {
                    var pos = new Vector2Int(x,y);
                    if(presenter.TryGetCellColor(pos,out var cellColor) && cellColor != roomGrid[x,y]) {
                        presenter.TryPut(pos);
                        return;
                    }
                }
            }
        }
    }
}