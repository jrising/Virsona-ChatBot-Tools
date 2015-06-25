/******************************************************************\
 *      Class Name:     IMessageReceiver
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * Anything that can take messages
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace PluggerBase
{
    /*
     * Implemented by classes able to receive messages
     */
    public interface IMessageReceiver
    {
        bool Receive(string message, object reference);
    }
}
	