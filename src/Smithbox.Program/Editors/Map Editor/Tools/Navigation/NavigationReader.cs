using Andre.IO.VFS;
using Havok.hk2018;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Reads a map's navigation data straight from a file system, as the game lays it out. The Map Editor uses it for
/// the vanilla data a project's maps are compared with; Tools/NavSurvey for every map of the game.
/// </summary>
public static class NavigationReader
{
    public static string NvaPath(string mapId) => $"/map/{mapId.Substring(0, 3)}/{mapId}/{mapId}.nva.dcx";

    public static string NvmPath(string mapId) => $"/map/{mapId.Substring(0, 3)}/{mapId}/{mapId}.nvmhktbnd.dcx";

    public static string OnavPath(string mapId) => $"/map/onav/{mapId}.onav.dcx";

    public static string NvcPath(string name) => $"/map/nvc/{name}.nvc.dcx";

    /// <summary>
    /// Reads a map's NVA, navmesh files, ONAV and the named NVC files. Null if the map has no NVA.
    /// </summary>
    public static NavigationMap Read(VirtualFileSystem fs, string mapId, Havok.HavokProfile profile, IEnumerable<string> nvcNames)
    {
        var nvaFile = fs.ReadFile(NvaPath(mapId));

        if (nvaFile == null)
            return null;

        var nva = NVA_ER.Read(nvaFile.Value.ToArray());
        var files = new Dictionary<string, NavmeshFile>();
        var bndFile = fs.ReadFile(NvmPath(mapId));

        if (bndFile != null)
        {
            var bnd = BND4.Read(bndFile.Value.ToArray());

            foreach (var f in bnd.Files)
            {
                var bytes = f.Bytes.ToArray();
                if (DCX.Is(bytes))
                    bytes = DCX.Decompress(bytes).ToArray();

                var name = Path.GetFileNameWithoutExtension(f.Name);
                files[name] = FromDocument(name, Havok.Files.HavokFile.Read(bytes, profile));
            }
        }

        var onavFile = fs.ReadFile(OnavPath(mapId));
        var onav = onavFile == null ? null : ONAV.Read(onavFile.Value.ToArray());

        var nvcs = new List<NvcFile>();
        foreach (var name in nvcNames)
        {
            var f = fs.ReadFile(NvcPath(name));
            if (f != null)
                nvcs.Add(new NvcFile { Name = name, Nvc = NVC.Read(f.Value.ToArray()) });
        }

        return new NavigationMap(mapId, nva, files, onav, nvcs);
    }

    /// <summary>
    /// A navmesh file from its Havok document.
    /// </summary>
    public static NavmeshFile FromDocument(string name, Havok.HavokDocument document)
    {
        return new NavmeshFile
        {
            Name = name,
            Document = document,
            Mesh = document.ObjectsOf<hkaiNavMesh>().FirstOrDefault(),
            HasMediator = document.ObjectsOf<hkaiStaticTreeNavMeshQueryMediator>().Any(),
            HasClearanceCache = document.ObjectsOf<hkaiNavMeshClearanceCache>().Any(),
        };
    }

    /// <summary>
    /// The NVC files of a list of names (e.g. m60_33_41_00_m60_33_42_00) that name the map, on either side.
    /// </summary>
    public static IEnumerable<string> NvcNamesFor(string mapId, IEnumerable<string> allNvcNames)
        => allNvcNames.Where(n => n.Length == 25 && (n.StartsWith(mapId, StringComparison.Ordinal) || n.EndsWith(mapId, StringComparison.Ordinal)));
}
