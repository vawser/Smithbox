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
        if (ImGui.BeginMenu("Data Transfer"))
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
        if (ImGui.BeginTabItem($"Import"))
        {
            UIHelper.WrappedText("Use this section to import JSON data, applying the data to your current project.");

            UIHelper.Spacer();
            UIHelper.SimpleHeader("Import Type", "Determines how the data is applied.");

            UIHelper.SetInputWidth();
            if (ImGui.BeginCombo("##importType", ImportType.GetDisplayName()))
            {
                foreach (var entry in Enum.GetValues(typeof(TextImportMode)))
                {
                    var curImportType = (TextImportMode)entry;

                    if (ImGui.Selectable($"{curImportType.GetDisplayName()}", curImportType == ImportType))
                    {
                        ImportType = curImportType;
                    }
                }

                ImGui.EndCombo();
            }

            UIHelper.Spacer();
            UIHelper.SimpleHeader("Actions", "");

            UIHelper.MultiButtonInput("importActions",
                "importTextFromFile", "Import from File", "", ImportTextFromFile);

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

        if (PlatformUtils.Instance.OpenFileDialog("Select text JSON", ["json"], out var path))
        {
            if (!File.Exists(path))
            {
                DialogResult message = PlatformUtils.Instance.MessageBox(
                    "Selected file is invalid.", "Error",
                    MessageBoxButtons.OK);
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

        if (ImGui.BeginMenu("Import"))
        {
            if (ImGui.BeginMenu("File", curView.Selection.SelectedContainerWrapper != null))
            {
                curView.FmgImporter.DisplayImportList(FmgImportType.Container);

                ImGui.EndMenu();
            }
            UIHelper.Tooltip("Import the selected text file on the container level, replacing all FMGs and their associated entries (if applicable).");

            if (ImGui.BeginMenu("Text File", curView.Selection.SelectedFmgWrapper != null))
            {
                curView.FmgImporter.DisplayImportList(FmgImportType.FMG);

                ImGui.EndMenu();
            }
            UIHelper.Tooltip("Import the selected text file on the FMG level, replacing all associated entries (if applicable).");

            if (ImGui.BeginMenu("Text Entry", curView.Selection._selectedFmgEntry != null))
            {
                curView.FmgImporter.DisplayImportList(FmgImportType.FMG_Entries);

                ImGui.EndMenu();
            }
            UIHelper.Tooltip("Import the selected text file on the FMG Entry level, replacing all matching entries.");

            ImGui.EndMenu();
        }
    }
    #endregion

    #region Export
    public void ExportTab()
    {
        if (ImGui.BeginTabItem($"Export"))
        {
            UIHelper.WrappedText("Use this section to export JSON data from your current project.");

            UIHelper.Spacer();
            UIHelper.SimpleHeader("Export Directory", "The directory to export the JSON data to.");
            UIHelper.SinglelineTextInput("textExportDir", ref ExportDirectory);

            UIHelper.MultiButtonInput("textExportDir",
                "setDirectory", "Set Export Directory", "", SetExportDirectory,
                "openDirectory", "Open Export Directory", "", OpenExportDirectory);

            UIHelper.Spacer();
            UIHelper.SimpleHeader("Export Filename", "The file to use for the exported file (blank will use an auto-generated name).");
            UIHelper.SinglelineTextInput("textExportFileName", ref ExportFilename);

            UIHelper.Spacer();
            UIHelper.SimpleHeader("Export Type", "Determines which data is exported.");

            UIHelper.SetInputWidth();
            if (ImGui.BeginCombo("##exportType", ExportModifier.GetDisplayName()))
            {
                foreach (var entry in Enum.GetValues(typeof(ExportModifier)))
                {
                    var curExportType = (ExportModifier)entry;

                    if (ImGui.Selectable($"{curExportType.GetDisplayName()}", curExportType == ExportModifier))
                    {
                        ExportModifier = curExportType;
                    }
                }

                ImGui.EndCombo();
            }

            UIHelper.Spacer();
            UIHelper.SimpleHeader("Output", "");
            // Has to use TextUnformatted as the CSV output string can be massive,
            // and it exceeds the internal buffers used by InputTextMultiline
            ImGui.BeginChild("OutputTextSection", new Vector2(0, 250), ImGuiChildFlags.Borders);
            ImGui.TextUnformatted(ExportString);
            ImGui.EndChild();

            UIHelper.MultiButtonInput("csvOutputActions",
                "copyToClipboard", "Copy to Clipboard", "Copy the output to the clibpaord", CopyOutputToClipboard);

            UIHelper.Spacer();
            UIHelper.SimpleHeader("Actions", "");

            UIHelper.MultiButtonInput("exportActions",
                "exportContainer", "Export Selected Container", "", ExportContainerAction,
                "exportFmg", "Export Selected Text File", "", ExportFmgAction,
                "exportEntry", "Export Selected Text Entry", "", ExportEntryAction);

            ImGui.EndTabItem();
        }
    }

    public void SetExportDirectory()
    {
        string path;
        var result = PlatformUtils.Instance.OpenFolderDialog("Select Export Destination", out path);
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
            Smithbox.LogError<TextDataTransferTool>("No container has been selected.");
            return;
        }

        if (ExportDirectory == "")
        {
            Smithbox.LogError<TextDataTransferTool>("No export directory has been defined.");
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
            Smithbox.LogError<TextDataTransferTool>("Filename is empty.");
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
            Smithbox.LogError<TextDataTransferTool>("No container has been selected.");
            return;
        }

        if (selectedFmgWrapper == null)
        {
            Smithbox.LogError<TextDataTransferTool>("No text file has been selected.");
            return;
        }

        if (ExportDirectory == "")
        {
            Smithbox.LogError<TextDataTransferTool>("No export directory has been defined.");
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
            Smithbox.LogError<TextDataTransferTool>("Filename is empty.");
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
            Smithbox.LogError<TextDataTransferTool>("No container has been selected.");
            return;
        }

        if (selectedFmgWrapper == null)
        {
            Smithbox.LogError<TextDataTransferTool>("No text file has been selected.");
            return;
        }

        if (selectedEntry == null)
        {
            Smithbox.LogError<TextDataTransferTool>("No text entry has been selected.");
            return;
        }

        if (ExportDirectory == "")
        {
            Smithbox.LogError<TextDataTransferTool>("No export directory has been defined.");
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
            Smithbox.LogError<TextDataTransferTool>("Filename is empty.");
            return;
        }

        var exportMod = ExportModifier.None;

        curView.FmgExporter.InitializeExport(filename, ExportType.FMG_Entries, exportMod);
    }

    public void ExportMenu()
    {
        var curView = Editor.ViewHandler.ActiveView;

        if (ImGui.BeginMenu("Export"))
        {
            // File
            if (ImGui.BeginMenu("File", curView.Selection.SelectedContainerWrapper != null))
            {
                if (ImGui.Selectable("Export Selected File"))
                {
                    curView.FmgExporter.DisplayExportModal(ExportType.Container);
                }

                ImGui.Separator();

                if (ImGui.Selectable("Export Modified Text"))
                {
                    curView.FmgExporter.DisplayExportModal(ExportType.Container, ExportModifier.ModifiedOnly);
                }
                UIHelper.Tooltip("Only include Text Entries (and therefore the associated Text Files)) that are considered 'modified'.");

                if (ImGui.Selectable("Export Unique Text"))
                {
                    curView.FmgExporter.DisplayExportModal(ExportType.Container, ExportModifier.UniqueOnly);
                }
                UIHelper.Tooltip("Only include Text Entries (and therefore the associated Text Files) that are considered 'unique'.");

                ImGui.EndMenu();
            }
            UIHelper.Tooltip("Export your currently selected File (including all of its Text Files and their Text Entries) to a export text file.");

            // FMG
            if (ImGui.BeginMenu("Text File", curView.Selection.SelectedFmgWrapper != null))
            {
                if (ImGui.Selectable("Export Selected Text File"))
                {
                    curView.FmgExporter.DisplayExportModal(ExportType.FMG);
                }

                ImGui.Separator();

                if (ImGui.Selectable("Export Modified Text"))
                {
                    curView.FmgExporter.DisplayExportModal(ExportType.FMG, ExportModifier.ModifiedOnly);
                }
                UIHelper.Tooltip("Only include Text Entries (and therefore the associated Text Files) that are considered 'modified'.");

                if (ImGui.Selectable("Export Unique Text"))
                {
                    curView.FmgExporter.DisplayExportModal(ExportType.FMG, ExportModifier.UniqueOnly);
                }
                UIHelper.Tooltip("Only include Text Entries (and therefore the associated Text Files) that are considered 'unique'.");

                ImGui.EndMenu();
            }
            UIHelper.Tooltip("Export your currently selected Text File (including all of its entries) to a export text file.");

            // FMG Entries
            if (ImGui.BeginMenu("Text Entry", curView.Selection._selectedFmgEntry != null))
            {
                if (ImGui.Selectable("Export Selected Text Entries"))
                {
                    curView.FmgExporter.DisplayExportModal(ExportType.FMG_Entries);
                }

                ImGui.Separator();

                if (ImGui.Selectable("Export Modified Text"))
                {
                    curView.FmgExporter.DisplayExportModal(ExportType.FMG_Entries, ExportModifier.ModifiedOnly);
                }
                UIHelper.Tooltip("Only include FMG entries that are considered 'modified'.");

                if (ImGui.Selectable("Export Unique Text"))
                {
                    curView.FmgExporter.DisplayExportModal(ExportType.FMG_Entries, ExportModifier.UniqueOnly);
                }
                UIHelper.Tooltip("Only include FMG entries that are considered 'unique'.");

                ImGui.EndMenu();
            }
            UIHelper.Tooltip("Export your currently selected FMG Entries to a export text file.");

            ImGui.EndMenu();
        }
    }

    #endregion
}

public enum TextImportMode
{
    [Display(Name = "Append")]
    Append,
    [Display(Name = "Overwrite")]
    Overwrite
}