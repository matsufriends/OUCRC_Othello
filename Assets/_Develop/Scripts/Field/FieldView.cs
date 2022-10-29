using System;
using System.Collections.Generic;
using Cell;
using UnityEngine;
namespace Field {
    public sealed class FieldView : IDisposable {
        private readonly FieldMono _field;
        private readonly Dictionary<Vector2Int,CellMono> _cellDictionary = new();
        private readonly Vector3 _offset;
        public FieldView(Vector3 offset) {
            _offset                   = offset;
            _field                    = FieldObjectPoolMono.Instance.Pop();
            _field.transform.position = offset;
        }
        public void UpdateCell(CellUpdateInfo cellUpdateInfo) {
            if(_cellDictionary.TryGetValue(cellUpdateInfo.Pos,out var cell) == false) {
                cell = CellPoolMono.Instance.Pop();
                _cellDictionary.Add(cellUpdateInfo.Pos,cell);
            }
            cell.Set(_offset,cellUpdateInfo);
        }
        public void Dispose() {
            foreach(var (_,value) in _cellDictionary) {
                CellPoolMono.Instance.Push(value);
            }
            UnityEngine.Object.Destroy(_field.gameObject);
        }
    }
}