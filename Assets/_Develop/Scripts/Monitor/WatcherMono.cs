using System;
using System.Collections.Generic;
using System.Threading;
using Cell;
using Cysharp.Threading.Tasks;
using MornLib.Cores;
using MornLib.Extensions;
using oucrcNet;
using Reversi;
using UniRx;
using UnityEngine;
namespace Monitor {
    public class WatcherMono : MonoBehaviour {
        private static readonly Vector2Int s_size = new(20,20);
        [SerializeField] private OucrcNetType _oucrcNetType;
        [SerializeField] private LayerMask _fieldLayerMask;
        [SerializeField] private Transform _markerParent;
        private readonly List<Vector2Int> _forProcess = new();
        private readonly List<ReversiPresenter> _presenterList = new();
        private CellColor _curColor;
        private MornTaskCanceller _loopCanceller;
        private Vector3 _offset;
        private void Awake() {
            _loopCanceller = new MornTaskCanceller(gameObject);
            OucrcNetUtility.Instance.OnUrlUpdated.Where(x => x == _oucrcNetType).Subscribe(
                x => {
                    _loopCanceller?.Cancel();
                    _loopCanceller = new MornTaskCanceller(gameObject);
                    WatchLoop(_loopCanceller.Token).Forget();
                }
            ).AddTo(this);
        }
        private void Update() {
            if(Input.GetMouseButtonDown(0)) {
                var mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                if(Physics.Raycast(mouseRay,out var hit,100,_fieldLayerMask)) {
                    var hitPos = hit.point - _offset;
                    hitPos.z *= -1;
                    var pos = new Vector2Int(Mathf.RoundToInt(hitPos.x),Mathf.RoundToInt(hitPos.z));
                    //OucrcNetUtility.Send("http://localhost:8000/rooms","RoomId0",pos,"UserId0");
                    /*
                    if(_presenter.TryPut(pos,_curColor)) {
                        _curColor = CellColorEx.GetOpposite(_curColor);
                        ShowCanPutMarker();
                    }*/
                }
            }
        }
        private async UniTask WatchLoop(CancellationToken token) {
            while(true) {
                var rooms = await OucrcNetUtility.Instance.GetAllRooms(_oucrcNetType,token);
                if(rooms != null)
                    for(var i = 0;i < rooms.Length;i++) {
                        if(_presenterList.Count <= i) {
                            var offset = transform.position + new Vector3(0.5f - s_size.x / 2f,0,-0.5f + s_size.y / 2f);
                            offset.x += (s_size.x + 4) * i;
                            _presenterList.Add(new ReversiPresenter(s_size,offset));
                        }
                        ApplyRoom(_presenterList[i],rooms[i]);
                    }
                await UniTask.Delay(TimeSpan.FromSeconds(5),cancellationToken: token);
            }
        }
        private void ApplyRoom(ReversiPresenter presenter,RoomInfo room) {
            var roomCellCount = room.GetCellCount();
            var presenterCellCount = presenter.GetCellCount();
            if(presenterCellCount == roomCellCount) return;
            if(presenterCellCount + 1 == roomCellCount) {
                var size = room.Size;
                var roomGrid = room.GetGrid();
                for(var x = 0;x < size.x;x++) {
                    for(var y = 0;y < size.y;y++) {
                        var pos = new Vector2Int(x,y);
                        if(presenter.TryGetCellColor(pos,out var cellColor) && cellColor != roomGrid[x,y]) {
                            presenter.TryPut(pos,roomGrid[x,y]);
                            return;
                        }
                    }
                }
            }
            presenter.InitializeBoard(room.GetGrid());
        }
        private void ResetGame() {
            //_offset        = transform.position + new Vector3(0.5f - s_size.x / 2f,0,-0.5f + s_size.y / 2f);
            //_presenterList = new FieldPresenter(s_size,_offset);
            //_curColor      = CellColor.Black;
        }
        private void ShowCanPutMarker() {
            _markerParent.DestroyChildren();
            var list = new List<Vector2Int>();
            /*
            if(_presenterList.TryGetCanPutPosses(_curColor,list))
                foreach(var pos in list) {
                    var marker = MarkerObjectPoolMono.Instance.Pop();
                    marker.transform.position = _offset + new Vector3(pos.x,0,-pos.y);
                    marker.transform.SetParent(_markerParent);
                }
            */
        }
        /*
        private async UniTask Loop() {
            while(true)
                //var result = await OucrcNetUtility.Get("http://localhost:8000/rooms","RoomId0");
                //_presenter.ReceiveData(result);
                await UniTask.Delay(TimeSpan.FromSeconds(1));
        }*/
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