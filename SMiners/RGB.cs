using Microsoft.Xna.Framework;
using System;

namespace SMiners
{
    internal class RGB : MinerColor
    {
        float r;
        float g;
        float b;

        public RGB(float red, float green, float blue) 
        { 
            r = red;
            g = green;
            b = blue;
        }

        public override void Mutate(float strength, Random rand)
        {
            r += (2 * rand.NextSingle() * strength) - strength;
            g += (2 * rand.NextSingle() * strength) - strength;
            b += (2 * rand.NextSingle() * strength) - strength;

            Math.Clamp(r, 0, 255);
            Math.Clamp(g, 0, 255);
            Math.Clamp(b, 0, 255);
        }

        public override double GetDistance(MinerColor other)
        {
            throw new NotImplementedException();
        }

        public override Color ToColor()
        {
            return new Color((int)r, (int)g, (int)b);
        }
    }
}
