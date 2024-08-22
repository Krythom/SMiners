using System;
using Microsoft.Xna.Framework;

namespace SMiners
{
    internal class HomeMiner : Miner
    {
        private readonly Point _home;

        public HomeMiner(Vector3 col, int worldX, int worldY, int x, int y)
        {
            Color = col;
            direction = Direction.Up;
            Type = MinerType.Home;
            _xMax = worldX;
            _yMax = worldY;
            Position = new Point(x, y);
            _home = new Point(x, y);
        }

        public override Point GetNext(Miner[,] world, Random rand)
        {
            int jumpsize = 1;
            Miner next = DecideMove(world, rand);

            while (next.Type != MinerType.Ore && next.Position != Position)
            {
                jumpsize++;
                next = GetFront(world, jumpsize);
            }

            return next.Position;
        }

        private Miner DecideMove(Miner[,] world, Random rand)
        {
            var neighbors = GetNeumann(world);

            double best = double.PositiveInfinity;
            
            int bestIndex = 0;

            for (int i = 0; i < neighbors.Count; i++)
            {
                if (neighbors[i].Type != MinerType.Ore) 
                    continue;
                
                double temp = Math.Pow(neighbors[i].Position.X - _home.X, 2) + Math.Pow(neighbors[i].Position.Y - _home.Y, 2);

                if (temp >= best) 
                    continue;
                
                best = temp;
                bestIndex = i;
            }

            if (best is double.PositiveInfinity)
            {
                bestIndex = rand.Next(neighbors.Count);
            }

            direction = (Direction) bestIndex;

            return neighbors[bestIndex];
        }
    }
}
