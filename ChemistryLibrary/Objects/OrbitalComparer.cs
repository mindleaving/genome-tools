using System.Collections.Generic;

namespace ChemistryLibrary.Objects
{
    public class OrbitalComparer : IComparer<Orbital>
    {
        public static OrbitalComparer Instance { get; } = new OrbitalComparer();

        public int Compare(Orbital orbital1, Orbital orbital2)
        {
            var orbtial1Order = CalculateOrbitalOrder(orbital1);
            var orbital2Order = CalculateOrbitalOrder(orbital2);
            return orbtial1Order.CompareTo(orbital2Order);
        }

        public static int CalculateOrbitalOrder(Orbital orbital1)
        {
            return 10*orbital1.Period + 9*(int)orbital1.Type;
        }
    }
}