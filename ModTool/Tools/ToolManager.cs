using UnityEngine;
using FracturedState.Game.State;
using FracturedState.ModTools;

public class ToolManager : MonoBehaviour
{
	protected static Rect toolBarRect = new Rect(0, 25, Screen.width, 30);
	protected static Rect optionsRect = new Rect(Screen.width - 200, 57, 200, Screen.height - 57);
	
    public GameObject ToolParent;
    protected ToolStateMachine<BaseTool> stateMachine;
	
	public bool NeedsInit = true;

    protected ModToolManager manager;

    void OnEnable()
    {
        if (ToolParent != null)
            ToolParent.SetActive(true);
			
		if (NeedsInit)
		{
			Init();
			NeedsInit = false;
		}
    }
	
	/// <summary>
	/// Provides an initialization hook for defaulting module behavior
	/// and setting default tools.
	/// </summary>
	protected virtual void Init()
	{
        manager = FindObjectOfType<ModToolManager>();
	}
	
	void OnDisable()
	{
		if (ToolParent != null)
			ToolParent.SetActive(false);
	}

	/// <summary>
	/// Draws a module's buttons
	/// Override this in child classes
	/// </summary>
	public virtual void DrawToolBar()
	{
		return;
	}
	
	void OnGUI()
	{
        GUI.skin = manager.Skin;
        GUI.Box(toolBarRect, GUIContent.none);

		// Tool bar container with call to overridden
		// module-specific buttons
		GUILayout.BeginArea(toolBarRect);
		GUILayout.BeginHorizontal();
		DrawToolBar();
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
		
		// draw the currently selected tool's options
		GUILayout.BeginArea(optionsRect);
		if (stateMachine != null)
			stateMachine.DrawToolState();
		GUILayout.EndArea();

        // if the current tool uses hotkeys then pass along keyboard events
        if (stateMachine.IsKeyStrokeState)
        {
            Event e = Event.current;
            if (e.type == EventType.KeyDown)
            {
                stateMachine.ProcessHotKey(e.keyCode);
            }
        }
	}
	
    public void ChangeState(BaseTool state)
    {
		stateMachine.ChangeState(state);
    }
}