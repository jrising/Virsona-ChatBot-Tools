/******************************************************************\
 *      Class Names:    IAgent, AgentBase
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * Basic interface for agents (which live in arenas), and a base
 * class which ensures a value for the internal arena
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using PluggerBase.FastSerializer;

namespace PluggerBase.ActionReaction.Evaluations
{
    // An IAgent is anything that can be stored in an arena
    //   The "do it" functions are different for different kinds (and not even necessary),
    //   and for each of developing IAgents are handled all in IArena
    public interface IAgent : IFastSerializable
    {
        int Initialize(IArena arena, double salience); // returns the expected time needs
        bool Complete();
    }

    public class AgentBase : IAgent
    {
        protected IArena arena;
        protected double salience;

        public AgentBase()
        {
            arena = new ImmediateArena();
            salience = 1.0;
        }
		
		public double Salience {
			get {
				return salience;
			}
		}

        #region IAgent Members

        public virtual int Initialize(IArena arena, double salience)
        {
            this.arena = arena;
            this.salience = salience;
            return 1;
        }

        public virtual bool Complete()
        {
            return true;
        }

        #endregion

        #region IFastSerializable Members

        public virtual void Deserialize(SerializationReader reader)
        {
            arena = (IArena)reader.ReadPointer();
            salience = reader.ReadDouble();
        }

        public virtual void Serialize(SerializationWriter writer)
        {
            writer.WritePointer(arena);
            writer.Write(salience);
        }

        #endregion
    }
}
