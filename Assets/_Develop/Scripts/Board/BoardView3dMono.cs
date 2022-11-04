using System;
using System.Collections.Generic;
using MornLib.Extensions;
using OucrcReversi.Cell;
using OucrcReversi.Marker;
using OucrcReversi.Network;
using TMPro;
using UniRx;
using UnityEngine;
namespace OucrcReversi.Board {
    public sealed class BoardView3dMono : MonoBehaviour,IBoardView {
        [SerializeField] private LayerMask _fieldLayerMask;
        [SerializeField] private Transform _markerParent;
        [Header("UI")] [SerializeField] private TextMeshPro _blackUserName;
        [SerializeField] private TextMeshPro _whiteUserName;
        [SerializeField] private TextMeshPro _blackCount;
        [SerializeField] private TextMeshPro _whiteCount;
        [Header("Scaler")] [SerializeField] private Transform _fieldGreen;
        [SerializeField] private Transform _line;
        [SerializeField] private SpriteRenderer _lineRenderer;
        [SerializeField] private Transform _top;
        [SerializeField] private Transform _down;
        [SerializeField] private Transform _left;
        [SerializeField] private Transform _right;
        private readonly Dictionary<Vector2Int,CellMono> _cellDictionary = new();
        private readonly Subject<Vector2Int> _onPutSubject = new();
        private static readonly int s_tiling = Shader.PropertyToID("_Tiling");
        public IObservable<Vector2Int> OnPut => _onPutSubject;
        public void Init(Vector3 boardOffset,Vector2Int size,OucrcNetType oucrcNetType,string blackUserName,string whiteUserName) {
            _blackUserName.text = blackUserName;
            _whiteUserName.text = whiteUserName;
            transform.position  = boardOffset;
            var center = new Vector3(size.x / 2f - 0.5f,0,-(size.y / 2f - 0.5f));
            _fieldGreen.localPosition = center + new Vector3(0,_fieldGreen.localPosition.y,0);
            _fieldGreen.localScale    = new Vector3(size.x + 1,_fieldGreen.localScale.y,size.y + 1);
            _line.localPosition       = center + new Vector3(0,_line.localPosition.y,0);
            _line.localScale          = new Vector3(size.x,size.y,_line.localScale.z);
            _lineRenderer.material.SetVector(s_tiling,new Vector4(size.x,size.y,0,0));
            _top.localPosition   = center + new Vector3(0,_top.localPosition.y,size.y / 2f + 0.75f);
            _top.localScale      = new Vector3(0.5f,1.1f,size.x + 2f);
            _down.localPosition  = center + new Vector3(0,_down.localPosition.y,-(size.y / 2f + 0.75f));
            _down.localScale     = new Vector3(0.5f,1.1f,size.x + 2f);
            _left.localPosition  = center + new Vector3(-(size.x / 2f + 0.75f),_left.localPosition.y,0);
            _left.localScale     = new Vector3(0.5f,1.1f,size.y + 2f);
            _right.localPosition = center + new Vector3(size.x / 2f + 0.75f,_right.localPosition.y,0);
            _right.localScale    = new Vector3(0.5f,1.1f,size.y + 2f);
            if(oucrcNetType == OucrcNetType.Watch) return;
            Observable.EveryUpdate().Where(_ => Input.GetMouseButtonDown(0)).Subscribe(
                _ => {
                    var mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if(Physics.Raycast(mouseRay,out var hit,100,_fieldLayerMask)) {
                        var hitPos = hit.point - transform.position;
                        hitPos.z *= -1;
                        _onPutSubject.OnNext(new Vector2Int(Mathf.RoundToInt(hitPos.x),Mathf.RoundToInt(hitPos.z)));
                    }
                }
            ).AddTo(this);
        }
        public void Dispose() {
            foreach(var value in _cellDictionary.Values) {
                CellObjectPoolMono.Instance.Return(value);
            }
            Destroy(gameObject);
        }
        public void UpdateCellCount((int,int) cellCounts) {
            _blackCount.text = cellCounts.Item1.ToString();
            _whiteCount.text = cellCounts.Item2.ToString();
        }
        public void UpdateCell(CellUpdateInfo cellUpdateInfo) {
            if(_cellDictionary.TryGetValue(cellUpdateInfo.Pos,out var cell) == false) {
                cell = CellObjectPoolMono.Instance.Rent();
                _cellDictionary.Add(cellUpdateInfo.Pos,cell);
            }
            cell.Set(transform.position,cellUpdateInfo);
        }
        public void UpdatePlaceablePos(IEnumerable<Vector2Int> placeablePosses) {
            _markerParent.DestroyChildren();
            foreach(var pos in placeablePosses) {
                var marker = MarkerObjectPoolMono.Instance.Rent();
                marker.transform.position = transform.position + new Vector3(pos.x,0,-pos.y);
                marker.transform.SetParent(_markerParent);
            }
        }
    }
}