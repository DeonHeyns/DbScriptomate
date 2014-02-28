using NextSequenceNumber.Contracts;
using ServiceStack.WebHost.Endpoints;

namespace NextSequenceNumber.Service
{
	public class AppHost : AppHostHttpListenerBase
	{
		public AppHost() : base("Next Sequence Number HttpListener", typeof(NextSequenceNumberService).Assembly) { }

		public override void Configure(Funq.Container container)
		{
			Routes
				.Add<Hi>("/Hi")
				.Add<Hi>("/Hi/{Name}")
				.Add<GetNextNumber>("/GetNextSequenceNumber/");
		}
	}
}
