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
        #endregion

        public void GoThroughGenerationsSync()
        {
            _GenerationCount = 0;
            while (_GenerationCount < _NumberOfGenerations)
            {
                Compete();

                NichePenalty();

                Reap();

                Survive();
            }
        }

        private void Compete()
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

            for (int i = 0; i < elimIndex - 1; ++i)
            {
                for(int j = i + 1; j < elimIndex; ++j)
                {
                    if (i == j)
                        continue;
                    float d = Distance(_Population[i].Individual, _Population[j].Individual);

                    //Making the penalty proportional to your rank can
                    // encourage newer geners, but it might
                    // also hurt the best genes a lot.
                    //  The better you are, the worse the penalty of being close 
                    //      float partialPenaltyFromI = (_Population.Count - i) / (d * d);
                    //      float partialPenaltyFromJ = (_Population.Count - j) / (d * d);
                    
                    //At distance 1, the penalty is one-tenth a standard deviation
                    //At distance 0.316, the penalty is a whole-standard deviation
                    float partialPenalty = scoreStdDev / 10 / _Population.Count / (d * d);
                    penalties[j] += partialPenalty;
                    penalties[i] += partialPenalty;
                }
            }

            // should we exclude the champ??
            // penalties[0] = 0;

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
