using Microsoft.Xna.Framework;

namespace SMiners
{
    internal class Ore : Miner
    {
        public Ore(int worldX, int worldY)
        {
            color = new Color(0,0,0);
            direction = Direction.Up;
            type = MinerType.Ore;
            xMax = worldX;
            yMax = worldY;
        }

        public override Point GetNext(Miner[,] world)
        {
            return position;
        }
    }
}
