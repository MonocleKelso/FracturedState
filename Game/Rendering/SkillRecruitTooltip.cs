using FracturedState.Game.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Game.Rendering
{
    public class SkillRecruitTooltip : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private Text skillName;
        [SerializeField] private Text skillDescription;
        [SerializeField] private Text skillConstraint;

        public void SetSkill(Ability skill)
        {
            icon.sprite = Ability.GetIcon(skill.IconAtlas, skill.IconPath);
            skillName.text = skill.DisplayName;
            skillDescription.text = skill.Description;
            switch (skill.ConstrainType)
            {
                case Constraint.None:
                    skillConstraint.text = "Usable anywhere";
                    break;
                case Constraint.Exterior:
                    skillConstraint.text = "Usable outside only";
                    break;
                case Constraint.Interior:
                    skillConstraint.text = "Usable inside only";
                    break;
                default:
                    skillConstraint.text = "Usable anywhere";
                    break;
            }
            
            
        }
    }
}