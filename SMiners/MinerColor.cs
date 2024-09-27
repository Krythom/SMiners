using Microsoft.Xna.Framework;
using System;

namespace SMiners
{
    public abstract class MinerColor
    {
        public abstract void Mutate(float strength, Random rand);

        public abstract Color ToColor();

        public abstract double GetDistance(MinerColor other);
    }
}
