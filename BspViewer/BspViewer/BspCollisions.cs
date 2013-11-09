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
    class BspCollisions
    {
        const int TT_RAY = 0;
        const int TT_SPHERE = 1;
        const int TT_BOX = 2;


        BspFile file;
        BspRenderer renderer;
        public BspCollisions(BspRenderer pRenderer)
        {
            renderer = pRenderer;
        }

        public float outputFraction;
        public Vector3 outputEnd;
        public bool outputStartsOut;
        public bool outputAllSolid;

        private Vector3 _inputStart;
        private Vector3 _inputEnd;

        public void Trace(Vector3 inputStart, Vector3 inputEnd)
        {
            _inputStart = inputStart;
            _inputEnd = inputEnd;
            file = renderer.BspFile;

            outputStartsOut = true;
            outputAllSolid = false;
            outputFraction = 1.0f;

            // walks in the bsp tree
            CheckNode(0, 0.0f, 1.0f, inputStart, inputEnd);

            if (outputFraction == 1.0f)
                outputEnd = inputEnd; // nothing blocked it
            else
                outputEnd = inputStart + outputFraction * (inputEnd - inputStart);

        }



        public void CheckNode(int nodeIndex, float startFraction, float endFraction, Vector3 start, Vector3 end)
        {

            if (nodeIndex < 0)
            {
                // this is a leaf

                leaf leaf = file.Leaves[-(nodeIndex + 1)];

                for (int i = 0; i < leaf.N_LeafBrushes; i++)
                {
                    brush brush = file.Brushes[file.LeafBrushes[leaf.LeafBrush + i].Brush];

                    if (brush.N_BrushSides > 0 && (file.Textures[brush.Texture].Contents & 1) != 0)
                    {
                        CheckBrush(brush);
                    }

                }
                return;
            }




            node node = file.Nodes[nodeIndex];
            plane plane = file.Planes[node.Plane];

            Vector3 planeNormal = BspRenderer.V3FromFloatArray(plane.Normal);
            float startDistance = Vector3.Dot(start, planeNormal) - plane.Dist;
            float endDistance = Vector3.Dot(end, planeNormal) - plane.Dist;


            float offset = 0;
            if (traceType == TT_RAY)
                offset = 0;
            else if (traceType == TT_SPHERE)
                offset = traceRadius;
            else if (traceType == TT_BOX)
                offset = (float)(Math.Abs(traceExtents.X * planeNormal.X) + Math.Abs(traceExtents.Y * planeNormal.Y) + Math.Abs(traceExtents.Z * planeNormal.Z));


            // A
            if (startDistance >= offset && endDistance >= offset)
            {
                // in front of the plane
                // checking front child
                CheckNode(node.Children[0], startFraction, endFraction, start, end);

            }// B
            else if (startDistance < -offset && endDistance < -offset)
            {
                // behind the plane
                // checking back child
                CheckNode(node.Children[1], startFraction, endFraction, start, end);
            }// C
            else
            {
                // splitting between planes
                int side;
                float fraction1, fraction2, middleFraction;
                Vector3 middle;

                // split the segment in 2
                if (startDistance < endDistance)
                {
                    side = 1; // back
                    float inverseDistance = 1.0f / (startDistance - endDistance);
                    fraction1 = (startDistance - offset + float.Epsilon) * inverseDistance;
                    fraction2 = (startDistance - offset + float.Epsilon) * inverseDistance;

                }
                else if (endDistance < startDistance)
                {
                    side = 0; // front
                    float inverseDistance = 1.0f / (startDistance - endDistance);
                    fraction1 = (startDistance + offset + float.Epsilon) * inverseDistance;
                    fraction2 = (startDistance - offset - float.Epsilon) * inverseDistance;
                }
                else
                {
                    side = 0; //front
                    fraction1 = 1.0f;
                    fraction2 = 0.0f;
                }

                // make sure numbers are valid

                if (fraction1 < 0.0f)
                    fraction1 = 0.0f;
                else if (fraction1 > 1.0f)
                    fraction1 = 1.0f;
                if (fraction2 < 0.0f)
                    fraction2 = 0.0f;
                else if (fraction2 > 1.0f)
                    fraction2 = 1.0f;

                // calculate middle point for the first side

                middleFraction = startFraction + (endFraction - startFraction) * fraction1;
                middle = start + fraction1 * (end - start);

                // check first side

                CheckNode(node.Children[side], startFraction, middleFraction, start, middle);


                // calculate middle point for the second side

                middleFraction = startFraction + (endFraction - startFraction) * fraction2;
                middle = start * fraction2 * (end - start);


                // check second side
                CheckNode(node.Children[inverseSide(side)], startFraction, middleFraction, start, middle);
                



            }
        }

        private int inverseSide(int side)
        {
            if (side == 0)
                return 1;
            else
                return 0;
        }

        private void CheckBrush(brush pBrush)
        {
            float startFraction = -1.0f;
            float endFraction = 1.0f;
            bool startsout = false;
            bool endsout = false;

            for (int i = 0; i < pBrush.N_BrushSides; i++)
            {
                brushside brushside = file.BrusheSides[pBrush.BrushSide + i];
                plane plane = file.Planes[brushside.Plane];
                Vector3 planeNormal = BspRenderer.V3FromFloatArray(plane.Normal);

                float startDistance = 0;
                float endDistance = 0;

                if(traceType == TT_RAY)
                {
                    startDistance = Vector3.Dot(_inputStart, planeNormal) - plane.Dist;
                    endDistance = Vector3.Dot(_inputEnd, planeNormal) - plane.Dist;
                }
                else if (traceType == TT_SPHERE)
                {
                    startDistance = Vector3.Dot(_inputStart, planeNormal) - (plane.Dist + traceRadius);
                    endDistance = Vector3.Dot(_inputEnd, planeNormal) - (plane.Dist + traceRadius);
                }
                else if (traceType == TT_BOX)
                {
                    Vector3 offset = Vector3.Zero;

                    if (planeNormal.X < 0)
                        offset.X = traceMaxs.X;
                    else
                        offset.X = traceMins.X;

                    if (planeNormal.Y < 0)
                        offset.Y = traceMaxs.Y;
                    else
                        offset.Y = traceMins.Y;

                    if (planeNormal.Z < 0)
                        offset.Z = traceMaxs.Z;
                    else
                        offset.Z = traceMins.Z;

                    startDistance = (_inputStart.X + offset.X) * planeNormal.X + (_inputStart.Y + offset.Y) * planeNormal.Y + (_inputStart.Z + offset.Z) * planeNormal.Z - plane.Dist;
                    endDistance = (_inputEnd.X + offset.X) * planeNormal.X + (_inputEnd.Y + offset.Y) * planeNormal.Y + (_inputEnd.Z + offset.Z) * planeNormal.Z - plane.Dist;

                }


                if (startDistance > 0)
                    startsout = true;
                if (endDistance > 0)
                    endsout = true;

                // make sure the trace isnt on one side of the brush only

                if (startDistance > 0 && endDistance > 0)
                {
                    // both in front of the plane
                    return;
                }
                if (startDistance <= 0 && endDistance <= 0)
                {
                    // both are behind the plane, it will get clipped by an other one
                    continue;
                }


                if (startDistance > endDistance)
                {
                    // line is in the brush
                    float fraction = (startDistance - float.Epsilon) / (startDistance - endDistance);
                    if (fraction > startFraction)
                        startFraction = fraction;
                }
                else
                {
                    //line is leaving the brush
                    float fraction = (startDistance + float.Epsilon) / (startDistance - endDistance);
                    if (fraction < endFraction)
                        endFraction = fraction;
                }
            }

            if (startsout == false)
            {
                outputStartsOut = false;
                if (endsout)
                    outputAllSolid = true;
                return;
            }

            if (startFraction < endFraction)
            {
                if (startFraction > -1 && startFraction < outputFraction)
                {
                    if(startFraction < 0)
                        startFraction = 0;
                    outputFraction = startFraction;
                }

            }
        }

        int traceType;
        float traceRadius;
        Vector3 traceMins;
        Vector3 traceMaxs;
        Vector3 traceExtents;
        public void TraceRay(Vector3 inputStart, Vector3 inputEnd)
        {
            traceType = TT_RAY;
            Trace(inputStart, inputEnd);
        }

        public void TraceSphere(Vector3 inputStart, Vector3 inputEnd, float inputRadius)
        {
            traceType = TT_SPHERE;
            traceRadius = inputRadius;
            Trace(inputStart, inputEnd);
        }

        public void TraceBox(Vector3 inputStart, Vector3 inputEnd, Vector3 inputMins, Vector3 inputMaxs)
        {
            if (inputMaxs == Vector3.Zero && inputMins == Vector3.Zero)
            {
                TraceRay(inputStart, inputEnd);
            }
            else
            {
                traceType = TT_BOX;
                traceMins = inputMins;
                traceMaxs = inputMaxs;

                traceExtents.X = -traceMins.X > traceMaxs.X ? -traceMins.X : traceMaxs.X;
                traceExtents.Y = -traceMins.Y > traceMaxs.Y ? -traceMins.Y : traceMaxs.Y;
                traceExtents.Z = -traceMins.Z > traceMaxs.Z ? -traceMins.Z : traceMaxs.Z;

                Trace(inputStart, inputEnd);

            }

        }

    }
}
