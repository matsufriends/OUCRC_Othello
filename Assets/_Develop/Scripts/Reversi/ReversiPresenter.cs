using System;
using System.Collections.Generic;
using OucrcReversi.Cell;
using UniRx;
using UnityEngine;
namespace OucrcReversi.Reversi {
    public sealed class ReversiPresenter : IDisposable {
        private readonly CompositeDisposable _compositeDisposable = new();
        private readonly ReversiModel _reversiModel;
        private readonly ReversiView _reversiView;
        public ReversiPresenter(Vector2Int size,Vector3 offset) {
            _reversiModel = new ReversiModel(size);
            _reversiView  = new ReversiView(offset);
            _compositeDisposable.Add(_reversiModel);
            _compositeDisposable.Add(_reversiView);
            _reversiModel.OnGridChanged.Subscribe(_reversiView.UpdateCell).AddTo(_compositeDisposable);
            _reversiModel.ForcePut(new Vector2Int(size.x / 2,size.y / 2),CellColor.White);
            _reversiModel.ForcePut(new Vector2Int(size.x / 2 - 1,size.y / 2 - 1),CellColor.White);
            _reversiModel.ForcePut(new Vector2Int(size.x / 2 - 1,size.y / 2),CellColor.Black);
            _reversiModel.ForcePut(new Vector2Int(size.x / 2,size.y / 2 - 1),CellColor.Black);
        }
        void IDisposable.Dispose() => _compositeDisposable.Dispose();
        public void InitializeBoard(CellColor[,] board) => _reversiModel.InitializeBoard(board);
        public int GetCellCount() => _reversiModel.GetCellCount();
        public bool TryGetCellColor(Vector2Int   pos,     out CellColor    cellColor) => _reversiModel.TryGetCellColor(pos,out cellColor);
        public bool TryGetCanPutPosses(CellColor putColor,List<Vector2Int> canPutPosList) => _reversiModel.TryGetCanPutPosses(putColor,canPutPosList);
        public bool TryPut(Vector2Int            putPos,  CellColor        putColor) => _reversiModel.TryPut(putPos,putColor);
    }
}