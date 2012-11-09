/******************************************************************\
 *      Class Name:     Profiler
 *      Written By:     James.R
 *      Copyright:      Virsona Inc
 *      -----------------------------------------------------------
 * Timing and profiling
\******************************************************************/
//#define PROFILE

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace DataTemple.Codeland
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
        }

        public double GetTime()
        {
			return stopwatch.ElapsedTicks * 1.0e9 / Stopwatch.Frequency;
        }

#if PROFILE
        protected static Dictionary<string, int> numExecuted = new Dictionary<string, int>();
        protected static Dictionary<string, long> expectedTimes = new Dictionary<string, long>();
        protected static Dictionary<string, long> totalCounts = new Dictionary<string, long>();
        protected static Dictionary<string, long> selfCounts = new Dictionary<string, long>();

        protected static Stack<Profiler> stack = new Stack<Profiler>();

        protected string name;
        protected long subCounts;

        protected Profiler(string name)
        {
            this.name = name;
            QueryPerformanceCounter(out startCount);
            subCounts = 0;

            stack.Push(this);
        }

        public static void Start(string name)
        {
            // stores in the stack
            new Profiler(name);
        }
        
        public static void End(long expected)
        {
            Profiler profiler = stack.Pop();
            string name = profiler.name;

            long stopCount;
            QueryPerformanceCounter(out stopCount);
            long count = stopCount - profiler.startCount;

            // Add this to our totals
            long totalCount = 0;
            totalCounts.TryGetValue(name, out totalCount);
            totalCounts[name] = totalCount + count;

            long selfCount = 0;
            selfCounts.TryGetValue(name, out selfCount);
            selfCounts[name] = selfCount + count - profiler.subCounts;

            // add to our expected time
            long expectedTime = 0;
            expectedTimes.TryGetValue(name, out expectedTime);
            expectedTimes[name] = expectedTime + expected;

            int numExecute = 0;
            numExecuted.TryGetValue(name, out numExecute);
            numExecuted[name] = numExecute + 1;

            // Add our totalCount to our parent's sub
            if (stack.Count > 0)
            {
                Profiler parent = stack.Peek();
                parent.subCounts += count;
            }
        }

        public static string WorstCode()
        {
            double worstScore = 0;
            string worstName = "";

            foreach (KeyValuePair<string, long> kvp in expectedTimes)
            {
                int numExecute = numExecuted[kvp.Key];
                double realTime = Duration(totalCounts[kvp.Key]);
                // score is how far off from expected
                double score = Math.Abs(kvp.Value - realTime) / numExecute;
                // ... times ratio to expected time
                score *= realTime / kvp.Value;
                double selfTime = Duration(selfCounts[kvp.Key]);
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
            string worst = Tools.Profiler.WorstCode();
            return "Worst Code: " + worst + ": " + Tools.Profiler.numExecuted[worst] + "x " + ((int) Tools.Profiler.AverageExpected(worst)) + " expected, " + ((int) Tools.Profiler.AverageTotal(worst)) + " (" + ((int) Tools.Profiler.AverageSelf(worst)) + ") actual";
        }

        public static string AnnounceEach()
        {
            StringBuilder result = new StringBuilder();
            foreach (KeyValuePair<string, long> kvp in expectedTimes)
                result.Append(kvp.Key + ": " + Tools.Profiler.numExecuted[kvp.Key] + "x " + ((int) Tools.Profiler.AverageExpected(kvp.Key)) + " expected, " + ((int)Tools.Profiler.AverageTotal(kvp.Key)) + " (" + ((int)Tools.Profiler.AverageSelf(kvp.Key)) + ") actual: " + ((long) Duration(totalCounts[kvp.Key])) + "\n");

            return result.ToString();
        }

        public static int NumExecuted(string name)
        {
            int num;
            numExecuted.TryGetValue(name, out num);
            return num;
        }

        public static double AverageTotal(string name) {
            return Duration(totalCounts[name]) / numExecuted[name];
        }

        public static double AverageSelf(string name)
        {
            return Duration(selfCounts[name]) / numExecuted[name];
        }

        public static double AverageExpected(string name)
        {
            return expectedTimes[name] / (double)numExecuted[name];
        }
#else
        public static void Start(string name) { }
        public static void End(long expected) { }
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
