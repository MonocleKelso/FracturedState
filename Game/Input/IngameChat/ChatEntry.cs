using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace FracturedState.Game
{
    public class ChatEntry : MonoBehaviour
    {
        [SerializeField] private Text text;
        
        private IEnumerator Start()
        {
            yield return new WaitForSeconds(10);
            while (text.color.a > 0)
            {
                var c = text.color;
                c.a -= Time.deltaTime;
                text.color = c;
                yield return null;
            }
            IngameChatManager.Instance.RemoveEntry(text);
            Destroy(gameObject);
        }
    }
}