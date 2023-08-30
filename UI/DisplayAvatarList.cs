using System.Net.Mime;
using FracturedState.Game.Data;
using UnityEngine;
using UnityEngine.UI;

namespace FracturedState.UI
{
    public class DisplayAvatarList : MonoBehaviour
    {
        [SerializeField] private Image avatarContainer;

        private void Awake()
        {
            var avatars = AvailableAvatars.Instance.Avatars;
            for (var i = 0; i < avatars.Length; i++)
            {
                var a = Instantiate(avatarContainer, transform);
                a.sprite = avatars[i];
                var setter = a.GetComponent<SetPlayerAvatar>();
                if (setter != null)
                {
                    setter.SetIndex(i);
                }
            }
        }
    }
}