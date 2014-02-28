using NextSequenceNumber.Contracts;
using System;

namespace NextSequenceNumber.Service
{
	public class NextSequenceNumberService : ServiceStack.ServiceInterface.Service
	{
		public object Any(Hi request)
		{
			return new HiResponse { Result = "Hi, " + request.Name };
		}

		public object Any(GetNextNumber request)
		{
			if (request.ForKey == null)
				throw new ArgumentException("ForKey property must not be null and must be file name safe.");
			return new GetNextNumberResponse
			{
				ForKey = request.ForKey,
				NextSequenceNumber = NumberStore.GetNextSequenceNumber(request.ForKey)
			};
		}
	}
}
