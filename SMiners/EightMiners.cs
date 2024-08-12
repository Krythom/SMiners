using System;
using Microsoft.Xna.Framework;

namespace SMiners
{
    internal class EightMiner : Miner
    {
        public EightMiner(Color col, int worldX, int worldY, int x, int y, Random random)
        {
            Color = col;
            eDirection = EightDirection.U;
            Type = MinerType.Eight;
            _xMax = worldX;
            _yMax = worldY;
            Position = new Point(x, y);
        }

        public override Point GetNext(Miner[,] world, Random rand)
        {
            int jumpDist = 1;
            
            Miner next = DecideMove(world, rand);

            while (next.Type != MinerType.Ore && next.Position != Position)
            {
                jumpDist++;
                next = GetFrontEight(world, jumpDist);
            }

            return next.Position;
        }

        private Miner DecideMove(Miner[,] world, Random rand)
        {
            eDirection = eDirection.Next();

            Miner m_pos = GetFrontEight(world, 1);
            if (m_pos.Type == MinerType.Ore) 
                return m_pos;

            eDirection = eDirection.Next();
            
            Miner m_neg = GetFrontEight(world, 1);
            if (m_neg.Type == MinerType.Ore) 
                return m_neg;

            return rand.Next(2) == 1 
                ? m_pos 
                : m_neg;
        }
    }
}
