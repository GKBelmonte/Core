//#define moveproviderBench
//#define _moveBench
//#define _getCellsBench
using Blaze.Ai;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blaze.Games.Gotcha
{
    public enum PlayerColor { Black = -1, White = 1} ;
    public enum Direction { Up=0, Down=4, Left=6, Right=2, UpLeft=7, UpRight=1, DownLeft=5, DownRight=3} ;
    
    public class Engine : IReadableState
    {
        struct MoveInfo {
           public int row;
           public int col;
           public Direction dir ;
           public override string ToString()
           { 
               return string.Format("{0}{1}{2}", row + 1, (char)(col+65), dir);
           }
        }

        Board _board;
        PlayerColor _turn ;
        bool _gameOver ;
        MoveInfo _lastMove;
        public AdversarialAi _Thinker;
        public List<Board> _undoStack;
        
        #region Constructors
        public Engine() : this(
            new AdversarialAi(
                MoveFinder: MoveProvider, 
                Evaluator: Evaluators.FitnessEvaluatorLevel2, 
                depth: 7,
                EvalParams: (object[])new float[][] 
                {
                    new float[] { 5, 9, 9, 5 },
                    new float[] { 9, 11, 11, 9 },
                    new float[] { 9, 11, 11, 9 },
                    new float[] { 5, 9, 9, 5 }
                }, 
                queuePrioritizer: Evaluators.Prioritizer))
        {
        }

        public Engine(AdversarialAi mind)
        {
            _turn = PlayerColor.Black;
            _gameOver = false;
            _board = new Board();
            _Thinker = mind;
            _undoStack = new List<Board>();
        }

        public Engine(Engine other)
        {
            _turn = other._turn;
            _gameOver = other._gameOver;
            _board = new Board(other.Board);
            _Thinker = null;//When creating copies we should not create a new AiLibrary
            _undoStack = null; //WHen creating copies dont allocate a CtrlZ stack
        }
        #endregion

        public PlayerColor CurrentPlayer()
        {
            return _turn;
        }

        public Board Board { get { return _board; } }

        public void CheckForGameOver()
        {
            for(int ii = 0; ii < 4;++ ii)
            {
                for(int jj = 0; jj< 4;++ jj)
                {
                    var fromVal = _board.GetCell(ii, jj);
                    if (fromVal > 0 && _turn == PlayerColor.White || fromVal < 0 && _turn == PlayerColor.Black)
                    {
                        for(int kk = -1 ; kk < 2; ++kk) 
                        {
                            for(int ll =-1;  ll <2 ; ++ll)
                            {
                                var row = ii + kk;
                                var col = jj + ll;
                                if (kk == 0 && ll == 0 || row < 0 || row > 3 || col < 0 || col > 3)
                                    continue;
                                var toVal = _board.GetCell(row, col);
                                if (toVal >= 0 && _turn == PlayerColor.White || toVal <= 0 && _turn == PlayerColor.Black)
                                { _gameOver = false ; return ; }
                            }
                        }

                    }
                }
            }
            _gameOver = true;
        }

        public void Undo()
        {
            _board = _undoStack[_undoStack.Count - 1];
            _undoStack.RemoveAt(_undoStack.Count - 1);
            _board = _undoStack[_undoStack.Count - 1];
            _undoStack.RemoveAt(_undoStack.Count - 1);
        }

        #region MOVE FUNCTIONS
        public string Move(char number, char letter, string dir)
        {
            string error = "";
            int row = 0;
            switch (number)
            {
                case '1':
                    row = 0;
                    break;
                case '2':
                    row = 1;
                    break;
                case '3':
                    row = 2;
                    break;
                case '4':
                    row = 3;
                    break;
                default:
                    error = "The number provided is not correct. Please try again";
                    break;
            }
            if (error.Length > 0)
                return error;

            int col = 0;
            switch (letter)
            {
                case 'A':
                case 'a':
                    col = 0;
                    break;
                case 'B':
                case 'b':
                    col = 1;
                    break;
                case 'C':
                case 'c':
                    col = 2;
                    break;
                case 'D':
                case 'd':
                    col = 3;
                    break;
                default:
                    error = "The letter provided is not correct. Please try again";
                    break;
            }
            if (error.Length > 0)
                return error;

            Direction dirEnum = Direction.Up;

               
            if (dir == "UpLeft")
                dirEnum = Direction.UpLeft;
            else if (dir == "UpRight")
                dirEnum = Direction.UpRight;
            else if (dir == "DownLeft")
                dirEnum = Direction.DownLeft;
            else if (dir == "DownRight")
                dirEnum = Direction.DownRight;
            else if (dir == "Up")
                dirEnum = Direction.Up;
            else if (dir == "Down")
                dirEnum = Direction.Down;
            else if (dir == "Left")
                dirEnum = Direction.Left;
            else if (dir == "Right")
                 dirEnum = Direction.Right;
            else
                error = "Unrecognized direction. We're on a 2-d board.";

            if (error.Length > 0)
                return error;

            return Move(row, col, dirEnum);
        }
        
        public string Move ( int row, int col, Direction dir)
        {
            var _oldBoard = new Board(_board);
            int cellValue = _board.GetCell(row, col);
            if (cellValue == 0)
                return "No pieces in the location. Try again.";
            else if (cellValue < 0 && _turn == PlayerColor.Black || cellValue > 0 && _turn == PlayerColor.White)
            {
                string s = _move(row, col, dir, cellValue, false);

                if (s.Length > 0) //s non-empty indicates error
                    return s;
            }
            else
            {
                string s  = string.Format("It's {0} turn to play. Try again.",_turn.ToString() );
                return s;
            }
            _turn = (_turn == PlayerColor.White ? PlayerColor.Black : PlayerColor.White);
            CheckForGameOver();
            if( _gameOver )
            {
                string s = string.Format("GAME OVER: {0} wins", (_turn == PlayerColor.White ? PlayerColor.Black : PlayerColor.White));
                return s;
            }
            else
            {
                _undoStack.Add(_oldBoard);
                return "OK";
            }
        }

        public void Move(string moveStr)
        {
            //string.Format("{0}{1}{2}", row + 1, (char)(col+65), dir)
            var row = int.Parse(moveStr[0].ToString()) - 1;
            var col = ((int)moveStr[1]) - 65;
            var dir = (Direction) Enum.Parse(Direction.Down.GetType(), moveStr.Substring(2));
            Move(row, col, dir);
        }

#if _moveBench
        static double b_moveTotal = 0;
        static double b_moveGetcellVals = 0;
        static double b_moveExecute = 0;

        static public void PrintMoveBench()
        {
            Console.WriteLine("Total time spent {0} in _move", b_moveTotal);
            Console.WriteLine("Time getting {0} : {1}%", b_moveGetcellVals, b_moveGetcellVals / b_moveTotal * 100);
            Console.WriteLine("Time moving {0}: {1}%", b_moveExecute, b_moveExecute / b_moveTotal * 100);
            b_moveTotal = 0;
            b_moveGetcellVals = 0;
            b_moveExecute = 0;
        }
#endif

        private string _move(int row, int col, Direction dir, int cellValue, bool turn)
        {
            var intDir = (int)dir;
#if _moveBench
            var _moveTotalStart = DateTime.Now;
#endif
            int avCases = 0;
#if _moveBench
            var _moveGetcellsStart = DateTime.Now;
#endif

            // Find how many cells are avalaible.
            avCases = _getCellsInDirection(row, col, intDir);
#if _moveBench
            b_moveGetcellVals += (DateTime.Now - _moveGetcellsStart).TotalMilliseconds;
#endif
            string s = string.Empty;
            if (avCases == 0)
            {
                s = string.Format("The move in direction {0} is illegal. Try again.", dir.ToString());
                return s;
            }

#if _moveBench
            var _moveExecuteStart = DateTime.Now;
#endif
            cellValue = _makeMoveInDirection(row, col, dir, cellValue, avCases);
#if _moveBench
            b_moveExecute += (DateTime.Now - _moveExecuteStart).TotalMilliseconds;
#endif

            if(turn)
                _turn = (_turn == PlayerColor.White ? PlayerColor.Black : PlayerColor.White); 
#if _moveBench
            b_moveTotal += (DateTime.Now - _moveTotalStart).TotalMilliseconds;
#endif

            return s;
        }

        #region MoveHelper
        private static int[,] DIRECTION_MODIFIER_LOOKUP = 
        { 
            { -1, 0 },
            { -1, 1 },
            { 0, 1 },
            { 1, 1 },
            { 1, 0 },
            { 1, -1 },
            { 0, -1 },
            { -1, -1 }
        };
#if _getCellsBench
       static double b_getCellsTotal = 0;
       static double b_getCellsModifier = 0;
       static double b_getCellsLoop = 0;
       static double b_getCellsGet = 0;
       static public void PrintGetBench()
       {
           Console.WriteLine("Total time spent {0} in _get", b_getCellsTotal);
           Console.WriteLine("Time modifier {0} : {1}%", b_getCellsModifier, b_getCellsModifier / b_getCellsTotal * 100);
           Console.WriteLine("Time loop {0}: {1}%", b_getCellsLoop, b_getCellsLoop / b_getCellsTotal * 100);
           Console.WriteLine("Time get {0}: {1}%", b_getCellsGet, b_getCellsGet / b_getCellsTotal * 100);
           b_getCellsTotal = 0;
           b_getCellsModifier = 0;
           b_getCellsLoop = 0;
           b_getCellsGet = 0;
       }
#endif
        private int _getCellsInDirection(int row, int col, int dir)
        {
#if _getCellsBench
            var _totalStart = DateTime.Now;
            var _modifierStart = DateTime.Now;
#endif
            var ii_mod = DIRECTION_MODIFIER_LOOKUP[dir, 0]; //(dir) % 4 > 0 ? (dir > 3 ? -1 : +1) : (0); 
            var jj_mod = DIRECTION_MODIFIER_LOOKUP[dir, 1]; //((dir + 6) % 8) % 4 > 0 ? (((dir + 6) % 8) > 3 ? -1 : +1) : (0);
#if _getCellsBench
            b_getCellsModifier += (DateTime.Now - _modifierStart).TotalMilliseconds;
#endif
            var raw = _board.Raw;
#if _getCellsBench
            var _loopStart = DateTime.Now;
#endif
            var res = 0;

            if (_turn == PlayerColor.Black)
            {
                var ii = row + ii_mod * (1);
                var jj = col + jj_mod * (1);
                if (ii >= 0 && ii < 4 && jj >= 0 && jj < 4  && raw[ii][jj] <= 0)
                {
                    ++res;
                    ii += ii_mod * (1);
                    jj += jj_mod * (1);
                    if (ii >= 0 && ii < 4 && jj >= 0 && jj < 4 && raw[ii][jj] <= 0)
                    {
                        ++res;
                        ii += ii_mod * (1);
                        jj += jj_mod * (1);
                        if (ii >= 0 && ii < 4 && jj >= 0 && jj < 4 && raw[ii][jj] <= 0)
                            ++res;
                    }

                }
            }
            else
            {
                var ii = row + ii_mod * (1);
                var jj = col + jj_mod * (1);
                if (ii >= 0 && ii < 4 && jj >= 0 && jj < 4 && raw[ii][jj] >= 0)
                {
                    ++res;
                    ii += ii_mod * (1);
                    jj += jj_mod * (1);
                    if (ii >= 0 && ii < 4 && jj >= 0 && jj < 4 && raw[ii][jj] >= 0)
                    {
                        ++res;
                        ii += ii_mod * (1);
                        jj += jj_mod * (1);
                        if (ii >= 0 && ii < 4 && jj >= 0 && jj < 4 && raw[ii][jj] >= 0)
                            ++res;
                    }
                        
                }
            }

#if _getCellsBench
            b_getCellsLoop += (DateTime.Now - _loopStart).TotalMilliseconds;
            b_getCellsTotal += (DateTime.Now - _totalStart).TotalMilliseconds;
#endif
            return res;
        }

        private int _makeMoveInDirection(int row, int col, Direction dir, int cellValue, int avCases)
        {
            _board.SetCell(row, col, 0);
            var row_mod = DIRECTION_MODIFIER_LOOKUP[(int)dir, 0];
            var col_mod = DIRECTION_MODIFIER_LOOKUP[(int)dir, 1];
            for (int ii = 1; ii <= avCases && cellValue != 0; ++ii)
            {
                int value = (int)_turn * ii;
                value = Math.Abs(value) > Math.Abs(cellValue) ? cellValue : value;
                int toAdd = (ii == avCases) ? cellValue : value;
                _board.AddToCell(row + ii * row_mod, col+ii*col_mod, toAdd);
                cellValue -= value;
                
            }
            _lastMove.col = col; _lastMove.row = row; _lastMove.dir = dir;
            return cellValue;
        }

        #endregion 




        #endregion


        public void PrintBoard()
        {
            _board.Print();
        }

        public bool GameOver ( )
        {
            return _gameOver;
        }

        private static Direction GetDirectionFromCoordinatePair( int kk, int ll)
        {
            if (0 > kk)//General up
            {
                if (ll > 0)//Right
                    return Direction.UpRight;
                else if (ll == 0)//Neither
                    return Direction.Up;
                else //Left
                    return Direction.UpLeft;
            }
            else if (0 == kk) //Left or right
            {
                if (ll > 0)//Right
                    return Direction.Right;
                else if (ll == 0)//Neither
                    throw new ArgumentException("The coordinates must be non-zero for a direction to exist");
                else //Left
                    return Direction.Left;
            }
            else //General down 
            {
                if (ll > 0)//Right
                    return Direction.DownRight;
                else if (ll == 0)//Neither
                    return Direction.Down;
                else //Left
                    return Direction.DownLeft;
            }
        }
        
        public double totalTime = 0;
        public string MakeAiMove()
        {
            return MakeAiMove(6000);
        }

        public string MakeAiMove(int timeout)
        {
            double time = 0;
            //var move =  (Engine)thinker.AlphaBeta(this, _turn == PlayerColor.White, ref time);
            var move = (Engine)_Thinker.AlphaBetaTimeLimited(this, _turn == PlayerColor.White, timeout, ref time);
            
            totalTime += time;
            this.Move(move._lastMove.row, move._lastMove.col, move._lastMove.dir);
            return move._lastMove.ToString(); ;
        }
        public string StateMove()
        {
            return _lastMove.ToString();
        }

        public string State()
        {
            return Board.ToString();
        }
        public string State(int padding)
        {
            var split = Board.ToString().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            for (var ii = 0; ii < split.Length; ++ii)
            {
                split[ii] = split[ii].PadRight(padding);
            }
            return string.Join("\n", split);
        }



#if moveproviderBench
        static double b_moveproviderTotal = 0;
        static double b_moveproviderClone = 0;
        static double b_moveproviderMove = 0;
        static public void PrintMoveproviderBench()
        {
            Console.WriteLine("Total time spent {0} in MoveProvider",b_moveproviderTotal);
            Console.WriteLine("Time cloning {0} : {1}%",b_moveproviderClone,b_moveproviderClone/b_moveproviderTotal*100);
            Console.WriteLine("Time moving {0}: {1}%", b_moveproviderMove, b_moveproviderMove / b_moveproviderTotal * 100);
            b_moveproviderTotal = 0;
            b_moveproviderClone = 0;
            b_moveproviderMove = 0;
        }
#endif
        public static FindAllPossibleMoves MoveProvider
        { get { return new FindAllPossibleMoves(_MoveProvider); } }

        private static List<object> _MoveProvider(object state)
        {
#if moveproviderBench
            var benchStart = DateTime.Now;
#endif
            var parseState = (Engine)state;
            PlayerColor next = parseState._turn;
            List<object> result = new List<object>();
            // Console.WriteLine("finding moves av to {0}", next);
            for (int ii = 0; ii < 4; ++ii)
            {
                for (int jj = 0; jj < 4; ++jj)
                {
                    var cellValue = parseState._board.GetCell(ii, jj);
                    if (cellValue > 0 && next == PlayerColor.White || cellValue < 0 && next == PlayerColor.Black)
                    {
                        for (int kk = -1; kk < 2; ++kk)
                        {
                            for (int ll = -1; ll < 2; ++ll)
                            {
                                var row = ii + kk; var col = jj + ll;
                                if (kk == 0 && ll == 0 || row < 0 || row > 3 || col < 0 || col > 3)
                                    continue;


                                if (parseState._board.GetCell(row, col) >= 0 && next == PlayerColor.White || parseState._board.GetCell(row, col) <= 0 && next == PlayerColor.Black)
                                {
#if moveproviderBench
                                    var cloneStart = DateTime.Now;
#endif
                                    var clone = /*parseState.Clone();*/ new Engine(parseState);
#if moveproviderBench
                                    b_moveproviderClone += (DateTime.Now - cloneStart).TotalMilliseconds;
#endif
                                    var dir = GetDirectionFromCoordinatePair(kk, ll);
                                    //                         Console.WriteLine("Direction possible is {0}", dir);
#if moveproviderBench
                                    var moveStart = DateTime.Now;
#endif
                                    clone._move(ii, jj, dir, cellValue, true);
#if moveproviderBench
                                    b_moveproviderMove += (DateTime.Now - moveStart).TotalMilliseconds;
#endif
                                    result.Add(clone);
                                }
                            }
                        }

                    }
                }
            }
#if moveproviderBench
            b_moveproviderTotal += (DateTime.Now - benchStart).TotalMilliseconds;
#endif
            return result;
        }

        public static int NumberOfPossibleMovesDifference(Engine state)
        {
            var parseState = state;
            PlayerColor next = parseState._turn;
            var result = 0;
            for (int ii = 0; ii < 4; ++ii)
            {
                for (int jj = 0; jj < 4; ++jj)
                {
                    var cellValue = parseState._board.GetCell(ii, jj);
                    var movePlayer = cellValue > 0 ? PlayerColor.White : PlayerColor.Black;
                    if (cellValue != 0 )
                    {
                        for (int kk = -1; kk < 2; ++kk)
                        {
                            for (int ll = -1; ll < 2; ++ll)
                            {
                                var row = ii + kk; var col = jj + ll;
                                if (kk == 0 && ll == 0 || row < 0 || row > 3 || col < 0 || col > 3)
                                    continue;
                                var nextCellVal = parseState._board.GetCell(row, col);
                                if (nextCellVal >= 0 && movePlayer == PlayerColor.White)
                                    ++result;
                                else if (nextCellVal <= 0 && movePlayer == PlayerColor.Black)
                                    --result;
                            }
                        }

                    }
                }
            }
            return result;
        }


    }


    
}
