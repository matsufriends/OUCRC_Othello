using DG.Tweening;
using MornLib.Scenes;
using OucrcReversi.oucrcNet;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
namespace OucrcReversi.Scene {
    public class TopBarMono : MornSceneMono {
        [SerializeField] private Vector2 _hidePos;
        [SerializeField] private Vector2 _showPos;
        [SerializeField] private float _duration;
        [SerializeField] private RectTransform _rect;
        [SerializeField] private Button _barToggleButton;
        [SerializeField] private TMP_InputField _watchUrlInputField;
        [SerializeField] private Button _watchSendButton;
        [SerializeField] private TMP_InputField _battleUrlInputField;
        [SerializeField] private Button _battleSendButton;
        private bool _isActive;
        private void Awake() {
            _watchSendButton.OnClickAsObservable().Subscribe(_ => OucrcNetUtility.Instance.SetUrl(OucrcNetType.Watch,_watchUrlInputField.text)).AddTo(this);
            _battleSendButton.OnClickAsObservable().Subscribe(_ => OucrcNetUtility.Instance.SetUrl(OucrcNetType.Battle,_battleUrlInputField.text)).AddTo(this);
            _barToggleButton.OnClickAsObservable().Subscribe(
                _ => {
                    _isActive = !_isActive;
                    _rect.DOComplete();
                    _rect.DOAnchorPos(_isActive ? _showPos : _hidePos,_duration);
                }
            ).AddTo(this);
        }
    }
}