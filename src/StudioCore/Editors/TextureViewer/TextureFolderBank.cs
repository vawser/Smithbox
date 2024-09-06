using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Core.Project;
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
    }

    private static void ScanMenuFolder(TextureViewCategory category)
    {
        var folderDir = @"\menu";
        var fileExt = @".tpf.dcx";

        if (Smithbox.ProjectType is ProjectType.AC6 or ProjectType.ER or ProjectType.SDT)
        {
            folderDir = @"\menu\hi";
        }

        FindTextureFolder(folderDir, fileExt, category);

        if (Smithbox.ProjectType is ProjectType.DS2S or ProjectType.DS2)
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

        if (Smithbox.ProjectType is ProjectType.AC6)
        {
            folderDir = @"\asset\environment\texture";

            FindTextureFolder(folderDir, fileExt, category);
        }

        if (Smithbox.ProjectType is ProjectType.ER)
        {
            var searchFolderDir = $@"\asset\aet\";

            if(Directory.Exists($"{Smithbox.ProjectRoot}\\{searchFolderDir}"))
            {
                searchFolderDir = $"{Smithbox.ProjectRoot}\\{searchFolderDir}";
            }
            else
            {
                searchFolderDir = $"{Smithbox.GameRoot}\\{searchFolderDir}";
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

        if (Smithbox.ProjectType == ProjectType.DS1)
        {
            fileExt = @".objbnd";
        }

        if (Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
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

        if (Smithbox.ProjectType is ProjectType.DES or ProjectType.DS1)
        {
            fileExt = ".tpf";
        }

        if (Smithbox.ProjectType is ProjectType.DS2S or ProjectType.DS2)
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

        if (Smithbox.ProjectType == ProjectType.DS1)
        {
            fileExt = @".partsbnd";
        }

        FindTextureFolder(folderDir, fileExt, category);

        if (Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
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

        if(Smithbox.ProjectType is ProjectType.ER)
        {
            folderDir = @"\parts";
            fileExt = @".tpf.dcx";

            FindTextureFolder(folderDir, fileExt, category);
        }
    }

    private static void ScanParticleFolder(TextureViewCategory category)
    {
        var folderDir = @"\sfx";
        var fileExt = @".ffxbnd.dcx";

        if(Smithbox.ProjectType is ProjectType.DS2S or ProjectType.DS2)
        {
            fileExt = @".ffxbnd";
        }

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

            if (File.Exists($"{Smithbox.ProjectRoot}\\{filePath}"))
            {
                AddTextureFolder($"{Smithbox.ProjectRoot}\\{filePath}", category, true);
            }
            else
            {
                AddTextureFolder($"{Smithbox.GameRoot}\\{filePath}", category, false);
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

        if(!FolderBank.ContainsKey(name))
            FolderBank.Add(name, tStruct);
    }

    public static List<string> GetFileNames(string fileDir, string fileExt)
    {
        HashSet<string> fileNames = new();
        List<string> ret = new();

        if (Directory.Exists(Smithbox.GameRoot + fileDir))
        {
            // ROOT
            var paramFiles = Directory.GetFileSystemEntries(Smithbox.GameRoot + fileDir, $@"*{fileExt}").ToList();
            foreach (var f in paramFiles)
            {
                var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));
                ret.Add(name);
                fileNames.Add(name);
            }

            // MOD
            if (Smithbox.ProjectRoot != null && Directory.Exists(Smithbox.ProjectRoot + fileDir))
            {
                paramFiles = Directory.GetFileSystemEntries(Smithbox.ProjectRoot + fileDir, $@"*{fileExt}").ToList();

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
        public string CachedName { get; set; }
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
