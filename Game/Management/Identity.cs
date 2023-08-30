
public class Identity : UnityEngine.MonoBehaviour
{
    private bool uidSet = false;
    public int UID { get; private set; }

    public void SetUID(int uid)
    {
        if (!uidSet)
        {
            UID = uid;
            uidSet = true;
        }
    }
}