using Andre.IO.VFS;
using SoulsFormats;
using StudioCore.Editors.MapEditor;
using System.Numerics;

// Checks the Collision Editor's Create tab against a game binder: a template file is made into a new collision with
// CollisionEditorShapes, appended to the binder as HavokCollisionBank.AddNewFiles does, and the binder written and read
// back. The new file must be found by name, read with the binder's compendium, and decode to the triangles given.
//   CollisionSurvey <GAME> <mapId> 0 create
static class Create
{
    public static string BodyName(object body) => body.GetType().GetField("m_name")?.GetValue(body) as string;

    public static void Survey(VirtualFileSystem fs, string gameName, string mapId)
    {
        var profile = Havok.HavokProfile.FromGame(gameName);

        foreach (var type in new[] { "h", "l" })
        {
            var bhdPath = gameName is "DS3" or "SDT" or "BB"
                ? $"/map/{mapId}/{type}{mapId.Substring(1)}.hkxbhd"
                : $"/map/{mapId.Substring(0, 3)}/{mapId}/{type}{mapId.Substring(1)}.hkxbhd";
            var bdtPath = Path.ChangeExtension(bhdPath, ".hkxbdt");
            var binder = BXF4.Read(fs.ReadFile(bhdPath).Value.ToArray(), fs.ReadFile(bdtPath).Value.ToArray());

            var compFile = binder.Files.FirstOrDefault(f => f.Name.Contains(".compendium"));
            Havok.HavokCompendium compendium = null;
            if (compFile != null)
            {
                var c = compFile.Bytes.ToArray();
                if (DCX.Is(c)) c = DCX.Decompress(c).ToArray();
                compendium = Havok.Files.HavokFile.ReadCompendium(c, profile);
            }

            var collisions = binder.Files.Where(f => f.Name.Contains(".hkx.dcx") && f.Name.Split('\\').Length == 2).ToList();
            Console.WriteLine($"== {type} binder: {binder.Files.Count} files, {collisions.Count} collisions, " +
                $"ids {binder.Files.Min(f => f.ID)}..{binder.Files.Max(f => f.ID)}, flags {string.Join(",", collisions.Select(f => f.Flags).Distinct())}, " +
                $"folder '{collisions[0].Name.Split('\\')[0]}', compendium {(compendium != null ? "yes" : "no")}");

            // The template, as ReadTemplateDocument picks it
            Havok.HavokDocument template = null;
            string templateName = null;
            foreach (var file in collisions)
            {
                var bytes = file.Bytes.ToArray();
                if (DCX.Is(bytes)) bytes = DCX.Decompress(bytes).ToArray();
                var doc = Havok.Files.HavokFile.Read(bytes, profile, compendium);
                if (CollisionEditorShapes.IsTemplate(doc)) { template = doc; templateName = file.Name; break; }
            }

            if (template == null) { Console.WriteLine("  no template"); continue; }

            var body = Havok.Shared.CollisionGeometry.Bodies(template.Objects).First();
            Console.WriteLine($"  template {templateName} body '{BodyName(body)}'");

            // A square with two materials, as the Create tab builds one
            var vertices = new List<Vector3> { new(-2, 0, -3), new(2, 0, -3), new(2, 0, 3), new(-2, 0, 3) };
            var indices = new List<int> { 0, 2, 1, 0, 3, 2 };
            var materials = Enumerable.Range(0, indices.Count / 3).Select(i => i == 0 ? 1u : 29u).ToList();

            if (!CollisionEditorShapes.MakeNewCollision(template, "123456", vertices, indices, materials))
            {
                Console.WriteLine("  MakeNewCollision failed");
                continue;
            }

            Console.WriteLine($"  new body '{BodyName(Havok.Shared.CollisionGeometry.Bodies(template.Objects).First())}'");

            // Appended as AddNewFiles does
            var sample = collisions[0];
            DCX.Decompress(sample.Bytes, out DCX.Type compression);
            var newName = $"{sample.Name.Split('\\')[0]}\\{type}{mapId.Substring(1)}_123456.hkx.dcx";
            binder.Files.Add(new BinderFile(sample.Flags, binder.Files.Max(f => f.ID) + 1, newName,
                DCX.Compress(Havok.Files.HavokFile.Write(template), compression)) { CompressionType = sample.CompressionType });

            binder.Write(out byte[] bhd, out byte[] bdt);

            // Read back
            var reread = BXF4.Read(bhd, bdt);
            var found = reread.Files.FirstOrDefault(f => f.Name == newName);
            Console.WriteLine($"  reread: {reread.Files.Count} files, new file {(found != null ? $"found id {found.ID}" : "MISSING")}, " +
                $"others intact {reread.Files.Count(f => collisions.Any(c => c.Name == f.Name)) == collisions.Count}");

            if (found == null) continue;

            var newBytes = found.Bytes.ToArray();
            var newCompression = DCX.Type.None;
            if (DCX.Is(newBytes)) newBytes = DCX.Decompress(newBytes, out newCompression).ToArray();
            var newDoc = Havok.Files.HavokFile.Read(newBytes, profile, compendium);
            var triangles = Havok.Shared.CollisionGeometry.Submeshes(newDoc.Objects).SelectMany(t => t).ToList();

            var expected = indices.Select(i => vertices[i]).ToList();
            var maxError = triangles.Count == expected.Count
                ? triangles.Zip(expected, (a, b) => Vector3.Distance(a, b)).Max()
                : float.NaN;

            var newShape = Havok.Shared.CollisionGeometry.Bodies(newDoc.Objects).First().Shape;
            var mats = CollisionEditorShapes.GetMaterialsByKey(newShape).Values.Distinct().OrderBy(m => m);

            Console.WriteLine($"  new file: compression {newCompression} (binder's {compression}), {triangles.Count / 3} triangles " +
                $"(expected {expected.Count / 3}), max corner error {maxError:0.####}, materials {string.Join(",", mats)}");
        }
    }
}
