using UnityEngine;

/// <summary>
/// A class responsible for making sure a helper mesh representing the currently selected object in the Place Object Tool "sticks" to the mouse cursor
/// </summary>
public class PlaceObjectToolMouseFollow : MonoBehaviour
{
	public Camera cam;
	public LayerMask RayMask;
    private float lastRotate;

	void Update()
	{
        if (Input.GetKey(KeyCode.LeftBracket))
        {
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                if (Time.time - lastRotate > 1)
                {
                    transform.Rotate(new Vector3(0, 90, 0));
                    lastRotate = Time.time;
                }
            }
            else
            {
                transform.Rotate(new Vector3(0, 1, 0));
            }
        }
        else if (Input.GetKey(KeyCode.RightBracket))
        {
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                if (Time.time - lastRotate > 1)
                {
                    transform.Rotate(new Vector3(0, -90, 0));
                    lastRotate = Time.time;
                }
            }
            else
            {
                transform.Rotate(new Vector3(0, -1, 0));
            }
        }

		Ray ray = cam.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, Mathf.Infinity, RayMask))
		{
			transform.position = hit.point;
		}
	}
}