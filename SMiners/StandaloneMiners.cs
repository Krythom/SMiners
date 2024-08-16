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
        private const int WorldX = 2000;
        private const int WorldY = 2000;
        private const int MutationStrength = 1;
        private const double Rarity = 0.999997;
        private const bool BatchMode = true;

        //0 to skip drawing, 1 for base speed, higher for faster
        private const int _speedup = 0;

        
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D _tex;
        
        private int seed;
        private Random rand;
        
        private readonly List<Miner> _changed;
        private HashSet<Point> _checkSet = [];
        
        private Color _startCol;
        
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
            _startCol = new Color(rand.Next(256), rand.Next(256), rand.Next(256));

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
                        Miner added = new DiamondMiner(_startCol, WorldX, WorldY, x, y);
                        _colors.Span[x, y] = _startCol;
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

            _startCol = RandomShift(_startCol);

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
                colors[x, y] = m.Color = _startCol;
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
                    
                    int lowest = int.MaxValue;
                    
                    foreach (Miner m in neighbors)
                    {
                        int colorDist = Math.Abs(m.Color.R - current.Color.R) + Math.Abs(m.Color.G - current.Color.G) +
                                        Math.Abs(m.Color.B - current.Color.B);

                        lowest = Math.Min(colorDist, lowest);
                    }

                    if (lowest > 3 * MutationStrength)
                    {
                        current.Color = neighbors[rand.Next(4)].Color;
                        _colors.Span[current.Position.X, current.Position.Y] = current.Color;
                    }
                }
            }
        }

        private Color RandomShift(Color col)
        {
            return new Color(
                col.R + rand.Next(-MutationStrength, MutationStrength + 1),
                col.G + rand.Next(-MutationStrength, MutationStrength + 1),
                col.B + rand.Next(-MutationStrength, MutationStrength + 1)
            );
        }

        private Color ConnectedShift(Color col)
        {
            int change = rand.Next(-MutationStrength, MutationStrength + 1);
            return new Color(
                col.R + change,
                col.G + change,
                col.B + change
            );
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
    }
}