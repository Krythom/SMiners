using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SColor = System.Drawing.Color;
using Bitmap = System.Drawing.Bitmap;

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
            worldX = 300;
            worldY = 300;
            cellSize = 2;
            mutationStrength = 1;
            rarity = 0.9999;
            batchMode = false;
            startCol = new Color(rand.Next(256), rand.Next(256), rand.Next(256));

            InitWorld();
            _graphics.PreferredBackBufferHeight = worldY * cellSize;
            _graphics.PreferredBackBufferWidth = worldX * cellSize;
            _graphics.SynchronizeWithVerticalRetrace = false;
            _graphics.ApplyChanges();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            square = Content.Load<Texture2D>("whiteSquare");
        }

        protected override void Update(GameTime gameTime)
        {
            if (completed)
            {
                if (!saved)
                {
                    SaveImage();
                    saved = true;
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
            Debug.WriteLine("fps: " + (1000 / ms) + " (" + ms + "ms)" + " iterations: " + iterations + " active " + checkSet.Count);
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
                        world[x, y].position = new Vector2(x, y);
                        checkSet.Add(new Vector2(x,y));
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
            startCol = new Color(startCol.R + rand.Next(-mutationStrength, mutationStrength + 1), startCol.G + rand.Next(-mutationStrength, mutationStrength + 1), startCol.B + rand.Next(-mutationStrength, mutationStrength + 1));

            Parallel.ForEach(checkSet, loc =>
            {
                Miner m = (Miner) world[(int) loc.X, (int) loc.Y].DeepCopy();
                Vector2 next = m.GetNext(world);
                m.position = next;

                lock (toWake)
                {
                    toWake.Add(next);
                }

                Changed.Add(m);
            });

            foreach (Miner m in Changed)
            {
                world[(int) m.position.X, (int) m.position.Y] = m;
                m.color = startCol;
            }

            checkSet = toWake;
        }

        private void SaveImage()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Bitmap img = new Bitmap(worldX, worldY);

                for (int x = 0; x < worldX; x++)
                {
                    for (int y = 0; y < worldY; y++)
                    {
                        img.SetPixel(x, y, ConvertColor(world[x, y].color));
                    }
                }

                img.Save(@"ScreenCap_v" + mutationStrength + "_i" + iterations + ".png", System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        private SColor ConvertColor(Color col)
        {
            SColor sCol = SColor.FromArgb(255, col.R, col.G, col.B);
            return sCol;
        }
    }
}