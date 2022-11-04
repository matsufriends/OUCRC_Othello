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
        private readonly string _userId;
        private readonly OucrcNetType _oucrcNetType;
        private readonly CancellationTokenSource _tokenSource = new();
        private readonly List<Vector2Int> _placeablePosList = new();
        public RandomPutAI(string userId,OucrcNetType oucrcNetType) {
            _userId       = userId;
            _oucrcNetType = oucrcNetType;
            WatchLoopTask().Forget();
        }
        public void Dispose() {
            _tokenSource.Cancel();
        }
        private async UniTask WatchLoopTask() {
            while(true) {
                var userInfo = await ServerUtility.Instance.GetUser(_oucrcNetType,_userId,_tokenSource.Token);
                if(userInfo != null) {
                    var room = await ServerUtility.Instance.GetRoom(_oucrcNetType,userInfo.status,_tokenSource.Token);
                    if(room != null)
                        if(room.next.id == userInfo.id) {
                            var color = room.black.id == room.next.id ? CellColor.Black : CellColor.White;
                            var model = new BoardModel(room.BoardSize,color);
                            model.InitializeBoard(room.GetGrid(),color);
                            model.GetPlaceablePos(_placeablePosList);
                            if(_placeablePosList.Count > 0) {
                                var pos = _placeablePosList.RandomValue();
                                await ServerUtility.Instance.PostAiPutData(
                                    _oucrcNetType,room.id,new AIPutPostData {
                                        user_id = _userId
                                       ,row     = pos.y
                                       ,column  = pos.x
                                    },_tokenSource.Token
                                );
                            }
                        }
                }
                await UniTask.Delay(TimeSpan.FromSeconds(ServerUtility.WatchInterval),cancellationToken: _tokenSource.Token);
            }
        }
    }
}