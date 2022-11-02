using System;
using OucrcReversi.Cell;
namespace OucrcReversi.Board {
    public interface IBoardView : IDisposable {
        void UpdateCell(CellUpdateInfo cellUpdateInfo);
    }
}