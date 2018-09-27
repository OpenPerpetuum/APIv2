using OpenPerpetuum.Core.Foundation.Processing;
using System;

namespace OpenPerpetuum.Core.DataServices.Context
{
	public class GenericContext : IGenericContext
	{
		public DateTimeOffset CurrentDateTime => DateTimeOffset.Now;
	}
}
