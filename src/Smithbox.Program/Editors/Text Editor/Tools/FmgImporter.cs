using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace StudioCore.Editors.TextEditor;


public class FmgImporter
{
    public TextEditorView Parent;
    public ProjectEntry Project;

    public Dictionary<string, StoredFmgContainer> ImportSources = new();

    private bool _wrappersLoaded = false;

    public FmgImporter(TextEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;
    }
    public void InvalidateWrapperCache() => _wrappersLoaded = false;

    /// <summary>
    /// Context Menu options in the Container list
    /// </summary>
    public void FileContextMenuOptions()
    {
        if (ImGui.BeginMenu("Import Text"))
        {
            DisplayImportList(FmgImportType.Container);

            ImGui.EndMenu();
        }
    }

    /// <summary>
    /// Context Menu options in the FMG list
    /// </summary>
    public void FmgContextMenuOptions()
    {
        if (ImGui.BeginMenu("Import Text"))
        {
            DisplayImportList(FmgImportType.FMG);

            ImGui.EndMenu();
        }
    }

    /// <summary>
    /// Context Menu options in the FMG Entry list
    /// </summary>
    public void FmgEntryContextMenuOptions()
    {
        if (ImGui.BeginMenu("Import Text"))
        {
            DisplayImportList(FmgImportType.FMG_Entries);

            ImGui.EndMenu();
        }
    }

    /// <summary>
    /// Display the possible import sources for the user to select from
    /// </summary>
    public void DisplayImportList(FmgImportType importType)
    {
        if(ImGui.BeginMenu("Append"))
        {
            if (ImGui.Selectable($"From external file"))
            {
                PromptExternalTextImport(ImportBehavior.Append);
            }

            ImGui.Separator();

            if (ImportSources.Count > 0)
            {
                foreach (var (key, entry) in ImportSources)
                {
                    if (ImGui.Selectable($"{entry.Name}"))
                    {
                        ImportText(entry, ImportBehavior.Append);
                    }
                }
            }
            else
            {
                ImGui.Text("No exported text exists yet.");
            }

            ImGui.EndMenu();
        }
        UIHelper.Tooltip("The selected stored text will be added to the current File.\n\nExisting entries will be NOT modified by the contents of the stored text.");

        if (ImGui.BeginMenu("Replace"))
        {
            if (ImGui.Selectable($"From external file"))
            {
                PromptExternalTextImport(ImportBehavior.Replace);
            }

            ImGui.Separator();

            if (ImportSources.Count > 0)
            {
                foreach (var (key, entry) in ImportSources)
                {
                    if (ImGui.Selectable($"{entry.Name}"))
                    {
                        ImportText(entry, ImportBehavior.Replace);
                    }
                }
            }
            else
            {
                ImGui.Text("No exported text exists yet.");
            }

            ImGui.EndMenu();
        }
        UIHelper.Tooltip("The selected stored text will be added to the current Text file.\n\nExisting entries WILL be modified by the contents of the stored text.");
    }

    private List<EditorAction> ImportActions;

    public void ImportText(StoredFmgContainer containerWrapper, ImportBehavior importBehavior)
    {
        if (containerWrapper.FmgWrappers == null) 
            return;

        ImportActions = new List<EditorAction>();

        var targetContainer = Parent.Selection.SelectedContainerWrapper;
        if (targetContainer == null) 
            return;

        // Index stored wrappers by ID once
        var storedWrapperMap = containerWrapper.FmgWrappers.ToDictionary(w => w.ID);

        foreach (var fmgWrapper in targetContainer.FmgWrappers)
        {
            if (storedWrapperMap.TryGetValue(fmgWrapper.ID, out var storedFmgWrapper))
            {
                ProcessFmg(targetContainer, fmgWrapper, storedFmgWrapper, importBehavior);
            }
        }

        var groupAction = new FmgGroupedAction(ImportActions);
        Parent.ActionManager.ExecuteAction(groupAction);
    }

    private void ProcessFmg(
        TextContainerWrapper containerWrapper, 
        TextFmgWrapper fmgWrapper, 
        StoredFmgWrapper storedWrapper, 
        ImportBehavior importBehavior)
    {
        var targetEntryMap = fmgWrapper.File.Entries.ToDictionary(e => e.ID);

        foreach (var storedEntry in storedWrapper.Fmg.Entries)
        {
            storedEntry.Parent = fmgWrapper.File;

            if (!targetEntryMap.TryGetValue(storedEntry.ID, out var targetEntry))
            {
                ImportActions.Add(new AddFmgEntry(Parent, containerWrapper, storedEntry, storedEntry, storedEntry.ID, true));
            }
            else if (importBehavior is not ImportBehavior.Append)
            {
                ImportActions.Add(new ChangeFmgEntryText(Parent, containerWrapper, targetEntry, storedEntry.Text, false, true));
            }
        }
    }

    public void OnGui()
    {
        LoadWrappers();
    }

    public void LoadWrappers()
    {
        if (_wrappersLoaded) 
            return;

        ImportSources = new();

        var wrapperPathList = TextUtils.GetStoredContainerWrappers(Project);

        if (wrapperPathList.Count > 0)
        {
            foreach (var path in wrapperPathList)
            {
                var filename = Path.GetFileName(path);
                var wrapper = new StoredFmgContainer();

                if (File.Exists(path))
                {
                    using (var stream = File.OpenRead(path))
                    {
                        try
                        {
                            wrapper = JsonSerializer.Deserialize(stream, StoredContainerWrapperSerializationContext.Default.StoredFmgContainer);
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }

                if (!ImportSources.ContainsKey(filename))
                {
                    ImportSources.Add(filename, wrapper);
                }
                else
                {
                    Smithbox.Log(this, $"Attempted to add stored text with existing key: {filename}");
                }
            }
        }

        _wrappersLoaded = true;
    }

    public StoredFmgContainer GenerateStoredFmgContainerFromJson(string jsonString)
    {
        var wrapper = new StoredFmgContainer();
        try
        {
            wrapper = JsonSerializer.Deserialize(jsonString, StoredContainerWrapperSerializationContext.Default.StoredFmgContainer);
        }
        catch (Exception e)
        {
            Smithbox.Log(this, $"Failed to read JSON file string\n{e.Message}", LogLevel.Warning);
        }

        return wrapper;
    }

    public StoredFmgContainer GenerateStoredFmgContainerFromFile(string path)
    {
        var filename = Path.GetFileName(path);
        var wrapper = new StoredFmgContainer();

        if (File.Exists(path))
        {
            using (var stream = File.OpenRead(path))
            {
                try
                {
                    wrapper = JsonSerializer.Deserialize(stream, StoredContainerWrapperSerializationContext.Default.StoredFmgContainer);
                }
                catch(Exception e)
                {
                    Smithbox.Log(this, $"Failed to read JSON file: {filename} at {path}\n{e.Message}", LogLevel.Warning);
                }

                return wrapper;
            }
        }

        return wrapper;
    }

    private void PromptExternalTextImport(ImportBehavior type)
    {
        if (PlatformUtils.Instance.OpenFileDialog("Select stored text JSON", ["json"], out var path))
        {
            if (!File.Exists(path))
            {
                DialogResult message = PlatformUtils.Instance.MessageBox(
                    "Selected file is invalid.", "Error",
                    MessageBoxButtons.OK);
                return;
            }

            var generatedStoredFmgContainer = GenerateStoredFmgContainerFromFile(path);
            if (generatedStoredFmgContainer != null)
            {
                ImportText(generatedStoredFmgContainer, type);
            }
        }
    }
}

