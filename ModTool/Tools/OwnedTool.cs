using UnityEngine;
using FracturedState.Game.State;

namespace FracturedState.ModTools
{
    /// <summary>
    /// A tool who is owned by a manager that needs to suppress execution due to menu interactions.
    /// This class wraps the call to ExecuteTool in logic that prevents execution if the mouse cursor is on a menu.
    /// Derivatives of this class should override DoToolExecution instead of ExecuteTool.
    /// </summary>
    public class OwnedTool<T> : BaseTool where T : MenuSuppressedManager
    {
        protected T owner;

        public T Owner { get { return owner; } }

        public OwnedTool(T owner)
        {
            this.owner = owner;
        }

        public override void ExecuteTool()
        {
            if (!owner.CursorInMenu())
                DoToolExecution();
        }

        protected virtual void DoToolExecution()
        {
            return;
        }
    }
}