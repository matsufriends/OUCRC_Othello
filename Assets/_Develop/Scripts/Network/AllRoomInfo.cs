using System;
namespace OucrcReversi.Network {
    [Serializable]
    public sealed class AllRoomInfo {
        //json形式に変換するため、変数名変更不可
        public RoomIdUsersAndBoard[] rooms;
    }
}