using System;
using Hashi.Bson;

namespace Hashi.Bson.Tests
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			BsonObject obj = BsonObject.Parse("{\"instanceId\":\"51de871530042082e040bb1a\",\"hostname\":\"127.0.0.1\",\"port\":6102}");

			Console.WriteLine(obj.ToString());
		}
	}
}
