using System;

namespace Hashi.Bson
{
    public enum BsonValueType
    {
        Invalid = -1,
        EOD = 0,
        Double = 1,
        String = 2,
        Object = 3,
        Array = 4,
        Boolean = 8,
        Null = 10,
        Int32 = 16,
        Int64 = 18
    }

    public class BsonObjectKeyNotFoundException : Exception
    {
    }
    
    public class BsonArrayOutOfIndexException : Exception
    {
    }
    
    public class BsonInvalidValueTypeException : Exception
    {
    }

    public class BsonNullValueReferenceException : Exception
    {
    }

    public class BsonDeserializationException : Exception
    {
    }

	public class BsonTextParserException : Exception
	{
	}
}

