using Hexa.NET.ImGui;
using Octokit;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditor;
public class GparamDataImport
{
    public static void DisplayMenu(GparamEditorView view)
    {
        var project = view.Project;
        var selection = view.Selection;
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
                ImportGPARAM(project, view, fileEntry, curGparam);
            }
            UIHelper.Tooltip("Replace the entire GPARAM from a previously exported JSON file.");

            // Param
            if (ImGui.MenuItem("Param", hasParam))
            {
                ImportGroup(project, view, fileEntry, curGparam, curParam);
            }
            UIHelper.Tooltip("Replace the selected Param from a previously exported JSON file.");

            // Field
            if (ImGui.MenuItem("Field", hasField))
            {
                ImportField(project, view, fileEntry, curGparam, curParam, curField);
            }
            UIHelper.Tooltip("Replace the selected Field from a previously exported JSON file.");

            // Field Value
            if (ImGui.MenuItem("Field Value", hasValue))
            {
                ImportValue(project, view, fileEntry, curGparam, curParam, curField, curValue, overwrite);
            }
            UIHelper.Tooltip("Replace the selected Field Value from a previously exported JSON file.");

            ImGui.Checkbox("Overwrite Value on Import", ref CFG.Current.GparamEditor_Data_Import_Overwrite);
            UIHelper.Tooltip("If enabled, the imported data will overwrite existing values with the same ID. Only applies to values.");


            ImGui.EndMenu();
        }
    }

    public static void ImportGPARAM(ProjectEntry project, GparamEditorView view, FileDictionaryEntry fileEntry, GPARAM curGparam)
    {
        var dialog = PlatformUtils.Instance.OpenFileDialog("Select GPARAM JSON", out var path);

        if (dialog && path.EndsWith(".json") && Path.Exists(path))
        {
            var json = File.ReadAllText(path);
            var newData = GPARAMJson.FromJson(json);

            var action = new ImportGparamAction(project, view, fileEntry, curGparam, newData);
            view.ActionManager.ExecuteAction(action);
        }

        // Clear selection
        view.Selection.ResetGparamFileSelection();
    }

    public static void ImportGroup(ProjectEntry project, GparamEditorView view, FileDictionaryEntry fileEntry, GPARAM curGparam, GPARAM.Param curParam)
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

    public static void ImportField(ProjectEntry project, GparamEditorView view, FileDictionaryEntry fileEntry, GPARAM curGparam, GPARAM.Param curParam, GPARAM.IField curField)
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

    public static void ImportValue(ProjectEntry project, GparamEditorView view, FileDictionaryEntry fileEntry, GPARAM curGparam, GPARAM.Param curParam, GPARAM.IField curField, GPARAM.IFieldValue curValue, bool overwrite)
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
}
