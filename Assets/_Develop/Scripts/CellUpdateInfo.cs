using UnityEngine;
public readonly struct CellUpdateInfo {
    public readonly Vector2Int Pos;
    public readonly CellColor CellColor;
    public readonly int AnimOffset;
    public CellUpdateInfo(Vector2Int pos,CellColor cellColor,int animOffset) {
        Pos        = pos;
        CellColor  = cellColor;
        AnimOffset = animOffset;
    }
}