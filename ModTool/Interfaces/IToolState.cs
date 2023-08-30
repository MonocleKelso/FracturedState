
namespace FracturedState.ModTools
{
    /// <summary>
    /// A common interface for all tools in the Mod Tool suite.  This interface requires 2 method: one for drawing the tool's menu
    /// and the othe for executing the action that the tool performs
    /// </summary>
    public interface IToolState
    {
        void DrawToolOptions();

        void ExecuteTool();
    }
}