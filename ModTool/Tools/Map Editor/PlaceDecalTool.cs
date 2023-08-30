using FracturedState.Game;
using ThreeEyedGames;
using UnityEngine;

namespace FracturedState.ModTools
{
    public class PlaceDecalTool : OwnedTool<MapEditorToolManager>
    {
        private static Decal[] decalList;

        private Decal selectedDecal;
        private Decal currentMenuDecal;
        private bool maskPickMode;
        private bool maskIsExterior;
        
        public PlaceDecalTool(MapEditorToolManager owner) : base(owner)
        {
            owner.RightClickAction = DeSelect;
            if (decalList == null)
            {
                decalList = Resources.LoadAll<Decal>(DataLocationConstants.BuiltInDecalDirecotry);
            }
        }

        public override void DrawToolOptions()
        {
            GUILayout.BeginVertical();

            if (selectedDecal != null)
            {
                DecalOptions();
            }
            else
            {
                DecalList();
            }
            
            GUILayout.EndVertical();
        }

        protected override void DoToolExecution()
        {
            if (maskPickMode)
            {
                PickDecalMask();
                return;
            }
            
            var ray = owner.MapCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            // if we're not placing a decal then do selection of existing decal
            if (currentMenuDecal == null)
            {
                if (!Physics.Raycast(ray, out hit, Mathf.Infinity, GameConstants.DecalMask)) return;

                selectedDecal = hit.collider.gameObject.GetComponent<Decal>();
                owner.SelectDecal(selectedDecal);
                return;
            }
            
            if (!Physics.Raycast(ray, out hit, Mathf.Infinity, GameConstants.DecalPlaceMask)) return;

            selectedDecal = Object.Instantiate(currentMenuDecal, hit.point, currentMenuDecal.transform.rotation);
            selectedDecal.transform.up = hit.normal;
            selectedDecal.gameObject.name = currentMenuDecal.gameObject.name;
            selectedDecal.gameObject.AddComponent<BoxCollider>();
            selectedDecal.gameObject.SetLayerRecursively(GameConstants.DecalLayer);
            owner.SelectDecal(selectedDecal);
            currentMenuDecal = null;
        }

        private void PickDecalMask()
        {
            var ray = owner.MapCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (!Physics.Raycast(ray, out hit, Mathf.Infinity, GameConstants.ExteriorMask)) return;

            var parent = hit.transform.GetAbsoluteParent();
            // hacky - but make sure we clicked a structure and not an interior prop
            if (parent.parent.name != "Structures") return;
            
            // enumerate children of structure parent and grab the first one that matches the layer we picked
            // (there should only be two children per structure)
            foreach (Transform child in parent)
            {
                if (maskIsExterior && child.gameObject.layer == GameConstants.ExteriorLayer)
                {
                    selectedDecal.LimitTo = child.gameObject;
                    break;
                }

                if (!maskIsExterior && child.gameObject.layer == GameConstants.InteriorLayer)
                {
                    selectedDecal.LimitTo = child.gameObject;
                    break;
                }
            }
            maskPickMode = false;
        }
        
        private void DecalOptions()
        {
            if (maskPickMode)
            {
                GUILayout.Label("Pick structure");
                return;
            }
            
            if (GUILayout.Button("All"))
            {
                selectedDecal.LimitTo = null;
            }
            
            if (GUILayout.Button("Terrain"))
            {
                selectedDecal.LimitTo = GameObject.Find("MapParent").transform.Find("Terrain").gameObject;
            }

            if (GUILayout.Button("Exterior"))
            {
                maskPickMode = true;
                maskIsExterior = true;
            }

            if (GUILayout.Button("Interior"))
            {
                maskPickMode = true;
                maskIsExterior = false;
            }
            
            GUILayout.Label("Alpha");
            selectedDecal.Fade = GUILayout.HorizontalSlider(selectedDecal.Fade, 0, 1);
        }

        private void DecalList()
        {
            foreach (var decal in decalList)
            {
                if (GUILayout.Button(decal.gameObject.name))
                {
                    currentMenuDecal = decal;
                }
            }
        }

        public override void Exit()
        {
            DeSelect();
        }

        private void DeSelect()
        {
            if (maskPickMode)
            {
                maskPickMode = false;
                return;
            }
            currentMenuDecal = null;
            owner.Unselect();
            selectedDecal = null;
        }
    }
}