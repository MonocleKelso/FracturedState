using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class IntroItemHandler : MonoBehaviour
{
    [SerializeField]
    float delay;
    [SerializeField]
    float fadeTime;
    [SerializeField]
    float holdTime;
    [SerializeField]
    Color color;
    [SerializeField]
    UnityEvent fadeInEvent;
    [SerializeField]
    UnityEvent doneEvent;

    System.Collections.IEnumerator Start()
    {
        var text = GetComponent<Text>();
        var image = GetComponent<Image>();
        if (text != null)
        {
            var c = text.color;
            c.a = 0;
            text.color = c;
        }
        if (image != null)
        {
            var c = image.color;
            c.a = 0;
            image.color = c;
        }
        yield return new WaitForSeconds(delay);
        float percent = 0;
        float time = 0;
        while (percent < 1)
        {
            time += Time.deltaTime;
            percent = Mathf.Lerp(0, 1, time / fadeTime);
            color.a = percent;
            if (text != null)
            {
                text.color = color;
            }
            if (image != null)
            {
                image.color = color;
            }
            yield return null;
        }
        if (fadeInEvent != null)
        {
            fadeInEvent.Invoke();
        }
        yield return new WaitForSeconds(holdTime);
        time = 0;
        while (percent > 0)
        {
            time += Time.deltaTime;
            percent = Mathf.Lerp(1, 0, time / fadeTime);
            color.a = percent;
            if (text != null)
            {
                text.color = color;
            }
            if (image != null)
            {
                image.color = color;
            }
            yield return null;
        }
        if (doneEvent != null)
        {
            doneEvent.Invoke();
        }
    }
}