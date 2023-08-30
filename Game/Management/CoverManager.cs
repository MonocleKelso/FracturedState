using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FracturedState.Game;
using FracturedState.Game.Data;
using FracturedState;

public class CoverManager : MonoBehaviour
{
    public Transform[] CoverPoints;

    public List<Transform> EmptyPoints { get; private set; }
    private Dictionary<string, CoverPoint> coverPointLookup;
    private Dictionary<UnitManager, Transform> fullPoints;

    private Prop propData;

    public bool IsOccupied
    {
        get
        {
            return fullPoints.Values.Count > 0;
        }
    }

    public void Init(Prop prop)
    {
        propData = prop;
        EmptyPoints = new List<Transform>();
        fullPoints = new Dictionary<UnitManager, Transform>();
        coverPointLookup = new Dictionary<string, CoverPoint>();
        propData = XmlCacheManager.Props[gameObject.name];
        for (var i = 0; i < propData.CoverPoints.Length; i++)
        {
            Transform coverTran = transform.Find(propData.CoverPoints[i].ParentName);
            if (coverTran == null)
                throw new FracturedStateException("Bad cover point parent reference \"" + propData.CoverPoints[i].ParentName + "\" in \"" + gameObject.name + "\"");

            Transform pointTran = coverTran.Find(propData.CoverPoints[i].Name);
            if (pointTran != null)
            {
                EmptyPoints.Add(pointTran);
                coverPointLookup[pointTran.name] = propData.CoverPoints[i];
            }
        }
        CoverPoints = EmptyPoints.ToArray();
    }

    public void CheckExteriorPoints()
    {
        foreach (Transform point in CoverPoints)
        {
            Ray ray = new Ray(point.position + Vector3.up * 100, -Vector3.up);
            if (RaycastUtil.RayCheckExterior(ray))
            {
                EmptyPoints.Remove(point);
            }
        }
        CoverPoints = EmptyPoints.ToArray<Transform>();
    }

    public void CheckInteriorPoints(StructureManager structure)
    {
        if (CoverPoints != null)
        {
            foreach (Transform point in CoverPoints)
            {
                if (!structure.ContainsPoint(point.position + Vector3.up))
                {
                    EmptyPoints.Remove(point);
                }
                else
                {
                    Ray ray = new Ray(point.position + Vector3.up * 100, -Vector3.up);
                    if (RaycastUtil.RayCheckInterior(ray))
                    {
                        EmptyPoints.Remove(point);
                    }
                }
            }
            CoverPoints = EmptyPoints.ToArray<Transform>();
        }
    }

    /// <summary>
    /// Returns true if the given unit is able to take cover behind this object
    /// </summary>
    public bool CanOccupy(UnitManager unit)
    {
        // false if all points are taken
        if (EmptyPoints.Count == 0)
        {
            return false;
        }
        // false if unit is in different world state
        else if ((unit.WorldState == FracturedState.Game.Nav.State.Exterior && gameObject.layer == GameConstants.InteriorLayer) ||
            (unit.WorldState == FracturedState.Game.Nav.State.Interior && gameObject.layer == GameConstants.ExteriorLayer))
        {
            return false;
        }
        // true if not occupied or if occupied and this object can provide sumultaneous cover
        else if (!IsOccupied || propData.SimultCover)
        {
            return true;
        }
        // if occupied and doesn't provide simult cover then check to see if occupied by friendly
        else
        {
            UnitManager occUnit = fullPoints.Keys.First();
            return occUnit.OwnerTeam == unit.OwnerTeam;
        }
    }

    /// <summary>
    /// Calculates the closest open point to a given unit in straight distance, closes that point for other units,
    /// and returns the position of that point so that the unit may calculate a path to it. Does not check eligibility
    /// </summary>
    public Transform OccupyClosestPoint(UnitManager unit)
    {
        Transform closestPoint = GetClosestPointToPosition(unit.transform.position);
        if (closestPoint != null)
        {
            fullPoints.Add(unit, closestPoint);
            EmptyPoints.Remove(closestPoint);
        }
        return closestPoint;
    }

    /// <summary>
    /// Calculates the closest cover point to the given unit without occupying it. Does not check eligibility.
    /// </summary>
    public Transform GetClosestPoint(UnitManager unit)
    {
        return GetClosestPointToPosition(unit.transform.position);
    }

    /// <summary>
    /// Calculates the closest cover point to the given position in world coordinates
    /// </summary>
    public Transform GetClosestPointToPosition(Vector3 position)
    {
        float closest = float.MaxValue;
        float dist;
        Transform closestPoint = null;
        for (var i = 0; i < EmptyPoints.Count; i++)
        {
            dist = (position - EmptyPoints[i].position).sqrMagnitude;
            if (dist < closest)
            {
                closest = dist;
                closestPoint = EmptyPoints[i];
            }
        }
        return closestPoint;
    }

    public Transform ProcessOpenPoints(UnitManager unit, out int bonus)
    {
        bonus = 0;
        if (EmptyPoints.Count == 0)
            return null;

        if (fullPoints.ContainsKey(unit))
            return null;

        Transform point = null;
        if (unit.Squad.UseFacing)
        {
            float faceAngle = 2;
            for (int i = 0; i < EmptyPoints.Count; i++)
            {
                CoverPoint p = coverPointLookup[EmptyPoints[i].name];
                int b = p.GetDirectionalBonus(EmptyPoints[i], unit.Squad.FacingVector);
                float angle = Vector3.Dot(unit.Squad.FacingVector, EmptyPoints[i].forward);
                if (b > bonus || (b == bonus && 1 - angle < faceAngle))
                {
                    bonus = b;
                    faceAngle = angle;
                    point = EmptyPoints[i];
                }
            }
        }
        else
        {
            List<UnitManager> enemies = unit.Squad.GetVisibleForUnit(unit);
            if (enemies != null && enemies.Count > 0)
            {
                for (int i = 0; i < enemies.Count; i++)
                {
                    if (enemies[i] != null && enemies[i].IsAlive)
                    {
                        for (int e = 0; e < EmptyPoints.Count; e++)
                        {
                            CoverPoint p = coverPointLookup[EmptyPoints[e].name];
                            int b = p.GetBonus(EmptyPoints[e], enemies[i].transform.position);
                            if (b > bonus)
                            {
                                bonus = b;
                                point = EmptyPoints[e];
                            }
                        }
                    }
                }
            }
            else
            {
                // if no enemies are around then take point that is most defensible 
                // based on the unit's vector to the point
                for (int i = 0; i < EmptyPoints.Count; i++)
                {
                    CoverPoint p = coverPointLookup[EmptyPoints[i].name];
                    Vector3 direction = (EmptyPoints[i].position - unit.transform.position).normalized;
                    int b = p.GetDirectionalBonus(EmptyPoints[i], direction);
                    if (b > bonus)
                    {
                        bonus = b;
                        point = EmptyPoints[i];
                    }
                }
            }
        }
        return point;
    }

    public void UnoccupyPoint(UnitManager unit)
    {
        Transform point;
        if (fullPoints.TryGetValue(unit, out point))
        {
            fullPoints.Remove(unit);
            // duplicate values are allowed due to deferred cover point states
            // so only make available globally if no one else has claimed
            Transform[] points = fullPoints.Values.ToArray();
            if (!points.Contains(point))
            {
                EmptyPoints.Add(point);
            }
        }
    }

    public Transform GetPointByName(string name)
    {
        for (var i = 0; i < EmptyPoints.Count; i++)
        {
            if (EmptyPoints[i].name == name)
            {
                return EmptyPoints[i];
            }
        }
        // if point isn't in empty set due to deferred capture
        return transform.GetChildByName(name);
    }

    public void ReservePoint(Transform point)
    {
        EmptyPoints.Remove(point);
    }

    public void ReturnReservedPoint(Transform point)
    {
        if (!EmptyPoints.Contains(point))
            EmptyPoints.Add(point);
    }

    public void OccupyPoint(UnitManager unit, Transform point)
    {
        fullPoints[unit] = point;
        EmptyPoints.Remove(point);
    }

    public CoverPoint GetPointInfo(Transform point)
    {
        for (var i = 0; i < propData.CoverPoints.Length; i++)
        {
            if (propData.CoverPoints[i].Name == point.name)
                return propData.CoverPoints[i];
        }

        throw new FracturedStateException("Cannot retrieve info for point " + propData.Name + "." + point.name);
    }
}