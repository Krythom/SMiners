using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace SMiners
{
    internal abstract class Miner
    {
        public MinerType type;
        protected Direction direction;
        protected EightDirection eDirection;
        private static readonly Point[] dir_lut = { new(0, -1), new(1, 0), new(0, 1), new(-1, 0) };
        private static readonly Point[] edir_lut = { new(0, -1), new(1, -1), new(1, 0), new(1, 1), new(0,1), new(-1,1), new(-1,0), new(-1,-1) };
        public Point position;
        protected int xMax;
        protected int yMax;
        public Color color;

        public abstract Point GetNext(Miner[,] world, Random rand);

        public object DeepCopy()
        {
            Miner copy = (Miner) MemberwiseClone();
            copy.direction = direction;
            copy.position = position;
            copy.color = color;
            copy.yMax = yMax;
            copy.xMax = xMax;
            return copy;
        }

        protected Miner GetFront(Miner[,] world, int distance)
        {
            Point dir = dir_lut[(int)direction];
            Point target = new Point(position.X + dir.X * distance, position.Y + dir.Y * distance);
            target.X = Mod(target.X, xMax);
            target.Y = Mod(target.Y, yMax);
            return world[target.X, target.Y];
        }
        
        protected Miner GetFrontEight(Miner[,] world, int distance)
        {
            Point dir = edir_lut[(int)eDirection];
            Point target = new Point(position.X + dir.X * distance, position.Y + dir.Y * distance);
            target.X = Mod(target.X, xMax);
            target.Y = Mod(target.Y, yMax);
            return world[target.X, target.Y];
        }

        public List<Miner> GetMoore(Miner[,] world)
        {
            List<Miner> neighbors = new List<Miner>();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (!(x == 0 && y == 0))
                    {
                        neighbors.Add(world[Mod(position.X + x, xMax), Mod(position.Y + y, yMax)]);
                    }
                }
            }

            return neighbors;
        }

        protected List<Miner> GetNeumann(Miner[,] world)
        {
            List<Miner> neighbors = new List<Miner>();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x != y && x != -y)
                    {
                        neighbors.Add(world[Mod(position.X + x, xMax), Mod(position.Y + y, yMax)]);
                    }
                }
            }

            return neighbors;
        }

        private static int Mod(int x, int m)
        {
            return (Math.Abs(x * m) + x) % m;
        }

        public enum MinerType
        {
            Ore,
            Diamond,
            Home,
            Eight
        }

        public enum Direction
        {
            Up,
            Right,
            Down,
            Left
        }
        public enum EightDirection
        {
            U,
            UR,
            R,
            DR,
            D,
            DL,
            L,
            UL
        }
    }
}
