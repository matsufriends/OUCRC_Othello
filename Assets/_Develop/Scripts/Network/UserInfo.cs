using System;
namespace OucrcReversi.Network {
    [Serializable]
    public class UserInfo {
        //json形式に変換するため、変数名変更不可
        public string id;
        public string name;
        public string status;
    }
}