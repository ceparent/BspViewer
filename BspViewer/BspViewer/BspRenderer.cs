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

            ICameraComponent camera = (ICameraComponent)Game.Services.GetService(typeof(ICameraComponent));

            base.LoadContent();
        }



        VertexBuffer VBuffer;
        IndexBuffer IBuffer;
        private void CreateBuffers()
        {
            VBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionNormalTexture), NbVertices, BufferUsage.WriteOnly);
            IBuffer = new IndexBuffer(Game.GraphicsDevice, IndexElementSize.ThirtyTwoBits, NbIndices, BufferUsage.WriteOnly);

            List<VertexPositionNormalTexture> vert = new List<VertexPositionNormalTexture>();
            List<int> indices = new List<int>();

            foreach (face f in file.Faces)
            {
                if (f.Type == 1 )
                {

                    for (int i = f.Vertex; i < f.Vertex + f.N_Vertices; i++)
                    {
                        vertex v = file.Vertices[i];

                        Vector3 position = new Vector3(v.Position[0], v.Position[2], -v.Position[1]);
                        Vector2 texcoord = new Vector2(v.TexCoord[0, 0], v.TexCoord[0, 1]);
                        Vector3 normal = new Vector3(v.Normal[0], v.Normal[1], v.Normal[2]);

                        vert.Add(new VertexPositionNormalTexture(position, normal, texcoord));
                    }

                   
                    
                    for (int i = f.Meshvert; i < f.Meshvert + f.N_Meshverts; i++)
                    {
                        int indice = file.MeshVerts[i].Offset + f.Vertex;
                        indices.Add(indice);
                    }
                }
            }
            VBuffer.SetData(vert.ToArray());
            IBuffer.SetData(indices.ToArray());
            


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
