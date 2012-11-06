using System;
using System.Collections.Generic;
using System.Text;

namespace DataTemple.Codeland
{
    public interface IResultReceiver<T>
    {
        void SetResult(T result, double weight);
    }
}
