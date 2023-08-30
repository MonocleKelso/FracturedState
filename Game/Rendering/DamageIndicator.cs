using FracturedState.Game;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DamageIndicator : MonoBehaviour
{
    [SerializeField]
    float speed;
    [SerializeField]
    float fadeStartTime;
    [SerializeField]
    float fadeDuration;

    Text[] texts;

    static Transform cam;

    protected void OnEnable()
    {
        if (cam == null)
        {
            cam = Camera.main.transform;
        }
        if (texts == null)
        {
            texts = GetComponentsInChildren<Text>();
        }
        for (int i = 0; i < texts.Length; i++)
        {
            Color c = texts[i].color;
            c.a = 1;
            texts[i].color = c;
        }
        StartCoroutine(Do());
    }

    protected IEnumerator Do()
    {
        yield return new WaitForSeconds(fadeStartTime);
        float a = 1;
        float fadeTime = 0;
        while (a > 0)
        {
            a = Mathf.Lerp(1, 0, fadeTime / fadeDuration);
            for (int i = 0; i < texts.Length; i++)
            {
                Color c = texts[i].color;
                c.a = a;
                texts[i].color = c;
            }
            fadeTime += Time.deltaTime;
            yield return null;
        }
        Return();
    }

    protected virtual void Return()
    {
        ObjectPool.Instance.ReturnDamageHelper(gameObject);
    }

    protected void Update()
    {
        transform.position += Vector3.up * speed * Time.deltaTime;
        transform.LookAt(cam);
    }
}