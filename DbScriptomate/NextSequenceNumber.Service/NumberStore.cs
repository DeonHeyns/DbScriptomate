using System;
using System.IO;
using System.Reflection;

namespace NextSequenceNumber.Service
{
	internal static class NumberStore
	{
		private static readonly object _locker = new object();

		internal static string GetNextSequenceNumber(string key)
		{
			lock (_locker)
			{
				return ReadAndIncrementSequenceNumber(key);
			}
		}

		private static string ReadAndIncrementSequenceNumber(string key)
		{
			string file = string.Format("SequenceNumber_{0}.txt", key);
			var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			file = Path.Combine(dir, file);
			Console.WriteLine("Reading from store file: " + file);
			if (!File.Exists(file))
				File.AppendAllText(file, "00000");

			string fileContent = File.ReadAllText(file);
			int number;
			if (!int.TryParse(fileContent, out number))
				return "failed to parse: " + fileContent;

			number++;
			string nextNumber = number.ToString("00000");
			File.WriteAllText(file, nextNumber);

			return nextNumber;
		}


	}
}
