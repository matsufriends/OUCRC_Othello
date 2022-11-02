using System;
using System.Collections.Generic;
using System.Linq;
using OucrcReversi.Cell;
using UniRx;
using UnityEngine;
namespace OucrcReversi.Reversi {
    public class ReversiModel : IDisposable {
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
        private readonly CellColor[,] _cellGrid;
        private readonly Subject<CellUpdateInfo> _gridChangedSubject = new();
        private readonly Vector2Int _size;
        public ReversiModel(Vector2Int size) {
            if(size.x % 2 != 0 || size.y % 2 != 0) throw new ArgumentException($"サイズが偶数じゃない:({size})");
            _size     = size;
            _cellGrid = new CellColor[size.x,size.y];
        }
        public IObservable<CellUpdateInfo> OnGridChanged => _gridChangedSubject;
        void IDisposable.Dispose() => _gridChangedSubject?.Dispose();
        public void InitializeBoard(CellColor[,] board) {
            if(board.GetLength(0) != _size.x) throw new ArgumentException("盤面の幅の不一致");
            if(board.GetLength(1) != _size.y) throw new ArgumentException("盤面の高さの不一致");
            for(var x = 0;x < _size.x;x++) {
                for(var y = 0;y < _size.y;y++) {
                    if(board[x,y] == _cellGrid[x,y]) continue;
                    ChangeCell(new Vector2Int(x,y),board[x,y],0);
                }
            }
        }
        private bool IsInner(Vector2Int pos) => 0 <= pos.x && pos.x < _size.x && 0 <= pos.y && pos.y < _size.y;
        public int GetCellCount() => _cellGrid.Cast<CellColor>().Count(cellColor => cellColor != CellColor.None);
        public bool TryGetCellColor(Vector2Int pos,out CellColor cellColor) {
            if(IsInner(pos) == false) {
                cellColor = CellColor.None;
                return false;
            }
            cellColor = _cellGrid[pos.x,pos.y];
            return true;
        }
        private void ChangeCell(Vector2Int pos,CellColor cellColor,int animOffset) {
            if(IsInner(pos) == false) throw new ArgumentException("配列の範囲外です");
            if(_cellGrid[pos.x,pos.y] == cellColor) return;
            _cellGrid[pos.x,pos.y] = cellColor;
            _gridChangedSubject.OnNext(new CellUpdateInfo(pos,cellColor,animOffset));
        }
        public void ForcePut(Vector2Int pos,CellColor cellColor) => ChangeCell(pos,cellColor,0);
        public bool TryPut(Vector2Int putPos,CellColor putColor) {
            var flipPosList = new List<Vector2Int>();
            if(TryGetFlipPosses(putPos,putColor,flipPosList) == false) return false;
            ChangeCell(putPos,putColor,0);
            foreach(var flipPos in flipPosList) {
                var dif = putPos - flipPos;
                var animOffset = Mathf.Max(Mathf.Abs(dif.x),Mathf.Abs(dif.y));
                ChangeCell(flipPos,putColor,animOffset);
            }
            return true;
        }
        public bool TryGetCanPutPosses(CellColor putColor,List<Vector2Int> canPutPosList) {
            canPutPosList.Clear();
            for(var x = 0;x < _size.y;x++) {
                for(var y = 0;y < _size.x;y++) {
                    var checkPos = new Vector2Int(x,y);
                    if(TryGetFlipPosses(checkPos,putColor,null,true)) canPutPosList.Add(checkPos);
                }
            }
            return canPutPosList.Count > 0;
        }
        private bool TryGetFlipPosses(Vector2Int putPos,CellColor putColor,List<Vector2Int> flipPosList) {
            if(flipPosList == null) return false;
            flipPosList.Clear();
            return TryGetFlipPosses(putPos,putColor,flipPosList,false);
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
                        for(var i = 1;i < dirLen;i++) {
                            flipPosList.Add(putPos + dir * i);
                        }
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