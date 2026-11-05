using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using SoulsFormats;
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
                PromptExternalTextImport(ImportBehavior.Append, importType);
            }

            ImGui.Separator();

            if (ImportSources.Count > 0)
            {
                foreach (var (key, entry) in ImportSources)
                {
                    if (ImGui.Selectable($"{entry.Name}##append_{key}"))
                    {
                        ImportText(entry, ImportBehavior.Append, importType);
                    }
                    ShowSourceTooltip(entry);
                }
            }
            else
            {
                ImGui.Text(LOC.Get("TEXT_Importer_No_Exported_Text"));
            }

            ImGui.EndMenu();
        }
        GUI.Tooltip(LOC.Get("TEXT_Importer_Header_Append_TT"));

        // Replace
        if (ImGui.BeginMenu($"{LOC.Get("TEXT_Importer_Header_Replace")}##replaceMenuHeader"))
        {
            if (ImGui.Selectable($"{LOC.Get("TEXT_Importer_Action_From_External_File")}##fromExternalFileReplace"))
            {
                PromptExternalTextImport(ImportBehavior.Replace, importType);
            }

            ImGui.Separator();

            if (ImportSources.Count > 0)
            {
                foreach (var (key, entry) in ImportSources)
                {
                    if (ImGui.Selectable($"{entry.Name}##replace_{key}"))
                    {
                        ImportText(entry, ImportBehavior.Replace, importType);
                    }
                    ShowSourceTooltip(entry);
                }
            }
            else
            {
                ImGui.Text(LOC.Get("TEXT_Importer_No_Exported_Text"));
            }

            ImGui.EndMenu();
        }
        GUI.Tooltip(LOC.Get("TEXT_Importer_Header_Replace_TT"));
    }

    private static void ShowSourceTooltip(StoredFmgContainer stored)
    {
        if (!ImGui.IsItemHovered())
            return;

        GUI.Tooltip(string.IsNullOrEmpty(stored.SourceContainer)
            ? LOC.Get("TEXT_Importer_Source_Unknown_TT")
            : LOC.Get("TEXT_Importer_Source_TT", stored.SourceContainer, stored.SourceCategory));
    }

    private List<EditorAction> ImportActions;
    private int AddedCount;
    private int ChangedCount;

    /// <summary>
    /// Import stored text into the selected container.
    /// importType limits the target: Container = every matching FMG in the container;
    /// FMG and FMG_Entries = only the selected FMG.
    /// A stored FMG matches a target FMG when the binder IDs match and, if the stored
    /// FMG has a name, the names match too, so an export from one container
    /// can't write into an unrelated FMG that happens to share a binder ID.
    /// </summary>
    public void ImportText(StoredFmgContainer containerWrapper, ImportBehavior importBehavior, FmgImportType importType = FmgImportType.Container)
    {
        if (containerWrapper == null || containerWrapper.FmgWrappers == null)
            return;

        ImportActions = new List<EditorAction>();
        AddedCount = 0;
        ChangedCount = 0;

        var targetContainer = Parent.Selection.SelectedContainerWrapper;
        if (targetContainer == null || targetContainer.FmgWrappers == null)
            return;

        // Warn when importing into a different container than the export came from.
        // Not refused: the ID + name match below decides what is actually imported, and
        // moving text between e.g. item_dlc01 and item_dlc02 is a legitimate use.
        if (!string.IsNullOrEmpty(containerWrapper.SourceContainer) &&
            containerWrapper.SourceContainer != targetContainer.FileEntry.Filename)
        {
            Smithbox.LogWarning(this, LOC.Get("TEXT_Importer_Container_Mismatch",
                containerWrapper.Name, containerWrapper.SourceContainer, targetContainer.FileEntry.Filename));
        }

        IEnumerable<TextFmgWrapper> targetFmgs = targetContainer.FmgWrappers;

        if (importType is FmgImportType.FMG or FmgImportType.FMG_Entries)
        {
            var selectedFmg = Parent.Selection.SelectedFmgWrapper;
            if (selectedFmg == null)
                return;

            var scopedFmgs = new List<TextFmgWrapper> { selectedFmg };

            // Entry exports can include the associated title/summary/description/effect
            // FMGs (Include Grouped Entries), so entry-level imports target the group too.
            if (importType is FmgImportType.FMG_Entries)
            {
                var groupManager = Parent.EntryGroupManager;
                var associated = new[]
                {
                    groupManager.GetAssociatedTitleWrapper(selectedFmg.ID),
                    groupManager.GetAssociatedSummaryWrapper(selectedFmg.ID),
                    groupManager.GetAssociatedDescriptionWrapper(selectedFmg.ID),
                    groupManager.GetAssociatedEffectWrapper(selectedFmg.ID)
                };

                foreach (var wrapper in associated)
                {
                    if (wrapper != null && !scopedFmgs.Contains(wrapper))
                        scopedFmgs.Add(wrapper);
                }
            }

            targetFmgs = scopedFmgs;
        }

        // Index stored wrappers by ID once; first one wins if the file has duplicate IDs
        var storedWrapperMap = new Dictionary<int, StoredFmgWrapper>();
        foreach (var storedWrapper in containerWrapper.FmgWrappers)
        {
            if (storedWrapper?.Fmg?.Entries != null)
                storedWrapperMap.TryAdd(storedWrapper.ID, storedWrapper);
        }

        var matchedAny = false;

        foreach (var fmgWrapper in targetFmgs)
        {
            if (!storedWrapperMap.TryGetValue(fmgWrapper.ID, out var storedFmgWrapper))
                continue;

            if (!string.IsNullOrEmpty(storedFmgWrapper.Name) && storedFmgWrapper.Name != fmgWrapper.Name)
                continue;

            matchedAny = true;
            ProcessFmg(targetContainer, fmgWrapper, storedFmgWrapper, importBehavior);
        }

        if (!matchedAny)
        {
            Smithbox.Log(this, LOC.Get("TEXT_Importer_No_Matching_FMG", containerWrapper.Name));
            return;
        }

        if (ImportActions.Count > 0)
        {
            var groupAction = new FmgGroupedAction(ImportActions);
            Parent.ActionManager.ExecuteAction(groupAction);

            // The actions skip per-entry diff tracking for speed; refresh it once at the end
            Parent.DifferenceManager.TrackFmgDifferences();
        }

        Smithbox.Log(this, LOC.Get("TEXT_Importer_Import_Result", containerWrapper.Name, AddedCount, ChangedCount));
    }

    private void ProcessFmg(
        TextContainerWrapper containerWrapper,
        TextFmgWrapper fmgWrapper,
        StoredFmgWrapper storedWrapper,
        ImportBehavior importBehavior)
    {
        // First entry wins when the target FMG has duplicate IDs (ToDictionary threw here)
        var targetEntryMap = new Dictionary<int, FMG.Entry>();
        foreach (var entry in fmgWrapper.File.Entries)
        {
            targetEntryMap.TryAdd(entry.ID, entry);
        }

        var addedIds = new HashSet<int>();

        foreach (var storedEntry in storedWrapper.Fmg.Entries)
        {
            if (!targetEntryMap.TryGetValue(storedEntry.ID, out var targetEntry))
            {
                // Skip repeats of the same ID within the stored file
                if (!addedIds.Add(storedEntry.ID))
                    continue;

                // A fresh entry, so the cached import source is never inserted (and shared) itself
                var newEntry = new FMG.Entry(fmgWrapper.File, storedEntry.ID, storedEntry.Text);

                ImportActions.Add(new AddFmgEntry(Parent, containerWrapper, newEntry, newEntry, storedEntry.ID, true));
                AddedCount++;
            }
            else if (importBehavior is not ImportBehavior.Append)
            {
                if (targetEntry.Text == storedEntry.Text)
                    continue;

                ImportActions.Add(new ChangeFmgEntryText(Parent, containerWrapper, targetEntry, storedEntry.Text, false, true));
                ChangedCount++;
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

    private void PromptExternalTextImport(ImportBehavior type, FmgImportType importType)
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
                ImportText(generatedStoredFmgContainer, type, importType);
            }
        }
    }
}

