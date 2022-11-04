using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using OucrcReversi.Board;
using OucrcReversi.Cell;
using OucrcReversi.Scene;
using UniRx;
using UnityEngine;
namespace OucrcReversi.Network {
    public sealed class BoardStatusPoller : IDisposable {
        private readonly string _userId;
        private const int c_maxRoomCount = 9;
        private readonly OucrcNetType _oucrcNetType;
        private readonly ValueTuple<int,BoardPresenter>[] _presenters;
        private readonly CancellationTokenSource _tokenSource = new();
        public BoardStatusPoller(OucrcNetType oucrcNetType) {
            _oucrcNetType = oucrcNetType;
            _presenters   = new ValueTuple<int,BoardPresenter>[c_maxRoomCount];
            for(var i = 0;i < c_maxRoomCount;i++) _presenters[i] = new ValueTuple<int,BoardPresenter>(-1,null);
            GameManagerMono.Instance.OnGetAllRoom.Where(tuple => tuple.Item1 == oucrcNetType && tuple.Item2 != null).Subscribe(
                tuple => {
                    var rooms = tuple.Item2;
                    var endRoomIndex = rooms.Length - 1;
                    var startRoomIndex = Mathf.Max(endRoomIndex - c_maxRoomCount + 1,0);
                    for(var i = startRoomIndex;i <= endRoomIndex;i++) UpdateRoom(rooms[i],i);
                }
            ).AddTo(_tokenSource.Token);
        }
        public void Dispose() {
            _tokenSource.Cancel();
        }
        private BoardPresenter GenerateBoard(int arrayIndex,RoomIdUsersAndBoard room) {
            var offset = new Vector3(0.5f - room.BoardSize.x / 2f,0,-0.5f + room.BoardSize.y / 2f);
            offset.x += (room.BoardSize.x + 12) * (arrayIndex % 3);
            offset.z += (room.BoardSize.y + 4) * (arrayIndex / 3);
            offset   += new Vector3(-20,0,-12);
            var view = BoardView3dObjectPoolMono.Instance.Rent();
            view.Init(offset,room.BoardSize,_oucrcNetType,room.black.name,room.white.name);
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