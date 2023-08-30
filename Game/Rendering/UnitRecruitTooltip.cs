using System.Collections.Generic;
using System.Linq;
using FracturedState.Game;
using FracturedState.Game.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Game.Rendering
{
    public class UnitRecruitTooltip : MonoBehaviour
    {
        [SerializeField] private Image unitIcon;
        [SerializeField] private Text unitName;
        [SerializeField] private Text unitRole;
        [SerializeField] private Text unitDescription;
        [SerializeField] private Text requiredStructures;
        [SerializeField] private Text skillsTitle;
        [SerializeField] private SkillRecruitTooltip skillTemplate;
        
        private readonly Dictionary<string, Sprite> UnitIconCache = new Dictionary<string, Sprite>();
        private readonly Dictionary<string, SkillRecruitTooltip> SkillCache = new Dictionary<string, SkillRecruitTooltip>();
        
        private string currentUnitName;
        private UnitObject unit;
        
        public void SetUnit(string uName)
        {
            if (string.IsNullOrEmpty(uName))
            {
                Hide();
                return;
            }
            
            if (currentUnitName == uName && gameObject.activeInHierarchy) return;

            if (currentUnitName == uName)
            {
                gameObject.SetActive(true);
                return;
            }

            var skills = GetComponentsInChildren<SkillRecruitTooltip>();
            if (skills != null && skills.Length > 0)
            {
                foreach (var skill in skills)
                {
                    skill.gameObject.SetActive(false);
                }
            }
            
            currentUnitName = uName;
            unit = XmlCacheManager.Units[currentUnitName];
            unitName.text = unit.Name;
            unitRole.text = unit.Role;
            unitDescription.text = unit.ShortDescription;
            requiredStructures.text = "None";
            if (unit.PrerequisiteStructures != null && unit.PrerequisiteStructures.Length > 0)
            {
                requiredStructures.text = unit.PrerequisiteStructures.Aggregate((a, b) => $"{a}{System.Environment.NewLine}{b}");
            }
            
            Sprite icon;
            if (!UnitIconCache.TryGetValue(currentUnitName, out icon))
            {
                icon = Sprite.Create(unit.Icon, new Rect(0, 0, 75, 75), new Vector2(0.5f, 0.5f), 100);
                UnitIconCache[currentUnitName] = icon;
            }

            unitIcon.sprite = icon;

            var hasSkills = unit.Abilities != null && unit.Abilities.Length > 0;
            skillsTitle.gameObject.SetActive(hasSkills);
            if (hasSkills)
            {
                foreach (var skillName in unit.Abilities)
                {
                    SkillRecruitTooltip tip;
                    if (SkillCache.TryGetValue(skillName, out tip))
                    {
                        tip.gameObject.SetActive(true);
                        continue;
                    }
                    
                    var skill = XmlCacheManager.Abilities[skillName];
                    if (skill.Type == AbilityType.PassivePerSquad || skill.Type == AbilityType.PassivePerUnit)
                        continue;
                    
                    tip = Instantiate(skillTemplate, transform);
                    tip.SetSkill(skill);
                    SkillCache[skillName] = tip;
                    tip.gameObject.SetActive(true);
                }
            }
            
            gameObject.SetActive(true);
        }

        private void Awake()
        {
            gameObject.SetActive(false);
        }
        
        private void Hide()
        {
            if (!gameObject.activeInHierarchy) return;
            
            gameObject.SetActive(false);
        }
    }
}