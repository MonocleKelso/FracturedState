using FracturedState.Game.Data;
using UnityEngine;
using UnityEngine.UI;

namespace FracturedState.Game
{
    public class SkillTooltip : MonoBehaviour
    {
        [SerializeField] private Text _name;
        [SerializeField] private Text _timer;
        [SerializeField] private Text _description;

        public void SetSkill(Ability skill)
        {
            _name.text = skill.DisplayName;
            _timer.text = skill.CooldownTime + "s";
            _description.text = XmlCacheManager.Abilities[skill.Name].Description;
        }
    }
}