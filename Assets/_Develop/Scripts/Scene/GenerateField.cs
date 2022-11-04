using Cysharp.Threading.Tasks;
using OucrcReversi.Network;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
namespace OucrcReversi.Scene {
    public class GenerateField : MonoBehaviour {
        [SerializeField] private OucrcNetType _oucrcNetType;
        [SerializeField] private bool _isAi;
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private Button _generateButton;
        private void Awake() {
            _inputField.onValueChanged.AsObservable().Subscribe(_ => UpdateButton()).AddTo(this);
            _generateButton.OnClickAsObservable().Subscribe(
                _ => {
                    ServerUtility.Instance.PostRegisterUserOrAi(
                        _oucrcNetType,_isAi,new RegisterUserPostData {
                            user_name = _inputField.text
                        },gameObject.GetCancellationTokenOnDestroy()
                    ).Forget();
                }
            ).AddTo(this);
            UpdateButton();
        }
        private void UpdateButton() {
            _generateButton.gameObject.SetActive(_inputField.text.Length > 0);
        }
    }
}