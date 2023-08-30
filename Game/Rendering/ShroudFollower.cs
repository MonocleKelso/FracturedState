using UnityEngine;

public class ShroudFollower : MonoBehaviour
{
    private Transform thisTransform;
    public Transform Target { get; private set; }

    void Awake()
    {
        thisTransform = transform;
    }

    public void SetTarget(Transform target)
    {
        Target = target;
    }

    void Update()
    {
        if (Target != null)
        {
            thisTransform.position = Target.position;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}