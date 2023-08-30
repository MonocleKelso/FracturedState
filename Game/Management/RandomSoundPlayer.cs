using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class RandomSoundPlayer : MonoBehaviour
{
    [SerializeField]
    private AudioClip[] clips;

    private void OnEnable()
    {
        if (clips.Length > 0)
        {
            GetComponent<AudioSource>().PlayOneShot(clips[Random.Range(0, clips.Length)]);
        }
    }
}