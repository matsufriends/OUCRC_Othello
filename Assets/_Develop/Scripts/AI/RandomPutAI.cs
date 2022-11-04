using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MornLib.Extensions;
using OucrcReversi.Board;
using OucrcReversi.Cell;
using OucrcReversi.Network;
using UnityEngine;
namespace OucrcReversi.AI {
    public sealed class RandomPutAI : IDisposable {
        private readonly string _userName;
        private readonly OucrcNetType _oucrcNetType;
        private readonly CancellationTokenSource _tokenSource = new();
        private readonly List<Vector2Int> _placeablePosList = new();
        public RandomPutAI(string name,OucrcNetType oucrcNetType) {
            _userName     = name;
            _oucrcNetType = oucrcNetType;
            WatchLoopTask().Forget();
        }
        public void Dispose() {
            _tokenSource.Cancel();
        }
        private async UniTask WatchLoopTask() {
            var userId = (await ServerUtility.Instance.PostRegisterUser(
                              _oucrcNetType,new RegisterUserPostData {
                                  user_name = _userName
                              }
                          )).id;
            while(true) {
                var userInfo = await ServerUtility.Instance.GetUser(_oucrcNetType,userId,_tokenSource.Token);
                if(userInfo != null) {
                    var room = await ServerUtility.Instance.GetRoom(_oucrcNetType,userInfo.id,_tokenSource.Token);
                    if(room != null)
                        if(room.next.id == userInfo.id) {
                            var color = room.black.id == room.next.id ? CellColor.Black : CellColor.White;
                            var model = new BoardModel(room.BoardSize,color);
                            model.InitializeBoard(room.GetGrid(),color);
                            model.GetPlaceablePos(_placeablePosList);
                            if(_placeablePosList.Count > 0) {
                                var pos = _placeablePosList.RandomValue();
                                await ServerUtility.Instance.PostPutData(
                                    _oucrcNetType,room.id,new PutPostData {
                                        user_id = userId
                                       ,is_user = false
                                       ,row     = pos.y
                                       ,column  = pos.x
                                    }
                                );
                            }
                        }
                }
                await UniTask.Delay(TimeSpan.FromSeconds(ServerUtility.WatchInterval),cancellationToken: _tokenSource.Token);
            }
        }
    }
}