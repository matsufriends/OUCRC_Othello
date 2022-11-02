using System;
namespace OucrcReversi.Network {
    [Serializable]
    public class CellPositionAndUserId {
        //json形式に変換するため、変数名変更不可
        public int row;
        public int column;
        public string user_id;
    }
}