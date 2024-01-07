using HKX2;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using ObjLoader;
using ObjLoader.Loader.Loaders;

namespace ColBakeTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var fact = new ObjLoaderFactory();
            var loader = fact.Create();

            using (var fs = File.OpenRead(args[0]))
            {
                var res = loader.Load(fs);

                List<Vector3> verts = new List<Vector3>();
                List<ushort> indices = new List<ushort>();

                foreach (var vert in res.Vertices)
                {
                    verts.Add(new Vector3(vert.X, vert.Y, vert.Z));
                }
                foreach (var idx in res.Groups[0].Faces)
                {
                    indices.Add((ushort)(idx[0].VertexIndex - 1));
                    indices.Add((ushort)(idx[1].VertexIndex - 1));
                    indices.Add((ushort)(idx[2].VertexIndex - 1));
                }

                HKX2.Builders.hknpCollisionMeshBuilder colBuilder = new HKX2.Builders.hknpCollisionMeshBuilder();
                colBuilder.AddMesh(verts, indices);
                var root = colBuilder.CookCollision();

                using (FileStream s2 = File.Create(args[0] + ".hkx"))
                {
                    BinaryWriterEx bw = new BinaryWriterEx(false, s2);
                    var s = new HKX2.PackFileSerializer();
                    s.Serialize(root, bw);
                }
            }
        }
    }
}
