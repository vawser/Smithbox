using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.GparamEditor;
using StudioCore.Utilities;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using Veldrid.MetalBindings;
using static HKLib.hk2018.hkSerialize.CompatTypeParentInfo;

namespace StudioCore.Editors.TextEditor;

public class TextDataTransferTool
{
    public TextEditorScreen Editor;
    public ProjectEntry Project;

    public TextImportMode ImportType = TextImportMode.Append;

    public string ImportString = "";
    public string ExportString = "";

    public string ExportDirectory = "";
    public string ExportFilename = "";

    public ExportModifier ExportModifier = ExportModifier.None;

    public TextDataTransferTool(TextEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
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

    #region Import
    public void ImportTab()
    {
        if (ImGui.BeginTabItem($"{LOC.Get("TEXT_DataTransfer_Tab_Import")}##importTab"))
        {
            UIHelper.WrappedText(LOC.Get("TEXT_DataTransfer_Import_Hint"));

            UIHelper.Spacer();
            UIHelper.SimpleHeader(
                LOC.Get("TEXT_DataTransfer_Header_Import_Type"),
                LOC.Get("TEXT_DataTransfer_Header_Import_Type_TT"));

            UIHelper.SetInputWidth();
            if (ImGui.BeginCombo("##importType", ImportType.GetDisplayName()))
            {
                foreach (var entry in Enum.GetValues(typeof(TextImportMode)))
                {
                    var curImportType = (TextImportMode)entry;

                    var displayName = LOC.Get(curImportType.GetDisplayName());

                    if (ImGui.Selectable(displayName, curImportType == ImportType))
                    {
                        ImportType = curImportType;
                    }
                }

                ImGui.EndCombo();
            }

            UIHelper.Spacer();
            UIHelper.SimpleHeader(
                LOC.Get("TEXT_DataTransfer_Header_Actions"),
                LOC.Get("TEXT_DataTransfer_Header_Actions_TT"));

            UIHelper.MultiButtonInput("importActions",
                "importTextFromFile", 
                LOC.Get("TEXT_DataTransfer_Action_Import_From_File"),
                LOC.Get("TEXT_DataTransfer_Action_Import_From_File_TT"),
                ImportTextFromFile);

            ImGui.EndTabItem();
        }
    }

    public void ImportTextFromInput()
    {
        var curView = Editor.ViewHandler.ActiveView;

        var importBehavior = ImportBehavior.Append;
        if (ImportType is TextImportMode.Overwrite)
            importBehavior = ImportBehavior.Replace;

        var generatedStoredFmgContainer = curView.FmgImporter.GenerateStoredFmgContainerFromJson(ImportString);
        if (generatedStoredFmgContainer != null)
        {
            curView.FmgImporter.ImportText(generatedStoredFmgContainer, importBehavior);
        }
    }

    public void ImportTextFromFile()
    {
        var curView = Editor.ViewHandler.ActiveView;

        var importBehavior = ImportBehavior.Append;
        if(ImportType is TextImportMode.Overwrite)
            importBehavior = ImportBehavior.Replace;

        if (PlatformUtils.Instance.OpenFileDialog(
            LOC.Get("TEXT_DataTransfer_Select_Text_JSON"), ["json"], out var path))
        {
            if (!File.Exists(path))
            {
                Smithbox.LogError(this, LOC.Get("TEXT_DataTransfer_Invalid_Text_JSON"));
                return;
            }

            var generatedStoredFmgContainer = curView.FmgImporter.GenerateStoredFmgContainerFromFile(path);
            if (generatedStoredFmgContainer != null)
            {
                curView.FmgImporter.ImportText(generatedStoredFmgContainer, importBehavior);
            }
        }
    }

    public void ImportMenu()
    {
        var curView = Editor.ViewHandler.ActiveView;

        if (ImGui.BeginMenu($"{LOC.Get("TEXT_DataTransfer_Header_Import")}##importMenuHeader"))
        {
            if (ImGui.BeginMenu($"{LOC.Get("TEXT_DataTransfer_Header_Import_Container")}##containerImport", curView.Selection.SelectedContainerWrapper != null))
            {
                curView.FmgImporter.DisplayImportList(FmgImportType.Container);

                ImGui.EndMenu();
            }
            UIHelper.Tooltip(LOC.Get("TEXT_DataTransfer_Import_Container_TT"));

            if (ImGui.BeginMenu($"{LOC.Get("TEXT_DataTransfer_Header_Import_Text_File")}##textFileImport", curView.Selection.SelectedFmgWrapper != null))
            {
                curView.FmgImporter.DisplayImportList(FmgImportType.FMG);

                ImGui.EndMenu();
            }
            UIHelper.Tooltip(LOC.Get("TEXT_DataTransfer_Import_Text_File_TT"));

            if (ImGui.BeginMenu($"{LOC.Get("TEXT_DataTransfer_Header_Import_Text_Entry")}##textEntryImport", curView.Selection._selectedFmgEntry != null))
            {
                curView.FmgImporter.DisplayImportList(FmgImportType.FMG_Entries);

                ImGui.EndMenu();
            }
            UIHelper.Tooltip(LOC.Get("TEXT_DataTransfer_Import_Text_Entry_TT"));

            ImGui.EndMenu();
        }
    }
    #endregion

    #region Export
    public void ExportTab()
    {
        if (ImGui.BeginTabItem($"{LOC.Get("TEXT_DataTransfer_Tab_Export")}##exportTab"))
        {
            UIHelper.WrappedText(LOC.Get("TEXT_DataTransfer_Export_Hint"));

            UIHelper.Spacer();
            UIHelper.SimpleHeader(
                LOC.Get("TEXT_DataTransfer_Header_Export_Directory"),
                LOC.Get("TEXT_DataTransfer_Header_Export_Directory_TT"));

            UIHelper.SinglelineTextInput("textExportDir", ref ExportDirectory);

            UIHelper.MultiButtonInput("textExportDir",
                "setDirectory", 
                LOC.Get("TEXT_DataTransfer_Action_Set_Export_Directory"),
                LOC.Get("TEXT_DataTransfer_Action_Set_Export_Directory_TT"),
                SetExportDirectory,

                "openDirectory", 
                LOC.Get("TEXT_DataTransfer_Action_Open_Export_Directory"),
                LOC.Get("TEXT_DataTransfer_Action_Open_Export_Directory_TT"),
                OpenExportDirectory);

            UIHelper.Spacer();
            UIHelper.SimpleHeader(
                LOC.Get("TEXT_DataTransfer_Header_Export_Filename"),
                LOC.Get("TEXT_DataTransfer_Header_Export_Filename_TT"));

            UIHelper.SinglelineTextInput("textExportFileName", ref ExportFilename);

            UIHelper.Spacer();
            UIHelper.SimpleHeader(
                LOC.Get("TEXT_DataTransfer_Header_Export_Type"),
                LOC.Get("TEXT_DataTransfer_Header_Export_Type_TT"));

            UIHelper.SetInputWidth();
            if (ImGui.BeginCombo("##exportType", ExportModifier.GetDisplayName()))
            {
                foreach (var entry in Enum.GetValues(typeof(ExportModifier)))
                {
                    var curExportType = (ExportModifier)entry;

                    var displayName = LOC.Get(curExportType.GetDisplayName());

                    if (ImGui.Selectable(displayName, curExportType == ExportModifier))
                    {
                        ExportModifier = curExportType;
                    }
                }

                ImGui.EndCombo();
            }

            UIHelper.Spacer();
            UIHelper.SimpleHeader(
                LOC.Get("TEXT_DataTransfer_Header_Output"),
                LOC.Get("TEXT_DataTransfer_Header_Output_TT"));

            // Has to use TextUnformatted as the CSV output string can be massive,
            // and it exceeds the internal buffers used by InputTextMultiline
            ImGui.BeginChild("OutputTextSection", new Vector2(0, 250), ImGuiChildFlags.Borders);
            ImGui.TextUnformatted(ExportString);
            ImGui.EndChild();

            UIHelper.MultiButtonInput("csvOutputActions",
                "copyToClipboard", 
                LOC.Get("TEXT_DataTransfer_Action_Copy_To_Clipboard"),
                LOC.Get("TEXT_DataTransfer_Action_Copy_To_Clipboard_TT"),
                CopyOutputToClipboard);

            UIHelper.Spacer();
            UIHelper.SimpleHeader(
                LOC.Get("TEXT_DataTransfer_Header_Actions"),
                LOC.Get("TEXT_DataTransfer_Header_Actions_TT"));

            UIHelper.MultiButtonInput("exportActions",
                "exportContainer", 
                LOC.Get("TEXT_DataTransfer_Action_Export_Container"),
                LOC.Get("TEXT_DataTransfer_Action_Export_Container_TT"),
                ExportContainerAction,

                "exportFmg", 
                LOC.Get("TEXT_DataTransfer_Action_Export_File"),
                LOC.Get("TEXT_DataTransfer_Action_Export_File_TT"),
                ExportFmgAction,

                "exportEntry",
                LOC.Get("TEXT_DataTransfer_Action_Export_Entry"),
                LOC.Get("TEXT_DataTransfer_Action_Export_Entry_TT"), 
                ExportEntryAction);

            ImGui.EndTabItem();
        }
    }

    public void SetExportDirectory()
    {
        string path;
        var result = PlatformUtils.Instance.OpenFolderDialog(LOC.Get("TEXT_DataTransfer_Dialog_Select_Export_Dest"), out path);
        if (result)
        {
            ExportDirectory = path;
        }
    }

    public void CopyOutputToClipboard()
    {
        PlatformUtils.Instance.SetClipboardText(ExportString);
    }

    public void OpenExportDirectory()
    {
        Process.Start("explorer.exe", ExportDirectory);
    }

    public void ExportContainerAction()
    {
        var curView = Editor.ViewHandler.ActiveView;
        var wrapper = curView.Selection.SelectedContainerWrapper;

        if(wrapper == null)
        {
            Smithbox.LogError<TextDataTransferTool>(LOC.Get("TEXT_DataTransfer_Log_No_Container_Selected"));
            return;
        }

        if (ExportDirectory == "")
        {
            Smithbox.LogError<TextDataTransferTool>(LOC.Get("TEXT_DataTransfer_Log_No_Export_Directory_Defined"));
            return;
        }

        var filename = "";

        if (ExportFilename == "")
        {
            filename = $"{wrapper.FileEntry.Filename}";
        }
        else
        {
            filename = ExportFilename;
        }

        if(filename == "")
        {
            Smithbox.LogError<TextDataTransferTool>(LOC.Get("TEXT_DataTransfer_Log_Filename_Empty"));
            return;
        }

        var exportMod = ExportModifier.None;

        curView.FmgExporter.InitializeExport(filename, ExportType.Container, exportMod);

    }
    public void ExportFmgAction()
    {
        var curView = Editor.ViewHandler.ActiveView;
        var wrapper = curView.Selection.SelectedContainerWrapper;
        var selectedFmgWrapper = curView.Selection.SelectedFmgWrapper;

        if (wrapper == null)
        {
            Smithbox.LogError<TextDataTransferTool>(LOC.Get("TEXT_DataTransfer_Log_No_Container_Selected"));
            return;
        }

        if (selectedFmgWrapper == null)
        {
            Smithbox.LogError<TextDataTransferTool>(LOC.Get("TEXT_DataTransfer_Log_No_Text_File_Selected"));
            return;
        }

        if (ExportDirectory == "")
        {
            Smithbox.LogError<TextDataTransferTool>(LOC.Get("TEXT_DataTransfer_Log_No_Export_Directory_Defined"));
            return;
        }

        var filename = "";

        if (ExportFilename == "")
        {
            filename = $"{wrapper.FileEntry.Filename}-{selectedFmgWrapper.ID}";
        }
        else
        {
            filename = ExportFilename;
        }

        if (filename == "")
        {
            Smithbox.LogError<TextDataTransferTool>(LOC.Get("TEXT_DataTransfer_Log_Filename_Empty"));
            return;
        }

        var exportMod = ExportModifier.None;

        curView.FmgExporter.InitializeExport(filename, ExportType.FMG, exportMod);
    }
    public void ExportEntryAction()
    {
        var curView = Editor.ViewHandler.ActiveView;
        var wrapper = curView.Selection.SelectedContainerWrapper;
        var selectedFmgWrapper = curView.Selection.SelectedFmgWrapper;
        var selectedEntry = curView.Selection._selectedFmgEntry;

        if (wrapper == null)
        {
            Smithbox.LogError<TextDataTransferTool>(LOC.Get("TEXT_DataTransfer_Log_No_Container_Selected"));
            return;
        }

        if (selectedFmgWrapper == null)
        {
            Smithbox.LogError<TextDataTransferTool>(LOC.Get("TEXT_DataTransfer_Log_No_Text_File_Selected"));
            return;
        }

        if (selectedEntry == null)
        {
            Smithbox.LogError<TextDataTransferTool>(LOC.Get("TEXT_DataTransfer_Log_No_Text_Entry_Selected"));
            return;
        }

        if (ExportDirectory == "")
        {
            Smithbox.LogError<TextDataTransferTool>(LOC.Get("TEXT_DataTransfer_Log_No_Export_Directory_Defined"));
            return;
        }

        var filename = "";

        if (ExportFilename == "")
        {
            filename = $"{wrapper.FileEntry.Filename}-{selectedFmgWrapper.ID}-{selectedEntry.ID}";
        }
        else
        {
            filename = ExportFilename;
        }

        if (filename == "")
        {
            Smithbox.LogError<TextDataTransferTool>(LOC.Get("TEXT_DataTransfer_Log_Filename_Empty"));
            return;
        }

        var exportMod = ExportModifier.None;

        curView.FmgExporter.InitializeExport(filename, ExportType.FMG_Entries, exportMod);
    }

    public void ExportMenu()
    {
        var curView = Editor.ViewHandler.ActiveView;

        if (ImGui.BeginMenu($"{LOC.Get("TEXT_DataTransfer_Menu_Header_Export")}##exportMenuHeader"))
        {
            // Container
            if (ImGui.BeginMenu($"{LOC.Get("TEXT_DataTransfer_Menu_Header_Container")}##containerMenuHeader", curView.Selection.SelectedContainerWrapper != null))
            {
                // Export Selected Container
                if (ImGui.Selectable($"{LOC.Get("TEXT_DataTransfer_Export_Selected_Container")}##exportContainerText"))
                {
                    curView.FmgExporter.DisplayExportModal(ExportType.Container);
                }

                ImGui.Separator();

                // Export Modified Text
                if (ImGui.Selectable($"{LOC.Get("TEXT_DataTransfer_Export_Modified_Text")}##containerExportModifiedText"))
                {
                    curView.FmgExporter.DisplayExportModal(ExportType.Container, ExportModifier.ModifiedOnly);
                }
                UIHelper.Tooltip(LOC.Get("TEXT_DataTransfer_Export_Modified_Text_TT"));

                // Export Unique Text
                if (ImGui.Selectable($"{LOC.Get("TEXT_DataTransfer_Export_Unique_Text")}##containerExportUniqueText"))
                {
                    curView.FmgExporter.DisplayExportModal(ExportType.Container, ExportModifier.UniqueOnly);
                }
                UIHelper.Tooltip(LOC.Get("TEXT_DataTransfer_Export_Unique_Text_TT"));

                ImGui.EndMenu();
            }
            UIHelper.Tooltip(LOC.Get("TEXT_DataTransfer_Menu_Header_Container_TT"));

            // Text File
            if (ImGui.BeginMenu($"{LOC.Get("TEXT_DataTransfer_Menu_Header_Text_File")}##textFileMenuHeader", curView.Selection.SelectedFmgWrapper != null))
            {
                // Export Text File
                if (ImGui.Selectable($"{LOC.Get("TEXT_DataTransfer_Export_Selected_Text_File")}##exportTextFileText"))
                {
                    curView.FmgExporter.DisplayExportModal(ExportType.FMG);
                }

                ImGui.Separator();

                // Export Modified Text
                if (ImGui.Selectable($"{LOC.Get("TEXT_DataTransfer_Export_Modified_Text")}##textFileExportModifiedText"))
                {
                    curView.FmgExporter.DisplayExportModal(ExportType.FMG, ExportModifier.ModifiedOnly);
                }
                UIHelper.Tooltip(LOC.Get("TEXT_DataTransfer_Export_Modified_Text_TT"));

                // Export Unique Text
                if (ImGui.Selectable($"{LOC.Get("TEXT_DataTransfer_Export_Unique_Text")}##textFileExportUniqueText"))
                {
                    curView.FmgExporter.DisplayExportModal(ExportType.FMG, ExportModifier.UniqueOnly);
                }
                UIHelper.Tooltip(LOC.Get("TEXT_DataTransfer_Export_Unique_Text_TT"));

                ImGui.EndMenu();
            }
            UIHelper.Tooltip(LOC.Get("TEXT_DataTransfer_Menu_Header_Text_File_TT"));

            // Text Entry
            if (ImGui.BeginMenu($"{LOC.Get("TEXT_DataTransfer_Menu_Header_Text_Entry")}##textEntryMenuHEader", curView.Selection._selectedFmgEntry != null))
            {
                // Export Text Entry
                if (ImGui.Selectable($"{LOC.Get("TEXT_DataTransfer_Export_Selected_Text_Entry")}##exportTextEntryText"))
                {
                    curView.FmgExporter.DisplayExportModal(ExportType.FMG_Entries);
                }

                ImGui.Separator();

                // Export Modified Text
                if (ImGui.Selectable($"{LOC.Get("TEXT_DataTransfer_Export_Modified_Text")}##textEntryExportModifiedText"))
                {
                    curView.FmgExporter.DisplayExportModal(ExportType.FMG_Entries, ExportModifier.ModifiedOnly);
                }
                UIHelper.Tooltip(LOC.Get("TEXT_DataTransfer_Export_Entry_Modified_Text_TT"));

                // Export Unique Text
                if (ImGui.Selectable($"{LOC.Get("TEXT_DataTransfer_Export_Unique_Text")}##textEntryExportUniqueText"))
                {
                    curView.FmgExporter.DisplayExportModal(ExportType.FMG_Entries, ExportModifier.UniqueOnly);
                }
                UIHelper.Tooltip(LOC.Get("TEXT_DataTransfer_Export_Entry_Unique_Text_TT"));

                ImGui.EndMenu();
            }
            UIHelper.Tooltip(LOC.Get("TEXT_DataTransfer_Menu_Header_Text_Entry_TT"));

            ImGui.EndMenu();
        }
    }

    #endregion
}

public enum TextImportMode
{
    [Display(Name = "TEXT_ENUM_TextImportMode_Append")]
    Append,

    [Display(Name = "TEXT_ENUM_TextImportMode_Overwrite")]
    Overwrite
}