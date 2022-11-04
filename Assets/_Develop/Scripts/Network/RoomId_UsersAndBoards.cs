using System;
using System.Linq;
using MornLib.Cores;
using MornLib.Pool;
using OucrcReversi.Cell;
using UnityEngine;
namespace OucrcReversi.Network {
    [Serializable]
    public class RoomIdUsersAndBoard {
        //json形式に変換するため、変数名変更不可
        public string id;
        public UserInfo black;
        public UserInfo white;
        public UserInfo next;
        public int[][] board;
        public Vector2Int BoardSize => new(board[0].Length,board.Length);
        public CellColor NextCellColor => next == null ? CellColor.None : next.id == black.id ? CellColor.Black : CellColor.White;
        public int GetCellCount() {
            return board.Sum(row => row.Count(color => color is 1 or 2));
        }
        public CellColor[,] GetGrid() {
            var size = BoardSize;
            var grid = new CellColor[size.x,size.y];
            for(var y = 0;y < size.y;y++) {
                for(var x = 0;x < size.x;x++) grid[x,y] = CellColorEx.IntToCellColor(board[y][x]);
            }
            return grid;
        }
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
    }
}