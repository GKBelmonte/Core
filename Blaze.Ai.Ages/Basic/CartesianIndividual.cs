using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blaze.Core.Extensions;

namespace Blaze.Ai.Ages.Basic
{
    public class CartesianIndividual : IIndividual
    {
        public double[] Values { get; }

        public CartesianIndividual(Random r = null) : this(6, 6, r) { }

        public CartesianIndividual(int order, Random r = null, bool empty = false) : this(order, 6, r, empty) { }

        public CartesianIndividual(int order, int sigma, Random r, bool empty = false)
        {
            r = r ?? Utils.ThreadRandom;
            Name = IndividualTools.CreateName();
            Values = new double[order];
            if (empty)
                return;
            for (int i = 0; i < Values.Length; ++i)
            {
                Values[i] = r.GausianNoise(sigma);
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

        protected void Normalize()
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
            const double epsilon = 0.00001;
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

        public virtual IIndividual Mutate(float probability, float sigma, Random r)
        {
            var newInd = new CartesianIndividual(this);
            for (int i = 0; i < Values.Length; ++i)
            {
                if (r.ProbabilityPass(probability))
                    newInd.Values[i] += r.GausianNoise(sigma);
            }
            return newInd;
        }

        public static IIndividual CrossOver(List<IIndividual> parents, Random r)
        {
            var parent = (CartesianIndividual)parents[0];

            int newLength =
                parents
                .Select(i => ((CartesianIndividual)i).Values.Length)
                .ToList()
                .PickRandom();

            var newInd = new CartesianIndividual(newLength, r, empty: true);

            for (var ii = 0; ii < newLength; ++ii)
            {
                //33% chance of taking single random parent gene, 66% chance of taking average of all parents
                var dice = r.Next(0, 3);
                if (dice < 1)
                {
                    //Take average of parents respective alleles
                    double tot = 0.0f;
                    for (var kk = 0; kk < parents.Count; ++kk)
                    {
                        var par = (CartesianIndividual)parents[kk];
                        //TODO: is the absence of value zero?
                        if(ii < par.Values.Length)
                            tot += par.Values[ii];
                    }
                    //TODO: is the absence of value zero?
                    tot = tot / parents.Count;
                    newInd.Values[ii] = tot;
                }
                else //Take single parent gene randomly
                {
                    var par = (CartesianIndividual)parents[r.Next(0, parents.Count)];
                    //TODO: Is the absence of value zero?
                    newInd.Values[ii] = ii < par.Values.Length 
                        ? par.Values[ii] 
                        : 0;
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
            return string.Join("\t", Values.Select(f => f.ToString("0.000").PadRight(7)));
        }
    }
}
