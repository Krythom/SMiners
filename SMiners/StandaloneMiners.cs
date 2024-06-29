using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        readonly Random rand = new();
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
            worldX = 1000;
            worldY = 1000;
            cellSize = 1;
            mutationStrength = 1;
            rarity = 0.99999;
            batchMode = false;
            startCol = new Color(rand.Next(256), rand.Next(256), rand.Next(256));

            InitWorld();
            _graphics.PreferredBackBufferHeight = (worldY * cellSize);
            _graphics.PreferredBackBufferWidth = (worldX * cellSize);
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
                    Exit();
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


            if(iterations % 100 != 0)
            {
                SuppressDraw();
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

            for (int x = 0; x < worldX; x++)
            {
                for (int y = 0; y < worldY; y++)
                {
                    Rectangle squarePos = new(new Point((x * cellSize), (y * cellSize)), new Point(cellSize, cellSize));
                    _spriteBatch.Draw(square, squarePos, world[x, y].color);
                }
            }


            _spriteBatch.End();
            base.Draw(gameTime);
        }

        private void InitWorld()
        {
            world = new Miner[worldX, worldY];

            for (int x = 0; x < worldX; x++)
            {
                for (int y = 0; y < worldY; y++)
                {
                    if (rand.NextDouble() > rarity)
                    {
                        Miner added = new DiamondMiner(startCol, worldX, worldY);
                        world[x, y] = added;
                        world[x, y].position = new Point(x, y);
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
            
            startCol = new Color(
                startCol.R + rand.Next(-mutationStrength, mutationStrength + 1),
                startCol.G + rand.Next(-mutationStrength, mutationStrength + 1),
                startCol.B + rand.Next(-mutationStrength, mutationStrength + 1)
            );

            foreach (Point loc in checkSet)
            {
                Miner m = (Miner) world[(int) loc.X, (int) loc.Y].DeepCopy();
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

            foreach (Miner m in _changed)
            {
                world[(int)m.position.X, (int)m.position.Y] = m;
                m.color = startCol;
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
                    neighbors = current.GetNeumann(world);
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
                "ScreenCap_v" + rand.Next(10) + "_i" + iterations + ".png",
                new PngEncoder()
            );
        }
    }
}