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

        private int EliminateIndexBegin => (int)Math.Floor((_Population.Count * (1 - EliminatedPercent)));

        private int EliteIndexEnd => (int)Math.Floor((_Population.Count * ElitismPercent));

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
                int dice = Utils.RandomInt(0, 12);
                if (dice < VariationSettings.SurvivalRatio)
                {
                    newPop.Add(pop[ii]); //Individual survived by chance
                }
                else if (dice < VariationSettings.SurvivalRatio + VariationSettings.MutationRatio)
                {
                    //Mutated
                    IIndividual newInd = pop[Utils.RandomInt(0, EliminateIndexBegin)]
                        .Individual
                        .Mutate(MutationProbability, 5f);
                    newPop.Add(newInd.ToEI());
                }
                else
                {
                    //Crossover
                    var parents = new List<IIndividual>(5);
                    var numOfParents = Utils.RandomInt(2, 5); //pick 2,3 or 4 parents
                    while (parents.Count < numOfParents)
                    {
                        //Pick a random surviving parent
                        var index = Utils.RandomInt(0, EliminateIndexBegin);
                        //Select him probabilistically based on rank (index)
                        float prob = ((float)(pop.Count - index)) / pop.Count;//Probability
                        if (Utils.ProbabilityPass(prob))
                        {
                            parents.Add(pop[index].Individual);
                        }
                    }
                    //Add the crossed individual
                    newPop.Add(_Crossover(parents).ToEI());
                }
            }

            //Fresh individuals for the failing percent
            for (var ii = EliminateIndexBegin; ii < pop.Count; ++ii)
            {
                if(Utils.RandomInt(0,2) == 0)
                    newPop.Add(_Generate().ToEI());
                else
                    newPop.Add(
                        pop[Utils.RandomInt(0,EliteIndexEnd)]
                            .Individual
                            .Mutate(MutationProbability,5f)
                            .ToEI());
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
