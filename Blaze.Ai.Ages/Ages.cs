using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Ages
{

    public class Ages
    {
        public Ages(int numberOfGenerations,Evaluate compareTwo ,CrossOver crossoverOp, List<IIndividual> population )
        {
          mNumberOfGenerations = numberOfGenerations;
	        mPopulation = population;
	        mTour = null;
          mGenerationCount = 0;
          mComparator = compareTwo;
          mCrossover = crossoverOp;
          mAsync = false;
        }

        public Ages(int numberOfGenerations , Evaluate compareTwo, CrossOver crossoverOp,Type generateThisType, int popSize)
        {
	        mPopulation = new List<IIndividual>(popSize);
          throw new NotImplementedException("Creating infered types not implemented");
          //for(var ii = 0; ii < popSize; ++ii)
          //{
          // // mPopulation.Add(generateThisType.);
          //}

        }


        public void GoThroughGenerationsSync()
        {
            while (mGenerationCount <= mNumberOfGenerations)
                OneGenerationSync();
        }

        //Epic name for a function.
        //Kills, replaces and chooses who mutates or crossovers
        public void Reap()
        {
	        var pop = mPopulation ;//for quick access
	        var newPop = new List<IIndividual>(pop.Count);
	        var fail =(int) Math.Floor((pop.Count *0.2));//TODO: Remove magic numbers
          var change = (int)Math.Floor((pop.Count * 0.8));
	
	        //Fresh individuals for the failing percend
	        for(var ii = 0; ii < fail; ++ii)
	        {
		        pop[ii].Regenerate();
		        newPop.Add(pop[ii]);
	        }
	
	        //Changed individuals
	        for(var ii = fail; ii < change; ++ii)
	        {
		        var dice = Utils.RandomInt(0,6);//6 not included
		        if(dice == 0)
		        {
			          newPop.Add(pop[ii]); //Individual survived by chance
		        }
		        else if(dice == 1 ||  dice == 2)
		        {
                //Mutated
			          newPop.Add(pop[Utils.RandomInt(fail,pop.Count)].Mutate(0.4f,1.0f));
		        }
		        else
		        {
			        //Crossover
			        var parents = new List<IIndividual>(5);
			        var numOfP = Utils.RandomInt(2,5); //pick 2,3 or 4 parents
              while (parents.Count < numOfP)
			        {
                //Pick a random surviving parent
				        var index = Utils.RandomInt(fail,pop.Count);
				        //Select him probabilistically based on rank (index)
                var prob = 100 * index / pop.Count;//Probability
				        if(Utils.RandomInt(0,101) <= prob )
				        {
					        parents.Add(pop[index]);
				        }
			        }
                //Add the crossed individual
			        newPop.Add(mCrossover(parents));
		        }
	        }

	        //Elite:
          for (var ii = change; ii < pop.Count; ++ii)
	        {
		        newPop.Add(pop[ii]);
	        }
	        mPopulation = newPop;
        }


        void OnCompletion(List<IIndividual> population)
        {
	        
          mGenerationCount += 1;
          mPopulation = population;
          Console.WriteLine("Generation Complete");
          Console.WriteLine("Number of matches: " + mTour.CompCount);
          Console.WriteLine("Number of Draws: " + mTour.DrawCount);

	        mTour = null;//kill the old evaluator

	
	        //Reap
	        Reap();
          if (mAsync)
          {
              if (mGenerationCount <= mNumberOfGenerations)
              {

                  Thread t = new Thread(new ThreadStart(OneGenerationAsync));
                  t.Start();

              }
              else
                  Console.WriteLine(mGenerationCount + " generations completed");
          }
        }

        //Executes one generation
        void OneGenerationAsync()
        {
            mTour = new QuickTournament(mPopulation, mComparator, new OnTournamentComplete(OnCompletion));
            mTour.Start();
        }

        void OneGenerationSync()
        {
            PrintGen();
            mTour = new QuickTournament(mPopulation, mComparator, new OnTournamentComplete(OnCompletion));
            mTour.Start();
        }

        void PrintGen()
        {
            StringBuilder gen = new StringBuilder();
            foreach (var ind in mPopulation)
            {
                gen.AppendLine(string.Format("{0}:{1}", ind.Name,ind.ToString()));
            }
            System.IO.File.WriteAllText(string.Format("Generation{0}.txt", mGenerationCount), gen.ToString());
        }

        #region Members
        QuickTournament mTour;
        List<IIndividual> mPopulation;
        CrossOver mCrossover;
        private Evaluate mComparator;
        private int mGenerationCount;
        private int mNumberOfGenerations;
        private bool mAsync;
        #endregion



    }

}
