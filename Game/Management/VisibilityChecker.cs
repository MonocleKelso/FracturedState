using System.Collections.Generic;
using FracturedState.Game.Network;
using UnityEngine;

namespace FracturedState.Game.Management
{
    public sealed class VisibilityChecker
    {
        private static GameObject checkTimer;
        private static VisibilityChecker instance;
        public static VisibilityChecker Instance
        {
            get
            {
                if (instance == null)
                    instance = new VisibilityChecker();

                if (checkTimer == null)
                {
                    checkTimer = new GameObject("Visibility");
                    checkTimer.AddComponent<SightTimeStep>();
                }

                return instance;
            }
        }

        private readonly List<Squad> squads = new List<Squad>();
        private readonly HashSet<UnitManager> visibleUnits = new HashSet<UnitManager>();
        private readonly HashSet<UnitManager> allUnits = new HashSet<UnitManager>();

        private CommonCameraController camera;

        private VisibilityChecker() { }

        public void EvaluateVisibility()
        {
            if (!FracNet.Instance.LocalTeam.IsActive)
            {
                return;
            }
            
            if (camera == null)
            {
                camera = (CommonCameraController)Object.FindObjectOfType(typeof(CommonCameraController));
            }

            visibleUnits.Clear();
            foreach (var currentSquad in squads)
            {
                var squadUnits = currentSquad.GetVisibleUnits();

                foreach (var sUnit in squadUnits)
                {
                    if (sUnit == null || visibleUnits.Contains(sUnit))
                        continue;

                    foreach (var member in currentSquad.Members)
                    {
                        // null check in case the unit was destroyed since our last evaluation
                        if (member == null) continue;
                        
                        // if the units are in different world states and we're not in a fire point
                        if (member.WorldState != sUnit.WorldState && member.CurrentFirePoint == null) continue;
                        
                        // if our unit is inside a sight blocking collider then they can't see anything
                        var r = new Ray(member.transform.position + Vector3.up * 100, Vector3.down);
                        if (Physics.Raycast(r, 100, GameConstants.SightBlockerMask))
                        {
                            continue;
                        }

                        var dir = ((sUnit.transform.position + Vector3.up) - (member.transform.position + Vector3.up));
                        var ray = new Ray(member.transform.position + Vector3.up, dir);

                        if (member.WorldState == Nav.State.Interior && sUnit.WorldState == Nav.State.Interior)
                        {
                            if (member.CurrentStructure == sUnit.CurrentStructure)
                            {
                                visibleUnits.Add(sUnit);
                            }
                        }
                        else
                        {
                            var hits = Physics.RaycastAll(ray, dir.magnitude, GameConstants.ExteriorSightMask);
                            if (hits != null)
                            {
                                var canSee = true;
                                for (var h = 0; h < hits.Length; h++)
                                {
                                    if (hits[h].collider.gameObject.layer == GameConstants.SightBlockerLayer || !hits[h].collider.CompareTag(GameConstants.PropTag))
                                    {
                                        canSee = false;
                                        break;
                                    }
                                }
                                if (canSee)
                                {
                                    visibleUnits.Add(sUnit);
                                }
                            }
                        }
                    }
                }
            }

            // enumerate all registered units and set renderers based
            // on their inclusion in the visible set
            foreach (var unit in allUnits)
            {
                if (unit != null)
                {
                    SetVisibility(unit, visibleUnits.Contains(unit));
                }
            }
        }

        private static void SetVisibility(Component unit, bool visible)
        {
            var renders = unit.GetComponentsInChildren<Renderer>(true);
            for (var r = 0; r < renders.Length; r++)
            {
                renders[r].enabled = visible;
            }
        }

        public bool IsVisible(UnitManager unit)
        {
            return visibleUnits.Contains(unit);
        }

        public bool HasSight(UnitManager unit, UnitManager other)
        {
            // resolve instances where units might LOS check through a building successfully
            if (unit.CurrentFirePoint == null && unit.WorldState != other.WorldState) return false;
            
            var dir = ((other.transform.position + Vector3.up) - (unit.transform.position + Vector3.up));
            var ray = new Ray(unit.transform.position + Vector3.up, dir);
            int mask;
            if (unit.WorldState == Nav.State.Exterior)
            {
                mask = GameConstants.WorldMask;
            }
            else
            {
                if (other.WorldState == Nav.State.Exterior)
                {
                    mask = GameConstants.WorldMask;
                }
                else
                {
                    if (unit.CurrentStructure != other.CurrentStructure)
                    {
                        return false;
                    }
                    mask = GameConstants.InteriorMask;
                }
            }
            
            var hits = Physics.RaycastAll(ray, dir.magnitude, mask);
            if (hits == null)
                return true;

            foreach (var hit in hits)
            {
                if (!hit.collider.CompareTag(GameConstants.PropTag))
                {
                    return false;
                }
            }
            return true;
        }

        public void RegisterSquad(Squad squad)
        {
            if (!squads.Contains(squad))
                squads.Add(squad);
        }

        public void UnregisterSquad(Squad squad)
        {
            squads.Remove(squad);
        }

        public void RegisterUnit(UnitManager unit)
        {
            allUnits.Add(unit);
        }

        public void UnregisterUnit(UnitManager unit)
        {
            allUnits.Remove(unit);
        }

        public void StopChecking()
        {
            Object.Destroy(checkTimer);
            checkTimer = null;
        }
    }
}