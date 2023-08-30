using UnityEngine;

namespace FracturedState.UI
{
    public class RandomImageSelect : MonoBehaviour
    {
        [SerializeField] private UnityEngine.UI.Image[] images;

        private void Awake()
        {
            if (images != null && images.Length > 0)
            {
                int index = Random.Range(0, images.Length);
                for (int i = 0; i < images.Length; i++)
                {
                    images[i].gameObject.SetActive(i == index);
                }
            }
        }
    }
}