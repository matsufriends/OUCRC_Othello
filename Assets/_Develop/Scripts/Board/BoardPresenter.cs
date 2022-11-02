using System;
using System.Collections.Generic;
using OucrcReversi.Cell;
using UniRx;
using UnityEngine;
namespace OucrcReversi.Board {
    public sealed class BoardPresenter : IDisposable {
        private readonly BoardModel _boardModel;
        private readonly BoardView3D _boardView3D;
        private readonly CompositeDisposable _compositeDisposable = new();
        public BoardPresenter(Vector2Int size,Vector3 offset) {
            _boardModel = new BoardModel(size);
            _boardView3D  = new BoardView3D(offset);
            _compositeDisposable.Add(_boardModel);
            _compositeDisposable.Add(_boardView3D);
            _boardModel.OnGridChanged.Subscribe(_boardView3D.UpdateCell).AddTo(_compositeDisposable);
        }
        void IDisposable.Dispose() => _compositeDisposable.Dispose();
        public void InitializeBoard(CellColor[,] board) => _boardModel.InitializeBoard(board);
        public int GetCellCount() => _boardModel.GetCellCount();
        public bool TryGetCellColor(Vector2Int   pos,     out CellColor    cellColor) => _boardModel.TryGetCellColor(pos,out cellColor);
        public bool TryGetCanPutPosses(CellColor putColor,List<Vector2Int> canPutPosList) => _boardModel.TryGetCanPutPosses(putColor,canPutPosList);
        public bool TryPut(Vector2Int            putPos,  CellColor        putColor) => _boardModel.TryPut(putPos,putColor);
    }
}