
namespace FracturedState.ModTools
{
    /// <summary>
    /// An interface for mod tool classes that turns the class into a listener for keyboard events
    /// </summary>
    public interface IKeyStrokeTool
    {
        void DoKeyStroke(UnityEngine.KeyCode keyCode);
    }
}