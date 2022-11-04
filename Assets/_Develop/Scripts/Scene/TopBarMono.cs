using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MornLib.Scenes;
using OucrcReversi.Network;
using UniRx;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;
namespace OucrcReversi.Scene {
    public class TopBarMono : MornSceneMono {
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
        private readonly HashSet<string> _watchUserHash = new();
        private readonly HashSet<string> _battleUserHash = new();
        private readonly HashSet<string> _aiIdHash = new();
        private bool _isActive;
        private void Awake() {
            NativeLeakDetection.Mode = NativeLeakDetectionMode.Disabled;
            _barToggleButton.OnClickAsObservable().Subscribe(
                _ => {
                    _isActive = !_isActive;
                    _rect.DOComplete();
                    _rect.DOAnchorPos(_isActive ? _showPos : _hidePos,_duration);
                    _showHideButton.DOComplete();
                    _showHideButton.DOSizeDelta(new Vector2(_showHideButton.sizeDelta.x,_isActive ? _closeHeight : _openHeight),_duration);
                }
            ).AddTo(this);
            var token = gameObject.GetCancellationTokenOnDestroy();
            _forceRefreshRoomsButton.OnClickAsObservable().Subscribe(_ => ServerUtility.Instance.PostRefreshRoom(OucrcNetType.Watch,token).Forget())
                                    .AddTo(this);
            GameManagerMono.Instance.OnGetAllAI.Where(tuple => tuple.Item2 != null).Subscribe(
                tuple => {
                    foreach(var ai in tuple.Item2) {
                        if(_aiIdHash.Add(ai.id)) {
                            var prefab = Instantiate(_userIdAndNameItemPrefab,_aiIdParent);
                            prefab.Init(ai.id,ai.name);
                        }
                    }
                }
            ).AddTo(this);
            GameManagerMono.Instance.OnGetAllUser.Where(tuple => tuple.Item2 != null).Subscribe(
                tuple => {
                    var hash = tuple.Item1 == OucrcNetType.Watch ? _watchUserHash : _battleUserHash;
                    var parent = tuple.Item1 == OucrcNetType.Watch ? _watchUserParent : _battleUserParent;
                    foreach(var user in tuple.Item2) {
                        if(hash.Add(user.id)) {
                            var prefab = Instantiate(_userIdAndNameItemPrefab,parent);
                            prefab.Init(user.id,user.name);
                        }
                    }
                }
            ).AddTo(this);
        }
    }
}