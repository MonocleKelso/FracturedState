using UnityEngine;
using FracturedState.Game.State;
using FracturedState.ModTools;

/// <summary>
/// A class representing a tool manager that needs to check
/// whether or not a user is clicking in a menu area in order to suppress
/// the execution of a tool.  This is mainly in cases where a left-click action
/// for a tool would manipulate something in the world (see map editor tools).
/// </summary>
public class MenuSuppressedManager : ToolManager
{
	/// <summary>
	/// Returns true if the mouse cursor is currently over a menu area
	/// </summary>
	public virtual bool CursorInMenu()
	{
        Vector2 mousePos = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
        return optionsRect.Contains(mousePos) || toolBarRect.Contains(mousePos);
	}
}