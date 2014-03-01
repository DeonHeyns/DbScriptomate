using Topshelf;

namespace NextSequenceNumber.Service
{
	class Program
	{
		static void Main(string[] args)
		{
			HostService();
		}

		private static void HostService()
		{
			HostFactory.Run(x =>
			{
				x.Service<WinService>(s =>
				{
					s.ConstructUsing(name => new WinService());
					s.WhenStarted((service, hostControl) => service.Start(hostControl));
					s.WhenStopped((service, hostControl) => service.Stop(hostControl));
				});
				x.RunAsLocalSystem();
				x.SetDescription("Next Sequence Number Service");
				x.SetDisplayName("Next Sequence Number Service");
				x.SetServiceName("Next.Sequence.Number.Service");
			});
		}
	}
}