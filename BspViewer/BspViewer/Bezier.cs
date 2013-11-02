using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BspViewer
{
    public class Bezier
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Bezier"/> class.
        /// </summary>
        /// <param name="graphics">The graphics.</param>
        public Bezier()
        {
            vertices = new List<VertexPositionNormalTexture>();
            indexes = new List<ushort>();
        }


        /// <summary>
        /// Queries the index of the current vertex. This starts at
        /// zero, and increments every time AddVertex is called.
        /// </summary>
        private int CurrentVertex
        {
            get { return vertices.Count; }
        }

        List<ushort> indexes;
        /// <summary>
        /// Creates indices for a patch that is tessellated at the specified level.
        /// </summary>
        public void CreatePatchIndices(int tessellation, bool isMirrored)
        {
            int stride = tessellation + 1;

            for (int i = 0; i < tessellation; ++i)
            {
                for (int j = 0; j < tessellation; j++)
                {
                    // Make a list of six index values (two triangles).
                    int[] indices =
                    {
                        i * stride + j,
                        (i + 1) * stride + j,
                        (i + 1) * stride + j + 1,

                        i * stride + j,
                        (i + 1) * stride + j + 1,
                        i * stride + j + 1,
                    };

                    // If this patch is mirrored, reverse the
                    // indices to keep the correct winding order.
                    if (isMirrored)
                    {
                        Array.Reverse(indices);
                    }

                    // Create the indices.
                    foreach (int index in indices)
                    {
                        indexes.Add((ushort)(CurrentVertex + index));
                    }
                }
            }
        }



        private int PascalsTriangle(int n, int x)
        {
            int[,] pascal = new int[n, n];
            pascal[0, 0] = 1;
            for (int row = 0; row < n; row++)
            {
                for (int col = 0; col < row; col++)
                {
                    int somme = -1;
                    if (col == 0 || col == row - 1)
                        somme = 1;
                    else
                        somme = pascal[row - 1, col - 1] + pascal[row - 1, col];

                    if (row == n - 1 && col == x - 1)
                        return somme;

                    pascal[row, col] = somme;
                }
            }


            return -1;
        }

          public vertex CreatePatch3x3(vertex[] Control,int s1,int s2,int s3,float u,float v)
          {

            float var;

            int n;			//nth series

            float A,B,C;

            vertex[] c = new vertex[3];
            vertex Final;		// Get Sub Controls



            // Get Sub Controls (Get U)

            for (int f=0;f	<	3;f++)
            {

                c[f].TexCoord[1,0] = 0;

                c[f].TexCoord[1,1] = 0;

                c[f].Normal = c[f].Position = new float[] { 0,0 };

                c[f].TexCoord[0,0] = 0;

                c[f].TexCoord[0,1] = 0;

            }

            Final = c[0];

            for (int i=0;i	<	3;	i++)
            {

                n = i+1;

                // A = Binomial co-efficent

                A = (float)(PascalsTriangle(n,3));

                // B=(1-t)^Terms-N

                B = (float)Math.Pow(1-u,3-n);

                // C=t^n-1

                C = (float)Math.Pow(u,n-1);



                var =A * B * C;



                // Get the Position

                for (int f=0;f	<	3;f++)

                {

                    int s = 0;

                    if (f == 0) s = s1;

                    if (f == 1) s = s2;

                    if (f == 2) s = s3;


                    Vector3 Finalposition = floatToV3Raw(c[f].Position);
                    Vector3 position = floatToV3Raw(Control[i + s].Position) * var;
                    c[f].Position = V3ToFloatRaw(Finalposition + position);


                    c[f].TexCoord[1, 0] += (Control[i + s].TexCoord[1, 0] * var);	// Add them all together

                    c[f].TexCoord[1, 1] += (Control[i + s].TexCoord[1, 1] * var);	// Add them all together

                    c[f].TexCoord[0, 0] += (Control[i + s].TexCoord[0, 0] * var);	// Add them all together

                    c[f].TexCoord[0, 1] += (Control[i + s].TexCoord[0, 1] * var);	// Add them all together

                    Vector3 Finalnormal = floatToV3Raw(c[f].Normal);
                    Vector3 normal = floatToV3Raw(Control[i + s].Normal) * var;
                    c[f].Normal = V3ToFloatRaw(Finalnormal + normal);

                }

            }





	        for (int i=0;i	<	3;	i++)

	        {

		        n = i+1;

		        // A = Binomial co-efficent

		        A = (float)(PascalsTriangle(n,3));

		        // B=(1-t)^Terms-N

		        B = (float)Math.Pow(1-v,3-n);

		        // C=t^n-1

		        C = (float)Math.Pow(v,n-1);



		        var =A * B * C;



		        // Get the Position

                Vector3 Finalposition = floatToV3Raw(Final.Position);
                Vector3 position = floatToV3Raw(c[i].Position) * var;
                Final.Position = V3ToFloatRaw(Finalposition + position);

		        Final.TexCoord[1,0]	+= (c[i].TexCoord[1,0] *var);	// Add them all together

		        Final.TexCoord[1,1]	+=	(c[i].TexCoord[1,1] *var);	// Add them all together

		        Final.TexCoord[0,0]	+=  (c[i].TexCoord[0,0] *var);	// Add them all together

		        Final.TexCoord[0,1]	+=	(c[i].TexCoord[0,1] *var);	// Add them all together


                Vector3 Finalnormal = floatToV3Raw(Final.Normal);
                Vector3 normal = floatToV3Raw(c[i].Normal) * var;
                Final.Normal = V3ToFloatRaw(Finalnormal + normal);




	        }

	        return Final;

        }

          private Vector3 floatToV3Raw(float[] floats)
          {
              return new Vector3(floats[0], floats[1], floats[2]);
          }
          private float[] V3ToFloatRaw(Vector3 vector)
          {
              return new float[] { vector.X, vector.Y, vector.Z };
          }










        /// <summary>
        /// Creates vertices for a patch that is tessellated at the specified level.
        /// </summary>
        public void CreatePatchVertices(Vector3[] patch, int tessellation, bool isMirrored)
        {
            Debug.Assert(patch.Length == 16);

            for (int i = 0; i <= tessellation; ++i)
            {
                float ti = (float)i / tessellation;

                for (int j = 0; j <= tessellation; j++)
                {
                    float tj = (float)j / tessellation;

                    // Perform four horizontal bezier interpolations
                    // between the control points of this patch.
                    Vector3 p1 = BezierInterpolate(patch[0], patch[1], patch[2], patch[3], ti);
                    Vector3 p2 = BezierInterpolate(patch[4], patch[5], patch[6], patch[7], ti);
                    Vector3 p3 = BezierInterpolate(patch[8], patch[9], patch[10], patch[11], ti);
                    Vector3 p4 = BezierInterpolate(patch[12], patch[13], patch[14], patch[15], ti);

                    // Perform a vertical interpolation between the results of the
                    // previous horizontal interpolations, to compute the position.
                    Vector3 position = BezierInterpolate(p1, p2, p3, p4, tj);

                    // Perform another four bezier interpolations between the control
                    // points, but this time vertically rather than horizontally.
                    Vector3 q1 = BezierInterpolate(patch[0], patch[4], patch[8], patch[12], tj);
                    Vector3 q2 = BezierInterpolate(patch[1], patch[5], patch[9], patch[13], tj);
                    Vector3 q3 = BezierInterpolate(patch[2], patch[6], patch[10], patch[14], tj);
                    Vector3 q4 = BezierInterpolate(patch[3], patch[7], patch[11], patch[15], tj);

                    // Compute vertical and horizontal tangent vectors.
                    Vector3 tangentA = BezierTangent(p1, p2, p3, p4, tj);
                    Vector3 tangentB = BezierTangent(q1, q2, q3, q4, ti);

                    // Cross the two tangent vectors to compute the normal.
                    Vector3 normal = Vector3.Cross(tangentA, tangentB);

                    if (normal.Length() > 0.0001f)
                    {
                        normal.Normalize();

                        // If this patch is mirrored, we must invert the normal.
                        if (isMirrored)
                            normal = -normal;
                    }
                    else
                    {
                        // In a tidy and well constructed bezier patch, the preceding
                        // normal computation will always work. But the classic teapot
                        // model is not tidy or well constructed! At the top and bottom
                        // of the teapot, it contains degenerate geometry where a patch
                        // has several control points in the same place, which causes
                        // the tangent computation to fail and produce a zero normal.
                        // We 'fix' these cases by just hard-coding a normal that points
                        // either straight up or straight down, depending on whether we
                        // are on the top or bottom of the teapot. This is not a robust
                        // solution for all possible degenerate bezier patches, but hey,
                        // it's good enough to make the teapot work correctly!

                        if (position.Y > 0)
                            normal = Vector3.Up;
                        else
                            normal = Vector3.Down;
                    }

                    // Create the vertex.
                    AddVertex(position, normal);
                }
            }
        }


        /// <summary>
        /// Performs a cubic bezier interpolation between four scalar control
        /// points, returning the value at the specified time (t ranges 0 to 1).
        /// </summary>
        static float BezierInterpolate(float p1, float p2, float p3, float p4, float t)
        {
            return p1 * (1 - t) * (1 - t) * (1 - t) +
                   p2 * 3 * t * (1 - t) * (1 - t) +
                   p3 * 3 * t * t * (1 - t) +
                   p4 * t * t * t;
        }


        /// <summary>
        /// Performs a cubic bezier interpolation between four Vector3 control
        /// points, returning the value at the specified time (t ranges 0 to 1).
        /// </summary>
        static Vector3 BezierInterpolate(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, float t)
        {
            Vector3 result = new Vector3();

            result.X = BezierInterpolate(p1.X, p2.X, p3.X, p4.X, t);
            result.Y = BezierInterpolate(p1.Y, p2.Y, p3.Y, p4.Y, t);
            result.Z = BezierInterpolate(p1.Z, p2.Z, p3.Z, p4.Z, t);

            return result;
        }


        /// <summary>
        /// Computes the tangent of a cubic bezier curve at the specified time,
        /// when given four scalar control points.
        /// </summary>
        static float BezierTangent(float p1, float p2, float p3, float p4, float t)
        {
            return p1 * (-1 + 2 * t - t * t) +
                   p2 * (1 - 4 * t + 3 * t * t) +
                   p3 * (2 * t - 3 * t * t) +
                   p4 * (t * t);
        }


        /// <summary>
        /// Computes the tangent of a cubic bezier curve at the specified time,
        /// when given four Vector3 control points. This is used for calculating
        /// normals (by crossing the horizontal and vertical tangent vectors).
        /// </summary>
        static Vector3 BezierTangent(Vector3 p1, Vector3 p2,
                                     Vector3 p3, Vector3 p4, float t)
        {
            Vector3 result = new Vector3();

            result.X = BezierTangent(p1.X, p2.X, p3.X, p4.X, t);
            result.Y = BezierTangent(p1.Y, p2.Y, p3.Y, p4.Y, t);
            result.Z = BezierTangent(p1.Z, p2.Z, p3.Z, p4.Z, t);

            result.Normalize();

            return result;
        }

        List<VertexPositionNormalTexture> vertices;
        private void AddVertex(Vector3 position, Vector3 normal)
        {
            VertexPositionNormalTexture vertex =new VertexPositionNormalTexture()
            {
                Position = position,
                Normal = normal,
                TextureCoordinate = new Vector2((float)(Math.Asin(normal.X) / MathHelper.Pi + 0.5),
                                                (float)(Math.Asin(normal.X) / MathHelper.Pi + 0.5)),
            };

            vertices.Add(vertex);
        }
    }
}
