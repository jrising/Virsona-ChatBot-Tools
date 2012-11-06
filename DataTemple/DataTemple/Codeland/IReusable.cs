/******************************************************************\
 *      Class Name:     IReusable
 *      Written By:     James.R
 *      Copyright:      Virsona Inc
 *      -----------------------------------------------------------
 * Object that can be reused by ReuseHeap
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace GenericTools
{
    /*
     * Implemented by classes that can be stored in a ReuseHeap
     */
    public interface IReusable
    {
        void Reset();
    }
}
