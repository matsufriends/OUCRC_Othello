using MornLib.Pool;
using MornLib.Singletons;
using UnityEngine;
namespace OucrcReversi.Cell {
    public class CellPoolMono : SingletonMono<CellPoolMono> {
        [SerializeField] private CellMono _cellPrefab;
        private MornObjectPool<CellMono> _mornObjectPool;
        protected override void MyAwake() => _mornObjectPool = new MornObjectPool<CellMono>(
                                                 () => Instantiate(_cellPrefab,transform),x => {
                                                     x.transform.SetParent(transform);
                                                     x.gameObject.SetActive(true);
                                                 },x => {
                                                     x.gameObject.SetActive(false);
                                                     x.transform.SetParent(transform);
                                                 },50
                                             );
        public CellMono Pop() => _mornObjectPool.Pop();
        public void Push(CellMono poolObject) => _mornObjectPool.Push(poolObject);
    }
}