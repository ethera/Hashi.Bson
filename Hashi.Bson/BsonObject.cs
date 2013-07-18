using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Hashi.Bson
{
    public class BsonObject
    {
        Dictionary<string, BsonValue> mDictionary = new Dictionary<string, BsonValue>();
        
        public BsonObject()
        {
        }

        public int Count { get { return mDictionary.Count; } }
        
        public BsonObject Put(string key, object obj)
        {
			BsonValue value = new BsonValue(obj);
            
            if (value.Type == BsonValueType.Invalid)
                throw new BsonInvalidValueTypeException();
            
            mDictionary.Add(key, value);
            
            return this;
        }

        public bool ContainsKey(string key)
        {
            return mDictionary.ContainsKey(key);
        }

        public object Get(string key)
        {
            if (!mDictionary.ContainsKey(key))
                throw new BsonObjectKeyNotFoundException();
            
            return mDictionary[key].Value;
        }
        
        public T Get<T>(string key)
        {
            if (!mDictionary.ContainsKey(key))
                throw new BsonObjectKeyNotFoundException();
            
            return mDictionary[key].To<T>();
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("{ ");

            foreach (var item in mDictionary)
            {
                builder.Append(item.Key);
                builder.Append(": ");
                if (item.Value.Type == BsonValueType.String)
                {
                    builder.Append("\"");
                }
                builder.Append(item.Value.ToString());
                if (item.Value.Type == BsonValueType.String)
                {
                    builder.Append("\"");
                }
                builder.Append(", ");
            }

            builder.Remove(builder.Length - 2, 2);
            builder.Append(" }");

            return builder.ToString();
        }
        
        public void Serialize(Stream output)
        {
            BsonBinaryWriter writer = new BsonBinaryWriter(output);

            writer.WriteStart();

            foreach (var item in mDictionary)
            {
                if (item.Value.Type == BsonValueType.EOD)
                    break;

                if (item.Value.Type == BsonValueType.Invalid)
                    throw new BsonInvalidValueTypeException();

                writer.WriteType(item.Value.Type);
                writer.WriteCString(item.Key);
                writer.WriteValue(item.Value);
            }

            writer.WriteDone();
        }
        
        public static BsonObject Deserialize(Stream input)
        {
            BsonObject obj = new BsonObject();
            
            BsonBinaryReader reader = new BsonBinaryReader(input);
            while (reader.HasMoreToRead)
            {
                BsonValueType type = reader.ReadType();
                if (type == BsonValueType.EOD)
                    break;

                if (type == BsonValueType.Invalid)
                    throw new BsonInvalidValueTypeException();

                obj.Put(reader.ReadCString(), reader.ReadValue(type));
            }
            
            return obj;
        }
    }
}

