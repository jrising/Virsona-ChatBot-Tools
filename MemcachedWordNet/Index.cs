/******************************************************************\
 *      Class Name:     Index
 *      Written By:     James Rising (borrowed from online source)
 *      Copyright:      2011, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * An Index contains a single entry in a WordNet index file.  The
 * most important pieces of information are the SynSetOffsets.
\******************************************************************/
using System.Collections.Generic;
using System;
using LanguageNet.Grammarian;

namespace LanguageNet.WordNet
{
    [Serializable]
	public struct Index
	{
		#region Member Variables

		private long mIdxOffset;		/* byte offset of entry in index file */
		private string mWord;			/* word string */
		private string mPartOfSpech;			/* part of speech */
		private int mSenseCount;		/* sense (collins) count */
		private int mOffsetCount;		/* number of offsets */
		private int mTaggedSensesCount;		/* number senses that are tagged */
		private List<long> mSynSetsOffsets;	/* offsets of synsets containing word */
		private int mPointersUsedCount;		/* number of pointers used */
		private List<int> mPointersUsed;		/* pointers used */

		#endregion Member Variables

		#region Properties

		#region Empty
		/// <summary>
		/// Get an empty index structure
		/// </summary>
		public static Index Empty { get { return new Index(); } }
		#endregion Empty
		
		public override bool Equals(object obj) {
			if (!(obj is Index))
				return false;
			Index other = (Index) obj;
			if (mIdxOffset != other.mIdxOffset || !mWord.Equals(other.mWord) || !mPartOfSpech.Equals(other.mPartOfSpech) ||
			    mSenseCount != other.mSenseCount || mOffsetCount != other.mOffsetCount || mTaggedSensesCount != other.mTaggedSensesCount ||
			    mPointersUsedCount != other.mPointersUsedCount || mSynSetsOffsets.Count != other.mSynSetsOffsets.Count)
				return false;
			for (int ii = 0; ii < mSynSetsOffsets.Count; ii++)
				if (mSynSetsOffsets[ii] != other.mSynSetsOffsets[ii])
					return false;
			for (int ii = 0; ii < mPointersUsed.Count; ii++)
				if (mPointersUsed[ii] != other.mPointersUsed[ii])
					return false;
			return true;
		}
		
		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}
		
		#region IdxOffset
		/// <summary>
		/// byte offset of entry in index file
		/// </summary>
		public long IdxOffset { get { return mIdxOffset; } set { mIdxOffset = value; } }
		#endregion IdxOffset

		#region Word
		/// <summary>
		/// word string
		/// </summary>
		public string Word { get { return mWord; } set { mWord = value; } }
		#endregion Word

		#region PartOfSpech
		/// <summary>
		/// part of speech
		/// </summary>
		public string PartOfSpech { get { return mPartOfSpech; } set { mPartOfSpech = value; } }
		#endregion PartOfSpech

        #region DbPartOfSpeech
        /// <summary>
        /// part of speech as the enum
        /// </summary>
        public WordNetAccess.PartOfSpeech DbPartOfSpeech
        {
            get {
                if (mPartOfSpech == "n")
                    return WordNetAccess.PartOfSpeech.Noun;
                if (mPartOfSpech == "v")
                    return WordNetAccess.PartOfSpeech.Verb;
                if (mPartOfSpech == "a")
                    return WordNetAccess.PartOfSpeech.Adj;
                if (mPartOfSpech == "r")
                    return WordNetAccess.PartOfSpeech.Adv;

                return WordNetAccess.PartOfSpeech.Noun;    // unknown!
            }
        }
        #endregion DbPartOfSpeech


        #region SenseCount
        /// <summary>
		/// sense (collins) count
		/// </summary>
		public int SenseCount { get { return mSenseCount; } set { mSenseCount = value; } }
		#endregion SenseCount

		#region OffsetCount
		/// <summary>
		/// number of offsets
		/// </summary>
		public int OffsetCount { get { return mOffsetCount; } set { mOffsetCount = value; } }
		#endregion OffsetCount

		#region TaggedSensesCount
		/// <summary>
		/// number senses that are tagged
		/// </summary>
		public int TaggedSensesCount { get { return mTaggedSensesCount; } set { mTaggedSensesCount = value; } }
		#endregion TaggedSensesCount

		#region SynSetsOffsets
		/// <summary>
		/// offsets of synsets containing word
		/// </summary>
		public List<long> SynSetsOffsets { get { return mSynSetsOffsets; } set { mSynSetsOffsets = value; } }
		#endregion SynSetsOffsets

		#region PointersUsedCount
		/// <summary>
		/// number of pointers used
		/// </summary>
		public int PointersUsedCount { get { return mPointersUsedCount; } set { mPointersUsedCount = value; } }
		#endregion PointersUsedCount

		#region PointersUsed
		/// <summary>
		/// pointers used
		/// </summary>
		public List<int> PointersUsed { get { return mPointersUsed; } set { mPointersUsed = value; } }
		#endregion PointersUsed

		#endregion Properties
	}
}
