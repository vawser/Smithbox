using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.Editors.GraphicsEditor.GparamParamBank;

namespace StudioCore.Editors.MapEditor.EntryFileList;
public static class EntryFileListBank
{
    public static bool IsLoaded { get; private set; }
    public static bool IsLoading { get; private set; }

    public static SortedDictionary<string, EntryFileListInfo> Bank { get; set; }

    public static void SaveEntryFileLists()
    {
        foreach (var (name, info) in Bank)
        {
            if (info.WasModified)
            {
                SaveEntryFileList(info);
                info.WasModified = false;
            }
        }
    }

    public static void SaveEntryFileList(EntryFileListInfo info)
    {
        ENFL list = info.EntryFileList;

        if (list == null)
            return;

        TaskLogs.AddLog($"SaveEntryFileList: {info.Path}");

        byte[] fileBytes = null;

        fileBytes = list.Write(DCX.Type.None);

        var paramDir = $@"\map\entryfilelist\{info.FolderName}\";
        var paramExt = @".entryfilelist";

        var assetRoot = $@"{Smithbox.GameRoot}\{paramDir}\{info.Name}{paramExt}";
        var assetMod = $@"{Smithbox.ProjectRoot}\{paramDir}\{info.Name}{paramExt}";

        // Add folder if it does not exist in GameModDirectory
        if (!Directory.Exists($"{Smithbox.ProjectRoot}\\{paramDir}\\"))
        {
            Directory.CreateDirectory($"{Smithbox.ProjectRoot}\\{paramDir}\\");
        }

        // Make a backup of the original file if a mod path doesn't exist
        if (Smithbox.ProjectRoot == null && !File.Exists($@"{assetRoot}.bak") && File.Exists(assetRoot))
        {
            File.Copy(assetRoot, $@"{assetRoot}.bak", true);
        }

        if (fileBytes != null)
        {
            // Write to GameModDirectory
            File.WriteAllBytes(assetMod, fileBytes);
            TaskLogs.AddLog($"Saved at: {assetMod}");
        }

        TaskLogs.AddLog($"Saved {info.Name} to {assetMod}");
    }

    public static void LoadEntryFileLists()
    {
        IsLoaded = false;
        IsLoading = true;

        Bank = new();

        var paramDir = @"\map\entryfilelist";
        var paramExt = @".entryfilelist";

        foreach (var folder in GetEntryListFolderNames())
        {
            var cFolder = folder;

            foreach (var name in GetEntryListFileNames(cFolder))
            {
                var filePath = $"{paramDir}\\{cFolder}\\{name}{paramExt}";

                if (File.Exists($"{Smithbox.ProjectRoot}\\{filePath}"))
                {
                    LoadEntryFileList($"{Smithbox.ProjectRoot}\\{filePath}", true, cFolder);
                    //TaskLogs.AddLog($"Loaded from GameModDirectory: {filePath}");
                }
                else
                {
                    LoadEntryFileList($"{Smithbox.GameRoot}\\{filePath}", false, cFolder);
                    //TaskLogs.AddLog($"Loaded from GameRootDirectory: {filePath}");
                }
            }
        }

        IsLoaded = true;
        IsLoading = false;

        TaskLogs.AddLog($"Entry File List Bank - Load Complete");
    }

    private static void LoadEntryFileList(string path, bool isModFile, string folder)
    {
        if (path == null)
        {
            TaskLogs.AddLog($"Could not locate {path} when loading EntryFileList file.",
                    LogLevel.Warning);
            return;
        }
        if (path == "")
        {
            TaskLogs.AddLog($"Could not locate {path} when loading EntryFileList file.",
                    LogLevel.Warning);
            return;
        }

        var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(path));

        EntryFileListInfo iStruct = new EntryFileListInfo(name, path);
        iStruct.EntryFileList = new ENFL();
        iStruct.IsModFile = isModFile;
        iStruct.FolderName = folder;

        iStruct.EntryFileList = ENFL.Read(path);

        Bank.Add(name, iStruct);
    }

    public static List<string> GetEntryListFolderNames()
    {
        var paramDir = @"\map\entryfilelist";
        var paramExt = @".entryfilelist";

        HashSet<string> folderNames = new();
        List<string> ret = new();

        TaskLogs.AddLog($"{Smithbox.GameRoot + paramDir}");

        // ROOT
        var fNames = Directory.GetDirectories(Smithbox.GameRoot + paramDir).ToList();
        foreach (var f in fNames)
        {
            var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));
            ret.Add(name);
            folderNames.Add(name);
        }

        // MOD
        if (Smithbox.ProjectRoot != null && Directory.Exists(Smithbox.ProjectRoot + paramDir))
        {
            fNames = Directory.GetDirectories(Smithbox.ProjectRoot + paramDir).ToList();

            foreach (var f in fNames)
            {
                var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));

                if (!folderNames.Contains(name))
                {
                    ret.Add(name);
                    folderNames.Add(name);
                }
            }
        }

        return ret;
    }

    public static List<string> GetEntryListFileNames(string folder)
    {
        var paramDir = $@"\map\entryfilelist\{folder}";
        var paramExt = @".entryfilelist";

        HashSet<string> fileNames = new();
        List<string> ret = new();

        // ROOT
        var fNames = Directory.GetFileSystemEntries(Smithbox.GameRoot + paramDir, $@"*{paramExt}").ToList();
        foreach (var f in fNames)
        {
            var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));
            ret.Add(name);
            fileNames.Add(name);
        }

        // MOD
        if (Smithbox.ProjectRoot != null && Directory.Exists(Smithbox.ProjectRoot + paramDir))
        {
            fNames = Directory.GetFileSystemEntries(Smithbox.ProjectRoot + paramDir, $@"*{paramExt}").ToList();

            foreach (var f in fNames)
            {
                var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));

                if (!fileNames.Contains(name))
                {
                    ret.Add(name);
                    fileNames.Add(name);
                }
            }
        }

        return ret;
    }
    public class EntryFileListInfo : IComparable<string>
    {
        public EntryFileListInfo(string name, string path)
        {
            Name = name;
            Path = path;
            WasModified = false;
        }

        public ENFL EntryFileList { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }

        public string FolderName { get; set; }
        public string EntityID { get; set; }

        public bool IsModFile { get; set; }

        public bool WasModified { get; set; }

        public int CompareTo(string other)
        {
            return Name.CompareTo(other);
        }
    }
}
