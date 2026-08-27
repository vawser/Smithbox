using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using StudioCore.Application;
using StudioCore.Logger;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace StudioCore.Editors.FileBrowser;

public class FileUnpackerTool
{
    public FileEditorView Parent;
    public ProjectEntry Project;

    public string UnpackDirectory = "";
    public bool IsUnpacking = false;
    public int TotalToUnpack = 0;
    public int CurrentUnpacked = 0;

    private List<(string Path, string Error)> FailedUnpackEntries = new();

    private const int MaxConcurrentUnpacks = 6;

    private FileDictionary BaseFileDictionary = new FileDictionary();
    private Dictionary<string, bool> SelectiveFolderDict = new();
    public List<string> TopFolderList = new();

    public bool IsDeleting = false;
    public int TotalToDelete = 0;
    public int CurrentDeleted = 0;

    public CancellationTokenSource CancelToken;

    public FileUnpackerTool(FileEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;

        UnpackDirectory = project.Descriptor.DataPath;
        if (CFG.Current.UnpackDirectory != "")
            UnpackDirectory = CFG.Current.UnpackDirectory;

        UpdateBaseFileDictionary();
    }

    public void Display()
    {
        ImGui.BeginChild("GameUnpackerToolSection", ImGuiChildFlags.Borders);

        var windowWidth = ImGui.GetWindowWidth() * 0.95f;

        GUI.WrappedText(LOC.Get("FILE_FileUnpacker_Hint"));

        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("FILE_FileUnpacker_Unpack_Dir_Header"),
            LOC.Get("FILE_FileUnpacker_Unpack_Dir_Header_TT"));

        GUI.HintTextInput("##unpackDirectory", ref UnpackDirectory, LOC.Get("FILE_FileUnpacker_Unpack_Hint"));
        if(ImGui.IsItemDeactivatedAfterEdit())
        {
            CFG.Current.UnpackDirectory = UnpackDirectory;
        }

        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("FILE_FileUnpacker_Actions_Header"),
            LOC.Get("FILE_FileUnpacker_Actions_Header_TT"));

        GUI.MultiButtonInput("unpackActions",
            "setUnpackDirectory", 
            LOC.Get("FILE_FileUnpacker_Set_Unpack_Dir"),
            LOC.Get("FILE_FileUnpacker_Set_Unpack_Dir_TT"),
            ConfigureUnpackDirectory,

            "unpackGame",
            LOC.Get("FILE_FileUnpacker_Unpack"),
            LOC.Get("FILE_FileUnpacker_Unpack_TT"), 
            UnpackGameAction,

            "deleteUnpackedFiles",
            LOC.Get("FILE_FileUnpacker_Delete"),
            LOC.Get("FILE_FileUnpacker_Delete_TT"), 
            DeleteUnpackedFilesAction);

        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("FILE_FileUnpacker_Selective_Unpack_Header"),
            LOC.Get("FILE_FileUnpacker_Selective_Unpack_Header_TT"));

        GUI.MultiButtonInput("selectiveUnpackActions",
            "toggleOptions",
            LOC.Get("FILE_FileUnpacker_Selective_Toggle_All"),
            LOC.Get("FILE_FileUnpacker_Selective_Toggle_All_TT"),
            ToggleSelectiveUnpackOptions);

        // Toggles
        ImGui.BeginChild("ToggleSection", new Vector2(0, 400), ImGuiChildFlags.Borders);
        foreach (var entry in SelectiveFolderDict)
        {
            var curToggle = entry.Value;
            ImGui.Checkbox($"{entry.Key}##toggleFolder_{entry.Key}", ref curToggle);
            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                SelectiveFolderDict[entry.Key] = curToggle;
            }
        }
        ImGui.EndChild();

        // Progress
        if (IsUnpacking)
        {
            float progress = TotalToUnpack > 0 ? (float)CurrentUnpacked / TotalToUnpack : 0f;
            string label = LOC.Get("FILE_FileUnpacker_Unpacking_Status", CurrentUnpacked, TotalToUnpack);
            ImGui.ProgressBar(progress, DPI.WholeWidthButton(windowWidth, 24), label);

            GUI.MultiButtonInput("cancelUnpackActions",
                "cancelUnpack",
                LOC.Get("FILE_FileUnpacker_Cancel_Unpack"),
                LOC.Get("FILE_FileUnpacker_Cancel_Unpack_TT"),
                CancelUnpack);
        }

        if (IsDeleting)
        {
            float progress = TotalToDelete > 0 ? (float)CurrentDeleted / TotalToDelete : 0f;
            string label = LOC.Get("FILE_FileUnpacker_Deleting_Status", CurrentDeleted, TotalToDelete);
            ImGui.ProgressBar(progress, DPI.WholeWidthButton(windowWidth, 24), label);

            GUI.MultiButtonInput("cancelDeleteActions",
                "cancelDelete",
                LOC.Get("FILE_FileUnpacker_Cancel_Delete"),
                LOC.Get("FILE_FileUnpacker_Cancel_Delete_TT"),
                CancelDelete);
        }

        if (!IsUnpacking && FailedUnpackEntries.Count > 0)
        {
            ImGui.BeginChild("FailedFilesChild", new Vector2(0, 0), ImGuiChildFlags.Borders);

            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("FILE_FileUnpacker_Failed_Unpack_Header"),
                LOC.Get("FILE_FileUnpacker_Failed_Unpack_Header_TT"));

            foreach (var (path, error) in FailedUnpackEntries)
            {
                ImGui.TextColored(new Vector4(1f, 0.3f, 0.3f, 1f), path);
                ImGui.PushTextWrapPos();
                ImGui.TextWrapped($"  - {error}");
                ImGui.PopTextWrapPos();
            }

            ImGui.EndChild();
        }

        ImGui.EndChild();
    }

    public void CancelUnpack()
    {
        CancelToken?.Cancel();
    }

    public void CancelDelete()
    {
        CancelToken?.Cancel();
    }

    public void ConfigureUnpackDirectory()
    {
        var unpackDirectory = "";
        var result = PlatformUtils.Instance.OpenFolderDialog(
            LOC.Get("FILE_FileUnpacker_Select_Unpack_Dir"), out unpackDirectory);

        if (result)
        {
            UnpackDirectory = unpackDirectory;
            CFG.Current.UnpackDirectory = unpackDirectory;
        }
    }

    public void ToggleSelectiveUnpackOptions()
    {
        foreach (var entry in SelectiveFolderDict)
        {
            SelectiveFolderDict[entry.Key] = !SelectiveFolderDict[entry.Key];
        }
    }

    public void UnpackGameAction()
    {
        if(UnpackDirectory == "")
        {
            Smithbox.Log<FileUnpackerTool>(
                LOC.Get("FILE_FileUnpacker_Unpack_Dir_Not_Set"));
            return;
        }

        if (IsUnpacking)
        {
            Smithbox.Log<FileUnpackerTool>(
                LOC.Get("FILE_FileUnpacker_Game_Data_Already_Unpacked"));
            return;
        }

        IsUnpacking = true;

        FailedUnpackEntries.Clear();

        bool IsFolderSelected(string folder)
        {
            foreach (var entry in SelectiveFolderDict)
            {
                if (!entry.Value)
                    continue;

                if (folder.StartsWith(entry.Key))
                {
                    return true;
                }
            }

            return false;
        }

        var newFileDictionary = new FileDictionary();
        newFileDictionary.Entries = BaseFileDictionary.Entries
            .Where(e => IsFolderSelected(e.Folder)).ToHashSet();

        _ = UnpackGameAsync(newFileDictionary);
    }

    public void DeleteUnpackedFilesAction()
    {
        if (IsDeleting)
        {
            Smithbox.Log<FileUnpackerTool>(
                LOC.Get("FILE_FileUnpacker_Game_Files_Being_Deleted"));
            return;
        }

        IsDeleting = true;

        _ = DeleteUnpackedDataAsync();

        // Delete the empty folders
        foreach (var entry in TopFolderList)
        {
            var absFolder = $@"{Project.Descriptor.DataPath}{entry}";

            if (Directory.Exists(absFolder))
            {
                try
                {
                    Directory.Delete(absFolder, true);
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, 
                        LOC.Get("FILE_FileUnpacker_Failed_Folder_Delete", absFolder), e);
                }
            }
        }
    }

    public bool HasUnpackedGame()
    {
        bool anyExist = false;

        var unpackPath = Project.Descriptor.DataPath;
        if (UnpackDirectory != "")
            unpackPath = UnpackDirectory;

        foreach (var folderName in TopFolderList)
        {
            string fullPath = $@"{unpackPath}/{folderName}";

            if (Directory.Exists(fullPath))
            {
                anyExist = true;
                break;
            }
        }

        return anyExist;
    }

    public async Task UnpackGameAsync(FileDictionary targetFileDictionary)
    {
        IsUnpacking = true;
        CancelToken = new CancellationTokenSource();
        var token = CancelToken.Token;

        FailedUnpackEntries.Clear();

        TotalToUnpack = targetFileDictionary.Entries.Count;
        CurrentUnpacked = 0;

        var semaphore = new SemaphoreSlim(MaxConcurrentUnpacks);
        var tasks = new List<Task>();

        foreach (var entry in targetFileDictionary.Entries)
        {
            tasks.Add(Task.Run(async () =>
            {
                await semaphore.WaitAsync(token).ConfigureAwait(false);
                try
                {
                    if (token.IsCancellationRequested)
                        return;

                    var data = Project.VFS.VanillaFS.ReadFile(entry.Path);
                    if (data != null)
                    {
                        var unpackPath = UnpackDirectory != "" ? UnpackDirectory : Project.Descriptor.DataPath;

                        var rawData = (Memory<byte>)data;
                        var absFolder = $@"{unpackPath}/{entry.Folder}";
                        var absPath = $@"{unpackPath}/{entry.Path}";

                        if (!Directory.Exists(absFolder))
                            Directory.CreateDirectory(absFolder);

                        if (!File.Exists(absPath))
                        {
                            File.WriteAllBytes(absPath, rawData.ToArray());
                            data = null;
                            rawData = null;
                        }

                        Interlocked.Increment(ref CurrentUnpacked);
                    }
                    else
                    {
                        Smithbox.LogError(this,
                            LOC.Get("FILE_FileUnpacker_Failed_File_Write", entry.Path));

                        lock (FailedUnpackEntries)
                            FailedUnpackEntries.Add((entry.Path, LOC.Get("FILE_FileUnpacker_Failed_Unpack_Entry")));

                        Interlocked.Increment(ref CurrentUnpacked);
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            }, token));
        }

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException)
        {
            Smithbox.Log(this, 
                LOC.Get("FILE_FileUnpacker_Unpack_Cancelled"), LogLevel.Warning);
        }
        catch (AggregateException ae) when (ae.InnerExceptions.All(e => e is OperationCanceledException))
        {
            Smithbox.Log(this,
                LOC.Get("FILE_FileUnpacker_Unpack_Cancelled"), LogLevel.Warning);
        }

        IsUnpacking = false;
        CancelToken = null;
    }

    public async Task DeleteUnpackedDataAsync()
    {
        IsDeleting = true;
        CancelToken = new CancellationTokenSource();
        var token = CancelToken.Token;

        TotalToDelete = BaseFileDictionary.Entries.Count;
        CurrentDeleted = 0;

        var semaphore = new SemaphoreSlim(MaxConcurrentUnpacks);
        var tasks = new List<Task>();

        foreach (var entry in BaseFileDictionary.Entries)
        {
            tasks.Add(Task.Run(async () =>
            {
                await semaphore.WaitAsync(token).ConfigureAwait(false);
                try
                {
                    if (token.IsCancellationRequested)
                        return;

                    var unpackPath = UnpackDirectory != "" ? UnpackDirectory : Project.Descriptor.DataPath;
                    var absPath = $@"{unpackPath}/{entry.Path}";

                    if (File.Exists(absPath))
                        File.Delete(absPath);

                    Interlocked.Increment(ref CurrentDeleted);
                }
                finally
                {
                    semaphore.Release();
                }
            }, token));
        }

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException)
        {
            Smithbox.Log(this,
                LOC.Get("FILE_FileUnpacker_Delete_Cancelled"), LogLevel.Warning);
        }
        catch (AggregateException ae) when (ae.InnerExceptions.All(e => e is OperationCanceledException))
        {
            Smithbox.Log(this,
                LOC.Get("FILE_FileUnpacker_Delete_Cancelled"), LogLevel.Warning);
        }

        IsDeleting = false;
        CancelToken = null;
    }

    public void UpdateBaseFileDictionary()
    {
        // Get the unmerged base file dictionary
        var folder = Path.Join(AppContext.BaseDirectory, "Assets", "File Dictionaries");
        var file = "";

        switch (Project.Descriptor.ProjectType)
        {
            case ProjectType.DES:
                file = "DES-File-Dictionary.json"; break;
            case ProjectType.DS1:
                file = "DS1-File-Dictionary.json"; break;
            case ProjectType.DS1R:
                file = "DS1R-File-Dictionary.json"; break;
            case ProjectType.DS2:
                file = "DS2-File-Dictionary.json"; break;
            case ProjectType.DS2S:
                file = "DS2S-File-Dictionary.json"; break;
            case ProjectType.DS3:
                file = "DS3-File-Dictionary.json"; break;
            case ProjectType.BB:
                file = "BB-File-Dictionary.json"; break;
            case ProjectType.SDT:
                file = "SDT-File-Dictionary.json"; break;
            case ProjectType.ER:
                file = "ER-File-Dictionary.json"; break;
            case ProjectType.AC6:
                file = "AC6-File-Dictionary.json"; break;
            case ProjectType.NR:
                file = "NR-File-Dictionary.json"; break;
            default: break;
        }

        var filepath = Path.Join(folder, file);

        var baseFileDictionary = new FileDictionary
        {
            Entries = new()
        };

        if (File.Exists(filepath))
        {
            try
            {
                var filestring = File.ReadAllText(filepath);

                try
                {
                    baseFileDictionary = JsonSerializer.Deserialize(filestring, ProjectJsonSerializerContext.Default.FileDictionary);
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, 
                        LOC.Get("FILE_FileUnpacker_Failed_Deserialize_File_Dict", filepath), e);
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this,
                    LOC.Get("FILE_FileUnpacker_Failed_Read_File_Dict", filepath), e);
            }
        }

        BaseFileDictionary = baseFileDictionary;

        SelectiveFolderDict.Clear();
        TopFolderList = new();

        foreach (var entry in baseFileDictionary.Entries)
        {
            var parts = entry.Folder.Split("/");

            if (parts.Length > 1)
            {
                var topFolder = $"/{parts[1]}";

                if (topFolder != "/")
                {
                    if (!TopFolderList.Contains(topFolder))
                    {
                        TopFolderList.Add(topFolder);

                        if (!SelectiveFolderDict.ContainsKey(topFolder))
                        {
                            SelectiveFolderDict.Add(topFolder, true);
                        }
                    }
                }
            }
        }
    }
}
