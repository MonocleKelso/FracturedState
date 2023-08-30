using FracturedState.Game;
using UnityEngine;

public class Lifetime : MonoBehaviour
{
    [SerializeField]
    private float lifetime;
    
    private ProjectileBehaviour parentObject;
    
    [SerializeField]
    private bool isPoolable;

    public Vector3 LocalOffset { get; set; }

    private void Start()
    {
        if (lifetime > 0)
        {
            StartCoroutine(DoLifetime());
        }
    }
    
    public void SetLifetime(float lifetime)
    {
        this.lifetime = lifetime;
        StartCoroutine(DoLifetime());
    }

    public void SetLifetime(float lifetime, ProjectileBehaviour parent)
    {
        parentObject = parent;
        this.lifetime = lifetime;
        LocalOffset = Vector3.zero;
        StartCoroutine(DoLifetime());
    }

    public void SetPool(bool pool)
    {
        isPoolable = pool;
    }

    private System.Collections.IEnumerator DoLifetime()
    {
        yield return new WaitForSeconds(lifetime);
        if (parentObject != null)
        {
            gameObject.SetActive(false);
            transform.position = parentObject.transform.position;
            transform.parent = parentObject.transform;
            transform.localPosition = LocalOffset;
            parentObject.Primed = true;
        }
        else if (isPoolable)
        {
            var rb = GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                rb.velocity = Vector3.zero;
                rb.Sleep();
            }
            ObjectPool.Instance.ReturnPooledObject(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}