using System;
public enum CellColor {
    None
   ,Black
   ,White
}
public static class CellColorEx { 
    public static bool IsSame(CellColor a,CellColor b) {
        return a switch {
            CellColor.None  => false
           ,CellColor.Black => b == CellColor.Black
           ,CellColor.White => b == CellColor.White
           ,_               => throw new ArgumentOutOfRangeException(nameof(a),a,null)
        };
    }
    public static bool IsOpposite(CellColor a,CellColor b) {
        return a switch {
            CellColor.None  => false
           ,CellColor.Black => b == CellColor.White
           ,CellColor.White => b == CellColor.Black
           ,_               => throw new ArgumentOutOfRangeException(nameof(a),a,null)
        };
    }
}