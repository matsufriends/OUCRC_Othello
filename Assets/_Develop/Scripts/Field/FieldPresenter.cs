using System;
using System.Collections.Generic;
using Cell;
using oucrcNet;
using UniRx;
using UnityEngine;
namespace Field {
    public sealed class FieldPresenter : IDisposable {
        private readonly FieldModel _fieldModel;
        private readonly FieldView _fieldView;
        private readonly CompositeDisposable _compositeDisposable = new();
        public Vector2Int Size => _fieldModel.Size;
        public FieldPresenter(Vector2Int size,Vector3 offset) {
            _fieldModel = new FieldModel(size);
            _fieldView  = new FieldView(offset);
            _compositeDisposable.Add(_fieldModel);
            _compositeDisposable.Add(_fieldView);
            _fieldModel.OnUpdateCell.Subscribe(_fieldView.UpdateCell).AddTo(_compositeDisposable);
            _fieldModel.Init();
        }
        public void ReceiveData(ReceiveInfo receive) {
            _fieldModel.ReceiveData(receive);
        }
        public void Dispose() {
            _compositeDisposable.Dispose();
        }
        public bool TryGetCell(Vector2Int pos,out CellColor cellColor) {
            return _fieldModel.TryGetCell(pos,out cellColor);
        }
        public bool TryGetCanPutPosses(CellColor putColor,List<Vector2Int> canPutPosList) {
            return _fieldModel.TryGetCanPutPosses(putColor,canPutPosList);
        }
        public bool TryGetFlipPosses(Vector2Int putPos,CellColor putColor,List<Vector2Int> flipPosList) {
            return _fieldModel.TryGetFlipPosses(putPos,putColor,flipPosList);
        }
        public bool TryPut(Vector2Int putPos,CellColor putColor) {
            return _fieldModel.TryPut(putPos,putColor);
        }
    }
}