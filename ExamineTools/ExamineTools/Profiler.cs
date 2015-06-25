/******************************************************************\
 *      Class Name:     Profiler
 *      Written By:     James.R
 *      Copyright:      Virsona Inc
 *      -----------------------------------------------------------
 * Timing and profiling
\******************************************************************/
#define PROFILE

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace ExamineTools
{
    /*
     * The Profile class keeps track of the time used by functions, as well as
     * their expected time, and time used by other recursed timed functions
     */
    public class Profiler
    {
		protected Stopwatch stopwatch;

        public Profiler() {
			stopwatch = new Stopwatch();
			stopwatch.Start();
        }

        public double GetTime()
        {
			return stopwatch.ElapsedTicks * 1.0e9 / Stopwatch.Frequency;
        }

#if PROFILE
        protected static Dictionary<string, int> numExecuted = new Dictionary<string, int>();
        protected static Dictionary<string, double> expectedTimes = new Dictionary<string, double>();
        protected static Dictionary<string, double> totalTimes = new Dictionary<string, double>();
        protected static Dictionary<string, double> selfTimes = new Dictionary<string, double>();

        protected static Stack<Profiler> stack = new Stack<Profiler>();

        protected string name;
        protected double subTimes;

        protected Profiler(string name)
        {
            this.name = name;
			stopwatch = new Stopwatch();
			stopwatch.Start();
            subTimes = 0;

            stack.Push(this);
        }

        public static void Start(string name)
        {
            // stores in the stack
            new Profiler(name);
        }
        
        public static void End(double expected)
        {
            Profiler profiler = stack.Pop();
            string name = profiler.name;

            double time = profiler.GetTime();

            // Add this to our totals
            double totalTime = 0;
            totalTimes.TryGetValue(name, out totalTime);
            totalTimes[name] = totalTime + time;

            double selfTime = 0;
            selfTimes.TryGetValue(name, out selfTime);
            selfTimes[name] = selfTime + time - profiler.subTimes;

            // add to our expected time
            double expectedTime = 0;
            expectedTimes.TryGetValue(name, out expectedTime);
            expectedTimes[name] = expectedTime + expected;

            int numExecute = 0;
            numExecuted.TryGetValue(name, out numExecute);
            numExecuted[name] = numExecute + 1;

            // Add our totalTime to our parent's sub
            if (stack.Count > 0)
            {
                Profiler parent = stack.Peek();
                parent.subTimes += time;
            }
        }

        public static string WorstCode()
        {
            double worstScore = 0;
            string worstName = "";

            foreach (KeyValuePair<string, double> kvp in expectedTimes)
            {
                int numExecute = numExecuted[kvp.Key];
                double realTime = totalTimes[kvp.Key];
                // score is how far off from expected
                double score = Math.Abs(kvp.Value - realTime) / numExecute;
                // ... times ratio to expected time
                score *= realTime / kvp.Value;
                double selfTime = selfTimes[kvp.Key];
                // ... plus how much is just self
                if (selfTime > kvp.Value)
                    score += (selfTime - kvp.Value) / numExecute;

                if (score > worstScore)
                {
                    worstName = kvp.Key;
                    worstScore = score;
                }
            }

            return worstName;
        }

        public static string AnnounceWorst()
        {
			if (Profiler.expectedTimes.Count == 0)
				return "Nothing profiled.";
			
            string worst = Profiler.WorstCode();
            return "Worst Code: " + worst + ": " + Profiler.numExecuted[worst] + "x " + ((int) Profiler.AverageExpected(worst)) + " expected, " + ((int) Profiler.AverageTotal(worst)) + " (" + ((int) Profiler.AverageSelf(worst)) + ") actual";
        }

        public static string AnnounceEach()
        {
            StringBuilder result = new StringBuilder();
            foreach (KeyValuePair<string, double> kvp in expectedTimes)
                result.Append(kvp.Key + ": " + Profiler.numExecuted[kvp.Key] + "x " + ((int) Profiler.AverageExpected(kvp.Key)) + " expected, " + ((int)Profiler.AverageTotal(kvp.Key)) + " (" + ((int)Profiler.AverageSelf(kvp.Key)) + ") actual: " + ((double) totalTimes[kvp.Key]) + "\n");

            return result.ToString();
        }

        public static int NumExecuted(string name)
        {
            int num;
            numExecuted.TryGetValue(name, out num);
            return num;
        }

        public static double AverageTotal(string name) {
            return totalTimes[name] / numExecuted[name];
        }

        public static double AverageSelf(string name)
        {
            return selfTimes[name] / numExecuted[name];
        }

        public static double AverageExpected(string name)
        {
            return expectedTimes[name] / (double)numExecuted[name];
        }
#else
        public static void Start(string name) { }
        public static void End(double expected) { }
        public static string AnnounceWorst()
        {
            return "Profiler disabled.";
        }

        public static string AnnounceEach()
        {
            return "Profiler disabled.";
        }

        public static int NumExecuted(string name)
        {
            return 0;
        }
#endif
    }
}
