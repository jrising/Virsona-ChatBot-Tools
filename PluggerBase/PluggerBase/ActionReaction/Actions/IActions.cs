/******************************************************************\
 *      Class Name:     IAction
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * Actions are ICallables with a define interface (IArgumentTypes)
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using PluggerBase.ActionReaction.Evaluations;
using PluggerBase.ActionReaction.Interfaces;

namespace PluggerBase.ActionReaction.Actions
{
    public interface IAction : ICallable
    {
        IArgumentType Input { get; }
        IArgumentType Output { get; }
    }
}
