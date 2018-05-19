//#define level0bench
#define logging
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;


namespace AiLibrary
{
    public class AiLibrary
    {
        #region Members
        int maxDepth;
        FindAllPossibleMoves moveFinder;
        GetFitnessForState eval;
        Prioritizer prioritize;
        object[] mEvalParams;
        OnMoveReady mCallback;
        bool mMaximizing;
        bool mTerminate;
        object lockTerminate = new object();
        float mTimeStopFactor = 0.98f;
        object move ;
        object lockMove = new object();
        #endregion


        #region Constructors
        public AiLibrary(FindAllPossibleMoves MoveFinder, GetFitnessForState Evaluator, int depth, object[] EvalParams)
            : this(MoveFinder, Evaluator, depth, EvalParams, null)
        {
        }
        public AiLibrary(FindAllPossibleMoves MoveFinder, GetFitnessForState Evaluator, int depth, object[] EvalParams, Prioritizer queuePrioritizer)
            : this(MoveFinder, Evaluator, depth, EvalParams, queuePrioritizer, null)
        {
        }

        public AiLibrary(FindAllPossibleMoves MoveFinder, GetFitnessForState Evaluator, int depth, object[] EvalParams, Prioritizer queuePrioritizer, OnMoveReady callback)
        {
#if logging
            FullLog = new List<List<string>>(depth + 1);  
            for(var ii = 0 ; ii< depth+1;++ii) FullLog.Add(new List<string>());
#endif
            moveFinder = MoveFinder;
            eval = Evaluator;
            maxDepth = depth;
            mEvalParams = EvalParams;
            mCallback = callback;
            mTerminate = false;
            prioritize = queuePrioritizer;
        }
        #endregion


        #region AlphaBetaDepthLimited
        public object AlphaBeta(object state,bool maximizing, ref double exectime)
        {
            var startTime = DateTime.Now;
            var start = new Node(state);
            
            var bestVal = _alphaBeta(start, 0, float.NegativeInfinity, float.PositiveInfinity, maximizing);
            //var bestVal = _alphaBetaBenched(start, 0, float.NegativeInfinity, float.PositiveInfinity, maximizing, ref exectime);
            mMaximizing = maximizing;
            lock (lockTerminate)
            {
                mTerminate = true;
            }
            var endTime = DateTime.Now;
            exectime = (endTime - startTime).TotalMilliseconds;
            
            Log("Real execution time is " + exectime);
            
            object res = null;
            foreach (var child in start.children)
            {
                if (child.inheritedValue == bestVal)
                {
                    res = child.State;
                    break;
                }
            }
            lock (lockMove)
            {
                move = res;
            }

            return res;
        }

        float _alphaBeta(Node n, int depth, float alpha, float beta, bool maximizing)
        {
            //Greatest depth, then terminate
            if (depth == maxDepth) //|| mTerminate)
            {
                n.trueValue = eval(n.State, mEvalParams);
                return n.trueValue;
            }

            var children = moveFinder(n.State);
            if (prioritize != null)
                prioritize(children, maximizing);


            var evaluatedChildren = 0;

            if (children.Count == 0)
            {
                n.trueValue = eval(n.State, mEvalParams);
                return n.trueValue;
            }

            foreach (var state in children)
            {
                ++evaluatedChildren;
                var child = new Node(state);
                child.inheritedValue = _alphaBeta(child, depth + 1, alpha, beta, !maximizing);
                if (maximizing) { alpha = Math.Max(alpha, child.inheritedValue); }
                else { beta = Math.Min(beta, child.inheritedValue); }
                n.children.Add(child);
                if (beta <= alpha)
                {
                    break;
                }
            }
            _evaluatedTotal += evaluatedChildren;
            _savedChildrenTotal += children.Count - evaluatedChildren;
            var res = maximizing ? alpha : beta;
            return res;

        }
        #endregion


        #region AlphaBetaTimeLimited
        public object AlphaBetaTimeLimited(object state, bool maximizing, int timeout, ref double exectime)
        {
            var startTime = DateTime.Now;
            var start = new Node(state);
            var bestVal = 0.0f;

            mTerminate = false;
            mMaximizing = maximizing;
       
            var nearEndtime = ((int)((float)timeout * this.mTimeStopFactor)) ; //Time to wait between stop deepening and complete stop
            Thread execute = new Thread(new ThreadStart(() => { bestVal = _alphaBetaWithTerminate(start, 0, float.NegativeInfinity, float.PositiveInfinity, maximizing); }));
            execute.Start();
            var terminated = execute.Join(nearEndtime);
            if (!terminated)
            {
                Log("** Stopping overall execution **");
                lock (lockTerminate)
                {
                    mTerminate = true;
                }
                execute.Join();
            }
            var endTime = DateTime.Now;
            exectime = (endTime - startTime).TotalMilliseconds;
            Log("Real execution time is " + exectime);

            object res = null;
            foreach (var child in start.children)
            {
                if (child.inheritedValue == bestVal)
                {
                    res = child.State;
                    break;
                }
            }
            lock (lockMove)
            {
                move = res;
            }
            return res;
        }
        int _maxReachedDepth;

        float _alphaBetaWithTerminate(Node n, int depth, float alpha, float beta, bool maximizing)
        {

            bool term = false;
            lock (lockTerminate)
            {
                term = mTerminate;
            }

            //Greatest depth || time up, then terminate
            if (depth >= maxDepth || mTerminate)
            {
                n.trueValue = eval(n.State, mEvalParams);
                return n.trueValue;
            }

            var children = moveFinder(n.State);
            if (prioritize != null)
                prioritize(children, maximizing);


            var evaluatedChildren = 0;

            if (children.Count == 0)
            {
                n.trueValue = eval(n.State, mEvalParams);
                return n.trueValue;
            }

            foreach (var state in children)
            {
                ++evaluatedChildren;
                var child = new Node(state);
                child.inheritedValue = _alphaBetaWithTerminate(child, depth + 1, alpha, beta, !maximizing);
                if (maximizing) { alpha = Math.Max(alpha, child.inheritedValue); }
                else { beta = Math.Min(beta, child.inheritedValue); }
                n.children.Add(child);
                if (beta < alpha)
                {
                    break;
                }
            }
            _evaluatedTotal += evaluatedChildren;
            _savedChildrenTotal += children.Count - evaluatedChildren;
            var res = maximizing ? alpha : beta;
            return res;

        }

        #endregion


        #region Benchmarks

        #region AlphaBetaBenched
        public object AlphaBetaBeched(object state, bool maximizing, ref double exectime)
        {
            var startTime = DateTime.Now;
            var start = new Node(state);

            //var bestVal = _alphaBeta(start, 0, float.NegativeInfinity, float.PositiveInfinity, maximizing);
            var bestVal = _alphaBetaBenched(start, 0, float.NegativeInfinity, float.PositiveInfinity, maximizing, ref exectime);
            mMaximizing = maximizing;
            lock (lockTerminate)
            {
                mTerminate = true;
            }
            var endTime = DateTime.Now;
            exectime = (endTime - startTime).TotalMilliseconds;

            Log("Real execution time is " + exectime);

            object res = null;
            foreach (var child in start.children)
            {
                if (child.inheritedValue == bestVal)
                {
                    res = child.State;
                    break;
                }
            }
            lock (lockMove)
            {
                move = res;
            }

            return res;
        }
      
        float _alphaBetaBenched(Node n, int depth, float alpha, float beta, bool maximizing, ref double mytime)
        {
#if level0bench
            var _alphabetaStart = DateTime.Now;
#endif
            bool term = false;
            lock (lockTerminate)
            {
                term = mTerminate;
            }
            if (depth == maxDepth) //|| mTerminate)
            {
#if level0bench
                var _evalStart = DateTime.Now;
#endif
                n.trueValue = eval(n.State, mEvalParams);
#if level0bench
                b_alphabetaEval += (DateTime.Now - _evalStart).TotalMilliseconds;
#endif
#if logging
                var readable = ((IReadableState)n.State);
                FullLog[depth].Add(string.Format("{1}\n{0}", readable.StateMove(), 
                    string.Format("V:{2};α:{0};β:{1};", alpha, beta, n.trueValue)
                    )
                    );
#endif
#if level0bench
                mytime = (DateTime.Now - _alphabetaStart).TotalMilliseconds;
                b_alphabetaTotalTime += mytime;
#endif
                return n.trueValue;
            }
#if level0bench
            var _movefinderStart = DateTime.Now;
#endif
            var children = moveFinder(n.State);
            if (prioritize != null)
                prioritize(children, maximizing);
#if level0bench
            b_alphabetaMoveFinder += (DateTime.Now - _movefinderStart).TotalMilliseconds;
#endif
            var childrenTime = 0.0;
            var evaluatedChildren = 0;
            if (children.Count == 0)
            {
                n.trueValue = eval(n.State, mEvalParams);
                return n.trueValue;
            }

            foreach (var state in children)
            {
                ++evaluatedChildren;
                var childtime = 0.0;
                var child = new Node(state);
                child.inheritedValue = _alphaBetaBenched(child, depth + 1, alpha, beta, !maximizing, ref childtime);
                childrenTime += childtime;
                if (maximizing) { alpha = Math.Max(alpha, child.inheritedValue); }
                else { beta = Math.Min(beta, child.inheritedValue); }
                n.children.Add(child);
                if (beta <= alpha)
                {
                    break;
                }
            }
            _evaluatedTotal += evaluatedChildren;
            _savedChildrenTotal += children.Count - evaluatedChildren;
            var res = maximizing ? alpha : beta;
            //      Console.WriteLine("Level {0} value = {1} for move {2} by {3}", depth, res, ((IReadableState)n.State).StateMove(), maximizing ? "Max":"Min");
#if logging

            var readable_2 = ((IReadableState)n.State);
            FullLog[depth].Add(
                string.Format("{1}\n{0}",
                    readable_2.StateMove().PadRight(PaddRight * evaluatedChildren),
             
                    string.Format("V:{1};α:{0};β:{4};C:{2};E:{3}", alpha, res, children.Count, evaluatedChildren, beta).PadRight(PaddRight * evaluatedChildren)
                    )
                );
#endif
#if level0bench
            mytime = (DateTime.Now - _alphabetaStart).TotalMilliseconds - childrenTime;
            b_alphabetaTotalTime += mytime;
#endif

            return res;

        }
        #endregion


        int _savedChildrenTotal = 0;
        public int SavedChildrenTotal { get { return _savedChildrenTotal; } }

        int _evaluatedTotal = 0;
        public int EvaluatedTotal { get { return _evaluatedTotal; } }




#if level0bench
        public void PrintBench()
        {
            Console.WriteLine("Total alphabeta time {0}.", b_alphabetaTotalTime);
            Console.WriteLine("Time spent in move finding {0} : {1}%", b_alphabetaMoveFinder,b_alphabetaMoveFinder/b_alphabetaTotalTime*100);
            Console.WriteLine("Time spent in evaluation {0} : {1}%", b_alphabetaEval, b_alphabetaEval / b_alphabetaTotalTime * 100);
            b_alphabetaTotalTime=0;
            b_alphabetaMoveFinder=0;
            b_alphabetaEval=0;
        }
#endif
        #endregion



#if level0bench
        double b_alphabetaTotalTime = 0;
        double b_alphabetaMoveFinder = 0;
        double b_alphabetaEval = 0;
#endif

        #region Log
        bool mLogging = true;
        void Log(string message)
        {
            if (mLogging)
                Console.WriteLine(message);
        }

#if logging

        public void LogTree()
        {
            var all = new StringBuilder();
            var c = 0;
            foreach (var level in FullLog)
            {
                var line = new List<StringBuilder>();
                line.AddRange(new StringBuilder[] { new StringBuilder(""), new StringBuilder(""), new StringBuilder(""), new StringBuilder(""), new StringBuilder(""), new StringBuilder(""), new StringBuilder(""), new StringBuilder("\n***********************") });
                foreach (var state in level)
                {
                    var lines = state.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    for (var ii = 0; ii < lines.Length; ++ii)
                    { line[ii].Append(lines[ii].PadRight(PaddRight)); }
                }
                foreach (var minline in line)
                {
                    all.AppendLine(minline.ToString());
                }
                all.AppendLine((((c++) % 2) == 1 ^ mMaximizing) ? "Maximizing" : "Minimizing");
                all.AppendLine("");
            }

            File.WriteAllText("log.log", all.ToString());
            foreach (var lev in FullLog) lev.Clear();
        }

        List<List<string>> FullLog;
        static int PaddRight = 40;
#endif
        #endregion
    }
}

//public void PlayGame(object start, FindAllPossibleMoves moveFinder , GetFitnessForState evaluator , bool maximize, int maxtime  )
//{
//    var startT = DateTime.Now;
//    var moves = moveFinder(start);
//    List<Thread> executingThreads = new List<Thread>();
//    foreach (object node in moves)
//    {
//        var t = new Thread(() => _alphaBeta(new Node(null, node), 0, maximize, moveFinder, evaluator));
//        t.Start();
//    }
//    Thread.Sleep((int)(0.80 * maxtime));
//    foreach (Thread child in executingThreads)
//    {
//        child.Join();
//    }
//    var bestValue = (maximize ? float.NegativeInfinity : float.PositiveInfinity);
//    var bestIndex = 0;
//    for (var ii = 0; ii < results.Count; ++ii)
//    {
//        if(maximize ?  (results[ii]  > bestValue) : (results[ii] < bestValue) )
//        {
//            bestValue = results[ii];
//            bestIndex = ii;
//        }
//    }
//    lock (lockNextMove)
//    {
//        nextMove = moves[bestIndex];
//    }
//    var endT = DateTime.Now;
//    Console.WriteLine(string.Format("Real execution time: {0}",(endT - startT).TotalMilliseconds));
//}

