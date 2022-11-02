using System;
using System.Collections.Generic;
using OucrcReversi.Cell;
using UniRx;
using UnityEngine;
namespace OucrcReversi.Board {
    public sealed class BoardPresenter : IDisposable {
        private readonly IBoardModel _boardModel;
        private readonly IBoardView _boardView;
        private readonly CompositeDisposable _compositeDisposable = new();
        public BoardPresenter(IBoardModel boardModel,IBoardView boardView) {
            _boardModel = boardModel;
            _boardView  = boardView;
            _compositeDisposable.Add(_boardModel);
            _compositeDisposable.Add(_boardView);
            _boardModel.OnGridChanged.Subscribe(_boardView.UpdateCell).AddTo(_compositeDisposable);
        }
        void IDisposable.Dispose() => _compositeDisposable.Dispose();
        public void InitializeBoard(CellColor[,] board) => _boardModel.InitializeBoard(board);
        public int GetCellCount() => _boardModel.GetCellCount();
        public bool TryGetCellColor(Vector2Int   pos,     out CellColor    cellColor) => _boardModel.TryGetCellColor(pos,out cellColor);
        public bool TryGetCanPutPosses(CellColor putColor,List<Vector2Int> canPutPosList) => _boardModel.TryGetCanPutPosses(putColor,canPutPosList);
        public bool TryPut(Vector2Int            putPos,  CellColor        putColor) => _boardModel.TryPut(putPos,putColor);
    }
}