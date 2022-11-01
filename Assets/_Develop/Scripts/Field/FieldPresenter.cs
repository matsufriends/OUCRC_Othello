using System;
using System.Collections.Generic;
using Cell;
using UniRx;
using UnityEngine;
namespace Field {
    public sealed class FieldPresenter : IDisposable {
        private readonly CompositeDisposable _compositeDisposable = new();
        private readonly FieldModel _fieldModel;
        private readonly FieldView _fieldView;
        public FieldPresenter(Vector2Int size,Vector3 offset) {
            _fieldModel = new FieldModel(size);
            _fieldView  = new FieldView(offset);
            _compositeDisposable.Add(_fieldModel);
            _compositeDisposable.Add(_fieldView);
            _fieldModel.OnGridChanged.Subscribe(_fieldView.UpdateCell).AddTo(_compositeDisposable);
            _fieldModel.ForcePut(new Vector2Int(size.x / 2,size.y / 2),CellColor.White);
            _fieldModel.ForcePut(new Vector2Int(size.x / 2 - 1,size.y / 2 - 1),CellColor.White);
            _fieldModel.ForcePut(new Vector2Int(size.x / 2 - 1,size.y / 2),CellColor.Black);
            _fieldModel.ForcePut(new Vector2Int(size.x / 2,size.y / 2 - 1),CellColor.Black);
        }
        public void Dispose() => _compositeDisposable.Dispose();
        public void InitializeBoard(CellColor[,] board) => _fieldModel.InitializeBoard(board);
        public bool TryGetCell(Vector2Int        pos,     out CellColor    cellColor) => _fieldModel.TryGetCellColor(pos,out cellColor);
        public bool TryGetCanPutPosses(CellColor putColor,List<Vector2Int> canPutPosList) => _fieldModel.TryGetCanPutPosses(putColor,canPutPosList);
        public bool TryGetFlipPosses(Vector2Int putPos,CellColor putColor,List<Vector2Int> flipPosList)
            => _fieldModel.TryGetFlipPosses(putPos,putColor,flipPosList);
        public bool TryPut(Vector2Int putPos,CellColor putColor) => _fieldModel.TryPut(putPos,putColor);
    }
}