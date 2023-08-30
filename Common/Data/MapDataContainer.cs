using UnityEngine;
using FracturedState;
using FracturedState.Game.Data;

/// <summary>
/// A wrapper class around CustomMapData to facilitate overriding values for objects placed on maps and
/// writing that data to a map file.
/// </summary>
public class MapDataContainer : MonoBehaviour
{
	public CustomMapData MapData = new CustomMapData();

    public void SetName(string name)
    {
        MapData.BaseObjectName = name;
    }
	
	public void SetPosition()
	{
        MapData.PositionString = new Vec3String(gameObject.transform.position);
	}
	
	public void SetRotation()
	{
        MapData.RotationString = new Vec3String(gameObject.transform.eulerAngles);
	}
	
	/// <summary>
	/// A convenience method for populating position and rotation in a single call.  This just
	/// wraps calls to SetPosition and SetRotation.
	/// </summary>
	public void SetPositionRotation()
	{
        SetPosition();
        SetRotation();
	}
}