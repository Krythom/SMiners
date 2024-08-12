using System;
using Microsoft.Xna.Framework;

namespace SMiners
{
    internal class DiamondMiner : Miner
    {
        private readonly Random rand;

        public DiamondMiner(Color col, int worldX, int worldY, int x, int y, int seed)
        {
            color = col;
            direction = Direction.Up;
            type = MinerType.Diamond;
            xMax = worldX;
            yMax = worldY;
            rand = new Random(seed);
            position = new Point(x, y);
        }

        public override Point GetNext(Miner[,] world)
        {
            int jumpsize = 1;
            Miner next = DecideMove(world);

            while (next.type != MinerType.Ore && next.position != position)
            {
                jumpsize++;
                next = GetFront(world, jumpsize);
            }

            return next.position;
        }

        private Miner DecideMove(Miner[,] world)
        {
            direction = (Direction)(((int)direction + 1) % 4);

            Miner m_pos = GetFront(world, 1);
            if (m_pos.type != MinerType.Diamond) return m_pos;

            direction = (Direction)(((int)direction + 2) % 4);
            Miner m_neg = GetFront(world, 1);
            if (m_neg.type != MinerType.Diamond) return m_neg;

            return (rand.Next(2) == 1) ? m_pos : m_neg ;
        }
    }
}
