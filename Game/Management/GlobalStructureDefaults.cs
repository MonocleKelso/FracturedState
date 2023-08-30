using UnityEngine;

public class GlobalStructureDefaults : MonoBehaviour
{
    [SerializeField] private GameObject meterPrefab;
    public GameObject MeterPrefab => meterPrefab;
}