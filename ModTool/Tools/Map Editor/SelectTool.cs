using UnityEngine;
using FracturedState;
using FracturedState.Game;
using FracturedState.Game.State;
using FracturedState.Game.Data;

namespace FracturedState.ModTools
{
    /// <summary>
    /// A Map Editor tool for selecting props in the world and displaying basic manipulation
    /// options (location, rotation, etc) in the options pane
    /// </summary>
    public class SelectTool : OwnedTool<MapEditorToolManager>, IMouseDownListener
    {
		// position
        private Vec3String uiPos;
        private Vec3String curPos;
		
		// rotation
		private Vec3String uiRot;
		private Vec3String curRot;

        private PlaceObjectToolMouseFollow dragger;

        public SelectTool(MapEditorToolManager owner)
			: base(owner)
		{
			owner.RightClickAction = null;
		}

        public override void Enter()
        {
            if (owner.SelectedObject != null)
            {
                curPos = new Vec3String(owner.SelectedObject.transform.position);
                uiPos = curPos;
                curRot = new Vec3String(owner.SelectedObject.transform.eulerAngles);
                uiRot = curRot;
            }
        }

        public override void DrawToolOptions()
        {
            if (owner.SelectedObject != null)
            {
                GUILayout.BeginVertical();

				// position
                GUILayout.Label("Position");
                GUILayout.BeginHorizontal();
                GUILayout.Label("X");
                uiPos.X = GUILayout.TextField(uiPos.X);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Y");
                uiPos.Y = GUILayout.TextField(uiPos.Y);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Z");
                uiPos.Z = GUILayout.TextField(uiPos.Z);
                GUILayout.EndHorizontal();

				// rotation
				GUILayout.Label("Rotation");
				GUILayout.BeginHorizontal();
                GUILayout.Label("X");
                uiRot.X = GUILayout.TextField(uiRot.X);
                GUILayout.EndHorizontal();
				
				GUILayout.BeginHorizontal();
                GUILayout.Label("Y");
                uiRot.Y = GUILayout.TextField(uiRot.Y);
                GUILayout.EndHorizontal();
				
				GUILayout.BeginHorizontal();
                GUILayout.Label("Z");
                uiRot.Z = GUILayout.TextField(uiRot.Z);
                GUILayout.EndHorizontal();
				
                GUILayout.EndVertical();

				// apply position change if needed
                if (uiPos != curPos)
                {
					Vector3 pos = Vector3.zero;
					if (uiPos.TryVector3(out pos))
					{
						owner.SelectedObject.transform.position = pos;
						owner.SelectedObject.GetComponent<MapDataContainer>().SetPosition();
						curPos = uiPos;
					}
                }
				
				// apply rotation change if needed
				if (uiRot != curRot)
				{
					Vector3 rot = Vector3.zero;
					if (uiRot.TryVector3(out rot))
					{
                        owner.SelectedObject.transform.rotation = Quaternion.identity;
						owner.SelectedObject.transform.Rotate(rot);
						owner.SelectedObject.GetComponent<MapDataContainer>().SetRotation();
						curRot = uiRot;
					}
				}
            }
        }

        public virtual void ExecuteMouseDown()
        {
            if (dragger == null && owner.SelectedObject != null)
            {
                Ray ray = owner.MapCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, owner.ObjMask))
                {
                    Transform t = hit.transform.GetAbsoluteParent();
                    if (t.gameObject == owner.SelectedObject)
                    {
                        dragger = t.gameObject.AddComponent<PlaceObjectToolMouseFollow>();
                        dragger.cam = owner.MapCamera;
                        dragger.RayMask = GameConstants.TerrainMask;
                    }
                }
            }
        }

        protected override void DoToolExecution()
        {
            ClearSelection();
            Ray ray = owner.MapCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, owner.ObjMask))
            {
                Transform t = hit.transform.GetAbsoluteParent();
                owner.SelectedObject = t.gameObject;
                curPos = new Vec3String(owner.SelectedObject.transform.position);
                uiPos = curPos;
				curRot = new Vec3String(owner.SelectedObject.transform.eulerAngles);
				uiRot = curRot;
            }
        }

        protected void ClearSelection()
        {
            if (owner.SelectedObject != null)
            {
                owner.SelectedObject = null;
            }
            if (dragger != null)
            {
                GameObject.Destroy(dragger);
            }
        }

    }
}