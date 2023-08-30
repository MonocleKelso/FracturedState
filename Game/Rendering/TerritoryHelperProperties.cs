using UnityEngine;

public class TerritoryHelperProperties : MonoBehaviour
{
    [SerializeField]
    Material helperMaterial;

    public Material HelperMaterial { get { return helperMaterial; } }
}