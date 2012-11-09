/******************************************************************\
 *      Class Name:     Codelet
 *      Written By:     James.R
 *      Copyright:      Virsona Inc
 *      -----------------------------------------------------------
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using PluggerBase.ActionReaction.Evaluations;
using PluggerBase.FastSerializer;

namespace DataTemple.Codeland
{
    /// <summary> 
    /// Performs an action with limited time and memory requirements
    /// </summary> 
    public class Codelet : AgentBase, IEvaluable
    {

#region Attributes
        /// <summary> 
        /// The coderack that we're part of
        /// </summary> 
        public Coderack coderack;

        /// <summary> 
        /// How much space do we require
        /// </summary> 
        public int space;
        /// <summary> 
        /// How much time do we need to be fully executed?
        /// </summary> 
        public int time;
	
		public CodeletTrace trace;

        /// <summary> 
        /// All our future codelets
        /// </summary> 
        protected Dictionary<string, Codelet> children = new Dictionary<string, Codelet>();

        /// <summary> 
        /// Flag for debugging codelets
        /// </summary> 
        public bool watched;

        /// <summary> 
        /// Flag to not remove a codelet on exception
        /// </summary> 
        public bool immune;

        /// <summary> 
        /// Randomizer for adding random salience
        /// </summary> 
        protected static Random randgen = new Random();

        /// <summary> 
        /// Do we need to be evaluated?
        /// </summary> 
        public bool NeedsEvaluation {
            get {
                return salience > 0 || time > 0;
            }
        }
#endregion

#region Methods

        /// <summary> 
        /// Create a codelet with needed information
        /// </summary> 
        public Codelet(Coderack cr, double sl, int sp, int tm)
        {
            coderack = cr;
            if (sl == 0)
                salience = sl;
            else
                salience = sl + randgen.NextDouble();
            space = sp + 8 * 4;
            time = tm;
			trace = new CodeletTrace();
            watched = false;
            immune = false;
        }

        /// Deserialization constructor
        public Codelet() {
            watched = false;
            immune = false;
        }

        public virtual void Reset()
        {
            children = new Dictionary<string, Codelet>();
        }

        public virtual object Clone()
        {
            Codelet clone = (Codelet) MemberwiseClone();
            clone.Reset();
            return clone;
        }
		
		public CodeletTrace Trace {
			get {
				return trace;
			}
			set {
				trace = value;
			}
		}

        /// <summary> 
        /// Check if we have a future codelet with the specified key
        /// </summary> 
        public Codelet GetFutureCodelet(string key) {
            Codelet found = null;
            if (children.TryGetValue(key, out found))
                return found;
            else
                return null;
        }

        /// <summary> 
        /// Add a future codelet with the specified key
        /// </summary> 
        public void AddFutureCodelet(string key, Codelet codelet) {
            children.Add(key, codelet);
        }

        /// <summary> 
        /// Remove a future codelet with the specified key
        /// </summary> 
        public void RemoveFutureCodelet(string key) {
            Codelet child = GetFutureCodelet(key);
			if (child != null)
	            children.Remove(key);
        }

        /// <summary> 
        /// Do whatever processing we do
        /// </summary> 
        public virtual int Evaluate() {
            return time;
        }

        /// <summary>
        /// Add any future codelets to the coderack
        /// </summary> 
        public override bool Complete() {
            foreach (KeyValuePair<string, Codelet> kvp in children) {
                if (kvp.Value.NeedsEvaluation)
                    coderack.AddCodelet(kvp.Value, "Complete child");
            }

            return true;
        }

        public int TotalTime() {
            int total = time;
            foreach (KeyValuePair<string, Codelet> kvp in children)
                total += kvp.Value.TotalTime();

            return total;
        }

        /// <summary>
        /// Make codelet inert, until another process completes it
        /// </summary>
        public void Deactivate() {
            AdjustSalience(-salience);
        }

        /// <summary> 
        /// Adjust the amount of time, space, salience we're allotted
        /// </summary> 
        public void AdjustTime(int delta) {
            time += delta;
        }

        public void AdjustSalience(double delta) {
            coderack.ChangeSalience(this, salience, salience + delta);
            salience += delta;
        }

#region IFastSerializable Members

        public override void Deserialize(SerializationReader reader) {
            coderack = (Coderack) reader.ReadPointer();    // coderack = CType(info.GetValue("coderack", GetType(Coderack)), Coderack)
            salience = reader.ReadDouble();  // salience = info.GetDouble("salience")
            space = reader.ReadInt32();  // space = info.GetInt32("space")
            time = reader.ReadInt32();   // time = info.GetInt32("time")
            reader.ReadDictionary(children); // children = CType(info.GetValue("children", GetType(Dictionary(Of String, Codelet))), Dictionary(Of String, Codelet))
            watched = reader.ReadBoolean();  // watched = info.GetBoolean("watched")
            immune = reader.ReadBoolean();   // immune = info.GetBoolean("immune")
        }

        public override void Serialize(SerializationWriter writer) {
            writer.WritePointer(coderack);   //info.AddValue("coderack", coderack)
            writer.Write(salience);  // info.AddValue("salience", salience)
            writer.Write(space); // info.AddValue("space", space)
            writer.Write(time);  // info.AddValue("time", time)
            writer.Write(children);  // info.AddValue("children", children)
            writer.Write(watched);   // info.AddValue("watched", watched)
            writer.Write(immune);    // info.AddValue("immune", immune)
        }

#endregion

#endregion

    }
}
