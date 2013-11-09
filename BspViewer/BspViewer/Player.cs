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
    class Player:GameComponent
    {

        public Player(Game pGame)
            :base(pGame)
        {
            Game.Services.AddService(typeof(Player), this);
        }

        private Vector3 _position;
        public Vector3 Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public bool NoClip { get; set; }

        Vector2 _rotation;
        public Vector2 Rotation
        {
            get { return _rotation; }
            set
            {
                _rotation = value;
                if (_rotation.Y <= -MathHelper.PiOver2)
                    _rotation.Y = -MathHelper.PiOver2 + 0.0001f;
                if (_rotation.Y >= MathHelper.PiOver2)
                    _rotation.Y = MathHelper.PiOver2 - 0.0001f;
            }
        }

        BspCollisions collisions;
        public override void Initialize()
        {

            Mouse.SetPosition(Game.Window.ClientBounds.Width / 2, Game.Window.ClientBounds.Height / 2);
            originalMouse = Mouse.GetState();

            _position = new Vector3(765, 125, -691);

            collisions = (BspCollisions)Game.Services.GetService(typeof(BspCollisions));

            NoClip = true;

            base.Initialize();
        }


        public override void Update(GameTime gameTime)
        {

            MoveAround();

            base.Update(gameTime);
        }

        MouseState originalMouse;
        float CameraSpeed = 0.003f;
        float WalkSpeed = 10f;
        float gravity = -0.8f;
        float velocity = 0;
        float height = 70;
        MouseState oldMs;
        KeyboardState oldKs;
        private void MoveAround()
        {
            KeyboardState ks = Keyboard.GetState();
            MouseState ms = Mouse.GetState();

            // Noclip
            if (ks.IsKeyDown(Keys.V) && oldKs.IsKeyUp(Keys.V))
            {
                NoClip = !NoClip;
                velocity = 0;
            }



            if (ms != originalMouse && Game.IsActive)
            {
                float xdiff = ms.X - originalMouse.X;
                float ydiff = ms.Y - originalMouse.Y;

                Vector2 diff = new Vector2(-xdiff * CameraSpeed, -ydiff * CameraSpeed);

                Rotation += diff;
                Mouse.SetPosition(Game.Window.ClientBounds.Width / 2, Game.Window.ClientBounds.Height / 2);
            }

            Vector3 movement = Vector3.Zero;
            if (ks.IsKeyDown(Keys.W))
                movement += Vector3.Forward;
            if (ks.IsKeyDown(Keys.S))
                movement += Vector3.Backward;
            if (ks.IsKeyDown(Keys.A))
                movement += Vector3.Left;
            if (ks.IsKeyDown(Keys.D))
                movement += Vector3.Right;

            if (movement != Vector3.Zero)
            {
                movement.Normalize();
            }




            Vector3 rotatedMov = RotateDirection(movement);
            if(!NoClip)
                rotatedMov.Y = 0;
            _position += rotatedMov * WalkSpeed;







            if (!NoClip)
            {

                //jump
                if (ks.IsKeyDown(Keys.Space) && velocity == 0 && !NoClip)
                {
                    velocity += 10;
                    _position.Y += 5f;
                }



                //gravity
                velocity += gravity;
                if (velocity < gravity * 32)
                    velocity = gravity * 32;

                _position.Y += velocity;

                //Collisions
                float tolerance = WalkSpeed * 2;


                Vector3[] points = new Vector3[] { Position, Position + Vector3.Down * height };


                collisions.TraceRay(Position, Position + Vector3.Forward * tolerance);
                if (collisions.outputFraction < 1.0f)
                    _position.Z += tolerance * (1 - collisions.outputFraction);

                collisions.TraceRay(Position, Position + Vector3.Backward * tolerance);
                if (collisions.outputFraction < 1.0f)
                    _position.Z -= tolerance * (1 - collisions.outputFraction);

                collisions.TraceRay(Position, Position + Vector3.Right * tolerance);
                if (collisions.outputFraction < 1.0f)
                    _position.X -= tolerance * (1 - collisions.outputFraction);

                collisions.TraceRay(Position, Position + Vector3.Left * tolerance);
                if (collisions.outputFraction < 1.0f)
                    _position.X += tolerance * (1 - collisions.outputFraction);


                collisions.TraceRay(Position, Position + Vector3.Up * tolerance);
                if (collisions.outputFraction < 1.0f)
                {
                    _position.Y -= tolerance * (1 - collisions.outputFraction);
                    velocity = -0.1f;
                }

                

                collisions.TraceRay(Position + Vector3.Down * height / 2, Position + Vector3.Down * height);
                if (collisions.outputFraction < 1.0f)
                {
                    _position.Y += height/2 * (1 - collisions.outputFraction);
                    velocity = 0;
                }
            }

            oldKs = ks;
            oldMs = ms;

        }

        public Vector3 RotateDirection(Vector3 pDirection)
        {
            Matrix rotation = Matrix.CreateRotationX(Rotation.Y) * Matrix.CreateRotationY(Rotation.X);
            Vector3 rotatedVector = Vector3.Transform(pDirection, rotation);
            return rotatedVector;
        }




    }
}
