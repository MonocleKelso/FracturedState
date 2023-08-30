using FracturedState.Game;
using UnityEngine;

public class TerrainToolMouseFollow : MonoBehaviour
{
    public Camera cam;

    private float width, height, wRayLen, hRayLen;

    void Start()
    {
        BoxCollider b = GetComponent<BoxCollider>();
        width = b.bounds.size.x;
        height = b.bounds.size.z;
        wRayLen = (width / 2f) + 1f;
        hRayLen = (height / 2f) + 1f;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            transform.Rotate(new Vector3(0, 90, 0), Space.World);
        }
        else if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            transform.Rotate(new Vector3(0, -90, 0), Space.World);
        }

        Vector3 sPos = Input.mousePosition;
        sPos.z = Vector3.Distance(cam.transform.position, new Vector3(cam.transform.position.x, 0, cam.transform.position.z));
        Vector3 worldPos = cam.ScreenToWorldPoint(sPos);
        worldPos.y = 0;

        if (!Input.GetKey(KeyCode.LeftControl))
        {
            worldPos.x = Mathf.Round(worldPos.x * 2) / 2f;
            worldPos.z = Mathf.Round(worldPos.z * 2) / 2f;
        }

        transform.position = worldPos;

        Vector3 worldForward = transform.TransformDirection(transform.forward);
        Vector3 worldBackward = -worldForward;

        RaycastHit[] forwardHits = Physics.RaycastAll(transform.position, worldForward, wRayLen, GameConstants.TerrainMask);
        RaycastHit[] backwardHits = Physics.RaycastAll(transform.position, worldBackward, wRayLen, GameConstants.TerrainMask);
        if (forwardHits != null)
        {
            ClampToHit(forwardHits, transform.forward, width);
        }
        if (backwardHits != null)
        {
            ClampToHit(backwardHits, -transform.forward, width);
        }

        int y = (int)transform.rotation.eulerAngles.y;

        Vector3 worldRight = y == 0 || y == 180 ? transform.TransformDirection(transform.right) : transform.TransformDirection(transform.up);
        Vector3 clamp = y == 0 || y == 180 ? transform.right : transform.up;
        Vector3 worldLeft = -worldRight;
        worldLeft.y = -0.01f; // fudge because raycasts to the left for rotated terrains seem to miss because they're too high
        
        RaycastHit[] rightHits = Physics.RaycastAll(transform.position, worldRight, hRayLen, GameConstants.TerrainMask);
        RaycastHit[] leftHits = Physics.RaycastAll(transform.position, worldLeft, hRayLen, GameConstants.TerrainMask);
        if (rightHits != null)
        {
            ClampToHit(rightHits, clamp, height);
        }
        if (leftHits != null)
        {
            ClampToHit(leftHits, -clamp, height);
        }

        var p = transform.position;
        p.y = 0;
        transform.position = p;
    }

    void ClampToHit(RaycastHit[] hits, Vector3 direction, float dimension)
    {
        float dist = float.MaxValue;
        Vector3 point = Vector3.zero;
        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit h = hits[i];
            if (h.transform != transform)
            {
                float d = (transform.position - h.point).sqrMagnitude;
                if (d < dist)
                {
                    dist = d;
                    point = h.point;
                }
            }
        }
        if (dist < float.MaxValue)
        {
            transform.position = transform.TransformPoint(transform.InverseTransformPoint(point) - (direction * (dimension / 2f)));
        }
    }
}