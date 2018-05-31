using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Blaze.Core.Maths;

namespace Blaze.Ai.Ages
{
    /// <summary>
    /// Applied Genetic Evolutionary System
    /// </summary>
    public partial class Ages
    {
        public Ages(
            int numberOfGenerations, 
            CompareEvaluate compareTwo, 
            CrossOver crossoverOp, 
            Generate generator,
            List<IIndividual> population)
        {
            Init(numberOfGenerations,
                compareTwo,
                null,
                crossoverOp,
                generator,
                population);
        }

        public Ages(
            int numberOfGenerations, 
            CompareEvaluate compareTwo, 
            CrossOver crossoverOp,
            Generate generator,
            int popSize)
        {
            Init(numberOfGenerations,
                compareTwo,
                null,
                crossoverOp,
                generator,
                null);

            Sow(generator, popSize);
        }

        public Ages(
            int numberOfGenerations,
            Evaluate eval,
            CrossOver crossOverOp,
            Generate generator,
            List<IIndividual> individuals)
        {
            Init(numberOfGenerations, 
                null, 
                eval, 
                crossOverOp, 
                generator, 
                individuals);
        }

        public Ages(
            int numberOfGenerations,
            Evaluate eval,
            CrossOver crossOverOp,
            Generate generator,
            int popSize)
        {
            Init(numberOfGenerations, 
                null, 
                eval, 
                crossOverOp, 
                generator, 
                null);

            Sow(generator, popSize);
        }

        private void Init(
            int numberOfGenerations, 
            CompareEvaluate compareTwo, 
            Evaluate evaluate,
            CrossOver crossoverOp,
            Generate generator,
            List<IIndividual> population)
        {
            Maximize = false;
            EliminatedPercent = 0.2f;
            ElitismPercent = 0.1f;
            _NumberOfGenerations = numberOfGenerations;
            _Population = population
                .Select(i => i.ToEI())
                .ToList();
            _Tournament = null;
            _GenerationCount = 0;
            _Compare = compareTwo;
            _Eval = evaluate;
            _Generate = generator;
            _Crossover = crossoverOp;
            _Champions = new List<EvaluatedIndividual>(_NumberOfGenerations);
            VariationSettings = new VariationSettings(6, 4, 2);
        }

        #region Properties and Members
        public bool Maximize { get; set; }

        public IReadOnlyList<EvaluatedIndividual> Population { get { return _Population; } }
        List<EvaluatedIndividual> _Population;

        public IReadOnlyList<EvaluatedIndividual> Champions { get { return _Champions; } }
        List<EvaluatedIndividual> _Champions;

        /// <summary>
        /// If provided, will
        /// </summary>
        public Func<IIndividual, IIndividual, float> Distance { get; set; }

        QuickTournament _Tournament;
        
        CrossOver _Crossover;
        private CompareEvaluate _Compare;
        private Evaluate _Eval;
        private Generate _Generate;
        private int _GenerationCount;
        private int _NumberOfGenerations;

        private double _RadiusAdjustMaxFactor = 2;
        private double _RadiusAdjustMinFactor;
        private double _RadiusAdjustPower;
        private float _NicheRadius = 0.03f;
        #endregion

        public void GoThroughGenerationsSync()
        {
            Begin();
            while (_GenerationCount < _NumberOfGenerations)
            {
                Fight();

                NichePenalty();

                Reap();

                Survive();
            }
        }

        private void Begin()
        {
            _GenerationCount = 0;
            _RadiusAdjustPower = Math.Log(_RadiusAdjustMaxFactor*_RadiusAdjustMaxFactor) / Math.Log(EliminateIndexBegin);
            _RadiusAdjustMinFactor = 1 / _RadiusAdjustMaxFactor;
        }

        private void Fight()
        {
            if (_Compare != null)
                Tournament();
            else
                IndividualPerformance();
        }

        private void IndividualPerformance()
        {
            _Population.ForEach(ei => ei.Score = _Eval(ei.Individual));

            _Population = Maximize 
                ? _Population.OrderByDescending(i => i.Score).ToList()
                : _Population.OrderBy(i => i.Score).ToList();

            Console.WriteLine($"Generation {_GenerationCount} Complete");

            _GenerationCount += 1;
        }

        private void Sow(Generate generator, int popSize)
        {
            _Population = Enumerable
                .Range(0, popSize)
                .Select(i => generator().ToEI())
                .ToList();
        }

        //Executes one generation
        //FIX! This was done when in web i ran the generation in a web 
        // worker cause it took so long
        // Instead, use multiple threading 
        // (after the first pivot I can parallelize tournaments)
        //private void OneGenerationAsync()
        //{
        //    _Tournament = new QuickTournament(_Population, _Compare, new TournamentComplete(OnTournamentCompletion));
        //    _Tournament.Start();
        //}

        private void Tournament()
        {
            PrintGen();
            _Tournament = new QuickTournament(
                _Population.Select(ei => ei.Individual).ToList(), 
                _Compare, 
                new TournamentComplete( (pop) =>
                {
                    _GenerationCount += 1;
                    _Tournament = null;
                    _Population = pop
                        .Select(i => i.ToEI())
                        .ToList();
                }));
            _Tournament.Start();
        }

        private void NichePenalty()
        {
            if (Distance == null)
                return;

            float scoreStdDev = (float)_Population
                .Select(i => (double)i.Score.Value)
                .Take(EliminateIndexBegin)
                .ToList()
                .StdDevP();

            float[] penalties = new float[_Population.Count];

            int elimIndex = EliminateIndexBegin;

            //punish all individuals similar to the best individual within a radius
            var distances = new List<float>(_Population.Count);
            for (int i = 0; i < elimIndex - 1; ++i)
            {
                int j = i + 1;

                float d = 0;
                //Punish all individuals too similiar to ith
                while (j < elimIndex && (d = Distance(_Population[i].Individual, _Population[j].Individual)) <= _NicheRadius)
                {
                    //At distance 1, the penalty is one-tenth a standard deviation
                    //At distance 0.316, the penalty is a whole-standard deviation
                    float partialPenalty = scoreStdDev / 10 / (d * d);
                    penalties[j] += partialPenalty;
                    j++;
                }

                //punish everyone else in the niche against each other
                //The more you repeat the worse
                for (int k = i + 1; k < j - 1; ++k)
                {
                    for (int w = k + 1; w < j; ++w)
                    {
                        float d2 = Distance(_Population[k].Individual, _Population[w].Individual);
                        float partialPenalty = scoreStdDev / 10 / (d2 * d2);
                        penalties[w] += partialPenalty;
                    }
                }

                distances.Add(d);
                // Either this individual was far or out-of bounds
                // Either way, I want to start from j next, 
                // (increment will happen right after)
                i = j - 1;
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
            double actualNicheCount = distances.Count;

            //double adjustFactor = _RadiusAdjustMinFactor * Math.Pow(actualNicheCount, _RadiusAdjustPower);
            //_NicheRadius = (float)(_NicheRadius * adjustFactor);

            if (Maximize)
            {
                for (int i = 0; i < _Population.Count; ++i)
                    _Population[i].Score -= penalties[i];

                _Population = _Population
                    .OrderByDescending(ei => ei.Score)
                    .ToList();
            }
            else
            {
                for (int i = 0; i < _Population.Count; ++i)
                    _Population[i].Score += penalties[i];

                _Population = _Population
                    .OrderBy(ei => ei.Score)
                    .ToList();
            }
        }

        private void Survive()
        {
            _Champions.Add(_Population[0]);
        }

        private void PrintGen()
        {
            StringBuilder gen = new StringBuilder();
            foreach (var ind in _Population)
            {
                gen.AppendLine(string.Format("{0}:{1}", ind.Individual.Name, ind.Individual.ToString()));
            }

            System.IO.File.WriteAllText(string.Format("Generation{0}.txt", _GenerationCount), gen.ToString());
        }

        public class EvaluatedIndividual
        {
            public IIndividual Individual;
            public float? Score;
            public override string ToString()
            {
                return $"Individual: {Individual}, Score: {Score}";
            }
        }
    }
}
