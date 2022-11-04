using System;
namespace OucrcReversi.Network {
    [Serializable]
    public class AIPutPostData {
        //json形式に変換するため、変数名変更不可
        public string user_id;
        public int row;
        public int column;
    }
}