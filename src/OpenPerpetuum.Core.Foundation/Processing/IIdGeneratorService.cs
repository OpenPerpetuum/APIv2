using System;

namespace OpenPerpetuum.Core.Foundation.Processing
{
	public interface IIdGeneratorService
	{
		Guid GenerateGuid();
	}
}
