using System;
using System.IO;
using System.Text;

namespace Hashi.Bson
{
    class BsonBinaryReader
    {
        Stream mInput;
        
        byte[] mBuf = new byte[sizeof(Int64)];
        int mNumBytesToRead = 0;
        
        public BsonBinaryReader(Stream input)
        {
            mInput = input;
            
            input.Read(mBuf, 0, sizeof(int));
            mNumBytesToRead = BitConverter.ToInt32(mBuf, 0) - sizeof(int);
        }
        
        public bool HasMoreToRead { get { return mNumBytesToRead > 0 ; } }

        public object ReadValue(BsonValueType type)
        {
            switch (type)
            {
                case BsonValueType.Int32:
                    return ReadInt32();
                case BsonValueType.Int64:
                    return ReadInt64();
                case BsonValueType.Double:
                    return ReadDouble();
                case BsonValueType.Boolean:
                    return ReadBoolean();
                case BsonValueType.String:
                    return ReadString();
                case BsonValueType.Array:
                    return ReadArray();
                case BsonValueType.Object:
                    return ReadObject();
                default:
                    return null;
            } 
        }
        
        public int ReadInt32()
        {
            if (mNumBytesToRead < sizeof(int))
                return default(int);
            
            mNumBytesToRead -= mInput.Read(mBuf, 0, sizeof(int));
            
            return BitConverter.ToInt32(mBuf, 0);
        }
        
        public long ReadInt64()
        {
            if (mNumBytesToRead < sizeof(long))
                return default(long);
            
            mNumBytesToRead -= mInput.Read(mBuf, 0, sizeof(long));
            
            return BitConverter.ToInt64(mBuf, 0);
        }
        
        public double ReadDouble()
        {
            if (mNumBytesToRead < sizeof(double))
                return default(double);
            
            mNumBytesToRead -= mInput.Read(mBuf, 0, sizeof(double));
            
            return BitConverter.ToDouble(mBuf, 0);
        }
        
        public bool ReadBoolean()
        {
            if (mNumBytesToRead < sizeof(bool))
                return default(bool);
            
            mNumBytesToRead -= mInput.Read(mBuf, 0, sizeof(bool));
            
            return BitConverter.ToBoolean(mBuf, 0);
        }
        
        public string ReadCString()
        {
            StringBuilder builder = new StringBuilder();
            
            mNumBytesToRead -= 1;
            
            for (int val = mInput.ReadByte(); val != 0; val = mInput.ReadByte())
            {
                builder.Append((char)val);
                
                mNumBytesToRead -= 1;
            }
            
            return builder.ToString();
        }
        
        public string ReadString()
        {
            int len = ReadInt32();
            
            if (mNumBytesToRead < len)
                return default(string);

            byte[] buf = new byte[len];
            mNumBytesToRead -= mInput.Read(buf, 0, len);
            
            return Encoding.UTF8.GetString(buf, 0, len - 1);
        }
        
        public BsonArray ReadArray()
        {
            long pos = mInput.Position;

            BsonArray arr = BsonArray.Deserialize(mInput);

            mNumBytesToRead -= (int)(mInput.Position - pos);

            return arr;
        }
        
        public BsonObject ReadObject()
        {
            long pos = mInput.Position;

            BsonObject obj = BsonObject.Deserialize(mInput);

            mNumBytesToRead -= (int)(mInput.Position - pos);

            return obj;
        }

        public BsonValueType ReadType()
        {
            if (mNumBytesToRead < 1)
                return BsonValueType.Invalid;

            mNumBytesToRead -= 1;

            return (BsonValueType)mInput.ReadByte();
        }
    }
}

