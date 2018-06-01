using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        }

        #region Properties and Members
        public bool Maximize { get; set; }

        public IReadOnlyList<EvaluatedIndividual> Population { get { return _Population; } }
        List<EvaluatedIndividual> _Population;

        public IReadOnlyList<EvaluatedIndividual> Champions { get { return _Champions; } }
        List<EvaluatedIndividual> _Champions;

        public NicheStrategy NicheStrat { get; set; }

        QuickTournament _Tournament;
        
        CrossOver _Crossover;
        private CompareEvaluate _Compare;
        private Evaluate _Eval;
        private Generate _Generate;
        private int _GenerationCount;
        private int _NumberOfGenerations;

        private int _PopSize;

        private Random _Rng;
        #endregion

        public void SetRandomSeed(int seed)
        {
            _Rng = new Random(seed);
        }

        public void GoThroughGenerations()
        {
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

            _GenerationCount += genCount;
        }

        private void Begin()
        {
            
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
                _Population.Select(ei => ei.Individual).ToList(), 
                _Compare, 
                new TournamentComplete( (pop) =>
                {
                    _Tournament = null;
                    _Population = pop
                        .Select(i => i.ToEI())
                        .ToList();
                }));
            _Tournament.Start();
        }

        private void NichePenalty()
        {
            if (NicheStrat == null)
                return;

            IReadOnlyList<float> penalties = NicheStrat.NichePenalties(_Population);

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
