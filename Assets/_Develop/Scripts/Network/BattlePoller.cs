using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using OucrcReversi.Board;
using OucrcReversi.Cell;
using OucrcReversi.Scene;
using UniRx;
using UnityEngine;
namespace OucrcReversi.Network {
    public sealed class BattlePoller : IDisposable {
        private readonly string _userId;
        private readonly int _maxRoomCount;
        private readonly OucrcNetType _oucrcNetType;
        private BoardPresenter _presenter;
        private readonly CancellationTokenSource _tokenSource = new();
        private readonly Vector3 _offset;
        public BattlePoller(OucrcNetType oucrcNetType,Vector3 offset,string roomId,string userId) {
            _oucrcNetType = oucrcNetType;
            _offset       = offset;
            _userId       = userId;
            GameManagerMono.Instance.OnGetAllRoom.Where(tuple => tuple.Item1 == oucrcNetType).Subscribe(
                tuple => {
                    var rooms = tuple.Item2;
                    var room = rooms.FirstOrDefault(x => x.id == roomId);
                    if(room != null) UpdateRoom(room);
                }
            ).AddTo(_tokenSource.Token);
        }
        public void Dispose() {
            _presenter?.Dispose();
            _tokenSource.Cancel();
        }
        private BoardPresenter GenerateBoard(RoomIdUsersAndBoard room) {
            var view = BoardView3dObjectPoolMono.Instance.Rent();
            view.Init(_offset,room.BoardSize,_oucrcNetType,room.black.name,room.white.name,room.black.id == _userId ? CellColor.Black : CellColor.White);
            view.OnPut.Subscribe(
                pos => {
                    ServerUtility.Instance.PostPlayerPutData(
                        _oucrcNetType,room.id,new PlayerPutPostData {
                            user_id = _userId
                           ,is_user = true
                           ,row     = pos.y
                           ,column  = pos.x
                        },_tokenSource.Token
                    ).Forget();
                }
            ).AddTo(_tokenSource.Token);
            return new BoardPresenter(new BoardModel(room.BoardSize),view);
        }
        private void UpdateRoom(RoomIdUsersAndBoard room) {
            _presenter ??= GenerateBoard(room);
            var roomCellCount = room.GetCellCount();
            var presenterCellCount = _presenter.GetCellCount();
            if(presenterCellCount == roomCellCount) return;
            if(presenterCellCount + 1 == roomCellCount) ApplyOnePut(room,_presenter);
            else _presenter.InitializeBoard(room.GetGrid(),room.NextCellColor);
        }
        private static void ApplyOnePut(RoomIdUsersAndBoard room,BoardPresenter presenter) {
            var roomGrid = room.GetGrid();
            for(var y = 0;y < room.BoardSize.y;y++) {
                for(var x = 0;x < room.BoardSize.x;x++) {
                    var pos = new Vector2Int(x,y);
                    if(presenter.TryGetCellColor(pos,out var cellColor) && cellColor == CellColor.None && roomGrid[x,y] != CellColor.None) {
                        if(presenter.TryPut(pos,roomGrid[x,y])) return;
                        Debug.Log("なんでやねん" + pos + " " + roomGrid[x,y]);
                        presenter.InitializeBoard(roomGrid,room.NextCellColor);
                        return;
                    }
                }
            }
        }
    }
}