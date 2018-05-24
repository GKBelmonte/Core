//#define AUTOPLAY
//#define BENCHING
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blaze.Games.Gotcha
{
    class Program
    {
#if AUTOPLAY
        static bool AUTOPLAY = true;
#else
        static bool AUTOPLAY = false;
#endif
        static void Main(string[] args)
        {
            bool AI = true;

            Console.WriteLine("Play ball!");
            Engine e = new Engine();
            bool inputOk = false;
            PlayerColor aiColor = PlayerColor.White;

            do
            {
                inputOk = true;
                Console.Write("Against AI? (y/n): ");
                var key = Console.ReadKey();
                Console.WriteLine();
                if (key.KeyChar == 'y')
                    AI = true;
                else if (key.KeyChar == 'n')
                    AI = false;
                else
                {
                    Console.WriteLine("Incorrect input. Expecting y/n");
                    inputOk = false;
                }

            } while (!inputOk);

            if (AI)
            {
                do
                {
                    inputOk = true;
                    Console.Write("Playing black or white? (b/w): ");
                    var key = Console.ReadKey();
                    Console.WriteLine();
                    if (key.KeyChar == 'b')
                        aiColor = PlayerColor.White;
                    else if (key.KeyChar == 'w')
                        aiColor = PlayerColor.Black;
                    else
                    {
                        Console.WriteLine("Incorrect input. Expecting b/w");
                        inputOk = false;
                    }

                } while (!inputOk);
            }
#if BENCHING || AUTOPLAY
            var count = 0;
            while (!e.GameOver() && count++ < 500)
            {
#else
            while (!e.GameOver() )
            {
#endif
                e.PrintBoard();

#if !AUTOPLAY
                if (e.CurrentPlayer() != aiColor || !AI)
                {
                    Console.WriteLine(string.Format("Player {0} pick a cell: ", e.CurrentPlayer()));
                    string read;
                    char num, let;
                    string dir;
                    read = Console.ReadLine();
                    if (read.Length < 2)
                    {
                        Console.WriteLine("Incorrect entry \"{0}\". Example entry is 1A", read);
                        continue;
                    }
                    else if (read == "ctrlz")
                    {
                        e.Undo();
                        continue;
                    }
                    num = read[0];
                    let = read[1];

                    if (read.Length >= 3)
                    {
                        dir = read[2].ToString();
                    }
                    else
                    {
                        Console.WriteLine("Direction: ");
                        dir = Console.ReadLine(); 
                        var dirInt = 0;
                        var isInt = int.TryParse(dir, out dirInt);
                        if (isInt)
                        {
                            dir = GetDirectionFromInt(dir, dirInt);
                        }
                        
                    }
                    Console.WriteLine(string.Format("\n{0}\n", e.Move(num, let, dir)));
                }
#endif
                if ((AI) && (e.CurrentPlayer() == aiColor) || AUTOPLAY)
                {
                    //Console.ReadKey(false); 
                    var aiMove = e.MakeAiMove();
                    Console.WriteLine(string.Format("AI made the move {0}", aiMove));
                    var aiMoveStd = AiMoveToStd(aiMove);
                    Console.WriteLine(aiMoveStd);
                    Console.WriteLine("Pruned total : {0} ; Evaluated total : {1}", e._Thinker.SavedChildrenTotal, e._Thinker.EvaluatedTotal);
                    // Engine.thinker.PrintBench();

                    //Engine.PrintMoveproviderBench();
                     e._Thinker.LogTree();
                    //  Engine.PrintMoveBench();
                    //  Engine.PrintGetBench();

                }

            }
            Console.WriteLine("Game Over");
#if BENCHING && AUTOPLAY
            Console.WriteLine("Total time: {0} ...  Per move : {1}", e.totalTime, e.totalTime / count);
#endif

            Console.ReadKey();

        }

        private static string AiMoveToStd(string aiMove)
        {

            if (aiMove.Contains("UpRight"))
                return aiMove.Substring(0, 2) + "2";

            else if (aiMove.Contains("DownRight"))
                return aiMove.Substring(0, 2) + "4";

            else if (aiMove.Contains("DownLeft"))
                return aiMove.Substring(0, 2) + "6";

            else if (aiMove.Contains("UpLeft"))
                return aiMove.Substring(0, 2) + "8";
            else if (aiMove.Contains("Right"))
                return aiMove.Substring(0, 2) + "3";
            else if (aiMove.Contains("Up"))
                return aiMove.Substring(0, 2) + "1";
            else if (aiMove.Contains("Down"))
                return aiMove.Substring(0, 2) + "5";
            else if (aiMove.Contains("Left"))
                return aiMove.Substring(0, 2) + "7";
            else
                return aiMove;
        }

        private static string GetDirectionFromInt(string dir, int dirInt)
        {
            switch (dirInt)
            {
                case 1:
                    dir = "Up";
                    break;
                case 2:
                    dir = "UpRight";
                    break;
                case 3:
                    dir = "Right";
                    break;
                case 4:
                    dir = "DownRight";
                    break;
                case 5:
                    dir = "Down";
                    break;
                case 6:
                    dir = "DownLeft";
                    break;
                case 7:
                    dir = "Left";
                    break;
                case 8:
                    dir = "UpLeft";
                    break;

            }
            return dir;
        }
    }
}
