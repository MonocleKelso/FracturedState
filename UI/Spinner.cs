using UnityEngine;

namespace FracturedState.UI
{
    public class Spinner : MonoBehaviour
    {
        [SerializeField] private float rotation;

        private void Update()
        {
            transform.Rotate(transform.forward, -rotation * Time.deltaTime);
        }
    }
}