using System;

namespace ChemistryLibrary
{
    public enum SpinType
    {
        Up,
        Down
    }

    public static class SpinExtensions
    {
        public static SpinType Invert(this SpinType spin)
        {
            switch (spin)
            {
                case SpinType.Up:
                    return SpinType.Down;
                case SpinType.Down:
                    return SpinType.Up;
                default:
                    throw new ArgumentOutOfRangeException(nameof(spin), spin, null);
            }
        }
    }
}