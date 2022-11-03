using MornLib.Scenes;
using OucrcReversi.AI;
using OucrcReversi.Network;
namespace OucrcReversi.Scene {
    public class WatchSceneMono : MornSceneMono {
        private void Awake() {
            var ai = new RandomPutAI("ランダム君",OucrcNetType.Watch);
            var a = new BoardStatusPoller(OucrcNetType.Watch);
        }
    }
}