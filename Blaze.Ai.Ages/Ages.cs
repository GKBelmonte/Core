using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Blaze.Ai.Ages.Strats;
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
            _PopSize = popSize;
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
            _PopSize = popSize;
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
            if (population != null)
            {
                _Population = population
                    .Select(i => i.ToEI())
                    .ToList();
                _PopSize = population.Count;
            }
            _Tournament = null;
            _GenerationCount = 0;
            _Compare = compareTwo;
            _Eval = evaluate;
            _Generate = generator;
            _Crossover = crossoverOp;
            _Champions = new List<EvaluatedIndividual>(_NumberOfGenerations);
            VariationSettings = new VariationSettings(6, 4, 2);
            _Rng = new Random();
            _mutationProbability = 0.6f;
            _mutationSigma = 1;
            Threads = 4;
        }

        #region Properties and Members
        public bool Maximize { get; set; }

        public IReadOnlyList<EvaluatedIndividual> Population { get { return _Population; } }
        List<EvaluatedIndividual> _Population;

        public IReadOnlyList<EvaluatedIndividual> Champions { get { return _Champions; } }
        List<EvaluatedIndividual> _Champions;

        public NicheStrategy NicheStrat { get; set; }

        public int Threads { get; set; }

        public int GenerationCount { get { return _GenerationCount; } }

        QuickTournament _Tournament;
        
        CrossOver _Crossover;
        private CompareEvaluate _Compare;
        private Evaluate _Eval;
        private Generate _Generate;
        private int _GenerationCount;
        private int _NumberOfGenerations;
        private int _PopSize;
        private int _Threads;
        private Random _Rng;
        #endregion

        public void SetRandomSeed(int seed)
        {
            _Rng = new Random(seed);
        }

        public void GoThroughGenerations()
        {
            //Cheap thread safety 
            Sow();

            Begin();

            int genCount = 0;
            while (genCount++ < _NumberOfGenerations)
            {
                Fight();

                NichePenalty();

                Reap();

                Survive();
            }
            
            //-1 because off by one error
            _GenerationCount += genCount - 1;
        }

        private void Begin()
        {
            _Threads = Threads;
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
            Action<EvaluatedIndividual> cheapEval =
                ei => { if (!ei.Score.HasValue) { ei.Score = _Eval(ei.Individual); } };

            if (_Threads == 1)
            {
                _Population.ForEach(cheapEval);
            }
            else
            {
                Parallel.ForEach(
                    _Population, 
                    new ParallelOptions{  MaxDegreeOfParallelism = _Threads },
                    cheapEval);
            }

            //Population is Ordered so that the first index is the best individual
            _Population.Sort((l,r) => Compare(l,r, Maximize));


            //Normalize score
            float min = Maximize ? _Population.Last().Score.Value : _Population.First().Score.Value;
            float max = Maximize ? _Population.First().Score.Value : _Population.Last().Score.Value;

            _Population.ForEach(ei => ei.NormalizedScore = (ei.Score - min) / (max - min));
            if (!Maximize)
                _Population.ForEach(ei => ei.NormalizedScore = 1 - ei.NormalizedScore);
            Console.WriteLine($"Generation {_GenerationCount} Complete");
        }

        private void Sow()
        {
            if (_Population != null)
                return;
            _Population = Enumerable
                .Range(0, _PopSize)
                .Select(i => _Generate(_Rng).ToEI())
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
                population: _Population.Select(ei => ei.Individual).ToList(), 
                comparer: _Compare, 
                callback: new TournamentComplete( (pop) =>
                    {
                        _Tournament = null;
                        _Population = pop
                            .Select(i => i.ToEI())
                            .ToList();
                    }),
                rng: _Rng);
            _Tournament.Start();
        }

        private void NichePenalty()
        {
            if (NicheStrat == null)
                return;

            IReadOnlyList<float> penalties = NicheStrat.NichePenalties(_Population);
            //TODO: check if NicheStrat applies to normalized score?

            for (int i = 0; i < _Population.Count; ++i)
                _Population[i].NormalizedScore -= penalties[i];

            _Population.Sort((l, e) => CompareNormalized(l, e));
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

        private static int Compare(
            EvaluatedIndividual left,
            EvaluatedIndividual right,
            bool maximizing)
        {
            bool leftGreater = left.Score > right.Score;
            if (leftGreater)
            {
                if (maximizing)
                    return -1;
                return 1;
            }
            else if (left.Score == right.Score)
            {
                return 0;
            }

            if (maximizing)
                return 1;
            return -1;
        }

        private static int CompareNormalized(
            EvaluatedIndividual left,
            EvaluatedIndividual right)
        {
            bool leftGreater = left.NormalizedScore > right.NormalizedScore;
            if (leftGreater)
                return -1;
            
            else if (left.NormalizedScore == right.NormalizedScore)
                return 0;

            return 1;
        }

        public class EvaluatedIndividual
        {
            public IIndividual Individual;
            public float? Score;
            // Always between 0 and 1
            // Best score is always 1
            public float? NormalizedScore;
            public override string ToString()
            {
                return $"Score: {Score}, Norm: {NormalizedScore}, Individual: {Individual}";
            }
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
