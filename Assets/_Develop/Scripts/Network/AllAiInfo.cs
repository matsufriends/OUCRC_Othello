using System;
namespace OucrcReversi.Network {
    [Serializable]
    public sealed class AllAiInfo {
        //json形式に変換するため、変数名変更不可
        public AiInfo[] ais;
    }
}