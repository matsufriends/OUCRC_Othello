using System;
using System.Collections.Generic;
using OucrcReversi.Cell;
using UnityEngine;
namespace OucrcReversi.Board {
    public interface IBoardView : IDisposable {
        IObservable<Vector2Int> OnPut { get; }
        void UpdateCellCount((int,int )   cellCounts);
        void UpdateCell(CellUpdateInfo    cellUpdateInfo);
        void UpdatePlaceablePos(CellColor cellColor,IEnumerable<Vector2Int> placeablePosses);
    }
}