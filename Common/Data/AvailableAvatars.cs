using UnityEngine;

namespace FracturedState.Game.Data
{
    public class AvailableAvatars : MonoBehaviour
    {
        public static AvailableAvatars Instance { get; private set; }

        [SerializeField]
        Sprite[] avatarList;

        public Sprite[] Avatars { get { return avatarList; } }

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
            }
            Instance = this;
        }
    }
}