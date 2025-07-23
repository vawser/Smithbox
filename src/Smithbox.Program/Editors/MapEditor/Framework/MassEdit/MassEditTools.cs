using Hexa.NET.ImGui;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Numerics;
using System.Text.Json;

namespace StudioCore.Editors.MapEditor.Framework.MassEdit;

public class MassEditTools
{
    private MapEditorScreen Editor;
    private MassEditHandler Handler;

    private string BackupDir = "";
    private string TemplateDir = "";

    public bool ShowToolView = true;

    public MassEditTools(MapEditorScreen screen, MassEditHandler handler)
    {
        Editor = screen;
        Handler = handler;
    }

    public void DisplayButton()
    {
        if (ImGui.Button($"{Icons.Database}##mapMassEditToolView"))
        {
            Handler.EditLog.ShowMassEditLog = false;
            ShowToolView = true;
        }
        UIHelper.Tooltip("Toggle visibility of the tools.");
    }

    public void Display()
    {
        var width = ImGui.GetWindowWidth();
        var buttonSize = new Vector2(width, 24);

        if(TemplateDir == "")
        {
            SetupTemplates();
        }

        if (ShowToolView)
        {
            ImGui.BeginChild("mapMassEditToolSection");

            /*
            ImGui.Separator();
            UIHelper.WrappedText("New Template");
            ImGui.Separator();

            ImGui.SetNextItemWidth(width);
            ImGui.InputText("##newTemplateName", ref NewTemplateName, 255);

            if(ImGui.Button("Save", buttonSize))
            {

            }
            UIHelper.ShowHoverTooltip("Save the current inputs under a name so they can be stored and applied quickly in the future.");

            ImGui.Separator();
            UIHelper.WrappedText("Templates");
            ImGui.Separator();

            int i = 0;
            foreach(var entry in Templates)
            {
                var template = entry.Value;
                if (ImGui.Selectable($"{template.Name}##{template.Name}_{i}"))
                {
                    Screen.MassEditHandler.MapSelectionLogic = (SelectionConditionLogic)template.MapLogic;
                    Screen.MassEditHandler.MapObjectSelectionLogic = (SelectionConditionLogic)template.SelectionLogic;

                    Screen.MassEditHandler.MapInputs = template.MapInputs;
                    Screen.MassEditHandler.SelectionInputs = template.SelectionInputs;
                    Screen.MassEditHandler.EditInputs = template.EditInputs;
                }

                i++;
            }
            */

            ImGui.Separator();
            UIHelper.WrappedText("Actions");
            ImGui.Separator();

            if (ImGui.Button("Backup Maps", buttonSize))
            {
                BackupMaps();
            }
            UIHelper.Tooltip("All maps as they currently exist will be backed up into a ZIP file within the .smithbox folder.");

            ImGui.EndChild();
        }
    }

    private Dictionary<string, MassEditTemplate> Templates = new Dictionary<string, MassEditTemplate>();

    private void SetupTemplates()
    {
        Templates = new Dictionary<string, MassEditTemplate>();
        TemplateDir = $"{Editor.Project.ProjectPath}\\.smithbox\\Workflow\\MSB\\Mass Edit Templates";

        if (!Directory.Exists(TemplateDir))
        {
            Directory.CreateDirectory(TemplateDir);
        }

        var files = Directory.GetFiles(TemplateDir, "*", SearchOption.AllDirectories)
            .Where(file => file.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var entry in files)
        {
            var template = LoadTemplate(entry);
            Templates.Add(entry, template);
        }
    }

    private MassEditTemplate LoadTemplate(string path)
    {
        var template = new MassEditTemplate();

        if (File.Exists(path))
        {
            using (var stream = File.OpenRead(path))
            {
                template = JsonSerializer.Deserialize(stream, MassEditTemplateSerializationContext.Default.MassEditTemplate);
            }
        }

        return template;
    }

    private void BackupMaps()
    {
        BackupDir = $"{Editor.Project.ProjectPath}\\.smithbox\\Workflow\\MSB\\Backups";
        if (!Directory.Exists(BackupDir))
        {
            Directory.CreateDirectory(BackupDir);
        }

        string mapRoot = $"{Editor.Project.DataPath}\\map";
        var mapFiles = GetMapFiles(mapRoot);

        if (BackupDir != "" && mapFiles.Count > 0)
        {
            var date = DateTime.Now;
            var zipPath = $"{BackupDir}/MSB_Backup_{date:yyyy_MM_dd_HH_mm_ss_fff}.zip";

            using (ZipArchive archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                foreach (string file in mapFiles)
                {
                    string relativePath = Path.GetRelativePath(mapRoot, file);
                    archive.CreateEntryFromFile(file, relativePath);
                }
            }

            TaskLogs.AddLog($"Backed up all maps at {zipPath}");
        }
    }

    public List<string> GetMapFiles(string directory)
    {
        if (!Directory.Exists(directory))
        {
            return new List<string>();
        }

        var files = Directory.GetFiles(directory, "*", SearchOption.AllDirectories)
            .Where(file => file.EndsWith(".msb", StringComparison.OrdinalIgnoreCase) ||
                           file.EndsWith(".msb.dcx", StringComparison.OrdinalIgnoreCase))
            .ToList();

        return files;
    }
}
