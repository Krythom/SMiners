using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using SixLabors.ImageSharp;
using Color = Microsoft.Xna.Framework.Color;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace SMiners
{
    public class StandaloneMiners : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
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
        HashSet<Vector2> checkSet = new();

        public StandaloneMiners()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            IsFixedTimeStep = false;
        }

        protected override void Initialize()
        {
            //config
            worldX = 1000;
            worldY = 1000;
            cellSize = 1;
            mutationStrength = 2;
            rarity = 0.9999;
            batchMode = false;
            startCol = new Color(rand.Next(256), rand.Next(256), rand.Next(256));

            InitWorld();
            _graphics.PreferredBackBufferHeight = worldY * cellSize;
            _graphics.PreferredBackBufferWidth = worldX * cellSize;
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
                    SaveImage();
                    saved = true;

                    this.Exit();
                }

                if (batchMode)
                {
                    completed = false;
                    saved = false;
                    Initialize();
                    iterations = 0;
                }
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
            /*
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
            */
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
                        world[x, y].position = new Vector2(x, y);
                        checkSet.Add(new Vector2(x, y));
                    }
                    else
                    {
                        world[x, y] = new Ore();
                        world[x, y].position = new Vector2(x, y);
                    }
                }
            }
        }

        private void Iterate()
        {
            List<Miner> Changed = new() { Capacity = worldX * worldY };
            HashSet<Vector2> toWake = new();
            HashSet<Vector2> toSleep = new();
            
            startCol = new Color(
                startCol.R + rand.Next(-mutationStrength, mutationStrength + 1),
                startCol.G + rand.Next(-mutationStrength, mutationStrength + 1),
                startCol.B + rand.Next(-mutationStrength, mutationStrength + 1)
            );

            foreach (Vector2 loc in checkSet)
            {
                Miner m = (Miner) world[(int) loc.X, (int) loc.Y].DeepCopy();
                Vector2 next = m.GetNext(world);

                if (m.position == next)
                {
                    toSleep.Add(next);
                }

                if (!toWake.Contains(next))
                {
                    m.position = next;
                }

                toWake.Add(m.position);
                Changed.Add(m);
            }

            foreach (Miner m in Changed)
            {
                world[(int) m.position.X, (int) m.position.Y] = m;
                m.color = new Color(
                    m.color.R + rand.Next(-mutationStrength, mutationStrength + 1),
                    m.color.G + rand.Next(-mutationStrength, mutationStrength + 1),
                    m.color.B + rand.Next(-mutationStrength, mutationStrength + 1)
                );
            }

            checkSet = toWake;
            checkSet.ExceptWith(toSleep);
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
                "ScreenCap_v" + mutationStrength + "_i" + iterations + ".png",
                new PngEncoder()
            );
        }
    }
}