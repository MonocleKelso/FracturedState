using FracturedState.Game.Data;
using UnityEngine;

public class BleedTicker : MonoBehaviour
{
    private float tick;
    private int damageAmount;
    private UnitManager unit;

    public void Init(float duration, int damageAmount)
    {
        this.damageAmount = damageAmount;
        tick = 1;
        unit = GetComponent<UnitManager>();
        Destroy(this, duration);
    }

    private void Update()
    {
        if (unit == null || !unit.IsAlive) return;
        
        tick -= Time.deltaTime;
        if (tick > 0) return;
        
        unit.NetMsg.CmdTakeDamage(damageAmount, null, Weapon.DummyName);
        tick = 1;
    }
}