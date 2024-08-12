using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMiners
{
    internal class HomeMiner : Miner
    {
        Random rand;
        Point home;

        public HomeMiner(Color col, int worldX, int worldY, int x, int y, int seed)
        {
            color = col;
            direction = Direction.Up;
            type = MinerType.Home;
            xMax = worldX;
            yMax = worldY;
            rand = new Random(seed);
            position = new Point(x, y);
            home = new Point(x, y);
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
