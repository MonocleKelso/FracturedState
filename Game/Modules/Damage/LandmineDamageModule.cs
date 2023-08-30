namespace FracturedState.Game.Modules
{
    /// <summary>
    /// A damage module for mines that kill themselves and deal damage to nearby units via their death weapon
    /// </summary>
    public class LandmineDamageModule : UnitDamageModule
    {
        // mine's cannot be healed
        public override void Heal(int amount) { }

    }
}