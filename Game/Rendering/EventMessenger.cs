using UnityEngine;
using System.Collections.Generic;

public class EventMessenger : MonoBehaviour
{
    private const float left = 5;
    private const float lifetime = 2;

    [SerializeField()]
    private GUISkin skin;

    private List<string> messages = new List<string>();
    private List<float> times = new List<float>();

    public void AddMessage(string message)
    {
        if (messages.Count == 5)
        {
            messages.RemoveAt(0);
        }
        messages.Add(message);
        times.Add(Time.time);
    }

    public void ClearAll()
    {
        messages.Clear();
    }

    void Update()
    {
        for (var i = messages.Count - 1; i >= 0; i-- )
        {
            if (Time.time - times[i] > lifetime)
            {
                messages.RemoveAt(i);
                times.RemoveAt(i);
            }
        }
    }

    void OnGUI()
    {
        GUI.skin = skin;

        for (var i = messages.Count - 1; i >= 0; i--)
        {
            float y = Screen.height - (25 * (messages.Count - i));
            GUI.Label(new Rect(left, y, 400, 25), messages[i]);
        }
    }
}