using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace SMiners
{
    public abstract class Miner
    {
        private static readonly Point[] _Cardinals =
        {
            new(0, -1),
            new(1, 0),
            new(0, 1),
            new(-1, 0)
        };

        private static readonly Point[] _ExtendedCardinals =
        {
            new(0, -1),
            new(1, -1),
            new(1, 0),
            new(1, 1),
            new(0, 1),
            new(-1, 1),
            new(-1, 0),
            new(-1, -1)
        };

        public MinerType Type;
        public Point Position;
        public MinerColor Color;
        
        protected Direction direction;
        protected EightDirection eDirection;
        
        protected int _xMax;
        protected int _yMax;

        public abstract Point GetNext(Miner[,] world, Random rand);

        public object DeepCopy()
        {
            Miner copy = (Miner) MemberwiseClone();
            copy.direction = direction;
            copy.Position = Position;
            copy.Color = Color;
            copy._yMax = _yMax;
            copy._xMax = _xMax;
            return copy;
        }

        protected Miner GetFront(Miner[,] world, int distance)
        {
            Point dir = _Cardinals[(int) direction];
            
            return world[
                Mod(Position.X + dir.X * distance, _xMax),
                Mod(Position.Y + dir.Y * distance, _yMax)
            ];
        }

        protected Miner GetFrontEight(Miner[,] world, int distance)
        {
            Point dir = _ExtendedCardinals[(int) eDirection];
            
            return world[
                Mod(Position.X + dir.X * distance, _xMax),
                Mod(Position.Y + dir.Y * distance, _yMax)
            ];
        }

        public List<Miner> GetMoore(Miner[,] world)
        {
            var neighbors = new List<Miner>();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (!(x == 0 && y == 0))
                    {
                        neighbors.Add(world[Mod(Position.X + x, _xMax), Mod(Position.Y + y, _yMax)]);
                    }
                }
            }

            return neighbors;
        }

        public List<Miner> GetNeumann(Miner[,] world)
        {
            var neighbors = new List<Miner>();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x != y && x != -y)
                    {
                        neighbors.Add(world[Mod(Position.X + x, _xMax), Mod(Position.Y + y, _yMax)]);
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