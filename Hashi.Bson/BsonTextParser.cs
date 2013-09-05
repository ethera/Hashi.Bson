using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Hashi.Bson
{
	class BsonTextParser
	{
		TextReader mReader;

		public BsonTextParser(TextReader reader)
		{
			mReader = reader;
		}

		public BsonValue Parse()
		{
			JsonToken token = GetNextToken();

			if (token.Type == JsonTokenType.OpenCurly)
				return new BsonValue(ReadObject());

			if (token.Type == JsonTokenType.OpenSquare)
				return new BsonValue(ReadArray());

			return new BsonValue(null);
		}

		BsonObject ReadObject()
		{
			BsonObject obj = new BsonObject();

			while (true)
			{
				JsonToken name = GetNextToken();
				if (obj.Count == 0 && name.Type == JsonTokenType.CloseCurly)
					break;
				if (name.Type != JsonTokenType.String)
					throw new BsonTextParserException();

				JsonToken colon = GetNextToken();
				if (colon.Type != JsonTokenType.Colon)
					throw new BsonTextParserException();

				JsonToken value = GetNextToken();
				if (value.Type == JsonTokenType.OpenCurly)
				{
					obj.Put((string)name.Value, ReadObject());
				}
				else if (value.Type == JsonTokenType.OpenSquare)
				{
					obj.Put((string)name.Value, ReadArray());
				}
				else
				{
					obj.Put((string)name.Value, value.Value);
				}

				JsonToken comma = GetNextToken();
				if (comma.Type == JsonTokenType.CloseCurly)
					break;
				if (comma.Type != JsonTokenType.Comma)
					throw new BsonTextParserException();
			}

			return obj;
		}

		BsonArray ReadArray()
		{
			BsonArray arr = new BsonArray();

			while (true)
			{
				JsonToken value = GetNextToken();
				if (arr.Count == 0 && value.Type == JsonTokenType.CloseSquare)
					break;

				if (value.Type == JsonTokenType.OpenCurly)
				{
					arr.Add(ReadObject());
				}
				else if (value.Type == JsonTokenType.OpenSquare)
				{
					arr.Add(ReadArray());
				}
				else
				{
					arr.Add(value.Value);
				}

				JsonToken comma = GetNextToken();
				if (comma.Type == JsonTokenType.CloseSquare)
					break;
				if (comma.Type != JsonTokenType.Comma)
					throw new BsonTextParserException();
			}

			return arr;
		}

		static readonly List<char> InvalidCharacters = new List<char>() { '\b', '\f', '\t', '\r', '\n', ' ' };

		static readonly Dictionary<char, JsonTokenType> SpecialCharacters = new Dictionary<char, JsonTokenType>() {
			{ '{', JsonTokenType.OpenCurly },
			{ '}', JsonTokenType.CloseCurly },
			{ '[', JsonTokenType.OpenSquare },
			{ ']', JsonTokenType.CloseSquare },
			{ ':', JsonTokenType.Colon },
			{ ',', JsonTokenType.Comma }
		};

		static readonly Dictionary<char, char> EscapeCharacters = new Dictionary<char, char>() {
			{ 'b', '\b' },
			{ 'f', '\f' },
			{ 't', '\t' },
			{ 'r', '\r' },
			{ 'n', '\n' },
			{ '\\', '\\' },
			{ '"', '"' }
		};

		static readonly Dictionary<char, int> HexadecimalValues = new Dictionary<char, int>() {
			{ '0', 0 }, { '1', 1 }, { '2', 2 }, { '3', 3 }, { '4', 4 }, 
			{ '5', 5 }, { '6', 6 }, { '7', 7 }, { '8', 8 }, { '9', 9 },
			{ 'a', 10 }, { 'A', 10 }, { 'b', 11 }, { 'B', 11 },
			{ 'c', 12 }, { 'C', 12 }, { 'd', 13 }, { 'D', 13 },
			{ 'e', 14 }, { 'E', 14 }, { 'f', 15 }, { 'F', 15 }
		};

		enum JsonTokenType
		{
			OpenCurly,
			CloseCurly,
			OpenSquare,
			CloseSquare,
			Colon,
			Comma,
			String,
			Int,
			Long,
			Double,
			Boolean,
			Invalid
		}

		class JsonToken
		{
			public JsonTokenType Type { get; private set; }
			public object Value { get; private set; }

			public JsonToken(JsonTokenType type)
			{
				Type = type;
			}

			public JsonToken(string value)
			{
				Type = JsonTokenType.String;
				Value = value;
			}

			public JsonToken(int value)
			{
				Type = JsonTokenType.Double;
				Value = value;
			}

			public JsonToken(long value)
			{
				Type = JsonTokenType.Double;
				Value = value;
			}

			public JsonToken(double value)
			{
				Type = JsonTokenType.Double;
				Value = value;
			}

			public JsonToken(bool value)
			{
				Type = JsonTokenType.Boolean;
				Value = value;
			}
		}

		JsonToken GetNextToken()
		{
			char? c1 = ReadValidChar();

			if (c1 == null)
				return new JsonToken(JsonTokenType.Invalid);

			if (SpecialCharacters.ContainsKey((char)c1))
				return new JsonToken(SpecialCharacters[(char)c1]);

			if (c1 == '"')
			{
				StringBuilder builder = new StringBuilder();

				while (PeekNextChar() != '"')
				{
					char? c2 = ReadStringChar();
					if (c2 == null)
						return new JsonToken(JsonTokenType.Invalid);

					builder.Append((char)c2);
				}

				ReadValidChar();

				return new JsonToken(builder.ToString());
			}
			else
			{
				StringBuilder builder = new StringBuilder();

				builder.Append(c1);

				while (PeekNextChar() != null && !SpecialCharacters.ContainsKey((char)PeekNextChar()))
				{
					char? c2 = ReadStringChar();
					if (c2 == null)
						return new JsonToken(JsonTokenType.Invalid);

					builder.Append((char)c2);
				}

				string built = builder.ToString();

				if (built.Length == 0)
					return new JsonToken(JsonTokenType.Invalid);

				int intv;
				if (int.TryParse(built, out intv))
					return new JsonToken(intv);

				long longv;
				if (long.TryParse(built, out longv))
					return new JsonToken(longv);

				double doublev;
				if (double.TryParse(built, out doublev))
					return new JsonToken(doublev);

				bool boolv;
				if (bool.TryParse(built, out boolv))
					return new JsonToken(boolv);

				return new JsonToken(built);
			}
		}

		char? ReadValidChar()
		{
			char c = ' ';

			while (InvalidCharacters.Contains(c))
			{
				int v = mReader.Read();
				if (v == -1)
					return null;

				c = (char)v;
			}

			return c;
		}

		char? PeekNextChar()
		{
			int v = mReader.Peek();
			if (v == -1)
				return null;

			return (char)v;
		}

		char? ReadStringChar()
		{
			int v1 = mReader.Read();
			if (v1 == -1)
				return null;

			char c1 = (char)v1;
			if (c1 == '\\')
			{
				int v2 = mReader.Read();
				if (v2 == -1)
					return null;

				char c2 = (char)v2;
				if (EscapeCharacters.ContainsKey(c2))
					return EscapeCharacters[c2];

				if (c2 == 'u')
				{
					StringBuilder builder = new StringBuilder();

					builder.Append("\\u");

					for (int i = 0; i < 4; ++i)
					{
						int v4 = mReader.Read();
						if (v4 == -1)
							return null;

						if (!HexadecimalValues.ContainsKey((char)v4))
							return null;

						builder.Append((char)v4);
					}

					return Char.Parse(builder.ToString()); 
				}

				return c2;
			}

			return c1;
		}
	}
}

