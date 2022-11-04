using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using OucrcReversi.Network;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
namespace OucrcReversi.Scene {
    public class DownBarMono : MonoBehaviour {
        [SerializeField] private TopBarMono _topBar;
        [Header("Animation")] [SerializeField] private RectTransform _rect;
        [SerializeField] private Vector2 _hidePos;
        [SerializeField] private Vector2 _showPos;
        [SerializeField] private RectTransform _showHideButton;
        [SerializeField] private float _openHeight;
        [SerializeField] private float _closeHeight;
        [SerializeField] private float _duration;
        [Header("UI")] [SerializeField] private Button _barToggleButton;
        [SerializeField] private TMP_InputField _userIdField;
        [SerializeField] private TMP_InputField _aiIdField;
        [SerializeField] private Button _makeRoomButton;
        private BattlePoller _battlePoller;
        private bool _isActive;
        public IObservable<Unit> OnOpen => _barToggleButton.OnClickAsObservable();
        private void Awake() {
            _topBar.OnOpen.Subscribe(
                _ => {
                    _isActive = false;
                    Anim();
                }
            ).AddTo(this);
            _barToggleButton.OnClickAsObservable().Subscribe(
                _ => {
                    _isActive = !_isActive;
                    Anim();
                }
            ).AddTo(this);
            var token = gameObject.GetCancellationTokenOnDestroy();
            _aiIdField.onValueChanged.AsObservable().Subscribe(_ => UpdateButton()).AddTo(this);
            _userIdField.onValueChanged.AsObservable().Subscribe(_ => UpdateButton()).AddTo(this);
            _makeRoomButton.OnClickAsObservable().Subscribe(
                async _ => {
                    var allUsers = await ServerUtility.Instance.GetAllUsers(OucrcNetType.Battle,false,token);
                    var user = allUsers.FirstOrDefault(x => x.id == _userIdField.text);
                    var roomId = "";
                    if(user != null && user.status != null) {
                        roomId = user.status;
                        Debug.Log("既存ルームの使い回し" + roomId);
                    } else {
                        var room = await ServerUtility.Instance.PostMakeBattleRoom(
                                       OucrcNetType.Battle,new MakeBattleRoomData {
                                           user_id = _userIdField.text
                                          ,ai_id   = _aiIdField.text
                                       },token
                                   );
                        roomId = room.id;
                        Debug.Log("新ルーム" + roomId);
                    }
                    _battlePoller?.Dispose();
                    _battlePoller = new BattlePoller(OucrcNetType.Battle,new Vector3(36.5f,0,0.8f),roomId,_userIdField.text);
                    _isActive     = false;
                    Anim();
                    CameraManagerMono.Instance.ChangeFocus(OucrcNetType.Battle);
                }
            ).AddTo(this);
            UpdateButton();
        }
        private void OnDestroy() {
            _battlePoller?.Dispose();
        }
        private void UpdateButton() {
            _makeRoomButton.gameObject.SetActive(_aiIdField.text.Length > 0 && _userIdField.text.Length > 0);
        }
        private void Anim() {
            _rect.DOComplete();
            _rect.DOAnchorPos(_isActive ? _showPos : _hidePos,_duration);
            _showHideButton.DOComplete();
            _showHideButton.DOSizeDelta(new Vector2(_showHideButton.sizeDelta.x,_isActive ? _closeHeight : _openHeight),_duration);
        }
    }
}