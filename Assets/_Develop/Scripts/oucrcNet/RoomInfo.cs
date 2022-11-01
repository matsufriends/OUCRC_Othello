using System;
using Cell;
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
        public Vector2Int Size => new(board.GetLength(1),board.GetLength(0));
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
        public int GetCellCount() {
            var cellCount = 0;
            var size = Size;
            for(var y = 0;y < size.y;y++) {
                for(var x = 0;x < size.x;x++) {
                    if(board[y][x] == 1 || board[y][x] == 2) cellCount++;
                }
            }
            return cellCount;
        }
        public CellColor[,] GetGrid() {
            var size = Size;
            var grid = new CellColor[size.x,size.y];
            for(var x = 0;x < size.x;x++) {
                for(var y = 0;y < size.y;y++) {
                    var cellColor = board[y][x] switch {
                        0 => CellColor.None
                       ,1 => CellColor.Black
                       ,2 => CellColor.White
                       ,_ => throw new ArgumentOutOfRangeException()
                    };
                    grid[x,y] = cellColor;
                }
            }
            return grid;
        }
    }
}