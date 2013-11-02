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

    class BspRenderer : DrawableGameComponent, IBspRenderer
    {
        BspFile file;
        string path;




        public BspRenderer(Game pGame, string pPath)
            :base(pGame)
        {
            Game.Services.AddService(typeof(IBspRenderer), this);
            path = pPath;
        }

        public override void Initialize()
        {


            base.Initialize();
        }

        protected override void LoadContent()
        {
            effect = new BasicEffect(GraphicsDevice);

            file = new BspFile(path);

            CreateBuffers();
            CreateBeziers();

            base.LoadContent();
        }



        public override void Update(GameTime gameTime)
        {

            DetermineVisibleFaces();


            base.Update(gameTime);
        }


        private void DetermineVisibleFaces()
        {
            
            int leafIndex = findLeaf();
            int clusterIndex = file.Leaves[leafIndex].Cluster;

            //Visible leaves
            List<leaf> visibleLeaves = new List<leaf>();
            foreach (leaf l in file.Leaves)
            {
                //if(isLeafVisible(clusterIndex,l.Cluster))
                    visibleLeaves.Add(l);
            }

            //visible faces
            HashSet<int> usedIndices = new HashSet<int>();
            List<leafface> leafFaces = new List<leafface>();
            List<face> visibleFaces = new List<face>();
            foreach (leaf l in visibleLeaves)
            {
                for (int i = l.LeafFace; i <  l.LeafFace + l.N_LeafFaces; i++)
                {
                    if (!usedIndices.Contains(i))
                    {
                        leafface f = file.LeafFaces[i];
                        leafFaces.Add(f);
                        usedIndices.Add(i);
 
                    }

                }

            }

            //face

            foreach (leafface l in leafFaces)
            {
                face f = file.Faces[l.Face];
                visibleFaces.Add(f);
            }




            //arrays
            List<int> indices = new List<int>();
            foreach (face f in visibleFaces)
            {
                if (f.Type == 1 || f.Type == 3)
                {
                    //Meshes and faces
                    for (int i = f.Meshvert; i < f.Meshvert + f.N_Meshverts; i++)
                    {
                        //index
                        int index = f.Vertex + file.MeshVerts[i].Offset;
                        indices.Add(index);
                    }

                }


            }
            IBuffer = new IndexBuffer(Game.GraphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Count, BufferUsage.WriteOnly);
            if (indices.Count != 0)
                IBuffer.SetData(indices.ToArray());
        }

        VertexBuffer BeziersVertices;
        IndexBuffer BeziersIndex;
        private void CreateBeziers()
        {
            List<VertexPositionNormalTexture> vertices = new List<VertexPositionNormalTexture>();
            List<int> indices = new List<int>();
            int nbTotalPatchs = 0;
            foreach (face f in file.Faces)
            {
                if (f.Type == 2)
                {
                    int width = (f.Size[0] - 1) / 2;
                    int height = (f.Size[1] - 1) / 2;




                    vertex[,] patch = new vertex[f.Size[0], f.Size[1]];

                    int cpt = f.Vertex;
                    for (int x = 0; x < f.Size[0]; x++)
                    {
                        for (int y = 0; y < f.Size[1]; y++)
                        {
                            patch[x, y] = file.Vertices[cpt];
                            cpt++;
                        }
                    }


                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            int i = 2 * x;
                            int j = 2 * y;


                            vertex[] controls = new vertex[3 * 3];
                            for (int u = 0; u < 3; u++)
                            {
                                for (int v = 0; v < 3; v++)
                                {
                                    vertex vert = patch[v + i, u + j];
                                    controls[v * 3 +  u] = vert;
                                    vertices.Add(new VertexPositionNormalTexture(V3FromFloatArray(vert.Position), V3FromFloatArray(vert.Normal), new Vector2(vert.TexCoord[0, 0], vert.TexCoord[0, 1])));
                                }
                            }


                            //indices
                            int nb = width * height;
                            int offset = 9;

                            for (int n = 0; n < nb; n++)
                            {
                                for (int row = 0; row < 2; row++)
                                {
                                    for (int col = 0; col < 2; col++)
                                    {
                                        // 0, 0
                                        indices.Add(col + 3 * row + offset * (nbTotalPatchs));
                                        // 1, 0
                                        indices.Add(col + 1 + 3 * row + offset * (nbTotalPatchs));
                                        // 1, 1
                                        indices.Add(col  + 1 + 3 * (row + 1) + offset * (nbTotalPatchs));

                                        // 0, 0
                                        indices.Add(col + 3 * row + offset * (nbTotalPatchs));
                                        //1,1
                                        indices.Add(col + 1 + 3 * (row + 1) + offset * (nbTotalPatchs));
                                        //0,1
                                        indices.Add(col + 3 * (row + 1) + offset * (nbTotalPatchs));

                                    }
                                }
                                nbTotalPatchs++;
                            }

                           
                        }
                    }


                    
                }
                
            }
            if (vertices.Count > 0)
            {
                BeziersVertices = new VertexBuffer(GraphicsDevice, typeof(VertexPositionNormalTexture), vertices.Count, BufferUsage.WriteOnly);
                BeziersVertices.SetData(vertices.ToArray());

                BeziersIndex = new IndexBuffer(GraphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Count, BufferUsage.WriteOnly);
                BeziersIndex.SetData(indices.ToArray(), 0, indices.Count);
            }
        }




        VertexBuffer VBuffer;
        IndexBuffer IBuffer;
        private void CreateBuffers()
        {
            

            List<VertexPositionNormalTexture> vert = new List<VertexPositionNormalTexture>();

            HashSet<int> vertexID = new HashSet<int>();
            foreach (vertex v in file.Vertices)
            {
                Vector3 position = V3FromFloatArray(v.Position);
                Vector2 texcoord = new Vector2(v.TexCoord[0, 0], v.TexCoord[0, 1]);
                Vector3 normal = V3FromFloatArray(v.Normal);

                vert.Add(new VertexPositionNormalTexture(position, normal, texcoord));
            }


            VBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionNormalTexture), vert.Count, BufferUsage.WriteOnly);
            VBuffer.SetData(vert.ToArray());

        }

        private int findLeaf()
        {
            ICameraComponent camera = (ICameraComponent)Game.Services.GetService(typeof(ICameraComponent));
            int index = 0;

            while (index >= 0)
            {
                node node = file.Nodes[index];
                plane plane = file.Planes[node.Plane];

                //dist
                double distance = Vector3.Dot(V3FromFloatArray(plane.Normal), Swizzle(camera.Position)) - plane.Dist;

                if (distance >= 0)
                    index = node.Children[0];
                else
                    index = node.Children[node.Children.Count() - 1];

            }

            return -index - 1;

        }

        private Vector3 Swizzle(Vector3 pVector)
        {
            float x = pVector.X;
            float y = -pVector.Z;
            float z = pVector.Y;
            return new Vector3(x, y, z);
        }

        private bool isLeafVisible(int cluster, int testCluster)
        {
            if (cluster < 0 || file.VisData.Vecs == null)
                return true;

            int i = (cluster * file.VisData.Sz_vecs) + testCluster >> 3;
            byte visSet = file.VisData.Vecs[i];

            return (visSet & (1 << (testCluster & 7))) != 0;

        }

        private Vector3 V3FromFloatArray(float[] array)
        {
            if (array.Count() != 3)
                throw new InvalidOperationException();

            return new Vector3(array[0], array[2], -array[1]);
        }

      


        BasicEffect effect;
        public override void Draw(GameTime gameTime)
        {
            
            GraphicsDevice device = Game.GraphicsDevice;
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);


            RasterizerState rs = new RasterizerState();
            rs.CullMode = CullMode.None;
            rs.FillMode = FillMode.Solid;
            device.RasterizerState = rs;

            ICameraComponent camera = (ICameraComponent)Game.Services.GetService(typeof(ICameraComponent));
            effect.EnableDefaultLighting();
            effect.World = Matrix.Identity;
            effect.View = camera.ViewMatrix;
            effect.Projection = camera.ProjectionMatrix;
            effect.TextureEnabled = true;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                effect.Texture = Game.Content.Load<Texture2D>("textures/gothic_ceiling/stucco7top");
                pass.Apply();

                GraphicsDevice.SetVertexBuffer(VBuffer);
                device.Indices = IBuffer;
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, VBuffer.VertexCount, 0, IBuffer.IndexCount / 3);
                
                GraphicsDevice.SetVertexBuffer(BeziersVertices);

                if (BeziersVertices != null)
                {
                    device.Indices = BeziersIndex;
                    device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, BeziersVertices.VertexCount, 0, BeziersVertices.VertexCount);
                }
                
                //device.DrawPrimitives(PrimitiveType.TriangleList, 0, BeziersVertices.VertexCount );
            }
             
            base.Draw(gameTime);

            
        }



        /*
        public vertex CreatePatch3x3(vertex[] Control, int s1, int s2, int s3, float u, float v)
        {

            float var;

            int n;			//nth series

            float A, B, C;

            vertex[] c = new vertex[3];
            for (int i = 0; i < c.Length; i++)
            {
                c[i] = new vertex(new float[3], new float[2, 2], new float[3], new byte[4]);
            }


            vertex Final;
            // Get Sub Controls



            // Get Sub Controls (Get U)

            for (int f = 0; f < 3; f++)
            {

                c[f].TexCoord[1, 0] = 0;

                c[f].TexCoord[1, 1] = 0;

                c[f].Normal = c[f].Position = new float[] { 0, 0, 0 };

                c[f].TexCoord[0, 0] = 0;

                c[f].TexCoord[0, 1] = 0;

            }

            Final = c[0];

            for (int i = 0; i < 3; i++)
            {

                n = i + 1;

                // A = Binomial co-efficent

                A = (float)(PascalsTriangle(n, 3));

                // B=(1-t)^Terms-N

                B = (float)Math.Pow(1 - u, 3 - n);

                // C=t^n-1

                C = (float)Math.Pow(u, n - 1);



                var = A * B * C;



                // Get the Position

                for (int f = 0; f < 3; f++)
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





            for (int i = 0; i < 3; i++)
            {

                n = i + 1;

                // A = Binomial co-efficent

                A = (float)(PascalsTriangle(n, 3));

                // B=(1-t)^Terms-N

                B = (float)Math.Pow(1 - v, 3 - n);

                // C=t^n-1

                C = (float)Math.Pow(v, n - 1);



                var = A * B * C;



                // Get the Position

                Vector3 Finalposition = floatToV3Raw(Final.Position);
                Vector3 position = floatToV3Raw(c[i].Position) * var;
                Final.Position = V3ToFloatRaw(Finalposition + position);

                Final.TexCoord[1, 0] += (c[i].TexCoord[1, 0] * var);	// Add them all together

                Final.TexCoord[1, 1] += (c[i].TexCoord[1, 1] * var);	// Add them all together

                Final.TexCoord[0, 0] += (c[i].TexCoord[0, 0] * var);	// Add them all together

                Final.TexCoord[0, 1] += (c[i].TexCoord[0, 1] * var);	// Add them all together


                Vector3 Finalnormal = floatToV3Raw(Final.Normal);
                Vector3 normal = floatToV3Raw(c[i].Normal) * var;
                Final.Normal = V3ToFloatRaw(Finalnormal + normal);




            }

            return Final;

        }
        */
        

        private Vector3 floatToV3Raw(float[] floats)
        {
            return new Vector3(floats[0], floats[1], floats[2]);
        }
        private float[] V3ToFloatRaw(Vector3 vector)
        {
            return new float[] { vector.X, vector.Y, vector.Z };
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


    }
}
