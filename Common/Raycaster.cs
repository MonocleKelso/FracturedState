using System.Collections.Generic;
using FracturedState.Game;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FracturedState
{
    public static class RaycastUtil
    {
        private static List<RaycastResult> uiResult = new List<RaycastResult>();
        
        public static bool IsMouseInUI()
        {
            var data = new PointerEventData(EventSystem.current) {position = Input.mousePosition};
            EventSystem.current.RaycastAll(data, uiResult);
            var count = uiResult.Count;
            var onRecruit = count == 1 && uiResult[0].gameObject.CompareTag("Respawn");
            uiResult.Clear();
            if (onRecruit) return false;
            
            return count > 0;
        }
        
        public static RaycastHit RaycastTerrain(Vector3 position)
        {
            return DoRaycast(new Ray(position, -Vector3.up), GameConstants.TerrainMask);
        }

        public static bool IsUnderTerrain(Vector3 position)
        {
            return DoRaycastNoHit(new Ray(position, -Vector3.up), GameConstants.TerrainMask);
        }

        public static bool IsMouseUnderTerrain()
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            return DoRaycastNoHit(ray, GameConstants.TerrainMask);
        }

        public static bool IsMouseUnderExteriorUnit()
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            return DoRaycastNoHit(ray, GameConstants.ExteriorUnitAllMask);
        }

        public static bool IsMouseUnderInteriorUnit()
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            return DoRaycastNoHit(ray, GameConstants.InteriorUnitAllMask);
        }

        public static StructureManager RaycastStructureAtMouse()
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var hit = DoRaycast(ray, GameConstants.ExteriorMask);
            return hit.transform != null ? hit.transform.GetAbsoluteParent().GetComponent<StructureManager>() : null;
        }
        
        public static UnitManager RaycastFriendlyAtMouse()
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, Mathf.Infinity, GameConstants.FriendlyUnitMask))
            {
                return rayHit.collider.GetComponent<UnitManager>();
            }
            return null;
        }

        public static UnitManager RaycastEnemyAtMouse()
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var hit = DoRaycast(ray, GameConstants.EnemyUnitMask);
            if (hit.transform != null)
            {
                return hit.transform.GetComponent<UnitManager>();
            }
            return null;
        }

        public static UnitManager RaycastExteriorEnemyAtMouse()
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var hit = DoRaycast(ray, GameConstants.ExteriorEnemyMask);
            if (hit.transform != null)
            {
                return hit.transform.GetComponent<UnitManager>();
            }
            return null;
        }

        public static UnitManager RaycastInteriorEnemyAtMouse()
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var hit = DoRaycast(ray, GameConstants.InteriorEnemyMask);
            if (hit.transform != null)
            {
                return hit.transform.GetComponent<UnitManager>();
            }
            return null;
        }

        public static RaycastHit RaycastTerrainAtMouse()
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            return DoRaycast(ray, GameConstants.TerrainMask);
        }

        public static RaycastHit RaycastExteriorAtMouse()
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            return DoRaycast(ray, GameConstants.ExteriorObjectMask);
        }

        public static RaycastHit RaycastExterior(Vector3 position)
        {
            return DoRaycast(new Ray(position, -Vector3.up), GameConstants.ExteriorMask);
        }

        public static RaycastHit RaycastInterior(Vector3 position)
        {
            return DoRaycast(new Ray(position, -Vector3.up), GameConstants.InteriorMask);
        }

        public static bool RayCheckExterior(Ray ray)
        {
            return DoRaycastNoHit(ray, GameConstants.ExteriorMask);
        }

        public static bool RayCheckInterior(Ray ray)
        {
            return DoRaycastNoHit(ray, GameConstants.InteriorMask);
        }

        public static bool RayCheckUi()
        {
            return EventSystem.current.currentSelectedGameObject != null;
        }
        
        public static RaycastHit RayCheckWeaponBlock(Vector3 start, Vector3 target)
        {
            var dir = target - start;
            var ray = new Ray(start, dir.normalized);
            RaycastHit hit;
            Physics.Raycast(ray, out hit, dir.magnitude, GameConstants.WeaponBlockMask);
            return hit;
        }

        private static RaycastHit DoRaycast(Ray ray, int layerMask)
        {
            RaycastHit hit;
            Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask);
            return hit;
        }

        private static bool DoRaycastNoHit(Ray ray, int layerMask)
        {
            return Physics.Raycast(ray, Mathf.Infinity, layerMask);
        }
    }
}