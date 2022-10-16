using System;
namespace Cell {
    public enum CellColor {
        None
       ,Black
       ,White
    }
    public static class CellColorEx {
        public static CellColor GetOpposite(CellColor a) {
            return a switch {
                CellColor.None  => CellColor.None
               ,CellColor.Black => CellColor.White
               ,CellColor.White => CellColor.Black
               ,_               => throw new ArgumentOutOfRangeException(nameof(a),a,null)
            };
        }
    }
}