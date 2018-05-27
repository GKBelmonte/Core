using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

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
            _Population = new List<IIndividual>(popSize);
            throw new NotImplementedException("Creating infered types not implemented");
            //for(var ii = 0; ii < popSize; ++ii)
            //{
            // // mPopulation.Add(generateThisType.);
            //}
        }

        public Ages(
            int numberOfGenerations,
            Evaluate eval,
            CrossOver crossOverOp,
            Generate generator,
            List<IIndividual> individuals)
        {
            Init(numberOfGenerations, null, eval, crossOverOp, generator, individuals);
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
            _Population = population;
            _Tournament = null;
            _GenerationCount = 0;
            _Compare = compareTwo;
            _Eval = evaluate;
            _Generate = generator;
            _Crossover = crossoverOp;
            _Async = false;
            _Champions = new List<IIndividual>(_NumberOfGenerations);
            VariationSettings = new VariationSettings(6, 4, 2);
        }

        #region Properties and Members
        public IReadOnlyList<IIndividual> Population { get { return _Population; } }
        List<IIndividual> _Population;

        public IReadOnlyList<IIndividual> Champions { get { return _Champions; } }
        List<IIndividual> _Champions;

        public bool Maximize { get; set; }

        QuickTournament _Tournament;
        
        CrossOver _Crossover;
        private CompareEvaluate _Compare;
        private Evaluate _Eval;
        private Generate _Generate;
        private int _GenerationCount;
        private int _NumberOfGenerations;
        private bool _Async;

        #endregion

        public void GoThroughGenerationsSync()
        {
            _GenerationCount = 0;
            while (_GenerationCount < _NumberOfGenerations)
            {
                if (_Compare != null)
                    TournamentGeneration();
                else
                    IndividualPerformanceGeneration();
                Reap();
                _Champions.Add(_Population[0]);
            }
        }

        private void IndividualPerformanceGeneration()
        {
            var ratedIndividuals = _Population
                .Select(i => new { Score = _Eval(i), Individual = i })
                .ToList();

            var orderedIndividuals = Maximize 
                ? ratedIndividuals.OrderByDescending(i => i.Score).ToList()
                : ratedIndividuals.OrderBy(i => i.Score).ToList();

            Console.WriteLine($"Generation {_GenerationCount} Complete");

            _GenerationCount += 1;
            _Population = orderedIndividuals
                .Select(i => i.Individual)
                .ToList();
            
        }

        //Executes one generation
        private void OneGenerationAsync()
        {
            _Tournament = new QuickTournament(_Population, _Compare, new TournamentComplete(OnTournamentCompletion));
            _Tournament.Start();
        }

        private void TournamentGeneration()
        {
            PrintGen();
            _Tournament = new QuickTournament(
                _Population, 
                _Compare, 
                new TournamentComplete(OnTournamentCompletion));
            _Tournament.Start();
        }

        private void OnTournamentCompletion(List<IIndividual> population)
        {
            _GenerationCount += 1;
            _Population = population;
            Console.WriteLine("Generation Complete");
            Console.WriteLine("Number of matches: " + _Tournament.CompCount);
            Console.WriteLine("Number of Draws: " + _Tournament.DrawCount);

            _Tournament = null;//kill the old evaluator

            //Reap
            Reap();

            if (_Async)
            {
                if (_GenerationCount <= _NumberOfGenerations)
                {
                    Thread t = new Thread(new ThreadStart(OneGenerationAsync));
                    t.Start();
                }
                else
                {
                    Console.WriteLine(_GenerationCount + " generations completed");
                }
            }
        }

        private void PrintGen()
        {
            StringBuilder gen = new StringBuilder();
            foreach (var ind in _Population)
            {
                gen.AppendLine(string.Format("{0}:{1}", ind.Name, ind.ToString()));
            }
            //Console.WriteLine(gen.ToString());
            System.IO.File.WriteAllText(string.Format("Generation{0}.txt", _GenerationCount), gen.ToString());
        }

    }
}
