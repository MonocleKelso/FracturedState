using UnityEngine;
using System.Collections.Generic;
using FracturedState.Game;

public class UnitHotKeyManager : MonoBehaviour
{
    const float camMoveDeltaTime = 1;

    public static UnitHotKeyManager Instance { get; private set; }

    static KeyCode[] teamKeys = new KeyCode[]
    {
        KeyCode.Alpha0,
        KeyCode.Alpha1,
        KeyCode.Alpha2,
        KeyCode.Alpha3,
        KeyCode.Alpha4,
        KeyCode.Alpha5,
        KeyCode.Alpha6,
        KeyCode.Alpha7,
        KeyCode.Alpha8,
        KeyCode.Alpha9
    };

    [SerializeField]
    CommonCameraController cam;

    Dictionary<KeyCode, List<Squad>> hotkeySquads = new Dictionary<KeyCode, List<Squad>>();
    KeyCode lastKey;
    float lastKeyTime;

    public void RemoveSquad(Squad squad)
    {
        foreach (var key in teamKeys)
        {
            var squadList = hotkeySquads[key];
            squadList.Remove(squad);
        }
    }

    void Awake()
    {
        if (Instance != null)
        {
            throw new FracturedStateException("Multiple instances of UnitHotKeyManager are not allowed");
        }
        Instance = this;
    }

    void OnEnable()
    {
        foreach (var key in teamKeys)
        {
            hotkeySquads[key] = new List<Squad>();
        }
    }

    void OnDisable()
    {
        hotkeySquads = new Dictionary<KeyCode, List<Squad>>();
    }

    void Update()
    {
        foreach (var key in teamKeys)
        {
            if (Input.GetKeyUp(key))
            {
                var hotkeySquad = hotkeySquads[key];
#if !UNITY_EDITOR
                bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
#else
                bool ctrl = Input.GetKey(KeyCode.Z);
#endif
                if (ctrl)
                {
                    hotkeySquad.Clear();
                    for (int i = 0; i < SelectionManager.Instance.SelectedUnits.Count; i++)
                    {
                        var unit = SelectionManager.Instance.SelectedUnits[i];
                        if (unit != null && unit.Squad != null && !hotkeySquad.Contains(unit.Squad))
                        {
                            hotkeySquad.Add(unit.Squad);
                        }
                    }
                }
                else
                {
                    if (hotkeySquad.Count > 0 && lastKey == key && Time.time - lastKeyTime < camMoveDeltaTime)
                    {
                        Vector3 loc = Vector3.zero;
                        for (int i = 0; i < hotkeySquad.Count; i++)
                        {
                            loc += hotkeySquad[i].GetAveragePosition();
                        }
                        loc /= hotkeySquad.Count;
                        cam.BattleMove(loc);
                    }
                    else
                    {
                        if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
                        {
                            SelectionManager.Instance.ClearSelection();
                        }
                        for (int i = 0; i < hotkeySquad.Count; i++)
                        {
                            for (int u = 0; u < hotkeySquad[i].Members.Count; u++)
                            {
                                if (hotkeySquad[i].Members[u] != null && hotkeySquad[i].Members[u].IsAlive)
                                {
                                    hotkeySquad[i].Members[u].OnSelected(true);
                                    break;
                                }
                            }
                        }
                    }
                    lastKey = key;
                    lastKeyTime = Time.time;
                }
            }
        }
    }
}