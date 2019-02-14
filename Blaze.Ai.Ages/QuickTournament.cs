using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blaze.Ai.Ages
{
    public class QuickTournament
    {
        //What happens is that the Evaluator has two call-backs
        // evaluator: call this to tell the Fighter what to evaluate
        // master: call this to tell the overall algorithm that a full evaluation of the
        //      population was completed.
        //
        // Population evaluation resolves in a quick-sort tournament.
        // Pick a random individual and test it against every other.
        // The better get thrown in one group and the worse in another.
        // Repeat which each sub-group ( O(nlog(n) )

        //arr: Array to be sorted
        //evaluator: A function capable of evaluating A > B for the objects in arr
        //	It should take objects A, B and this object, and call-back the result
        //	with callback.Execute ( A - B ) or callback.Execute ( A > B )
        //master: A function to call to let the object owner that the total evaluation was completed 

        List<IIndividual> _individuals;
        int _begin;
        int _end;
        IIndividual _pivotIndividual;
        int _pivotIndex = 0;
        List<IIndividual> _betterIndividuals;
        List<IIndividual> _worseIndividuals;
        CompareEvaluate Compare;
        TournamentComplete OnCompleteCallBack;

        bool _done;
        bool _executing;

        public int CompCount { get; private set; }
        /// <summary>
        /// Number of draws. As the population reaches an optimum
        /// this will increase, since there will be stagnation
        /// </summary>
        public int DrawCount { get; private set; }

        int[] _histogram;


        private struct QuickSortPair
        {
            public int Begin;
            public int End;
            public QuickSortPair(int begin, int end)
            {
                Begin = begin;
                End = end;
            }
        }

        List<QuickSortPair> _queue;
        int _queueIndex;
        Random _Rng;

        public QuickTournament(
            IReadOnlyList<IIndividual> population, 
            CompareEvaluate comparer, 
            TournamentComplete callback,
            Random rng = null)
        {
            _Rng = rng ?? new Random();
            //std vals
            _individuals = population.ToList();
            _begin = 0;
            _end = _individuals.Count - 1;

            //piv vals
            var pivot = _Rng.Next(0, _individuals.Count);
            _pivotIndividual = _individuals[pivot];

            _swap(_individuals, _end, pivot);

            //changing vals
            _pivotIndex = 0;
            _betterIndividuals = new List<IIndividual>();
            _worseIndividuals = new List<IIndividual>();
            _queue = new List<QuickSortPair>();
            _queueIndex = 0;
            _queue.Add(new QuickSortPair(0, _end));

            //callbacks
            Compare = comparer; //Roughly speakin is A > B 
            OnCompleteCallBack = callback;

            //check var
            _done = false;

            _executing = false;
            CompCount = 0;
            DrawCount = 0;

            /*0-9,10-19,20-29,30-39,40-49,50-59, 60+*/
            _histogram = new int[] { 0, 0, 0, 0, 0, 0, 0 };
            Console.WriteLine("Current Pivot is " + _pivotIndividual.Name);
        }

        void Evaluate(float score)
        {
            if (!_done)
            {
                CompCount++;
                if (score > 0)
                {
                    _betterIndividuals.Add(_individuals[_pivotIndex]);
                }
                else
                {
                    _worseIndividuals.Add(_individuals[_pivotIndex]);

                    if (score == 0)
                        DrawCount++;
                }
                //scope to add stuff to histogram
                {
                    var posScore = (score > 0) ? score : -score;
                    var histInd = (int)Math.Floor(posScore / 10);
                    histInd = (histInd > 6 ? 6 : histInd);
                    _histogram[histInd] += 1;
                }
                _pivotIndex++;
                if (_pivotIndex >= _end)
                {
                    var pivPos = -1;
                    {//Reorder array
                     //Untouched first chunk
                        var Temp = new List<IIndividual>();
                        for (var ii = 0; ii < _begin; ++ii)
                        {
                            Temp.Add(_individuals[ii]);
                        }
                        //Worse than pivot
                        Temp.AddRange(_worseIndividuals);
                        pivPos = Temp.Count;
                        //Pivot
                        Temp.Add(_pivotIndividual);
                        //Better than pivot and the other untouched better chunk
                        Temp.AddRange(_betterIndividuals);
                        // Second untouched chunk
                        for (var ii = _end + 1; ii < _individuals.Count; ++ii)
                        {
                            Temp.Add(_individuals[ii]);
                        }

                        _individuals = Temp;
                    }
                    //Add execution queue if relevant (more than 1 individual left to fight)
                    if (_begin < pivPos - 1)
                        _queue.Add(new QuickSortPair(_begin, pivPos - 1));
                    if (pivPos + 1 < _end)
                        _queue.Add(new QuickSortPair(pivPos + 1, _end));


                    _queueIndex++;
                    if (_queueIndex < _queue.Count)
                    {
                        //Reset all values taking into consideration the queue
                        _begin = _queue[_queueIndex].Begin;
                        _end = _queue[_queueIndex].End;
                        var pivot = _Rng.Next(_begin, _end + 1);
                        _pivotIndividual = _individuals[pivot];
                        _swap(_individuals, _end, pivot);
                        _pivotIndex = _begin;
                        _betterIndividuals = new List<IIndividual>();
                        _worseIndividuals = new List<IIndividual>();
                    }
                    else
                    {
                        _done = true;
                    }

                }
                //Call-back evaluator to continue
                if (_done)
                {
                    _executing = false;
                    OnCompleteCallBack(_individuals);//Tell the master algorithm this the evaluation is complete. Send it the "sorted" array.
                }
            }
            else
                Console.WriteLine("Evaluation already complete");
        }

        public void Start()
        {
            _executing = true;
            while (!_done)
            {
                float score = Compare(_individuals[_pivotIndex], _pivotIndividual);
                Evaluate(score);
            }
        }

        //helper swap function.
        static void _swap(List<IIndividual> arr, int a, int b)
        {
            var t = arr[a];
            arr[a] = arr[b];
            arr[b] = t;
        }

    }//endclass

}
