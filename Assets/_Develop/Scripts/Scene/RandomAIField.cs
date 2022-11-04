using OucrcReversi.AI;
using OucrcReversi.Network;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
namespace OucrcReversi.Scene {
    public class RandomAIField : MonoBehaviour {
        [SerializeField] private OucrcNetType _oucrcNetType;
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private Button _generateButton;
        private RandomPutAI _randomPutAI;
        private void Awake() {
            _inputField.onValueChanged.AsObservable().Subscribe(_ => UpdateButton()).AddTo(this);
            _generateButton.OnClickAsObservable().Subscribe(
                _ => {
                    _randomPutAI?.Dispose();
                    _randomPutAI = new RandomPutAI(_inputField.text,_oucrcNetType);
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