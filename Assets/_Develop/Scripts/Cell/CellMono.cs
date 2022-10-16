using System;
using Cysharp.Threading.Tasks;
using MornLib.Cores;
using UnityEngine;
namespace Cell {
    public class CellMono : MonoBehaviour {
        private MornTaskCanceller _canceller;
        private void Awake() {
            _canceller = new MornTaskCanceller(gameObject);
        }
        public void Set(Vector3 offset,CellUpdateInfo updateInfo) {
            _canceller.Cancel();
            if(updateInfo.AnimOffset == 0) InitAsync(offset,updateInfo).Forget();
            else SetAsync(offset,updateInfo).Forget();
        }
        private async UniTask InitAsync(Vector3 offset,CellUpdateInfo updateInfo) {
            transform.position    = offset + new Vector3(updateInfo.Pos.x,0,updateInfo.Pos.y);
            transform.eulerAngles = CellColorToEulerAngles(updateInfo.CellColor);
        }
        private async UniTask SetAsync(Vector3 offset,CellUpdateInfo updateInfo) {
            await UniTask.Delay(TimeSpan.FromSeconds(updateInfo.AnimOffset * 0.1f),cancellationToken: _canceller.Token);
            transform.eulerAngles = CellColorToEulerAngles(updateInfo.CellColor);
        }
        private Vector3 CellColorToEulerAngles(CellColor cellColor) {
            return cellColor switch {
                CellColor.Black => Vector3.zero
               ,CellColor.White => new Vector3(180,0,0)
               ,_               => Vector3.zero
            };
        }
    }
}