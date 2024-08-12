using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace SMiners
{
    internal class HomeMiner : Miner
    {
        private readonly Point home;

        public HomeMiner(Color col, int worldX, int worldY, int x, int y)
        {
            color = col;
            direction = Direction.Up;
            type = MinerType.Home;
            xMax = worldX;
            yMax = worldY;
            position = new Point(x, y);
            home = new Point(x, y);
        }

        public override Point GetNext(Miner[,] world, Random rand)
        {
            int jumpsize = 1;
            Miner next = DecideMove(world, rand);

            while (next.type != MinerType.Ore && next.position != position)
            {
                jumpsize++;
                next = GetFront(world, jumpsize);
            }

            return next.position;
        }

        private Miner DecideMove(Miner[,] world, Random rand)
        {
            List<Miner> neighbors = GetNeumann(world);
            double temp;
            double best = double.MaxValue;
            int bestIndex = 0;

            for (int i = 0; i < neighbors.Count; i++)
            {
                if (neighbors[i].type == MinerType.Ore)
                {
                    temp = Math.Pow(neighbors[i].position.X - home.X, 2) + Math.Pow(neighbors[i].position.Y - home.Y, 2);
                    if (temp < best)
                    {
                        best = temp;
                        bestIndex = i;
                    }
                }
            }

            if (best == double.MaxValue)
            {
                bestIndex = rand.Next(neighbors.Count);
            }

            direction = (Direction) bestIndex;

            return neighbors[bestIndex];
        }
    }
}
