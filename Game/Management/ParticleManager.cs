using UnityEngine;
using FracturedState.Game;

public class ParticleManager : MonoBehaviour
{
    private ParticleSystem system;

    public void OnEnable()
    {
        if (system == null)
            system = GetComponent<ParticleSystem>();

        StartCoroutine(Wait());
    }

    private System.Collections.IEnumerator Wait()
    {
        yield return null;
        while (system.IsAlive(true))
        {
            yield return null;
        }
        ParticlePool.Instance.ReturnSystem(system);
    }
}