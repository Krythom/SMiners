using Microsoft.Xna.Framework;
using System;

namespace SMiners
{
    abstract class Miner
    {
        public MinerType type;
        public Direction direction;
        public Vector2[] dir_lut = { new Vector2(0, -1), new Vector2(1, 0), new Vector2(0, 1), new Vector2(-1, 0) };
        public Vector2 position;
        public int xMax;
        public int yMax;
        public Color color;

        public abstract Vector2 GetNext(Miner[,] world);

        public object DeepCopy()
        {
            Miner copy = (Miner) this.MemberwiseClone();
            copy.direction = direction;
            copy.position = position;
            copy.color = color;
            copy.yMax = yMax;
            copy.xMax = xMax;
            return copy;
        }

        public static int Mod(int x, int m)
        {
            return (Math.Abs(x * m) + x) % m;
        }

        public enum MinerType
        {
            Ore,
            Diamond,
        }

        public enum Direction
        {
            Up,
            Right,
            Down,
            Left
        }
    }
}
