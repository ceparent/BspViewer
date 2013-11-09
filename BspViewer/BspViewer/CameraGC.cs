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
        static float FAR_PLANE = 5000.0f;

        Player player;
        public CameraGC(Game pGame)
            : base(pGame)
        {
            Game.Services.AddService(typeof(ICameraComponent), this);
        }

        public override void Initialize()
        {
            player = (Player)Game.Services.GetService(typeof(Player));

            player.Rotation = Vector2.Zero;
            
            Target = Vector3.Forward;
            UpVector = Vector3.Up;

            UpdateViewMatrix();
            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(VIEW_ANGLE, Game.GraphicsDevice.Viewport.AspectRatio, NEAR_PLANE, FAR_PLANE);

            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            UpdateViewMatrix();

            base.Update(gameTime);
        }

       




        public Vector3 RotatedTarget
        {
            get
            {
                Matrix rotation = Matrix.CreateRotationX(player.Rotation.Y) * Matrix.CreateRotationY(player.Rotation.X);
                return  Vector3.Transform(Target, rotation);
            }

        }

        private void UpdateViewMatrix()
        {


            ViewMatrix = Matrix.CreateLookAt(player.Position, player.Position + RotatedTarget, UpVector);
        }

        public void Move(Vector3 pMovement, float pSpeed)
        {
            Matrix rotation = Matrix.CreateRotationX(player.Rotation.Y) * Matrix.CreateRotationY(player.Rotation.X);
            Vector3 rotatedVector = Vector3.Transform(pMovement, rotation);
            player.Position += pSpeed * rotatedVector;
            
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


        public Vector3 RotateDirectionWithoutY(Vector3 pDirection)
        {
            Matrix rotation = Matrix.CreateRotationX(player.Rotation.Y) * Matrix.CreateRotationY(player.Rotation.X);
            Vector3 rotatedVector = Vector3.Transform(pDirection, rotation);
            rotatedVector.Y = 0;
            return rotatedVector;
        }




        public Vector3 Position
        {
            get
            {
                return player.Position;
            }
            set
            {
                player.Position = value;
            }
        }
    }
}
