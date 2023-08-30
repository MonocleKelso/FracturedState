using System.Linq;

namespace FracturedState.Game.Mutators
{
    public class CheckpointDebuffMutator : IMutator
    {
        public int Cost => 1;
        
        public bool CanMutate(UnitManager unit)
        {
            return unit.Data.Name == "Checkpoint";
        }

        public void Mutate(UnitManager unit)
        {
            var skills = unit.Data.Abilities.Where(a => a != "CheckpointDeploy").ToList();
            skills.Add("CheckpointDeployBuffed");
            unit.Data.Abilities = skills.ToArray();
        }
    }
}