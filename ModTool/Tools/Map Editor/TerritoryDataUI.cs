using FracturedState.Game.Data;
using FracturedState.ModTools;
using UnityEngine;

public class TerritoryDataUI : MonoBehaviour
{
    private TerritoryDataHandler handler;
    private TerritoryData territory;

    private Rect wRect;
    private string tPop = "0";
    private int tPopInt;

    float r, g, b;

    GUISkin skin;

    bool settingRallyPoint;

    public void Init(TerritoryDataHandler handler, TerritoryData territory)
    {
        this.handler = handler;
        this.territory = territory;
        wRect = new Rect(Screen.width * 0.5f - 150, Screen.height * 0.5f - 150, 300, 300);
        r = territory.UnityColor.r;
        g = territory.UnityColor.g;
        b = territory.UnityColor.b;
        tPop = territory.PopulationBonus.ToString();
        skin = FindObjectOfType<ModToolManager>().Skin;
    }

    void Update()
    {
        if (settingRallyPoint && Input.GetMouseButtonDown(0))
        {
            Ray ray = handler.Owner.Owner.MapCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, handler.Owner.Owner.TerrainMask))
            {
                territory.RallyPoint = new FracturedState.Vec3String(hit.point);
                settingRallyPoint = false;
                var helpers = FindObjectsOfType<RallyPointHelper>();
                RallyPointHelper helper = null;
                foreach (var h in helpers)
                {
                    if (h.Territory == territory)
                    {
                        helper = h;
                    }
                }
                if (helper == null)
                {
                    GameObject g = new GameObject("TerritoryRallyHelper");
                    helper = g.AddComponent<RallyPointHelper>();
                }
                helper.transform.position = hit.point;
                helper.Init(territory, handler.Owner.Owner.MapCamera);
            }
        }
    }

    void OnGUI()
    {
        if (!settingRallyPoint)
        {
            GUI.skin = skin;
            wRect = GUI.ModalWindow(0, wRect, DoWindow, "Properties: " + territory.Name);
            territory.UpdateColor(new Color(r, g, b));
        }
    }

    void DoWindow(int windowId)
    {
        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Name:");
        territory.Name = GUILayout.TextField(territory.Name, GUILayout.MinWidth(150), GUILayout.ExpandWidth(false));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Can Reinforce:");
        territory.Recruit = GUILayout.Toggle(territory.Recruit, GUIContent.none);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        if (int.TryParse(tPop, out tPopInt))
        {
            territory.PopulationBonus = tPopInt;
        }
        else
        {
            territory.PopulationBonus = 0;
            tPop = "";
        }

        GUILayout.BeginHorizontal();
        GUILayout.Label("Population:");
        tPop = GUILayout.TextField(tPop, GUILayout.MinWidth(50), GUILayout.ExpandWidth(false));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Color:");

        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Red:", GUILayout.MinWidth(50));
        r = GUILayout.HorizontalSlider(r, 0, 1, GUILayout.MinWidth(100));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Green:", GUILayout.MinWidth(50));
        g = GUILayout.HorizontalSlider(g, 0, 1, GUILayout.MinWidth(100));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Blue:", GUILayout.MinWidth(50));
        b = GUILayout.HorizontalSlider(b, 0, 1, GUILayout.MinWidth(100));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        GUI.enabled = territory.Recruit;
        if (GUILayout.Button("Set Rally Location", GUILayout.MaxWidth(200)))
        {
            settingRallyPoint = true;
        }
        GUI.enabled = true;

        GUILayout.FlexibleSpace();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("OK", GUILayout.MaxWidth(200)))
        {
            handler.Close();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
    }
}