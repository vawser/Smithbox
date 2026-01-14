using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace StudioCore.Editors.FileBrowser;

public class FileToolView
{
    public FileBrowserScreen Editor;
    public ProjectEntry Project;

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

    public FileToolView(FileBrowserScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;

        UpdateBaseFileDictionary();
    }

    public void Display()
    {
        ImGui.Begin($"Tools##FileBrowserToolView", ImGuiWindowFlags.MenuBar);

        if (ImGui.BeginMenuBar())
        {
            ViewMenu();

            ImGui.EndMenuBar();
        }

        if (CFG.Current.Interface_FileBrowser_Tool_GameUnpacker)
        {
            if (ImGui.CollapsingHeader("Unpack Game Data"))
            {
                DisplayUnpacker();
            }
        }

        ImGui.End();
    }

    public void ViewMenu()
    {
        if (ImGui.BeginMenu("View"))
        {
            if (ImGui.MenuItem("Game Unpacker"))
            {
                CFG.Current.Interface_FileBrowser_Tool_GameUnpacker = !CFG.Current.Interface_FileBrowser_Tool_GameUnpacker;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_FileBrowser_Tool_GameUnpacker);

            ImGui.EndMenu();
        }
    }

    public string UnpackDirectory = "";

    public void DisplayUnpacker()
    {
        var windowWidth = ImGui.GetWindowWidth() * 0.95f;

        UIHelper.WrappedText("This is a tool to unpack the base game data for the game this project targets, if it has not already been unpacked.");
        UIHelper.WrappedText("");

        if (UnpackDirectory == "")
        {
            if (ImGui.Button("Set Unpack Directory", DPI.WholeWidthButton(windowWidth, 24)))
            {
                var unpackDirectory = "";
                var result = PlatformUtils.Instance.OpenFolderDialog("Select Unpack Directory", out unpackDirectory);

                if (result)
                {
                    UnpackDirectory = unpackDirectory;
                }
            }
            UIHelper.Tooltip("Set the unpack directory to a different path other than the game data path. This is where all the unpacked files will be placed.");
        }

        if (!HasUnpackedGame())
        {
            UIHelper.SimpleHeader("selectiveUnpack", "Selective Unpack", "Select which folders to include in the unpack.", UI.Current.ImGui_AliasName_Text);

            if(ImGui.Button($"{Icons.Bars} Toggle All", DPI.StandardButtonSize))
            {
                foreach (var entry in SelectiveFolderDict)
                {
                    SelectiveFolderDict[entry.Key] = !SelectiveFolderDict[entry.Key];
                }
            }

            foreach(var entry in SelectiveFolderDict)
            {
                var curToggle = entry.Value;
                ImGui.Checkbox($"{entry.Key}##toggleFolder_{entry.Key}", ref curToggle);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    SelectiveFolderDict[entry.Key] = curToggle;
                }
            }
        }
        else
        {
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, "The base game for this project is most likely already been unpacked.");
            UIHelper.WrappedText("");
        }

        if (!IsUnpacking && ( !HasUnpackedGame()))
        {
            if (ImGui.Button("Unpack Game", DPI.WholeWidthButton(windowWidth, 24)))
            {
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
            UIHelper.Tooltip("This will unpack the data data based on your selective filters.");
        }
        else if (IsUnpacking)
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

        if (ImGui.Button("Rebuild File Dictionary##rebuildFileDict_main", DPI.WholeWidthButton(windowWidth, 24)))
        {
            UpdateBaseFileDictionary();
        }
        UIHelper.Tooltip("This will rebuild the file dictionary from the JSON file.");

        if (!IsDeleting && HasUnpackedGame())
        {
            if (ImGui.Button("Delete Unpacked Data", DPI.WholeWidthButton(windowWidth, 24)))
            {
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
                        catch(Exception e)
                        {
                            TaskLogs.AddLog($"[Smithbox] Failed to delete folder: {absFolder}", LogLevel.Error, LogPriority.High, e);
                        }
                    }
                }
            }
            UIHelper.Tooltip("This will clear all the unpacked folders and files.");
        }
        else if(IsDeleting)
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
            ImGui.BeginChild("FailedFilesChild", new Vector2(0, 600));

            foreach (var (path, error) in FailedUnpackEntries)
            {
                ImGui.TextColored(new Vector4(1f, 0.3f, 0.3f, 1f), path);
                ImGui.PushTextWrapPos();
                ImGui.TextWrapped($"  - {error}");
                ImGui.PopTextWrapPos();
            }

            ImGui.EndChild();
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
                    TaskLogs.AddLog($"[Smithbox] Failed to write file: {entry.Path}", LogLevel.Error, LogPriority.High);

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
            TaskLogs.AddLog("[Smithbox] Unpacking was cancelled.", LogLevel.Warning);
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
            TaskLogs.AddLog("[Smithbox] Deleting was cancelled.", LogLevel.Warning);
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
                    TaskLogs.AddLog($"[Smithbox] Failed to deserialize the file dictionary: {filepath}", LogLevel.Error, LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Smithbox] Failed to read the file dictionary: {filepath}", LogLevel.Error, LogPriority.High, e);
            }
        }

        BaseFileDictionary = baseFileDictionary;

        SelectiveFolderDict.Clear();
        TopFolderList = new();

        foreach (var entry in baseFileDictionary.Entries)
        {
            var parts = entry.Folder.Split("/");

            if(parts.Length > 1)
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
