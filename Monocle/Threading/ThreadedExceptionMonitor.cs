using UnityEngine;

public class ThreadedExceptionMonitor : MonoBehaviour
{
    public static ThreadedExceptionMonitor Instance { get; private set; }

    private System.Exception exception;

    public void ThrowException(System.Exception e)
    {
        exception = e;
    }

    private void Awake()
    {
        if (Instance != null)
            throw new FracturedState.Game.FracturedStateException("Only a single instance of ThreadedExceptionMonitor is allowed");

        Instance = this;
    }

    private void Update()
    {
        if (exception != null)
        {
            throw exception;
        }
    }
}