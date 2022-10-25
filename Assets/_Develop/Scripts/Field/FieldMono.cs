using System;
using System.Collections.Generic;
using Cell;
using Cysharp.Threading.Tasks;
using Marker;
using MornLib.Extensions;
using oucrcNet;
using Unity.Collections;
using UnityEngine;
namespace Field {
    public class FieldMono : MonoBehaviour {
        [SerializeField] private LayerMask _fieldLayerMask;
        [SerializeField] private Transform _markerParent;
        private FieldPresenter _presenter;
        private CellColor _curColor;
        private Vector3 _offset;
        private readonly List<Vector2Int> _forProcess = new();
        private static readonly Vector2Int s_size = new(20,20);
        private void Awake() {
            NativeLeakDetection.Mode = NativeLeakDetectionMode.EnabledWithStackTrace;
            ResetGame();
            ShowCanPutMarker();
            Loop().Forget();
        }
        private void ResetGame() {
            _offset    = transform.position + new Vector3(0.5f - s_size.x / 2f,0,-0.5f + s_size.y / 2f);
            _presenter = new FieldPresenter(s_size,_offset);
            _curColor  = CellColor.Black;
        }
        private void ShowCanPutMarker() {
            _markerParent.DestroyChildren();
            var list = new List<Vector2Int>();
            if(_presenter.TryGetCanPutPosses(_curColor,list)) {
                foreach(var pos in list) {
                    var marker = MarkerObjectPoolMono.Instance.Pop();
                    marker.transform.position = _offset + new Vector3(pos.x,0,-pos.y);
                    marker.transform.SetParent(_markerParent);
                }
            }
        }
        private void Update() {
            if(Input.GetMouseButtonDown(0)) {
                var mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                if(Physics.Raycast(mouseRay,out var hit,100,_fieldLayerMask)) {
                    var hitPos = hit.point - _offset;
                    hitPos.z *= -1;
                    var pos = new Vector2Int(Mathf.RoundToInt(hitPos.x),Mathf.RoundToInt(hitPos.z));
                    OucrcNetUtility.Send("http://localhost:8000/rooms","RoomId0",pos,"UserId0");
                    /*
                    if(_presenter.TryPut(pos,_curColor)) {
                        _curColor = CellColorEx.GetOpposite(_curColor);
                        ShowCanPutMarker();
                    }*/
                }
            }
        }
        private async UniTask Loop() {
            while(true) {
                var result = await OucrcNetUtility.Get("http://localhost:8000/rooms","RoomId0");
                _presenter.ReceiveData(result);
                await UniTask.Delay(TimeSpan.FromSeconds(1));
            }
        }
        /*
        private async UniTask Loop() {
            while(true) {
                ShowCanPutMarker();
                if(_presenter.TryPut(_canPutPosList.RandomValue(),_curColor)) {
                    _curColor = CellColorEx.GetOpposite(_curColor);
                    ShowCanPutMarker();
                    if(_canPutPosList.Count == 0) {
                        _curColor = CellColorEx.GetOpposite(_curColor);
                        ShowCanPutMarker();
                        if(_canPutPosList.Count == 0) {
                            _presenter.Dispose();
                            ResetGame();
                        }
                    }
                }
                await UniTask.Yield(gameObject.GetCancellationTokenOnDestroy());
            }
        }*/
    }
}