using FracturedState.Game;

public class HealIndicator : DamageIndicator
{
    protected override void Return()
    {
        ObjectPool.Instance.ReturnHealHelper(gameObject);
    }
}