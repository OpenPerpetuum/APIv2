using OpenPerpetuum.Core.Foundation.Processing;
using System;

namespace OpenPerpetuum.Core.DataServices.Context
{
	public class IdGeneratorService : IIdGeneratorService
	{
		public Guid GenerateGuid()
		{
			return Guid.NewGuid();
		}
	}
}
