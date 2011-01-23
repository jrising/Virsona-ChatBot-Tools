/******************************************************************\
 *      Class Name:     MemorySource
 *      Written By:     James Rising
 *      Copyright:      2011, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * A source that produces data entered into it, and stores the data
 * in the memory
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using PluggerBase;

namespace GenericTools.DataSources
{
    public class MemorySource<TKey, TValue> : Dictionary<TKey, TValue>, IDataSource<TKey, TValue>
    {
    }
}
