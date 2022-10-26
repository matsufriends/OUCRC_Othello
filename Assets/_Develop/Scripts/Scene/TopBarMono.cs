using MornLib.Scenes;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
namespace Scene {
    public class TopBarMono : MornSceneMono {
        [SerializeField] private Vector2 _hidePos;
        [SerializeField] private Vector2 _showPos;
        [SerializeField] private RectTransform _barRect;
        [SerializeField] private Button _barToggleButton;
        private bool _isActive;
        private void Awake() {
            _barToggleButton.OnClickAsObservable().Subscribe(
                _ => {
                    _isActive = !_isActive;
                }
            ).AddTo(this);
        }
    }
}