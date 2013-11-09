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
using System.IO;

namespace BspViewer
{

    class BspRenderer : DrawableGameComponent, IBspRenderer
    {
        BspFile file;
        public BspFile BspFile
        {
            get { return file; }
        }
        private string _path;
        public string Path
        {
            get { return _path; }
            set 
            {
                _path = value;
                file = new BspFile(Path);
                Generate();
            }
        }

        private bool _wireFramed = false;
        public bool WireFrame
        {
            get { return _wireFramed; }
            set { _wireFramed = value; }
        }


        public BspRenderer(Game pGame, string pPath)
            :base(pGame)
        {
            Game.Services.AddService(typeof(IBspRenderer), this);
            _path = pPath;
        }

        public override void Initialize()
        {
            file = new BspFile(Path);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            effect = new BasicEffect(GraphicsDevice);
            base.LoadContent();

            _tesselation = 5;
            Generate();
            
        }

        private void Generate()
        {
            CreateBuffers();
            CreateBeziersWithTesselation();
            LoadTextures();
        }
        Dictionary<string, Texture2D> _textures;
        private void LoadTextures()
        {
            StreamWriter writer = new StreamWriter("missing.txt");

            _textures = new Dictionary<string, Texture2D>();
            foreach (texture t in file.Textures)
            {
                if (t.Name.Length > 9)
                {
                    string textureName = "textures/Quake3/" + t.Name.Substring(9);
                    if (File.Exists(Game.Content.RootDirectory + "/" + textureName + ".xnb"))
                    {
                        _textures.Add(textureName, Game.Content.Load<Texture2D>(textureName));
                    }
                    else
                    {
                        writer.WriteLine(textureName);
                    }
                }
            }

            writer.Close();

        }




        public override void Update(GameTime gameTime)
        {

            DetermineVisibleFaces();

            base.Update(gameTime);
        }

        int[] textureLengths;
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



            List<int>[] Texturearrays = new List<int>[file.Textures.Count()];
            for (int i = 0; i < file.Textures.Count(); i++)
            {
                Texturearrays[i] = new List<int>();
            }

            //arrays
            foreach (face f in visibleFaces)
            {
                if (f.Type == 1 || f.Type == 3)
                {
                    //Meshes and faces
                    for (int i = f.Meshvert; i < f.Meshvert + f.N_Meshverts; i++)
                    {
                        //index
                        int index = f.Vertex + file.MeshVerts[i].Offset;
                        Texturearrays[f.Texture].Add(index);
                    }

                }


            }

            List<int> realList = new List<int>();
            textureLengths = new int[file.Textures.Count()];
            int cpt = 0;
            for (int i = 0; i < Texturearrays.Length; i++)
            {
                realList.AddRange(Texturearrays[i]);
                cpt += Texturearrays[i].Count;
                textureLengths[i] = cpt;
            }

            IBuffer = new IndexBuffer(Game.GraphicsDevice, IndexElementSize.ThirtyTwoBits, realList.Count, BufferUsage.WriteOnly);
            IBuffer.SetData(realList.ToArray());



        }

        VertexBuffer BeziersVertices;
        IndexBuffer BeziersIndex;
        int[] BezierstextureLengths;
        private void CreateBeziersWithTesselation()
        {
            List<int> bezierTextureIds = new List<int>();
            List<vertex[]> controlList = new List<vertex[]>();
            List<int> textureIds = new List<int>();
            foreach (face f in file.Faces)
            {
                if (f.Type == 2)
                {
                    int width = (f.Size[0] - 1) / 2;
                    int height = (f.Size[1] - 1) / 2;

                    bezierTextureIds.Add(f.Texture);

                    vertex[,] patch = new vertex[f.Size[0], f.Size[1]];

                    int cpt = f.Vertex;
                    for (int y = 0; y < f.Size[1]; y++)
                    {
                        for (int x = 0; x < f.Size[0]; x++)
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
                                    vertex vert = patch[u + i, v + j];
                                    controls[u * 3 + v] = vert;
                                    
                                }
                            }
                            controlList.Add(controls);
                            textureIds.Add(f.Texture);
                        }
                    }


                }

            }


            List<vertex> bspVertices;

            bspVertices = new List<vertex>();



            List<int>[] Texturearrays = new List<int>[file.Textures.Count()];
            for (int i = 0; i < file.Textures.Count(); i++)
            {
                Texturearrays[i] = new List<int>();
            }

            TesselationOffset = 0;
            for (int i = 0; i < controlList.Count; i++)
            {
                Texturearrays[textureIds[i]].AddRange(TesselateOnePatch(bspVertices, controlList[i]));
            }


            List<int> realList = new List<int>();
            BezierstextureLengths = new int[file.Textures.Count()];
            int cpt3 = 0;
            for (int i = 0; i < Texturearrays.Length; i++)
            {
                realList.AddRange(Texturearrays[i]);
                cpt3 += Texturearrays[i].Count;
                BezierstextureLengths[i] = cpt3;
            }


            List<VertexPositionNormalTexture> verticesList = new List<VertexPositionNormalTexture>();
            int cpt2 = 0;
            foreach (vertex vert in bspVertices)
            {
                Vector3 normal = V3FromFloatArray(vert.Normal);
                normal.Normalize();

                verticesList.Add(new VertexPositionNormalTexture(V3FromFloatArray(vert.Position), normal, new Vector2(vert.TexCoord[0, 0], vert.TexCoord[0, 1])));
                cpt2++;
            }
            if (bspVertices.Count > 0)
            {
                BeziersVertices = new VertexBuffer(GraphicsDevice, typeof(VertexPositionNormalTexture), bspVertices.Count, BufferUsage.WriteOnly);
                BeziersVertices.SetData(verticesList.ToArray());

                BeziersIndex = new IndexBuffer(GraphicsDevice, IndexElementSize.ThirtyTwoBits, realList.Count, BufferUsage.WriteOnly);
                BeziersIndex.SetData(realList.ToArray(), 0, realList.Count);
            }
            else
            {
                BeziersIndex = null;
                BeziersVertices = null;
            }
        }

        private int _tesselation;
        public int Tesselation
        {
            get { return _tesselation; }
            set
            {
                _tesselation = value;
                if (_tesselation < 1)
                    _tesselation = 1;
                if (_tesselation > 10)
                    _tesselation = 10;

                CreateBeziersWithTesselation();
            }
        }
        private int TesselationOffset = 0;
        private List<int> TesselateOnePatch(List<vertex> pVerticesList, vertex[] pControls)
        {
            int Length = Tesselation + 1;

            vertex[,] newControls = new vertex[3, Length];

            for (int i = 0; i < 3; i++)
            {

                vertex p0 = pControls[i * 3];
                vertex p1 = pControls[(i  * 3) + 1];
                vertex p2 = pControls[(i * 3) + 2];

                for (int l = 0; l < Length; l++)
                {
                    double a = l / (double)Tesselation;
                    double b = 1 - a;

                    newControls[i, l] = p0 * b * b + p1 * 2 * b * a  + p2 * a * a;
                }
            }

            vertex[] vertices = new vertex[Length * Length];

            for (int x = 0; x < Length; x++)
            {
                vertex p0 = newControls[0, x];
                vertex p1 = newControls[1, x];
                vertex p2 = newControls[2, x];

                double c = x / (double)Tesselation; // texcoord

                for (int y = 0; y < Length; y++)
                {
                    
                    double a = y / (double)Tesselation;
                    double b = 1 - a;

                    //2nd pass
                    vertices[y +x  * Length] = p0 * b * b + p1 * 2 * b * a + p2 * a * a;
                    vertices[y + x * Length].TexCoord = new float[,] { { p0.TexCoord[0,0] + (float)a, p0.TexCoord[0,0] +(float)c }, { p0.TexCoord[0,1] +(float)a,  p0.TexCoord[0,1] +(float)c } };

                }
            }

            List<int> indices = new List<int>();
            int offset = Length * Length;

            for (int row = 0; row < Length - 1; row++)
            {
                for (int col = 0; col < Length - 1; col++)
                {
                    // 0, 0
                    indices.Add(col + (Length * row) + (TesselationOffset * offset));
                    // 1, 1
                    indices.Add((col + 1) + (Length * (row + 1)) + (TesselationOffset * offset));
                    // 1, 0
                    indices.Add((col + 1) + (Length * row) + (TesselationOffset * offset));


                    // 0, 0
                    indices.Add(col + (Length * row) + (TesselationOffset * offset));
                    // 0, 1
                    indices.Add(col + (Length * (row + 1)) + (TesselationOffset * offset));
                    // 1, 1
                    indices.Add((col + 1) + (Length * (row + 1)) + (TesselationOffset * offset));


                }
            }
            
            

            TesselationOffset++;
            pVerticesList.AddRange(vertices);
            return indices;

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

        public static Vector3 V3FromFloatArray(float[] array)
        {
            if (array.Count() != 3)
                throw new InvalidOperationException();

            return new Vector3(array[0], array[2], -array[1]);
        }

      


        BasicEffect effect;
        public override void Draw(GameTime gameTime)
        {

            GraphicsDevice device = Game.GraphicsDevice;


            RasterizerState rs = new RasterizerState();
            rs.CullMode = CullMode.CullCounterClockwiseFace;

            if (!WireFrame)
            {
                rs.FillMode = FillMode.Solid;
                effect.EnableDefaultLighting();
            }
            else
            {
                rs.FillMode = FillMode.WireFrame;
                effect.LightingEnabled = false;
            }

            device.RasterizerState = rs;

            ICameraComponent camera = (ICameraComponent)Game.Services.GetService(typeof(ICameraComponent));
            effect.World = Matrix.Identity;
            effect.View = camera.ViewMatrix;
            effect.Projection = camera.ProjectionMatrix;
            effect.TextureEnabled = true;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {

                GraphicsDevice.SetVertexBuffer(VBuffer);

                int lastOffset = 0;
                for (int i = 0; i < textureLengths.Count() ; i++)
                {
                    if ((textureLengths[i] - lastOffset) > 0)
                    {
                        string textureName = "textures/Quake3/" + file.Textures[i].Name.Substring(9);
                        if(_textures.ContainsKey(textureName))
                        {
                            effect.Texture = _textures[textureName];
                        }
                        else
                        {
                            effect.Texture = Game.Content.Load<Texture2D>("textures/devgrid");
                        }

                        pass.Apply();
                        device.Indices = IBuffer;
                        device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, IBuffer.IndexCount, lastOffset, (textureLengths[i] - lastOffset) / 3);

                        lastOffset = textureLengths[i];
                    }
                }

                


                
                GraphicsDevice.SetVertexBuffer(BeziersVertices);

                if (BeziersVertices != null)
                {

                    lastOffset = 0;
                    for (int i = 0; i < BezierstextureLengths.Count(); i++)
                    {
                        if ((BezierstextureLengths[i] - lastOffset) > 0)
                        {
                            string textureName = "textures/Quake3/" + file.Textures[i].Name.Substring(9);
                            if (_textures.ContainsKey(textureName))
                            {
                                effect.Texture = _textures[textureName];
                            }
                            else
                            {
                                effect.Texture = Game.Content.Load<Texture2D>("textures/devgrid");
                            }

                            pass.Apply();
                            device.Indices = BeziersIndex;
                            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, BeziersIndex.IndexCount, lastOffset, (BezierstextureLengths[i] - lastOffset) / 3);

                            lastOffset = BezierstextureLengths[i];
                        }
                    }


                }


            }
             
            base.Draw(gameTime);

            
        } 

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
