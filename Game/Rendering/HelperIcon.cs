using UnityEngine;

public class HelperIcon : MonoBehaviour
{
    static Transform cam;

    void Start()
    {
        if (cam == null)
        {
            cam = Camera.main.transform;
        }
    }

    void Update()
    {
        transform.LookAt(cam);
        Vector3 rot = transform.rotation.eulerAngles;
        rot.x = 0;
        transform.rotation = Quaternion.Euler(rot);
    }
}