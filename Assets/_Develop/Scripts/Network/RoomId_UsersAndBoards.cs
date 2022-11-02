using System;
using System.Linq;
using MornLib.Cores;
using MornLib.Pool;
using OucrcReversi.Cell;
using OucrcReversi.oucrcNet;
using UnityEngine;
namespace OucrcReversi.Network {
    [Serializable]
    public class RoomIdUsersAndBoards {
        //json形式に変換するため、変数名変更不可
        public string id = " ";
        public UserInfo black = new();
        public UserInfo white = new();
        public UserInfo next = new();
        public int[][] board = new int[20][];
        public Vector2Int Size => new(board.GetLength(1),board.GetLength(0));
        public void Log() {
            var builder = MornSharedObjectPool<MornStringBuilder>.Rent();
            builder.Init(',');
            builder.Append($"id:{id}\n");
            foreach(var row in board) {
                foreach(var color in row) {
                    builder.Append(color.ToString());
                }
                builder.Append("\n");
            }
            Debug.Log(builder.Get());
            MornSharedObjectPool<MornStringBuilder>.Return(builder);
        }
        public int GetCellCount() => board.Sum(row => row.Count(color => color is 1 or 2));
        public CellColor[,] GetGrid() {
            var size = Size;
            var grid = new CellColor[size.x,size.y];
            for(var y = 0;y < size.y;y++) {
                for(var x = 0;x < size.x;x++) {
                    grid[x,y] = CellColorEx.IntToCellColor(board[y][x]);
                }
            }
            return grid;
        }
    }
}