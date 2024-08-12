using EightDirection = SMiners.Miner.EightDirection;
using Direction = SMiners.Miner.Direction;

public static class EnumsExt
{
    public static EightDirection Next(this EightDirection self, int advance = 1, int cycle = 8)
    {
        return (EightDirection) (((int) self + advance) % cycle);
    }
    
    public static Direction Next(this Direction self, int advance = 1, int cycle = 4)
    {
        return (Direction) (((int) self + advance) % cycle);
    }
}