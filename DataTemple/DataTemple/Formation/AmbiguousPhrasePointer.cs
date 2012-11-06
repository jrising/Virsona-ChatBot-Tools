using System;
namespace DataTemple
{
	public class AmbiguousPhrasePointer
	{
		public int stringTokenIndex;
		
		public AmbiguousPhrasePointer(int stringTokenIndex)
		{
			this.stringTokenIndex = stringTokenIndex;
		}
		
		public int StringTokenIndex {
			get {
				return stringTokenIndex;
			}
		}
		
		public AmbiguousPhrasePointer PointerFurtheredBy(int count) {
			return new AmbiguousPhrasePointer(stringTokenIndex + count);
		}
		
		public override bool Equals(object obj)
		{
			if (obj is AmbiguousPhrasePointer)
				return ((AmbiguousPhrasePointer) obj).stringTokenIndex == stringTokenIndex;
			return base.Equals(obj);
		}
		
		public override int GetHashCode()
		{
			return stringTokenIndex;
		}
	}
}

