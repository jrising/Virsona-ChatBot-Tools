/******************************************************************\
 *      Class Name:     SalienceArena
 *      Written By:     James.R
 *      Copyright:      Virsona Inc
 *      -----------------------------------------------------------
\******************************************************************/
using System.Collections.Generic;
using System;
using System.IO;
using System.Collections.Specialized;
using PluggerBase;
using PluggerBase.ActionReaction.Evaluations;
using PluggerBase.FastSerializer;
using DataTemple.Codeland.SearchTree;
using InOutTools;

namespace DataTemple.Codeland
{
    /// <summary> 
    /// Manages the collection of Codelets
    /// </summary> 
    public class Coderack : IArena
    {

#region Attributes
        /// <summary> 
        /// All active codelets
        /// </summary> 
        protected ISalienceSet<Codelet> codelets;

        /// <summary> 
        /// Codelets used as descriptors while in the middle of processing
        /// </summary> 
        protected Codelet isEvaluating;
        protected Codelet isCompleting;
        public const string nameActive = "(active)";

        /// <summary> 
        /// Is the current codelet being watched?
        /// </summary> 
        protected bool watching;

        /// <summary> 
        /// A message reciever
        /// </summary> 
        protected IMessageReceiver receiver;
		
		protected Codelet currentEvaluating;
#endregion

#region Properties
        /// <summary> 
        /// Calculate how many codelets are at the top level
        /// </summary> 
        public int CountTop {
            get {
                return codelets.Count;
            }
        }

        /// <summary> 
        /// Calculate the summed salience of codelets
        /// </summary> 
        public double SalienceTotal {
            get {
                return codelets.SalienceTotal;
            }
        }

        public bool Watching
        {
            get
            {
                return watching;
            }
        }

        public IMessageReceiver Receiver
        {
            get
            {
                return receiver;
            }
        }
#endregion

#region Methods
        /// <summary> 
        /// Create a new coderack
        /// </summary> 
        public Coderack(IMessageReceiver receiver) {
            codelets = new SalienceTree<Codelet>();
            isEvaluating = new DescriptorCodelet(this, "evaluating");
            isCompleting = new DescriptorCodelet(this, "completing");
            watching = false;
            this.receiver = receiver;
        }

        /// Deserialization constructor
        public Coderack() {
            isEvaluating = new DescriptorCodelet(this, "evaluating");
            isCompleting = new DescriptorCodelet(this, "completing");
            watching = false;
        }

        /// <summary> 
        /// Reset coderack to have no codelets
        /// </summary>
        public virtual void Reset(object defaultResult) {
            codelets.Clear();
        }

        public void setReceiver(IMessageReceiver receiver) {
            this.receiver = receiver;
        }

        /// <summary> 
        /// Execute the coderack for a given amount of time
        /// </summary> 
        public virtual void Execute(int time, bool debugMode) {
            //int origtime = time;
            //Profiler profiler = new Profiler();

            while (time > 0 && CountTop > 0) {
                   // && (profiler.GetTime() / 1000 < origtime)) {
                int used = ExecuteOne(debugMode);
                if (used == 0) {
                    // This shouldn't happen-- we're out of executable codelets
                    break;
                }

                time -= used;
            }

            // Check if we're taking the time we say we are
            /*long micros = (long) (profiler.GetTime() / 1000);
            if (Math.Abs((origtime - time) - micros) > Math.Min((origtime - time), micros) / 100) {
                receiver.Receive("Time Miscalculation: " + (origtime - time).ToString() + " units took " + micros.ToString(), this);
                if (debugMode) {
                    receiver.Receive(Profiler.AnnounceWorst(), this);
                    //receiver.Receive(profiler.AnnounceEach(), this);
                    //throw new Exception(Profiler.AnnounceEach());
                    //SingleUserLog.Replace("Complete");
                }
            }*/

            if (time > 0)
                receiver.Receive("Coderack Exhausted.", this);
            else
                receiver.Receive("Time Limit Reached.", this);
        }

        public virtual int ExecuteOne(bool debugMode)
        {
            Codelet codelet = SelectSalientCodelet();
            if (codelet == null)
                return 0; // nothing to do!

            if (!codelet.NeedsEvaluation)
            {
                // Skip it!
                return 1;
            }

            watching = codelet.watched;
            int used = 1;

            try
            {
                used = EvaluateCodelet(codelet, debugMode) + 1;
            }
			catch (UserException e) {
                receiver.Receive(e.Message, codelet);
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

            return used;
        }

        /// <summary> 
        /// Choose a codelet based on salience
        /// </summary> 
        public Codelet SelectSalientCodelet() {
            return codelets.SelectSalientItem(RandomSearchQuality.Fast);
        }

        /// <summary> 
        /// Choose a codelet, regardless of salience
        /// </summary> 
        public Codelet SelectRandomCodelet() {
            return codelets.SelectRandomItem(RandomSearchQuality.Fast);
        }

        /// <summary> 
        /// Add the given codelet
        /// </summary> 
        public virtual bool AddCodelet(Codelet codelet, string location) {
            //Console.WriteLine("Add " + codelet.ToString() + ": " + location + ": " + codelets.Count);
            if (codelet.coderack != this)
                throw new ArgumentException("codelet does not have this coderack!");
			
			if (currentEvaluating != null)
				codelet.Trace = currentEvaluating.Trace.AppendFrame(currentEvaluating, location);
			else
				codelet.Trace.AppendFrame(null, location);
            codelets.Add(codelet.Salience, codelet);
            if (watching)
                codelet.watched = true;  // we're watched if our adder was

            return true;
        }

        /// <summary> 
        /// Request to complete a codelet
        /// </summary> 
        public virtual bool CompleteCodelet(Codelet codelet) {
            if (codelet.GetFutureCodelet(nameActive) != null)
                return true;

            codelet.AddFutureCodelet(nameActive, isCompleting);
            if (codelet.Complete()) {
                DeleteCodelet(codelet);
                return true;
            } else {
                codelet.RemoveFutureCodelet(nameActive);
                return false;
            }
        }

        /// <summary> 
        /// Remove the given codelet
        /// </summary> 
        public void DeleteCodelet(Codelet codelet) {
            // Console.WriteLine("Delete " + codelet.ToString)
            if (!codelets.Remove(codelet.Salience, codelet))
                receiver.Receive("Could not delete " + codelet.ToString(), codelet);
        }

        /// <summary> 
        /// Request to evaluate the codelet with given time
        /// </summary> 
        public int EvaluateCodelet(Codelet codelet, bool debugMode) {
            if (codelet.GetFutureCodelet(nameActive) != null)
                return 0;
			currentEvaluating = codelet;

            codelet.AddFutureCodelet(nameActive, isEvaluating);
            if (debugMode) {
                //Profiler.Start(codelet.GetType().FullName);
                //SingleUserLog.Update(codelet.ToString(), Profiler.NumExecuted(codelet.ToString()));
                //SingleUserLog.Replace(codelet.ToString());
            }
            receiver.Receive("EvaluateCodelet", codelet);
            int used = codelet.Evaluate();
            if (debugMode) {
                //Profiler.End(used * 1000);
			}

            codelet.AdjustTime(-used);
            codelet.RemoveFutureCodelet(nameActive);

            if (codelet.time <= 0 && codelet.Salience > 0) {
                // Complete it!
                CompleteCodelet(codelet);
                used += 1;
            }
			currentEvaluating = null;

            return used;
        }

        /// <summary> 
        /// Delete a codelet at random
        /// </summary> 
        public virtual void DeleteRandomCodelet() {
            Codelet codelet = SelectRandomCodelet();
            if (codelet.GetFutureCodelet(nameActive) == null)
                DeleteCodelet(codelet);
        }

        /// <summary> 
        /// Adjust the total amount of salience, as a result of a codelet changing
        /// </summary> 
        public void ChangeSalience(Codelet codelet, double before, double after) {
            codelets.ChangeSalience(codelet, before, after);
        }
#endregion

#region IFastSerializable Members

        public virtual void Deserialize(SerializationReader reader) {
            codelets = (ISalienceSet<Codelet>) reader.ReadPointer();  // codelets = CType(info.GetValue("codelets", GetType(SearchTree.ISalienceSet<Codelet))), SearchTree.ISalienceSet<Codelet))
        }

        public virtual void Serialize(SerializationWriter writer) {
            writer.WritePointer(codelets);   // info.AddValue("codelets", codelets)
        }
#endregion
		
#region IArena Members
		public int Call(ICallable callable, double salience, object value, IContinuation succ, IFailure fail)
		{
			AddCodelet(new CodeletEvaluableWrapper(new CallableAsEvaluable(callable, value, succ, fail), this, salience, 1, 1), "Call");
			return 1;
		}
		
		public int Continue(IContinuation cont, double salience, object value, IFailure fail)
		{
			AddCodelet(new CodeletEvaluableWrapper(new ContinuationAsEvaluable(cont, value, fail), this, salience, 1, 1), "Continue");
			return 1;
		}
		
		public int Fail(IFailure fail, double salience, string reason, IContinuation skip)
		{
			AddCodelet(new CodeletEvaluableWrapper(new FailureAsEvaluable(fail, reason, skip), this, salience, 1, 1), "Coderack Fail");
			return 1;
		}
		
		public int Evaluate (IEvaluable evaluable, double salience)
		{
			AddCodelet(new CodeletEvaluableWrapper(evaluable, this, salience, 1, 1), "Evaluate");
			return 1;
		}
#endregion
    }
}
