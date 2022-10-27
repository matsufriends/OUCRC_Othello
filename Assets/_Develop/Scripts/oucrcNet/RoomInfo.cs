using System;
using MornLib.Cores;
using UnityEngine;
namespace oucrcNet {
    [Serializable]
    public class RoomInfo {
        public string id = " ";
        public UserInfo black = new();
        public UserInfo white = new();
        public UserInfo next = new();
        public int[][] board = new int[20][];
        public void Log() {
            MornStringBuilder.Init(',');
            MornStringBuilder.Append($"id:{id}\n");
            for(var y = 0;y < board.GetLength(0);y++) {
                for(var x = 0;x < board[y].GetLength(0);x++) {
                    var color = board[y][x];
                    MornStringBuilder.Append(color.ToString());
                }
                MornStringBuilder.Append("\n");
            }
            Debug.Log(MornStringBuilder.Get());
        }
    }
}