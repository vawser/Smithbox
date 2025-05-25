using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editors.TextEditor.Enums;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Resource.Locators;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace StudioCore.Editors.TextEditor;

public class FmgExporter
{
    public TextEditorScreen Editor;
    public ProjectEntry Project;

    public FmgExporter(TextEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void OnGui()
    {
        Editor.TextExportModal.Display();
    }

    /// <summary>
    /// Context Menu options in the Container list
    /// </summary>
    public void MenubarOptions()
    {
        if (ImGui.BeginMenu("Export"))
        {
            // File
            if (ImGui.BeginMenu("File", Editor.Selection.SelectedContainerWrapper != null))
            {
                if (ImGui.Selectable("Export Selected File"))
                {
                    DisplayExportModal(ExportType.Container);
                }

                ImGui.Separator();

                if (ImGui.Selectable("Export Modified Text"))
                {
                    DisplayExportModal(ExportType.Container, ExportModifier.ModifiedOnly);
                }
                UIHelper.Tooltip("Only include Text Entries (and therefore the associated Text Files)) that are considered 'modified'.");

                if (ImGui.Selectable("Export Unique Text"))
                {
                    DisplayExportModal(ExportType.Container, ExportModifier.UniqueOnly);
                }
                UIHelper.Tooltip("Only include Text Entries (and therefore the associated Text Files) that are considered 'unique'.");

                ImGui.EndMenu();
            }
            UIHelper.Tooltip("Export your currently selected File (including all of its Text Files and their Text Entries) to a export text file.");

            // FMG
            if (ImGui.BeginMenu("Text File", Editor.Selection.SelectedFmgWrapper != null))
            {
                if (ImGui.Selectable("Export Selected Text File"))
                {
                    DisplayExportModal(ExportType.FMG);
                }

                ImGui.Separator();

                if (ImGui.Selectable("Export Modified Text"))
                {
                    DisplayExportModal(ExportType.FMG, ExportModifier.ModifiedOnly);
                }
                UIHelper.Tooltip("Only include Text Entries (and therefore the associated Text Files) that are considered 'modified'.");

                if (ImGui.Selectable("Export Unique Text"))
                {
                    DisplayExportModal(ExportType.FMG, ExportModifier.UniqueOnly);
                }
                UIHelper.Tooltip("Only include Text Entries (and therefore the associated Text Files) that are considered 'unique'.");

                ImGui.EndMenu();
            }
            UIHelper.Tooltip("Export your currently selected Text File (including all of its entries) to a export text file.");

            // FMG Entries
            if (ImGui.BeginMenu("Text Entry", Editor.Selection._selectedFmgEntry != null))
            {
                if (ImGui.Selectable("Export Selected Text Entries"))
                {
                    DisplayExportModal(ExportType.FMG_Entries);
                }

                ImGui.Separator();

                if (ImGui.Selectable("Export Modified Text"))
                {
                    DisplayExportModal(ExportType.FMG_Entries, ExportModifier.ModifiedOnly);
                }
                UIHelper.Tooltip("Only include FMG entries that are considered 'modified'.");

                if (ImGui.Selectable("Export Unique Text"))
                {
                    DisplayExportModal(ExportType.FMG_Entries, ExportModifier.UniqueOnly);
                }
                UIHelper.Tooltip("Only include FMG entries that are considered 'unique'.");

                ImGui.EndMenu();
            }
            UIHelper.Tooltip("Export your currently selected FMG Entries to a export text file.");

            if (ImGui.MenuItem("Clear Text Storage"))
            {
                DialogResult result = PlatformUtils.Instance.MessageBox(
                    $"All export text files will be deleted. Do you proceed?",
                    "Warning",
                    MessageBoxButtons.YesNo);

                if (result == DialogResult.Yes)
                {
                    var wrapperPathList = TextLocator.GetStoredContainerWrappers(Editor.Project);

                    foreach (var path in wrapperPathList)
                    {
                        if (File.Exists(path))
                        {
                            File.Delete(path);
                        }
                    }
                }
            }
            UIHelper.Tooltip("Clears all the export text files from the storage folder.");

            ImGui.EndMenu();
        }
    }

    /// <summary>
    /// Context Menu options in the Container list
    /// </summary>
    public void FileContextMenuOptions()
    {
        if (ImGui.BeginMenu("Export Text"))
        {
            if (ImGui.Selectable("Export Selected File"))
            {
                DisplayExportModal(ExportType.Container);
            }

            ImGui.Separator();

            if (ImGui.Selectable("Export Modified Text"))
            {
                DisplayExportModal(ExportType.Container, ExportModifier.ModifiedOnly);
            }
            UIHelper.Tooltip("Only include Text Entries (and therefore the associated Text Files) that are considered 'modified'.");

            if (ImGui.Selectable("Export Unique Text"))
            {
                DisplayExportModal(ExportType.Container, ExportModifier.UniqueOnly);
            }
            UIHelper.Tooltip("Only include Text Entries (and therefore the associated Text Files) that are considered 'unique'.");

            ImGui.EndMenu();
        }
        UIHelper.Tooltip("Export all associated Text Files and the Text Entries within this container.");
    }

    /// <summary>
    /// Context Menu options in the FMG list
    /// </summary>
    public void FmgContextMenuOptions()
    {
        if (ImGui.BeginMenu("Export Text"))
        {
            if (ImGui.Selectable("Export Selected Text File"))
            {
                DisplayExportModal(ExportType.FMG);
            }

            ImGui.Separator();

            if (ImGui.Selectable("Export Modified Text"))
            {
                DisplayExportModal(ExportType.FMG, ExportModifier.ModifiedOnly);
            }
            UIHelper.Tooltip("Only include Text Entries (and therefore the associated Text File) that are considered 'modified'.");

            if (ImGui.Selectable("Export Unique Text"))
            {
                DisplayExportModal(ExportType.FMG, ExportModifier.UniqueOnly);
            }
            UIHelper.Tooltip("Only include Text Entries (and therefore the associated Text File) that are considered 'unique'.");

            ImGui.EndMenu();
        }
        UIHelper.Tooltip("Export all associated entries within this Text File.");
    }

    /// <summary>
    /// Context Menu options in the FMG Entry list
    /// </summary>
    public void FmgEntryContextMenuOptions()
    {
        if (ImGui.BeginMenu("Export Text"))
        {
            if (ImGui.Selectable("Export Selected Text Entries"))
            {
                DisplayExportModal(ExportType.FMG_Entries);
            }

            ImGui.Separator();

            if (ImGui.Selectable("Export Modified Text"))
            {
                DisplayExportModal(ExportType.FMG_Entries, ExportModifier.ModifiedOnly);
            }
            UIHelper.Tooltip("Only include Text Entries that are considered 'modified'.");

            if (ImGui.Selectable("Export Unique Text"))
            {
                DisplayExportModal(ExportType.FMG_Entries, ExportModifier.UniqueOnly);
            }
            UIHelper.Tooltip("Only include Text Entries that are considered 'unique'.");

            ImGui.EndMenu();
        }
        UIHelper.Tooltip("Export all selected Text Entries.");
    }

    public void DisplayExportModal(ExportType exportType, ExportModifier exportModifier = ExportModifier.None)
    {
        CurrentExportType = exportType;
        CurrentExportModifier = exportModifier;

        if (CFG.Current.TextEditor_TextExport_UseQuickExport)
        {
            var exportName = CFG.Current.TextEditor_TextExport_QuickExportPrefix;

            ProcessExport(exportName);
        }
        else
        {
            Editor.TextExportModal.ShowModal = true;
        }
    }

    public ExportType CurrentExportType = ExportType.Container;
    public ExportModifier CurrentExportModifier = ExportModifier.None;

    /// <summary>
    /// Export the FMG entries and save them under the specified wrapper name.
    /// </summary>
    public void ProcessExport(string storedName)
    {
        // Stored Text
        var storedFmgContainer = new StoredFmgContainer();
        storedFmgContainer.Name = storedName;
        storedFmgContainer.FmgWrappers = new List<StoredFmgWrapper>();

        // Container
        if (CurrentExportType is ExportType.Container)
        {
            var selectedContainer = Editor.Selection.SelectedContainerWrapper;

            foreach(var wrapper in selectedContainer.FmgWrappers)
            {
                ProcessFmg(wrapper, storedFmgContainer);
            }
        }

        // FMG
        if (CurrentExportType is ExportType.FMG)
        {
            var selectedFmgWrapper = Editor.Selection.SelectedFmgWrapper;

            ProcessFmg(selectedFmgWrapper, storedFmgContainer);
        }

        // FMG Entries
        if (CurrentExportType is ExportType.FMG_Entries)
        {
            var selectedFmgWrapper = Editor.Selection.SelectedFmgWrapper;

            // Export associated group entries as well
            if(CFG.Current.TextEditor_TextExport_IncludeGroupedEntries)
            {
                var currentEntry = Editor.Selection._selectedFmgEntry;
                var fmgEntryGroup = Editor.EntryGroupManager.GetEntryGroup(currentEntry);

                if (fmgEntryGroup.SupportsGrouping)
                {
                    // This sends the associated fmg wrappers for processing.
                    // The current entry multiselection IDs will still match, even though the entries
                    // (as an object) is not within the associated wrappers.
                    if (fmgEntryGroup.SupportsTitle)
                    {
                        var wrapper = Editor.EntryGroupManager.GetAssociatedTitleWrapper(selectedFmgWrapper.ID);

                        // If not null, then use the associated wrapper
                        if(wrapper != null)
                            ProcessFmg(wrapper, storedFmgContainer, true);
                        // If null, then it means the current selection IS this wrapper, so use that
                        else
                            ProcessFmg(selectedFmgWrapper, storedFmgContainer, true);
                    }
                    if (fmgEntryGroup.SupportsSummary)
                    {
                        var wrapper = Editor.EntryGroupManager.GetAssociatedSummaryWrapper(selectedFmgWrapper.ID);
                        if (wrapper != null)
                            ProcessFmg(wrapper, storedFmgContainer, true);
                        else
                            ProcessFmg(selectedFmgWrapper, storedFmgContainer, true);
                    }
                    if (fmgEntryGroup.SupportsDescription)
                    {
                        var wrapper = Editor.EntryGroupManager.GetAssociatedDescriptionWrapper(selectedFmgWrapper.ID);
                        if (wrapper != null)
                            ProcessFmg(wrapper, storedFmgContainer, true);
                        else
                            ProcessFmg(selectedFmgWrapper, storedFmgContainer, true);
                    }
                    if (fmgEntryGroup.SupportsEffect)
                    {
                        var wrapper = Editor.EntryGroupManager.GetAssociatedEffectWrapper(selectedFmgWrapper.ID);
                        if (wrapper != null)
                            ProcessFmg(wrapper, storedFmgContainer, true);
                        else
                            ProcessFmg(selectedFmgWrapper, storedFmgContainer, true);
                    }
                }
                else
                {
                    ProcessFmg(selectedFmgWrapper, storedFmgContainer, true);
                }
            }
            // Otherwise just export the selected entries
            else
            {
                ProcessFmg(selectedFmgWrapper, storedFmgContainer, true);
            }
        }

        WriteWrapper(storedFmgContainer);
    }

    private void ProcessFmg(TextFmgWrapper wrapper, StoredFmgContainer storedFmgText, bool selectionOnly = false)
    {
        // Build wrapper
        var storedFmgWrapper = new StoredFmgWrapper();
        storedFmgWrapper.Name = wrapper.Name;
        storedFmgWrapper.ID = wrapper.ID;

        // Build FMG
        var storedFmg = new FMG();
        storedFmg.Name = wrapper.File.Name;
        storedFmg.Version = wrapper.File.Version;
        storedFmg.BigEndian = wrapper.File.BigEndian;
        storedFmg.Entries = new List<FMG.Entry>();

        // We have to call this so the diff cache updates for each wrapper,
        // without changing the user selection
        Editor.DifferenceManager.TrackFmgDifferences(wrapper.ID);

        // Build FMG entries
        foreach (var entry in wrapper.File.Entries)
        {
            ProcessFmgEntries(storedFmg, entry, selectionOnly);
        }

        // Add built FMG to wrapper
        storedFmgWrapper.Fmg = storedFmg;

        // Add FMG wrapper to stored text list (only if it contains entries)
        if(storedFmg.Entries.Count > 0)
            storedFmgText.FmgWrappers.Add(storedFmgWrapper);
    }

    private void ProcessFmgEntries(FMG storedFmg, FMG.Entry entry, bool selectionOnly = false)
    {
        // For the FMG entry selection only option
        if(selectionOnly)
        {
            bool skip = true;

            foreach(var sel in Editor.Selection.FmgEntryMultiselect.StoredEntries)
            {
                var fmgEntry = sel.Value;

                if(fmgEntry.ID == entry.ID)
                {
                    skip = false;
                }
            }

            if(skip)
            {
                return;
            }
        }

        // All
        if (CurrentExportModifier is ExportModifier.None)
        {
            var storedFmgEntry = new FMG.Entry(storedFmg, entry.ID, entry.Text);
            storedFmg.Entries.Add(storedFmgEntry);
        }
        // Modified Omly
        if (CurrentExportModifier is ExportModifier.ModifiedOnly)
        {
            if (Editor.DifferenceManager.IsDifferentToVanilla(entry))
            {
                var storedFmgEntry = new FMG.Entry(storedFmg, entry.ID, entry.Text);
                storedFmg.Entries.Add(storedFmgEntry);
            }
        }
        // Unique Only
        if (CurrentExportModifier is ExportModifier.UniqueOnly)
        {
            if (Editor.DifferenceManager.IsUniqueToProject(entry))
            {
                var storedFmgEntry = new FMG.Entry(storedFmg, entry.ID, entry.Text);
                storedFmg.Entries.Add(storedFmgEntry);
            }
        }
    }

    public void WriteWrapper(StoredFmgContainer wrapper)
    {
        var writeDir = TextLocator.GetStoredTextDirectory(Editor.Project);
        var writePath = $"{writeDir}\\{wrapper.Name}.json";

        if(!Directory.Exists(writeDir))
        {
            Directory.CreateDirectory(writeDir);
        }

        var proceed = false;

        if (File.Exists(writePath))
        {
            DialogResult result = PlatformUtils.Instance.MessageBox(
                    $"Exported text already exists under this name. Overwrite?", 
                    "Warning", 
                    MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                proceed = true;
            }
        }
        else
        {
            proceed = true;
        }

        if(proceed)
        {
            string jsonString = JsonSerializer.Serialize(wrapper, typeof(StoredFmgContainer), StoredContainerWrapperSerializationContext.Default);

            try
            {
                var fs = new FileStream(writePath, System.IO.FileMode.Create);
                var data = Encoding.ASCII.GetBytes(jsonString);
                fs.Write(data, 0, data.Length);
                fs.Flush();
                fs.Dispose();

                TaskLogs.AddLog($"Text Exporter: exported text: {wrapper.Name} at {writePath}");
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog($"Text Exporter: failed to export text: {wrapper.Name} at {writePath}\n{ex}");
            }
        }
    }
}

