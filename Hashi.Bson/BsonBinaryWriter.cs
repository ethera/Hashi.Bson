using System;
using System.IO;
using System.Text;

namespace Hashi.Bson
{
    class BsonBinaryWriter
    {     
        Stream mOutput;

        long mHeadPosition;

        public BsonBinaryWriter(Stream output)
        {
            mOutput = output;
        }

        public void WriteStart()
        {
            mHeadPosition = mOutput.Position;

            WriteInt32(0);
        }

        public void WriteDone()
        {
            mOutput.WriteByte(0);

            long curpos = mOutput.Position;

            mOutput.Position = mHeadPosition;

            WriteInt32((int)(curpos - mHeadPosition));

            mOutput.Position = curpos;
        }

        public void WriteValue(BsonValue value)
        {
            switch (value.Type)
            {
                case BsonValueType.Int32:
                    WriteInt32(value.To<int>());
                    break;
                case BsonValueType.Int64:
                    WriteInt64(value.To<long>());
                    break;
                case BsonValueType.Double:
                    WriteDouble(value.To<double>());
                    break;
                case BsonValueType.Boolean:
                    WriteBoolean(value.To<bool>());
                    break;
                case BsonValueType.String:
                    WriteString(value.To<string>());
                    break;
                case BsonValueType.Array:
                    WriteArray(value.To<BsonArray>());
                    break;
                case BsonValueType.Object:
                    WriteObject(value.To<BsonObject>());
                    break;
            }
        }

        public void WriteType(BsonValueType type)
        {
            mOutput.WriteByte((byte)type);
        }

        public void WriteInt32(int value)
        {
            mOutput.Write(BitConverter.GetBytes(value), 0, sizeof(int));
        }

        public void WriteInt64(long value)
        {
            mOutput.Write(BitConverter.GetBytes(value), 0, sizeof(long));
        }

        public void WriteDouble(double value)
        {
            mOutput.Write(BitConverter.GetBytes(value), 0, sizeof(double));
        }

        public void WriteBoolean(bool value)
        {
            mOutput.Write(BitConverter.GetBytes(value), 0, sizeof(bool));
        }

        public void WriteString(string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            mOutput.Write(BitConverter.GetBytes((int)bytes.Length + 1), 0, sizeof(int));
            mOutput.Write(bytes, 0, bytes.Length);
            mOutput.WriteByte(0);
        }

        public void WriteCString(string value)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(value);
            mOutput.Write(bytes, 0, bytes.Length);
            mOutput.WriteByte(0);
        }

        public void WriteArray(BsonArray value)
        {
            value.Serialize(mOutput);
        }

        public void WriteObject(BsonObject value)
        {
            value.Serialize(mOutput);
        }
    }
}

