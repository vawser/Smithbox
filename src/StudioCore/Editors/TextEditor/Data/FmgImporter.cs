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
    public static Dictionary<string, StoredFmgWrapper> ImportSources = new();

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
                var wrapper = new StoredFmgWrapper();

                if (File.Exists(path))
                {
                    using (var stream = File.OpenRead(path))
                    {
                        wrapper = JsonSerializer.Deserialize(stream, StoredFmgWrapperSerializationContext.Default.StoredFmgWrapper);
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
            if (ImportSources.Count > 0)
            {
                foreach (var (key, entry) in ImportSources)
                {
                    if (ImGui.Selectable($"{entry.Name}"))
                    {
                        AppendEntries(entry);
                    }
                }
            }
            else
            {
                ImGui.Text("No exported FMG wrappers exist yet.");
            }

            ImGui.EndMenu();
        }
        UIHelper.ShowHoverTooltip("Append the selected wrapper contents to the currently selected FMG entries.\n\nExisting entries will NOT be modified.");

        if (ImGui.BeginMenu("Replace"))
        {
            if (ImportSources.Count > 0)
            {
                foreach (var (key, entry) in ImportSources)
                {
                    if (ImGui.Selectable($"{entry.Name}"))
                    {
                        ReplaceEntries(entry);
                    }
                }
            }
            else
            {
                ImGui.Text("No exported FMG wrappers exist yet.");
            }

            ImGui.EndMenu();
        }
        UIHelper.ShowHoverTooltip("Replace the selected wrapper contents to the currently selected FMG entries.\n\nExisting entries will be modified by the contents of the wrapper.");

        if (ImGui.BeginMenu("Overwrite"))
        {
            if (ImportSources.Count > 0)
            {
                foreach (var (key, entry) in ImportSources)
                {
                    if (ImGui.Selectable($"{entry.Name}"))
                    {
                        OverwriteEntries(entry);
                    }
                }
            }
            else
            {
                ImGui.Text("No exported FMG wrappers exist yet.");
            }

            ImGui.EndMenu();
        }
        UIHelper.ShowHoverTooltip("Overwrite the entire FMG entry list with the selected wrapper contents.");
    }

    /// <summary>
    /// Append contents of the selected source to the contents of the currently selected FMG (respecting ID order)
    /// </summary>
    public static void AppendEntries(StoredFmgWrapper wrapper)
    {
        var editor = Smithbox.EditorHandler.TextEditor;

        var selectedFmgInfo = editor.Selection.SelectedFmgInfo;

        var action = new AppendFmgEntries(selectedFmgInfo, wrapper);
        editor.EditorActionManager.ExecuteAction(action);

        TaskLogs.AddLog($"Imported FMG Wrapper {wrapper.Name}, appending current entries.");
    }

    /// <summary>
    /// Replace the contents of the currently selected FMG with the contents of the selected source if they match, 
    /// or append otherwise
    /// </summary>
    public static void ReplaceEntries(StoredFmgWrapper wrapper)
    {
        var editor = Smithbox.EditorHandler.TextEditor;

        var selectedFmgInfo = editor.Selection.SelectedFmgInfo;

        var action = new ReplaceFmgEntries(selectedFmgInfo, wrapper);
        editor.EditorActionManager.ExecuteAction(action);

        TaskLogs.AddLog($"Imported FMG Wrapper {wrapper.Name}, replacing current entries.");
    }

    /// <summary>
    /// Overwrite the contents of the currently selected FMG with the contents of the selected source if they match, 
    /// or append otherwise.
    /// </summary>
    public static void OverwriteEntries(StoredFmgWrapper wrapper)
    {
        var editor = Smithbox.EditorHandler.TextEditor;

        var selectedFmgInfo = editor.Selection.SelectedFmgInfo;

        var action = new OverwriteFmgEntries(selectedFmgInfo, wrapper);
        editor.EditorActionManager.ExecuteAction(action);

        TaskLogs.AddLog($"Imported FMG Wrapper {wrapper.Name}, replacing current entries.");
    }
}

