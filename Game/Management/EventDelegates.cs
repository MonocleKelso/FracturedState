namespace FracturedState.Game.Management
{
	// unit level events
	public delegate void MoveCommandDelegate(UnityEngine.Vector3 destination);
    public delegate void TargetChangedDelegate(UnitManager target);
    public delegate void UnitSelectedDelegate(bool propagate);
    public delegate void UnitUnSelectedDelegate(bool propagate);
    public delegate void EnterCommandDelegate(StructureManager structure);
    public delegate void ExitCommandDelegate(UnityEngine.Vector3 destination);
    public delegate void TakeFirePointDelegate(UnityEngine.Transform firePoint);
    public delegate void TakeCoverPointDelegate(CoverManager cover, UnityEngine.Transform coverPoint);

    // team level events
    public delegate void DefeatDelegate(Team team);
}