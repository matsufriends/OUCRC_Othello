using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MornLib.Scenes;
using OucrcReversi.Network;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
namespace OucrcReversi.Scene {
    public class TopBarMono : MornSceneMono {
        [Header("Animation")] [SerializeField] private RectTransform _rect;
        [SerializeField] private Vector2 _hidePos;
        [SerializeField] private Vector2 _showPos;
        [SerializeField] private float _duration;
        [Header("UI")] [SerializeField] private Button _barToggleButton;
        [SerializeField] private TMP_InputField _watchUrlInputField;
        [SerializeField] private Button _watchUrlSetButton;
        [SerializeField] private TMP_InputField _watchUserNameInputField;
        [SerializeField] private Button _watchUserNameSetButton;
        [SerializeField] private Button _forceRefreshRoomsButton;
        [SerializeField] private TMP_InputField _battleUrlInputField;
        [SerializeField] private Button _battleUrlSetButton;
        [SerializeField] private TMP_InputField _battleUserNameInputField;
        [SerializeField] private Button _battleNameSetButton;
        [SerializeField] private Button _addAiButton;
        [SerializeField] private UserIdAndNameItemMono _userIdAndNameItemPrefab;
        [SerializeField] private AiIdItemMono _aiIdItemPrefab;
        [SerializeField] private Transform _watchUserParent;
        [SerializeField] private Transform _battleUserParent;
        [SerializeField] private Transform _aiIdParent;
        private readonly HashSet<string> _watchUserHash = new();
        private readonly HashSet<string> _battleUserHash = new();
        private readonly HashSet<string> _aiIdHash = new();
        private bool _isActive;
        private void Awake() {
            _barToggleButton.OnClickAsObservable().Subscribe(
                _ => {
                    _isActive = !_isActive;
                    _rect.DOComplete();
                    _rect.DOAnchorPos(_isActive ? _showPos : _hidePos,_duration);
                }
            ).AddTo(this);
            var token = gameObject.GetCancellationTokenOnDestroy();
            _watchUrlSetButton.OnClickAsObservable().Subscribe(_ => ServerUtility.Instance.SetUrl(OucrcNetType.Watch,_watchUrlInputField.text)).AddTo(this);
            _watchUserNameSetButton.OnClickAsObservable().Subscribe(
                _ => ServerUtility.Instance.PostRegisterUser(
                    OucrcNetType.Watch,new RegisterUserPostData {
                        user_name = _watchUserNameInputField.text
                    },token
                ).Forget()
            ).AddTo(this);
            _forceRefreshRoomsButton.OnClickAsObservable().Subscribe(_ => ServerUtility.Instance.PostRefreshRoom(OucrcNetType.Watch,token).Forget())
                                    .AddTo(this);
            _battleNameSetButton.OnClickAsObservable().Subscribe(
                _ => ServerUtility.Instance.PostRegisterUser(
                    OucrcNetType.Battle,new RegisterUserPostData {
                        user_name = _battleUserNameInputField.text
                    },token
                ).Forget()
            ).AddTo(this);
            _addAiButton.OnClickAsObservable().Subscribe(_ => ServerUtility.Instance.PostRegisterAi(OucrcNetType.Battle,token).Forget()).AddTo(this);
            _battleUrlSetButton.OnClickAsObservable().Subscribe(_ => ServerUtility.Instance.SetUrl(OucrcNetType.Battle,_battleUrlInputField.text)).AddTo(this);
            CheckLoop().Forget();
        }
        private async UniTask CheckLoop() {
            var token = gameObject.GetCancellationTokenOnDestroy();
            while(true) {
                var watchUsers = await ServerUtility.Instance.GetAllUsers(OucrcNetType.Watch,token);
                var ais = await ServerUtility.Instance.GetAllAIs(OucrcNetType.Battle,token);
                var battleUsers = await ServerUtility.Instance.GetAllUsers(OucrcNetType.Battle,token);
                if(watchUsers != null)
                    foreach(var user in watchUsers) {
                        if(_watchUserHash.Add(user.id)) {
                            var prefab = Instantiate(_userIdAndNameItemPrefab,_watchUserParent);
                            prefab.Init(user.id,user.name);
                        }
                    }
                if(ais != null)
                    foreach(var ai in ais) {
                        if(_aiIdHash.Add(ai.id)) {
                            var prefab = Instantiate(_aiIdItemPrefab,_aiIdParent);
                            prefab.Init(ai.id);
                        }
                    }
                if(battleUsers != null)
                    foreach(var user in battleUsers) {
                        if(_battleUserHash.Add(user.id)) {
                            var prefab = Instantiate(_userIdAndNameItemPrefab,_battleUserParent);
                            prefab.Init(user.id,user.name);
                        }
                    }
                await UniTask.Delay(TimeSpan.FromSeconds(ServerUtility.WatchInterval),cancellationToken: token);
            }
        }
    }
}