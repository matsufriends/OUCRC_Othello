using System;
using System.Collections.Generic;
using System.Linq;
using MornLib.Cores;
using MornLib.Pool;
using OucrcReversi.Cell;
using UniRx;
using UnityEngine;
namespace OucrcReversi.Board {
    public class BoardModel : IBoardModel {
        private static readonly Vector2Int[] s_dirVec = {
            new(-1,-1)
           ,new(-1,0)
           ,new(-1,1)
           ,new(0,-1)
           ,new(0,1)
           ,new(1,-1)
           ,new(1,0)
           ,new(1,1)
        };
        private readonly Vector2Int _size;
        private CellColor _nextCellColor;
        private readonly CellColor[,] _cellGrid;
        private readonly List<Vector2Int> _placeablePosList = new();
        private readonly Subject<(int,int)> _countChangeSubject = new();
        private readonly Subject<CellUpdateInfo> _gridChangedSubject = new();
        private readonly Subject<(CellColor,IEnumerable<Vector2Int>)> _placeablePosSubject = new();
        public IObservable<(CellColor,IEnumerable<Vector2Int>)> OnPlaceablePosChanged => _placeablePosSubject;
        public IObservable<(int,int)> OnCountChanged => _countChangeSubject;
        public IObservable<CellUpdateInfo> OnGridChanged => _gridChangedSubject;
        public BoardModel(Vector2Int size) {
            if(size.x % 2 != 0 || size.y % 2 != 0) throw new ArgumentException($"サイズが偶数じゃない:({size})");
            _size          = size;
            _cellGrid      = new CellColor[size.x,size.y];
            _nextCellColor = CellColor.None;
        }
        public void InitializeBoard(CellColor[,] board,CellColor nextCellColor) {
            if(board.GetLength(0) != _size.x) throw new ArgumentException("盤面の幅の不一致");
            if(board.GetLength(1) != _size.y) throw new ArgumentException("盤面の高さの不一致");
            for(var x = 0;x < _size.x;x++) {
                for(var y = 0;y < _size.y;y++) {
                    if(board[x,y] == _cellGrid[x,y]) continue;
                    ChangeCell(new Vector2Int(x,y),board[x,y],0);
                }
            }
            _nextCellColor = nextCellColor;
            _countChangeSubject.OnNext((GetCellCount(CellColor.Black),GetCellCount(CellColor.White)));
            UpdatePlaceablePos();
        }
        public void Dispose() {
            _gridChangedSubject?.Dispose();
        }
        private int GetCellCount(CellColor cellColor) {
            return _cellGrid.Cast<CellColor>().Count(x => x == cellColor);
        }
        public int GetCellCount() {
            return _cellGrid.Cast<CellColor>().Count(cellColor => cellColor != CellColor.None);
        }
        private bool IsInner(Vector2Int pos) {
            return 0 <= pos.x && pos.x < _size.x && 0 <= pos.y && pos.y < _size.y;
        }
        public bool TryGetCellColor(Vector2Int pos,out CellColor cellColor) {
            if(IsInner(pos) == false) {
                cellColor = CellColor.None;
                return false;
            }
            cellColor = _cellGrid[pos.x,pos.y];
            return true;
        }
        public bool TryPut(Vector2Int putPos) {
            return TryPut(putPos,_nextCellColor);
        }
        public bool TryPut(Vector2Int putPos,CellColor cellColor) {
            var flipPosList = new List<Vector2Int>();
            if(TryGetFlipPosses(putPos,cellColor,flipPosList,false) == false) return false;
            ChangeCell(putPos,cellColor,0);
            foreach(var flipPos in flipPosList) {
                var dif = putPos - flipPos;
                var animOffset = Mathf.Max(Mathf.Abs(dif.x),Mathf.Abs(dif.y));
                ChangeCell(flipPos,cellColor,animOffset);
            }
            _nextCellColor = CellColorEx.GetOpposite(cellColor);
            _countChangeSubject.OnNext((GetCellCount(CellColor.Black),GetCellCount(CellColor.White)));
            UpdatePlaceablePos();
            return true;
        }
        public void Log() {
            var builder = MornSharedObjectPool<MornStringBuilder>.Rent();
            builder.Init(',');
            for(var y = 0;y < _size.y;y++) {
                for(var x = 0;x < _size.x;x++) builder.Append(_cellGrid[x,y].ToString());
                builder.Append("\n");
            }
            Debug.Log(builder.Get());
            MornSharedObjectPool<MornStringBuilder>.Return(builder);
        }
        private void ChangeCell(Vector2Int pos,CellColor cellColor,int animOffset) {
            if(IsInner(pos) == false) throw new ArgumentException("配列の範囲外です");
            if(_cellGrid[pos.x,pos.y] == cellColor) return;
            _cellGrid[pos.x,pos.y] = cellColor;
            _gridChangedSubject.OnNext(new CellUpdateInfo(pos,cellColor,animOffset));
        }
        private void UpdatePlaceablePos() {
            GetPlaceablePos(_placeablePosList);
            if(_placeablePosList.Count == 0) {
                _nextCellColor = CellColorEx.GetOpposite(_nextCellColor);
                GetPlaceablePos(_placeablePosList);
            }
            _placeablePosSubject.OnNext((_nextCellColor,_placeablePosList));
        }
        public void GetPlaceablePos(List<Vector2Int> placeablePosList) {
            if(placeablePosList == null) return;
            placeablePosList.Clear();
            if(_nextCellColor != CellColor.None)
                for(var x = 0;x < _size.y;x++) {
                    for(var y = 0;y < _size.x;y++) {
                        var checkPos = new Vector2Int(x,y);
                        if(TryGetFlipPosses(checkPos,_nextCellColor,null,true)) placeablePosList.Add(checkPos);
                    }
                }
        }
        private bool TryGetFlipPosses(Vector2Int putPos,CellColor putColor,List<Vector2Int> flipPosList,bool checkCanPutOnly) {
            if(TryGetCellColor(putPos,out var cellColor) == false || cellColor != CellColor.None) return false;
            foreach(var dir in s_dirVec) {
                var hasOpposite = false;
                for(var dirLen = 1;dirLen < Mathf.Max(_size.x,_size.y);dirLen++) {
                    if(TryGetCellColor(putPos + dir * dirLen,out var checkCellColor) == false || checkCellColor == CellColor.None) break;
                    if(CellColorEx.GetOpposite(putColor) == checkCellColor) {
                        hasOpposite = true;
                    } else if(putColor == checkCellColor) {
                        if(hasOpposite == false) break;
                        if(checkCanPutOnly) return true;
                        for(var i = 1;i < dirLen;i++) flipPosList.Add(putPos + dir * i);
                        break;
                    } else {
                        break;
                    }
                }
            }
            return checkCanPutOnly == false && flipPosList.Count > 0;
        }
    }
}