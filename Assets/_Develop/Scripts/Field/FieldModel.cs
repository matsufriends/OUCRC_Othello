using System;
using System.Collections.Generic;
using Cell;
using UniRx;
using UnityEngine;
namespace Field {
    public class FieldModel : IDisposable {
        public readonly Vector2Int Size;
        private readonly CellColor[,] _cellGrid;
        private readonly Subject<CellUpdateInfo> _updateCellSubject = new();
        public IObservable<CellUpdateInfo> OnUpdateCell => _updateCellSubject;
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
        public FieldModel(Vector2Int size) {
            if(size.x % 2 != 0 || size.y % 2 != 0) throw new ArgumentException($"サイズが偶数じゃない:({size})");
            Size      = size;
            _cellGrid = new CellColor[size.x,size.y];
        }
        public void Init() {
            //左下、右上が黒
            //右下、左上が白
            SetCell(new Vector2Int(Size.x / 2,Size.y / 2),CellColor.White,0);
            SetCell(new Vector2Int(Size.x / 2 - 1,Size.y / 2 - 1),CellColor.White,0);
            SetCell(new Vector2Int(Size.x / 2 - 1,Size.y / 2),CellColor.Black,0);
            SetCell(new Vector2Int(Size.x / 2,Size.y / 2 - 1),CellColor.Black,0);
        }
        private bool IsInner(Vector2Int pos) {
            return 0 <= pos.x && pos.x < Size.x && 0 <= pos.y && pos.y < Size.y;
        }
        public bool TryGetCell(Vector2Int pos,out CellColor cellColor) {
            if(IsInner(pos) == false) {
                cellColor = CellColor.None;
                return false;
            }
            cellColor = _cellGrid[pos.x,pos.y];
            return true;
        }
        private void SetCell(Vector2Int pos,CellColor cellColor,int animOffset) {
            if(IsInner(pos) == false) throw new ArgumentException("配列の範囲外です");
            _cellGrid[pos.x,pos.y] = cellColor;
            _updateCellSubject.OnNext(new CellUpdateInfo(pos,cellColor,animOffset));
        }
        public bool TryPut(Vector2Int putPos,CellColor putColor) {
            var flipPosList = new List<Vector2Int>();
            if(TryGetFlipPosses(putPos,putColor,flipPosList) == false) return false;
            SetCell(putPos,putColor,0);
            foreach(var flipPos in flipPosList) {
                var dif = putPos - flipPos;
                var animOffset = Mathf.Max(Mathf.Abs(dif.x),Mathf.Abs(dif.y));
                SetCell(flipPos,putColor,animOffset);
            }
            return true;
        }
        public bool TryGetFlipPosses(Vector2Int putPos,CellColor putColor,List<Vector2Int> flipPosList) {
            if(flipPosList == null) return false;
            flipPosList.Clear();
            if(TryGetCell(putPos,out var cellColor) == false || cellColor != CellColor.None) {
                return false;
            }
            foreach(var dir in s_dirVec) {
                var hasOpposite = false;
                for(var dirLen = 1;dirLen < Mathf.Max(Size.x,Size.y);dirLen++) {
                    if(TryGetCell(putPos + dir * dirLen,out var checkCellColor) == false || checkCellColor == CellColor.None) break;
                    if(CellColorEx.GetOpposite(putColor) == checkCellColor) {
                        hasOpposite = true;
                    } else if(putColor == checkCellColor) {
                        if(hasOpposite == false) break;
                        for(var i = 1;i < dirLen;i++) flipPosList.Add(putPos + dir * i);
                        break;
                    } else break;
                }
            }
            return flipPosList.Count > 0;
        }
        public void Dispose() {
            _updateCellSubject?.Dispose();
        }
    }
}