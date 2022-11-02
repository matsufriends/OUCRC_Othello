using System;
using System.Collections.Generic;
using OucrcReversi.Cell;
using UnityEngine;
namespace OucrcReversi.Board {
    public interface IBoardModel : IDisposable {
        public IObservable<CellUpdateInfo> OnGridChanged { get; }
        void InitializeBoard(CellColor[,] board);
        int GetCellCount();
        bool TryGetCellColor(Vector2Int   pos,     out CellColor    cellColor);
        bool TryGetCanPutPosses(CellColor putColor,List<Vector2Int> canPutPosList);
        bool TryPut(Vector2Int            putPos,  CellColor        putColor);
    }
}