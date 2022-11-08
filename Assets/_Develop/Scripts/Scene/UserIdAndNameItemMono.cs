using System.Linq;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
namespace OucrcReversi.Scene {
    public class UserIdAndNameItemMono : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI _userIdText;
        [SerializeField] private TextMeshProUGUI _userNameText;
        [SerializeField] private TextMeshProUGUI _userStatusText;
        [SerializeField] private TextMeshProUGUI _resultText;
        [SerializeField] private Button _copyButton;
        private string _userId;
        private void Awake() {
            _copyButton.OnClickAsObservable().Subscribe(_ => GUIUtility.systemCopyBuffer = _userId).AddTo(this);
            GameManagerMono.Instance.OnGetAllAI.Subscribe(
                tuple => {
                    var userInfo = tuple.Item2.FirstOrDefault(x => x.id == _userId);
                    if(userInfo != null) Init(userInfo.id,userInfo.name,userInfo.status);
                }
            ).AddTo(this);
            GameManagerMono.Instance.OnGetAllUser.Subscribe(
                tuple => {
                    var userInfo = tuple.Item2.FirstOrDefault(x => x.id == _userId);
                    if(userInfo != null) Init(userInfo.id,userInfo.name,userInfo.status);
                }
            ).AddTo(this);
            GameManagerMono.Instance.OnGetAllRoom.Subscribe(
                _ => {
                    if(GameManagerMono.Instance.TryGetBattleResult(_userId,out var battleResult)) {
                        var rate = battleResult.Wins * 1f / battleResult.Battles * 100;
                        _resultText.text = $"{battleResult.Wins} / {battleResult.Battles}({rate:000.0}%)";
                    }
                }
            ).AddTo(this);
        }
        public void Init(string userId,string userName,string status) {
            _userId              = userId;
            _userIdText.text     = $"{userId}";
            _userNameText.text   = $"{userName}";
            _userStatusText.text = status ?? "null";
        }
    }
}