using System;
using System.Collections.Generic;
using Cell;
using Cysharp.Threading.Tasks;
using MornLib.Extensions;
using UnityEngine;
namespace Field {
    public class FieldMono : MonoBehaviour {
        private FieldPresenter _presenter;
        private CellColor _curColor;
        private readonly List<Vector2Int> _canPutPosList = new();
        private readonly List<Vector2Int> _forProcess = new();
        private static readonly Vector2Int s_size = new(20,20);
        private void Awake() {
            ResetGame();
            Loop().Forget();
        }
        private void ResetGame() {
            _presenter = new FieldPresenter(s_size,transform.position + new Vector3(0.5f - s_size.x / 2f,0,0.5f - s_size.y / 2f));
            _curColor  = CellColor.Black;
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
                            ResetGame();
                        }
                    }
                }
                await UniTask.Yield(gameObject.GetCancellationTokenOnDestroy());
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
}