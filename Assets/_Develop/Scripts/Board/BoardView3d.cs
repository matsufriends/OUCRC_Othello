using System;
using System.Collections.Generic;
using OucrcReversi.Cell;
using UnityEngine;
namespace OucrcReversi.Board {
    public sealed class BoardView3d : MonoBehaviour,IBoardView {
        private readonly Dictionary<Vector2Int,CellMono> _cellDictionary = new();
        void IDisposable.Dispose() {
            foreach(var value in _cellDictionary.Values) {
                CellPoolMono.Instance.Push(value);
            }
            Destroy(gameObject);
        }
        public void UpdateCell(CellUpdateInfo cellUpdateInfo) {
            if(_cellDictionary.TryGetValue(cellUpdateInfo.Pos,out var cell) == false) {
                cell = CellPoolMono.Instance.Pop();
                _cellDictionary.Add(cellUpdateInfo.Pos,cell);
            }
            cell.Set(transform.position,cellUpdateInfo);
        }
        public void Init(Vector3 boardOffset) => transform.position = boardOffset;
    }
}