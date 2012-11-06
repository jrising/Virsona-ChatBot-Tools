using System;
using System.Collections.Generic;
using System.Text;

namespace DataTemple.Matching
{
    public interface IDeclinable
    {
        // Decline this object into an instance of into, or null if impossible
        object Decline(IDeclinable into);
        // Return a copy of this object, associated with the given suffix,
        //   so that on setting/propogation/declination, that suffix is set
        IDeclinable Associate(string name);
    }
}
