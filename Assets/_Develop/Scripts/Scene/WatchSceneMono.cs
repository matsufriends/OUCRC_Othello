using MornLib.Scenes;
using OucrcReversi.Network;
namespace OucrcReversi.Scene {
    public class WatchSceneMono : MornSceneMono {
        private void Awake() {
            var a = new BoardStatusPoller(OucrcNetType.Watch);
        }
    }
}