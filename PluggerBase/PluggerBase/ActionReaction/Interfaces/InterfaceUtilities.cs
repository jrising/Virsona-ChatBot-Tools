/******************************************************************\
 *      Class Name:     InterfaceUtilities
 *      Written By:     James.R
 *      Copyright:      Virsona Inc
 *      -----------------------------------------------------------
 * A collection of classes to support the interface system
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace PluggerBase.ActionReaction.Interfaces
{
    public interface IProgressUpdatingAction : IAction
    {
        void SetNotify(InterfaceUtilities.ProgressNotify notify);
    }

    public interface IMessageReceiverAction : IAction
    {
        void SetReceiver(IMessageReceiver receiver);
    }

    public class InterfaceUtilities
    {
        // return false to abort!
        public delegate bool ProgressNotify(double value);
    }
}
