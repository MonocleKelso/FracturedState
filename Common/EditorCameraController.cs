using UnityEngine;
using FracturedState.Game;

public class EditorCameraController : CameraController
{
    private Vector3 startPos;

    public CameraViewState ViewState;

    [SerializeField()]
    private MapEditorToolManager manager;

    protected override void Start()
    {
        base.Start();
        moveSpeed = 50f;
        rotateSpeed = 10f;
        mouseSensitivity = 4f;
        ViewState = CameraViewState.Exterior;
    }

    private void OnEnable()
    {
        GetComponent<Camera>().cullingMask = GameConstants.EditorExteriorCameraMask;
        ViewState = CameraViewState.Exterior;
    }

    protected override void Update()
    {
        base.Update();

        // if user is holding right mouse button then
        // either move camera or check if mouse has moved enough
        // to start moving the camera
        if (rightClick)
        {
            if (CameraCanMove)
            {
                MouseMoveCamera();
            }
            else
            {
                CameraCanMove = Vector3.Distance(Input.mousePosition, startPos) > 1;
            }
        }
        // if rightClick is false but the cursor is close to a screen edge
        // then move camera
        else
        {
            DoKeyboardMove();
#if !UNITY_EDITOR
            DoScreenEdgeMove();
#endif
        }

        // if user presses right mouse on this frame
        if (Input.GetMouseButtonDown(1) && (manager == null || !manager.CursorInMenu()))
        {
            rightClick = true;
            startPos = Input.mousePosition;
        }
        // if user releases right mouse on this frame
        else if (Input.GetMouseButtonUp(1) && (manager == null || !manager.CursorInMenu()))
        {
            rightClick = false;
        }
        // if rightClick is false but camera is still in move mode then
        // turn off move.  This results in a 1 frame delay which is necessary for managers
        // to properly poll camera movement for right click actions
        else if (!rightClick && CameraCanMove)
        {
            CameraCanMove = false;
        }

        // middle mouse button is rotate
        if (Input.GetMouseButton(2))
        {
            MouseRotateCamera();
        }

        if (Input.GetKey(KeyCode.Q))
        {
            tran.RotateAround(centeredGround, tran.up, 0.5f * rotateSpeed);
        }
        else if (Input.GetKey(KeyCode.E))
        {
            tran.RotateAround(centeredGround, tran.up, -0.5f * rotateSpeed);
        }

        // tab key switches interior/exterior view
        // suppress modifier keys to support OS keystrokes
        if (Input.GetKeyUp(KeyCode.Tab) && !Input.GetKey(KeyCode.LeftCommand) && !Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.LeftWindows))
        {
            SwapCameraView();
        }

        if (Input.GetAxis("Mouse ScrollWheel") != 0 && (manager == null || !manager.CursorInMenu()))
        {
            ZoomCamera(Input.GetAxis("Mouse ScrollWheel") * 3f);
        }
        else
        {
            // plus and minus keys zoom camera in and out
            if (Input.GetKeyDown(KeyCode.Equals))
            {
                ZoomCamera(1);
            }
            else if (Input.GetKeyDown(KeyCode.Minus))
            {
                ZoomCamera(-1);
            }
        }
        
    }

    private void ZoomCamera(float amount)
    {
        transform.position += transform.forward * amount;
    }

    // moves the camera using the mouse
    // constructs a Vector2 representing the delta of movement between the mouse position when the user right clicked and
    // the current mouse position and converting it into a "percentage" of the screen.  Two Vector3's are then constructed using
    // the local forward and local right multiplied by the Vector2 components as well as overall speed and time.  The end result is that
    // the camera moves faster the farther away from the click spot you move the cursor.
    private void MouseMoveCamera()
    {
        Vector2 dir = new Vector2((mouseSensitivity * (Input.mousePosition.y - startPos.y) / Screen.height), (mouseSensitivity * (Input.mousePosition.x - startPos.x) / Screen.width));
        tran.position += (tran.forward * ((dir.x * moveSpeed) * Time.deltaTime)) + (tran.right * ((dir.y * moveSpeed) * Time.deltaTime));
    }

    // rotates the camera around the point on the terrain that is centered in the viewport
    private void MouseRotateCamera()
    {
        float deg = Input.GetAxis("Mouse X");
        tran.RotateAround(centeredGround, tran.up, deg * rotateSpeed);
    }

    private void SwapCameraView()
    {
        if (ViewState == CameraViewState.Exterior)
        {
            GetComponent<Camera>().cullingMask = GameConstants.EditorInteriorCameraMask;
            ViewState = CameraViewState.Interior;
        }
        else
        {
            GetComponent<Camera>().cullingMask = GameConstants.EditorExteriorCameraMask;
            ViewState = CameraViewState.Exterior;
        }
    }
}