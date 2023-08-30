using UnityEngine;
using FracturedState.Game;
using FracturedState.Game.Management;

public class SightTimeStep : MonoBehaviour
{
    public System.Collections.IEnumerator Start()
    {
        while (true)
        {
            yield return new WaitForSeconds(GameConstants.SightEvalTime);
            VisibilityChecker.Instance.EvaluateVisibility();
        }
    }
}