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
    interface ICameraComponent
    {
        Vector3 Position { get; set; }
        Vector3 Target { get; }
        Vector3 UpVector { get; }

        Matrix ViewMatrix { get; }
        Matrix ProjectionMatrix { get;}
        Vector2 Rotation { get; set; }

        Vector3 RotatedTarget { get; }

        void Move(Vector3 pMovement, float pSpeed);
        Vector3 RotateDirectionWithoutY(Vector3 pDirection);
    }
}
