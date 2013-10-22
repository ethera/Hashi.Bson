using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Hashi.Bson
{
    public class BsonArray : IEnumerable
    {
        List<BsonValue> mArray = new List<BsonValue>();

        public BsonArray()
        {
        }

        public BsonArray(object[] array)
        {
            foreach (var val in array)
            {
                Add(val);
            }
        }

        public int Count { get { return mArray.Count; } }
        
        public IEnumerator GetEnumerator()
        {
            return mArray.GetEnumerator();
        }

        public BsonValue[] ToArray()
        {
            return mArray.ToArray();
        }
        
        public BsonArray Add(object obj)
        {
			BsonValue value = new BsonValue(obj);

			if (value.Type == BsonValueType.Invalid)
                throw new BsonInvalidValueTypeException();

			mArray.Add(value);

            return this;
        }

		public bool IsNull(int idx)
		{
			if (idx < 0 || idx >= Count)
				throw new BsonArrayOutOfIndexException();

			return mArray[idx].Type == BsonValueType.Null;
		}

        public object Get(int idx)
        {
            if (idx < 0 || idx >= Count)
                throw new BsonArrayOutOfIndexException();

            return mArray[idx].Value;
        }

        public T Get<T>(int idx)
        {
            if (idx < 0 || idx >= Count)
                throw new BsonArrayOutOfIndexException();

            return mArray[idx].To<T>();
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            
            builder.Append("[ ");
            
            foreach (var value in mArray)
            {
                if (value.Type == BsonValueType.String)
                {
                    builder.Append("\"");
                }
                builder.Append(value.ToString());
                if (value.Type == BsonValueType.String)
                {
                    builder.Append("\"");
                }
                builder.Append(", ");
            }

            if (mArray.Count > 0)
            {
                builder.Remove(builder.Length - 2, 2);
            }

            builder.Append(" ]");

            return builder.ToString();
        }

        public void Serialize(Stream output)
        {
            BsonBinaryWriter writer = new BsonBinaryWriter(output);

            writer.WriteStart();

            for(int i = 0; i < mArray.Count; ++i)
            {
                var item = mArray[i];

                if (item.Type == BsonValueType.EOD)
                    break;

                if (item.Type == BsonValueType.Invalid)
                    throw new BsonInvalidValueTypeException();

                writer.WriteType(item.Type);
                writer.WriteCString(i.ToString());
                writer.WriteValue(item);
            }

            writer.WriteDone();
        }

        public static BsonArray Deserialize(Stream input)
        {
            BsonArray obj = new BsonArray();

            BsonBinaryReader reader = new BsonBinaryReader(input);
            while (reader.HasMoreToRead)
            {
                BsonValueType type = reader.ReadType();
                if (type == BsonValueType.EOD)
                    break;

                if (type == BsonValueType.Invalid)
                    throw new BsonInvalidValueTypeException();

                int idx = -1;
                Int32.TryParse(reader.ReadCString(), out idx);

                if (idx == -1 || idx != obj.Count)
                    throw new BsonDeserializationException();

                obj.Add(reader.ReadValue(type));
            }

            return obj;
        }
    }
}

