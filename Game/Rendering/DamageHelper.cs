using UnityEngine;
using UnityEngine.UI;

public class DamageHelper : MonoBehaviour
{
    public void Init(int damage)
    {
        Text[] texts = GetComponentsInChildren<Text>();
        for (int i = 0; i < texts.Length; i++)
        {
            texts[i].text = damage.ToString();
        }
    }
}