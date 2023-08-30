using UnityEngine;
using FracturedState.UI;

public class CompassMenuManager : MonoBehaviour
{
    [SerializeField()]
    private Camera compassCamera;

    [SerializeField()]
    private Material compassBGMaterial;
    [SerializeField()]
    private Material compassDirectionMaterial;
    [SerializeField()]
    private CompassUI ui;

    public CompassUI UI => ui;

    private Vector3 center;
    private GameObject compassDirections;

    public Rect CompassRect { get; private set; }

    private GameObject drawer;

    private void Start()
    {
        CompassRect = new Rect(UnityEngine.Screen.width - 249, UnityEngine.Screen.height - 136, 249, 136);
        compassCamera.pixelRect = CompassRect;
        ui.SetRecruitPanel(CompassRect);
        if (drawer != null)
        {
            DestroyImmediate(drawer);
        }
        drawer = new GameObject("Compass");
        var r = drawer.AddComponent<MeshRenderer>();
        var f = drawer.AddComponent<MeshFilter>();
        f.mesh = InterfaceManager.MakeFullCameraQuad(compassCamera);
        r.material = compassBGMaterial;

        compassDirections = new GameObject("Directions");
        var mesh = InterfaceManager.MakeCameraQuad(compassCamera, 20, 132, 114, 114, 0.1f);
        center = Vector3.zero;
        for (var i = 0; i < 4; i++)
        {
            center += mesh.vertices[i];
        }
        center /= 4;
        r = compassDirections.AddComponent<MeshRenderer>();
        f = compassDirections.AddComponent<MeshFilter>();
        f.mesh = mesh;
        r.material = compassDirectionMaterial;
        compassDirections.transform.parent = drawer.transform;
    }

    public void RotateCompass(float amount)
    {
        compassDirections.transform.RotateAround(center, Vector3.forward, amount);
    }

    public void SetEnabled(bool compassEnabled)
    {
        drawer.SetActive(compassEnabled);
        UI.Draw = compassEnabled;
    }
}