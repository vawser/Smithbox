using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Numerics;

namespace StudioCore.Editors.GparamEditor;

public class GparamDataTransferTool
{
    private GparamEditorView View;
    private ProjectEntry Project;

    public GparamImportType ImportType = GparamImportType.Overwrite;

    public string ExportDirectory = "";
    public string ExportString = "";

    public GparamDataTransferTool(GparamEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
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

    public void DisplayDropdown()
    {
        if (ImGui.BeginMenu("Data Transfer"))
        {
            ImportMenu();
            ExportMenu();

            ImGui.EndMenu();
        }
    }

    #region Import
    public void ImportTab()
    {
        var selection = View.Selection;
        var fileEntry = selection?.SelectedFileEntry;
        var curGparam = selection?.GetSelectedGparam();
        var curParam = selection.GetSelectedGroup();
        var curField = selection.GetSelectedField();
        var curValue = selection.GetSelectedValue();

        if (ImGui.BeginTabItem($"Import"))
        {
            GUI.WrappedText("Use this section to import JSON data, applying the data to your current project.");

            GUI.Spacer();
            GUI.SimpleHeader("Value Import Type", "Determines how the data is applied.");

            GUI.SetInputWidth();
            if (ImGui.BeginCombo("##importType", ImportType.GetDisplayName()))
            {
                foreach (var entry in Enum.GetValues(typeof(GparamImportType)))
                {
                    var curImportType = (GparamImportType)entry;

                    if (ImGui.Selectable($"{curImportType.GetDisplayName()}", curImportType == ImportType))
                    {
                        ImportType = curImportType;
                    }
                }

                ImGui.EndCombo();
            }

            GUI.Spacer();
            GUI.SimpleHeader("Actions", "");

            GUI.MultiButtonInput("importActions",
                "importGparam", "Import GPARAM", "", ImportGparamAction,
                "importGroup", "Import Group", "", ImportGroupAction,
                "importField", "Import Field", "", ImportFieldAction,
                "importValue", "Import Value", "", ImportValueAction);

            ImGui.EndTabItem();
        }
    }

    public void ImportGparamAction()
    {
        var selection = View.Selection;
        var fileEntry = selection?.SelectedFileEntry;
        var curGparam = selection?.GetSelectedGparam();

        if (curGparam == null)
        {
            Smithbox.LogError<GparamDataTransferTool>("No file has been selected.");
            return;
        }

        ImportGPARAM(Project, View, fileEntry, curGparam);

        Smithbox.Log<GparamDataTransferTool>($"Overwritten the contents of {fileEntry.Filename} with the JSON data.");
    }

    public void ImportGroupAction()
    {
        var selection = View.Selection;
        var fileEntry = selection?.SelectedFileEntry;
        var curGparam = selection?.GetSelectedGparam();
        var curParam = selection.GetSelectedGroup();

        if (curGparam == null)
        {
            Smithbox.LogError<GparamDataTransferTool>("No file has been selected.");
            return;
        }

        if (curParam == null)
        {
            Smithbox.LogError<GparamDataTransferTool>("No group has been selected.");
            return;
        }

        ImportGroup(Project, View, fileEntry, curGparam, curParam);

        Smithbox.Log<GparamDataTransferTool>($"Overwritten the contents of {curParam.Name} with the JSON data.");
    }

    public void ImportFieldAction()
    {
        var selection = View.Selection;
        var fileEntry = selection?.SelectedFileEntry;
        var curGparam = selection?.GetSelectedGparam();
        var curParam = selection.GetSelectedGroup();
        var curField = selection.GetSelectedField();

        if (curGparam == null)
        {
            Smithbox.LogError<GparamDataTransferTool>("No file has been selected.");
            return;
        }

        if (curParam == null)
        {
            Smithbox.LogError<GparamDataTransferTool>("No group has been selected.");
            return;
        }

        if (curField == null)
        {
            Smithbox.LogError<GparamDataTransferTool>("No field has been selected.");
            return;
        }

        ImportField(Project, View, fileEntry, curGparam, curParam, curField);

        Smithbox.Log<GparamDataTransferTool>($"Overwritten the contents of {curField.Name} with the JSON data.");
    }

    public void ImportValueAction()
    {
        var selection = View.Selection;
        var fileEntry = selection?.SelectedFileEntry;
        var curGparam = selection?.GetSelectedGparam();
        var curParam = selection.GetSelectedGroup();
        var curField = selection.GetSelectedField();
        var curValue = selection.GetSelectedValue();

        if (curGparam == null)
        {
            Smithbox.LogError<GparamDataTransferTool>("No file has been selected.");
            return;
        }

        if (curParam == null)
        {
            Smithbox.LogError<GparamDataTransferTool>("No group has been selected.");
            return;
        }

        if (curField == null)
        {
            Smithbox.LogError<GparamDataTransferTool>("No field has been selected.");
            return;
        }

        if (curValue == null)
        {
            Smithbox.LogError<GparamDataTransferTool>("No value has been selected.");
            return;
        }

        if (ImportType is GparamImportType.Overwrite)
        {
            ImportValue(Project, View, fileEntry, curGparam, curParam, curField, curValue, true);
            Smithbox.Log<GparamDataTransferTool>($"Overwritten the {curValue.ID} with in the value list of {curField.Name} with the JSON data.");
        }
        else if(ImportType is GparamImportType.Append)
        {
            ImportValue(Project, View, fileEntry, curGparam, curParam, curField, curValue, false);
            Smithbox.Log<GparamDataTransferTool>($"Appends the JSON data to the value list of {curField.Name}");
        }

    }

    public void ImportMenu()
    {
        var selection = View.Selection;
        var fileEntry = selection?.SelectedFileEntry;
        var curGparam = selection?.GetSelectedGparam();
        var curParam = selection.GetSelectedGroup();
        var curField = selection.GetSelectedField();
        var curValue = selection.GetSelectedValue();

        bool hasFile = fileEntry != null;
        bool hasParam = hasFile && curParam != null;
        bool hasField = hasParam && curField != null;
        bool hasValue = hasField && curValue != null;

        bool overwrite = CFG.Current.GparamEditor_Data_Import_Overwrite;

        if (ImGui.BeginMenu("Import", hasFile))
        {
            // GPARAM (full file)
            if (ImGui.MenuItem("GPARAM"))
            {
                ImportGPARAM(Project, View, fileEntry, curGparam);
            }
            GUI.Tooltip("Replace the entire GPARAM from a previously exported JSON file.");

            // Param
            if (ImGui.MenuItem("Param", hasParam))
            {
                ImportGroup(Project, View, fileEntry, curGparam, curParam);
            }
            GUI.Tooltip("Replace the selected Param from a previously exported JSON file.");

            // Field
            if (ImGui.MenuItem("Field", hasField))
            {
                ImportField(Project, View, fileEntry, curGparam, curParam, curField);
            }
            GUI.Tooltip("Replace the selected Field from a previously exported JSON file.");

            // Field Value
            if (ImGui.MenuItem("Field Value", hasValue))
            {
                ImportValue(Project, View, fileEntry, curGparam, curParam, curField, curValue, overwrite);
            }
            GUI.Tooltip("Replace the selected Field Value from a previously exported JSON file.");

            ImGui.Checkbox("Overwrite Value on Import", ref CFG.Current.GparamEditor_Data_Import_Overwrite);
            GUI.Tooltip("If enabled, the imported data will overwrite existing values with the same ID. Only applies to values.");


            ImGui.EndMenu();
        }
    }

    public void ImportGPARAM(ProjectEntry project, GparamEditorView view, FileDictionaryEntry fileEntry, GPARAM curGparam)
    {
        var dialog = PlatformUtils.Instance.OpenFileDialog("Select GPARAM JSON", out var path);

        if (dialog && path.EndsWith(".json") && Path.Exists(path))
        {
            var json = File.ReadAllText(path);
            var newData = GPARAMJson.FromJson(json);

            if (project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
            {
                newData.WideStrings = false;
            }
            else
            {
                newData.WideStrings = true;
            }

            var action = new ImportGparamAction(project, view, fileEntry, curGparam, newData);
            view.ActionManager.ExecuteAction(action);
        }

        // Clear selection
        view.Selection.ResetGparamFileSelection();
    }

    public void ImportGroup(ProjectEntry project, GparamEditorView view, FileDictionaryEntry fileEntry, GPARAM curGparam, GPARAM.Param curParam)
    {
        var dialog = PlatformUtils.Instance.OpenFileDialog("Select Param JSON", out var path);

        if (dialog && path.EndsWith(".json") && Path.Exists(path))
        {
            var json = File.ReadAllText(path);
            var newParam = GPARAMJson.ParamFromJson(json);

            var action = new ImportParamAction(project, view, fileEntry, curGparam, curParam, newParam);
            view.ActionManager.ExecuteAction(action);
        }
    }

    public void ImportField(ProjectEntry project, GparamEditorView view, FileDictionaryEntry fileEntry, GPARAM curGparam, GPARAM.Param curParam, GPARAM.IField curField)
    {
        var dialog = PlatformUtils.Instance.OpenFileDialog("Select Field JSON", out var path);

        if (dialog && path.EndsWith(".json") && Path.Exists(path))
        {
            var json = File.ReadAllText(path);
            var newField = GPARAMJson.FieldFromJson(json);

            var action = new ImportFieldAction(project, view, fileEntry, curGparam, curParam, curField, newField);
            view.ActionManager.ExecuteAction(action);
        }
    }

    public void ImportValue(ProjectEntry project, GparamEditorView view, FileDictionaryEntry fileEntry, GPARAM curGparam, GPARAM.Param curParam, GPARAM.IField curField, GPARAM.IFieldValue curValue, bool overwrite)
    {
        var dialog = PlatformUtils.Instance.OpenFileDialog("Select Field Value JSON", out var path);

        if (dialog && path.EndsWith(".json") && Path.Exists(path))
        {
            var json = File.ReadAllText(path);
            var newValue = GPARAMJson.FieldValueFromJson(json);

            var action = new InsertFieldValueAction(project, view, fileEntry, curGparam, curParam, curField, curValue, newValue, overwrite);
            view.ActionManager.ExecuteAction(action);
        }
    }

    #endregion

    #region Export
    public void ExportTab()
    {
        if (ImGui.BeginTabItem($"Export"))
        {
            GUI.WrappedText("Use this section to export JSON data from your current project.");

            GUI.Spacer();
            GUI.SimpleHeader("Export Directory", "The directory to export the CSV data to.");
            GUI.SinglelineTextInput("csvExportDir", ref ExportDirectory);

            GUI.MultiButtonInput("csvExportDir",
                "setDirectory", "Set Export Directory", "", SetExportDirectory,
                "openDirectory", "Open Export Directory", "", OpenExportDirectory);

            GUI.Spacer();
            GUI.SimpleHeader("Output", "");
            // Has to use TextUnformatted as the CSV output string can be massive,
            // and it exceeds the internal buffers used by InputTextMultiline
            ImGui.BeginChild("OutputTextSection", new Vector2(0, 250), ImGuiChildFlags.Borders);
            ImGui.TextUnformatted(ExportString);
            ImGui.EndChild();

            GUI.MultiButtonInput("csvOutputActions",
                "copyToClipboard", "Copy to Clipboard", "Copy the output to the clibpaord", CopyOutputToClipboard);

            GUI.Spacer();
            GUI.SimpleHeader("Actions", "");

            GUI.MultiButtonInput("exportActions",
                "exportGparam", "Export Selected GPARAM", "", ExportGparamAction,
                "exportGroup", "Export Selected Group", "", ExportGroupAction,
                "exportField", "Export Selected Field", "", ExportFieldAction,
                "exportValue", "Export Selected Value", "", ExportValueAction);


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

    public void ExportGparamAction()
    {
        var selection = View.Selection;
        var fileEntry = selection?.SelectedFileEntry;
        var curGparam = selection?.GetSelectedGparam();

        if (curGparam == null)
        {
            Smithbox.LogError<GparamDataTransferTool>("No file has been selected.");
            return;
        }

        ExportGparamFile(fileEntry, curGparam);
    }

    public void ExportGroupAction()
    {

        var selection = View.Selection;
        var fileEntry = selection?.SelectedFileEntry;
        var curGparam = selection?.GetSelectedGparam();
        var curParam = selection.GetSelectedGroup();

        if (curGparam == null)
        {
            Smithbox.LogError<GparamDataTransferTool>("No file has been selected.");
            return;
        }

        if (curParam == null)
        {
            Smithbox.LogError<GparamDataTransferTool>("No group has been selected.");
            return;
        }

        ExportGroupFile(fileEntry, curGparam, curParam);
    }

    public void ExportFieldAction()
    {

        var selection = View.Selection;
        var fileEntry = selection?.SelectedFileEntry;
        var curGparam = selection?.GetSelectedGparam();
        var curParam = selection.GetSelectedGroup();
        var curField = selection.GetSelectedField();

        if (curGparam == null)
        {
            Smithbox.LogError<GparamDataTransferTool>("No file has been selected.");
            return;
        }

        if (curParam == null)
        {
            Smithbox.LogError<GparamDataTransferTool>("No group has been selected.");
            return;
        }

        if (curField == null)
        {
            Smithbox.LogError<GparamDataTransferTool>("No field has been selected.");
            return;
        }

        ExportFieldFile(fileEntry, curGparam, curParam, curField);
    }

    public void ExportValueAction()
    {

        var selection = View.Selection;
        var fileEntry = selection?.SelectedFileEntry;
        var curGparam = selection?.GetSelectedGparam();
        var curParam = selection.GetSelectedGroup();
        var curField = selection.GetSelectedField();
        var curValue = selection.GetSelectedValue();

        if (curGparam == null)
        {
            Smithbox.LogError<GparamDataTransferTool>("No file has been selected.");
            return;
        }

        if (curParam == null)
        {
            Smithbox.LogError<GparamDataTransferTool>("No group has been selected.");
            return;
        }

        if (curField == null)
        {
            Smithbox.LogError<GparamDataTransferTool>("No field has been selected.");
            return;
        }

        if (curValue == null)
        {
            Smithbox.LogError<GparamDataTransferTool>("No value has been selected.");
            return;
        }

        ExportValueFile(fileEntry, curGparam, curParam, curField, curValue);
    }

    public void ExportMenu()
    {
        var selection = View.Selection;
        var fileEntry = selection?.SelectedFileEntry;
        var curGparam = selection?.GetSelectedGparam();
        var curParam = selection.GetSelectedGroup();
        var curField = selection.GetSelectedField();
        var curValue = selection.GetSelectedValue();

        bool hasFile = fileEntry != null;
        bool hasParam = hasFile && curParam != null;
        bool hasField = hasParam && curField != null;
        bool hasValue = hasField && curValue != null;

        if (ImGui.BeginMenu("Export", hasFile))
        {
            // GPARAM (full file)
            if (ImGui.MenuItem("GPARAM"))
            {
                ExportDirectory = ""; // Clear this so the user is always prompted to select location 
                ExportGparamFile(fileEntry, curGparam);
            }
            GUI.Tooltip("Export the entire GPARAM to a JSON file.");

            // Param
            if (ImGui.MenuItem("Param", hasParam))
            {
                ExportDirectory = ""; // Clear this so the user is always prompted to select location
                ExportGroupFile(fileEntry, curGparam, curParam);
            }
            GUI.Tooltip("Export the selected Param to a JSON file.");

            // Field
            if (ImGui.MenuItem("Field", hasField))
            {
                ExportDirectory = ""; // Clear this so the user is always prompted to select location
                ExportFieldFile(fileEntry, curGparam, curParam, curField);
            }
            GUI.Tooltip("Export the selected Field to a JSON file.");

            // Field Value
            if (ImGui.MenuItem("Field Value", hasValue))
            {
                ExportDirectory = ""; // Clear this so the user is always prompted to select location
                ExportValueFile(fileEntry, curGparam, curParam, curField, curValue);
            }
            GUI.Tooltip("Export the selected Field Value to a JSON file.");

            ImGui.EndMenu();
        }
    }
    private string SanitizeFilename(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Concat(name.Split(invalid)).Replace(' ', '_');
    }

    public void ExportGparamFile(FileDictionaryEntry fileEntry, GPARAM curGparam, string overrideFileName = "")
    {
        if (ExportDirectory == "")
        {
            var dialog = PlatformUtils.Instance.OpenFolderDialog("Select Export Folder", out var folder);

            if (dialog)
            {
                ExportDirectory = folder;
            }
        }

        if(Directory.Exists(ExportDirectory))
        {
            var json = GPARAMJson.ToJson(curGparam);

            var fileName = $"{fileEntry.Filename}.{fileEntry.Extension}.json";
            if (overrideFileName != "")
            {
                fileName = $"{overrideFileName}.json";
            }

            var exportPath = Path.Combine(ExportDirectory, fileName);
            File.WriteAllText(exportPath, json);

            ExportString = json;

            Smithbox.Log<GparamDataTransferTool>($"Exported GPARAM: {exportPath}.");
        }
    }

    public void ExportGroupFile(FileDictionaryEntry fileEntry, GPARAM curGparam, GPARAM.Param curParam, string overrideFileName = "")
    {
        if (ExportDirectory == "")
        {
            var dialog = PlatformUtils.Instance.OpenFolderDialog("Select Export Folder", out var folder);

            if (dialog)
            {
                ExportDirectory = folder;
            }
        }

        if (Directory.Exists(ExportDirectory))
        {
            var json = GPARAMJson.ToJson(curParam);
            var safeName = SanitizeFilename(curParam.Key);

            var fileName = $"{fileEntry.Filename}.{fileEntry.Extension}.param.{safeName}.json";
            if (overrideFileName != "")
            {
                fileName = $"{overrideFileName}.json";
            }

            var exportPath = Path.Combine(ExportDirectory, fileName);
            File.WriteAllText(exportPath, json);

            ExportString = json;

            Smithbox.Log<GparamDataTransferTool>($"Exported GPARAM group: {exportPath}.");
        }
    }

    public void ExportFieldFile(FileDictionaryEntry fileEntry, GPARAM curGparam, GPARAM.Param curParam, GPARAM.IField curField, string overrideFileName = "")
    {
        if (ExportDirectory == "")
        {
            var dialog = PlatformUtils.Instance.OpenFolderDialog("Select Export Folder", out var folder);

            if (dialog)
            {
                ExportDirectory = folder;
            }
        }

        if (Directory.Exists(ExportDirectory))
        {
            var json = GPARAMJson.ToJson(curField);
            var safeName = SanitizeFilename(curField.Key);

            var fileName = $"{fileEntry.Filename}.{fileEntry.Extension}.field.{safeName}.json";
            if (overrideFileName != "")
            {
                fileName = $"{overrideFileName}.json";
            }

            var exportPath = Path.Combine(ExportDirectory, fileName);
            File.WriteAllText(exportPath, json);

            ExportString = json;

            Smithbox.Log<GparamDataTransferTool>($"Exported GPARAM field: {exportPath}.");
        }
    }

    public void ExportValueFile(FileDictionaryEntry fileEntry, GPARAM curGparam, GPARAM.Param curParam, GPARAM.IField curField, GPARAM.IFieldValue curValue, string overrideFileName = "")
    {
        if (ExportDirectory == "")
        {
            var dialog = PlatformUtils.Instance.OpenFolderDialog("Select Export Folder", out var folder);

            if (dialog)
            {
                ExportDirectory = folder;
            }
        }

        if (Directory.Exists(ExportDirectory))
        {
            var discriminator = GPARAMJson.GetFieldTypeDiscriminator(curField);
            var json = GPARAMJson.ToJson(curValue, discriminator);
            var safeName = SanitizeFilename(curField.Key);

            var fileName = $"{fileEntry.Filename}.{fileEntry.Extension}.value.{safeName}.{curValue.ID}.json";
            if (overrideFileName != "")
            {
                fileName = $"{overrideFileName}.json";
            }

            var exportPath = Path.Combine(ExportDirectory, fileName);
            File.WriteAllText(exportPath, json);

            ExportString = json;

            Smithbox.Log<GparamDataTransferTool>($"Exported GPARAM value: {exportPath}.");
        }
    }
    #endregion
}

public enum GparamImportType
{
    [Display(Name ="Overwrite")]
    Overwrite,
    
    [Display(Name = "Append")]
    Append
}