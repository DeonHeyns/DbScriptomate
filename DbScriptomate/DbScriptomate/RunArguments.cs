using Microsoft.SqlServer.Management.Common;
using System;
using System.IO;
using System.Linq;
using System.Security.Principal;

namespace DbScriptomate
{
    internal class RunArguments
    {
        internal RunMode RunMode { get; set; }

        internal string Author { get; set; }
        internal string Description { get; set; }
        internal string DdlOrDmlType { get; set; }
        internal string ScriptNumber { get; set; }
        internal string DbKey { get; set; }
        internal string DbConnectionString { get; set; }
        internal string DbConnectionProvider { get; set; }
        internal string DbDir { get; set; }
        internal bool UseLocal { get; set; }

        internal RunArguments(
            string[] args,
            DirectoryInfo appDir)
        {
            this.RunMode = RunMode.Interactive;
            Description = GetCurrentUserName();
            DdlOrDmlType = "XXX";
            Author = "XX";
            UseLocal = false;

            this.ParseRunArguments(args, appDir);
        }

        private static string GetCurrentUserName()
        {
            // Use the current windows account under which this is running.
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            var currentUser = identity.Name.Replace(@"\", "_");
            return currentUser;
        }

        private void ParseRunArguments(string[] args, DirectoryInfo appDir)
        {
            if (args.Any(x => x.StartsWith("/?"))
                || args.Select(a => a.ToLower()).Any(x => x.Contains("/help")))
            {
                this.RunMode = RunMode.Help;
            }
            else if (args.Length == 0)
            {
                this.RunMode = RunMode.Interactive;
            }
            else if (args.Select(a => a.ToLower()).Any(x => x.Contains("/applyscripts")))
            {
                if (args.Count() < 4)
                {
                    Console.WriteLine("/ApplyScripts requires at least 4 arguments. Run /? for help.");
                    Environment.Exit(1);
                }
                this.RunMode = RunMode.ApplyScriptsToDb;
                this.DbKey = args[1];
                this.DbConnectionString = args[2];
                this.DbConnectionProvider = args[3];

                this.DbDir = args.SingleOrDefault(a => a.ToLower().StartsWith("dbdir="));
                if (this.DbDir != null)
                {
                    this.DbDir = this.DbDir.Replace("dbdir=", string.Empty);
                    if (!Directory.Exists(this.DbDir))
                        throw new InvalidArgumentException(
                            "The directory specified for the DbDir command line param does not exist: " + this.DbDir);
                }
                else
                {
                    this.DbDir = Path.Combine(appDir.FullName, this.DbKey);
                }

                Console.WriteLine("Applying Scripts to DB with Key: {0}", this.DbKey);
                Console.WriteLine("Connection string: {0}", this.DbConnectionString);
                Console.WriteLine("Provider: {0}", this.DbConnectionProvider);
            }
            else if (args.Length == 3)
            {
                this.RunMode = RunMode.GenerateNewScript;
                this.DdlOrDmlType = args[0];
                this.Author = args[1];
                this.Description = args[2];
            }
            else if (args.Any(a => a.ToLower().Equals("setupdb")))
            {
                this.RunMode = RunMode.SetupDb;
            }
            else if (args.Any(a => a.ToLower().Equals("uselocal")))
            {
                this.UseLocal = true;
            }
        }
    }
}