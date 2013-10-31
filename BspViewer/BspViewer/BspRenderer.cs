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


        public int NbVertices
        {
            get { return file.Vertices.Count(); }
        }
        public int NbIndices
        {
            get { return file.MeshVerts.Count(); }
        }
        public int NbFaces
        {
            get { return file.Faces.Count(); }
        }
        public int NbTextures
        {
            get { return file.Textures.Count(); }
        }
        public int Lightmaps
        {
            get { return file.Lightmaps.Count(); }
        }



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
                if(isLeafVisible(clusterIndex,l.Cluster))
                    visibleLeaves.Add(l);
            }

            //visible faces
            HashSet<int> usedIndices = new HashSet<int>();
            List<face> visibleFaces = new List<face>();
            foreach (leaf l in visibleLeaves)
            {
                for (int i = 0; i < l.N_LeafFaces; i++)
                {
                    if (!usedIndices.Contains(i + l.LeafFace) && file.Faces.Count() > i + l.LeafFace)
                    {
                        face f = file.Faces[i + l.LeafFace];
                        visibleFaces.Add(f);
                        usedIndices.Add(i + l.LeafFace);
                    
                    }

                }

            }


            //arrays
            List<int> indices = new List<int>();
            foreach (face f in visibleFaces)
            {
                if (f.Type == 1 || f.Type == 3)
                {
                    for (int i = f.Meshvert; i < f.Meshvert + f.N_Meshverts; i++)
                    {
                        int index = f.Vertex + file.MeshVerts[i].Offset;
                        indices.Add(index);
                    }

                }
            }
            IBuffer = new IndexBuffer(Game.GraphicsDevice, IndexElementSize.ThirtyTwoBits, NbIndices, BufferUsage.WriteOnly);
            if (indices.Count != 0)
                IBuffer.SetData(indices.ToArray());
        }


        VertexBuffer VBuffer;
        IndexBuffer IBuffer;
        private void CreateBuffers()
        {
            VBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionNormalTexture), NbVertices, BufferUsage.WriteOnly);

            List<VertexPositionNormalTexture> vert = new List<VertexPositionNormalTexture>();
            List<int> indices = new List<int>();

            HashSet<int> vertexID = new HashSet<int>();
            foreach (vertex v in file.Vertices)
            {
                Vector3 position = V3FromFloatArray(v.Position);
                Vector2 texcoord = new Vector2(v.TexCoord[0, 0], v.TexCoord[0, 1]);
                Vector3 normal = V3FromFloatArray(v.Normal);

                vert.Add(new VertexPositionNormalTexture(position, normal, texcoord));
            }

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
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, NbVertices, 0, NbVertices / 3);
                
            }
             
            base.Draw(gameTime);

            
        }
    }


}
