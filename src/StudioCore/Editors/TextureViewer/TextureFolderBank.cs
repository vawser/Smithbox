using Microsoft.Extensions.Logging;
using SoulsFormats;
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
        Character = 3,
        Object = 4,
        Other = 5,
        Part = 6,
        Particle = 7
    }

    public static bool IsLoaded { get; private set; }
    public static bool IsLoading { get; private set; }

    public static SortedDictionary<string, TextureViewInfo> FolderBank { get; set; }

    public static void LoadTextureFolders()
    {
        IsLoaded = false;
        IsLoading = true;

        FolderBank = new();

        // Menu
        ScanMenuFolder(TextureViewCategory.Menu);

        // Assets: AC6 and ER only
        ScanAssetFolder(TextureViewCategory.Asset);

        // Objects: Sekiro and before
        ScanObjectFolder(TextureViewCategory.Object);

        // Characters
        ScanCharacterFolder(TextureViewCategory.Character);

        // Parts
        ScanPartsFolder(TextureViewCategory.Part);

        // SFX
        ScanParticleFolder(TextureViewCategory.Particle);

        // Other
        ScanOtherFolder(TextureViewCategory.Other);

        IsLoaded = true;
        IsLoading = false;

        TaskLogs.AddLog($"Texture Folder Bank - Load Complete");
    }

    private static void ScanMenuFolder(TextureViewCategory category)
    {
        var folderDir = @"\menu";
        var fileExt = @".tpf.dcx";

        if (Project.Type is ProjectType.AC6 or ProjectType.ER or ProjectType.SDT)
        {
            folderDir = @"\menu\hi";
        }

        FindTextureFolder(folderDir, fileExt, category);

        if (Project.Type is ProjectType.DS2S)
        {
            folderDir = @"\menu\tex\icon";
            fileExt = @".tpf";

            FindTextureFolder(folderDir, fileExt, category);

            folderDir = @"\menu\tex\icon\bonfire_area";

            FindTextureFolder(folderDir, fileExt, category);

            folderDir = @"\menu\tex\icon\bonfire_list";

            FindTextureFolder(folderDir, fileExt, category);

            folderDir = @"\menu\tex\icon\charamaking";

            FindTextureFolder(folderDir, fileExt, category);

            folderDir = @"\menu\tex\icon\effect";

            FindTextureFolder(folderDir, fileExt, category);

            folderDir = @"\menu\tex\icon\item_category";

            FindTextureFolder(folderDir, fileExt, category);

            // TODO: support all the languages
            folderDir = @"\menu\tex\icon\mapname\english";

            FindTextureFolder(folderDir, fileExt, category);

            folderDir = @"\menu\tex\icon\vow";

            FindTextureFolder(folderDir, fileExt, category);
        }
    }

    private static void ScanAssetFolder(TextureViewCategory category)
    {
        var folderDir = @"";
        var fileExt = @".tpf.dcx";

        if (Project.Type is ProjectType.AC6)
        {
            folderDir = @"\asset\environment\texture";

            FindTextureFolder(folderDir, fileExt, category);
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

                    FindTextureFolder(folderDir, fileExt, category);
                }
            }
        }
    }

    private static void ScanObjectFolder(TextureViewCategory category)
    {
        var folderDir = @"\obj";
        var fileExt = @".objbnd.dcx";

        if (Project.Type == ProjectType.DS1)
        {
            fileExt = @".objbnd";
        }

        if (Project.Type == ProjectType.DS2S)
        {
            folderDir = @"\model\obj";
            fileExt = @".bnd";
        }

        FindTextureFolder(folderDir, fileExt, category);
    }

    private static void ScanCharacterFolder(TextureViewCategory category)
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

        FindTextureFolder(folderDir, fileExt, category);
    }

    private static void ScanPartsFolder(TextureViewCategory category)
    {
        var folderDir = @"\parts";
        var fileExt = @".partsbnd.dcx";

        if (Project.Type == ProjectType.DS1)
        {
            fileExt = @".partsbnd";
        }

        FindTextureFolder(folderDir, fileExt, category);

        if (Project.Type == ProjectType.DS2S)
        {
            folderDir = @"\model\parts";
            fileExt = @".commonbnd.dcx";

            FindTextureFolder(folderDir, fileExt, category);

            fileExt = @".bnd";
            folderDir = @"\model\parts";

            FindTextureFolder(folderDir, fileExt, category);

            fileExt = @".bnd";
            folderDir = @"\model\parts\accessories";

            FindTextureFolder(folderDir, fileExt, category);

            folderDir = @"\model\parts\arm";

            FindTextureFolder(folderDir, fileExt, category);

            folderDir = @"\model\parts\body";

            FindTextureFolder(folderDir, fileExt, category);

            folderDir = @"\model\parts\face";

            FindTextureFolder(folderDir, fileExt, category);

            folderDir = @"\model\parts\head";

            FindTextureFolder(folderDir, fileExt, category);

            folderDir = @"\model\parts\leg";

            FindTextureFolder(folderDir, fileExt, category);

            folderDir = @"\model\parts\shield";

            FindTextureFolder(folderDir, fileExt, category);

            folderDir = @"\model\parts\weapon";

            FindTextureFolder(folderDir, fileExt, category);
        }
    }

    private static void ScanParticleFolder(TextureViewCategory category)
    {
        var folderDir = @"\sfx";
        var fileExt = @".ffxbnd.dcx";

        FindTextureFolder(folderDir, fileExt, category);
    }

    private static void ScanOtherFolder(TextureViewCategory category)
    {
        var folderDir = @"\other";
        var fileExt = @".tpf.dcx";

        FindTextureFolder(folderDir, fileExt, category);
    }

    // General
    private static void FindTextureFolder(string folderDir, string fileExt, TextureViewCategory category)
    {
        foreach (var name in GetFileNames(folderDir, fileExt))
        {
            var filePath = $"{folderDir}\\{name}{fileExt}";

            if (File.Exists($"{Project.GameModDirectory}\\{filePath}"))
            {
                AddTextureFolder($"{Project.GameModDirectory}\\{filePath}", category, true);
            }
            else
            {
                AddTextureFolder($"{Project.GameRootDirectory}\\{filePath}", category, false);
            }
        }
    }

    private static void AddTextureFolder(string path, TextureViewCategory category, bool isModFile)
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
