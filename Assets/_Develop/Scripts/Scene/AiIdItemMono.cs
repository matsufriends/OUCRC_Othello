using TMPro;
using UnityEngine;
namespace OucrcReversi.Scene {
    public class AiIdItemMono : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI _userIdText;
        public void Init(string userId) {
            _userIdText.text = $"AI ID: {userId}";
        }
    }
}