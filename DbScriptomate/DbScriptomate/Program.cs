using System;
using System.IO;
using System.Reflection;

namespace DbScriptomate
{
	class Program
	{

		static void Main(string[] args)
		{
			Console.WriteLine("DbScriptomate");

			var currentExePath = Assembly.GetExecutingAssembly().Location;
			var appDir = new DirectoryInfo(Path.GetDirectoryName(currentExePath));
			var runArgs = new RunArguments(args, appDir);
			if (runArgs.RunMode == RunMode.Help)
			{
				ShowHelp();
				return;
			}
			Console.WriteLine("Run /? for command line help.");

			var app = new App(runArgs, appDir);
			app.Execute();

			if (runArgs.RunMode == RunMode.Interactive)
			{
				Console.WriteLine("Press enter to continue...");
				Console.ReadLine();
			}
		}

		private static void ShowHelp()
		{
			Console.WriteLine("DbScriptomate creates a new file with the following file name format: [ScriptNumber].[Author (Initials)].[ScriptType (DDL or DML)].[Short description].sql\r\nOptionally add the UseLocal flag to generate sequence numbers using the local sequence number provider.");
			Console.WriteLine("usage:> DbScriptomate.exe [\"ScriptType\" \"Author\" \"Short description\" \"UseLocal\"]");
			Console.WriteLine(Environment.NewLine);
			Console.WriteLine("Apply scripts to specific database.");
			Console.WriteLine(@"usage: /ApplyScripts <DbKey> <""con string""> <""provider""> [""DbDir=<script directory>""]");
			Console.WriteLine(Environment.NewLine);
			Console.WriteLine("Setup your database with the required Function, Table and Stored Procedure to let DbScriptomate manage your migrations.");
			Console.WriteLine("usage: /SetupDb");
		}


	}
}
