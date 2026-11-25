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

    // Last write time of each ImportSources file (same keys), read once per load for the export list
    public Dictionary<string, DateTime> ImportSourceDates = new();

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

            if (ImGui.Selectable($"{LOC.Get("TEXT_Importer_Action_From_Clipboard")}##fromClipboardAppend"))
            {
                ImportFromClipboard(ImportBehavior.Append, importType);
            }
            GUI.Tooltip(LOC.Get("TEXT_Importer_Action_From_Clipboard_TT"));

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

            if (ImGui.Selectable($"{LOC.Get("TEXT_Importer_Action_From_Clipboard")}##fromClipboardReplace"))
            {
                ImportFromClipboard(ImportBehavior.Replace, importType);
            }
            GUI.Tooltip(LOC.Get("TEXT_Importer_Action_From_Clipboard_TT"));

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

    public static void ShowSourceTooltip(StoredFmgContainer stored)
    {
        if (!ImGui.IsItemHovered())
            return;

        GUI.Tooltip(string.IsNullOrEmpty(stored.SourceContainer)
            ? LOC.Get("TEXT_Importer_Source_Unknown_TT")
            : LOC.Get("TEXT_Importer_Source_TT", stored.SourceContainer, stored.SourceCategory));
    }

    /// <summary>The import preview popup (shown when TextEditor_DataTransfer_Preview_Imports is on).</summary>
    public TextImportPreviewPopup PreviewPopup;

    /// <summary>
    /// Import stored text into the selected container: shows the preview first when the setting is on,
    /// otherwise applies at once. Every import (menus, Data Transfer panel, external file) comes here.
    /// </summary>
    public void ImportText(StoredFmgContainer containerWrapper, ImportBehavior importBehavior, FmgImportType importType = FmgImportType.Container)
    {
        var plan = BuildPlan(containerWrapper, importBehavior, importType);
        if (plan == null)
            return;

        if (CFG.Current.TextEditor.TextEditor_DataTransfer_Preview_Imports)
        {
            PreviewPopup ??= new TextImportPreviewPopup(Parent, this);
            PreviewPopup.Show(plan);
            return;
        }

        ApplyPlan(plan);
    }

    /// <summary>
    /// Work out what importing would do, without changing anything. Null when there is nothing to import into.
    /// importType limits the target: Container = every matching FMG in the container;
    /// FMG = only the selected FMG; FMG_Entries = the selected FMG and the FMGs of its entry group.
    /// A stored FMG matches a target FMG when the binder IDs match and, if the stored
    /// FMG has a name, the names match too, so an export from one container
    /// can't write into an unrelated FMG that happens to share a binder ID.
    /// </summary>
    public TextImportPlan BuildPlan(StoredFmgContainer containerWrapper, ImportBehavior importBehavior, FmgImportType importType)
    {
        if (containerWrapper == null || containerWrapper.FmgWrappers == null)
            return null;

        var targetContainer = Parent.Selection.SelectedContainerWrapper;
        if (targetContainer == null || targetContainer.FmgWrappers == null)
            return null;

        var plan = new TextImportPlan
        {
            Source = containerWrapper,
            Behavior = importBehavior,
            Scope = importType,
            Target = targetContainer,

            // Not refused: the ID + name match below decides what is actually imported, and
            // moving text between e.g. item_dlc01 and item_dlc02 is a legitimate use.
            ContainerMismatch = !string.IsNullOrEmpty(containerWrapper.SourceContainer) &&
                containerWrapper.SourceContainer != targetContainer.FileEntry.Filename
        };

        IEnumerable<TextFmgWrapper> targetFmgs = targetContainer.FmgWrappers;

        if (importType is FmgImportType.FMG or FmgImportType.FMG_Entries)
        {
            var selectedFmg = Parent.Selection.SelectedFmgWrapper;
            if (selectedFmg == null)
                return null;

            var scopedFmgs = new List<TextFmgWrapper> { selectedFmg };

            // Entry exports can include the FMGs of the entry's group (Include Grouped
            // Entries), so entry-level imports target the whole group too.
            if (importType is FmgImportType.FMG_Entries)
            {
                foreach (var (_, wrapper) in Parent.EntryGroupManager.GetGroupWrappers(selectedFmg.ID))
                {
                    if (!scopedFmgs.Contains(wrapper))
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

        var matchedStored = new HashSet<StoredFmgWrapper>();

        foreach (var fmgWrapper in targetFmgs)
        {
            if (!storedWrapperMap.TryGetValue(fmgWrapper.ID, out var storedFmgWrapper))
                continue;

            if (!string.IsNullOrEmpty(storedFmgWrapper.Name) && storedFmgWrapper.Name != fmgWrapper.Name)
                continue;

            matchedStored.Add(storedFmgWrapper);
            plan.Fmgs.Add(PlanFmg(fmgWrapper, storedFmgWrapper, importBehavior));
        }

        foreach (var storedWrapper in containerWrapper.FmgWrappers)
        {
            if (storedWrapper?.Fmg?.Entries != null && !matchedStored.Contains(storedWrapper))
                plan.Unmatched.Add(storedWrapper);
        }

        return plan;
    }

    private static TextImportFmgPlan PlanFmg(TextFmgWrapper fmgWrapper, StoredFmgWrapper storedWrapper, ImportBehavior importBehavior)
    {
        var fmgPlan = new TextImportFmgPlan { Target = fmgWrapper, Stored = storedWrapper };

        // First entry wins when the target FMG has duplicate IDs
        var targetEntryMap = new Dictionary<int, FMG.Entry>();
        foreach (var entry in fmgWrapper.File.Entries)
        {
            targetEntryMap.TryAdd(entry.ID, entry);
        }

        var addedIds = new HashSet<int>();
        var seenIds = new HashSet<int>();

        foreach (var storedEntry in storedWrapper.Fmg.Entries)
        {
            if (!targetEntryMap.TryGetValue(storedEntry.ID, out var targetEntry))
            {
                // Skip repeats of the same ID within the stored file
                if (addedIds.Add(storedEntry.ID))
                    fmgPlan.Added.Add(storedEntry);
            }
            else
            {
                // Repeats of an existing ID: the first stored one decides
                if (!seenIds.Add(storedEntry.ID))
                    continue;

                if (targetEntry.Text == storedEntry.Text)
                    fmgPlan.Identical++;
                else if (importBehavior is ImportBehavior.Append)
                    fmgPlan.Kept.Add((targetEntry, storedEntry.Text));
                else
                    fmgPlan.Changed.Add((targetEntry, storedEntry.Text));
            }
        }

        return fmgPlan;
    }

    /// <summary>
    /// Apply a plan as one undoable action. The plan is rebuilt first from the same inputs, so it is
    /// never applied to entries that changed (or were undone) since the preview was built.
    /// </summary>
    public void ApplyPlan(TextImportPlan plan)
    {
        if (plan == null)
            return;

        var fresh = BuildPlan(plan.Source, plan.Behavior, plan.Scope);
        if (fresh == null)
            return;

        if (fresh.ContainerMismatch)
        {
            Smithbox.LogWarning(this, LOC.Get("TEXT_Importer_Container_Mismatch",
                fresh.Source.Name, fresh.Source.SourceContainer, fresh.Target.FileEntry.Filename));
        }

        if (fresh.Fmgs.Count == 0)
        {
            Smithbox.Log(this, LOC.Get("TEXT_Importer_No_Matching_FMG", fresh.Source.Name));
            return;
        }

        var actions = new List<EditorAction>();

        foreach (var fmgPlan in fresh.Fmgs)
        {
            foreach (var storedEntry in fmgPlan.Added)
            {
                // A fresh entry, so the cached import source is never inserted (and shared) itself
                var newEntry = new FMG.Entry(fmgPlan.Target.File, storedEntry.ID, storedEntry.Text);
                actions.Add(new AddFmgEntry(Parent, fresh.Target, newEntry, newEntry, storedEntry.ID, true));
            }

            foreach (var (entry, newText) in fmgPlan.Changed)
            {
                actions.Add(new ChangeFmgEntryText(Parent, fresh.Target, entry, newText, false, true));
            }
        }

        if (actions.Count > 0)
        {
            Parent.ActionManager.ExecuteAction(new FmgGroupedAction(actions));

            // The actions skip per-entry diff tracking for speed; refresh it once at the end
            Parent.DifferenceManager.TrackFmgDifferences();
        }

        Smithbox.Log(this, LOC.Get("TEXT_Importer_Import_Result", fresh.Source.Name, fresh.Added, fresh.Changed));
    }

    public void OnGui()
    {
        LoadWrappers();

        PreviewPopup?.Display();
    }

    public void LoadWrappers()
    {
        if (_wrappersLoaded) 
            return;

        ImportSources = new();
        ImportSourceDates = new();

        var wrapperPathList = TextUtils.GetStoredContainerWrappers(Project);

        if (wrapperPathList.Count > 0)
        {
            foreach (var path in wrapperPathList)
            {
                // Keyed by full path: the Exported Text folder and the custom export folder
                // can both hold a file with the same name
                var key = path;
                var wrapper = new StoredFmgContainer();

                if (File.Exists(path))
                {
                    using (var stream = File.OpenRead(path))
                    {
                        try
                        {
                            wrapper = JsonSerializer.Deserialize(stream, StoredContainerWrapperSerializationContext.Default.StoredFmgContainer);
                        }
                        catch (Exception e)
                        {
                            // This used to skip the file without saying so
                            Smithbox.LogWarning(this, LOC.Get("TEXT_Importer_Stored_File_Skipped", path), e);
                            continue;
                        }
                    }
                }

                // Valid JSON that isn't a text export (a custom folder can hold other JSON): not listed
                if (wrapper?.FmgWrappers == null)
                    continue;

                if (string.IsNullOrEmpty(wrapper.Name))
                    wrapper.Name = Path.GetFileNameWithoutExtension(path);

                if (!ImportSources.ContainsKey(key))
                {
                    ImportSources.Add(key, wrapper);
                    ImportSourceDates[key] = File.GetLastWriteTime(path);
                }
                else
                {
                    Smithbox.LogError(this, LOC.Get("TEXT_Importer_Stored_Text_Key_Collision", key));
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

    /// <summary>
    /// Import a text export held on the clipboard: e.g. copied from the Data Transfer panel's Export Output,
    /// or shared as text by someone else. Goes through ImportText, so the preview shows first.
    /// </summary>
    public void ImportFromClipboard(ImportBehavior type, FmgImportType importType)
    {
        var text = PlatformUtils.Instance.GetClipboardText();

        if (string.IsNullOrWhiteSpace(text))
        {
            Smithbox.LogWarning(this, LOC.Get("TEXT_Importer_Clipboard_Empty"));
            return;
        }

        StoredFmgContainer container = null;
        try
        {
            container = JsonSerializer.Deserialize(text, StoredContainerWrapperSerializationContext.Default.StoredFmgContainer);
        }
        catch (Exception)
        {
            // Reported below: any non-export text on the clipboard ends up here
        }

        if (container?.FmgWrappers == null)
        {
            Smithbox.LogWarning(this, LOC.Get("TEXT_Importer_Clipboard_Not_Export"));
            return;
        }

        if (string.IsNullOrEmpty(container.Name))
            container.Name = LOC.Get("TEXT_Importer_Clipboard_Name");

        ImportText(container, type, importType);
    }

    public void PromptExternalTextImport(ImportBehavior type, FmgImportType importType)
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

