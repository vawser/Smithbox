using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.TextEditor.Enums;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Resource.Locators;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace StudioCore.Editors.TextEditor;


public class FmgImporter
{
    public TextEditorScreen Editor;
    public ProjectEntry Project;

    public Dictionary<string, StoredFmgContainer> ImportSources = new();

    public FmgImporter(TextEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// Context Menu options in the Container list
    /// </summary>
    public void MenubarOptions()
    {
        if (ImGui.BeginMenu("Import"))
        {
            if (ImGui.BeginMenu("File", Editor.Selection.SelectedContainerWrapper != null))
            {
                DisplayImportList(ImportType.Container);

                ImGui.EndMenu();
            }
            UIHelper.Tooltip("Import the selected text file on the container level, replacing all FMGs and their associated entries (if applicable).");

            if (ImGui.BeginMenu("Text File", Editor.Selection.SelectedFmgWrapper != null))
            {
                DisplayImportList(ImportType.FMG);

                ImGui.EndMenu();
            }
            UIHelper.Tooltip("Import the selected text file on the FMG level, replacing all associated entries (if applicable).");

            if (ImGui.BeginMenu("Text Entry", Editor.Selection._selectedFmgEntry != null))
            {
                DisplayImportList(ImportType.FMG_Entries);

                ImGui.EndMenu();
            }
            UIHelper.Tooltip("Import the selected text file on the FMG Entry level, replacing all matching entries.");

            ImGui.EndMenu();
        }
    }

    /// <summary>
    /// Context Menu options in the Container list
    /// </summary>
    public void FileContextMenuOptions()
    {
        if (ImGui.BeginMenu("Import Text"))
        {
            DisplayImportList(ImportType.Container);

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
            DisplayImportList(ImportType.FMG);

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
            DisplayImportList(ImportType.FMG_Entries);

            ImGui.EndMenu();
        }
    }

    /// <summary>
    /// Get the FMG Wrapper sources on project load
    /// </summary>
    public void OnProjectChanged()
    {
        LoadWrappers();
    }

    /// <summary>
    /// Display the possible import sources for the user to select from
    /// </summary>
    public void DisplayImportList(ImportType importType)
    {
        LoadWrappers();

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

    private void ImportText(StoredFmgContainer containerWrapper, ImportBehavior importBehavior)
    {
        if(containerWrapper.FmgWrappers == null)
        {
            return;
        }

        ImportActions = new List<EditorAction>();

        var targetContainer = Editor.Selection.SelectedContainerWrapper;

        if (targetContainer == null)
            return;

        foreach (var fmgWrapper in targetContainer.FmgWrappers)
        {
            foreach (var storedFmgWrapper in containerWrapper.FmgWrappers)
            {
                if (fmgWrapper.ID == storedFmgWrapper.ID)
                {
                    ProcessFmg(targetContainer, fmgWrapper, storedFmgWrapper, importBehavior);
                }
            }
        }

        var groupAction = new FmgGroupedAction(ImportActions);
        Editor.EditorActionManager.ExecuteAction(groupAction);
    }
    private void ProcessFmg(
        TextContainerWrapper containerWrapper, 
        TextFmgWrapper fmgWrapper, 
        StoredFmgWrapper storedWrapper, 
        ImportBehavior importBehavior)
    {
        var targetEntries = fmgWrapper.File.Entries;

        foreach (var storedEntry in storedWrapper.Fmg.Entries)
        {
            storedEntry.Parent = fmgWrapper.File;

            // New entry
            if (!targetEntries.Any(e => e.ID == storedEntry.ID))
            {
                ImportActions.Add(new AddFmgEntry(Editor, containerWrapper, storedEntry, storedEntry, storedEntry.ID));
            }
            // Existing entry
            else if(targetEntries.Any(e => e.ID == storedEntry.ID) && importBehavior is not ImportBehavior.Append)
            {
                var targetEntry = targetEntries.Where(e => e.ID == storedEntry.ID).FirstOrDefault();

                if (targetEntry != null)
                {
                    ImportActions.Add(new ChangeFmgEntryText(Editor, containerWrapper, targetEntry, storedEntry.Text));
                }
            }
        }
    }

    /// <summary>
    /// Load the wrappers into the FmgWrapper object and fill the ImportSources dictionary
    /// </summary>
    private void LoadWrappers()
    {
        ImportSources = new();

        var wrapperPathList = TextLocator.GetStoredContainerWrappers(Editor.Project);

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
                        wrapper = JsonSerializer.Deserialize(stream, StoredContainerWrapperSerializationContext.Default.StoredFmgContainer);
                    }
                }

                if (!ImportSources.ContainsKey(filename))
                {
                    ImportSources.Add(filename, wrapper);
                }
                else
                {
                    TaskLogs.AddLog($"Attempted to add stored text with existing key: {filename}");
                }
            }
        }
    }

    private StoredFmgContainer GenerateStoredFmgContainer(string path)
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
                    TaskLogs.AddLog($"Failed to read JSON file: {filename} at {path}\n{e.Message}", LogLevel.Warning);
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

            var generatedStoredFmgContainer = GenerateStoredFmgContainer(path);
            if (generatedStoredFmgContainer != null)
            {
                ImportText(generatedStoredFmgContainer, type);
            }
        }
    }
}

