using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FracturedState.Game.AI;
using FracturedState.Game.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FracturedState.Game
{
    public class SkillManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public static Ability ActiveSKill { get; private set; }
        public static Ability HoverSkill { get; private set; }

        public static void ResetSkill()
        {
            ActiveSKill = null;
        }

        public static void VerifyActiveSkill()
        {
            if (ActiveSKill == null) return;
            if (SelectionManager.Instance.SelectedUnits.Count == 0)
            {
                ResetSkill();
                return;
            }
            bool good = false;
            foreach (var unit in SelectionManager.Instance.SelectedUnits)
            {
                if (unit.HasAbility(ActiveSKill.Name))
                {
                    good = true;
                    break;
                }
            }
            if (!good) ResetSkill();
        }
        
        private const float CapWidth = 64;
        private const float MidWidth = 54;
        private const float SingleWidth = 74;
        private const float CapAdjustment = 5;
        private static readonly Vector3 LeftPosition = new Vector3(CapAdjustment, 0, 0);
        private static readonly Vector3 RightPosition = new Vector3(-CapAdjustment, 0, 0);

        [SerializeField] private Sprite mid;
        [SerializeField] private Sprite leftEnd;
        [SerializeField] private Sprite rightEnd;
        [SerializeField] private Sprite single;
        
        [SerializeField] private Button skillButton;

        [SerializeField] private Text availCount;
        [SerializeField] private Text cooldown;

        [SerializeField] private Text hotKeyDisplay;
        
        private Ability skill;
        private static Dictionary<string, KeyCode> keyMap;
        private KeyCode hotkey;
        
        private static Color32 AvailableCountColor = new Color32(120, 255, 0, 255);
        
        private void BindHotkey()
        {
            if (string.IsNullOrEmpty(skill.Hotkey))
            {
                hotKeyDisplay.text = "";
                return;
            }
            if (keyMap == null)
            {
                keyMap = new Dictionary<string, KeyCode>();
                foreach (var key in Enum.GetValues(typeof(KeyCode)))
                {
                    keyMap[key.ToString()] = (KeyCode)key;
                }
            }
            hotkey = keyMap[skill.Hotkey];
            hotKeyDisplay.text = skill.Hotkey;
        }
        
        public void Init(Ability skill, bool first, bool last)
        {
            this.skill = skill;
            skillButton.image.sprite = Ability.GetIcon(skill.IconAtlas, skill.IconPath);
            cooldown.gameObject.SetActive(false);
            BindHotkey();
            if (first && last)
            {
                GetComponent<Image>().sprite = single;
                GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, SingleWidth);
                skillButton.transform.localPosition = Vector3.zero;
                cooldown.transform.localPosition = Vector3.zero;
                var countPos = availCount.transform.localPosition;
                countPos.x = 0;
                availCount.transform.localPosition = countPos;

            }
            else if (first)
            {
                GetComponent<Image>().sprite = leftEnd;
                GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, CapWidth);
                skillButton.transform.localPosition = LeftPosition;
                cooldown.transform.localPosition = LeftPosition;
                var countPos = availCount.transform.localPosition;
                countPos.x = CapAdjustment;
                availCount.transform.localPosition = countPos;
            }
            else if (last)
            {
                GetComponent<Image>().sprite = rightEnd;
                GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, CapWidth);
                skillButton.transform.localPosition = RightPosition;
                cooldown.transform.localPosition = RightPosition;
                var countPos = availCount.transform.localPosition;
                countPos.x = -CapAdjustment;
                availCount.transform.localPosition = countPos;
            }
            else
            {
                GetComponent<Image>().sprite = mid;
                GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, MidWidth);
                skillButton.transform.localPosition = Vector3.zero;
                cooldown.transform.localPosition = Vector3.zero;
                var countPos = availCount.transform.localPosition;
                countPos.x = 0;
                availCount.transform.localPosition = countPos;
            }
        }

        public void DoSkill()
        {
            SkillBarManager.HideToolTip();
            if (skill.Type == AbilityType.PerSquad)
            {
                DoSquadSkill();
            }
            else if (skill.Type == AbilityType.PerUnit)
            {
                DoUnitSKill();
            }
        }

        private void DoSquadSkill()
        {
            if (skill.Targetting == TargetType.None)
            {
                var squads = new HashSet<Squad>();
                foreach (var unit in SelectionManager.Instance.SelectedUnits)
                {
                    if (unit == null) continue;
                    if (unit.Squad == null) continue;
                    if (squads.Add(unit.Squad))
                    {
                        unit.Squad.ExecuteSquadAbility(skill);
                    }
                }
            }
            else
            {
                ActiveSKill = skill;
            }
        }

        private void DoUnitSKill()
        {
            if (skill.Targetting == TargetType.Ground)
            {
                ActiveSKill = skill;
                var indicator = DataUtil.LoadGroundIndicator(skill.IndicatorPath);
                var man = indicator.GetComponent<ISkillIndicatorManager>();
                man?.SetSkill(skill);
            }
            else if (skill.Targetting == TargetType.None)
            {
                ActiveSKill = skill;
                for (int i = 0; i < SelectionManager.Instance.SelectedUnits.Count; i++)
                {
                    var unit = SelectionManager.Instance.SelectedUnits[i];
                    if (unit != null && unit.IsAlive && unit.HasAbility(skill.Name))
                    {
                        var indicator = Instantiate(SkillBarManager.GetWorldIndicator(), SkillBarManager.WorldSkillParent);
                        var manager = indicator.GetComponent<ISkillIndicatorManager>();
                        manager?.SetSkill(skill);
                        var unitBind = indicator.GetComponent<ISkillUnitBinder>();
                        unitBind?.SetUnit(unit);
                    }
                }
            }
        }

        private void Update()
        {
            if (skill.Type == AbilityType.PerSquad)
            {
                UpdateSquadCount();
            }
            else if (skill.Type == AbilityType.PerUnit)
            {
                UpdateUnitCount();
            }
            
            MonitorHotkey();

            if (ActiveSKill == skill)
            {
                MonitorActiveSkill();
            }
        }

        private void MonitorHotkey()
        {
            if (IngameChatManager.Instance.ChatInputOpen || string.IsNullOrEmpty(skill.Hotkey)) return;
            if (Input.GetKeyUp(hotkey)) DoSkill();
        }
        
        private void MonitorActiveSkill()
        {
            if (skill.Type != AbilityType.PerSquad) return;
            if (skill.Targetting != TargetType.Enemy && skill.Targetting != TargetType.Structure) return;
            if (Input.GetMouseButtonUp(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                ResetSkill();
                return;
            }
            if (Input.GetMouseButtonUp(0))
            {
                if (skill.Targetting == TargetType.Enemy)
                {
                    var enemy = RaycastUtil.RaycastEnemyAtMouse();
                    if (enemy == null) return;
                    ExecuteForClosestSquad(enemy);
                }
                else if (skill.Targetting == TargetType.Structure)
                {
                    var structure = RaycastUtil.RaycastStructureAtMouse();
                    if (structure != null)
                    {
                        var squads = SelectionManager.Instance.SelectedUnits.Select(u => u.Squad).Distinct().ToList();
                        foreach (var squad in squads)
                        {
                            squad?.ExecuteSquadAbility(skill);
                        }
                        ResetSkill();
                    }
                }
            }
        }

        private void ExecuteForClosestSquad(UnitManager target)
        {
            var dist = float.MaxValue;
            Squad exSquad = null;
            var squads = new HashSet<Squad>();
            foreach (var unit in SelectionManager.Instance.SelectedUnits)
            {
                if (unit == null || unit.Squad == null || !squads.Add(unit.Squad)) continue;
                var pos = Vector3.zero;
                foreach (var member in unit.Squad.Members)
                {
                    if (member == null) continue;
                    pos += member.transform.position;
                }
                pos /= unit.Squad.Members.Count;
                if ((pos - target.transform.position).sqrMagnitude < dist)
                {
                    dist = pos.sqrMagnitude;
                    exSquad = unit.Squad;
                }
            }
            if (exSquad == null) return;
            exSquad.ExecuteSquadAbility(skill);
            ResetSkill();
        }

        private void UpdateSquadCount()
        {
            var minCool = float.MaxValue;
            var squads = new HashSet<Squad>();
            for (var i = 0; i < SelectionManager.Instance.SelectedUnits.Count; i++)
            {
                var unit = SelectionManager.Instance.SelectedUnits[i];
                if (unit != null && unit.Squad != null && !squads.Contains(unit.Squad))
                {
                    var skills = unit.GetSquadAbilities();
                    if (skills == null) continue;
                    
                    foreach (var s in skills)
                    {
                        if (s.Name != skill.Name) continue;
                        var time = unit.GetRemainingAbilityTime(s.Name);
                        if (time <= 0)
                        {
                            squads.Add(unit.Squad);
                            break;
                        }
                        
                        if (time < minCool)
                        {
                            minCool = time;
                        }
                    }
                }
            }
            UpdateDisplay(squads.Count, minCool);
        }

        private void UpdateUnitCount()
        {
            var minCool = float.MaxValue;
            var count = 0;
            for (int i = 0; i < SelectionManager.Instance.SelectedUnits.Count; i++)
            {
                var unit = SelectionManager.Instance.SelectedUnits[i];
                if (unit != null && unit.IsAlive && unit.HasAbility(skill.Name))
                {
                    if (!skill.SkipStateValidation && unit.StateMachine.CurrentState is MicroUseAbilityState) continue;
                    
                    var cool = unit.GetRemainingAbilityTime(skill.Name);
                    if (cool <= 0)
                    {
                        count++;
                    }
                    else if (cool < minCool)
                    {
                        minCool = cool;
                    }
                }
            }
            UpdateDisplay(count, minCool);
        }

        private void UpdateDisplay(int count, float coolTime)
        {
            var oldCount = 999;
            if (!string.IsNullOrEmpty(availCount.text)) int.TryParse(availCount.text, out oldCount);
            
            availCount.text = count.ToString();
            if (oldCount < count)
            {
                StartCoroutine(LerpAvailableColor(availCount));
            }
            cooldown.gameObject.SetActive(count == 0);
            skillButton.interactable = count > 0 && ActiveSKill == null;
            if (!cooldown.gameObject.activeSelf) return;
            
            if (coolTime == float.MaxValue)
            {
                cooldown.text = "";
                return;
            }

            cooldown.text = coolTime < 2 ? coolTime.ToString("0.0") : Mathf.CeilToInt(coolTime).ToString();
        }

        private IEnumerator LerpAvailableColor(Text a)
        {
            var time = 0f;
            var scale = Vector3.one * 2;
            a.transform.localScale = scale;
            while (time < 1f)
            {
                var t = time / 1f;
                a.color = Color32.Lerp(Color.white, AvailableCountColor, t);
                a.transform.localScale = Vector3.Lerp(scale, Vector3.one, t);
                time += Time.deltaTime;
                yield return null;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            SkillBarManager.ShowToolTip(transform as RectTransform, skill);
            HoverSkill = skill;
            var inds = FindObjectsOfType<WorldCooldownIndicator>();
            foreach (var i in inds)
            {
                Destroy(i.gameObject);
            }

            if (ActiveSKill != null) return;
            
            if (skill.Type != AbilityType.PerUnit) return;
            
            foreach (var unit in SelectionManager.Instance.SelectedUnits)
            {
                if (unit.HasAbility(skill.Name))
                {
                    var cool = Instantiate(SkillBarManager.GetWorldCooldown(), SkillBarManager.WorldSkillParent);
                    cool.SetSkill(skill);
                    cool.SetUnit(unit);
                }
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (eventData.pointerCurrentRaycast.gameObject == null || eventData.pointerCurrentRaycast.gameObject.GetComponent<SkillManager>() == null)
            {
                SkillBarManager.HideToolTip();
                HoverSkill = null;
                var inds = FindObjectsOfType<WorldCooldownIndicator>();
                foreach (var i in inds)
                {
                    Destroy(i.gameObject);
                }
            }
        }

        private void OnDisable()
        {
            availCount.text = null;
        }

        private void OnEnable()
        {
            availCount.color = AvailableCountColor;
            availCount.transform.localScale = Vector3.one;
        }
    }
}