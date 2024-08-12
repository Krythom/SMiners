using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMiners
{
    internal class EightMiner : Miner
    {
        Random rand;

        public EightMiner(Color col, int worldX, int worldY, int x, int y, int seed)
        {
            color = col;
            eDirection = EightDirection.U;
            type = MinerType.Eight;
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
                next = GetFrontEight(world, jumpsize);
            }

            return next.position;
        }

        private Miner DecideMove(Miner[,] world)
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
