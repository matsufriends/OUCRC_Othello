using System;
namespace OucrcReversi.Cell {
    public enum CellColor {
        None = 0
       ,Black = 1
       ,White = 2
    }
    public static class CellColorEx {
        public static CellColor GetOpposite(CellColor a) => a switch {
            CellColor.None  => CellColor.None
           ,CellColor.Black => CellColor.White
           ,CellColor.White => CellColor.Black
           ,_               => throw new ArgumentOutOfRangeException(nameof(a),a,null)
        };
    }
}