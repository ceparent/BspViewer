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

            graphics.PreferredBackBufferHeight = 900;
            graphics.PreferredBackBufferWidth = 1400;
        }

        protected override void Initialize()
        {
            AddComponents();
            base.Initialize();
        }


        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);


            _devFont = Content.Load<SpriteFont>("DevFont");

            base.LoadContent();
        }

        private void AddComponents()
        {
            int o = 0;
            int d = 0;

            camera = new CameraGC(this);
            Components.Add(camera);
            camera.UpdateOrder = o++;

            bspRender = new BspRenderer(this, "Content/maps/q3dm17.bsp");
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
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1, 0);



            base.Draw(gameTime);

            float frameRate = 1 / (float)gameTime.ElapsedGameTime.TotalSeconds;
            DrawInfos(frameRate);
        }

        SpriteFont _devFont;
        private float _lowestFps = float.MaxValue;
        private void DrawInfos(float pFps)
        {
            spriteBatch.Begin();
            float offset = 25;
            int cpt = 0;

            //Player
            Vector3 RotatedTarget = camera.RotatedTarget;

            spriteBatch.DrawString(_devFont, "Position  {  X : " + Math.Floor(camera.Position.X).ToString() + " // Y : " + Math.Floor(camera.Position.Y).ToString() + "(+5) // Z: " + Math.Floor(camera.Position.Z).ToString() + " } ", new Vector2(0, offset * cpt++), Color.White);
            spriteBatch.DrawString(_devFont, "Face {  X : " + RotatedTarget.X.ToString("n2") + "  // Y : " + RotatedTarget.Y.ToString("n2") + " // Z : " + RotatedTarget.Z.ToString("n2") + " } ", new Vector2(0, offset * cpt++), Color.White);
            cpt++;



            //FPS:
            if (_lowestFps > pFps)
                _lowestFps = pFps;
            spriteBatch.DrawString(_devFont, "FPS : " + pFps.ToString("n0"), new Vector2(Window.ClientBounds.Width - _devFont.MeasureString("FPS : " + pFps.ToString("n0")).X, 0), Color.Yellow);
            spriteBatch.DrawString(_devFont, "LOW : " + _lowestFps.ToString("n0"), new Vector2(Window.ClientBounds.Width - _devFont.MeasureString("LOW : " + _lowestFps.ToString("n0")).X, offset), Color.Yellow);

            spriteBatch.End();

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap; 
        }
    }
}
