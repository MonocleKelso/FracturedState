using UnityEngine;

namespace FracturedState.Game
{
    public class IndicatorFollow : MonoBehaviour
    {
        private void Update()
        {
            var hit = RaycastUtil.RaycastTerrainAtMouse();
            if (hit.collider != null)
            {
                transform.position = hit.point;
            }
        }
    }
}