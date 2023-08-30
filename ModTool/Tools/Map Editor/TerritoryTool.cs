using UnityEngine;
using FracturedState.Game.Data;

namespace FracturedState.ModTools
{
    public class TerritoryTool : OwnedTool<MapEditorToolManager>
    {
        private TerritoryData currentTerritory, deferredTerritory;
        private TerritoryDataHandler handler;

        private bool deletePrompt;

        public TerritoryTool(MapEditorToolManager owner)
            :base(owner)
        {
            owner.RightClickAction = () =>
            {
                if (handler == null)
                {
                    if (currentTerritory != null)
                    {
                        currentTerritory = null;
                    }
                    else
                    {
                        var ray = this.owner.MapCamera.ScreenPointToRay(Input.mousePosition);
                        RaycastHit hit;
                        if (Physics.Raycast(ray, out hit, Mathf.Infinity, owner.TerrainMask))
                        {
                            owner.RemoveTerritoryAssignment(hit.transform.gameObject);
                        }
                    }
                }
            };
        }

        public override void Enter()
        {
            base.Enter();
            owner.ToggleTerritoryHelpers(true);
        }

        public override void Exit()
        {
            base.Exit();
            owner.ToggleTerritoryHelpers(false);
        }

        public override void DrawToolOptions()
        {
            GUILayout.BeginVertical();

            for (var i = 0; i < owner.Territories.Count; i++)
            {
                var territory = owner.Territories[i];
                var s = new GUIStyle {normal = {background = territory.EditorTexture}};
                GUILayout.BeginHorizontal();
                GUI.enabled = currentTerritory != territory;
                if (GUILayout.Button("-"))
                {
                    currentTerritory = territory;
                }
                GUI.enabled = true;
                if (GUILayout.Button(territory.Name))
                {
                    if (handler == null)
                    {
                        handler = new TerritoryDataHandler(territory, this);
                        // required to properly handle user pressing OK button on modal window when a territory is selected
                        deferredTerritory = currentTerritory;
                        currentTerritory = null;
                    }
                }
                GUILayout.Box(GUIContent.none, s);
                GUILayout.EndHorizontal();
            }
            
            if (GUILayout.Button("+"))
            {
                owner.Territories.Add(new TerritoryData("New Territory"));
            }

            GUI.enabled = currentTerritory != null;
            if (GUILayout.Button("Delete Selected Territory"))
            {
                deletePrompt = true;
            }
            GUI.enabled = true;

            GUILayout.EndVertical();

            if (deletePrompt)
            {
                var r = new Rect(Screen.width * 0.5f - 200, Screen.height * 0.5f - 100, 400, 200);
                GUI.ModalWindow(0, r, DeletePromptWindow, "Delete Territory?");
            }
        }

        private void DeletePromptWindow(int windowId)
        {
            GUILayout.BeginVertical();

            GUILayout.Label("Are you sure you want to delete the Territory Definition for " + currentTerritory.Name + "?\nAll terrain pieces belonging to this definition will be marked as unassigned.");

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Yes"))
            {
                DeleteTerritoryDefinition(currentTerritory);
                currentTerritory = null;
                deletePrompt = false;
            }
            if (GUILayout.Button("No"))
            {
                deletePrompt = false;
                // required to prevent click through on window and accidentally assign terrain underneath
                deferredTerritory = currentTerritory;
                currentTerritory = null;
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        protected override void DoToolExecution()
        {
            if (currentTerritory != null)
            {
                var ray = owner.MapCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, owner.TerrainMask))
                {
                    owner.AssignTerritory(hit.transform.gameObject, currentTerritory);
                }
            }
            else if (deferredTerritory != null && handler == null)
            {
                // required to properly handle user pressing OK button on modal window when a territory is selected
                currentTerritory = deferredTerritory;
                deferredTerritory = null;
            }
        }

        private void DeleteTerritoryDefinition(TerritoryData territory)
        {
            var helpers = Object.FindObjectsOfType<RallyPointHelper>();
            RallyPointHelper helper = null;
            foreach (var h in helpers)
            {
                if (h.Territory == territory)
                {
                    helper = h;
                }
            }
            if (helper != null)
            {
                Object.Destroy(helper);
            }
            owner.RemoveTerritoryDefinition(territory);
        }

        public void ClosePropertyWindow(TerritoryData territory)
        {
            owner.UpdateTerritoryData(territory);
            handler = null;
        }
    }
}