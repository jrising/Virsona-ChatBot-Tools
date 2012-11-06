/******************************************************************\
 *      Class Name:     Coderack
 *      Written By:     James.R
 *      Copyright:      Virsona Inc
 *      -----------------------------------------------------------
 * Coderack optimized for brief codelets
 *   Only compares adjacent pairs of salience, so always executes
 *   half of the coderack
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using PluggerBase;
using DataTemple.Codeland.SearchTree;

namespace DataTemple.Codeland
{
    public class ZippyCoderack : Coderack
    {
        protected Random randgen = new Random();

        #region Overrides
        /// <summary> 
        /// Create a new coderack
        /// </summary> 
        public ZippyCoderack(IMessageReceiver receiver, int space)
            : base()
        {
            codelets = new SalienceList<Codelet>();
            this.receiver = receiver;
        }

        /// <summary> 
        /// Execute the coderack for a given amount of time
        /// </summary> 
        public override void Execute(int time, double exitScore, bool debugMode)
        {
            int origtime = time;
            Profiler profiler = new Profiler();
            SalienceList<Codelet> codelist = (SalienceList<Codelet>)codelets;

            while (time > 0 && CountTop > 0 &&
                   (profiler.GetTime() / 1000 < origtime))
            {
                // start from the end and move backwards, since new are added to end
                LinkedListNode<double> keys = codelist.LinkedKeys.Last;
                LinkedListNode<Codelet> values = codelist.LinkedValues.Last;
                if (values == null)
                {
                    // This shouldn't happen-- we're out of executable codelets
                    break;
                }

                while (values != null)
                {
                    Codelet codelet = values.Value;
                    if (values.Previous != null)
                    {
                        // Use this one or the last one?
                        if ((keys.Previous.Value + keys.Value) * randgen.NextDouble() > keys.Value)
                            codelet = values.Previous.Value;
                    }

                    if (codelet == null)
                    {
                        // This shouldn't happen-- we're out of executable codelets
                        break;
                    }

                    if (!codelet.NeedsEvaluation)
                    {
                        // Skip it!
                        time -= 1;
                        continue;
                    }

                    watching = codelet.watched;

                    // Console.WriteLine("=(" + CountPrime.ToString + "/" + codelets.Count.ToString + ")=> " + codelet.ToString())
                    try
                    {
                        time -= EvaluateCodelet(codelet, debugMode) + 1;
                    }
                    catch (Exception e)
                    {
                        // Record exception here!
                        receiver.Receive(codelet.ToString() + " threw exception " + e.Message, codelet);
                        receiver.Receive(e.StackTrace, e);
                        if (!codelet.immune)
                        {
                            // Remove the bad codelet
                            DeleteCodelet(codelet);
                        }
                        else
                            codelet.RemoveFutureCodelet(nameActive);
                        if (debugMode)
                            throw new Exception(e.Message + " - " + e.StackTrace, e);
                    }

                    watching = false;

                    // Move back two
                    values = values.Previous;
                    keys = keys.Previous;
                    if (values != null)
                    {
                        values = values.Previous;
                        keys = keys.Previous;
                    }
                }
            }

            // Check if we're taking the time we say we are
            long micros = (long)(profiler.GetTime() / 1000);
            if (Math.Abs((origtime - time) - micros) > Math.Min((origtime - time), micros) / 100)
            {
                receiver.Receive("Time Miscalculation: " + (origtime - time).ToString() + " units took " + micros.ToString(), this);
                if (debugMode)
                {
                    receiver.Receive(Profiler.AnnounceWorst(), this);
                    //receiver.Receive(profiler.AnnounceEach(), this);
                    //throw new Exception(Profiler.AnnounceEach());
                    //SingleUserLog.Replace("Complete");
                }
            }

            if (time > 0)
                receiver.Receive("Coderack Exhausted.", this);
            else
                receiver.Receive("Time Limit Reached.", this);
        }

        public override int ExecuteOne(bool debugMode)
        {
            SalienceList<Codelet> codelist = (SalienceList<Codelet>)codelets;

            // start from the end and move backwards, since new are added to end
            LinkedListNode<double> keys = codelist.LinkedKeys.Last;
            LinkedListNode<Codelet> values = codelist.LinkedValues.Last;
            if (values == null)
                return 0;   // nothing to do!

            Codelet codelet = values.Value;
            if (values.Previous != null)
            {
                // Use this one or the last one?
                if ((keys.Previous.Value + keys.Value) * randgen.NextDouble() > keys.Value)
                    codelet = values.Previous.Value;
            }

            if (codelet == null)
                return 0;   // nothing to do!

            if (!codelet.NeedsEvaluation)
            {
                // Skip it!
                return 1;
            }

            watching = codelet.watched;
            int used = 1;

            // Console.WriteLine("=(" + CountPrime.ToString + "/" + codelets.Count.ToString + ")=> " + codelet.ToString())
            try
            {
                used = EvaluateCodelet(codelet, debugMode) + 1;
            }
            catch (Exception e)
            {
                // Record exception here!
                receiver.Receive(codelet.ToString() + " threw exception " + e.Message, codelet);
                receiver.Receive(e.StackTrace, e);
                if (!codelet.immune)
                {
                    // Remove the bad codelet
                    DeleteCodelet(codelet);
                }
                else
                    codelet.RemoveFutureCodelet(nameActive);
                if (debugMode)
                    throw new Exception(e.Message + " - " + e.StackTrace, e);
            }

            watching = false;

            // Move back two
            values = values.Previous;
            keys = keys.Previous;
            if (values != null)
            {
                values = values.Previous;
                keys = keys.Previous;
            }

            return used;
        }
        #endregion
    }
}
