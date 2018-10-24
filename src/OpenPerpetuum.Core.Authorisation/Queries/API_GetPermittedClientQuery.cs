using OpenPerpetuum.Core.Authorisation.Models;
using OpenPerpetuum.Core.Foundation.Processing;
using System;
using System.Collections.ObjectModel;

namespace OpenPerpetuum.Core.Authorisation.Queries
{
	public class API_GetPermittedClientQuery : IQuery<ReadOnlyCollection<AccessClientModel>>
	{
		/// <summary>
		/// Omitting this field will return all registered clients
		/// </summary>
		public Guid? ClientId
		{
			get;
			set;
		}
	}
}
