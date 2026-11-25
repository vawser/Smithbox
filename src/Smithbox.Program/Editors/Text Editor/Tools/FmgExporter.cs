using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace StudioCore.Editors.TextEditor;

public class FmgExporter
{
    public TextEditorView View;
    public ProjectEntry Project;

    public FmgExporter(TextEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void OnGui()
    {
        View.TextExportModal.Display();
    }

    public void ContainerDropdownOptions()
    {
        // Export Text
        if (ImGui.BeginMenu($"{LOC.Get("TEXT_Exporter_Header_Export_Text")}##exportTextMenuHeader"))
        {
            // Export Selected Container
            if (ImGui.Selectable($"{LOC.Get("TEXT_Exporter_Action_Export_Container")}##exportContainer"))
            {
                DisplayExportModal(ExportType.Container);
            }
            GUI.Tooltip(LOC.Get("TEXT_Exporter_Action_Export_Container_TT"));

            ImGui.Separator();

            // Export Modified Text
            if (ImGui.Selectable($"{LOC.Get("TEXT_Exporter_Action_Export_Modified")}##exportModified"))
            {
                DisplayExportModal(ExportType.Container, ExportModifier.ModifiedOnly);
            }
            GUI.Tooltip(LOC.Get("TEXT_Exporter_Action_Export_Modified_TT"));

            // Export Unique Text
            if (ImGui.Selectable($"{LOC.Get("TEXT_Exporter_Action_Export_Unique")}##exportUnique"))
            {
                DisplayExportModal(ExportType.Container, ExportModifier.UniqueOnly);
            }
            GUI.Tooltip(LOC.Get("TEXT_Exporter_Action_Export_Unique_TT"));

            ImGui.EndMenu();
        }
    }

    public void TextFileDropdownOptions()
    {
        if (ImGui.BeginMenu($"{LOC.Get("TEXT_Exporter_Header_Export_Text")}##exportTextMenuHeader"))
        {
            if (ImGui.Selectable($"{LOC.Get("TEXT_Exporter_Action_Export_Text_File")}##exportTextFile"))
            {
                DisplayExportModal(ExportType.FMG);
            }
            GUI.Tooltip(LOC.Get("TEXT_Exporter_Action_Export_Text_File_TT"));

            ImGui.Separator();

            if (ImGui.Selectable($"{LOC.Get("TEXT_Exporter_Action_Export_Modified")}##exportModified"))
            {
                DisplayExportModal(ExportType.FMG, ExportModifier.ModifiedOnly);
            }
            GUI.Tooltip(LOC.Get("TEXT_Exporter_Action_Export_Modified_TT"));

            if (ImGui.Selectable($"{LOC.Get("TEXT_Exporter_Action_Export_Unique")}##exportUnique"))
            {
                DisplayExportModal(ExportType.FMG, ExportModifier.UniqueOnly);
            }
            GUI.Tooltip(LOC.Get("TEXT_Exporter_Action_Export_Unique_TT"));

            ImGui.EndMenu();
        }
    }

    public void TextEntryDropdownOptions()
    {
        if (ImGui.BeginMenu($"{LOC.Get("TEXT_Exporter_Header_Export_Text")}##exportTextMenuHeader"))
        {
            if (ImGui.Selectable($"{LOC.Get("TEXT_Exporter_Action_Export_Text_Entry")}##exportTextEntry"))
            {
                DisplayExportModal(ExportType.FMG_Entries);
            }
            GUI.Tooltip(LOC.Get("TEXT_Exporter_Action_Export_Text_Entry_TT"));

            ImGui.Separator();

            if (ImGui.Selectable($"{LOC.Get("TEXT_Exporter_Action_Export_Modified")}##exportModified"))
            {
                DisplayExportModal(ExportType.FMG_Entries, ExportModifier.ModifiedOnly);
            }
            GUI.Tooltip(LOC.Get("TEXT_Exporter_Action_Export_Modified_TextEntry_TT"));

            if (ImGui.Selectable($"{LOC.Get("TEXT_Exporter_Action_Export_Unique")}##exportUnique"))
            {
                DisplayExportModal(ExportType.FMG_Entries, ExportModifier.UniqueOnly);
            }
            GUI.Tooltip(LOC.Get("TEXT_Exporter_Action_Export_Unique_TextEntry_TT"));

            ImGui.EndMenu();
        }
    }

    public void DisplayExportModal(ExportType exportType, ExportModifier exportModifier = ExportModifier.None)
    {
        CurrentExportType = exportType;
        CurrentExportModifier = exportModifier;

        if (CFG.Current.TextEditor.TextEditor_Text_Export_Enable_Quick_Export)
        {
            // Prefix + what was exported + a timestamp: every quick export is kept
            // (it used to reuse the prefix alone, overwriting the previous quick export each time)
            var parts = new[]
            {
                CFG.Current.TextEditor.TextEditor_TextExport_QuickExportPrefix?.Trim(),
                AutoExportName(exportType, exportModifier),
                DateTime.Now.ToString("yyyyMMdd-HHmmss")
            };

            InitializeExport(string.Join("-", parts.Where(p => !string.IsNullOrEmpty(p))), exportType, exportModifier);
        }
        else
        {
            View.TextExportModal.Open(AutoExportName(exportType, exportModifier));
        }
    }

    /// <summary>
    /// Export the scope to {export folder}/{exportName}.json. Every export (panel, modal, Quick Export) comes
    /// here, so the name is made filename-safe in one place.
    /// </summary>
    public void InitializeExport(string exportName, ExportType exportType, ExportModifier exportModifier = ExportModifier.None)
    {
        exportName = TextUtils.SanitizeFileName(exportName);

        if (exportName == "")
        {
            Smithbox.LogError(this, LOC.Get("TEXT_Exporter_Log_Filename_Empty"));
            return;
        }

        CurrentExportType = exportType;
        CurrentExportModifier = exportModifier;

        var outputWrapper = ProcessExport(exportName);

        WriteWrapper(TextUtils.GetExportDirectory(Project), exportName, outputWrapper);
    }

    /// <summary>
    /// The default export name for the current selection: container, language, text file ID and entry ID as
    /// the scope needs, plus -modified / -unique for a filtered export, e.g. item.msgbnd-english-10-1000-modified.
    /// "" when the selection doesn't have what the scope needs.
    /// </summary>
    public string AutoExportName(ExportType scope, ExportModifier filter)
    {
        var container = View.Selection.SelectedContainerWrapper;
        var fmg = View.Selection.SelectedFmgWrapper;
        var entry = View.Selection._selectedFmgEntry;

        if (container == null)
            return "";
        if (scope is ExportType.FMG or ExportType.FMG_Entries && fmg == null)
            return "";
        if (scope is ExportType.FMG_Entries && entry == null)
            return "";

        var name = container.FileEntry.Filename;

        // The same container exists once per language: without it, exports of two languages share a name
        if (container.ContainerDisplayCategory is not (TextContainerCategory.None or TextContainerCategory.Common))
            name = $"{name}-{container.ContainerDisplayCategory.ToString().ToLowerInvariant()}";

        name = scope switch
        {
            ExportType.FMG => $"{name}-{fmg.ID}",
            ExportType.FMG_Entries => $"{name}-{fmg.ID}-{entry.ID}",
            _ => name
        };

        // A filtered export doesn't overwrite the full export of the same scope
        name = filter switch
        {
            ExportModifier.ModifiedOnly => $"{name}-modified",
            ExportModifier.UniqueOnly => $"{name}-unique",
            _ => name
        };

        return TextUtils.SanitizeFileName(name);
    }

    public ExportType CurrentExportType = ExportType.Container;
    public ExportModifier CurrentExportModifier = ExportModifier.None;

    /// <summary>
    /// Export the FMG entries and save them under the specified wrapper name.
    /// </summary>
    public StoredFmgContainer ProcessExport(string storedName)
    {
        // Stored Text
        var storedFmgContainer = new StoredFmgContainer();
        storedFmgContainer.Name = storedName;
        storedFmgContainer.FmgWrappers = new List<StoredFmgWrapper>();

        // Record where the text came from, so the import menu can show it and warn on a mismatch
        var sourceContainer = View.Selection.SelectedContainerWrapper;
        if (sourceContainer != null)
        {
            storedFmgContainer.SourceContainer = sourceContainer.FileEntry.Filename;
            storedFmgContainer.SourceCategory = sourceContainer.ContainerDisplayCategory.ToString();
        }

        // Container
        if (CurrentExportType is ExportType.Container)
        {
            var selectedContainer = View.Selection.SelectedContainerWrapper;

            foreach(var wrapper in selectedContainer.FmgWrappers)
            {
                ProcessFmg(wrapper, storedFmgContainer);
            }
        }

        // FMG
        if (CurrentExportType is ExportType.FMG)
        {
            var selectedFmgWrapper = View.Selection.SelectedFmgWrapper;

            ProcessFmg(selectedFmgWrapper, storedFmgContainer);
        }

        // FMG Entries
        if (CurrentExportType is ExportType.FMG_Entries)
        {
            var selectedFmgWrapper = View.Selection.SelectedFmgWrapper;

            // Export associated group entries as well
            if(CFG.Current.TextEditor.TextEditor_Text_Export_Include_Grouped_Entries)
            {
                var currentEntry = View.Selection._selectedFmgEntry;
                var fmgEntryGroup = View.EntryGroupManager.GetEntryGroup(currentEntry);

                if (currentEntry != null && fmgEntryGroup != null)
                {
                    if (fmgEntryGroup.SupportsGrouping)
                    {
                        // Every FMG of the group (up to 10 slots), the selected FMG included.
                        // The current entry multiselection IDs still match, even though the entries
                        // (as objects) aren't in the other FMGs.
                        foreach (var slot in fmgEntryGroup.Slots)
                        {
                            ProcessFmg(slot.Wrapper, storedFmgContainer, true);
                        }
                    }
                    else
                    {
                        ProcessFmg(selectedFmgWrapper, storedFmgContainer, true);
                    }
                }
            }
            // Otherwise just export the selected entries
            else
            {
                ProcessFmg(selectedFmgWrapper, storedFmgContainer, true);
            }
        }

        // ProcessFmg points the diff cache at each exported FMG in turn; point it back at the
        // selected FMG, or the entry list's modified/unique colours showed the last exported one
        View.DifferenceManager.TrackFmgDifferences();

        return storedFmgContainer;
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
        View.DifferenceManager.TrackFmgDifferences(wrapper.ID);

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

            foreach(var sel in View.Selection.FmgEntryMultiselect.StoredEntries)
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
        else if (CurrentExportModifier is ExportModifier.ModifiedOnly)
        {
            if (View.DifferenceManager.IsDifferentToVanilla(entry))
            {
                var storedFmgEntry = new FMG.Entry(storedFmg, entry.ID, entry.Text);
                storedFmg.Entries.Add(storedFmgEntry);
            }
        }
        // Unique Only
        else if (CurrentExportModifier is ExportModifier.UniqueOnly)
        {
            if (View.DifferenceManager.IsUniqueToProject(entry))
            {
                var storedFmgEntry = new FMG.Entry(storedFmg, entry.ID, entry.Text);
                storedFmg.Entries.Add(storedFmgEntry);
            }
        }
    }

    public void WriteWrapper(string writeDir, string filename, StoredFmgContainer wrapper)
    {
        if(!Directory.Exists(writeDir))
        {
            Directory.CreateDirectory(writeDir);
        }

        var filePath = Path.Combine(writeDir, $"{filename}.json");

        var proceed = false;

        // Check the full path: a bare filename resolved against the working directory, so this never fired
        if (File.Exists(filePath))
        {
            DialogResult result = PlatformUtils.Instance.MessageBox(
                    LOC.Get("TEXT_Exporter_Export_Text_Exists_Overwrite"),
                    LOC.Get("SYS_Warning_Header"),
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
            try
            {
                // Readable UTF-8 (was ASCII with every non-ASCII character as \uXXXX)
                var jsonString = StoredFmgJson.Write(filePath, wrapper);

                View.ToolView.DataTransferTool.ExportString = jsonString;

                // Reload the stored-export list so the new file shows in the import menus
                View.FmgImporter.InvalidateWrapperCache();

                // Select the new export in the Import tab's table (keys are full paths)
                View.ToolView.DataTransferTool.ExportList.SelectedKey = Path.GetFullPath(filePath);

                Smithbox.Log(this, LOC.Get("TEXT_Exporter_Exported_Text_PASS", wrapper.Name, filePath));
            }
            catch (Exception ex)
            {
                Smithbox.LogError(this, LOC.Get("TEXT_Exporter_Exported_Text_FAIL", wrapper.Name, filePath), ex);
            }
        }
    }
}

