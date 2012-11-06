/******************************************************************\
 *      Class Name:     RedBlackException
 *      Written By:     James.R
 *      Copyright:      Virsona Inc
 *
 *      Modifications:
 *      -----------------------------------------------------------
 *      Date            Author          Modification
 *
\******************************************************************/
///<summary>
/// The RedBlackException class distinguishes read black tree exceptions from .NET
/// exceptions. 
///</summary>
using System;

namespace DataTemple.Codeland.SearchTree
{
	public class RedBlackException : Exception
	{
		public RedBlackException()
        {
    	}
			
		public RedBlackException(string msg) : base(msg) 
        {
		}			
	}
}
