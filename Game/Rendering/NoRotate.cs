using UnityEngine;

[ExecuteInEditMode]
public class NoRotate : MonoBehaviour
{
    private Quaternion r;

    void Start()
    {
        r = transform.localRotation;
    }

    void Update()
    {
        transform.rotation = r;
    }
}