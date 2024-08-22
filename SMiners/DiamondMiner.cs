using System;
using Microsoft.Xna.Framework;

namespace SMiners
{
    internal class DiamondMiner : Miner
    {
        public DiamondMiner(Vector3 col, int worldX, int worldY, int x, int y)
        {
            Color = col;
            direction = Direction.Up;
            Type = MinerType.Diamond;
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
                next = GetFront(world, jumpDist);
            }

            return next.Position;
        }

        private Miner DecideMove(Miner[,] world, Random rng)
        {
            direction = direction.Next();

            Miner m_pos = GetFront(world, 1);
            if (m_pos.Type != MinerType.Diamond)
                return m_pos;

            direction = direction.Next(2);
            
            Miner m_neg = GetFront(world, 1);
            if (m_neg.Type != MinerType.Diamond)
                return m_neg;

            return rng.Next(2) == 1 
                ? m_pos 
                : m_neg;
        }
    }
}