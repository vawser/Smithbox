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

public class FileUnpackTool
{
    public FileEditorView Parent;
    public ProjectEntry Project;

    public string UnpackDirectory = "";
    public bool IsUnpacking = false;
    public int TotalToUnpack = 0;
    public int CurrentUnpacked = 0;

    private List<(string Path, string Error)> FailedUnpackEntries = new();
    private CancellationTokenSource unpackCts = null;

    private const int MaxConcurrentUnpacks = 6;

    private FileDictionary BaseFileDictionary = new FileDictionary();
    private Dictionary<string, bool> SelectiveFolderDict = new();
    public List<string> TopFolderList = new();

    public bool IsDeleting = false;
    public int TotalToDelete = 0;
    public int CurrentDeleted = 0;

    public FileUnpackTool(FileEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;

        UpdateBaseFileDictionary();
    }

    public void Display()
    {
        ImGui.BeginChild("GameUnpackerToolSection", ImGuiChildFlags.Borders);

        var windowWidth = ImGui.GetWindowWidth() * 0.95f;

        UIHelper.WrappedText("This is a tool to unpack the base game data for the game this project targets, if it has not already been unpacked.");
        UIHelper.WrappedText("");

        UIHelper.SimpleHeader("Actions", "");

        UIHelper.MultiButtonInput("unpackActions",
            "setUnpackDirectory", "Configure Unpack Directory", "", ConfigureUnpackDirectory,
            "unpackGame", "Unpack Game Files", "", UnpackGameAction,
            "rebuildFileDir", "Refresh File Directory", "", UpdateBaseFileDictionary,
            "deleteUnpackedFiles", "Delete Unpacked Game Files", "", DeleteUnpackedFilesAction);

        UIHelper.WrappedText("");
        UIHelper.SimpleHeader("Selective Unpack", "");

        UIHelper.MultiButtonInput("selectiveUnpackActions",
            "toggleOptions", "Toggle All", "", ToggleSelectiveUnpackOptions);

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
            string label = $"Unpacking... {CurrentUnpacked} / {TotalToUnpack} files";
            ImGui.ProgressBar(progress, DPI.WholeWidthButton(windowWidth, 24), label);

            if (ImGui.Button("Cancel", DPI.WholeWidthButton(windowWidth, 24)))
            {
                unpackCts?.Cancel();
            }
            UIHelper.Tooltip("This will cancel the game data unpack.");
        }

        if (IsDeleting)
        {
            float progress = TotalToDelete > 0 ? (float)CurrentDeleted / TotalToDelete : 0f;
            string label = $"Deleting... {CurrentDeleted} / {TotalToDelete} files";
            ImGui.ProgressBar(progress, DPI.WholeWidthButton(windowWidth, 24), label);

            if (ImGui.Button("Cancel", DPI.WholeWidthButton(windowWidth, 24)))
            {
                unpackCts?.Cancel();
            }
            UIHelper.Tooltip("This will cancel the delete.");
        }

        if (!IsUnpacking && FailedUnpackEntries.Count > 0)
        {
            ImGui.BeginChild("FailedFilesChild", new Vector2(0, 0), ImGuiChildFlags.Borders);

            UIHelper.WrappedText("");
            UIHelper.SimpleHeader("Failed to Unpack", "");

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

    public void ConfigureUnpackDirectory()
    {
        var unpackDirectory = "";
        var result = PlatformUtils.Instance.OpenFolderDialog("Select Unpack Directory", out unpackDirectory);

        if (result)
        {
            UnpackDirectory = unpackDirectory;
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
            Smithbox.Log<FileUnpackTool>("Unpack directory has not been set.");
            return;
        }

        if (IsUnpacking)
        {
            Smithbox.Log<FileUnpackTool>("Game files are already being unpacked.");
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
            .Where(e => IsFolderSelected(e.Folder)).ToList();

        _ = UnpackGameAsync(newFileDictionary);
    }

    public void DeleteUnpackedFilesAction()
    {
        if (IsDeleting)
        {
            Smithbox.Log<FileUnpackTool>("Game files are already being deleted.");
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
                    Smithbox.LogError(this, $"[File Browser] Failed to delete folder: {absFolder}", LogPriority.High, e);
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
        unpackCts = new CancellationTokenSource();
        var token = unpackCts.Token;

        FailedUnpackEntries.Clear();

        TotalToUnpack = targetFileDictionary.Entries.Count;
        CurrentUnpacked = 0;

        var semaphore = new SemaphoreSlim(MaxConcurrentUnpacks);
        var tasks = new List<Task>();

        foreach (var entry in targetFileDictionary.Entries)
        {
            tasks.Add(Task.Run(async () =>
            {
                await semaphore.WaitAsync(token);

                if (token.IsCancellationRequested)
                    return;

                var data = Project.VFS.VanillaFS.ReadFile(entry.Path);
                if (data != null)
                {
                    var unpackPath = Project.Descriptor.DataPath;
                    if (UnpackDirectory != "")
                        unpackPath = UnpackDirectory;

                    var rawData = (Memory<byte>)data;
                    var absFolder = $@"{unpackPath}/{entry.Folder}";
                    var absPath = $@"{unpackPath}/{entry.Path}";

                    if (!Directory.Exists(absFolder))
                    {
                        Directory.CreateDirectory(absFolder);
                    }

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
                    Smithbox.LogError(this, $"[File Browser] Failed to write file: {entry.Path}", LogPriority.High);

                    lock (FailedUnpackEntries)
                    {
                        FailedUnpackEntries.Add((entry.Path, "Failed to add."));
                    }

                    Interlocked.Increment(ref CurrentUnpacked);
                }

                semaphore.Release();
            }));
        }

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException)
        {
            Smithbox.Log(this, "[File Browser] Unpacking was cancelled.", LogLevel.Warning);
        }

        IsUnpacking = false;
        unpackCts = null;
    }

    public async Task DeleteUnpackedDataAsync()
    {
        IsDeleting = true;
        unpackCts = new CancellationTokenSource();
        var token = unpackCts.Token;

        TotalToDelete = BaseFileDictionary.Entries.Count;
        CurrentDeleted = 0;

        var semaphore = new SemaphoreSlim(MaxConcurrentUnpacks);
        var tasks = new List<Task>();

        foreach (var entry in BaseFileDictionary.Entries)
        {
            tasks.Add(Task.Run(async () =>
            {
                await semaphore.WaitAsync(token);

                if (token.IsCancellationRequested)
                    return;

                var unpackPath = Project.Descriptor.DataPath;
                if (UnpackDirectory != "")
                    unpackPath = UnpackDirectory;

                var absPath = $@"{unpackPath}/{entry.Path}";

                if (File.Exists(absPath))
                {
                    File.Delete(absPath);
                }

                Interlocked.Increment(ref CurrentDeleted);

                semaphore.Release();
            }));
        }

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException)
        {
            Smithbox.Log(this, "[File Browser] Deleting was cancelled.", LogLevel.Warning);
        }

        IsDeleting = false;
        unpackCts = null;
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

        var baseFileDictionary = new FileDictionary();
        baseFileDictionary.Entries = new();

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
                    Smithbox.LogError(this, $"[File Browser] Failed to deserialize the file dictionary: {filepath}", LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, $"[File Browser] Failed to read the file dictionary: {filepath}", LogPriority.High, e);
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
