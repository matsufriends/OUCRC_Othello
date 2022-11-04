using OucrcReversi.Network;
using UnityEngine;
namespace OucrcReversi.AI {
    public class RandomPutAiMono : MonoBehaviour {
        [SerializeField] private string id;
        private void Start() {
            var a = new RandomPutAI(id,OucrcNetType.Watch);
        }
    }
}