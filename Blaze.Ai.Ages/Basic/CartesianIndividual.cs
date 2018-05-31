using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Ai.Ages.Basic
{
    public class CartesianIndividual : IIndividual
    {
        public double[] Values { get; }

        public CartesianIndividual() : this(6,6) { }

        public CartesianIndividual(int order) : this(order, 6) { }

        public CartesianIndividual(int order, int sigma)
        {
            Name = IndividualTools.CreateName();
            Values = new double[order];
            for (int i = 0; i < Values.Length; ++i)
            {
                Values[i] = Utils.GausianNoise(sigma);
            }
        }

        public CartesianIndividual(CartesianIndividual other)
        {
            Name = IndividualTools.CreateName();
            Values = new double[other.Values.Length];
            for (int i = 0; i < Values.Length; ++i)
                Values[i] = other.Values[i];
        }

        public string Name { get; }

        public float? Score { get; set; }

        private void Normalize()
        {
            double min = Values[0], max = Values[0];
            for (int i = 1; i < Values.Length; ++i)
            {
                if (Values[i] > max)
                    max = Values[i];
                if (Values[i] < min)
                    min = Values[i];
            }

            double diff = max - min;
            double epsilon = 0.00001;
            if (Math.Abs(diff - epsilon) < epsilon)
            {
                diff = 1;
            }

            for (int i = 0; i < Values.Length; ++i)
            {
                double f = Values[i];
                Values[i] = (f - min) / diff;
            }
        }

        public IIndividual Mutate(float probability, float sigma)
        {
            var newInd = new CartesianIndividual(this);
            for (int i = 0; i < Values.Length; ++i)
            {
                if (Utils.ProbabilityPass(probability))
                    newInd.Values[i] += Utils.GausianNoise(sigma);
            }
            return newInd;
        }

        public static IIndividual CrossOver(List<IIndividual> parents)
        {
            var parent = (CartesianIndividual)parents[0];
            var newInd = new CartesianIndividual(parent.Values.Length);
            for (var ii = 0; ii < newInd.Values.Length; ++ii)
            {
                //33% chance of taking single random parent gene, 66% chance of taking average of all parents
                var dice = Utils.RandomInt(0, 3);
                if (dice < 1)
                {
                    //Take average of parents respective alleles
                    double tot = 0.0f;
                    for (var kk = 0; kk < parents.Count; ++kk)
                    {
                        var par = (CartesianIndividual)parents[kk];
                        tot += par.Values[ii];
                    }
                    tot = tot / parents.Count;
                    newInd.Values[ii] = tot;
                }
                else //Take single parent gene randomly
                {
                    var par = (CartesianIndividual)parents[Utils.RandomInt(0, parents.Count)];
                    newInd.Values[ii] = par.Values[ii];
                }
            }

            return newInd;
        }


        public static float Distance(CartesianIndividual a, CartesianIndividual b)
        {
            double res = 0;
            for (int i = 0; i < a.Values.Length && i < b.Values.Length; ++i)
            {
                double a_i = i < a.Values.Length ? a.Values[i] : 0;
                double b_i = i < b.Values.Length ? b.Values[i] : 0;
                double diff = b_i - a_i;
                res += diff * diff;
            }
            //not technically mandatory
            res = Math.Sqrt(res);
            return (float)res;
        }

        public override string ToString()
        {
            return string.Join("\t", Values.Select(f => f.ToString("0.000")));
        }
    }
}
