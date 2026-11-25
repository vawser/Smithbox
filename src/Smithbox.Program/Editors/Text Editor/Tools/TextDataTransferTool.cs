using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Utilities;
using System.Diagnostics;
using System.Numerics;

namespace StudioCore.Editors.TextEditor;

/// <summary>
/// The Data Transfer tool: JSON export / import of text. The tool panel and the Tools menu use the
/// same model: Scope (Container / Text File / Selected Entries) x Filter (All / Modified / Unique)
/// for export, Scope x Mode (Append / Replace) for import, and the same export folder
/// (TextUtils.GetExportDirectory).
/// </summary>
public class TextDataTransferTool
{
    public TextEditorView View;
    public ProjectEntry Project;

    // The panel's choices live in the settings, so they are kept between sessions
    // (ref properties, so the combos can edit the setting directly)
    private ref ExportType ExportScope => ref CFG.Current.TextEditor.TextEditor_DataTransfer_Export_Scope;
    private ref ExportModifier ExportFilter => ref CFG.Current.TextEditor.TextEditor_DataTransfer_Export_Filter;
    private ref FmgImportType ImportScope => ref CFG.Current.TextEditor.TextEditor_DataTransfer_Import_Scope;
    private ref ImportBehavior ImportMode => ref CFG.Current.TextEditor.TextEditor_DataTransfer_Import_Mode;

    public string ExportFilename = "";

    // Lines of the last export shown in Export Output; Copy to Clipboard copies all of it
    private const int PreviewLineLimit = 200;

    private string _exportString = "";
    private string _exportPreview = "";

    /// <summary>The JSON of the last export. Setting it also builds the shortened preview, once.</summary>
    public string ExportString
    {
        get => _exportString;
        set
        {
            _exportString = value ?? "";
            _exportPreview = BuildPreview(_exportString);
        }
    }

    private static string BuildPreview(string text)
    {
        // Count lines without splitting the whole (possibly huge) string
        var index = 0;
        for (var line = 0; line < PreviewLineLimit; line++)
        {
            index = text.IndexOf('\n', index);
            if (index < 0)
                return text; // Short enough to show whole

            index++;
        }

        var remaining = 1;
        for (var i = index; i < text.Length; i++)
        {
            if (text[i] == '\n')
                remaining++;
        }

        return text[..index] + LOC.Get("TEXT_DataTransfer_Output_Truncated", remaining);
    }

    // The Import tab's table of stored exports (selection, rename, archive)
    public TextStoredExportList ExportList;

    // Edit buffer of the export folder field; applied to the setting when the field is left
    private string _directoryInput;

    public TextDataTransferTool(TextEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        ExportList = new TextStoredExportList(view, project);
    }

    public void DisplayDropdown()
    {
        if (ImGui.BeginMenu($"{LOC.Get("TEXT_DataTransfer_Menu_Header")}##dataTransferMenuHeader"))
        {
            ImportMenu();
            ExportMenu();

            ImGui.EndMenu();
        }
    }

    public void Display()
    {
        ImGui.BeginChild("DataTransferSection", ImGuiChildFlags.Borders);

        ImGui.BeginTabBar("dataTransferTabs");

        ImportTab();
        ExportTab();

        ImGui.EndTabBar();

        ImGui.EndChild();
    }

    #region Shared
    /// <summary>Why the scope can't be used with the current selection, or null when it can.</summary>
    private string ScopeUnavailableReason(ExportType scope)
    {
        if (View.Selection.SelectedContainerWrapper == null)
            return LOC.Get("TEXT_DataTransfer_Log_No_Container_Selected");

        if (scope is ExportType.FMG or ExportType.FMG_Entries && View.Selection.SelectedFmgWrapper == null)
            return LOC.Get("TEXT_DataTransfer_Log_No_Text_File_Selected");

        if (scope is ExportType.FMG_Entries && View.Selection._selectedFmgEntry == null)
            return LOC.Get("TEXT_DataTransfer_Log_No_Text_Entry_Selected");

        return null;
    }

    private string ScopeUnavailableReason(FmgImportType scope)
    {
        return ScopeUnavailableReason(scope switch
        {
            FmgImportType.FMG => ExportType.FMG,
            FmgImportType.FMG_Entries => ExportType.FMG_Entries,
            _ => ExportType.Container
        });
    }

    private static void EnumCombo<T>(string id, ref T value) where T : struct, Enum
    {
        GUI.SetInputWidth();
        if (ImGui.BeginCombo(id, LOC.Get(value.GetDisplayName())))
        {
            foreach (var entry in Enum.GetValues<T>())
            {
                if (ImGui.Selectable(LOC.Get(entry.GetDisplayName()), entry.Equals(value)))
                {
                    value = entry;
                }
            }

            ImGui.EndCombo();
        }
    }

    private static void ReasonText(string reason)
    {
        if (reason != null)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, reason);
    }
    #endregion

    #region Import
    public void ImportTab()
    {
        if (ImGui.BeginTabItem($"{LOC.Get("TEXT_DataTransfer_Tab_Import")}##importTab"))
        {
            GUI.WrappedText(LOC.Get("TEXT_DataTransfer_Import_Hint"));

            // Scope
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("TEXT_DataTransfer_Header_Import_Scope"),
                LOC.Get("TEXT_DataTransfer_Header_Import_Scope_TT"));

            EnumCombo("##importScope", ref ImportScope);

            // Mode
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("TEXT_DataTransfer_Header_Import_Type"),
                LOC.Get("TEXT_DataTransfer_Header_Import_Type_TT"));

            EnumCombo("##importMode", ref ImportMode);

            GUI.WrappedText(LOC.Get(ImportMode is ImportBehavior.Replace
                ? "TEXT_DataTransfer_Import_Mode_Replace_Desc"
                : "TEXT_DataTransfer_Import_Mode_Append_Desc"));

            ImGui.Checkbox($"{LOC.Get("TEXT_ImportPreview_Option_Preview")}##dataTransferPreviewToggle",
                ref CFG.Current.TextEditor.TextEditor_DataTransfer_Preview_Imports);
            GUI.Tooltip(LOC.Get("TEXT_ImportPreview_Option_Preview_TT"));

            // Sources
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("TEXT_DataTransfer_Header_Import_Sources"),
                LOC.Get("TEXT_DataTransfer_Header_Import_Sources_TT"));

            ExportList.Display(ImportSelectedSource);

            var hasSource = ExportList.HasSelection;

            GUI.ConditionalMultiButtonInput("storedExportActions",
                "showInFolder",
                LOC.Get("TEXT_DataTransfer_Action_Show_In_Folder"),
                LOC.Get("TEXT_DataTransfer_Action_Show_In_Folder_TT"),
                ExportList.ShowSelectedInFolder, hasSource,

                "renameExport",
                LOC.Get("TEXT_DataTransfer_Action_Rename_Export"),
                LOC.Get("TEXT_DataTransfer_Action_Rename_Export_TT"),
                ExportList.BeginRenameSelected, hasSource,

                "archiveExport",
                LOC.Get("TEXT_DataTransfer_Action_Archive_Export"),
                LOC.Get("TEXT_DataTransfer_Action_Archive_Export_TT"),
                ExportList.ArchiveSelected, hasSource);

            // Actions
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("TEXT_DataTransfer_Header_Actions"),
                LOC.Get("TEXT_DataTransfer_Header_Actions_TT"));

            var reason = ScopeUnavailableReason(ImportScope);

            GUI.ConditionalMultiButtonInput("importActions",
                "importSelected",
                LOC.Get("TEXT_DataTransfer_Action_Import_Selected"),
                LOC.Get("TEXT_DataTransfer_Action_Import_Selected_TT"),
                ImportSelectedSource, reason == null && hasSource,

                "importTextFromFile",
                LOC.Get("TEXT_DataTransfer_Action_Import_From_File"),
                LOC.Get("TEXT_DataTransfer_Action_Import_From_File_TT"),
                ImportTextFromFile, reason == null,

                "importTextFromClipboard",
                LOC.Get("TEXT_DataTransfer_Action_Import_From_Clipboard"),
                LOC.Get("TEXT_DataTransfer_Action_Import_From_Clipboard_TT"),
                ImportTextFromClipboard, reason == null,

                "refreshSources",
                LOC.Get("TEXT_DataTransfer_Action_Refresh_Sources"),
                LOC.Get("TEXT_DataTransfer_Action_Refresh_Sources_TT"),
                View.FmgImporter.InvalidateWrapperCache, true);

            ReasonText(reason ?? (hasSource ? null : LOC.Get("TEXT_DataTransfer_Log_No_Source_Selected")));

            ImGui.EndTabItem();
        }
    }

    public void ImportSelectedSource()
    {
        var source = ExportList.Selected;
        if (source == null)
        {
            Smithbox.LogError<TextDataTransferTool>(LOC.Get("TEXT_DataTransfer_Log_No_Source_Selected"));
            return;
        }

        View.FmgImporter.ImportText(source, ImportMode, ImportScope);
    }

    public void ImportTextFromClipboard()
    {
        View.FmgImporter.ImportFromClipboard(ImportMode, ImportScope);
    }

    public void ImportTextFromFile()
    {
        View.FmgImporter.PromptExternalTextImport(ImportMode, ImportScope);
    }

    public void ImportMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("TEXT_DataTransfer_Header_Import")}##importMenuHeader"))
        {
            if (ImGui.BeginMenu($"{LOC.Get("TEXT_DataTransfer_Header_Import_Container")}##containerImport", View.Selection.SelectedContainerWrapper != null))
            {
                View.FmgImporter.DisplayImportList(FmgImportType.Container);

                ImGui.EndMenu();
            }
            GUI.Tooltip(LOC.Get("TEXT_DataTransfer_Import_Container_TT"));

            if (ImGui.BeginMenu($"{LOC.Get("TEXT_DataTransfer_Header_Import_Text_File")}##textFileImport", View.Selection.SelectedFmgWrapper != null))
            {
                View.FmgImporter.DisplayImportList(FmgImportType.FMG);

                ImGui.EndMenu();
            }
            GUI.Tooltip(LOC.Get("TEXT_DataTransfer_Import_Text_File_TT"));

            if (ImGui.BeginMenu($"{LOC.Get("TEXT_DataTransfer_Header_Import_Text_Entry")}##textEntryImport", View.Selection._selectedFmgEntry != null))
            {
                View.FmgImporter.DisplayImportList(FmgImportType.FMG_Entries);

                ImGui.EndMenu();
            }
            GUI.Tooltip(LOC.Get("TEXT_DataTransfer_Import_Text_Entry_TT"));

            ImGui.EndMenu();
        }
    }
    #endregion

    #region Export
    public void ExportTab()
    {
        if (ImGui.BeginTabItem($"{LOC.Get("TEXT_DataTransfer_Tab_Export")}##exportTab"))
        {
            GUI.WrappedText(LOC.Get("TEXT_DataTransfer_Export_Hint"));

            // Scope
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("TEXT_DataTransfer_Header_Export_Scope"),
                LOC.Get("TEXT_DataTransfer_Header_Export_Scope_TT"));

            EnumCombo("##exportScope", ref ExportScope);

            // Filter
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("TEXT_DataTransfer_Header_Export_Type"),
                LOC.Get("TEXT_DataTransfer_Header_Export_Type_TT"));

            EnumCombo("##exportFilter", ref ExportFilter);

            // Folder
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("TEXT_DataTransfer_Header_Export_Directory"),
                LOC.Get("TEXT_DataTransfer_Header_Export_Directory_TT"));

            DisplayDirectoryInput();

            GUI.MultiButtonInput("textExportDir",
                "setDirectory",
                LOC.Get("TEXT_DataTransfer_Action_Set_Export_Directory"),
                LOC.Get("TEXT_DataTransfer_Action_Set_Export_Directory_TT"),
                SetExportDirectory,

                "openDirectory",
                LOC.Get("TEXT_DataTransfer_Action_Open_Export_Directory"),
                LOC.Get("TEXT_DataTransfer_Action_Open_Export_Directory_TT"),
                OpenExportDirectory,

                "clearDirectory",
                LOC.Get("TEXT_DataTransfer_Action_Clear_Export_Directory"),
                LOC.Get("TEXT_DataTransfer_Action_Clear_Export_Directory_TT"),
                () => SetCustomDirectory(""));

            // Filename
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("TEXT_DataTransfer_Header_Export_Filename"),
                LOC.Get("TEXT_DataTransfer_Header_Export_Filename_TT"));

            // The hint shows the name used when the field is blank
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
            ImGui.InputTextWithHint("##textExportFileName", AutoExportName(), ref ExportFilename, 255);

            // Actions
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("TEXT_DataTransfer_Header_Actions"),
                LOC.Get("TEXT_DataTransfer_Header_Actions_TT"));

            var reason = ScopeUnavailableReason(ExportScope);

            GUI.ConditionalMultiButtonInput("exportActions",
                "export",
                LOC.Get("TEXT_DataTransfer_Action_Export"),
                LOC.Get("TEXT_DataTransfer_Action_Export_TT"),
                ExportAction, reason == null);

            ReasonText(reason);

            // Output
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("TEXT_DataTransfer_Header_Output"),
                LOC.Get("TEXT_DataTransfer_Header_Output_TT"));

            // TextUnformatted, as the output can exceed InputTextMultiline's buffers; and only the first
            // lines, as drawing a whole-container export every frame is slow
            ImGui.BeginChild("OutputTextSection", new Vector2(0, 250), ImGuiChildFlags.Borders);
            ImGui.TextUnformatted(_exportPreview);
            ImGui.EndChild();

            GUI.MultiButtonInput("csvOutputActions",
                "copyToClipboard",
                LOC.Get("TEXT_DataTransfer_Action_Copy_To_Clipboard"),
                LOC.Get("TEXT_DataTransfer_Action_Copy_To_Clipboard_TT"),
                CopyOutputToClipboard);

            ImGui.EndTabItem();
        }
    }

    private void DisplayDirectoryInput()
    {
        var saved = TextUtils.GetCustomExportDirectory();

        _directoryInput ??= saved;

        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
        ImGui.InputTextWithHint("##textExportDir", LOC.Get("TEXT_DataTransfer_Export_Directory_Hint"), ref _directoryInput, 1024);

        // Applied when the field is left after an edit (Enter, Tab or a click), not on every key
        if (GUI.InputCommitted())
        {
            SetCustomDirectory(_directoryInput);
        }
        // Not being edited: follow the setting (Browse / Use Project Folder, or Escape)
        else if (!ImGui.IsItemActive())
        {
            _directoryInput = saved;
        }

        GUI.WrappedText(LOC.Get("TEXT_DataTransfer_Export_Directory_Resolved", TextUtils.GetExportDirectory(Project)));
    }

    private void SetCustomDirectory(string path)
    {
        path = path?.Trim() ?? "";

        if (path == TextUtils.GetCustomExportDirectory())
            return;

        CFG.Current.TextEditor.TextEditor_DataTransfer_Export_Directory = path;
        _directoryInput = path;

        // The import lists include the export folder
        View.FmgImporter.InvalidateWrapperCache();
    }

    public void SetExportDirectory()
    {
        if (PlatformUtils.Instance.OpenFolderDialog(LOC.Get("TEXT_DataTransfer_Dialog_Select_Export_Dest"), out var path))
        {
            SetCustomDirectory(path);
        }
    }

    public void CopyOutputToClipboard()
    {
        PlatformUtils.Instance.SetClipboardText(ExportString);
    }

    public void OpenExportDirectory()
    {
        var dir = TextUtils.GetExportDirectory(Project);

        try
        {
            // The Exported Text folder only exists after the first export
            Directory.CreateDirectory(dir);
            Process.Start("explorer.exe", $"\"{dir}\"");
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("TEXT_DataTransfer_Log_Open_Directory_Failed", dir), ex);
        }
    }

    /// <summary>The filename used when the Export Filename field is blank ("" when the scope can't be used).</summary>
    private string AutoExportName() => View.FmgExporter.AutoExportName(ExportScope, ExportFilter);

    public void ExportAction()
    {
        var reason = ScopeUnavailableReason(ExportScope);
        if (reason != null)
        {
            Smithbox.LogError<TextDataTransferTool>(reason);
            return;
        }

        // Made filename-safe in InitializeExport
        var filename = string.IsNullOrWhiteSpace(ExportFilename) ? AutoExportName() : ExportFilename.Trim();

        if (TextUtils.SanitizeFileName(filename) == "")
        {
            Smithbox.LogError<TextDataTransferTool>(LOC.Get("TEXT_DataTransfer_Log_Filename_Empty"));
            return;
        }

        View.FmgExporter.InitializeExport(filename, ExportScope, ExportFilter);
    }

    public void ExportMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("TEXT_DataTransfer_Menu_Header_Export")}##exportMenuHeader"))
        {
            // Container
            if (ImGui.BeginMenu($"{LOC.Get("TEXT_DataTransfer_Menu_Header_Container")}##containerMenuHeader", View.Selection.SelectedContainerWrapper != null))
            {
                // Export Selected Container
                if (ImGui.Selectable($"{LOC.Get("TEXT_DataTransfer_Export_Selected_Container")}##exportContainerText"))
                {
                    View.FmgExporter.DisplayExportModal(ExportType.Container);
                }

                ImGui.Separator();

                // Export Modified Text
                if (ImGui.Selectable($"{LOC.Get("TEXT_DataTransfer_Export_Modified_Text")}##containerExportModifiedText"))
                {
                    View.FmgExporter.DisplayExportModal(ExportType.Container, ExportModifier.ModifiedOnly);
                }
                GUI.Tooltip(LOC.Get("TEXT_DataTransfer_Export_Modified_Text_TT"));

                // Export Unique Text
                if (ImGui.Selectable($"{LOC.Get("TEXT_DataTransfer_Export_Unique_Text")}##containerExportUniqueText"))
                {
                    View.FmgExporter.DisplayExportModal(ExportType.Container, ExportModifier.UniqueOnly);
                }
                GUI.Tooltip(LOC.Get("TEXT_DataTransfer_Export_Unique_Text_TT"));

                ImGui.EndMenu();
            }
            GUI.Tooltip(LOC.Get("TEXT_DataTransfer_Menu_Header_Container_TT"));

            // Text File
            if (ImGui.BeginMenu($"{LOC.Get("TEXT_DataTransfer_Menu_Header_Text_File")}##textFileMenuHeader", View.Selection.SelectedFmgWrapper != null))
            {
                // Export Text File
                if (ImGui.Selectable($"{LOC.Get("TEXT_DataTransfer_Export_Selected_Text_File")}##exportTextFileText"))
                {
                    View.FmgExporter.DisplayExportModal(ExportType.FMG);
                }

                ImGui.Separator();

                // Export Modified Text
                if (ImGui.Selectable($"{LOC.Get("TEXT_DataTransfer_Export_Modified_Text")}##textFileExportModifiedText"))
                {
                    View.FmgExporter.DisplayExportModal(ExportType.FMG, ExportModifier.ModifiedOnly);
                }
                GUI.Tooltip(LOC.Get("TEXT_DataTransfer_Export_Modified_Text_TT"));

                // Export Unique Text
                if (ImGui.Selectable($"{LOC.Get("TEXT_DataTransfer_Export_Unique_Text")}##textFileExportUniqueText"))
                {
                    View.FmgExporter.DisplayExportModal(ExportType.FMG, ExportModifier.UniqueOnly);
                }
                GUI.Tooltip(LOC.Get("TEXT_DataTransfer_Export_Unique_Text_TT"));

                ImGui.EndMenu();
            }
            GUI.Tooltip(LOC.Get("TEXT_DataTransfer_Menu_Header_Text_File_TT"));

            // Text Entry
            if (ImGui.BeginMenu($"{LOC.Get("TEXT_DataTransfer_Menu_Header_Text_Entry")}##textEntryMenuHEader", View.Selection._selectedFmgEntry != null))
            {
                // Export Text Entry
                if (ImGui.Selectable($"{LOC.Get("TEXT_DataTransfer_Export_Selected_Text_Entry")}##exportTextEntryText"))
                {
                    View.FmgExporter.DisplayExportModal(ExportType.FMG_Entries);
                }

                ImGui.Separator();

                // Export Modified Text
                if (ImGui.Selectable($"{LOC.Get("TEXT_DataTransfer_Export_Modified_Text")}##textEntryExportModifiedText"))
                {
                    View.FmgExporter.DisplayExportModal(ExportType.FMG_Entries, ExportModifier.ModifiedOnly);
                }
                GUI.Tooltip(LOC.Get("TEXT_DataTransfer_Export_Entry_Modified_Text_TT"));

                // Export Unique Text
                if (ImGui.Selectable($"{LOC.Get("TEXT_DataTransfer_Export_Unique_Text")}##textEntryExportUniqueText"))
                {
                    View.FmgExporter.DisplayExportModal(ExportType.FMG_Entries, ExportModifier.UniqueOnly);
                }
                GUI.Tooltip(LOC.Get("TEXT_DataTransfer_Export_Entry_Unique_Text_TT"));

                ImGui.EndMenu();
            }
            GUI.Tooltip(LOC.Get("TEXT_DataTransfer_Menu_Header_Text_Entry_TT"));

            ImGui.EndMenu();
        }
    }

    #endregion
}
