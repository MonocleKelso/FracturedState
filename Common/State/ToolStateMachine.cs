using FracturedState.ModTools;

namespace FracturedState.Game.State
{
	public class ToolStateMachine<T> : StateMachine<T> where T : IToolState, IState
	{
        /// <summary>
        /// Returns true if the current state implements the IKeyStrokeTool interface and therefore takes keyboard input
        /// </summary>
        public bool IsKeyStrokeState { get { return currentState is IKeyStrokeTool; } }

		public ToolStateMachine(T newState)
			: base(newState)
		{
            newState.Enter();
		}

        public void ExecuteState()
        {
            if (currentState != null)
                currentState.ExecuteTool();
        }

        public void ExecuteMouseDown()
        {
            if (currentState is IMouseDownListener)
            {
                ((IMouseDownListener)currentState).ExecuteMouseDown();
            }
        }

		public void DrawToolState()
		{
			currentState.DrawToolOptions();
		}

        public void ProcessHotKey(UnityEngine.KeyCode keyCode)
        {
            (currentState as IKeyStrokeTool).DoKeyStroke(keyCode);
        }
	}
}