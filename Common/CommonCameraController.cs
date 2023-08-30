using UnityEngine;
using FracturedState.Game;
using FracturedState.Game.Data;
using FracturedState.Game.Management;
using FracturedState.Game.Network;

public class CommonCameraController : CameraController
{
	private Vector3 startPos;
    private bool battleMoving;
    private Vector3 battleLocation;

    private PlayerProfile playerProfile;

    public bool MonitorHeight { get; set; }

    [SerializeField()]
    private CompassMenuManager compassManager;
    public CompassMenuManager CompassManager => compassManager;

    [SerializeField]
    private CommandManager commandManager;

    [SerializeField] private Transform audioListener;

	protected override void Start()
	{
        base.Start();
        moveSpeed = ConfigSettings.Instance.Values.CameraMoveSpeed;
        rotateSpeed = ConfigSettings.Instance.Values.CameraRotateSpeed;
        mouseSensitivity = ConfigSettings.Instance.Values.CameraSensitivity;
        MonitorHeight = true;
	}

	private void OnEnable()
    {
        transform.parent.rotation = Quaternion.identity;
        transform.rotation = Quaternion.Euler(new Vector3(ConfigSettings.Instance.Values.CameraDefaultAngle, 0, 0));
        MonitorHeight = true;
        compassManager.gameObject.SetActive(true);
        var team = FracNet.Instance.LocalTeam;
        GetComponent<ShroudCamera>().enabled = !team.IsSpectator;
        battleMoving = false;
        playerProfile = ProfileManager.GetActiveProfile();
    }
	
	protected override void Update()
	{
        if (commandManager.EscapeMenuOpen || commandManager.InTacticalTransition || commandManager.FaceMoving)
            return;

        base.Update();

        audioListener.position = centeredGround;
        audioListener.rotation = tran.rotation;

        // if user is holding right mouse button then
        // either move camera or check if mouse has moved enough
        // to start moving the camera
        if (rightClick)
		{
			if (CameraCanMove)
			{
				MouseMoveCamera();
                battleMoving = false;
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
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
            DoScreenEdgeMove();
#endif
        }

		// if user presses right mouse on this frame
		if (Input.GetMouseButtonDown(1) && !Input.GetKey(KeyCode.LeftShift))
		{
			rightClick = true;
			startPos = Input.mousePosition;
		}
		// if user releases right mouse on this frame
		else if (Input.GetMouseButtonUp(1))
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
            battleMoving = false;
		}

		if (!IngameChatManager.Instance.ChatInputOpen)
		{
			if (Input.GetKey(playerProfile.KeyBindConfig.RotateCamClockWise.Key))
			{
				tran.RotateAround(centeredGround, tran.up, 0.5f * rotateSpeed);
				compassManager.RotateCompass(0.5f * rotateSpeed);
				battleMoving = false;
			}
			else if (Input.GetKey(playerProfile.KeyBindConfig.RotateCamCounterClockWise.Key))
			{
				tran.RotateAround(centeredGround, tran.up, -0.5f * rotateSpeed);
				compassManager.RotateCompass(-0.5f * rotateSpeed);
				battleMoving = false;
			}
		}

		if (battleMoving)
        {
            tran.position = Vector3.MoveTowards(tran.position, battleLocation, 250 * Time.deltaTime);
        }

        if (MonitorHeight)
        {
            FracturedState.UI.ScreenEdgeNotificationManager.Instance.UpdateBattleIcons();
            AdjustHeight();
        }
	}
	
	// moves the camera using the mouse
	// constructs a Vector2 representing the delta of movement between the mouse position when the user right clicked and
	// the current mouse position and converting it into a "percentage" of the screen.  Two Vector3's are then constructed using
	// the local forward and local right multiplied by the Vector2 components as well as overall speed and time.  The end result is that
	// the camera moves faster the farther away from the click spot you move the cursor.
	private void MouseMoveCamera()
	{
        var pos = tran.position;
        var forward = tran.forward;
		var dir = new Vector2((mouseSensitivity * (Input.mousePosition.y - startPos.y) / Screen.height), (mouseSensitivity * (Input.mousePosition.x - startPos.x) / Screen.width));
		pos += (forward * ((dir.x * moveSpeed) * Time.deltaTime)) + (tran.right * ((dir.y * moveSpeed) * Time.deltaTime));
        var downAngle = (90f - transform.rotation.eulerAngles.x) * Mathf.Deg2Rad;
        var diff = pos.y * Mathf.Tan(downAngle);
        var frontDiff = Vector3.Dot(Vector3.forward, forward) * diff;
        var rightDiff = Vector3.Dot(Vector3.right, forward) * diff;
        pos.x = Mathf.Clamp(pos.x, SkirmishVictoryManager.CurrentMap.XLowerBound - rightDiff, SkirmishVictoryManager.CurrentMap.XUpperBound - rightDiff);
        pos.z = Mathf.Clamp(pos.z, SkirmishVictoryManager.CurrentMap.ZLowerBound - frontDiff, SkirmishVictoryManager.CurrentMap.ZUpperBound - frontDiff);
        tran.position = pos;
	}
	
	// rotates the camera around the point on the terrain that is centered in the viewport
	private void MouseRotateCamera()
	{
		var deg = Input.GetAxis("Mouse X");
        tran.RotateAround(centeredGround, tran.up, deg * rotateSpeed);
        compassManager.RotateCompass(deg * rotateSpeed);
    }

    protected override void DoKeyboardMove()
    {
	    if (IngameChatManager.Instance.ChatInputOpen) return;
	    
        float x = 0;
        float y = 0;

        if (Input.GetKey(playerProfile.KeyBindConfig.MoveCamUp.Key))
            y = 1;
        else if (Input.GetKey(playerProfile.KeyBindConfig.MoveCamDown.Key))
            y = -1;

        if (Input.GetKey(playerProfile.KeyBindConfig.MoveCamRight.Key))
            x = 1;
        else if (Input.GetKey(playerProfile.KeyBindConfig.MoveCamLeft.Key))
            x = -1;

        var multi = MonitorHeight ? 1f : 2f;
        var rightLeft = tran.right * moveSpeed * multi * x * Time.deltaTime;
        var upDown = tran.forward * moveSpeed * multi * y * Time.deltaTime;
        tran.position += (rightLeft + upDown);
        if (x != 0 && y != 0)
        {
            battleMoving = false;
        }
    }

    private void AdjustHeight()
    {
        var ray = new Ray(tran.position + Vector3.forward * 10, -Vector3.up);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, GameConstants.ExteriorObjectMask))
        {
            var heightDiff = tran.position.y - hit.point.y;
            if (heightDiff < ConfigSettings.Instance.Values.CameraBufferHeight)
            {
                var pos = tran.position + Vector3.up * ConfigSettings.Instance.Values.CameraHeightUnitAdjustment * Time.deltaTime;
                tran.position = pos;
                if (transform.rotation.eulerAngles.x < ConfigSettings.Instance.Values.CameraBufferAngle)
                {
                    var rot = transform.rotation.eulerAngles;
                    rot.x += ConfigSettings.Instance.Values.CameraAngleUnitAdjustment * Time.deltaTime;
                    transform.rotation = Quaternion.Euler(rot);
                }
            }
        }
        else
        {
            if (tran.position.y > ConfigSettings.Instance.Values.CameraDefaultHeight)
            {
                var pos = tran.position - Vector3.up * ConfigSettings.Instance.Values.CameraHeightUnitAdjustment * Time.deltaTime;
                tran.position = pos;
            }
            if (transform.rotation.eulerAngles.x > ConfigSettings.Instance.Values.CameraDefaultAngle)
            {
                var rot = transform.rotation.eulerAngles;
                rot.x -= ConfigSettings.Instance.Values.CameraAngleUnitAdjustment * Time.deltaTime;
                transform.rotation = Quaternion.Euler(rot);
            }
        }
    }

    public void BattleMove(Vector3 location)
    {
        battleMoving = true;
        var camDownAngle = transform.rotation.eulerAngles.x;
        var rad = (90f - camDownAngle) * Mathf.Deg2Rad;
        var z = tran.position.y * Mathf.Tan(rad);
        var p = new Vector3(location.x, tran.position.y, location.z);
        p -= tran.forward * z;
        battleLocation = p;
    }
}