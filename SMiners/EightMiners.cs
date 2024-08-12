using System;
using Microsoft.Xna.Framework;

namespace SMiners
{
    internal class EightMiner : Miner
    {
        public EightMiner(Color col, int worldX, int worldY, int x, int y, Random random)
        {
            color = col;
            eDirection = EightDirection.U;
            type = MinerType.Eight;
            xMax = worldX;
            yMax = worldY;
            position = new Point(x, y);
        }

        public override Point GetNext(Miner[,] world, Random rand)
        {
            int jumpsize = 1;
            Miner next = DecideMove(world, rand);

            while (next.type != MinerType.Ore && next.position != position)
            {
                jumpsize++;
                next = GetFrontEight(world, jumpsize);
            }

            return next.position;
        }

        private Miner DecideMove(Miner[,] world, Random rand)
        {
            eDirection = (EightDirection)(((int)eDirection + 1) % 8);

            Miner m_pos = GetFrontEight(world, 1);
            if (m_pos.type == MinerType.Ore) return m_pos;

            eDirection = (EightDirection)(((int)eDirection + 1) % 8);
            Miner m_neg = GetFrontEight(world, 1);
            if (m_neg.type == MinerType.Ore) return m_neg;

            return (rand.Next(2) == 1) ? m_pos : m_neg;
        }
    }
}
