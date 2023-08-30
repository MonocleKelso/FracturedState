using FracturedState.Game.AI;

namespace FracturedState.Game.Music
{
    public class TrackStateMachine
    {
        public IState CurrentState { get; private set; }

        public void ChangeState(IState newState)
        {
            CurrentState?.Exit();
            CurrentState = newState;
            CurrentState?.Enter();
        }

        public void Update()
        {
            CurrentState?.Execute();
        }
    }
}