using System;
using Microsoft.Xna.Framework;

namespace SMiners
{
    internal class Ore : Miner
    {
        public Ore(int worldX, int worldY)
        {
            Color = new Vector3(0,0,0);
            direction = Direction.Up;
            Type = MinerType.Ore;
            _xMax = worldX;
            _yMax = worldY;
        }

        public override Point GetNext(Miner[,] world, Random rand)
        {
            return Position;
        }
    }
}
