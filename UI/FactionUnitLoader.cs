using FracturedState.Game;
using UnityEngine;

namespace FracturedState.UI
{
    public class FactionUnitLoader : MonoBehaviour
    {
        [SerializeField] private string faction;

        [SerializeField] private UnitTemplate unitTemplate;

        private void Start()
        {
            var fac = XmlCacheManager.Factions[faction];
            foreach (var unit in fac.TrainableUnits)
            {
                var ut = Instantiate(unitTemplate, transform);
                ut.SetUnit(XmlCacheManager.Units[unit]);
            }
        }
    }
}