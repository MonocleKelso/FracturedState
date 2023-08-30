using FracturedState.Game;
using FracturedState.Game.Data;
using FracturedState.Game.Management;
using FracturedState.Game.Network;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FracturedState;
using FracturedState.Game.Management.StructureBonus;
using UnityEngine;

public class StructureHelperUI : MonoBehaviour
{
    private static Camera mainCamera;
    private static GUISkin skin;
    private static GUIStyle flavorStyle;
    private static GUIStyle iconStyle;
    private static GUIStyle bonusStyle;
    private static Texture2D flagIcon;
    private static Texture2D healthIcon;

    private StructureManager owner;
    private string structureName;
    private string description;
    private bool isMine;
    private Vector3 screenPos;
    private Rect container;

    private List<UnitObject> unlockUnits;
    private string bonus;

    private void Start()
    {
        owner = GetComponent<StructureManager>();
        structureName = string.IsNullOrEmpty(owner.StructureData.DisplayName) ? owner.StructureData.Name : owner.StructureData.DisplayName;
        bonus = StructureBonusLookup.GetStructureBonus(owner.StructureData.Name)?.HelperText;
        if (!string.IsNullOrEmpty(bonus)) bonus = LocalizedString.GetString(bonus);
        
        var team = FracNet.Instance.NetworkActions.LocalTeam;
        var flavorText = owner.StructureData.FlavorTexts?.FirstOrDefault<FactionFlavorText>(t => t.Faction == team.Faction);
        if (flavorText != null)
        {
            description = flavorText.Text;
        }

        unlockUnits = new List<UnitObject>();
        var teamFaction = XmlCacheManager.Factions[team.Faction];
        if (owner.StructureData.Unlockables?.Units == null) return;
        
        foreach (var unitName in owner.StructureData.Unlockables.Units)
        {
            var unit = XmlCacheManager.Units[unitName];
            if (SkirmishVictoryManager.IsSpectating || teamFaction.TrainableUnits.FirstOrDefault(u => u == unit.Name) != null)
            {
                unlockUnits.Add(unit);
            }
        }
    }

    private void OnEnable()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        if (skin == null)
        {
            skin = Resources.Load<GUISkin>("Textures/UI/HoverSkin");
            flavorStyle = skin.GetStyle("FlavorText");
            iconStyle = skin.GetStyle("BigIcon");
            bonusStyle = skin.GetStyle("BonusText");
        }
        if (flagIcon == null)
        {
            flagIcon = Resources.Load<Texture2D>("Textures/UI/FlagIcon");
        }
        if (healthIcon == null)
        {
            healthIcon = Resources.Load<Texture2D>("Textures/UI/HealthIcon");
        }
    }

    private void Update()
    {
        isMine = owner.OwnerTeam != null && owner.OwnerTeam.IsHuman && owner.OwnerTeam == FracNet.Instance.LocalTeam;
        screenPos = mainCamera.WorldToScreenPoint(owner.transform.position);
        float leftAdjust = owner.StructureData.Unlockables != null ? 125 : 75;
        container = new Rect(screenPos.x - leftAdjust, Screen.height - screenPos.y, 250, 500);
    }

    private void OnGUI()
    {
        GUI.skin = skin;

        GUILayout.BeginArea(container);
        GUILayout.BeginHorizontal();

        if (unlockUnits.Count > 0)
        {
            GUILayout.BeginArea(new Rect(0, 0, 50, container.height));
            GUILayout.BeginVertical();
            for (var i = 0; i < unlockUnits.Count; i++)
            {
                GUILayout.Label(unlockUnits[i].Icon, iconStyle, GUILayout.Width(50), GUILayout.Height(50));
            }
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        if (unlockUnits.Count > 0)
        {
            GUILayout.BeginArea(new Rect(50, 0, container.width - 50, container.height));
        }
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        GUILayout.Label(structureName);
        if (owner.StructureData.CapturePoints > 0)
        {
            var capContent = new GUIContent(owner.StructureData.CapturePoints.ToString(CultureInfo.InvariantCulture), flagIcon);
            GUILayout.Label(capContent);
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        if (!string.IsNullOrEmpty(description))
        {
            GUILayout.Label(description, flavorStyle);
        }

        if (!string.IsNullOrEmpty(bonus))
        {
            GUILayout.Label(bonus, bonusStyle);
        }
        
        if (isMine && owner.CurrentPoints >= owner.StructureData.CapturePoints)
        {
            if (GUILayout.Button("Relinquish"))
            {
                FracNet.Instance.NetworkActions.CmdReleaseStructure(owner.gameObject.GetComponent<Identity>().UID);
            }
        }
        
        GUILayout.EndVertical();
        if (unlockUnits.Count > 0)
        {
            GUILayout.EndArea();
        }
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }
}