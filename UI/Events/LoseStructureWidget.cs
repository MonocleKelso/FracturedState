using UnityEngine;
using UnityEngine.UI;

namespace FracturedState.UI.Events
{
    public class LoseStructureWidget : MonoBehaviour
    {
        [SerializeField] private Text structureName;

        public void SetStructureName(string sName)
        {
            structureName.text = sName;
        }
    }
}