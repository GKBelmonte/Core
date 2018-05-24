using Blaze.Ai;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blaze.Games.Gotcha
{
    public static class Evaluators
    {
        public static GetFitnessForState FitnessEvaluatorLevel2
        { get { return new GetFitnessForState(fitnessEvaluatorLevel2); } }

        static float fitnessEvaluatorLevel2(object state, object parameters)
        {
            Engine castState = (Engine)state;
            castState.CheckForGameOver();
            if (castState.GameOver())
            {
                if (castState.CurrentPlayer() == PlayerColor.Black)
                    return float.PositiveInfinity;//win condition for white: black can't move
                else
                    return float.NegativeInfinity;
            }
            var b = castState.Board.Raw;
            float res = 0;
            var gradient = (float[][])parameters;
            for (var ii = 0; ii < 4; ++ii)
            {
                for (var jj = 0; jj < 4; ++jj)
                {
                    var v = b[ii][jj];
                    if (v != 0)
                        res += gradient[ii][jj] / (float)v;
                }
            }
            return res;
        }


        public static GetFitnessForState FitnessEvaluatorLevel1
        { get { return new GetFitnessForState(fitnessEvaluatorLevel1); } }

        static float fitnessEvaluatorLevel1(object state, object parameters)
        {
            Engine castState = (Engine)state;
            castState.CheckForGameOver();
            if (castState.GameOver())
            {
                if (castState.CurrentPlayer() == PlayerColor.Black)
                    return float.PositiveInfinity;//win condition for white: black can't move
                else
                    return float.NegativeInfinity;
            }
            var b = castState.Board;
            float res = 0;
            for (var ii = 0; ii < 4; ++ii)
            {
                for (var jj = 0; jj < 4; ++jj)
                {
                    var v = b.GetCell(ii, jj);
                    if (v > 0)
                        res += 1;
                    else if (v < 0)
                        res -= 1;

                }
            }
            return res;
        }


        static object[] paras = (object[])new float[][] { new float[] { 5, 9, 9, 5 }, new float[] { 9, 11, 11, 9 }, new float[] { 9, 11, 11, 9 }, new float[] { 5, 9, 9, 5 } };

        public static Prioritizer Prioritizer
        { get { return  new Prioritizer(fPrioritizer); } }

        static void fPrioritizer(List<object> mves, bool maximizing)
        {
            if(!maximizing)
                mves.Sort((left, right) => (int)(floatToComparaison(fitnessEvaluatorLevel2(left, paras), fitnessEvaluatorLevel2(right, paras))));
               // mves.Sort((left, right) => (int)( fitnessEvaluatorLevel1(left, null) - fitnessEvaluatorLevel1(right, null) ));
            else
                mves.Sort((left, right) => (int)(floatToComparaison(fitnessEvaluatorLevel2(right, paras), fitnessEvaluatorLevel2(left, paras))));
               // mves.Sort((left, right) => (int)(fitnessEvaluatorLevel1(right, null)- fitnessEvaluatorLevel1(left, null)));
            
        }

        static int floatToComparaison(float x, float y)
        {
            
            if (float.IsInfinity(x) )
            {
                if (float.IsPositiveInfinity(x))
                {
                    if (float.IsPositiveInfinity(y)) //+inf - ( +inf)
                        return 0;
                    else if (float.IsNegativeInfinity(y)) // +inf - (-inf)
                        return int.MaxValue;
                    else                // +inf - (val)
                        return int.MaxValue;
                }
                else //if (float.IsNegativeInfinity(x))
                {
                    if (float.IsPositiveInfinity(y)) // (-inf) - (inf)
                        return int.MinValue;
                    else if (float.IsNegativeInfinity(y))//(-inf) - ( - inf )
                        return 0;
                    else
                        return int.MinValue;
                }
            }
            else if ( float.IsInfinity(y) ) //But NOT x
            {
                if (float.IsPositiveInfinity(y)) // val - (+inf)
                    return int.MinValue;
                else
                    return int.MaxValue;
            }
            else
            {
                return (int)(x - y);
            }
        }


        public static GetFitnessForState FitnessEvaluatorAvalaibleMoves
        { get { return new GetFitnessForState(fitnessEvaluatorAvalaibleMoves); } }

        static float fitnessEvaluatorAvalaibleMoves(object state, object parameters)
        {
            Engine castState = (Engine)state;
            castState.CheckForGameOver();
            if (castState.GameOver())
            {
                if (castState.CurrentPlayer() == PlayerColor.Black)
                    return float.PositiveInfinity;//win condition for white: black can't move
                else
                    return float.NegativeInfinity;
            }
            return Engine.NumberOfPossibleMovesDifference(castState);
        }


        public static GetFitnessForState FitnessEvaluatorLevel3
        { get { return new GetFitnessForState(fitnessEvaluatorLevel3); } }

        static float fitnessEvaluatorLevel3(object state, object parameters)
        {
            var eval = Evaluators.fitnessEvaluatorLevel2(state, new float[][] { new float[] { 5, 9, 9, 5 }, new float[] { 9, 11, 11, 9 }, new float[] { 9, 11, 11, 9 }, new float[] { 5, 9, 9, 5 } });
            if (float.IsInfinity(eval))
                return eval;
            else
            { 
                var weights = (float [] ) parameters;
                var numberDifference = (float) Engine.NumberOfPossibleMovesDifference((Engine)state); 
                return numberDifference*weights[0] + eval*weights[1];
            }
        }

    }
}
