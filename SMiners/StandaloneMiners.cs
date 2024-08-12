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

namespace SMiners
{
    public class StandaloneMiners : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private readonly List<Miner> _changed;
        private Texture2D _tex;
        private int seed;
        private Random rand;
        private Miner[,] world;
        private Color startCol;
        private int worldX;
        private int worldY;
        private int mutationStrength;
        private double rarity;
        private bool batchMode;
        private bool completed;
        private bool saved;
        private int iterations;
        private HashSet<Point> checkSet = new();
        private Color[] _backingColors;
        private Memory2D<Color> _colors;

        public StandaloneMiners()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            IsFixedTimeStep = false;
            _changed = new List<Miner> { Capacity = worldX * worldY };
        }

        protected override void Initialize()
        {
            //config
            seed = Environment.TickCount;
            rand = new Random(seed);
            worldX = 1000;
            worldY = 1000;
            mutationStrength = 1;
            rarity = 0.9999;
            batchMode = true;
            startCol = new Color(rand.Next(256), rand.Next(256), rand.Next(256));

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
            if (checkSet.Count == 0)
            {
                completed = true;
            }

            if (completed)
            {
                if (!saved)
                {
                    Cleanup();
                    SaveImage();
                    saved = true;
                }
                if (!batchMode)
                {
                    return;
                }

                completed = false;
                saved = false;
                
                Initialize();
                iterations = 0;
            }
            else
            {
                Iterate();
                iterations++;
            }

            double ms = gameTime.ElapsedGameTime.TotalMilliseconds;
            Debug.WriteLine(
                "fps: " + (1000 / ms) + " (" + ms + "ms)" + " iterations: " + iterations + " active " + checkSet.Count
            );

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin();

            _tex.SetData(_backingColors);
            _spriteBatch.Draw(_tex, new Vector2(0, 0), Color.White);

            _spriteBatch.End();
            
            base.Draw(gameTime);
        }

        private void InitWorld()
        {
            world = new Miner[worldX, worldY];
            _backingColors = new Color[worldX * worldY];
            _colors = new Memory2D<Color>(_backingColors, worldX, worldY);
            _tex = new Texture2D(GraphicsDevice, worldX, worldY);

            for (int x = 0; x < worldX; x++)
            {
                for (int y = 0; y < worldY; y++)
                {
                    if (rand.NextDouble() > rarity)
                    {
                        Miner added = new EightMiner(startCol, worldX, worldY, x, y, rand);
                        world[x, y] = added;
                        checkSet.Add(new Point(x, y));
                    }
                    else
                    {
                        world[x, y] = new Ore(worldX, worldY);
                        world[x, y].position = new Point(x, y);
                    }
                }
            }
        }

        private void Iterate()
        {
            HashSet<Point> toWake = new();
            HashSet<Point> toSleep = new();

            startCol = RandomShift(startCol);

            foreach (Point loc in checkSet)
            {
                Miner m = (Miner) world[loc.X, loc.Y].DeepCopy();
                Point next = m.GetNext(world, rand);

                if (m.position == next)
                {
                    toSleep.Add(next);
                }

                if (!toWake.Contains(next))
                {
                    m.position = next;
                }

                toWake.Add(m.position);
                _changed.Add(m);
            }

            var colors = _colors.Span;

            foreach (Miner m in _changed)
            {
                var (x, y) = m.position;
                world[x, y] = m;
                colors[x, y] = m.color = startCol;
            }

            checkSet = toWake;
            checkSet.ExceptWith(toSleep);
            _changed.Clear();
        }

        private void Cleanup()
        {
            List<Miner> neighbors;
            for (int x = 0; x < worldX;  x++)
            {
                for (int y = 0; y < worldY; y++)
                {
                    Miner current = world[x, y];
                    neighbors = current.GetMoore(world);
                    int lowest = 999;
                    foreach (Miner m in neighbors)
                    {
                        int colorDist = Math.Abs(m.color.R - current.color.R) + Math.Abs(m.color.G - current.color.G) + Math.Abs(m.color.B - current.color.B);

                        lowest = Math.Max(colorDist, lowest);
                        
                    }
                    if (lowest > 3 * mutationStrength)
                    {
                        current.color = neighbors[rand.Next(4)].color;
                    }
                }
            }
        }

        private Color RandomShift(Color col)
        {
            return new Color(
                col.R + rand.Next(-mutationStrength, mutationStrength + 1),
                col.G + rand.Next(-mutationStrength, mutationStrength + 1),
                col.B + rand.Next(-mutationStrength, mutationStrength + 1)
            );
        }

        private Color ConnectedShift(Color col)
        {
            int change = rand.Next(-mutationStrength, mutationStrength + 1);
            return new Color(
                col.R + change,
                col.G + change,
                col.B + change
            );
        }

        private Color FromPalette()
        {
            return Color.Aqua;
        }

        private void SaveImage()
        {
            unsafe
            {
                fixed (void* ptr = _backingColors)
                {

                    Image<Rgba32> img = Image.WrapMemory<Rgba32>(
                        ptr,
                        _backingColors.AsSpan().AsBytes().Length,
                        worldX,
                        worldY
                    );

                    string date = DateTime.Now.ToString("s").Replace("T", " ").Replace(":", "-");

                    img.Save(
                        $"{date}s{seed}_v{mutationStrength}_r{rarity}_i{iterations}.png",
                        new PngEncoder()
                    );
                }
            }
        }
    }
}