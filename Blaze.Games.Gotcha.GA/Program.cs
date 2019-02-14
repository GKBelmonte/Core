using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Blaze.Ai.Ages;

namespace Blaze.Games.Gotcha.GA
{
    class Program
    {
        static void Main(string[] args)
        {
            var pop = new List<IIndividual>(32);
            Console.WriteLine("Read from file individuals?(y/n)");
            var yayornay = Console.ReadLine();
            if (yayornay == "y")
            {
                Console.WriteLine("File path");
                var path = Console.ReadLine();
                string []  strArr = null;
                try{
                    strArr = File.ReadAllLines(path);}
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.ReadKey(false);
                    Environment.Exit(1);
                }
                foreach (var str in strArr)
                {
                    Console.WriteLine("Read " + str);
                    if (str.Trim() == string.Empty)
                        continue;
                    pop.Add(new GotchaIndividual(str));
                }
            }
            else
            {
                Console.WriteLine("Creating individuals");
                for (var ii = 0; ii < 32; ++ii)
                {
                    pop.Add(new GotchaIndividual());
                }
            }

            Ages myGA = new Ages(
                1000,
                new CompareEvaluate(Evaluate),
                GotchaIndividual.GarboCrossoverOperator,
                (r) => new GotchaIndividual(),
                pop);

            myGA.SetRandomSeed(0);

            myGA.GoThroughGenerations();

            Console.ReadKey();
        }
        const int MAX_MOVES = 45;

        private static float Evaluate(IIndividual l, IIndividual r)
        {
            var left = ((GotchaIndividual)l ).CreateInstanceEngine();
            var right = ((GotchaIndividual)r ).CreateInstanceEngine();
            Console.WriteLine(l.Name + "                       vs.                        " + r.Name);
            //Console.WriteLine(((GarboIndividual)l).ToString() + " vs. " + ((GarboIndividual)l).ToString());
            var turns = 0;
            var lMove = left.MakeAiMove();
            right.Move(lMove);
            var rMove = right.MakeAiMove();
            right.PrintBoard();
            do
            {
                left.Move(rMove); //execute right's move on left engine
                if (left.GameOver()) //If game over, left lost
                {
                    Console.WriteLine(l.Name + " lost");
                    return -turns;
                }
                lMove = left.MakeAiMove(); //execute lefts move
                if (left.GameOver()) //If game over, left won
                {
                    Console.WriteLine(l.Name + " won");
                    return turns;
                }
                right.Move(lMove); //Execute lefts move on right engine
                rMove = right.MakeAiMove(); //Get right's next move
                if(turns %10 == 0)
                    right.PrintBoard();
               // Console.ReadKey(false);
            } while (turns++ < MAX_MOVES);
            var rMoves = Engine.MoveProvider(right).Count;
            var lMoves = Engine.MoveProvider(left).Count;
            Console.WriteLine(right.totalTime / turns );
            Console.WriteLine(l.Name + " drew");
            var adv = rMoves-lMoves;
            Console.WriteLine("Advantage to " + (adv > 0  ? "left":"right") + " by " + adv);
            return adv; 
        }
    }
}
