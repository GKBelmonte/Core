using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ages
{
    class QuickTournament
    {
        //What happens is that the Evaluator has two call-backs
        // evaluator: call this to tell the Fighter what to evaluate
        // master: call this to tell the overall algorithm that a full evaluation of the
        // 			population was completed.
        //	
        //	Population evaluation resolves in a quick-sort tournament.
        // Pick a random individual and test it against every other.
        // The better get thrown in one group and the worse in another.
        // Repeat which each sub-group ( O(nlog(n) )

        //arr: Array to be sorted
        //evaluator: A function capable of evaluating A > B for the objects in arr
        //	It should take objects A, B and this object, and call-back the result
        //	with callback.Execute ( A - B ) or callback.Execute ( A > B )
        //master: A function to call to let the object owner that the total evaluation was completed 

        List<IIndividual> mArr;
        int begin ;
        int end ;
        IIndividual pivotVal ;
        int pointing = 0;
        List<IIndividual> ArrBetter ;
        List<IIndividual> ArrWorse ;
        Evaluate Compare;  
        OnTournamentComplete Master;
        bool _done;
        bool _executing;
        public int CompCount { get { return _CompCount; } }
        int _CompCount;
        public int DrawCount { get { return _DrawCount; } }
        int _DrawCount;
        int [] _Histogram ;
        
        
        private struct QuickSortNextPair { public int begin; public int end; public QuickSortNextPair(int begin, int end) { this.begin = begin; this.end = end; } }
        List<QuickSortNextPair> queue;
        int queuepointer;
        
        public QuickTournament(List<IIndividual> arr, Evaluate evaluator,OnTournamentComplete  master)
        {
	        //std vals
	        this.mArr = arr;
	        this.begin = 0;
	        this.end = mArr.Count -1;
	
	        //piv vals
	        var pivot = Utils.RandomInt(0,mArr.Count);
	        pivotVal = mArr[pivot];
          
	        _swap(mArr,this.end,pivot);
	
	        //changing vals
	        this.pointing = 0;
	        this.ArrBetter = new List<IIndividual>();
	        this.ArrWorse = new List<IIndividual>();
	        this.queue = new List<QuickSortNextPair>();
	        this.queuepointer = 0;
	        this.queue.Add( new QuickSortNextPair(0,this.end)  );
	
	        //callbacks
	        this.Compare = evaluator; //Roughly speakin is A > B 
	        this.Master = master;
	
	        //check var
	        this._done = false;
	
	        this._executing = false;
	        this._CompCount = 0;
	        this._DrawCount = 0;
	
	        /*0-9,10-19,20-29,30-39,40-49,50-59, 60+*/
	        this._Histogram = new int [] {0, 0, 0, 0, 0, 0, 0};
	        Console.WriteLine("Current Pivot is " + this.pivotVal.Name);
        }

        void Evaluate (float score)
        {
	        if(!this._done)
	        {
		        this._CompCount++;
		        if(score > 0 )
		        {
			        this.ArrBetter.Add(this.mArr[this.pointing]);
		        }
		        else 
		        {
			        this.ArrWorse.Add(this.mArr[this.pointing]);

			        if(score == 0)
				        this._DrawCount++;
		        }
		        //scope to add stuff to histogram
		        {
			        var posScore = (score > 0) ? score: - score;
			        var histInd = (int) Math.Floor(posScore / 10);
			        histInd = (histInd > 6 ? 6 : histInd);
			        this._Histogram[histInd] += 1;
		        }
		        this.pointing++;
		        if(this.pointing >= this.end)
		        {
                var pivPos = -1;
                {//Reorder array
			              //Untouched first chunk
			              var Temp = new List<IIndividual>();
			              for(var ii = 0; ii < this.begin;++ii)
			              {
                    Temp.Add(this.mArr[ii]);
			              }
			              //Worse than pivot
			              Temp.AddRange(this.ArrWorse);
                    pivPos = Temp.Count;
			              //Pivot
			              Temp.Add(pivotVal);
                    //Better than pivot and the other untouched better chunk
                    Temp.AddRange(ArrBetter);
                    // Second untouched chunk
                    for(var ii = end+1; ii < mArr.Count;++ii)
			              {
                    Temp.Add(this.mArr[ii]);
			              }
              
			              mArr = Temp;
                }
			        //Add execution queue if relevant (more than 1 individual left to fight)
			        if(begin < pivPos-1)
				        queue.Add(new QuickSortNextPair( begin, pivPos-1) );
			        if(pivPos + 1 < end)
				        queue.Add(new QuickSortNextPair( pivPos+1,end));
			
			
			        this.queuepointer++;
			        if(this.queuepointer < this.queue.Count)
			        {
				        //Reset all values taking into consideration the queue
				        this.begin = this.queue[this.queuepointer].begin;
				        this.end = this.queue[this.queuepointer].end;
				        var pivot = Utils.RandomInt(this.begin,this.end+1);
				        this.pivotVal = this.mArr[pivot];
				        _swap(this.mArr,this.end,pivot);
				        this.pointing = this.begin;
				        this.ArrBetter = new List<IIndividual>();
				        this.ArrWorse = new List<IIndividual>();
			        }
			        else
			        {
				        this._done = true;
			        }
				
		        }
		        //Call-back evaluator to continue
		        if(_done)
		        {
			        _executing = false;
			        Master(mArr) ;//Tell the master algorithm this the evaluation is complete. Send it the "sorted" array.
		        }
	        }
	        else
		        Console.WriteLine("Evaluation already complete");
        }


        
        //Start the evaluator by calling the call-back with the required parameters
        //Two objects two compare and the object to call-back the score from
        //Evaluator.prototype.Start = function()
        //{
        //  this.Executing = true;
        //  this.Continue(this.Arr[this.pointing],this.pivotVal, this);
        //}

        public void Start()
        {
            this._executing = true;
            while (!_done)
            { 
              var score = Compare(this.mArr[this.pointing], this.pivotVal);
              Evaluate(score);
            }
        }

        //helper swap function.
        static void _swap (List<IIndividual> arr, int a,int b)
        {
	        var t = arr[a];
	        arr[a] = arr[b];
	        arr[b] = t;
        }

    }//endclass



}



//Integer test of Evaluator
//var TestEval = false;
//var ex;
//if(TestEval)
//{
//  var Arr = new Array(); for(var ii = 0; ii < 64; ++ ii) { Arr.push(HelperFunctions.RandomInt(0,100)) }
//  evalu = function(a, b, call)
//  { //Firefox bug in setTimeout = Error: useless setTimeout call (missing quotes around argument?)
//    if( a > b) {setTimeout(call.Execute(1),00); }
//    else { setTimeout(call.Execute(0),00); }
//  }
//  console.log(Arr);
//  ex = new Evaluator(Arr,evalu,function(event){console.log("Done") ; console.log(event);} );
//}
