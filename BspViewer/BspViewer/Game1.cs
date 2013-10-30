using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace BspViewer
{

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        CameraGC camera;
        BspRenderer bspRender;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            AddComponents();
            base.Initialize();
        }


        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            



            base.LoadContent();
        }

        private void AddComponents()
        {
            int o = 0;
            int d = 0;

            camera = new CameraGC(this);
            Components.Add(camera);
            camera.UpdateOrder = o++;

            bspRender = new BspRenderer(this, "Content/maps/q3dm8.bsp");
            Components.Add(bspRender);
            bspRender.UpdateOrder = o++;
            bspRender.DrawOrder = d++;


        }

        protected override void UnloadContent()
        {
            base.UnloadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState ks = Keyboard.GetState();

            if (ks.IsKeyDown(Keys.Escape))
                this.Exit();


            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);


            base.Draw(gameTime);
        }
    }
}
