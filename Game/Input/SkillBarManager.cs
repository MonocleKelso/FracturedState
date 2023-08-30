using System.Collections.Generic;
using System.Linq;
using FracturedState.Game.Data;
using FracturedState.Game.Network;
using UnityEngine;
using UnityEngine.UI;

namespace FracturedState.Game
{
    public class SkillBarManager : MonoBehaviour
    {
        private static SkillBarManager instance;

        [SerializeField] private RectTransform skillParent;
        [SerializeField] private SkillManager skillPrefab;
        [SerializeField] private GameObject worldIndicatorManager;
        [SerializeField] private Text noCastText;
        [SerializeField] private SkillTooltip toolTip;
        [SerializeField] private WorldCooldownIndicator worldCooldown;
        [SerializeField] private Button combineButton;

        private Coroutine noCastFade;

        private readonly List<SkillManager> slots = new List<SkillManager>();

        private readonly List<Ability> abilities = new List<Ability>();

        public static Transform WorldSkillParent => instance.transform;

        public static void Activate()
        {
            if (instance == null) return;
            instance.gameObject.SetActive(true);
            instance.skillParent.gameObject.SetActive(false);

        }

        public static GameObject GetWorldIndicator()
        {
            return instance != null ? instance.worldIndicatorManager : null;
        }

        public static WorldCooldownIndicator GetWorldCooldown()
        {
            return instance != null ? instance.worldCooldown : null;
        }

        public static void ShowNoCast()
        {
            if (instance == null) return;
            if (instance.noCastFade != null) instance.StopCoroutine(instance.noCastFade);
            instance.noCastFade = instance.StartCoroutine(instance.CastFade());
        }

        public static void ShowToolTip(RectTransform skillTransform, Ability ability)
        {
            if (instance == null) return;
            instance.toolTip.SetSkill(ability);
//            var pos = instance.toolTip.transform.position;
//            pos.x = skillTransform.position.x;
//            instance.toolTip.transform.position = pos;
            instance.toolTip.gameObject.SetActive(true);
        }

        public static void HideToolTip()
        {
            if (instance == null) return;
            instance.toolTip.gameObject.SetActive(false);
        }

        // event hook for button
        public void Consolidate()
        {
            InterfaceSoundPlayer.PlayButtonClick();
            var units = SelectionManager.Instance.SelectedUnits.Select(u => u.NetMsg.NetworkId.netId).ToArray();
            FracNet.Instance.NetworkActions.CmdCombineSquads(units);
        }

        private System.Collections.IEnumerator CastFade()
        {
            noCastText.gameObject.SetActive(true);
            var color = noCastText.color;
            color.a = 1;
            noCastText.color = color;
            yield return new WaitForSeconds(2);
            while (color.a > 0)
            {
                color.a -= Time.deltaTime;
                noCastText.color = color;
                yield return null;
            }
            noCastText.gameObject.SetActive(false);
        }
        
        private SkillManager GetSlot()
        {
            if (slots.Count > 0)
            {
                var slot = slots[slots.Count - 1];
                slots.RemoveAt(slots.Count - 1);
                slot.gameObject.SetActive(true);
                return slot;
            }
            return Instantiate(skillPrefab, skillParent);
        }

        private void ReturnSlot(SkillManager slot)
        {
            slot.gameObject.SetActive(false);
            if (!slots.Contains(slot))
                slots.Add(slot);
        }
        
        private void Awake()
        {
            if (instance != null)
                Destroy(instance.gameObject);
            instance = this;
            instance.gameObject.SetActive(false);
        }

        private void ProcessSelection()
        {
            Reset();
            skillParent.gameObject.SetActive(SelectionManager.Instance.SelectedUnits.Count > 0);
            SkillManager.VerifyActiveSkill();
            if (SelectionManager.Instance.SelectedUnits.Count == 0) return;
            var canCombine = SelectionManager.Instance.SquadCount > 1;
            if (canCombine)
            {
                var pop = 0;
                foreach (var unit in SelectionManager.Instance.SelectedUnits)
                {
                    pop += unit.Data.PopulationCost;
                }

                canCombine = pop <= 10;
            }

            combineButton.interactable = canCombine;
            GatherSkills();
            for (var i = 0; i < abilities.Count; i++)
            {
                var slot = GetSlot();
                slot.transform.SetSiblingIndex(i);
                slot.Init(abilities[i], i == 0, i == abilities.Count - 1);
            }
        }

        private void Reset()
        {
            for (var i = 0; i < skillParent.transform.childCount; i++)
            {
                var man = skillParent.transform.GetChild(i).GetComponent<SkillManager>();
                if (man != null)
                {
                    ReturnSlot(man);
                }
            }
            abilities.Clear();
            if (noCastFade != null) StopCoroutine(noCastFade);
            noCastText.gameObject.SetActive(false);
            combineButton.interactable = false;
        }
        
        private void GatherSkills()
        {
            var uSkills = new HashSet<string>();
            for (var i = 0; i < SelectionManager.Instance.SelectedUnits.Count; i++)
            {
                var unit = SelectionManager.Instance.SelectedUnits[i];
                var skills = unit.GetSquadAbilities();
                if (skills == null) continue;
                for (var u = 0; u < skills.Length; u++)
                {
                    if (uSkills.Add(skills[u].Name))
                    {
                        abilities.Add(skills[u]);
                    }
                }
                var unitSkills = unit.GetAbilities();
                for (var u = 0; u < unitSkills.Length; u++)
                {
                    if (uSkills.Add(unitSkills[u].Name))
                    {
                        abilities.Add(unitSkills[u]);
                    }
                }
            }
            abilities.Sort();
        }
        
        private void OnEnable()
        {
            if (SelectionManager.Instance != null)
                SelectionManager.Instance.OnSelectionChanged.AddListener(ProcessSelection);
        }

        private void OnDisable()
        {
            if (SelectionManager.Instance != null)
                SelectionManager.Instance.OnSelectionChanged.RemoveListener(ProcessSelection);
        }

        private void OnDestroy()
        {
            if (SelectionManager.Instance != null)
                SelectionManager.Instance.OnSelectionChanged.RemoveListener(ProcessSelection);
        }
    }
}