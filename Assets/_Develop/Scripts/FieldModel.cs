using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
public class FieldModel {
    public readonly int Width;
    public readonly int Height;
    private readonly CellColor[,] _cellGrid;
    private readonly Subject<CellUpdateInfo> _updateCellObject = new();
    public IObservable<CellUpdateInfo> OnUpdateCell => _updateCellObject;
    private static readonly Vector2Int[] s_dirVec = {
        new(-1,-1)
       ,new(-1,0)
       ,new(-1,1)
       ,new(0,-1)
       ,new(0,1)
       ,new(1,-1)
       ,new(1,0)
       ,new(1,1)
    };
    public FieldModel(int width,int height) {
        if(width % 2 != 0 || height % 2 != 0) throw new ArgumentException($"サイズが偶数じゃない:({width},{height})");
        Width     = width;
        Height    = height;
        _cellGrid = new CellColor[width,height];
    }
    private bool IsInner(Vector2Int pos) {
        return 0 <= pos.x && pos.x < Width && 0 <= pos.y && pos.y < Height;
    }
    private bool TryGetCell(Vector2Int pos,out CellColor cellColor) {
        if(IsInner(pos) == false) {
            cellColor = CellColor.None;
            return false;
        }
        cellColor = _cellGrid[pos.x,pos.y];
        return true;
    }
    private void SetCell(Vector2Int pos,CellColor cellColor,int animOffset) {
        if(IsInner(pos) == false) throw new ArgumentException("配列の範囲外です");
        _cellGrid[pos.x,pos.y] = cellColor;
    }
    public bool TryPut(Vector2Int putPos,CellColor putColor) {
        var flipPosList = new List<Vector2Int>();
        if(TryGetFlipPosses(putPos,putColor,flipPosList) == false) return false;
        SetCell(putPos,putColor,0);
        foreach(var flipPos in flipPosList) {
            var dif = putPos - flipPos;
            var animOffset = Mathf.Max(Mathf.Abs(dif.x),Mathf.Abs(dif.y));
            SetCell(flipPos,putColor,animOffset);
        }
        return true;
    }
    public bool TryGetFlipPosses(Vector2Int putPos,CellColor putColor,List<Vector2Int> flipPosList) {
        if(flipPosList == null) return false;
        flipPosList.Clear();
        if(TryGetCell(putPos,out var cellColor) == false || cellColor != CellColor.None) return false;
        foreach(var dir in s_dirVec) {
            var hasOpposite = false;
            for(var dirLen = 1;dirLen < Mathf.Max(Width,Height);dirLen++) {
                if(TryGetCell(putPos + dir * dirLen,out var checkCellColor) == false) break;
                if(CellColorEx.IsOpposite(putColor,checkCellColor)) {
                    if(hasOpposite) break;
                    hasOpposite = true;
                } else if(CellColorEx.IsSame(putColor,checkCellColor)) {
                    if(hasOpposite == false) break;
                    for(var i = 1;i <= dirLen;i++) flipPosList.Add(putPos + dir * i);
                    break;
                }
            }
        }
        return flipPosList.Count > 0;
    }
}