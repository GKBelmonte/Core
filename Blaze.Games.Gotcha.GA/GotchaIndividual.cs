using Blaze.Ai;
using Blaze.Ai.Ages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Blaze.Games.Gotcha.GA
{
    class GotchaIndividual : IIndividual
    {
        string mName;
        float[][] mGradient;

        public GotchaIndividual()
        {
            mGradient = new float[4][];
            for (var ii = 0; ii < 4; ++ii)
            {
                mGradient[ii] = new float[4];
                for (var jj = 0; jj < 4; ++jj)
                {
                    mGradient[ii][jj] = (float)(Utils.GaussianNoise(6)+6);
                }
            }
            mName = IndividualTools.CreateName();
        }

        public GotchaIndividual(GotchaIndividual other)
        {
            mGradient = new float[4][];
            for (var ii = 0; ii < 4; ++ii)
            {
                mGradient[ii] = new float[4];
                for (var jj = 0; jj < 4; ++jj)
                {
                    mGradient[ii][jj] = other.mGradient[ii][jj];
                }
            }
            mName = IndividualTools.CreateName();
        }

        public Engine CreateInstanceEngine()
        { 
            return new Engine( 
                new AdversarialAi(
                    Engine.MoveProvider, 
                    Evaluators.FitnessEvaluatorLevel2,
                    3,
                    mGradient, 
                    Evaluators.Prioritizer));
        }

        public GotchaIndividual(string str)
        {
            mGradient = new float[4][];
            var name_vals_split = str.Split(':');
            this.mName = name_vals_split[0];
            var rows_split = name_vals_split[1].Split(';');
            for (var ii = 0; ii < 4; ++ii)
            {
                var valsSplit = rows_split[ii].Split(',');
                mGradient[ii] = new float[4];
                for (var jj = 0; jj < 4; ++jj)
                {
                    mGradient[ii][jj] = float.Parse(valsSplit[jj].Trim());
                }
            }
        }

        public IIndividual Mutate(float probability, float sigma, Random rng)
        {
            var newInd = new GotchaIndividual(this);
            for (var ii = 0; ii < 4; ++ii)
            {
                for (var jj = 0; jj < 4; ++jj)
                {
                    if (rng.ProbabilityPass(probability))
                        newInd.mGradient[ii][jj] = (float)(mGradient[ii][jj] + rng.GausianNoise(sigma));
                }
            }
            return newInd;
        }

        public string Name
        {
            get { return mName; }
        }

        static public CrossOver GarboCrossoverOperator { get { return new CrossOver(CrossOver); } }
        public static IIndividual CrossOver(List<IIndividual > parents, Random r)
        {
            var newInd = new GotchaIndividual();
            for (var ii = 0; ii < 4; ++ii)
            {
                for (var jj = 0; jj < 4; ++jj)
                {
                    //33% chance of taking single random parent gene, 66% chance of taking average of all parents
                    var dice = Utils.RandomInt(0, 3);
                    if (dice < 1)
                    {
                        //Take average of parents respective alleles
                        var tot = 0.0f;
                        for (var kk = 0; kk < parents.Count; ++kk)
                        {
                            var par = (GotchaIndividual)parents[kk];
                            tot += par.mGradient[ii][jj];
                        }
                        tot = tot / parents.Count;
                        newInd.mGradient[ii][jj] = tot;
                    }
                    else //Take single parent gene randomly
                    {
                        newInd.mGradient[ii][jj] = ((GotchaIndividual)parents[Utils.RandomInt(0, parents.Count)]).mGradient[ii][jj];
                    }
                }
            }

            return newInd;
        }

        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            for (var ii = 0; ii < 4; ++ii)
            {
                for (var jj = 0; jj < 4; ++jj)
                {
                    b.Append(mGradient[ii][jj]);
                    b.Append(" , ");
                }
                b.Append("  ;  ");
            }
            return b.ToString();
        }

    }
}
