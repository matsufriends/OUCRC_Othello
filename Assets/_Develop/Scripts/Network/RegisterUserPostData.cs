using System;
namespace OucrcReversi.Network {
    [Serializable]
    public class RegisterUserPostData {
        //json形式に変換するため、変数名変更不可
        public string user_name;
    }
}