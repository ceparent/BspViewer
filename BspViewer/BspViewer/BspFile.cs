using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BspViewer
{
    /**************************************************************************************DATA STRUCTURES***********************************************************************************************/

    public struct header
    {
        public string MagicString;
        public int Version;
        public dirEntry[] DirEntries;

        public header(string magic, int version, dirEntry[] dirEntries)
        {
            MagicString = magic;
            Version = version;
            DirEntries = dirEntries;
        }

    }
    public struct dirEntry
    {
        public int Offset;
        public int Length;

        public dirEntry(int offset, int length)
        {
            Offset = offset;
            Length = length;
        }
    }

    public struct texture
    {
        public string Name;
        public int Flags;
        public int Contents;

        public texture(string name, int flags, int contents)
        {
            Name = name;
            Flags = flags;
            Contents = contents;
        }

    }

    public struct plane
    {
        public float[] Normal;
        public float Dist;

        public plane(float[] normal, float dist)
        {
            Normal = normal;
            Dist = dist;
        }

    }

    public struct node
    {
        public int Plane;
        public int[] Children;
        public int[] Mins;
        public int[] Maxs;

        public node(int plane, int[] children, int[] mins, int[] maxs)
        {
            Plane = plane;
            Children = children;
            Mins = mins;
            Maxs = maxs;
        }

    }

    public struct leaf
    {
        public int Cluster;
        public int Area;
        public int[] Mins;
        public int[] Maxs;
        public int LeafFace;
        public int N_LeafFaces;
        public int LeafBrush;
        public int N_LeafBrushes;

        public leaf(int cluster, int area, int[] mins, int[] maxs, int leafface, int n_leaffaces, int leafbrush, int n_leafbrushes)
        {
            Cluster = cluster;
            Area = area;
            Mins = mins;
            Maxs = maxs;
            LeafFace = leafface;
            N_LeafFaces = n_leaffaces;
            LeafBrush = leafbrush;
            N_LeafBrushes = n_leafbrushes;
        }
    }

    public struct leafface
    {
        public int Face;

        public leafface(int face)
        {
            Face = face;
        }

    }

    public struct leafbrush
    {
        public int Brush;

        public leafbrush(int brush)
        {
            Brush = brush;
        }

    }

    public struct model
    {
        public float[] Mins;
        public float[] Maxs;
        public int Face;
        public int N_Faces;
        public int Brush;
        public int N_Brushes;

        public model(float[] mins, float[] maxs, int face, int n_faces, int brush, int n_brushes)
        {
            Mins = mins;
            Maxs = maxs;
            Face = face;
            N_Faces = n_faces;
            Brush = brush;
            N_Brushes = n_brushes;
        }
    }

    public struct brush
    {
        public int BrushSide;
        public int N_BrushSides;
        public int Texture;

        public brush(int brushside, int n_brushsides, int texture)
        {
            BrushSide = brushside;
            N_BrushSides = n_brushsides;
            Texture = texture;
        }

    }

    public struct brushside
    {
        public int Plane;
        public int Texture;

        public brushside(int plane, int texture)
        {
            Plane = plane;
            Texture = texture;
        }

    }

    public struct vertex
    {
        public float[] Position;
        public float[,] TexCoord;
        public float[] Normal;
        public byte[] Color;

        public vertex(float[] position, float[,] texcoord, float[] normal, byte[] color)
        {
            Position = position;
            TexCoord = texcoord;
            Normal = normal;
            Color = color;
        }

    }

    public struct meshvert
    {
        public int Offset;

        public meshvert(int offset)
        {
            Offset = offset;
        }

    }

    public struct effect
    {
        public string Name;
        public int Brush;
        public int Unknown;

        public effect(string name, int brush, int unknown)
        {
            Name = name;
            Brush = brush;
            Unknown = unknown;
        }
    }


    public struct face
    {
        public int Texture;
        public int Effect;
        public int Type;
        public int Vertex;
        public int N_Vertices;
        public int Meshvert;
        public int N_Meshverts;
        public int Lm_index;
        public int[] Lm_start;
        public int[] Lm_size;
        public float[] Lm_origin;
        public float[,] Lm_vecs;
        public float[] Normal;
        public int[] Size;

        public face(int texture, int effect, int type, int vertex, int n_vertices, int meshvert, int n_meshverts, int lm_index, int[] lm_start, int[] lm_size, float[] lm_origin, float[,] lm_vecs, float[] normal, int[] size)
        {
            Texture = texture;
            Effect = effect;
            Type = type;
            Vertex = vertex;
            N_Vertices = n_vertices;
            Meshvert = meshvert;
            N_Meshverts = n_meshverts;
            Lm_index = lm_index;
            Lm_start = lm_start;
            Lm_size = lm_size;
            Lm_origin = lm_origin;
            Lm_vecs = lm_vecs;
            Normal = normal;
            Size = size;

        }

    }

    public struct lightmap
    {
        public byte[, ,] Map;

        public lightmap(byte[, ,] map)
        {
            Map = map;
        }
    }

    public struct lightvol
    {
        public byte[] Ambient;
        public byte[] Directional;
        public byte[] Dir;

        public lightvol(byte[] ambient, byte[] directional, byte[] dir)
        {
            Ambient = ambient;
            Directional = directional;
            Dir = dir;
        }

    }

    public struct visdata
    {
        public int N_vecs;
        public int Sz_vecs;
        public byte[] Vecs;

        public visdata(int n_vecs, int sz_vecs, byte[] vecs)
        {
            N_vecs = n_vecs;
            Sz_vecs = sz_vecs;
            Vecs = vecs;
        }

    }


    class BspFile
    {

        BinaryReader reader;
        public BspFile(string path)
        {
            LoadData(path);

        }

        /*****************************************************************************************OTHER*METHODS***********************************************************************************************/
        private string ReadString(int length)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                char character = (char)reader.ReadByte();

                if(character == '\0')
                    character = ' ';

                builder.Append(character);
            }

            return builder.ToString().TrimEnd();
        }
        private byte ReadByte()
        {
            return reader.ReadByte();
        }
        private int ReadInt()
        {
            return reader.ReadInt32();
        }
        private float ReadFloat()
        {
            return BitConverter.ToSingle(reader.ReadBytes(4), 0);
        }

        private void Sync(int Step)
        {
            int offset = Header.DirEntries[Step].Offset;
            reader.BaseStream.Position = offset;
        }

        /*******************************************************************************************LOAD*****************************************************************************************************/
        public void LoadData(string path)
        {
            FileStream stream = new FileStream(path, FileMode.Open);
            reader = new BinaryReader(stream);

            ReadHeader();


            Sync(0);
            ReadEntities();
    
            Sync(1);
            ReadTextures();

            Sync(2);
            ReadPlanes();

            Sync(3);
            ReadNodes();

            Sync(4);
            ReadLeaves();

            Sync(5);
            ReadLeafFaces();

            Sync(6);
            ReadLeafBrushes();

            Sync(7);
            ReadModels();

            Sync(8);
            ReadBrushes();

            Sync(9);
            ReadBrusheSides();

            Sync(10);
            ReadVertices();

            Sync(11);
            ReadMeshVerts();

            Sync(12);
            ReadEffects();

            Sync(13);
            ReadFaces();

            Sync(14);
            ReadLightMaps();

            Sync(15);
            ReadLightVols();

            Sync(16);
            ReadVisdata();

            reader.Close();
            
        }

        public header Header { get; set; }
        private void ReadHeader()
        {
            // Magic string
            string magic = ReadString(4);
            if (magic != "IBSP")
                throw new InvalidDataException("Not a valid file");

            // Version
            int version = ReadInt();

            // DirEntries
            dirEntry[] entries = new dirEntry[17];
            for (int i = 0; i < entries.Length; i++)
            {
                // Entry
                entries[i] = new dirEntry(ReadInt(), ReadInt());
            }


            Header = new header(magic, version, entries);
        }

        public string Entities { get; set; }
        private void ReadEntities()
        {
            //Length of the Lump
            int stringLength = Header.DirEntries[0].Length;

            //Read entities
            Entities = ReadString(stringLength);
        }

        public texture[] Textures { get; set; }
        private void ReadTextures()
        {
            //Length of the Lump
            int TextureLength = Header.DirEntries[1].Length;
            //Number of textures to load
            int nbTExtures = TextureLength / (sizeof(byte) *64 + sizeof(int) * 2);

            //Load textures;
            Textures = new texture[nbTExtures];
            for (int i = 0; i < nbTExtures; i++)
            {
                //Name
                string name = ReadString(64);

                //flags
                int flags = ReadInt();

                //contents
                int contents = ReadInt();

                Textures[i] = new texture(name, flags, contents);
            }

        }

        public plane[] Planes { get; set; }
        private void ReadPlanes()
        {
            //Length of the Lump
            int PlanesLength = Header.DirEntries[2].Length;
            //Number of textures to load
            int nbPlanes = PlanesLength / (sizeof(float) * 4);

            Planes = new plane[nbPlanes];
            for (int i = 0; i < nbPlanes; i++)
            {
                // Normal
                float[] normal = new float[3] { ReadFloat(), ReadFloat(), ReadFloat() };

                //Dist
                float dist = ReadFloat();

                Planes[i] = new plane(normal, dist);
            }
        }

        public node[] Nodes { get; set; }
        private void ReadNodes()
        {
            //Length of the Lump
            int NodesLength = Header.DirEntries[3].Length;
            //Number of textures to load
            int nbNodes = NodesLength / (sizeof(int) * 9);

            Nodes = new node[nbNodes];
            for (int i = 0; i < nbNodes; i++)
            {
                //Plane
                int plane = ReadInt();

                //Children
                int[] children = new int[2] { ReadInt(), ReadInt() };

                //Mins
                int[] mins = new int[3] { ReadInt(), ReadInt(), ReadInt()};

                // Maxs
                int[] maxs = new int[3] { ReadInt(), ReadInt(), ReadInt() };

                Nodes[i] = new node(plane, children, mins, maxs);
            }

        }

        public leaf[] Leaves { get; set; }
        private void ReadLeaves()
        {
            //Length of the Lump
            int LeavesLength = Header.DirEntries[4].Length;
            //Number of textures to load
            int nbLeaves = LeavesLength / (sizeof(int) * 12);

            Leaves = new leaf[nbLeaves];
            for (int i = 0; i < nbLeaves; i++)
            {
                //Cluster
                int cluster = ReadInt();

                //Area
                int area = ReadInt();

                //Mins
                int[] mins = new int[3] { ReadInt(), ReadInt(), ReadInt() };

                // Maxs
                int[] maxs = new int[3] { ReadInt(), ReadInt(), ReadInt() };

                //leafface
                int leafface = ReadInt();

                //n_leaffaces
                int n_leaffaces = ReadInt();

                //leafbrush
                int leafbrush = ReadInt();

                //n_leafbrush
                int n_leafbrush = ReadInt();


                Leaves[i] = new leaf(cluster, area, mins, maxs, leafface, n_leaffaces, leafbrush, n_leafbrush);
            }

        }

        public leafface[] LeafFaces { get; set; }
        private void ReadLeafFaces()
        {
            //Length of the Lump
            int LeafFacesLength = Header.DirEntries[5].Length;
            //Number of textures to load
            int nbLeafFaces = LeafFacesLength / (sizeof(int));

            LeafFaces = new leafface[nbLeafFaces];
            for (int i = 0; i < nbLeafFaces; i++)
            {
                //face
                int face = ReadInt();

                LeafFaces[i] = new leafface(face);

            }

        }

        public leafbrush[] LeafBrushes { get; set; }
        private void ReadLeafBrushes()
        {
            //Length of the Lump
            int LeafBrushesLength = Header.DirEntries[6].Length;
            //Number of textures to load
            int nbLeafBrushes = LeafBrushesLength / (sizeof(int));

            LeafBrushes = new leafbrush[nbLeafBrushes];
            for (int i = 0; i < nbLeafBrushes; i++)
            {
                //brush
                int brush = ReadInt();

                LeafBrushes[i] = new leafbrush(brush);

            }

        }

        public model[] Models { get; set; }
        private void ReadModels()
        {
            //Length of the Lump
            int ModelsLength = Header.DirEntries[7].Length;
            //Number of textures to load
            int nbModels = ModelsLength / (sizeof(float) * 6 + sizeof(int) * 4);

            Models = new model[nbModels];
            for (int i = 0; i < nbModels; i++)
            {

                //Mins
                float[] mins = new float[3] { ReadFloat(), ReadFloat(), ReadFloat() };

                // Maxs
                float[] maxs = new float[3] { ReadFloat(), ReadFloat(), ReadFloat() };

                //face
                int face = ReadInt();

                //n_face
                int n_faces = ReadInt();

                //brush
                int brush = ReadInt();

                //n_brushes
                int n_brushes = ReadInt();

                Models[i] = new model(mins, maxs, face, n_faces, brush, n_brushes);

            }

        }

        public brush[] Brushes { get; set; }
        private void ReadBrushes()
        {
            //Length of the Lump
            int BrushesLength = Header.DirEntries[8].Length;
            //Number of textures to load
            int nbBrushes = BrushesLength / (sizeof(int) * 3);


            Brushes = new brush[nbBrushes];
            for (int i = 0; i < nbBrushes; i++)
            {
                //brushside
                int brushside = ReadInt();

                //n_brushside
                int n_brushside = ReadInt();

                //texture
                int texture = ReadInt();

                Brushes[i] = new brush(brushside, n_brushside, texture);

            }
        }

        public brushside[] BrusheSides { get; set; }
        private void ReadBrusheSides()
        {
            //Length of the Lump
            int BrusheSideLength = Header.DirEntries[9].Length;
            //Number of textures to load
            int nbBrusheSides = BrusheSideLength / (sizeof(int) * 2);


            BrusheSides = new brushside[nbBrusheSides];
            for (int i = 0; i < nbBrusheSides; i++)
            {
                //plane
                int plane = ReadInt();

                //texture
                int texture = ReadInt();


                BrusheSides[i] = new brushside(plane, texture);

            }
        }

        public vertex[] Vertices { get; set; }
        private void ReadVertices()
        {
            //Length of the Lump
            int VerticesLength = Header.DirEntries[10].Length;
            //Number of textures to load
            int nbVertices = VerticesLength / (sizeof(float) * 10 + sizeof(byte) * 4);


            Vertices = new vertex[nbVertices];
            for (int i = 0; i < nbVertices; i++)
            {
                //position
                float[] position = new float[3] { ReadFloat(), ReadFloat(), ReadFloat() };

                //texcoord
                float[,] texcoord = new float[2,2] { { ReadFloat(), ReadFloat() }, { ReadFloat(), ReadFloat() } };

                //position
                float[] normal = new float[3] { ReadFloat(), ReadFloat(), ReadFloat() };

                //position
                byte[] color = new byte[4] { ReadByte(), ReadByte(), ReadByte(), ReadByte() };

                Vertices[i] = new vertex(position, texcoord, normal, color);

            }

        }

        public meshvert[] MeshVerts { get; set; }
        private void ReadMeshVerts()
        {
            //Length of the Lump
            int MeshVertsLength = Header.DirEntries[11].Length;
            //Number of textures to load
            int nbMeshVerts = MeshVertsLength / (sizeof(int));


            MeshVerts = new meshvert[nbMeshVerts];
            for (int i = 0; i < nbMeshVerts; i++)
            {
                //offset
                int offset = ReadInt();

                MeshVerts[i] = new meshvert(offset);

            }

        }

        public effect[] Effects { get; set; }
        private void ReadEffects()
        {
            //Length of the Lump
            int EffectsLength = Header.DirEntries[12].Length;
            //Number of textures to load
            int nbEffects = EffectsLength / (sizeof(byte) * 64 + sizeof(int) * 2);


            Effects = new effect[nbEffects];
            for (int i = 0; i < nbEffects; i++)
            {
                // name
                string name = ReadString(64);

                //brush
                int brush = ReadInt();

                //unknown
                int unknown = ReadInt();

                Effects[i] = new effect(name, brush, unknown);

            }
        }

        public face[] Faces { get; set; }
        private void ReadFaces()
        {
            //Length of the Lump
            int FacesLength = Header.DirEntries[13].Length;
            //Number of textures to load
            int nbFaces = FacesLength / (sizeof(int) * 14 + sizeof(float) * 12);


            Faces = new face[nbFaces];
            for (int i = 0; i < nbFaces; i++)
            {
                //texture
                int texture = ReadInt();

                //effect
                int effect = ReadInt();

                //type
                int type = ReadInt();

                //vertex
                int vertex = ReadInt();

                //n_vertices
                int n_vertices = ReadInt();

                //meshvert
                int meshvert = ReadInt();

                //n_meshvert
                int n_meshvert = ReadInt();

                //lm_index
                int lm_index = ReadInt();

                //lm_start
                int[] lm_start = new int[2] { ReadInt(), ReadInt() };

                //lm_size
                int[] lm_size = new int[2] { ReadInt(), ReadInt() };

                //lm_origin
                float[] lm_origin = new float[3] { ReadFloat(), ReadFloat(), ReadFloat() };

                //lm_vecs
                float[,] lm_vecs = new float[2, 3] { { ReadFloat(), ReadFloat(), ReadFloat() }, { ReadFloat(), ReadFloat(), ReadFloat() } };

                //normal
                float[] normal = new float[3] { ReadFloat(), ReadFloat(), ReadFloat() };

                //size
                int[] size = new int[2] { ReadInt(), ReadInt() };

                Faces[i] = new face(texture, effect, type, vertex, n_vertices, meshvert, n_meshvert, lm_index, lm_start, lm_size, lm_origin, lm_vecs, normal, size);

            }
        }

        public lightmap[] Lightmaps { get; set; }
        private void ReadLightMaps()
        {
            //Length of the Lump
            int lmLength = Header.DirEntries[14].Length;
            //Number of textures to load
            int nblm = lmLength / (sizeof(byte) * 128*128*3);

            Lightmaps = new lightmap[nblm];
            for (int i = 0; i < nblm; i++)
            {
                //Map
                byte[, ,] map = new byte[128, 128, 3];

                for (int x = 0; x < 128; x++)
                {
                    for (int y = 0; y < 128; y++)
                    {
                        for (int z = 0; z < 3; z++)
                        {
                            map[x, y, z] = ReadByte();
                        }
                    }
                }

                Lightmaps[i] = new lightmap(map);

            }
        }

        public lightvol[] LightVols { get; set; }
        private void ReadLightVols()
        {
            //Length of the Lump
            int LightVolsLength = Header.DirEntries[15].Length;
            //Number of textures to load
            int nbLightVols = LightVolsLength / (sizeof(byte) * 8);

            LightVols = new lightvol[nbLightVols];
            for (int i = 0; i < nbLightVols; i++)
            {
                //ambient
                byte[] ambient = new byte[3] { ReadByte(), ReadByte(), ReadByte() };

                //directional
                byte[] directional = new byte[3] { ReadByte(), ReadByte(), ReadByte() };

                //dir
                byte[] dir = new byte[2] { ReadByte(), ReadByte() };

                LightVols[i] = new lightvol(ambient, directional, dir);
            }
        }

        public visdata VisData { get; set; }
        private void ReadVisdata()
        {
            // n_vecs
            int n_vecs = ReadInt();

            // sz_vecs
            int sz_vecs = ReadInt();

            //vecs
            byte[] vecs = new byte[n_vecs * sz_vecs];
            for (int i = 0; i < n_vecs * sz_vecs; i++)
            {
                vecs[i] = ReadByte();
            }

            VisData = new visdata(n_vecs, sz_vecs, vecs);

        }

    } 
}
