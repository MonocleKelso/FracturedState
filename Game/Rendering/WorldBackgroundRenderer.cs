using UnityEngine;

namespace FracturedState.Game
{
    public class WorldBackgroundRenderer : MonoBehaviour
    {
        [SerializeField] private Camera _renderCamera;

        private void OnEnable()
        {
            if (_renderCamera != null) _renderCamera.gameObject.SetActive(true);
        }
        
        private void OnDisable()
        {
            if (_renderCamera != null) _renderCamera.gameObject.SetActive(false);
        }
    }
}