
namespace FracturedState.Game.AI
{
	public class UnitStateMachine
	{
        public IState CurrentState { get; private set; }

        public bool IsCoverPrepped => CurrentState is UnitTakeCoverState;

		public void ChangeState(IState newState)
		{
			CurrentState?.Exit();
			CurrentState = newState;
			CurrentState?.Enter();
		}
	}
}