using System;
using System.Collections.Generic;
using OucrcReversi.Cell;
using UnityEngine;
namespace OucrcReversi.Board {
    public interface IBoardModel : IDisposable {
        IObservable<(int,int)> OnCountChanged { get; }
        IObservable<CellUpdateInfo> OnGridChanged { get; }
        IObservable<(CellColor,IEnumerable<Vector2Int>)> OnPlaceablePosChanged { get; }
        void InitializeBoard(CellColor[,] board,CellColor nextCellColor);
        int GetCellCount();
        bool TryGetCellColor(Vector2Int pos,out CellColor cellColor);
        bool TryPut(Vector2Int          putPos);
        bool TryPut(Vector2Int          putPos,CellColor cellColor);
        void Log();
    }
}