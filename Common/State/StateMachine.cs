
namespace FracturedState.Game.State
{
    public abstract class StateMachine<T> where T : IState
    {
		protected T currentState;
	
        public StateMachine(T state)
        {
            currentState = state;
			currentState.Enter();
        }

		public virtual void ChangeState(T newState)
		{
			if (currentState != null)
				currentState.Exit();
			
			currentState = newState;
			currentState.Enter();
		}
    }
}