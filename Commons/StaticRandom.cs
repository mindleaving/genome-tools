using System;

namespace Commons
{
    public static class StaticRandom
    {
        public static Random Rng { get; } = new Random();
    }
}
