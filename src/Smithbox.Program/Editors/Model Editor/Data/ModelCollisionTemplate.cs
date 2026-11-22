using Havok.Builders;
using Havok.Shared;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// A MOPP mesh of the older games (hkp: DES, DS1, DS1R, DS2S) to base a new mesh on, for an object whose collision has
/// none: most of their objects are boxes and convex hulls (ModelCollisionSurvey), which have no storage, material
/// parameter or code settings for a mesh. Another object of the game with a mesh collision has them all, in the same
/// class set, as the game's own files do. Found by reading the project's objects in name order, once per project.
/// </summary>
public static class ModelCollisionTemplate
{
    private sealed class Found
    {
        public object Shape;
        public string Source;
    }

    private static readonly ConditionalWeakTable<ProjectEntry, Found> Cache = new();

    // The objects read at most, so a game without any mesh object doesn't read them all every time
    private const int MaxBinders = 3000;

    /// <summary>The first object mesh of the project's objects, and the collision file it is in. Null if there is none.</summary>
    public static object FindHkpMesh(ProjectEntry project, out string source)
    {
        if (!Cache.TryGetValue(project, out var found))
        {
            found = Search(project);
            Cache.AddOrUpdate(project, found);
        }

        source = found.Source;
        return found.Shape;
    }

    private static Found Search(ProjectEntry project)
    {
        var bank = project.Handler.ModelData?.PrimaryBank;
        var profile = Havok.HavokProfile.FromGame(project.Descriptor.ProjectType.ToString());

        if (bank?.Assets == null || profile == null)
            return new Found();

        foreach (var container in bank.Assets.Values.OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase).Take(MaxBinders))
        {
            try
            {
                using var binder = container.ReadContainer();

                if (binder == null)
                    continue;

                foreach (var file in binder.Files)
                {
                    var name = FilePathUtils.GetPureFilename(file.Name);

                    // The object's collision, as the Model Editor reads it (o0000.hkx, DS2S o00_0100_h.hkx)
                    if (!file.Name.Contains(".hkx", StringComparison.OrdinalIgnoreCase)
                        || file.Name.Contains(".hkxpwv", StringComparison.OrdinalIgnoreCase)
                        || !(name.Equals(container.Name, StringComparison.OrdinalIgnoreCase)
                            || name.Equals($"{container.Name}_h", StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }

                    var bytes = DCX.Is(file.Bytes) ? DCX.Decompress(file.Bytes).ToArray() : file.Bytes.ToArray();
                    var document = Havok.Files.HavokFile.Read(bytes, profile);
                    var body = CollisionGeometry.Bodies(document.Objects).FirstOrDefault(b => HkpMeshShapeRebuild.IsSupported(b.Shape));

                    if (body != null)
                        return new Found { Shape = body.Shape, Source = name };
                }
            }
            catch
            {
                // An object that doesn't read (DES's Havok 4 files) is passed over
            }
        }

        return new Found();
    }
}
