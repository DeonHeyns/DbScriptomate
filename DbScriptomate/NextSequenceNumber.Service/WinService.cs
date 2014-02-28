using System;
using ServiceStack.Configuration;
using Topshelf;

namespace NextSequenceNumber.Service
{
	internal class WinService
	{
		private AppHost _appHost;
		HostControl _hostControl;

		public bool Start(HostControl hostControl)
		{
			_hostControl = hostControl;

			var listeningOn = new AppSettings().GetString("ListenOn");
			_appHost = new AppHost();
			_appHost.Init();
			_appHost.Start(listeningOn);

			Console.WriteLine("AppHost Created at {0}, listening on {1}", DateTime.Now, listeningOn);

			return true;
		}

		public bool Stop(HostControl hostControl)
		{
			_appHost.Stop();
			_hostControl.Stop();
			return true;
		}

	}
}
