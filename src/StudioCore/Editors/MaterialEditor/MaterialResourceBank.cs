using SoulsFormats;
using StudioCore.Editor;
using StudioCore.AssetLocator;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.IO;

namespace StudioCore.Editors.MaterialEditor;

public class MaterialResourceBank
{
    private static Dictionary<string, MaterialInfo> _mtds = new();
    private static Dictionary<string, MaterialInfo> _matbins = new();

    public static IReadOnlyDictionary<string, MaterialInfo> Mtds => _mtds;

    public static IReadOnlyDictionary<string, MaterialInfo> Matbins => _matbins;

    public static void LoadMaterials()
    {
        TaskManager.Run(new TaskManager.LiveTask("Resource - Load Materials", TaskManager.RequeueType.WaitThenRequeue, false,
        () =>
        {
            LoadMatBins();
            LoadMtds();
        }));
    }

    public struct MaterialInfo
    {
        public string Name;
        public string Path;
        public MATBIN Matbin;
        public MTD Mtd;

        public MaterialInfo(string name, string path, MATBIN matbin, MTD mtd)
        {
            Name = name;
            Path = path;
            Matbin = matbin;
            Mtd = mtd;
        }
    }

    public static void LoadMatBins()
    {
        _matbins = new Dictionary<string, MaterialInfo>();

        var modPath = $"{Project.GameModDirectory}//material//";
        if (Directory.Exists(modPath))
        {
            var modFiles = Directory.GetFiles(modPath);

            // Mod
            foreach (var file in modFiles)
            {
                LoadMatbinFile(file);
            }
        }

        var rootPath = $"{Project.GameRootDirectory}//material//";
        if (Directory.Exists(rootPath))
        {
            var rootFiles = Directory.GetFiles(rootPath);

            // Root
            foreach (var file in rootFiles)
            {
                LoadMatbinFile(file);
            }
        }
    }

    public static void LoadMatbinFile(string file)
    {
        IBinder binder = null;

        if (file.Contains(".matbinbnd.dcx"))
        {
            binder = BND4.Read(file);
            using (binder)
            {
                foreach (BinderFile f in binder.Files)
                {
                    var path = f.Name;
                    var matname = Path.GetFileNameWithoutExtension(f.Name);

                    MaterialInfo info = new MaterialInfo(matname, path, MATBIN.Read(f.Bytes), null);

                    if (!_matbins.ContainsKey(matname))
                        _matbins.Add(matname, info);
                }
            }
        }
    }

    public static void LoadMtds()
    {
        _mtds = new Dictionary<string, MaterialInfo>();

        var modPath = $"{Project.GameModDirectory}//mtd//";
        if (Directory.Exists(modPath))
        {
            var modFiles = Directory.GetFiles(modPath);

            // Mod
            foreach (var file in modFiles)
            {
                LoadMtdFile(file);
            }
        }

        var rootPath = $"{Project.GameRootDirectory}//mtd//";
        if (Directory.Exists(rootPath))
        {
            var rootFiles = Directory.GetFiles(rootPath);

            // Root
            foreach (var file in rootFiles)
            {
                LoadMtdFile(file);
            }
        }
    }

    public static void LoadMtdFile(string file)
    {
        IBinder binder = null;

        if (file.Contains(".mtd.dcx"))
        {
            binder = BND4.Read(file);
            using (binder)
            {
                foreach (BinderFile f in binder.Files)
                {
                    var path = f.Name;
                    var matname = Path.GetFileNameWithoutExtension(f.Name);

                    MaterialInfo info = new MaterialInfo(matname, path, null, MTD.Read(f.Bytes));

                    if (!_mtds.ContainsKey(matname))
                        _mtds.Add(matname, info);
                }
            }
        }
    }

    public static void Setup()
    {
        if (Project.Type == ProjectType.Undefined)
            return;

        LoadMaterials();
    }
}
