/******************************************************************\
 *      Class Name:     ISalienceSet
 *      Written By:     James.R
 *      Copyright:      Virsona Inc
 *
 *      Modifications:
 *      -----------------------------------------------------------
 *      Date            Author          Modification
 *
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using PluggerBase.FastSerializer;

namespace DataTemple.Codeland.SearchTree
{
    public interface ISalienceSet<DataType> : IDictionary<double, DataType>, IFastSerializable
    {
        double SalienceTotal
        {
            get;
        }

        void ChangeSalience(DataType obj, double before, double after);

        DataType SelectRandomItem(RandomSearchQuality quality);
        DataType SelectSalientItem(RandomSearchQuality quality);

        // search is slow-- avoid it!
        //double Search(DataType obj);
        // remove calls obj.Dispose
        bool Remove(double key, DataType obj);

        double GetMinKey();
        double GetMaxKey();
    }
}
