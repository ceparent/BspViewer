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
    class CameraGC : GameComponent, ICameraComponent
    {

        static float VIEW_ANGLE = MathHelper.PiOver4;
        static float NEAR_PLANE = 0.1f;
        static float FAR_PLANE = 1500.0f;

        public CameraGC(Game pGame)
            : base(pGame)
        {
            Game.Services.AddService(typeof(ICameraComponent), this);
        }

        public override void Initialize()
        {
            Rotation = Vector2.Zero;
            
            Target = Vector3.Forward;
            UpVector = Vector3.Up;

            UpdateViewMatrix();
            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(VIEW_ANGLE, Game.GraphicsDevice.Viewport.AspectRatio, NEAR_PLANE, FAR_PLANE);

            Mouse.SetPosition(Game.Window.ClientBounds.Width / 2, Game.Window.ClientBounds.Height / 2);
            originalMouse = Mouse.GetState();

            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            UpdateViewMatrix();

            Game1 CastedGame = (Game1)Game;
            MoveAround();

            base.Update(gameTime);
        }

        MouseState originalMouse;
        float CameraSpeed = 0.003f;
        float WalkSpeed = 2f;
        private void MoveAround()
        {
            KeyboardState ks = Keyboard.GetState();
            MouseState ms = Mouse.GetState();
            

            
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
                movement.Normalize();


            Vector3 rotatedMov = RotateDirection(movement);
            _position += rotatedMov * WalkSpeed;

        }




        public Vector3 RotatedTarget
        {
            get
            {
                Matrix rotation = Matrix.CreateRotationX(Rotation.Y) * Matrix.CreateRotationY(Rotation.X);
                return  Vector3.Transform(Target, rotation);
            }

        }

        private void UpdateViewMatrix()
        {


            ViewMatrix = Matrix.CreateLookAt(Position, Position + RotatedTarget, UpVector);
        }

        public void Move(Vector3 pMovement, float pSpeed)
        {
            Matrix rotation = Matrix.CreateRotationX(Rotation.Y) * Matrix.CreateRotationY(Rotation.X);
            Vector3 rotatedVector = Vector3.Transform(pMovement, rotation);
            Position += pSpeed * rotatedVector;
            
        }

        private Vector3 _position;
        public Vector3 Position
        {
            get { return _position; }
            set { _position = value; }
        }

        private Vector3 _target;
        public Vector3 Target
        {
            get { return _target; }
            set { _target = value; }
        }

        private Vector3 _upVector;
        public Vector3 UpVector
        {
            get { return _upVector; }
            set { _upVector = value; }
        }

        private Matrix _viewMatrix;
        public Matrix ViewMatrix
        {
            get { return _viewMatrix; }
            set { _viewMatrix = value; }
        }

        private Matrix _projectionMatrix;
        public Matrix ProjectionMatrix
        {
            get { return _projectionMatrix; }
            set { _projectionMatrix = value; }
        }

        Vector2 _rotation;
        public Vector2 Rotation
        {
            get { return _rotation; }
            set {
                _rotation = value;
                if (_rotation.Y <= -MathHelper.PiOver2)
                    _rotation.Y =  -MathHelper.PiOver2 + 0.0001f;
                if (_rotation.Y >= MathHelper.PiOver2)
                    _rotation.Y = MathHelper.PiOver2 - 0.0001f;
            }
        }


        public Vector3 RotateDirection(Vector3 pDirection)
        {
            Matrix rotation = Matrix.CreateRotationX(Rotation.Y) * Matrix.CreateRotationY(Rotation.X);
            Vector3 rotatedVector = Vector3.Transform(pDirection, rotation);
            return rotatedVector;
        }

        public Vector3 RotateDirectionWithoutY(Vector3 pDirection)
        {
            Matrix rotation = Matrix.CreateRotationX(Rotation.Y) * Matrix.CreateRotationY(Rotation.X);
            Vector3 rotatedVector = Vector3.Transform(pDirection, rotation);
            rotatedVector.Y = 0;
            return rotatedVector;
        }



    }
}
