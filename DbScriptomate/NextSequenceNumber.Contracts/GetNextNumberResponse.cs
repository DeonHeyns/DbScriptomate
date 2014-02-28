
namespace NextSequenceNumber.Contracts
{
	public class GetNextNumberResponse
    {
		/// <summary>
		 /// The Key for which the sequence number was returned.
		/// </summary>
		public string ForKey { get; set; } 
		public string NextSequenceNumber { get; set; }

		public override string ToString()
		{
			return string.Format("{0}: {1}", this.ForKey, this.NextSequenceNumber);
		}
    }
}
