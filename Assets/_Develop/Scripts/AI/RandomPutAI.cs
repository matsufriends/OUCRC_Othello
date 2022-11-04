using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MornLib.Extensions;
using OucrcReversi.Board;
using OucrcReversi.Cell;
using OucrcReversi.Network;
using OucrcReversi.Scene;
using UniRx;
using UnityEngine;
namespace OucrcReversi.AI {
    public sealed class RandomPutAI : IDisposable {
        private readonly string _userId;
        private readonly OucrcNetType _oucrcNetType;
        private readonly CancellationTokenSource _tokenSource = new();
        private readonly List<Vector2Int> _placeablePosList = new();
        private UserInfo _userInfo;
        private RoomIdUsersAndBoard _room;
        public RandomPutAI(string userId,OucrcNetType oucrcNetType) {
            _userId       = userId;
            _oucrcNetType = oucrcNetType;
            if(oucrcNetType == OucrcNetType.Battle)
                GameManagerMono.Instance.OnGetAllAI.Where(tuple => tuple.Item1 == oucrcNetType).Subscribe(
                    tuple => {
                        _userInfo = tuple.Item2.FirstOrDefault(x => x.id == _userId);
                    }
                ).AddTo(_tokenSource.Token);
            GameManagerMono.Instance.OnGetAllUser.Where(tuple => tuple.Item1 == oucrcNetType).Subscribe(
                tuple => {
                    _userInfo = tuple.Item2.FirstOrDefault(x => x.id == _userId);
                }
            ).AddTo(_tokenSource.Token);
            GameManagerMono.Instance.OnGetAllRoom.Where(tuple => tuple.Item1 == oucrcNetType && _userInfo != null).Subscribe(
                tuple => {
                    _room = tuple.Item2.FirstOrDefault(x => x.id == _userInfo.status);
                }
            ).AddTo(_tokenSource.Token);
            WatchLoopTask().Forget();
        }
        public void Dispose() {
            _tokenSource.Cancel();
        }
        private async UniTask WatchLoopTask() {
            while(true) {
                if(_room != null && _userInfo != null && _room.next != null && _room.next.id == _userInfo.id) {
                    var color = _room.black.id == _room.next.id ? CellColor.Black : CellColor.White;
                    var model = new BoardModel(_room.BoardSize);
                    model.InitializeBoard(_room.GetGrid(),color);
                    model.GetPlaceablePos(_placeablePosList);
                    if(_placeablePosList.Count > 0) {
                        var pos = _placeablePosList.RandomValue();
                        await ServerUtility.Instance.PostAiPutData(
                            _oucrcNetType,_room.id,new AIPutPostData {
                                user_id = _userId
                               ,row     = pos.y
                               ,column  = pos.x
                            },_tokenSource.Token
                        );
                    }
                }
                await UniTask.Yield(_tokenSource.Token);
            }
        }
    }
}