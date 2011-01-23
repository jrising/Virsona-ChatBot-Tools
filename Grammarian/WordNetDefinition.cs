/******************************************************************\
 *      Class Name:     WordNetDefinition
 *      Written By:     James Rising (borrowed from online source)
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * Entry from a WordNet definition file.
\******************************************************************/
using System.Collections.Generic;

namespace LanguageNet.Grammarian
{
	public class WordNetDefinition
	{
		#region Member Variables

		private long mPosition;		/* current file position */
		private int mDefinitionType;			/* type of ADJ definition */
		private int mFileNumber;			/* file number that definition comes from */
		private string mPartOfSpeech;			/* part of speech */
		private int mWordCount;			/* number of words in definition */
		private List<string> mWords = new List<string>();		/* words in definition */
		private int mLexId;			/* unique id in lexicographer file */
		private List<int> mSenseNumbers = new List<int>();			/* sense number in wordnet */
		private int mWhichWord;		/* which word in definition we're looking for */
		private int mPtrCount;		/* number of pointers */
		private List<int> mPtrTypes = new List<int>();		/* pointer types */
		private List<long> mPtrOffsets = new List<long>();		/* pointer offsets */
		private List<int> mPtrPartOfSpeech = new List<int>();			/* pointer part of speech */
		private List<int> mPtrToFields = new List<int>();			/* pointer 'to' fields */
		private List<int> mPtrFromFields = new List<int>();			/* pointer 'from' fields */
		private int mVerbFrameCount;			/* number of verb frames */
		private List<int> mFrameIds = new List<int>();			/* frame numbers */
		private List<int> mFrameToFields = new List<int>();			/* frame 'to' fields */
		private string mDefinitionText;			/* definition gloss (definition) */
		private uint mKey;		/* unique definition key */

		/* these fields are used if a data structure is returned instead of a text buffer */
		private WordNetDefinition mNextDefinition;		/* ptr to next definition containing searchword */
		private WordNetDefinition mNextForm;	/* ptr to list of definitions for alternate spelling of wordform */
		private int mSearchType;		/* type of search performed */
		private WordNetDefinition mPtrList;		/* ptr to definition list result of search */
		private string mHeadWord;		/* if pos is "s", this is cluster head word */
		private short mHeadSense;		/* sense number of headword */

		#endregion Member Variables

		#region Properties

		#region DisplayPartOfSpeech
		/// <summary>
		/// Gets the POS (Part of Speech) suitable for display
		/// </summary>
		public string DisplayPartOfSpeech
		{
			get
			{
				string retVal = string.Empty;
				if( PtrPartOfSpeech.Count > 0 )
				{
					retVal = ( ( WordNetAccess.PartOfSpeech )PtrPartOfSpeech[0] ).ToString();
				}
				/*else
				{
					retVal = FileParser.GetSynSetTypeCode( PartOfSpeech ).ToString();
				}*/
				return retVal;
			}
		}
		#endregion DisplayPartOfSpeech

		#region Position
		///<summary>
		/// current file position
		///</summary>
		public long Position { get { return mPosition; } set { mPosition = value; } }
		#endregion Position

		#region DefinitionType
		///<summary>
		/// type of ADJ definition
		///</summary>
		public int DefinitionType { get { return mDefinitionType; } set { mDefinitionType = value; } }
		#endregion DefinitionType

		#region FileNumber
		///<summary>
		/// file number that definition comes from
		///</summary>
		public int FileNumber { get { return mFileNumber; } set { mFileNumber = value; } }
		#endregion FileNumber

		#region PartOfSpeech
		///<summary>
		/// part of speech
		///</summary>
		public string PartOfSpeech { get { return mPartOfSpeech; } set { mPartOfSpeech = value; } }
		#endregion PartOfSpeech

		#region WordCount
		///<summary>
		/// number of words in definition
		///</summary>
		public int WordCount { get { return mWordCount; } set { mWordCount = value; } }
		#endregion WordCount

		#region Words
		///<summary>
		/// words in definition
		///</summary>
		public List<string> Words { get { return mWords; } set { mWords = value; } }
		#endregion Words

		#region LexId
		///<summary>
		/// unique id in lexicographer file
		///</summary>
		public int LexId { get { return mLexId; } set { mLexId = value; } }
		#endregion LexId

		#region SenseNumbers
		///<summary>
		/// sense number in wordnet
		///</summary>
		public List<int> SenseNumbers { get { return mSenseNumbers; } set { mSenseNumbers = value; } }
		#endregion SenseNumbers

		#region WhichWord
		///<summary>
		/// which word in definition we're looking for
		///</summary>
		public int WhichWord { get { return mWhichWord; } set { mWhichWord = value; } }
		#endregion WhichWord

		#region PtrCount
		///<summary>
		/// number of pointers
		///</summary>
		public int PtrCount { get { return mPtrCount; } set { mPtrCount = value; } }
		#endregion PtrCount

		#region PtrTypes
		///<summary>
		/// pointer types
		///</summary>
		public List<int> PtrTypes { get { return mPtrTypes; } set { mPtrTypes = value; } }
		#endregion PtrTypes

		#region PtrOffset
		///<summary>
		/// pointer offsets
		///</summary>
		public List<long> PtrOffsets { get { return mPtrOffsets; } set { mPtrOffsets = value; } }
		#endregion PtrOffset

		#region PtrPartOfSpeech
		///<summary>
		/// pointer part of speech
		///</summary>
		public List<int> PtrPartOfSpeech { get { return mPtrPartOfSpeech; } set { mPtrPartOfSpeech = value; } }
		#endregion PtrPartOfSpeech

		#region PtrToFields
		///<summary>
		/// pointer 'to' fields
		///</summary>
		public List<int> PtrToFields { get { return mPtrToFields; } set { mPtrToFields = value; } }
		#endregion PtrToFields

		#region PtrFromFields
		///<summary>
		/// pointer 'from' fields
		///</summary>
		public List<int> PtrFromFields { get { return mPtrFromFields; } set { mPtrFromFields = value; } }
		#endregion PtrFromFields

		#region VerbFrameCount
		///<summary>
		/// number of verb frames
		///</summary>
		public int VerbFrameCount { get { return mVerbFrameCount; } set { mVerbFrameCount = value; } }
		#endregion VerbFrameCount

		#region FrameIds
		///<summary>
		/// frame numbers
		///</summary>
		public List<int> FrameIds { get { return mFrameIds; } set { mFrameIds = value; } }
		#endregion FrameIds

		#region FrameToFields
		///<summary>
		/// frame 'to' fields
		///</summary>
		public List<int> FrameToFields { get { return mFrameToFields; } set { mFrameToFields = value; } }
		#endregion FrameToFields

		#region DefinitionText
		///<summary>
		/// definition gloss (definition)
		///</summary>
		public string DefinitionText { get { return mDefinitionText; } set { mDefinitionText = value; } }
		#endregion DefinitionText

		#region Key
		///<summary>
		/// unique definition key
		///</summary>
		public uint Key { get { return mKey; } set { mKey = value; } }
		#endregion Key


		/* these fields are used if a data structure is returned instead of a text buffer */
		#region NextDefinition
		///<summary>
		/// ptr to next definition containing searchword
		///</summary>
		public WordNetDefinition NextDefinition { get { return mNextDefinition; } set { mNextDefinition = value; } }
		#endregion NextDefinition

		#region NextForm
		///<summary>
		/// ptr to list of definitions for alternate spelling of wordform
		///</summary>
		public WordNetDefinition NextForm { get { return mNextForm; } set { mNextForm = value; } }
		#endregion NextForm

		#region SearchType
		///<summary>
		/// type of search performed
		///</summary>
		public int SearchType { get { return mSearchType; } set { mSearchType = value; } }
		#endregion SearchType

		#region PtrList
		///<summary>
		/// ptr to definition list result of search
		///</summary>
		public WordNetDefinition PtrList { get { return mPtrList; } set { mPtrList = value; } }
		#endregion PtrList

		#region HeadWord
		///<summary>
		/// if pos is "s", this is cluster head word
		///</summary>
		public string HeadWord { get { return mHeadWord; } set { mHeadWord = value; } }
		#endregion HeadWord

		#region HeadSense
		///<summary>
		/// sense number of headword
		///</summary>
		public short HeadSense { get { return mHeadSense; } set { mHeadSense = value; } }
		#endregion HeadSense

		#endregion Properties
	}
}
