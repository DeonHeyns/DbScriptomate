
using ServiceStack.ServiceHost;
namespace NextSequenceNumber.Contracts
{
	public class GetNextNumber : IReturn<GetNextNumberResponse>
	{
		/// <summary>
		/// The Key for which to return the sequence number.
		/// </summary>
		public string ForKey { get; set; }
	}
}
