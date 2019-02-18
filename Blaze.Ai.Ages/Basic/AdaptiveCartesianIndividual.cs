using Blaze.Core.Extensions;
using Blaze.Core.Maths;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Ai.Ages.Basic
{
    public class AdaptiveCartesianIndividual : CartesianIndividual
    {
        /// <summary>
        /// The mutation range of the mutation parameters
        /// 0.3 makes it so that most of the time the factor applied to sigma
        /// will be betwee 0.6 and 1.4
        /// </summary>
        public static double Tau { get; set; } = 0.3;
        public static double DefaultSigma { get; set; } = 1;
        /// <summary>
        /// Increase the number to make mutation probability change more.
        /// Decrease the number to make mutation probability change less
        /// The bigger this number the less the probability of mutation changes
        /// By default, the probability of mutation will change between -0.1 and 0.1
        /// The range will be (0.5 / MPM) (e.g. for 5 => [-0.1, 0.1])
        /// (e.g. for 10 => [-0.05, 0.05])
        /// </summary>
        public static double MutationProbabilityMutation { get; set; } = 5;

        public double[] Sigmas { get; }
        public double MutationProbability { get; set; }
        /// <summary>
        /// The higher this probability, the more likely to take the average
        /// during crossover instead of a single gene
        /// </summary>
        public double AverageCrossOverProbabilty { get; set; }

        public AdaptiveCartesianIndividual(AdaptiveCartesianIndividual other) : base(other)
        {
            Sigmas = other.Sigmas.ToArray();
            MutationProbability = 0.6;
            AverageCrossOverProbabilty = 0.66;
        }

        public AdaptiveCartesianIndividual(
            int order, 
            int sigma = 6, 
            Random r = null, 
            bool empty = false,
            //This parameter makes it so that the rng generation
            // is the same for normal and adaptive individuals.
            // This permits us testing against normal individuals (same initial conditions)
            bool emptySigma = false) 
            : base(order, sigma, r, empty)
        {
            r = r ?? Utils.ThreadRandom;
            Sigmas = Enumerable
                .Range(0, order)
                .Select(i =>
                    empty || emptySigma
                    ? DefaultSigma
                    : r.GausianNoise(Tau) + DefaultSigma)
                .ToArray();
            MutationProbability = 0.6;
            AverageCrossOverProbabilty = 0.66;
        }

        public override IIndividual Mutate(float probability, float sigma, Random r)
        {
            var newInd = new AdaptiveCartesianIndividual(this);
            //Mutate mutation (add a number between (-0.1,0.1) )
            //Fucked up side effect of not setting the probability to the child but the parent instead.
            // The children had worse values as sigmas skyroketted, keeping the parents alive even longer
            // The parents were making they're children worse off so they could keep surviving :/

            newInd.MutationProbability = (MutationProbability + (r.NextDouble() - 0.5) / 5).Clamp(0, 1);
            newInd.AverageCrossOverProbabilty = (AverageCrossOverProbabilty + (r.NextDouble() - 0.5) / 5).Clamp(0, 1);

            //Mutate sigmas based on Tau
            for (int i = 0; i < Sigmas.Length; ++i)
            {
                if (r.ProbabilityPass((float)newInd.MutationProbability))
                {
                    double modFactor = r.GausianNoise(Tau) + 1;
                    newInd.Sigmas[i] = newInd.Sigmas[i] * modFactor;
                }
            }

            //Mutate actual values based on Sigmas
            for (int i = 0; i < Values.Length; ++i)
            {
                if (r.ProbabilityPass((float)newInd.MutationProbability))
                    //Here we were using the parent's sigmas... 
                    newInd.Values[i] += r.GausianNoise(newInd.Sigmas[i]);
            }

            return newInd;
        }

        public static IIndividual CrossOver(List<IIndividual> parents, Random r)
        {
            var typedParents = parents.OfType<AdaptiveCartesianIndividual>().ToList();
            int newLength =
                typedParents
                .PickRandom(r)
                .Values
                .Length;

            float aveCrossOverProb = (float)typedParents.Select(i => i.AverageCrossOverProbabilty).Average();

            var newInd = new AdaptiveCartesianIndividual(newLength, r: r, empty: true);
            bool takeAverage = false;

            //Go through the values and the sigma
            for (var ii = 0; ii < newLength; ++ii)
            {
                takeAverage = r.ProbabilityPass(aveCrossOverProb);
                if (takeAverage)
                {
                    //Take average of parents respective alleles
                    double valueTot = 0.0;
                    double sigmaTot = 0.0;
                    for (var kk = 0; kk < parents.Count; ++kk)
                    {
                        AdaptiveCartesianIndividual par = typedParents[kk];
                        //TODO: is the absence of value zero?
                        if (ii < par.Values.Length)
                        {
                            valueTot += par.Values[ii];
                            sigmaTot += par.Sigmas[ii];
                        }
                    }
                    //TODO: is the absence of value zero?
                    newInd.Values[ii] = valueTot / parents.Count;
                    newInd.Sigmas[ii] = sigmaTot / parents.Count;
                }
                else //Take single parent gene randomly
                {
                    AdaptiveCartesianIndividual par = typedParents[r.Next(0, typedParents.Count)];
                    //TODO: Is the absence of value zero?
                    newInd.Values[ii] = ii < par.Values.Length
                        ? par.Values[ii]
                        : 0;

                    newInd.Sigmas[ii] = ii < par.Sigmas.Length
                        ? par.Sigmas[ii]
                        : DefaultSigma;
                }
            }

            if (r.ProbabilityPass(aveCrossOverProb))
                newInd.MutationProbability = typedParents.Select(i => i.MutationProbability).Average();
            else
                newInd.MutationProbability = typedParents.PickRandom(r).MutationProbability;

            if (r.ProbabilityPass(aveCrossOverProb))
                newInd.AverageCrossOverProbabilty = typedParents.Select(i => i.AverageCrossOverProbabilty).Average();
            else
                newInd.AverageCrossOverProbabilty = typedParents.PickRandom(r).AverageCrossOverProbabilty;

            return newInd;
        }
    }
}
