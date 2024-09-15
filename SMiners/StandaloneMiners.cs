using System;
using System.Collections.Generic;
using System.Diagnostics;
using CommunityToolkit.HighPerformance;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace SMiners
{
    public class StandaloneMiners : Game
    {
        private const int WorldX = 3000;
        private const int WorldY = 3000;
        private const float MutationStrength = 0.5f;
        private const double Rarity = 0.999999;
        private const bool BatchMode = true;
        private const bool UseHSL = false;

        //0 to skip drawing, 1 for base speed, higher for faster
        private const int _speedup = 500;

        
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D _tex;
        
        private int seed;
        private Random rand;
        
        private readonly List<Miner> _changed;
        private HashSet<Point> _checkSet = [];
        
        private Vector3 _startCol;
        
        private bool _completed;
        private bool _saved;
        private int _iterations;
        
        private Miner[,] world;
        private Color[] _backingColors;
        private Memory2D<Color> _colors;

        public StandaloneMiners()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            IsFixedTimeStep = false;
            _changed = new List<Miner> { Capacity = WorldX * WorldY };
        }

        protected override void Initialize()
        {
            seed = Environment.TickCount;
            rand = new Random(seed);
            _startCol = new Vector3(rand.Next(256), rand.Next(256), rand.Next(256));

            InitWorld();
            _graphics.PreferredBackBufferHeight = 1000;
            _graphics.PreferredBackBufferWidth = 1000;
            _graphics.SynchronizeWithVerticalRetrace = false;
            _graphics.ApplyChanges();

            InactiveSleepTime = new TimeSpan(0);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            if (_checkSet.Count == 0)
            {
                _completed = true;
            }

            if (_completed)
            {
                if (!_saved)
                {
                    SaveImage();
                    _saved = true;
                }

                if (!BatchMode)
                    return;

                _completed = false;
                _saved = false;
                Initialize();
                _iterations = 0;
            }
            else
            {
                Iterate();
                _iterations++;
                if (_speedup == 0 || _iterations % _speedup != 0)
                {
                    SuppressDraw();
                }
            }

            double ms = gameTime.ElapsedGameTime.TotalMilliseconds;
            Debug.WriteLine(
                "fps: " + (1000 / ms) + " (" + ms + "ms)" + " iterations: " + _iterations + " active " + _checkSet.Count
            );

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin();

            _tex.SetData(_backingColors);
            _spriteBatch.Draw(_tex, new Rectangle(0,0,1000,1000), Color.White);

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void InitWorld()
        {
            world = new Miner[WorldX, WorldY];
            _backingColors = new Color[WorldX * WorldY];
            _colors = new Memory2D<Color>(_backingColors, WorldX, WorldY);
            _tex = new Texture2D(GraphicsDevice, WorldX, WorldY);

            foreach (ref Color c in _backingColors.AsSpan())
                c = Color.Black;

            for (int x = 0; x < WorldX; x++)
            {
                for (int y = 0; y < WorldY; y++)
                {
                    if (rand.NextDouble() > Rarity)
                    {
                        Miner added = new DiamondMiner(HslConvert(_startCol), WorldX, WorldY, x, y);
                        if (UseHSL)
                        {
                            added.Color = HslConvert(added.Color);
                        }

                        _colors.Span[x, y] = new Color((int) added.Color.X, (int) added.Color.Y, (int) added.Color.Z);
                        world[x, y] = added;
                        _checkSet.Add(new Point(x, y));
                    }
                    else
                    {
                        world[x, y] = new Ore(WorldX, WorldY);
                        world[x, y].Position = new Point(x, y);
                    }
                }
            }
        }

        private void Iterate()
        {
            HashSet<Point> toWake = [];
            HashSet<Point> toSleep = [];

            if (rand.NextDouble() > 1)
            {
                _startCol = ConnectedShift(_startCol);
            }
            else
            {
                _startCol = RandomShift(_startCol);
            }

            foreach (Point loc in _checkSet)
            {
                Miner m = (Miner) world[loc.X, loc.Y].DeepCopy();
                Point next = m.GetNext(world, rand);

                if (m.Position == next)
                    toSleep.Add(next);

                if (!toWake.Contains(next))
                    m.Position = next;

                toWake.Add(m.Position);
                _changed.Add(m);
            }

            var colors = _colors.Span;

            foreach (Miner m in _changed)
            {
                var (x, y) = m.Position;
                world[x, y] = m;

                if (UseHSL)
                {
                    m.Color = HslConvert(_startCol);
                }
                else
                {
                    m.Color = _startCol;
                }

                colors[x, y] = new Color((int) m.Color.X, (int) m.Color.Y, (int) m.Color.Z);
            }

            _checkSet = toWake;
            _checkSet.ExceptWith(toSleep);
            _changed.Clear();
        }

        private void Cleanup()
        {
            for (int x = 0; x < WorldX; x++)
            {
                for (int y = 0; y < WorldY; y++)
                {
                    Miner current = world[x, y];
                    var neighbors = current.GetNeumann(world);
                    
                    float lowest = float.MaxValue;
                    
                    foreach (Miner m in neighbors)
                    {
                        float colorDist = Math.Abs(m.Color.X - current.Color.X) + Math.Abs(m.Color.Y - current.Color.Y) +
                                        Math.Abs(m.Color.Z - current.Color.Z);

                        lowest = Math.Min(colorDist, lowest);
                    }

                    if (lowest > 3 * MutationStrength)
                    {
                        current.Color = neighbors[rand.Next(4)].Color;
                        _colors.Span[current.Position.X, current.Position.Y] = new Color((int) current.Color.X, (int) current.Color.Y, (int) current.Color.Z);
                    }
                }
            }
        }

        private Vector3 RandomShift(Vector3 col)
        {
            if (UseHSL)
            {
                return new Vector3(
                    (col.X + (rand.NextSingle() * (MutationStrength + MutationStrength)) - MutationStrength) % 360,
                    Math.Clamp(col.Y + (rand.NextSingle() * (MutationStrength + MutationStrength)) - MutationStrength, 0, 256),
                    Math.Clamp(col.Z + (rand.NextSingle() * (MutationStrength + MutationStrength)) - MutationStrength, 0, 256)
                );
            }
            else
            {
                return new Vector3(
                    Math.Clamp(col.X + (rand.NextSingle() * (MutationStrength + MutationStrength)) - MutationStrength, 0, 256),
                    Math.Clamp(col.Y + (rand.NextSingle() * (MutationStrength + MutationStrength)) - MutationStrength, 0, 256),
                    Math.Clamp(col.Z + (rand.NextSingle() * (MutationStrength + MutationStrength)) - MutationStrength, 0, 256)
                );
            }
        }

        private Vector3 ConnectedShift(Vector3 col)
        {
            float change = (rand.NextSingle() * (MutationStrength + MutationStrength)) - MutationStrength;

            if (UseHSL)
            {
                return new Vector3(
                    (col.X + change) % 360,
                    Math.Clamp(col.Y + change, 0, 256),
                    Math.Clamp(col.Z + change, 0, 256)
                );
            }
            else
            {
                return new Vector3(
                    Math.Clamp(col.X + change, 0, 256),
                    Math.Clamp(col.Y + change, 0, 256),
                    Math.Clamp(col.Z + change, 0, 256)
                );
            }

        }

        private void SaveImage()
        {
            unsafe
            {
                fixed (void* ptr = _backingColors)
                {
                    var img = Image.WrapMemory<Rgba32>(
                        ptr,
                        _backingColors.AsSpan().AsBytes().Length,
                        WorldX,
                        WorldY
                    );

                    string date = DateTime.Now.ToString("s").Replace("T", " ").Replace(":", "-");

                    img.Save(
                        $"{date}s{seed}_v{MutationStrength}_r{Rarity}_i{_iterations}.png",
                        new PngEncoder()
                    );
                }
            }
        }

        private Vector3 HslConvert(Vector3 col)
        {
            col = RangeAdjust(col);

            float c = (1 - Math.Abs((2 * col.Z) - 1)) * col.Y;
            float h = col.X / 60;
            float q = c * (1 - Math.Abs((h % 2) - 1));
            float m = col.Z - c / 2;

            Vector3 newCol;

            if (h <=1)
            {
                newCol = new Vector3(c, q, 0);
            }
            else if (h <= 2)
            {
                newCol = new Vector3(q, c, 0);
            }
            else if (h <= 3)
            {
                newCol = new Vector3(0, c, q);
            }
            else if (h <= 4)
            {
                newCol = new Vector3(0, q, c);
            }
            else if (h <= 5)
            {
                newCol = new Vector3(q, 0, c);
            }
            else
            {
                newCol = new Vector3(c, 0, q);
            }

            newCol = new Vector3(newCol.X + m, newCol.Y + m, newCol.Z + m);
            newCol *= 255;
            return newCol;
        }

        private Vector3 RangeAdjust(Vector3 col)
        {
            return new Vector3(360 * col.X/255, col.Y/255, col.Z/255);
        }
    }
}