using OucrcReversi.Network;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
namespace OucrcReversi.Scene {
    public class IPAndPortInputFieldMono : MonoBehaviour {
        [SerializeField] private OucrcNetType _oucrcNetType;
        [SerializeField] private TMP_InputField _ipInputField;
        [SerializeField] private TMP_InputField _portInputField;
        [SerializeField] private Button _button;
        private const string c_ipKey = "IpKey";
        private const string c_portKey = "PortKey";
        private void Awake() {
            _ipInputField.text   = PlayerPrefs.GetString(_oucrcNetType + c_ipKey,"");
            _portInputField.text = PlayerPrefs.GetString(_oucrcNetType + c_portKey,"");
            _ipInputField.onValueChanged.AsObservable().Subscribe(_ => UpdateButton()).AddTo(this);
            _portInputField.onValueChanged.AsObservable().Subscribe(_ => UpdateButton()).AddTo(this);
            _button.OnClickAsObservable().Subscribe(
                _ => {
                    PlayerPrefs.SetString(_oucrcNetType + c_ipKey,_ipInputField.text);
                    PlayerPrefs.SetString(_oucrcNetType + c_portKey,_portInputField.text);
                    ServerUtility.Instance.SetUrl(_oucrcNetType,GetUrl());
                }
            ).AddTo(this);
            UpdateButton();
            if(_button.gameObject.activeSelf) ServerUtility.Instance.SetUrl(_oucrcNetType,GetUrl());
        }
        private string GetUrl() {
            return $"http://{_ipInputField.text}:{_portInputField.text}";
        }
        private void UpdateButton() {
            _button.gameObject.SetActive(_ipInputField.text.Length > 0 && _portInputField.text.Length > 0);
        }
    }
}