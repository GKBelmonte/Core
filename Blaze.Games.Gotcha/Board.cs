using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GotchaEngine
{
    public class Board
    {
        int[][] _board ;


        public Board ()
        {
            _board = new int[][] 
            {
                new int[] {-10,0,0,0},
                new int[] {0,0,0,0},
                new int[] {0,0,0,0},
                new int[] {0,0,0,10}
            };
        }

        public Board( Board other)
        {
            var raw = other._board;
            _board = new int[][] 
            {
                new int[] {raw[0][0],raw[0][1],raw[0][2],raw[0][3]},
                new int[] {raw[1][0],raw[1][1],raw[1][2],raw[1][3]},
                new int[] {raw[2][0],raw[2][1],raw[2][2],raw[2][3]},
                new int[] {raw[3][0],raw[3][1],raw[3][2],raw[3][3]}
            };
        }

        public int[][] Raw
        {
            get { return _board; }
        }

        public void Print()
        {
            //Console.WriteLine(this);

            Console.WriteLine("     A   B   C   D");
            for (int ii = 0; ii < 4; ++ii)
            {
                Console.Write(string.Format("{0}|", ii + 1));
                for (int jj = 0; jj < 4; ++jj)
                {
                    if (_board[ii][jj] < 0) Console.ForegroundColor = ConsoleColor.Red;
                    else if (_board[ii][jj] > 0) Console.ForegroundColor = ConsoleColor.Cyan;
                    else Console.ForegroundColor = ConsoleColor.DarkBlue;
                    Console.Write(string.Format("{0}", _board[ii][jj]).PadLeft(4));
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
                Console.WriteLine();
            }
        }

        public int GetCell(int row, int col)
        {
            return _board[row][col];
        }

        public void SetCell(int row, int col, int val)
        {
            _board[row][col] = val;
        }

        public void AddToCell(int row , int col, int val)
        {
            _board[row][col] += val;
        }


        public override string  ToString()
        {
            StringBuilder build = new StringBuilder();
           build.AppendLine("     A    B    C    D");
            for (int ii = 0; ii < 4; ++ii)
            {
                build.Append(string.Format("{0}|", ii + 1));
                for (int jj = 0; jj < 4; ++jj)
                {
                    build.Append(string.Format("{0}", _board[ii][ jj]).PadLeft(5));
                }
                build.AppendLine();
            }
            return build.ToString();
        }
    }
}
