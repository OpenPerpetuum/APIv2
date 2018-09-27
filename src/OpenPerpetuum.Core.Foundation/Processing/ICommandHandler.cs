namespace OpenPerpetuum.Core.Foundation.Processing
{
	public interface ICommandHandler<TCommand> where TCommand : ICommand
	{
		void Handle(TCommand command);
	}
}
