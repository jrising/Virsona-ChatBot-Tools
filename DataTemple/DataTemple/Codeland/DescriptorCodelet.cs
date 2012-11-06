/******************************************************************\
 *      Class Name:     DescriptorCodelet
 *      Written By:     James.R
 *      Copyright:      Virsona Inc
 *      -----------------------------------------------------------
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace DataTemple.Codeland
{
    /// <summary> 
    /// A simple named description to go with this codelet
    /// </summary> 
    public class DescriptorCodelet : Codelet
    {
        public readonly string Name;    // Any name

        public DescriptorCodelet(Coderack c, string n)
            : base(c, 0, (int)(n.Length / 4 + 1), 0)
        {
            Name = n;
        }
    }
}
