using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.AssetLocator;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextureViewer;

public static class TextureViewBank
{
    public static bool IsLoaded { get; private set; }
    public static bool IsLoading { get; private set; }

    public static SortedDictionary<string, TextureViewInfo> TextureBank { get; set; }

    public static void LoadTextureFolders()
    {
        IsLoaded = false;
        IsLoading = true;

        TextureBank = new();

        var paramDir = @"\menu";
        var paramExt = @".tpf.dcx";

        if (Project.Type is ProjectType.AC6 or ProjectType.ER or ProjectType.SDT)
        {
            paramDir = @"\menu\hi";
        }

        foreach (var name in GetFileNames())
        {
            var filePath = $"{paramDir}\\{name}{paramExt}";

            if (File.Exists($"{Project.GameModDirectory}\\{filePath}"))
            {
                LoadTextureFolder($"{Project.GameModDirectory}\\{filePath}", true);
                //TaskLogs.AddLog($"Loaded from GameModDirectory: {filePath}");
            }
            else
            {
                LoadTextureFolder($"{Project.GameRootDirectory}\\{filePath}", false);
                //TaskLogs.AddLog($"Loaded from GameRootDirectory: {filePath}");
            }
        }

        IsLoaded = true;
        IsLoading = false;

        //TaskLogs.AddLog($"Graphics Param Bank - Load Complete");
    }

    private static void LoadTextureFolder(string path, bool isModFile)
    {
        if (path == null)
        {
            TaskLogs.AddLog($"Could not locate {path} when loading Texture TPF file.",
                    LogLevel.Warning);
            return;
        }
        if (path == "")
        {
            TaskLogs.AddLog($"Could not locate {path} when loading Texture TPF file.",
                    LogLevel.Warning);
            return;
        }

        var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(path));
        TextureViewInfo tStruct = new TextureViewInfo(name, path);
        tStruct.IsModFile = isModFile;

        var tpf = TPF.Read(path);
        tStruct.Textures = tpf.Textures;

        TextureBank.Add(name, tStruct);
    }

    public static List<string> GetFileNames()
    {
        var paramDir = @"\menu";
        var paramExt = @".tpf.dcx";

        if (Project.Type is ProjectType.AC6 or ProjectType.ER or ProjectType.SDT)
        {
            paramDir = @"\menu\hi";
        }

        HashSet<string> paramNames = new();
        List<string> ret = new();

        if (Directory.Exists(Project.GameRootDirectory + paramDir))
        {
            // ROOT
            var paramFiles = Directory.GetFileSystemEntries(Project.GameRootDirectory + paramDir, $@"*{paramExt}").ToList();
            foreach (var f in paramFiles)
            {
                var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));
                ret.Add(name);
                paramNames.Add(name);
            }

            // MOD
            if (Project.GameModDirectory != null && Directory.Exists(Project.GameModDirectory + paramDir))
            {
                paramFiles = Directory.GetFileSystemEntries(Project.GameModDirectory + paramDir, $@"*{paramExt}").ToList();

                foreach (var f in paramFiles)
                {
                    var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));

                    if (!paramNames.Contains(name))
                    {
                        ret.Add(name);
                        paramNames.Add(name);
                    }
                }
            }
        }

        return ret;
    }

    public class TextureViewInfo : IComparable<string>
    {
        public TextureViewInfo(string name, string path)
        {
            Name = name;
            Path = path;
            WasModified = false;
        }

        public string Name { get; set; }
        public string Path { get; set; }

        public bool IsModFile { get; set; }

        public bool WasModified { get; set; }

        public List<TPF.Texture> Textures { get; set; }

        public int CompareTo(string other)
        {
            return Name.CompareTo(other);
        }
    }
}
