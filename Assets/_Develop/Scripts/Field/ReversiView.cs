using System;
using System.Collections.Generic;
using Cell;
using UnityEngine;
using Object = UnityEngine.Object;
namespace Field {
    public sealed class ReversiView : IDisposable {
        private readonly Dictionary<Vector2Int,CellMono> _cellDictionary = new();
        private readonly FieldMono _field;
        private readonly Vector3 _offset;
        public ReversiView(Vector3 offset) {
            _offset                   = offset;
            _field                    = FieldObjectPoolMono.Instance.Pop();
            _field.transform.position = offset;
        }
        void IDisposable.Dispose() {
            foreach(var (_,value) in _cellDictionary) {
                CellPoolMono.Instance.Push(value);
            }
            Object.Destroy(_field.gameObject);
        }
        public void UpdateCell(CellUpdateInfo cellUpdateInfo) {
            if(_cellDictionary.TryGetValue(cellUpdateInfo.Pos,out var cell) == false) {
                cell = CellPoolMono.Instance.Pop();
                _cellDictionary.Add(cellUpdateInfo.Pos,cell);
            }
            cell.Set(_offset,cellUpdateInfo);
        }
    }
}