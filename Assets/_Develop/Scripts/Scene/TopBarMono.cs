using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using OucrcReversi.Network;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
namespace OucrcReversi.Scene {
    public class TopBarMono : MonoBehaviour {
        [SerializeField] private DownBarMono _downBar;
        [Header("Animation")] [SerializeField] private RectTransform _rect;
        [SerializeField] private Vector2 _hidePos;
        [SerializeField] private Vector2 _showPos;
        [SerializeField] private RectTransform _showHideButton;
        [SerializeField] private float _openHeight;
        [SerializeField] private float _closeHeight;
        [SerializeField] private float _duration;
        [Header("UI")] [SerializeField] private Button _barToggleButton;
        [SerializeField] private Button _forceRefreshRoomsButton;
        [SerializeField] private UserIdAndNameItemMono _userIdAndNameItemPrefab;
        [SerializeField] private Transform _watchUserParent;
        [SerializeField] private Transform _battleUserParent;
        [SerializeField] private Transform _aiIdParent;
        private readonly HashSet<string> _aiIdHash = new();
        private readonly HashSet<string> _battleUserHash = new();
        private readonly HashSet<string> _watchUserHash = new();
        private bool _isActive;
        public IObservable<Unit> OnOpen => _barToggleButton.OnClickAsObservable();
        private void Awake() {
            _downBar.OnOpen.Subscribe(
                _ => {
                    _isActive = false;
                    Anim();
                }
            );
            _barToggleButton.OnClickAsObservable().Subscribe(
                _ => {
                    _isActive = !_isActive;
                    Anim();
                }
            ).AddTo(this);
            var token = gameObject.GetCancellationTokenOnDestroy();
            _forceRefreshRoomsButton.OnClickAsObservable().Subscribe(_ => ServerUtility.Instance.PostRefreshRoom(OucrcNetType.Watch,token).Forget())
                                    .AddTo(this);
            GameManagerMono.Instance.OnGetAllAI.Subscribe(
                tuple => {
                    foreach(var ai in tuple.Item2) {
                        if(_aiIdHash.Add(ai.id)) {
                            var prefab = Instantiate(_userIdAndNameItemPrefab,_aiIdParent);
                            prefab.Init(ai.id,ai.name,ai.status);
                        }
                    }
                }
            ).AddTo(this);
            GameManagerMono.Instance.OnGetAllUser.Subscribe(
                tuple => {
                    var hash = tuple.Item1 == OucrcNetType.Watch ? _watchUserHash : _battleUserHash;
                    var parent = tuple.Item1 == OucrcNetType.Watch ? _watchUserParent : _battleUserParent;
                    foreach(var user in tuple.Item2) {
                        if(hash.Add(user.id)) {
                            var prefab = Instantiate(_userIdAndNameItemPrefab,parent);
                            prefab.Init(user.id,user.name,user.status);
                        }
                    }
                }
            ).AddTo(this);
        }
        private void Anim() {
            _rect.DOComplete();
            _rect.DOAnchorPos(_isActive ? _showPos : _hidePos,_duration);
            _showHideButton.DOComplete();
            _showHideButton.DOSizeDelta(new Vector2(_showHideButton.sizeDelta.x,_isActive ? _closeHeight : _openHeight),_duration);
        }
    }
}