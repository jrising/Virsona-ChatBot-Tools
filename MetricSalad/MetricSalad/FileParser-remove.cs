using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MetricSalad
{
	public class FileParser
	{
        #region FastSearch
        /// <summary>
		/// Searches the specified file for the specified keyword
		/// </summary>
		/// <param name="keyword">The keyword to find</param>
		/// <param name="dbFileName">The full path to the file to search in</param>
		/// <returns>The offset in the file at which the word was found; otherwise 0</returns>
		internal static long FastSearch(string keyword, string dbFileName, char[] tokenizer)
		{
			long retVal = 0L;
			string key = string.Empty;
			Encoding enc = Encoding.Default;

			using( StreamReader reader = new StreamReader( dbFileName, true ) )
			{
				enc = reader.CurrentEncoding;
				reader.Close();
			}

			using( FileStream fs = File.OpenRead( dbFileName ) )
			{
				long diff = 666;
				string line = string.Empty;

				fs.Seek( 0, SeekOrigin.End );
				long top = 0;
				long bottom = fs.Position;
				long mid = ( bottom - top ) / 2;

				do
				{
					fs.Seek( mid - 1, SeekOrigin.Begin );
					if( mid != 1 )
					{
						while( fs.ReadByte() != '\n' && fs.Position < fs.Length )
						{
							retVal = fs.Position;
						}
					}

					byte[] btData = new byte[ Constants.KEY_LEN ];
					int count = fs.Read( btData, 0, btData.Length );
					fs.Seek( fs.Position - count, SeekOrigin.Begin );

					string readData = enc.GetString( btData );
                    if (readData[0] == ' ')
                    {
                        // oops-- too far!
                        bottom = mid;
                        diff = (bottom - top) / 2;
                        mid = top + diff;
                    }
                    else
                    {
                        key = readData.TrimStart(tokenizer).Split(tokenizer)[0];
                        int comparison = string.Compare(key, keyword, StringComparison.Ordinal);

                        if (comparison != 0)
                        {
                            if (comparison > 0)
                            {
                                bottom = mid;
                                diff = (bottom - top) / 2;
                                mid = top + diff;
                            }

                            if (comparison < 0)
                            {
                                top = mid;
                                diff = (bottom - top) / 2;
                                mid = top + diff;
                            }
                        }
                    }
				}
                while (string.Compare(key, keyword, StringComparison.Ordinal) != 0 && diff != 0);
			}

            if (string.Compare(key, keyword, StringComparison.Ordinal) != 0)
				retVal = 0L;
			else
				retVal++;

			return retVal;
		}
		#endregion FastSearch
		
        #region ReadRecord
        /// <summary>
		/// Reads the record at the specified offset from the specified file
		/// </summary>
		/// <param name="offset">The offset (record id) to read</param>
		/// <param name="dbFileName">The full path of the file to read from</param>
		/// <returns>The record as a string is successfull; otherwise an empty string</returns>
		internal static string ReadRecord( long offset, string dbFileName )
		{
			string retVal = string.Empty;

            using( StreamReader reader = new StreamReader( dbFileName, true ) )
			{
				reader.BaseStream.Seek( offset, SeekOrigin.Begin );
                retVal = reader.ReadLine();
				reader.Close();
			}

			return retVal;
        }
        #endregion ReadRecord

		
		#region EncodeWord
		/// <summary>
		/// Converts a compound word/phrase into the recognized format
		/// </summary>
		/// <param name="data">The text to convert</param>
		/// <returns>A normalized string</returns>
		public static string EncodeWord( string data )
		{
			string retVal = string.Empty;
			retVal = data.Replace( ' ', '_' );
			return retVal.ToLower();
		}
		#endregion EncodeWord

		#region DecodeWord
		/// <summary>
		/// Converts a normalized string into a presentable format
		/// </summary>
		/// <param name="data">The normalized string</param>
		/// <returns>A presentable format string</returns>
		public static string DecodeWord( string data )
		{
			string retVal = string.Empty;
			retVal = data.Replace( '_', ' ' );
			return retVal;
		}
		#endregion DecodeWord
	}
}

