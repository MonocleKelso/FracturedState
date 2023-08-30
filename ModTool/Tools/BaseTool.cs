using UnityEngine;
using FracturedState.Game.State;

namespace FracturedState.ModTools
{
	/// <summary>
	/// An abstract implementation of the IToolState and IState interfaces that is meant to be used in conjunction with
	/// a ToolStateMachine<T>
	/// </summary>
    public abstract class BaseTool : IToolState, IState
    {
        public virtual void Enter()
        {
            return;
        }

        public virtual void Exit()
        {
            return;
        }

        public virtual void DrawToolOptions()
        {
			return;
        }

        public virtual void ExecuteTool()
        {
			return;
        }
    }
}