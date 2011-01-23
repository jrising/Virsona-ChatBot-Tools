/******************************************************************\
 *      Class Name:     IArena
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * Interface for arenas-- any place where evaluables, callables,
 * continuations, and failures can do their work.
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using PluggerBase.FastSerializer;

namespace PluggerBase.ActionReaction.Evaluations
{
    public interface IArena : IFastSerializable
    {
        int Evaluate(IEvaluable evaluable, double salience);

        int Call(ICallable callable, double salience, object value, IContinuation succ, IFailure fail);
        // Clone the continuation on calling, so we can handle multiple calls
        int Continue(IContinuation cont, double salience, object value, IFailure fail);
        // Clone the failure on calling, so we can handle multiple calls
        int Fail(IFailure fail, double salience, string reason, IContinuation skip);
    }
}
