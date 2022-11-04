using System;
namespace OucrcReversi.Network {
    [Serializable]
    public sealed class MakeBattleRoomData {
        public string user_id;
        public string ai_id;
    }
}