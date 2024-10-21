using ImGuiNET;
using Microsoft.AspNetCore.Components.Forms;
using SoulsFormats;
using StudioCore.Banks.AliasBank;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Resource.Locators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

public static class FmgExporter
{
    public static void OnGui()
    {
        var editor = Smithbox.EditorHandler.TextEditor;

        editor.TextExportModal.Display();
    }

    public static void DisplayExportList(bool ignoreSelected = false)
    {
        if (ImGui.Selectable("All"))
        {
            FmgExporter.DisplayExportModal(ExportType.All);
        }
        UIHelper.ShowHoverTooltip("All entries from the currently selected FMG will be exported.");

        if (!ignoreSelected)
        {
            if (ImGui.Selectable("Selected"))
            {
                FmgExporter.DisplayExportModal(ExportType.Selected);
            }
            UIHelper.ShowHoverTooltip("Only selected entries from the currently selected FMG will be exported.");
        }
    }

    public static void DisplayExportModal(ExportType exportType)
    {
        var editor = Smithbox.EditorHandler.TextEditor;

        editor.TextExportModal.ShowModal = true;
        editor.TextExportModal.ExportType = exportType;
    }

    /// <summary>
    /// Export the FMG entries and save them under the specified wrapper name.
    /// </summary>
    public static void ProcessExport(string wrapperName, ExportType exportType)
    {
        var editor = Smithbox.EditorHandler.TextEditor;

        var selectedFmgInfo = editor.Selection.SelectedFmgWrapper;
        var selectedFmgEntries = editor.Selection.FmgEntryMultiselect.StoredEntries;

        var fmgWrapper = new StoredFmgWrapper();
        fmgWrapper.Name = wrapperName;

        // Create new FMG for the wrapper
        var exportFmg = new FMG();
        exportFmg.Name = selectedFmgInfo.File.Name;
        exportFmg.Version = selectedFmgInfo.File.Version;
        exportFmg.BigEndian = selectedFmgInfo.File.BigEndian;
        exportFmg.Entries = new List<FMG.Entry>();

        if (exportType is ExportType.All)
        {
            foreach(var entry in selectedFmgInfo.File.Entries)
            {
                var newEntry = new FMG.Entry(exportFmg, entry.ID, entry.Text);
                exportFmg.Entries.Add(newEntry);
            }

            fmgWrapper.Fmg = exportFmg;
        }
        else if(exportType is ExportType.Selected)
        {
            foreach (var (key, entry) in selectedFmgEntries)
            {
                var newEntry = new FMG.Entry(exportFmg, entry.ID, entry.Text);
                exportFmg.Entries.Add(newEntry);
            }

            fmgWrapper.Fmg = exportFmg;
        }

        WriteWrapper(fmgWrapper);
    }

    public static void WriteWrapper(StoredFmgWrapper wrapper)
    {
        var writeDir = TextLocator.GetFmgWrapperDirectory();
        var writePath = $"{writeDir}\\{wrapper.Name}.json";

        if(!Directory.Exists(writeDir))
        {
            Directory.CreateDirectory(writeDir);
        }

        var proceed = false;

        if (File.Exists(writePath))
        {
            DialogResult result = PlatformUtils.Instance.MessageBox(
                    $"Stored text already exists under this name. Overwrite?", 
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
            string jsonString = JsonSerializer.Serialize(wrapper, typeof(StoredFmgWrapper), StoredFmgWrapperSerializationContext.Default);

            try
            {
                var fs = new FileStream(writePath, System.IO.FileMode.Create);
                var data = Encoding.ASCII.GetBytes(jsonString);
                fs.Write(data, 0, data.Length);
                fs.Flush();
                fs.Dispose();

                TaskLogs.AddLog($"Exported stored text as {wrapper.Name} at: {writePath}");
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog($"{ex}");
            }
        }
    }
}

