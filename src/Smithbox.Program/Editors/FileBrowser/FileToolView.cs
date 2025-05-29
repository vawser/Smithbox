using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using Silk.NET.SDL;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Formats.JSON;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace StudioCore.FileBrowserNS;

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

    public FileToolView(FileBrowserScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;

        BaseFileDictionary = GetBaseFileDictionary();
    }

    public void Display()
    {
        ImGui.Begin($"Tools##FileBrowserToolView");

        if(ImGui.CollapsingHeader("Unpack Game Data"))
        {
            DisplayUnpacker();
        }

        ImGui.End();
    }

    public void DisplayUnpacker()
    {
        var width = ImGui.GetWindowWidth() * 0.95f;

        UIHelper.WrappedText("This is a tool to unpack the base game data for the game this project targets, if it has not already been unpacked.");
        UIHelper.WrappedText("");

        if (HasUnpackedGame())
        {
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, "The base game for this project has already been unpacked.");
            UIHelper.WrappedText("");

            if (ImGui.Button("Clear Unpacked Data", new Vector2(width, 24)))
            {
                ClearUnpackedData();
            }
        }
        else
        {
            UIHelper.WrappedTextColored(UI.Current.ImGui_Logger_Warning_Color, "The base game for this project has not been unpacked.");
            UIHelper.WrappedText("");
        }

        // TODO: build list of top-level folders using File Dictionary

        // TODO: add selective unpacking options

        if(!IsUnpacking)
        {
            if (ImGui.Button("Unpack Game", new Vector2(width, 24)))
            {
                IsUnpacking = true;

                _ = UnpackGameAsync(BaseFileDictionary);
            }
        }
        else if (IsUnpacking)
        {
            float progress = TotalToUnpack > 0 ? (float)CurrentUnpacked / TotalToUnpack : 0f;
            string label = $"Unpacking... {CurrentUnpacked} / {TotalToUnpack} files";
            ImGui.ProgressBar(progress, new Vector2(width, 24), label);

            if (ImGui.Button("Cancel", new Vector2(width, 24)))
            {
                unpackCts?.Cancel();
            }
        }

        if (!IsUnpacking && FailedUnpackEntries.Count > 0)
        {
            if (ImGui.CollapsingHeader("Failed Files", ImGuiTreeNodeFlags.DefaultOpen))
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
    }

    public bool HasUnpackedGame()
    {
        // TODO: check for some of the folders to detect if the game is unpacked

        return false;
    }

    public FileDictionary GetBaseFileDictionary()
    {
        // Get the unmerged base file dictionary
        var folder = @$"{AppContext.BaseDirectory}\Assets\File Dictionaries\";
        var file = "";

        switch (Project.ProjectType)
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
            default: break;
        }

        var filepath = $"{folder}{file}";

        var baseFileDictionary = new FileDictionary();
        baseFileDictionary.Entries = new();

        if (File.Exists(filepath))
        {
            try
            {
                var filestring = File.ReadAllText(filepath);

                try
                {
                    var options = new JsonSerializerOptions();
                    baseFileDictionary = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.FileDictionary);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox] Failed to deserialize the file dictionary: {filepath}", LogLevel.Error, Tasks.LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Smithbox] Failed to read the file dictionary: {filepath}", LogLevel.Error, Tasks.LogPriority.High, e);
            }
        }

        return baseFileDictionary;
    }

    public async Task UnpackGameAsync(FileDictionary curFileDictionary)
    {
        IsUnpacking = true;
        unpackCts = new CancellationTokenSource();
        var token = unpackCts.Token;

        FailedUnpackEntries.Clear();

        TotalToUnpack = Project.FileDictionary.Entries.Count;
        CurrentUnpacked = 0;

        var semaphore = new SemaphoreSlim(MaxConcurrentUnpacks);
        var tasks = new List<Task>();

        foreach (var entry in Project.FileDictionary.Entries)
        {
            tasks.Add(Task.Run(async () =>
            {
                await semaphore.WaitAsync(token);

                if (token.IsCancellationRequested) 
                    return;

                var data = Project.VanillaFS.ReadFile(entry.Path);
                if (data != null)
                {
                    var rawData = (Memory<byte>)data;
                    var absFolder = $@"{Project.DataPath}/{entry.Folder}";
                    var absPath = $@"{Project.DataPath}/{entry.Path}";

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
                    TaskLogs.AddLog($"[Smithbox] Failed to write file: {entry.Path}", LogLevel.Error, Tasks.LogPriority.High);

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

    public async void ClearUnpackedData()
    {

    }
}
