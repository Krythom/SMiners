using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using CommunityToolkit.HighPerformance;
using SixLabors.ImageSharp;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Point = Microsoft.Xna.Framework.Point;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using Microsoft.Xna.Framework.Input;

namespace SMiners
{
    public class StandaloneMiners : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        List<Miner> _changed;
        Random seeder = new Random();
        private Texture2D _tex;
        int seed;
        Random rand;
        Texture2D square;
        Miner[,] world;
        Color startCol;
        int worldX;
        int worldY;
        int cellSize;
        int mutationStrength;
        double rarity;
        bool batchMode;
        bool completed = false;
        bool saved = false;
        int iterations = 0;
        HashSet<Point> checkSet = new();
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
            seed = seeder.Next();
            rand = new Random(seed);
            worldX = 1000;
            worldY = 1000;
            cellSize = 1;
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
            square = Content.Load<Texture2D>("whiteSquare");
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
                        Miner added = new EightMiner(startCol, worldX, worldY, x, y, rand.Next());
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
                Point next = m.GetNext(world);

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
                        if (colorDist < lowest)
                        {
                            lowest = colorDist;
                        }
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
            col.R + seeder.Next(-mutationStrength, mutationStrength + 1),
            col.G + seeder.Next(-mutationStrength, mutationStrength + 1),
            col.B + seeder.Next(-mutationStrength, mutationStrength + 1)
            );
        }

        private Color ConnectedShift(Color col)
        {
            int change = seeder.Next(-mutationStrength, mutationStrength + 1);
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
            var img = new Image<Rgba32>(worldX, worldY);

            for (int x = 0; x < worldX; x++)
            {
                for (int y = 0; y < worldY; y++)
                {
                    img[x, y] = new Rgba32(world[x, y].color.PackedValue);
                }
            }

            img.Save(
                "s" + seed + "_v" + mutationStrength + "_r" + rarity + "_i" + iterations + ".png",
                new PngEncoder()
            );
        }
    }
}