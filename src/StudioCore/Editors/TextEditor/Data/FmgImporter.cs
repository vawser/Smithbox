using ImGuiNET;
using SoulsFormats;
using StudioCore.Banks.AliasBank;
using StudioCore.Editors.TextEditor.Actions;
using StudioCore.Interface;
using StudioCore.Resource.Locators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;


public static class FmgImporter
{
    public static Dictionary<string, FmgWrapper> ImportSources = new();

    /// <summary>
    /// Get the FMG Wrapper sources on project load
    /// </summary>
    public static void OnProjectChanged()
    {
        LoadWrappers();
    }

    /// <summary>
    /// Load the wrappers into the FmgWrapper object and fill the ImportSources dictionary
    /// </summary>
    private static void LoadWrappers()
    {
        ImportSources = new();

        var wrapperPathList = TextLocator.GetFmgWrappers();

        if (wrapperPathList.Count > 0)
        {
            foreach (var path in wrapperPathList)
            {
                var filename = Path.GetFileName(path);
                var wrapper = new FmgWrapper();

                if (File.Exists(path))
                {
                    using (var stream = File.OpenRead(path))
                    {
                        wrapper = JsonSerializer.Deserialize(stream, FmgWrapperSerializationContext.Default.FmgWrapper);
                    }
                }

                if(!ImportSources.ContainsKey(filename))
                {
                    ImportSources.Add(filename, wrapper);
                }
                else
                {
                    TaskLogs.AddLog($"Attempted to add FmgWrapper with existing key!: {filename}");
                }
            }
        }
    }

    /// <summary>
    /// Display the possible import sources for the user to select from
    /// </summary>
    public static void DisplayImportList()
    {
        LoadWrappers();

        if(ImGui.BeginMenu("Append"))
        {
            foreach(var (key, entry) in ImportSources)
            {
                if(ImGui.Selectable($"{entry.Name}"))
                {
                    AppendEntries(entry);
                }
            }

            ImGui.EndMenu();
        }
        UIHelper.ShowHoverTooltip("Append the selected FMG Wrapper contents to the currently selected FMG entries. Existing entries that match Wrapper entries will be overwritten.");

        if (ImGui.BeginMenu("Replace"))
        {
            foreach (var (key, entry) in ImportSources)
            {
                if (ImGui.Selectable($"{entry.Name}"))
                {
                    ReplaceEntries(entry);
                }
            }

            ImGui.EndMenu();
        }
        UIHelper.ShowHoverTooltip("Replace the currently selected FMG entries with the contents of the selected FMG Wrapper entirely.");

        if (ImGui.BeginMenu("Unique Insert"))
        {
            foreach (var (key, entry) in ImportSources)
            {
                if (ImGui.Selectable($"{entry.Name}"))
                {
                    InsertUniqueEntries(entry);
                }
            }

            ImGui.EndMenu();
        }
        UIHelper.ShowHoverTooltip("Insert the selected FMG Wrapper contents into the currently selected FMG entires, but only if they are unique rows. Non-unique rows are ignored.");
    }

    /// <summary>
    /// Append contents of the selected source to the contents of the currently selected FMG (respecting ID order)
    /// </summary>
    public static void AppendEntries(FmgWrapper wrapper)
    {
        var editor = Smithbox.EditorHandler.TextEditor;

        var selectedFmgInfo = editor.Selection.SelectedFmgInfo;

        var action = new AppendFmgEntries(selectedFmgInfo, wrapper);
        editor.EditorActionManager.ExecuteAction(action);

        TaskLogs.AddLog($"Imported FMG Wrapper {wrapper.Name}, appending current entries.");
    }

    /// <summary>
    /// Replace the contents of the currently selected FMG with the contents of the selected source
    /// </summary>
    public static void ReplaceEntries(FmgWrapper wrapper)
    {
        var editor = Smithbox.EditorHandler.TextEditor;

        var selectedFmgInfo = editor.Selection.SelectedFmgInfo;

        var action = new ReplaceFmgEntries(selectedFmgInfo, wrapper);
        editor.EditorActionManager.ExecuteAction(action);

        TaskLogs.AddLog($"Imported FMG Wrapper {wrapper.Name}, replacing current entries.");
    }

    /// <summary>
    /// Insert contents of the selected source if they are not present in 
    /// the contents of the currently selected FMG (respecting ID order) 
    /// </summary>
    public static void InsertUniqueEntries(FmgWrapper wrapper)
    {
        var editor = Smithbox.EditorHandler.TextEditor;

        var selectedFmgInfo = editor.Selection.SelectedFmgInfo;

        var action = new InsertUniqueFmgEntries(selectedFmgInfo, wrapper);
        editor.EditorActionManager.ExecuteAction(action);

        TaskLogs.AddLog($"Imported FMG Wrapper {wrapper.Name}, inserting only unique entries from the wrapper.");
    }
}

