using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMiners
{
    internal class Ore : Miner
    {
        public Ore()
        {
            color = new Color(0,0,0);
            direction = Direction.Up;
            type = MinerType.Ore;
        }

        public override Vector2 GetNext(Miner[,] world)
        {
            return position;
        }
    }
}
