/*********************************************************************************
 * Created: 8/5/2008
 * by Waleed A. Zaghloul
 * 
 * This dll allows for the use of feature reduction on any given strings.
 * 
 * *******************************************************************************/

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;

namespace FeatureReducer
{
    [ClassInterface(ClassInterfaceType.None)]
    public class FeatureReduction //FRInterface
    {
		protected string datadir;
		
        public FeatureReduction(string datadir)
        {
			this.datadir = datadir;
        }

        public bool isStopWord(string s)
        {
            s = s.ToLower();

            String[] stopWords = File.ReadAllLines(Path.Combine(datadir, "Stop words list.txt"));

            for (int j = 0; j < stopWords.Length; j++)
            {
                if (stopWords[j].Length != 0)
                {
                    if (stopWords[j] == s)
                        return true;
                }
            }

            return false;
        }
        
        public string reduceFeatures(string s)
        {
            String[] puncList = File.ReadAllLines(Path.Combine(datadir, "Punc_List.txt"));
            s = " " + s;
            s = s.ToLower();
            for (int i = 0; i < puncList.Length; i++)
            {
                if (puncList[i].Length != 0)
                {
                    s = s.Replace(puncList[i], " ");
                }
            }

            // clean up the "s" left over from examples like "people's"
            s = s.Replace(" s ", " ");

            String[] stopWords = File.ReadAllLines(Path.Combine(datadir, "Stop words list.txt"));

            for (int j = 0; j < stopWords.Length; j++)
            {
                if (stopWords[j].Length != 0)
                {
                    s = s.Replace(" " + stopWords[j] + " ", " ");
                }
            }
            return (s);
        }
    }
}