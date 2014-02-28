using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using NextSequenceNumber.Contracts;
using ServiceStack.ServiceClient.Web;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace DbScriptomate
{
	class Program
	{
		private static Regex _sequenceRegex = new Regex(@"(^\d\d\d+)[.,].*");
		private static DirectoryInfo _appDir;

		static void Main(string[] args)
		{
			if (args.Any(x => x.Contains("/?")))
			{
				ShowHelp();
				return;
			}
			Console.WriteLine("Run /? for command line help.");

			var currentExePath = Assembly.GetExecutingAssembly().Location;
			_appDir = new DirectoryInfo(Path.GetDirectoryName(currentExePath));
			var runArgs = ParseRunArguments(args);

			switch (runArgs.RunMode)
			{
				case RunMode.Interactive:
					{
						var dbDir = LetUserPickDbDir(_appDir);
						Console.WriteLine("Selected " + dbDir.Name);
						RunInteractively(runArgs, dbDir);
						break;
					}
				case RunMode.GenerateNewScript:
					throw new NotImplementedException("DbKey needs to be implemented on this command line option");
					//GenerateNewScript(runArgs, dbDir);
					break;
				case RunMode.ApplyScriptsToDb:
					{
						var dbDir = new DirectoryInfo(runArgs.DbDir);
						if (!dbDir.Exists)
						{
							Console.WriteLine("The following DB Dir does not exist: " + dbDir.FullName);
							Environment.Exit(1);
						}
						ApplyScriptsToDb(runArgs, dbDir);
						break;
					}
			}

			if (runArgs.RunMode == RunMode.Interactive)
			{
				Console.WriteLine("Press enter to continue...");
				Console.ReadLine();
			}
		}

		private static List<FileInfo> GetExistingScriptFiles(DirectoryInfo dbDir)
		{
			var scripts = dbDir.GetFiles("*.sql", SearchOption.AllDirectories).AsEnumerable()
				.Where(f => !f.Directory.Name.StartsWith("_"))
				.Where(f => _sequenceRegex.IsMatch(f.Name))
				.OrderBy(f => ToInt(f.Name))
				.ToList();
			return scripts;
		}

		private static DirectoryInfo LetUserPickDbDir(DirectoryInfo currentDir)
		{
			var dbDirs = currentDir.GetDirectories()
				.Where(d => !d.Name.StartsWith("_"))
				.OrderBy(d => d.Name)
				.ToArray();
			if (!dbDirs.Any())
			{
				Console.WriteLine("No DB Folders found.");
				Environment.Exit(0);
			}
			for (int i = 1; i <= dbDirs.Count(); i++)
			{
				var info = string.Format("{0}) {1}", i, dbDirs[i - 1].Name);
				Console.WriteLine(info);
			}

			var userOption = Console.ReadKey();
			Console.Clear();
			int selectedIndex = int.Parse(userOption.KeyChar.ToString()) - 1;
			if (selectedIndex >= dbDirs.Count())
			{
				Console.WriteLine("Invalid selection.");
				Environment.Exit(0);
			}
			var dbDir = dbDirs[selectedIndex];
			return dbDir;
		}

		private static void ApplyScriptsToDb(
			RunArguments runArgs,
			DirectoryInfo dbDir)
		{
			var scripts = GetExistingScriptFiles(dbDir);
			if (scripts.Count() == 0)
				Console.WriteLine("No previous scripts detected.");

			ConnectionStringSettings conSettings;
			if (runArgs.RunMode == RunMode.Interactive)
				conSettings = LetUserPickDbConnection(dbDir.Name);
			else if (runArgs.RunMode == RunMode.ApplyScriptsToDb)
				conSettings = new ConnectionStringSettings(runArgs.DbKey, runArgs.DbConnectionString, runArgs.DbConnectionProvider);
			else
				throw new InvalidArgumentException("Invalid execution path.");

			ApplyMissingScripts(runArgs, conSettings, scripts);
		}

		private static ConnectionStringSettings LetUserPickDbConnection(
			string connectionPrefix)
		{
			Console.WriteLine("Pick connection string to use for:" + connectionPrefix);
			var connectionList = ConfigurationManager.ConnectionStrings.Cast<ConnectionStringSettings>()
				.Where(cs => cs.Name != "LocalSqlServer")
				.Where(cs => cs.Name.StartsWith(connectionPrefix))
				.ToList();
			foreach (ConnectionStringSettings cs in connectionList)
			{
				var index = 1 + connectionList.IndexOf(cs);
				var connectionInfo = string.Format("{0}) {1}", index, cs.Name);
				Console.WriteLine(connectionInfo);
			}
			var input = Console.ReadKey();
			Console.Clear();
			int selectedIndex = int.Parse(input.KeyChar.ToString()) - 1;
			Console.WriteLine(string.Format("{1} selected: {0}", connectionList[selectedIndex].Name, selectedIndex + 1));
			var con = connectionList[selectedIndex];
			return con;
		}

		private static void ApplyMissingScripts(
			RunArguments runArgs,
			ConnectionStringSettings conSettings,
			IList<FileInfo> scripts)
		{
			var scriptsToRun = new List<FileInfo>();
			var dbScriptNumbers = GetDbScripts(conSettings);
			Console.WriteLine(string.Format("{0} scripts logged in dbo.DbScripts", dbScriptNumbers.Count()));
			Console.WriteLine("The following are scripts not yet run on the selected DB:");
			foreach (var scriptFile in scripts)
			{
				var scriptNumber = ToInt(scriptFile.Name);
				if (!dbScriptNumbers.Contains(scriptNumber))
				{
					PrintScriptItemInfo(scriptFile);
					scriptsToRun.Add(scriptFile);
				}
			}
			Console.WriteLine("Missing scripts check completed");
			bool userSelectedToApplyScripts = false;
			if (runArgs.RunMode == RunMode.Interactive)
			{
				Console.WriteLine("Would you like me to run them one at a time? I'll break on any errors?");
				Console.WriteLine("1 - Yes, please run one at a time.");
				Console.WriteLine("2 - No thanks, I'll run them  later myself.");
				int selectedIndex = int.Parse(Console.ReadKey().KeyChar.ToString());
				Console.Clear();

				if (selectedIndex == 1)
					userSelectedToApplyScripts = true;
			}

			if (runArgs.RunMode == RunMode.Interactive && userSelectedToApplyScripts
				|| runArgs.RunMode == RunMode.ApplyScriptsToDb)
			{
				foreach (var scriptFile in scriptsToRun)
				{
					Console.WriteLine(string.Format("Running {0}", scriptFile.Name));
					string errorMessage = string.Empty;
					bool success = RunDbScript(conSettings, scriptFile, out errorMessage);
					Console.WriteLine(success ? "Succeeded" : "Failed:");
					if (!success)
					{
						Console.WriteLine(string.Format("{0}", errorMessage));
						if (runArgs.RunMode == RunMode.Interactive)
						{
							Console.WriteLine("1 - Skip, 2 - Abort?");
							var innerInput = Console.ReadKey();
							int selectedIndex = int.Parse(innerInput.KeyChar.ToString());
							if (selectedIndex == 2)
								return;
							else
								continue;
						}
						else // not interactive 
							Environment.Exit(2222);
					}
				}
			}
			else // Don't apply anything
				return;
		}

		private static void PrintScriptItemInfo(FileInfo scriptFile)
		{
			var item = string.Format("{0}", scriptFile.Name);
			if (item.Length > 75)
			{
				item = item.Substring(0, Math.Min(item.Length, 75));
				item += "...";
			}
			Console.WriteLine(item);
		}

		private static bool RunDbScript(ConnectionStringSettings connectionSettings, FileInfo scriptFile, out string value)
		{
			value = "";
			string sql = scriptFile.OpenText().ReadToEnd();
			var builder = new SqlConnectionStringBuilder(connectionSettings.ConnectionString);
			builder.MultipleActiveResultSets = false;
			var connectionString = builder.ToString();
			var sqlConnection = new SqlConnection(connectionString);
			Server server = null;
			try
			{
				var serverConnection = new ServerConnection(sqlConnection);
				server = new Server(serverConnection);
				server.ConnectionContext.BeginTransaction();
				server.ConnectionContext.ExecuteNonQuery(sql);
				server.ConnectionContext.CommitTransaction();
			}
			catch (Exception e)
			{
				if (server != null)
					server.ConnectionContext.RollBackTransaction();

				e = e.GetBaseException();
				value = e.Message;
				return false;
			}
			finally
			{
				if (sqlConnection != null)
					sqlConnection.Dispose();
			}
			return true;
		}

		private static List<decimal> GetDbScripts(ConnectionStringSettings connectionSettings)
		{
			var dbScriptNumbers = new List<decimal>();
			using (SqlConnection connection = new SqlConnection(connectionSettings.ConnectionString))
			{
				var sql = @"select ScriptNumber from dbo.DbScripts";
				var command = new SqlCommand(sql, connection);
				command.CommandType = System.Data.CommandType.Text;
				connection.Open();
				var reader = command.ExecuteReader();
				while (reader.Read())
				{
					var value = reader["ScriptNumber"].ToString();
					dbScriptNumbers.Add(decimal.Parse(value));
				}
				reader.Close();
				connection.Close();
				return dbScriptNumbers;
			}
		}

		private static void RunInteractively(
			RunArguments runArgs,
			DirectoryInfo dbDir)
		{
			Console.WriteLine("Pick:");
			Console.WriteLine("1) Detect last script number and generate new script template");
			Console.WriteLine("2) Detect missing scripts in DB");
			var input = Console.ReadKey();
			Console.Clear();
			switch (input.KeyChar)
			{
				case '1':
					GenerateNewScript(runArgs, dbDir);
					break;
				case '2':
					ApplyScriptsToDb(runArgs, dbDir);
					break;
			}
		}

		private static GetNextNumberResponse GetNextSequenceNumber(string key)
		{
			string url = (string)new AppSettingsReader().GetValue("NextSequenceNumberServiceUrl", typeof(string));
			using (var client = new JsonServiceClient(url))
			{
				var response = client.Post<GetNextNumberResponse>(new GetNextNumber { ForKey = key });
				return response;
			}
		}

		private static void GenerateNewScript(
			RunArguments runArgs,
			DirectoryInfo dbDir)
		{
			var response = GetNextSequenceNumber(dbDir.Name);
			Console.WriteLine(response.ToString());
			runArgs.ScriptNumber = response.NextSequenceNumber;
			var newScript = CreateNewScriptFile(dbDir, runArgs);

			Console.WriteLine("using {0} next", runArgs.ScriptNumber);
			Console.WriteLine("created file: {0}\\{1}", dbDir.Name, newScript);
			Console.WriteLine();
		}

		private static void ShowHelp()
		{
			Console.WriteLine("DbScriptomate creates a new file with the following file name format: [ScriptNumber].[Author (Initials)].[ScriptType (DDL or DML)].[Short description].sql");
			Console.WriteLine("usage:> DbScriptomate.exe [\"ScriptType\" \"Author\" \"Short description\"]");
			Console.WriteLine(Environment.NewLine);
			Console.WriteLine(string.Format("Apply scripts to specific database."));
			Console.WriteLine(string.Format(@"usage: /ApplyScripts <DbKey> <""con string""> <""provider""> [""DbDir=<script directory>""]"));
		}

		private static RunArguments ParseRunArguments(string[] args)
		{
			var runArgs = new RunArguments();
			if (args.Length == 0)
			{
				runArgs.RunMode = RunMode.Interactive;
			}
			else if (args.Select(a => a.ToLower()).Any(x => x.Contains("/applyscripts")))
			{
				if (args.Count() < 4)
				{
					Console.WriteLine("/ApplyScripts requires at least 4 arguments. Runn /? for help.");
					Environment.Exit(1);
				}
				runArgs.RunMode = RunMode.ApplyScriptsToDb;
				runArgs.DbKey = args[1];
				runArgs.DbConnectionString = args[2];
				runArgs.DbConnectionProvider = args[3];

				runArgs.DbDir = args.SingleOrDefault(a => a.ToLower().StartsWith("dbdir="));
				if (runArgs.DbDir != null)
				{
					runArgs.DbDir = runArgs.DbDir.Replace("dbdir=", string.Empty);
					if (!Directory.Exists(runArgs.DbDir))
						throw new InvalidArgumentException("The directory specified for the DbDir command line param does not exist: " + runArgs.DbDir);
				}
				else
				{
					runArgs.DbDir = Path.Combine(_appDir.FullName, runArgs.DbKey);
				}

				Console.WriteLine(string.Format("Applying Scripts to DB with Key: {0}", runArgs.DbKey));
				Console.WriteLine(string.Format("Connection string: {0}", runArgs.DbConnectionString));
				Console.WriteLine(string.Format("Provider: {0}", runArgs.DbConnectionProvider));
			}
			else if (args.Length == 3)
			{
				runArgs.RunMode = RunMode.GenerateNewScript;
				runArgs.DdlOrDmlType = args[0];
				runArgs.Author = args[1];
				runArgs.Description = args[2];
			}
			return runArgs;
		}

		private static int ToInt(string filename)
		{
			var value = _sequenceRegex.Match(filename).Groups[1].Captures[0].Value;
			var num = Convert.ToInt32(value);
			return num;
		}

		private static string CreateNewScriptFile(
			DirectoryInfo dir,
			RunArguments attributes)
		{
			const string fileNameFormat = "{0:000.0}.{1}.{2}.{3}.sql";
			string templateFile = "_NewScriptTemplate.sql";
			templateFile = Path.Combine(dir.FullName, templateFile);
			if (!File.Exists(templateFile))
				throw new FileNotFoundException(templateFile);
			var template = File.ReadAllText(templateFile);
			var contents = string.Format(template, attributes.ScriptNumber, attributes.DdlOrDmlType, attributes.Author, attributes.Description);
			var filename = string.Format(fileNameFormat, attributes.ScriptNumber, attributes.Author, attributes.DdlOrDmlType, attributes.Description);
			File.AppendAllText(Path.Combine(dir.FullName, filename), contents, Encoding.UTF8);
			return filename;
		}
	}
}
