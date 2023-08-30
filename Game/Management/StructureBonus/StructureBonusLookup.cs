using System.Collections.Generic;

namespace FracturedState.Game.Management.StructureBonus
{
    public static class StructureBonusLookup
    {
        private static Dictionary<string, IStructureBonus> Lookup;
        
        public static IStructureBonus GetStructureBonus(string structure)
        {
            if (Lookup == null)
            {
                Lookup = CreateLookup();
            }

            IStructureBonus bonus;
            return Lookup.TryGetValue(structure, out bonus) ? bonus : null;
        }

        private static Dictionary<string, IStructureBonus> CreateLookup()
        {
            return new Dictionary<string, IStructureBonus>
            {
                ["Conservatory"] = new ConservatoryBonus(),
                ["Bank"] = new BankBonus(),
                ["Sanctuary"] = new SanctuaryBonus()
            };
        }
    }
}