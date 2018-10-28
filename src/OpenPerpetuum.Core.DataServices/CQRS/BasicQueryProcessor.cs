using OpenPerpetuum.Core.Foundation.Processing;
using System;
using System.Diagnostics;

namespace OpenPerpetuum.Core.DataServices.CQRS
{
	public class BasicQueryProcessor : IQueryProcessor
	{
		private readonly IServiceProvider serviceProvider;

		public BasicQueryProcessor(IServiceProvider serviceProvider)
		{
			this.serviceProvider = serviceProvider;
		}

		[DebuggerStepThrough]
		public TResult Process<TResult>(IQuery<TResult> query)
		{
			Type handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
			// I would rather use GetRequiredService but that requires a dependency on the whole MS IoC chain
			dynamic handler = serviceProvider.GetService(handlerType);

			if (handler == null)
				throw new EntryPointNotFoundException($"Unable to find the requested service \"{handlerType.FullName}\"");

			return handler.Handle((dynamic)query);
		}
	}
}
