using FracturedState.Game.Data;
using UnityEngine;

public class RallyPointHelper : MonoBehaviour
{
    public TerritoryData Territory { get; private set; }
    Camera mapCamera;

    public void Init(TerritoryData territory, Camera mapCamera)
    {
        Territory = territory;
        this.mapCamera = mapCamera;
    }

    void OnGUI()
    {
        Vector3 pos = mapCamera.WorldToScreenPoint(transform.position);
        pos.y = Screen.height - pos.y;
        GUI.Label(new Rect(pos.x, pos.y, 200, 100), "Rally point for \n" + Territory.Name);
    }
}