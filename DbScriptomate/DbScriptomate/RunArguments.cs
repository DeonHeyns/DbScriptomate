using System.Security.Principal;

namespace DbScriptomate
{
	internal class RunArguments
	{
		public RunMode RunMode { get; set; }

		public string Author { get; set; }
		public string Description { get; set; }
		public string DdlOrDmlType { get; set; }
		public string ScriptNumber { get; set; }
		public string DbKey { get; set; }
		public string DbConnectionString { get; set; }
		public string DbConnectionProvider { get; set; }
		public string DbDir { get; set; }

		public RunArguments()
		{
			this.RunMode = RunMode.Interactive;
			Description = GetCurrentUserName();
			DdlOrDmlType = "XXX";
			Author = "XX";
		}

		private static string GetCurrentUserName()
		{
			// Use the current windows account under which this is running.
			WindowsIdentity identity = WindowsIdentity.GetCurrent();
			var currentUser = identity.Name.Replace(@"\", "_");
			return currentUser;
		}
	}
}
