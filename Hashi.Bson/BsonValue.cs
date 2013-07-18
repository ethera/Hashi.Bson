using System;
using System.IO;
using System.Text;

namespace Hashi.Bson
{
    public class BsonValue
    {
        object mValue;
        
        public BsonValue(object value)
        {
            mValue = value;     
        }
        
        public BsonValueType Type
        {
            get
            {
                if (mValue == null)
                    return BsonValueType.Null;

                return ToBsonValueType(mValue.GetType());
            }
        }
        
        public static BsonValueType ToBsonValueType(Type type)
        {
            if (type == typeof(String))
                return BsonValueType.String;
            if (type == typeof(int))
                return BsonValueType.Int32;
            if (type == typeof(long))
                return BsonValueType.Int64;
            if (type == typeof(double))
                return BsonValueType.Double;
            if (type == typeof(bool))
                return BsonValueType.Boolean;
            if (type == typeof(BsonObject))
                return BsonValueType.Object;
            if (type == typeof(BsonArray))
                return BsonValueType.Array;

            return BsonValueType.Invalid;
        }

        public object Value { get { return mValue; } }

        public T To<T>()
        {
            if (mValue == null)
                throw new BsonNullValueReferenceException();

            BsonValueType type = ToBsonValueType(typeof(T));
            if (type != Type)
            {
                if (Type == BsonValueType.Double && type == BsonValueType.Int32)
                    return (T)(object)(int)Math.Truncate((double)mValue);
                    
                if (Type == BsonValueType.Double && type == BsonValueType.Int64)
                    return (T)(object)(long)Math.Truncate((double)mValue);

                if (Type == BsonValueType.Int64 && (long)mValue <= Int32.MaxValue)
                    return (T)(object)(int)(long)mValue;

                throw new BsonInvalidValueTypeException();
            }                
            
            return (T)(mValue);
        }

        public override string ToString()
        {
            if (mValue == null)
                return "null";

            return mValue.ToString();
        }

		public static BsonValue FromJson(string text)
		{
			var parser = new BsonTextParser(new StringReader(text));

			return parser.Parse();
		}
    }
}

