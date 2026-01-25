using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class ParamDeltaPatcher
{
    public ParamEditorScreen Editor;
    public ProjectEntry Project;

    private string ExportName = "";

    public ParamDeltaPatcher(ParamEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Display()
    {
        var windowWidth = ImGui.GetWindowWidth();
        var inputBoxSize = new Vector2((windowWidth * 0.725f), 32);

        var paramData = Project.Handler.ParamData;

        if (ImGui.CollapsingHeader("Param Delta Patcher"))
        {
            if(ImGui.BeginTabBar("deltaTabs"))
            {
                if(ImGui.BeginTabItem("Import"))
                {
                    DisplayImportTab();

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Export"))
                {
                    DisplayExportTab();

                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }
        }
    }

    public void DisplayImportTab()
    {

    }

    public void DisplayExportTab()
    {
        ImGui.Text("Filename:");
        ImGui.InputText("##inputFileName", ref ExportName, 255);

        if(ImGui.Button("Generate", DPI.StandardButtonSize))
        {
            GenerateDeltaPatch();
        }

        ImGui.SameLine();

        if (ImGui.Button("View Param Deltas", DPI.StandardButtonSize))
        {
            var storageDir = ProjectUtils.GetParamDeltaFolder();

            Process.Start("explorer.exe", storageDir);
        }
    }

    public void GenerateDeltaPatch()
    {
        if (ExportName == "" || ExportName == null)
        {
            TaskLogs.AddError("Filename must not be empty.");
            return;
        }

        var writeDir = ProjectUtils.GetParamDeltaFolder();
        var writePath = Path.Combine(writeDir, $"{ExportName}.json");

        if(File.Exists(writePath))
        {
            var dialog = PlatformUtils.Instance.MessageBox($"Do you want to overwrite this delta patch file: {writePath}", "Delta Patcher", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if(dialog is DialogResult.No)
            {
                return;
            }
        }

        var deltaPatch = BuildDeltaPatch();

        if (deltaPatch.Params.Count > 0)
        {
            WriteDeltaPatch(deltaPatch, ExportName);
            TaskLogs.AddLog($"Saved param delta: {ExportName}.json");
        }
        else
        {
            TaskLogs.AddLog($"Aborted param delta as no changes were detected.");
        }
    }

    public ParamDeltaPatch BuildDeltaPatch()
    {
        var patch = new ParamDeltaPatch();

        var primaryBank = Project.Handler.ParamData.PrimaryBank;
        var vanillaBank = Project.Handler.ParamData.VanillaBank;

        foreach(var primaryParam in primaryBank.Params)
        {
            if (!vanillaBank.Params.ContainsKey(primaryParam.Key))
                continue;

            var vanillaParam = vanillaBank.Params[primaryParam.Key];

            var curRowID = 0;
            var internalIndex = 0;

            for (int i = 0; i < primaryParam.Value.Rows.Count; i++)
            {
                Param.Row row = primaryParam.Value.Rows[i];

                // Handle indexed rows
                if(row.ID == curRowID)
                {
                    var vInternalIndex = 0;

                    foreach(var vRow in vanillaParam.Rows)
                    {
                        if(vRow.ID == row.ID)
                        {
                            if(internalIndex == vInternalIndex)
                            {
                                HandleFieldComparison(row, vRow);
                            }

                            vInternalIndex++;
                        }
                    }

                    internalIndex++;
                }
                else
                {
                    internalIndex = 0;

                    var vanillaRow = vanillaParam.Rows.FirstOrDefault(e => e.ID == row.ID);
                    if(vanillaRow != null)
                    {
                        HandleFieldComparison(row, vanillaRow);
                    }
                }

                curRowID = row.ID;
            }
        }

        return patch;
    }

    public void HandleFieldComparison(Param.Row primaryRow, Param.Row vanillaRow)
    {
        if(primaryRow.DataEquals(vanillaRow))
        {
            return;
        }
        else
        {
            foreach(var col in primaryRow.Columns)
            {
                var curField = col.Def.InternalName;
                var curValue = col.GetValue(primaryRow);
            }
        }
    }

    public ParamDeltaPatch LoadDeltaPatch(string name)
    {
        var storageDir = ProjectUtils.GetParamDeltaFolder();

        var readPath = Path.Combine(storageDir, $"{ExportName}.json");

        var deltaPatch = new ParamDeltaPatch();

        if (File.Exists(readPath))
        {
            try
            {
                var filestring = File.ReadAllText(readPath);

                try
                {
                    deltaPatch = JsonSerializer.Deserialize(filestring, ParamEditorJsonSerializerContext.Default.ParamDeltaPatch);
                }
                catch (Exception e)
                {
                    TaskLogs.AddError("Failed to deserialize delta patch", e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddError("Failed to read delta patch", e);
            }
        }
        else
        {
            TaskLogs.AddError("Failed to find delta patch");
        }

        return deltaPatch;
    }

    public void WriteDeltaPatch(ParamDeltaPatch patch, string name)
    {
        if(name == "" || name == null)
        {
            TaskLogs.AddError("Failed to write delta patch as filename is empty.");
            return;
        }

        var writeDir = ProjectUtils.GetParamDeltaFolder();

        var writePath = Path.Combine(writeDir, $"{name}.json");

        if (!Directory.Exists(writeDir))
        {
            Directory.CreateDirectory(writeDir);
        }

        try
        {
            string jsonString = JsonSerializer.Serialize(patch, ParamEditorJsonSerializerContext.Default.ParamDeltaPatch);

            var fs = new FileStream(writePath, FileMode.Create);
            var data = Encoding.ASCII.GetBytes(jsonString);
            fs.Write(data, 0, data.Length);
            fs.Flush();
            fs.Dispose();
        }
        catch (Exception ex)
        {
            TaskLogs.AddError("Failed to write delta patch", ex);
        }
    }
}
