using OpenPerpetuum.Core.Foundation.Processing;
using System;

namespace OpenPerpetuum.Core.DataServices.CQRS
{
	public class BasicCommandProcessor : ICommandProcessor
	{
		private readonly IServiceProvider serviceProvider;

		public BasicCommandProcessor(IServiceProvider serviceProvider)
		{
			this.serviceProvider = serviceProvider;
		}
		
		public void Process<TCommand>(TCommand command) where TCommand : ICommand
		{
			var handlerType = typeof(ICommandHandler<>).MakeGenericType(typeof(TCommand));

			dynamic handler = serviceProvider.GetService(handlerType);

			if (handler == null)
				throw new EntryPointNotFoundException($"Unable to find the requested service \"{handlerType.FullName}\"");

			handler.Handle((dynamic)command);
		}
	}
}
