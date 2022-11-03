using System;
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
            _boardModel.OnPlaceablePosChanged.Subscribe(_boardView.UpdatePlaceablePos).AddTo(_compositeDisposable);
            _boardModel.OnPlaceablePosChanged.Subscribe(_boardView.UpdatePlaceablePos).AddTo(_compositeDisposable);
            _boardView.OnPut.Subscribe(x => _boardModel.TryPut(x)).AddTo(_compositeDisposable);
        }
        public void Dispose() {
            _compositeDisposable.Dispose();
        }
        public void InitializeBoard(CellColor[,] board,CellColor nextCellColor) {
            _boardModel.InitializeBoard(board,nextCellColor);
        }
        public int GetCellCount() {
            return _boardModel.GetCellCount();
        }
        public bool TryGetCellColor(Vector2Int pos,out CellColor cellColor) {
            return _boardModel.TryGetCellColor(pos,out cellColor);
        }
        public bool TryPut(Vector2Int putPos) {
            return _boardModel.TryPut(putPos);
        }
    }
}