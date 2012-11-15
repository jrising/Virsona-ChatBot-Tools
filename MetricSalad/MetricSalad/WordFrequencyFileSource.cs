using System;
using GenericTools;
using GenericTools.DataSources;

namespace MetricSalad
{
	public class WordFrequencyFileSource : AlphabeticFile<ThreeTuple<uint, double, double>>
	{
		public static char[] delimiter = new char[] {','};
		
        public WordFrequencyFileSource(string filename)
            : base(filename, delimiter, 20)
        {
        }
		
		public override ThreeTuple<uint, double, double> ReadStringEntry(string line)
		{
            string[] cols = line.Split(delimiter);
			uint occs = uint.Parse(cols[1]);
			double freq = double.Parse(cols[2]);
			double weight = double.Parse(cols[3]);
			return new ThreeTuple<uint, double, double>(occs, freq, weight);
		}
	}
}