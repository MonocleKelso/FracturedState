namespace FracturedState.Game.AI
{
    public interface IState
	{
		void Enter();
        void Execute();
		void Exit();
	}
}