﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blaze.Core.Maths;

namespace Blaze.Ai.Ages.Strats
{
    public abstract class NicheStrategy
    {
        public abstract IReadOnlyList<float> NichePenalties(IReadOnlyList<Ages.EvaluatedIndividual> population);
    }

    /// <summary>
    /// This strategy will increase the radius until the
    /// niche density is met, or likewise decrease it.
    /// Elements other than the best of the niche will be penalized.
    /// Elements in the elimination threshold will not be considered
    /// </summary>
    public class NicheDensityStrategy : NicheStrategy
    {
        private Distance _distance { get; }
        private Ages _parent { get;  }
        public float NicheRadius { get; private set; }

        /// <summary>
        /// This factor is applied to the radius if the radius is creating max number of niches
        /// </summary>
        private double _RadiusAdjustMaxFactor = 2;

        /// <summary>
        /// This factor is the reciprocal of _RadiusAdjustMaxFactor
        /// It is applied to the radius, if there's only one niche
        /// </summary>
        private double _RadiusAdjustMinFactor;

        /// <summary>
        /// This internal value is used to have a perfectly smooth curve
        /// the min/max factor requirements and if the number of niches == 
        /// the number of members per niche, it will be exactly one
        /// </summary>
        private double _RadiusAdjustPower;

        public NicheDensityStrategy(Ages parent, Distance distance)
        {
            _distance = distance;
            _parent = parent;
            _RadiusAdjustPower = 
                Math.Log(_RadiusAdjustMaxFactor * _RadiusAdjustMaxFactor) 
                / Math.Log(_parent.EliminateIndexBegin);

            _RadiusAdjustMinFactor = 1 / _RadiusAdjustMaxFactor;
        }

        public override IReadOnlyList<float> NichePenalties(IReadOnlyList<Ages.EvaluatedIndividual> population)
        {
            float[] penalties = new float[population.Count];

            float scoreStdDev = (float)population
                .Select(i => (double)i.Score.Value)
                .Take(_parent.EliminateIndexBegin)
                .ToList()
                .StdDevP();

            int elimIndex = _parent.EliminateIndexBegin;

            //punish all individuals similar to the best individual within a radius
            var niches = new List<Niche>(population.Count);

            for (int i = 0; i < elimIndex - 1; ++i)
            {
                int j = i + 1;
                var reference = population[i].Individual;

                double d = 0;
                var niche = new Niche(reference);

                //Punish all individuals too similiar to ith
                while (j < elimIndex && (d = _distance(reference, population[j].Individual)) <= NicheRadius)
                {
                    //At distance 1, the penalty is one-tenth a standard deviation
                    //At distance 0.316, the penalty is a whole-standard deviation
                    double partialPenalty = scoreStdDev / 10 / (d * d);
                    penalties[j] += (float)partialPenalty;
                    niche.Members.Add(population[j].Individual);
                    j++;
                }
                niche.MaxRadius = d;

                //punish everyone else in the niche against each other
                //The more you repeat the worse
                for (int k = i + 1; k < j - 1; ++k)
                {
                    for (int w = k + 1; w < j; ++w)
                    {
                        double d2 = _distance(population[k].Individual, population[w].Individual);
                        double partialPenalty = scoreStdDev / 10 / (d2 * d2);
                        penalties[w] += (float)partialPenalty;
                    }
                }

                // Either this individual was far or out-of bounds
                // Either way, I want to start from j next, 
                // (increment will happen right after)
                i = j - 1;
                niches.Add(niche);
            }

            // niche count = |distances|

            // If the number of distances is high either
            //  * My pop is diverse (Yay!)
            //  * My radius is too small

            // If the number of distances is small, either
            //  * My pop is super nichey
            //  * My radius is too large

            // Worst case on large radius, 1 distance, every is within the radius
            // Worst case on small radius, |pop| distances, everyone is their own niche
            // Obvious middle case, |pop|/2 distances, every pair is a niche (probably still small radius)
            // Probably the best then is to have sqrt(|pop|) niches with sqrt(|pop|) individs
            // This guarantees the most individuals and the most niches

            // if the number of niches is at max, we should half the radius
            // if the number of niches is at min, we should double it
            // if the number of niches is just right, not change it
            // So I want a f(n) such that
            //  f(1) = 2,
            //  f(sqrt(|pop|)) = 1
            //  f(|pop|) = 1/2
            // where f(n) is the factor to apply to the radius and n is the number of niches
            // By playing around, I found that f(n, p) = 1/2 * n ^( 1/ lg_4 (p) )
            //  f(n, p) = 1/2 * n ^ ( ln(p) / ln(2*2) )
            double nicheAverageDensity = niches.Average(n => n.Members.Count);

            double adjustFactor = _RadiusAdjustMinFactor * Math.Pow(nicheAverageDensity, _RadiusAdjustPower);
            NicheRadius = (float)(NicheRadius / adjustFactor);
            return penalties;
        }

        //Cheap cluster?
        private class Niche
        {
            public IIndividual Reference { get; }
            public List<IIndividual> Members { get; }
            public double MaxRadius { get; set; }
            public Niche(IIndividual referece)
            {
                Reference = referece;
                Members = new List<IIndividual>();
                Members.Add(Reference);
            }
        }
    }
}