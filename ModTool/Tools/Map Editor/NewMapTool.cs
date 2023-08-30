using UnityEngine;

namespace FracturedState.ModTools
{
    /// <summary>
    /// A Map Editor tool that facilitates the creation of a new map.  Basic map properties are displayed in
    /// the options pane.  This tool is also responsible for destroying any old map data in the event a user
    /// makes multiple new maps per session.
    /// </summary>
    public class NewMapTool : OwnedTool<MapEditorToolManager>
	{	
		public NewMapTool(MapEditorToolManager owner)
			: base(owner)
		{
			owner.RightClickAction = null;
		}
		
		public override void DrawToolOptions()
		{
			GUILayout.BeginVertical();
			
			if (GUILayout.Button("Create Map"))
				CreateNewMap();
			
			GUILayout.EndVertical();
		}
		
		private void CreateNewMap()
		{
            owner.ClearCurrentMap();
		}
	}
}