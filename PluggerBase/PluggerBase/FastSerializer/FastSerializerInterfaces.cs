/******************************************************************\
 *      Class Name:     FastSerializerInterfaces
 *        http://www.codeproject.com/KB/cs/FastSerialization.aspx
 *      Copyright:      2006, Tim Haynes
 *                      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 * 
 *                      Original written by Tim Haynes
 *                      Modified by James Rising
 *      -----------------------------------------------------------
 * Utility classes for the FastSerialization system
\******************************************************************/
// Remove the DEBUG condition if you want to always check for not optimizable values at the small expense of runtime speed
#if DEBUG
#define THROW_IF_NOT_OPTIMIZABLE
#endif

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Collections.Generic;

namespace PluggerBase.FastSerializer
{

	/// <summary>
	/// Exception thrown when a value being optimized does not meet the required criteria for optimization.
	/// </summary>
	public class OptimizationException: Exception
	{
		public OptimizationException(string message): base(message) {}
	}
	
	/// <summary>
	/// Allows a class to specify that it can be recreated during deserialization using a default constructor
	/// and then calling DeserializeOwnedData()
	/// </summary>
	public interface IOwnedDataSerializableAndRecreatable: IOwnedDataSerializable {}

	/// <summary>
	/// Allows a class to save/retrieve their internal data to/from an existing SerializationWriter/SerializationReader.
	/// </summary>
	public interface IOwnedDataSerializable
	{
		/// <summary>
		/// Lets the implementing class store internal data directly into a SerializationWriter.
		/// </summary>
		/// <param name="writer">The SerializationWriter to use</param>
		/// <param name="context">Optional context to use as a hint as to what to store (BitVector32 is useful)</param>
		void SerializeOwnedData(SerializationWriter writer, object context);

		/// <summary>
		/// Lets the implementing class retrieve internal data directly from a SerializationReader.
		/// </summary>
		/// <param name="reader">The SerializationReader to use</param>
		/// <param name="context">Optional context to use as a hint as to what to retrieve (BitVector32 is useful) </param>
		void DeserializeOwnedData(SerializationReader reader, object context);
	}

	/// <summary>
	/// Interface to allow helper classes to be used to serialize objects
	/// that are not directly supported by SerializationWriter/SerializationReader
	/// </summary>
	public interface IFastSerializationTypeSurrogate
	{
		/// <summary>
		/// Allows a surrogate to be queried as to whether a particular type is supported
		/// </summary>
		/// <param name="type">The type being queried</param>
		/// <returns>true if the type is supported; otherwise false</returns>
		bool SupportsType(Type type);
		/// <summary>
		/// FastSerializes the object into the SerializationWriter.
		/// </summary>
		/// <param name="writer">The SerializationWriter into which the object is to be serialized.</param>
		/// <param name="value">The object to serialize.</param>
		void Serialize(SerializationWriter writer, object value);
		/// <summary>
		/// Deserializes an object of the supplied type from the SerializationReader.
		/// </summary>
		/// <param name="reader">The SerializationReader containing the serialized object.</param>
		/// <param name="type">The type of object required to be deserialized.</param>
		/// <returns></returns>
		object Deserialize(SerializationReader reader, Type type);
	}

	/// <summary>
	/// Stores information about a type or type/value.
	/// Internal use only.
	/// </summary>
	internal enum SerializedType: byte
	{
		// Codes 0 to 127 reserved for String token tables
		
		NullType = 128,            // Used for all null values
		NullSequenceType,          // Used internally to identify sequences of null values in object[]
		DBNullType,                // Used for DBNull.Value
		DBNullSequenceType,        // Used internally to identify sequences of DBNull.Value values in object[] (DataSets)
		OtherType,                 // Used for any unrecognized types - uses an internal BinaryWriter/Reader.

		BooleanTrueType,           // Stores Boolean type and values
		BooleanFalseType,

		ByteType,                  // Standard numeric value types
		SByteType,
		CharType,
		DecimalType,
		DoubleType,
		SingleType,
		Int16Type,
		Int32Type,
		Int64Type,
		UInt16Type,
		UInt32Type,
		UInt64Type,

		ZeroByteType,              // Optimization to store type and a zero value - all numeric value types
		ZeroSByteType,
		ZeroCharType,
		ZeroDecimalType,
		ZeroDoubleType,
		ZeroSingleType,
		ZeroInt16Type,
		ZeroInt32Type,
		ZeroInt64Type,
		ZeroUInt16Type,
		ZeroUInt32Type,
		ZeroUInt64Type,

		OneByteType,               // Optimization to store type and a one value - all numeric value types
		OneSByteType,
		OneCharType,
		OneDecimalType,
		OneDoubleType,
		OneSingleType,
		OneInt16Type,
		OneInt32Type,
		OneInt64Type,
		OneUInt16Type,
		OneUInt32Type,
		OneUInt64Type,

		MinusOneInt16Type,         // Optimization to store type and a minus one value - Signed Integer types only
		MinusOneInt32Type,
		MinusOneInt64Type,

		OptimizedInt16Type,        // Optimizations for specific value types
		OptimizedInt16NegativeType,
		OptimizedUInt16Type,
		OptimizedInt32Type,
		OptimizedInt32NegativeType,
		OptimizedUInt32Type,
		OptimizedInt64Type,
		OptimizedInt64NegativeType,
		OptimizedUInt64Type,
		OptimizedDateTimeType,
		OptimizedTimeSpanType,


		EmptyStringType,           // String type and optimizations
		SingleSpaceType,
		SingleCharStringType,
		YStringType,
		NStringType,

		DateTimeType,              // Date type and optimizations
		MinDateTimeType,
		MaxDateTimeType,

		TimeSpanType,              // TimeSpan type and optimizations
		ZeroTimeSpanType,

		GuidType,                  // Guid type and optimizations
		EmptyGuidType,

		BitVector32Type,           // Specific optimization for BitVector32 type

		DuplicateValueType,        // Used internally by Optimized object[] pair to identify values in the 
		                           // second array that are identical to those in the first
		DuplicateValueSequenceType,
		
		BitArrayType,              // Specific optimization for BitArray

		TypeType,                  // Identifies a Type type 

		SingleInstanceType,        // Used internally to identify that a single instance object should be created
		                           // (by storing the Type and using Activator.GetInstance() at deserialization time)

		ArrayListType,             // Specific optimization for ArrayList type


		ObjectArrayType,           // Array types
		EmptyTypedArrayType,
		EmptyObjectArrayType,		

		NonOptimizedTypedArrayType, // Identifies a typed array and how it is optimized
		FullyOptimizedTypedArrayType,
		PartiallyOptimizedTypedArrayType,
		OtherTypedArrayType,
		
		BooleanArrayType,
		ByteArrayType,
		CharArrayType,
		DateTimeArrayType,
		DecimalArrayType,
		DoubleArrayType,
		SingleArrayType,
		GuidArrayType,
		Int16ArrayType,
		Int32ArrayType,
		Int64ArrayType,
		SByteArrayType,
		TimeSpanArrayType,
		UInt16ArrayType,
		UInt32ArrayType,
		UInt64ArrayType,
		StringArrayType,

		OwnedDataSerializableAndRecreatableType,
		
		EnumType,
		OptimizedEnumType,

		SurrogateHandledType,
        FastSerializableType,
        FastSerializableTypeType,

        PointerType,                // for where pointers may or may not be used

        // Placeholders to indicate number of Type Codes remaining
        Reserved21,
        Reserved20,
		Reserved19,
		Reserved18,
		Reserved17,
		Reserved16,
		Reserved15,
		Reserved14,
		Reserved13,
		Reserved12,
		Reserved11,
		Reserved10,
		Reserved9,
		Reserved8,
		Reserved7,
		Reserved6,
		Reserved5,
		Reserved4,
		Reserved3,
		Reserved2,
		Reserved1
	}
}
