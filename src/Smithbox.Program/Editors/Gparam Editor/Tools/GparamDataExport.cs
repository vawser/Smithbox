using Hexa.NET.ImGui;
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

public static class GparamDataExport
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

        if (ImGui.BeginMenu("Export", hasFile))
        {
            // GPARAM (full file)
            if (ImGui.MenuItem("GPARAM"))
            {
                ExportGPARAM(fileEntry, curGparam);
            }
            UIHelper.Tooltip("Export the entire GPARAM to a JSON file.");

            // Param
            if (ImGui.MenuItem("Param", hasParam))
            {
                ExportGroup(fileEntry, curGparam, curParam);
            }
            UIHelper.Tooltip("Export the selected Param to a JSON file.");

            // Field
            if (ImGui.MenuItem("Field", hasField))
            {
                ExportField(fileEntry, curGparam, curParam, curField);
            }
            UIHelper.Tooltip("Export the selected Field to a JSON file.");

            // Field Value
            if (ImGui.MenuItem("Field Value", hasValue))
            {
                ExportValue(fileEntry, curGparam, curParam, curField, curValue);
            }
            UIHelper.Tooltip("Export the selected Field Value to a JSON file.");

            ImGui.EndMenu();
        }
    }

    private static string SanitizeFilename(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Concat(name.Split(invalid)).Replace(' ', '_');
    }

    public static void ExportGPARAM(FileDictionaryEntry fileEntry, GPARAM curGparam, string overrideFileName = "")
    {
        var dialog = PlatformUtils.Instance.OpenFolderDialog("Select Export Folder", out var folder);

        if (dialog)
        {
            var json = GPARAMJson.ToJson(curGparam);

            var fileName = $"{fileEntry.Filename}.{fileEntry.Extension}.json";
            if (overrideFileName != "")
            {
                fileName = $"{overrideFileName}.json";
            }

            var exportPath = Path.Combine(folder, fileName);
            File.WriteAllText(exportPath, json);
        }
    }

    public static void ExportGroup(FileDictionaryEntry fileEntry, GPARAM curGparam, GPARAM.Param curParam, string overrideFileName = "")
    {
        var dialog = PlatformUtils.Instance.OpenFolderDialog("Select Export Folder", out var folder);

        if (dialog)
        {
            var json = GPARAMJson.ToJson(curParam);
            var safeName = SanitizeFilename(curParam.Key);

            var fileName = $"{fileEntry.Filename}.{fileEntry.Extension}.param.{safeName}.json";
            if (overrideFileName != "")
            {
                fileName = $"{overrideFileName}.json";
            }

            var exportPath = Path.Combine(folder, fileName);
            File.WriteAllText(exportPath, json);
        }
    }

    public static void ExportField(FileDictionaryEntry fileEntry, GPARAM curGparam, GPARAM.Param curParam, GPARAM.IField curField, string overrideFileName = "")
    {
        var dialog = PlatformUtils.Instance.OpenFolderDialog("Select Export Folder", out var folder);

        if (dialog)
        {
            var json = GPARAMJson.ToJson(curField);
            var safeName = SanitizeFilename(curField.Key);

            var fileName = $"{fileEntry.Filename}.{fileEntry.Extension}.field.{safeName}.json";
            if (overrideFileName != "")
            {
                fileName = $"{overrideFileName}.json";
            }

            var exportPath = Path.Combine(folder, fileName);
            File.WriteAllText(exportPath, json);
        }
    }

    public static void ExportValue(FileDictionaryEntry fileEntry, GPARAM curGparam, GPARAM.Param curParam, GPARAM.IField curField, GPARAM.IFieldValue curValue, string overrideFileName = "")
    {
        var dialog = PlatformUtils.Instance.OpenFolderDialog("Select Export Folder", out var folder);

        if (dialog)
        {
            var discriminator = GPARAMJson.GetFieldTypeDiscriminator(curField);
            var json = GPARAMJson.ToJson(curValue, discriminator);
            var safeName = SanitizeFilename(curField.Key);

            var fileName = $"{fileEntry.Filename}.{fileEntry.Extension}.value.{safeName}.{curValue.ID}.json";
            if (overrideFileName != "")
            {
                fileName = $"{overrideFileName}.json";
            }

            var exportPath = Path.Combine(folder, fileName);
            File.WriteAllText(exportPath, json);
        }
    }
}
