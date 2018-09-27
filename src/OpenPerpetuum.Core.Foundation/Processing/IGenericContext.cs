using System;

namespace OpenPerpetuum.Core.Foundation.Processing
{
	/// <summary>
	/// Contains mockable services for generic items, such as the date and time, core constants, and configurable settings
	/// </summary>
	public interface IGenericContext
	{
		DateTimeOffset CurrentDateTime { get; }
	}
}
