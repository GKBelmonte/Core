using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze.Ai.Ages
{
    public partial class Ages
    {
        private float _eliminatedPercent;
        public float EliminatedPercent
        {
            get { return _eliminatedPercent; }
            set
            {
                const float min = 0, max = 0.9375f;
                if (value < min || value > max)
                    throw new ArgumentOutOfRangeException($"Value must be within {min} and {max}");
                if (1f < ElitismPercent + value)
                    throw new ArgumentOutOfRangeException($"Can't eliminate {value * 100}% and simultaneously keep {ElitismPercent}");
                _eliminatedPercent = value;
            }
        }

        private float _elitismPercent;
        public float ElitismPercent
        {
            get { return _elitismPercent; }
            set
            {
                const float min = 0, max = 0.9375f;
                if (value < min || value > max)
                    throw new ArgumentOutOfRangeException($"Value must be within {min} and {max}");
                if (1f < EliminatedPercent + value)
                    throw new ArgumentOutOfRangeException($"Can't keep {value * 100}% and simultaneously eliminate {EliminatedPercent}");
                _elitismPercent = value;
            }
        }

        // E.G.: Elitism=Elimintaion=25%
        //                              EliminateIndexBeg (6)
        //                  EliteIndexEnd (1)
        //                               ⇓                   ⇓
        // Population (Maximizing): [23, 17, 13, 11, 7 , 3 , 2, 1]

        /// <summary>
        /// The first index of the population that will be eliminated no matter what
        /// </summary>
        public int EliminateIndexBegin => (int)Math.Floor((_Population.Count * (1 - EliminatedPercent)));

        /// <summary>
        /// The last index of the population that will be kept no matter what
        /// </summary>
        public int EliteIndexEnd => (int)Math.Floor((_Population.Count * ElitismPercent));

        private float _mutationProbability;
        public float MutationProbability
        {
            get { return _mutationProbability; }
            set
            {
                const float min = 0, max = 1f;
                if (value < min || value > max)
                    throw new ArgumentOutOfRangeException($"Value must be within {min} and {max}");
                _mutationProbability = value;
            }
        }

        private float _mutationSigma;
        public float MutationSigma
        {
            get { return _mutationSigma; }
            set
            {
                const float min = 0, max = float.MaxValue;
                if (value < min || value > max)
                    throw new ArgumentOutOfRangeException($"Value must be within {min} and {max}");
                _mutationSigma = value;
            }
        }

        public VariationSettings VariationSettings { get; set; }

        //Epic name for a function.
        //Kills, replaces and chooses who mutates or crossovers
        private void Reap()
        {
            var pop = _Population;
            var newPop = new List<EvaluatedIndividual>(pop.Count);

            //Elite:
            for (var ii = 0; ii < EliteIndexEnd; ++ii)
            {
                newPop.Add(pop[ii]);
            }

            //Changed individuals
            for (int ii = EliteIndexEnd; ii < EliminateIndexBegin; ++ii)
            {
                int dice = _Rng.Next(0, 12);
                if (dice < VariationSettings.SurvivalRatio)
                {
                    newPop.Add(pop[ii]); //Individual survived by chance
                }
                else if (dice < VariationSettings.SurvivalRatio + VariationSettings.MutationRatio)
                {
                    //Mutated
                    int mutatedIndex = _Rng.Next(0, EliminateIndexBegin);
                    IIndividual newInd = pop[mutatedIndex]
                        .Individual
                        .Mutate(MutationProbability, _mutationSigma, _Rng);
                    newPop.Add(newInd.ToEI());
                }
                else
                {
                    //Crossover
                    var parents = new List<IIndividual>(5);
                    var numOfParents = _Rng.Next(2, 5); //pick 2,3 or 4 parents
                    while (parents.Count < numOfParents)
                    {
                        //Pick a random surviving parent
                        var index = _Rng.Next(0, EliminateIndexBegin);
                        //Select him probabilistically based on rank (index)
                        float prob = ((float)(pop.Count - index)) / pop.Count;//Probability
                        if (_Rng.ProbabilityPass(prob))
                        {
                            parents.Add(pop[index].Individual);
                        }
                    }
                    //Add the crossed individual
                    newPop.Add(_Crossover(parents, _Rng).ToEI());
                }
            }

            //Fresh individuals for the failing percent
            for (var ii = EliminateIndexBegin; ii < pop.Count; ++ii)
            {
                EvaluatedIndividual ei;
                if (_Rng.Next(0, 2) == 0)
                {
                    ei = _Generate(_Rng).ToEI();
                }
                else
                {
                    int index = _Rng.Next(0, EliteIndexEnd);
                    ei =
                        pop[index]
                            .Individual
                            .Mutate(MutationProbability, _mutationSigma, _Rng)
                            .ToEI();
                }
                newPop.Add(ei);
            }

            _Population = newPop;
        }
    }

    public struct VariationSettings
    {
        internal const int RatioTotal = 12;
        public VariationSettings(
            int mutationRatio,
            int crossoverRatio,
            int survivalRatio)
        {
            if (mutationRatio + crossoverRatio + survivalRatio != 12)
                throw new ArgumentOutOfRangeException($"The sum of the rations must be {RatioTotal}");
            MutationRatio = mutationRatio;
            SurvivalRatio = survivalRatio;
            CrossOverRatio = crossoverRatio;
        }

        public int MutationRatio { get; }

        public int SurvivalRatio { get; }

        public int CrossOverRatio { get; }
    }
}
