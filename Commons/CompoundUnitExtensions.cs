using System;
using System.Collections.Generic;
using System.Linq;

namespace Commons
{
    public static class CompoundUnitExtensions
    {
        private static readonly Dictionary<CompoundUnit, Unit> CompoundUnitToUnitMap =
            ((Unit[]) Enum.GetValues(typeof(Unit))).ToDictionary(x => x.ToCompoundUnit(), x => x);

        public static Unit ToUnit(this CompoundUnit unit)
        {
            foreach (var kvp in CompoundUnitToUnitMap)
            {
                if (kvp.Key.Equals(unit))
                    return kvp.Value;
            }
            return Unit.Compound;
        }
    }
}