using UnityEngine;
using UnityEngine.UI;

public class CaptureProgressUI : MonoBehaviour
{
    private GameObject meterInstance;
    private Transform meterTransform;
    private Slider progress;
    private Text progressText;

    private StructureManager owner;
    private GlobalStructureDefaults defaultValues;

    public void Init(GlobalStructureDefaults defaults, StructureManager ownerStructure)
    {
        defaultValues = defaults;
        owner = ownerStructure;
    }

    public void SetColor(Color color)
    {
        color.a = 0.5f;
        progress.fillRect.GetComponent<Image>().color = color;
    }
    
    private void OnEnable()
    {
        if (owner == null) return;
        
        meterInstance = Instantiate(defaultValues.MeterPrefab);
        meterTransform = meterInstance.transform.GetChild(0);
        meterTransform.position = owner.transform.position + Vector3.up * 10;
        progress = meterInstance.GetComponentInChildren<Slider>();
        progressText = meterInstance.GetComponentInChildren<Text>();
    }

    private void OnDisable()
    {
        if (meterInstance == null) return;
        
        Destroy(meterInstance);
    }

    private void Update()
    {
        var screenPos = Camera.main.WorldToScreenPoint(owner.transform.position);
        meterTransform.position = screenPos;
        progress.value = owner.CurrentPoints / owner.StructureData.CapturePoints;
        progressText.text = $"{Mathf.Floor(owner.CurrentPoints)}/{owner.StructureData.CapturePoints}";
    }
}