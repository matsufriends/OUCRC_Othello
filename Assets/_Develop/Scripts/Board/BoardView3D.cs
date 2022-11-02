using System;
using System.Collections.Generic;
using OucrcReversi.Cell;
using UnityEngine;
using Object = UnityEngine.Object;
namespace OucrcReversi.Board {
    public sealed class BoardView3D : IDisposable {
        private readonly BoardObjectMono _boardObject;
        private readonly Vector3 _boardOffset;
        private readonly Dictionary<Vector2Int,CellMono> _cellDictionary = new();
        public BoardView3D(Vector3 boardOffset) {
            _boardOffset                    = boardOffset;
            _boardObject                    = BordObjectPoolMono.Instance.Pop();
            _boardObject.transform.position = boardOffset;
        }
        void IDisposable.Dispose() {
            foreach(var value in _cellDictionary.Values) {
                CellPoolMono.Instance.Push(value);
            }
            Object.Destroy(_boardObject.gameObject);
        }
        public void UpdateCell(CellUpdateInfo cellUpdateInfo) {
            if(_cellDictionary.TryGetValue(cellUpdateInfo.Pos,out var cell) == false) {
                cell = CellPoolMono.Instance.Pop();
                _cellDictionary.Add(cellUpdateInfo.Pos,cell);
            }
            cell.Set(_boardOffset,cellUpdateInfo);
        }
    }
}