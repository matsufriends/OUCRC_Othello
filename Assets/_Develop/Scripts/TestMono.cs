using System;
using System.Collections.Generic;
using Cell;
using Cysharp.Threading.Tasks;
using Field;
using MornLib.Extensions;
using UnityEngine;
public class TestMono : MonoBehaviour {
    [SerializeField] private float _duration = 0.1f;
    private FieldPresenter _presenter;
    private CellColor _curColor;
    private readonly List<Vector2Int> _canPutPosList = new();
    private readonly List<Vector2Int> _forProcess = new();
    private void Awake() {
        _presenter = new FieldPresenter(new Vector2Int(100,100),transform.position);
        _curColor  = CellColor.Black;
        Loop().Forget();
    }
    private async UniTask Loop() {
        while(true) {
            CalculateCanPut();
            if(_presenter.TryPut(_canPutPosList.RandomValue(),_curColor)) {
                _curColor = CellColorEx.GetOpposite(_curColor);
                CalculateCanPut();
                if(_canPutPosList.Count == 0) {
                    _curColor = CellColorEx.GetOpposite(_curColor);
                    CalculateCanPut();
                    if(_canPutPosList.Count == 0) {
                        _presenter.Dispose();
                        _presenter = new FieldPresenter(new Vector2Int(8,8),transform.position);
                        _curColor  = CellColor.Black;
                    }
                }
            }
            await UniTask.Delay(TimeSpan.FromSeconds(_duration));
        }
    }
    private void CalculateCanPut() {
        _canPutPosList.Clear();
        for(var y = _presenter.Size.y - 1;y >= 0;y--) {
            for(var x = 0;x < _presenter.Size.x;x++) {
                var checkPos = new Vector2Int(x,y);
                if(_presenter.TryGetFlipPosses(checkPos,_curColor,_forProcess)) {
                    _canPutPosList.Add(checkPos);
                } else {
                    _presenter.TryGetCell(checkPos,out var cellColor);
                }
            }
        }
    }
}