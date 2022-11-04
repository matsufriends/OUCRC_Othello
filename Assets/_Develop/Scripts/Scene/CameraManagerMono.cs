using MornLib.Singletons;
using OucrcReversi.Network;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
namespace OucrcReversi.Scene {
    public class CameraManagerMono : SingletonMono<CameraManagerMono> {
        [SerializeField] private Transform _camera;
        [SerializeField] private Vector3 _watchPos;
        [SerializeField] private Vector3 _battlePos;
        [SerializeField] private float _watchDistance;
        [SerializeField] private float _battleDistance;
        [SerializeField] private float _lerpK;
        [SerializeField] private Button _watchCameraButton;
        [SerializeField] private Button _battleCameraButton;
        private OucrcNetType _oucrcNetType = OucrcNetType.Watch;
        public void ChangeFocus(OucrcNetType oucrcNetType) {
            _oucrcNetType = oucrcNetType;
            _watchCameraButton.gameObject.SetActive(oucrcNetType == OucrcNetType.Battle);
            _battleCameraButton.gameObject.SetActive(oucrcNetType == OucrcNetType.Watch);
        }
        protected override void MyAwake() {
            _watchCameraButton.OnClickAsObservable().Subscribe(_ => ChangeFocus(OucrcNetType.Watch)).AddTo(this);
            _battleCameraButton.OnClickAsObservable().Subscribe(_ => ChangeFocus(OucrcNetType.Battle)).AddTo(this);
            ChangeFocus(OucrcNetType.Watch);
        }
        private void Update() {
            var pos = _oucrcNetType == OucrcNetType.Watch ? _watchPos : _battlePos;
            var distance = _oucrcNetType == OucrcNetType.Watch ? _watchDistance : _battleDistance;
            transform.position    = Vector3.Lerp(transform.position,pos,Time.deltaTime * _lerpK);
            _camera.localPosition = Vector3.Lerp(_camera.localPosition,new Vector3(0,0,distance),Time.deltaTime * _lerpK);
        }
    }
}