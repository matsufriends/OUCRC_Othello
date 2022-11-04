using TMPro;
using UnityEngine;
namespace OucrcReversi.Scene {
    public class UserIdAndNameItemMono : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI _userIdText;
        [SerializeField] private TextMeshProUGUI _userNameText;
        public void Init(string userId,string userName) {
            _userIdText.text   = $"UserId: {userId}";
            _userNameText.text = $"Name: {userName}";
        }
    }
}