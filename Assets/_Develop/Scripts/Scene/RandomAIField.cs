using OucrcReversi.AI;
using OucrcReversi.Network;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
namespace OucrcReversi.Scene {
    public class RandomAIField : MonoBehaviour {
        [SerializeField] private RandomAIDeleteItemMono _randomDeleteButton;
        [SerializeField] private Transform _randomAiParent;
        [SerializeField] private OucrcNetType _oucrcNetType;
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private Button _generateButton;
        private void Awake() {
            _inputField.onValueChanged.AsObservable().Subscribe(_ => UpdateButton()).AddTo(this);
            _generateButton.OnClickAsObservable().Subscribe(
                _ => {
                    var randomPutAI = new RandomPutAI(_inputField.text,_oucrcNetType);
                    var delete = Instantiate(_randomDeleteButton,_randomAiParent);
                    delete.Init(_inputField.text,randomPutAI);
                    _generateButton.gameObject.SetActive(false);
                }
            ).AddTo(this);
            UpdateButton();
        }
        private void UpdateButton() {
            _generateButton.gameObject.SetActive(_inputField.text.Length > 0);
        }
    }
}