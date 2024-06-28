using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMiners
{
    internal class DiamondMiner : Miner
    {
        Random rand = new Random();

        public DiamondMiner(Color col, int worldX, int worldY)
        {
            color = col;
            direction = Direction.Up;
            type = MinerType.Diamond;
            xMax = worldX;
            yMax = worldY;
        }

        public override Vector2 GetNext(Miner[,] world)
        {
            int jumpsize = 1;
            Miner next = DecideMove(world);

            while (next.type == MinerType.Diamond && next.position != position)
            {
                jumpsize++;
                next = GetFront(world, jumpsize);
            }

            return next.position;
        }

        private Miner DecideMove(Miner[,] world)
        {
            direction = (Direction) (((int) direction + 1) % 4);
            Miner m_pos = GetFront(world, 1);
            if (m_pos.type != MinerType.Diamond) return m_pos;

            direction = (Direction)(((int)direction + 2) % 4);
            Miner m_neg = GetFront(world, 1);
            if (m_neg.type != MinerType.Diamond) return m_neg;

            return (rand.Next(2) == 1) ? m_pos : m_neg ;
        }

        private Miner GetFront(Miner[,] world, int distance)
        {
            Vector2 target = position + dir_lut[(int) direction] * distance;
            target.X = Mod((int) target.X, xMax);
            target.Y = Mod((int) target.Y, yMax);
            return world[(int) target.X, (int) target.Y];
        }
    }
}
