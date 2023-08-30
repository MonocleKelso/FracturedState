using UnityEngine;

public class CameraController : MonoBehaviour
{
    protected const float screenEdgeThreshold = 5f;

    protected float moveSpeed;
    protected float rotateSpeed;
    protected float mouseSensitivity;

    protected Transform tran;
    protected Vector3 centeredGround;
    protected bool rightClick;

    public bool CameraCanMove { get; protected set; }

    protected virtual void Start()
    {
        tran = transform.parent;
    }

    protected virtual void Update()
    {
        float r = (90f - transform.rotation.eulerAngles.x) * Mathf.Deg2Rad;
        float z = tran.position.y * Mathf.Tan(r);
        centeredGround = new Vector3(tran.position.x, 0, tran.position.z);
        centeredGround += tran.forward * z;
    }

    protected virtual void DoKeyboardMove()
    {
        float x = 0;
        float y = 0;

        if (Input.GetKey(KeyCode.W))
            y = 1;
        else if (Input.GetKey(KeyCode.S))
            y = -1;

        if (Input.GetKey(KeyCode.D))
            x = 1;
        else if (Input.GetKey(KeyCode.A))
            x = -1;

        Vector3 rightLeft = tran.right * moveSpeed * x * Time.deltaTime;
        Vector3 upDown = tran.forward * moveSpeed * y * Time.deltaTime;
        tran.position += (rightLeft + upDown);
    }

    // determines if the mouse cursor is close to a screen edge and moves
    // the camera accordingly.  This is a flat move in that it does not scale with distance
    // like a right-click move does
    protected virtual void DoScreenEdgeMove()
    {
        float x = 0;
        float y = 0;

        if (Input.mousePosition.x < screenEdgeThreshold)
            x = -1;
        else if (Input.mousePosition.x > Screen.width - screenEdgeThreshold)
            x = 1;

        if (Input.mousePosition.y < screenEdgeThreshold)
            y = -1;
        else if (Input.mousePosition.y > Screen.height - screenEdgeThreshold)
            y = 1;

        // scale local forward and right vectors according to cursor position
        // the basic idea being that 1 moves forward/right and -1 moves back/left
        // and if none of the conditions above are true then the result is 0 and nothing moves
        Vector3 rightLeft = tran.right * moveSpeed * x * Time.deltaTime;
        Vector3 upDown = tran.forward * moveSpeed * y * Time.deltaTime;
        tran.position += (rightLeft + upDown);
    }
}