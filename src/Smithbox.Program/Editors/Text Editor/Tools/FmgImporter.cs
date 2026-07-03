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

    public void ContainerDropdownOptions()
    {
        if (ImGui.BeginMenu($"{LOC.Get("TEXT_Importer_Header_Import_Text")}##importMenuHeader_Container"))
        {
            DisplayImportList(FmgImportType.Container);

            ImGui.EndMenu();
        }
    }

    public void TextFileDropdownOptions()
    {
        if (ImGui.BeginMenu($"{LOC.Get("TEXT_Importer_Header_Import_Text")}##importMenuHeader_TextFile"))
        {
            DisplayImportList(FmgImportType.FMG);

            ImGui.EndMenu();
        }
    }

    public void TextEntryDropdownOptions()
    {
        if (ImGui.BeginMenu($"{LOC.Get("TEXT_Importer_Header_Import_Text")}##importMenuHeader_TextEntry"))
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
        // Append
        if(ImGui.BeginMenu($"{LOC.Get("TEXT_Importer_Header_Append")}##appendMenuHeader"))
        {
            if (ImGui.Selectable($"{LOC.Get("TEXT_Importer_Action_From_External_File")}##fromExternalFileAppend"))
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
                ImGui.Text(LOC.Get("TEXT_Importer_No_Exported_Text"));
            }

            ImGui.EndMenu();
        }
        UIHelper.Tooltip(LOC.Get("TEXT_Importer_Header_Append_TT"));

        // Replace
        if (ImGui.BeginMenu($"{LOC.Get("TEXT_Importer_Header_Replace")}##replaceMenuHeader"))
        {
            if (ImGui.Selectable($"{LOC.Get("TEXT_Importer_Action_From_External_File")}##fromExternalFileReplace"))
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
                ImGui.Text(LOC.Get("TEXT_Importer_No_Exported_Text"));
            }

            ImGui.EndMenu();
        }
        UIHelper.Tooltip(LOC.Get("TEXT_Importer_Header_Replace_TT"));
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
                    Smithbox.LogError(this, LOC.Get("TEXT_Importer_Stored_Text_Key_Collision", filename));
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
            Smithbox.LogError(this, LOC.Get("TEXT_Importer_Failed_JSON_String_Deserialization"), e);
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
                    Smithbox.LogError(this, LOC.Get("TEXT_Importer_Failed_JSON_String_Deserialization", path), e);
                }

                return wrapper;
            }
        }

        return wrapper;
    }

    private void PromptExternalTextImport(ImportBehavior type)
    {
        if (PlatformUtils.Instance.OpenFileDialog(LOC.Get("TEXT_Importer_Select_Stored_JSON"), ["json"], out var path))
        {
            if (!File.Exists(path))
            {
                Smithbox.LogError(this, LOC.Get("TEXT_Importer_Invalid_Selected_File", path));
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

