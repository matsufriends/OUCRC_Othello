using System.Linq;
using OucrcReversi.AI;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
namespace OucrcReversi.Scene {
    public class RandomAIDeleteItemMono : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI _userIdText;
        [SerializeField] private TextMeshProUGUI _userNameText;
        [SerializeField] private TextMeshProUGUI _userStatusText;
        [SerializeField] private Button _deleteButton;
        private RandomPutAI _randomPutAI;
        private string _userId;
        private void Awake() {
            _deleteButton.OnClickAsObservable().Subscribe(
                _ => {
                    _randomPutAI?.Dispose();
                    Destroy(gameObject);
                }
            ).AddTo(this);
            GameManagerMono.Instance.OnGetAllAI.Subscribe(
                tuple => {
                    var userInfo = tuple.Item2.FirstOrDefault(x => x.id == _userId);
                    if(userInfo != null) UpdateName(userInfo.name,userInfo.status);
                }
            ).AddTo(this);
            GameManagerMono.Instance.OnGetAllUser.Subscribe(
                tuple => {
                    var userInfo = tuple.Item2.FirstOrDefault(x => x.id == _userId);
                    if(userInfo != null) UpdateName(userInfo.name,userInfo.status);
                }
            ).AddTo(this);
        }
        public void Init(string userId,RandomPutAI randomPutAI) {
            _userId          = userId;
            _userIdText.text = userId;
            _randomPutAI     = randomPutAI;
        }
        private void UpdateName(string userName,string status) {
            _userNameText.text   = $"{userName}";
            _userStatusText.text = status ?? "null";
        }
    }
}