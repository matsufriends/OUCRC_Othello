using System;
namespace OucrcReversi.Network {
    public enum OucrcNetType {
        Watch
       ,Battle
    }
    public static class OucrcNetTypeEx {
        public static string ToString(OucrcNetType oucrcNetType) => oucrcNetType switch {
            OucrcNetType.Watch  => "Watch"
           ,OucrcNetType.Battle => "Battle"
           ,_                   => throw new ArgumentOutOfRangeException(nameof(oucrcNetType),oucrcNetType,null)
        };
    }
}