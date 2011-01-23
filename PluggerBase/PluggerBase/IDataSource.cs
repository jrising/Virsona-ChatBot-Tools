/******************************************************************\
 *      Class Name:     IDataSource
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * An abstraction around how to get structured data
 * An IDataSource will produce a stream of data
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace PluggerBase
{
    public interface IDataSource
    {
    }

    public interface IDataSource<TKey, TValue> : IDataSource, IEnumerable<KeyValuePair<TKey, TValue>>
    {
        /// <summary>
        /// Get a single value from a source; returns default(TValue) if not found
        /// </summary>
        bool TryGetValue(TKey key, out TValue value);
    }
}
