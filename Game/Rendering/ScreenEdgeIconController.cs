using FracturedState.Game;
using FracturedState.UI;
using System.Collections;
using UnityEngine;

public class ScreenEdgeIconController : MonoBehaviour
{
    [SerializeField] private Animator anim;

    private bool doRemove;
    private float removeTime;
    private Coroutine remove;
    private Squad squad;

    private void OnEnable()
    {
        if (doRemove)
        {
            StartRemoval();
            removeTime = Time.time;
        }
    }

    private void OnDisable()
    {
        if (remove != null)
        {
            StopCoroutine(remove);
            remove = null;
        }
        if (doRemove)
        {
            ScreenEdgeNotificationManager.Instance.RemoveIcon(squad);
        }
    }

    public void Init(Squad squad)
    {
        if (this.squad == null)
        {
            this.squad = squad;
        }
        if (remove != null)
        {
            StopCoroutine(remove);
            remove = null;
        }
    }

    public void StartRemoval()
    {
        if (gameObject.activeSelf)
        {
            if (Time.time - removeTime > 10 && doRemove)
            {
                ScreenEdgeNotificationManager.Instance.RemoveIcon(squad);
            }
            else
            {
                if (remove != null)
                {
                    StopCoroutine(remove);
                }
                remove = StartCoroutine(Remove());
            }
        }
        else
        {
            doRemove = true;
        }
    }

    private IEnumerator Remove()
    {
        anim.enabled = true;
        var t = 10f;
        while (t > 0 && squad != null && squad.Members.Count > 0)
        {
            t -= Time.deltaTime;
            yield return null;
        }

        if (squad == null)
        {
            Destroy(gameObject);
        }
        else
        {
            ScreenEdgeNotificationManager.Instance.RemoveIcon(squad);
        }
    }
}