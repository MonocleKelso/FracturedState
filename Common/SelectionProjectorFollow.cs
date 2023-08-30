using UnityEngine;
using FracturedState.Game;

public class SelectionProjectorFollow : MonoBehaviour
{
    [SerializeField] private Renderer _render;
    
    private UnitManager _target;
    private int _lastHealth;
    
    private bool _fade;
    private float _lastFadeTime;

    private const float FadeKeep = 2;

    public void SetTarget(UnitManager target)
    {
        _target = target;
        _fade = false;
        if (_target == null) return;
        UpdateHealthColor();
        gameObject.SetLayerRecursively(target.gameObject.layer);
    }

    public void DoFade()
    {
        _fade = true;
    }

    public void Update()
    {
        if (_target == null)
        {
            ObjectPool.Instance.ReturnSelectionProjector(gameObject);
            return;
        }
        
        transform.position = _target.transform.position + Vector3.up * 0.1f;
        if (_target.DamageProcessor.CurrentHealth != _lastHealth)
        {
            UpdateHealthColor();
        }

        if (!_fade) return;

        if (_target.IsMine && SelectionManager.Instance.SelectedUnits.Contains(_target))
        {
            ObjectPool.Instance.ReturnSelectionProjector(gameObject);
            return;
        }
        
        _lastFadeTime -= Time.deltaTime;
        if (_lastFadeTime > 0) return;
        
        var c = _render.material.color;
        c.a -= Time.deltaTime * 0.25f;
        if (c.a <= 0)
        {
            ObjectPool.Instance.ReturnSelectionProjector(gameObject);
        }
        else
        {
            _render.material.color = c;
        }
    }

    private void UpdateHealthColor()
    {
        _lastHealth = _target.DamageProcessor.CurrentHealth;
        var color = Loader.Instance.HealthGradient.Evaluate(1 -(_lastHealth / (float) _target.DamageProcessor.MaxHealth));
        _render.material.color = color;
        _lastFadeTime = FadeKeep;
    }
}