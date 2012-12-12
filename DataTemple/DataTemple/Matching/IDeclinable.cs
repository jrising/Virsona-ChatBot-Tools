using System;
using System.Collections.Generic;
using System.Text;

namespace DataTemple.Matching
{
	public interface IDeclination
	{
		bool IsInDeclination(object value);
		object Decline(object value);
	}
}
