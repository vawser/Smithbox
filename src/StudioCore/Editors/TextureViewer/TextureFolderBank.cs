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

public static class TextureFolderBank
{
    public enum TextureViewCategory
    {
        None = 0,
        Menu = 1,
        Asset = 2,
        Character = 3
    }

    public static bool IsLoaded { get; private set; }
    public static bool IsLoading { get; private set; }

    public static SortedDictionary<string, TextureViewInfo> FolderBank { get; set; }

    public static void LoadTextureFolders()
    {
        IsLoaded = false;
        IsLoading = true;

        FolderBank = new();

        CollectMenuFolders(TextureViewCategory.Menu);

        // AC6 and ER only
        CollectAssetFolders(TextureViewCategory.Asset);

        // Characters
        CollectCharacterFolders(TextureViewCategory.Character);

        // TODO: support tpfbnd
        //CollectMenuFolders(TextureViewCategory.Menu, @".tpfbhd", true);

        IsLoaded = true;
        IsLoading = false;

        TaskLogs.AddLog($"Texture Folder Bank - Load Complete");
    }

    private static void CollectMenuFolders(TextureViewCategory category)
    {
        var folderDir = @"\menu";
        var fileExt = @".tpf.dcx";

        if (Project.Type is ProjectType.AC6 or ProjectType.ER or ProjectType.SDT)
        {
            folderDir = @"\menu\hi";
        }

        foreach (var name in GetFileNames(folderDir, fileExt))
        {
            var filePath = $"{folderDir}\\{name}{fileExt}";

            if (File.Exists($"{Project.GameModDirectory}\\{filePath}"))
            {
                LoadTextureFolder($"{Project.GameModDirectory}\\{filePath}", category, true);
            }
            else
            {
                LoadTextureFolder($"{Project.GameRootDirectory}\\{filePath}", category, false);
            }
        }
    }

    private static void CollectAssetFolders(TextureViewCategory category)
    {
        var folderDir = @"";
        var fileExt = @".tpf.dcx";

        if (Project.Type is ProjectType.AC6)
        {
            folderDir = @"\asset\environment\texture";

            foreach (var name in GetFileNames(folderDir, fileExt))
            {
                var filePath = $"{folderDir}\\{name}{fileExt}";

                if (File.Exists($"{Project.GameModDirectory}\\{filePath}"))
                {
                    LoadTextureFolder($"{Project.GameModDirectory}\\{filePath}", category, true);
                }
                else
                {
                    LoadTextureFolder($"{Project.GameRootDirectory}\\{filePath}", category, false);
                }
            }
        }

        if (Project.Type is ProjectType.ER)
        {
            var searchFolderDir = $@"\asset\aet\";

            if(Directory.Exists($"{Project.GameModDirectory}\\{searchFolderDir}"))
            {
                searchFolderDir = $"{Project.GameModDirectory}\\{searchFolderDir}";
            }
            else
            {
                searchFolderDir = $"{Project.GameRootDirectory}\\{searchFolderDir}";
            }

            if(Directory.Exists(searchFolderDir))
            {
                foreach (var folder in Directory.GetDirectories(searchFolderDir))
                {
                    var folderName = folder.Substring(folder.Length - 6); // Assumes folders follow aetXXX

                    folderDir = $@"\asset\aet\{folderName}";

                    foreach (var name in GetFileNames(folderDir, fileExt))
                    {
                        var filePath = $"{folderDir}\\{name}{fileExt}";

                        if (File.Exists($"{Project.GameModDirectory}\\{filePath}"))
                        {
                            LoadTextureFolder($"{Project.GameModDirectory}\\{filePath}", category, true);
                        }
                        else
                        {
                            LoadTextureFolder($"{Project.GameRootDirectory}\\{filePath}", category, false);
                        }
                    }
                }
            }
        }
    }

    private static void CollectCharacterFolders(TextureViewCategory category)
    {
        var folderDir = @"\chr";
        var fileExt = ".texbnd.dcx";

        if (Project.Type is ProjectType.DES or ProjectType.DS1)
        {
            fileExt = ".tpf";
        }

        if (Project.Type is ProjectType.DS2S)
        {
            folderDir = @"\model\chr\";
            fileExt = ".texbnd";
        }

        foreach (var name in GetFileNames(folderDir, fileExt))
        {
            var filePath = $"{folderDir}\\{name}{fileExt}";

            if (File.Exists($"{Project.GameModDirectory}\\{filePath}"))
            {
                LoadTextureFolder($"{Project.GameModDirectory}\\{filePath}", category, true);
            }
            else
            {
                LoadTextureFolder($"{Project.GameRootDirectory}\\{filePath}", category, false);
            }
        }
    }

    // General
    private static void LoadTextureFolder(string path, TextureViewCategory category, bool isModFile)
    {
        if (path == null)
        {
            TaskLogs.AddLog($"Could not locate {path} when loading Texture file.",
                    LogLevel.Warning);
            return;
        }
        if (path == "")
        {
            TaskLogs.AddLog($"Could not locate {path} when loading Texture file.",
                    LogLevel.Warning);
            return;
        }

        var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(path));
        TextureViewInfo tStruct = new TextureViewInfo(name, path);
        tStruct.IsModFile = isModFile;
        tStruct.Category = category;
        tStruct.Textures = null; // Done after selection in the Viewer

        FolderBank.Add(name, tStruct);
    }

    public static List<string> GetFileNames(string fileDir, string fileExt)
    {
        HashSet<string> fileNames = new();
        List<string> ret = new();

        if (Directory.Exists(Project.GameRootDirectory + fileDir))
        {
            // ROOT
            var paramFiles = Directory.GetFileSystemEntries(Project.GameRootDirectory + fileDir, $@"*{fileExt}").ToList();
            foreach (var f in paramFiles)
            {
                var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));
                ret.Add(name);
                fileNames.Add(name);
            }

            // MOD
            if (Project.GameModDirectory != null && Directory.Exists(Project.GameModDirectory + fileDir))
            {
                paramFiles = Directory.GetFileSystemEntries(Project.GameModDirectory + fileDir, $@"*{fileExt}").ToList();

                foreach (var f in paramFiles)
                {
                    var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));

                    if (!fileNames.Contains(name))
                    {
                        ret.Add(name);
                        fileNames.Add(name);
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

        public TextureViewCategory Category { get; set; }

        public string Name { get; set; }
        public string Path { get; set; }

        public bool IsModFile { get; set; }

        public bool WasModified { get; set; }

        public List<TPF.Texture> Textures { get; set; }

        public IBinder TextureBinder { get; set; }

        public int CompareTo(string other)
        {
            return Name.CompareTo(other);
        }
    }
}
