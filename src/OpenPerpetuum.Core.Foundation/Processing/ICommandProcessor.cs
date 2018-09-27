namespace OpenPerpetuum.Core.Foundation.Processing
{
	public interface ICommandProcessor
	{
		void Process<TCommand>(TCommand command) where TCommand : ICommand;
	}
}
