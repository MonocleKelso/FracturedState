using UnityEngine;

public class ShroudCamera : MonoBehaviour
{
    [SerializeField]
    private Camera shroudCamera;
    [SerializeField]
    private Material shroudRenderMaterial;
    [SerializeField]
    private Material shroudObjectMaterial;

    public Material ShroudMaterial { get { return shroudObjectMaterial; } }

    public Color ShroudColor { get { return shroudCamera.backgroundColor; } }

    public GameObject ShroudPrefab;

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        shroudCamera.targetTexture = RenderTexture.GetTemporary(Screen.width, Screen.height, 16);
        shroudCamera.Render();
        shroudRenderMaterial.SetTexture("_ShroudTex", shroudCamera.targetTexture);
        Graphics.Blit(src, dest, shroudRenderMaterial);
        RenderTexture.ReleaseTemporary(shroudCamera.targetTexture);
    }
}