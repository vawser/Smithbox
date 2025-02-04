using ImGuiNET;
using StudioCore.Interface;
using StudioCore.Resource.Locators;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Framework.MassEdit;

public class MassEditBackups
{
    private MapEditorScreen Screen;
    private MassEditHandler Handler;

    private string BackupDir = "";

    public bool ShowBackupManager = true;

    public MassEditBackups(MapEditorScreen screen, MassEditHandler handler)
    {
        Screen = screen;
        Handler = handler;

        BackupDir = $"{Smithbox.ProjectRoot}/.smithbox/Assets/MSB/{MiscLocator.GetGameIDForDir()}";
        if(!Directory.Exists(BackupDir))
        {
            Directory.CreateDirectory(BackupDir);
        }
    }

    public void DisplayButton()
    {
        if (ImGui.Button($"{ForkAwesome.FileExcelO}##mapBackupManagerView"))
        {
            Handler.EditLog.ShowMassEditLog = false;
            Handler.TemplateManager.ShowTemplateManager = false;
            ShowBackupManager = true;
        }
        UIHelper.ShowHoverTooltip("Toggle visibility of the backup manager.");

    }

    public void Display()
    {
        var width = ImGui.GetWindowWidth();
        var buttonSize = new Vector2(width, 24);

        if (ShowBackupManager)
        {
            ImGui.BeginChild("mapBackupManagerSection");

            if (ImGui.Button("Backup Maps", buttonSize))
            {
                BackupMaps();
            }
            UIHelper.ShowHoverTooltip("All maps as they currently exist will be backed up into a ZIP file within the .smithbox folder.");

            ImGui.EndChild();
        }
    }

    private void BackupMaps()
    {
        var mapPaths = GetMapPaths();

        // Ignore if not set correctly, don't want to be making ZIPs on the desktop
        if (BackupDir != "")
        {
            var date = DateTime.Now;

            var zipPath = $"{BackupDir}/MSB_Backup_{date.Year}_{date.Month}_{date.Day}_{date.Hour}_{date.Minute}_{date.Second}_{date.Millisecond}.zip";

            using (ZipArchive archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                foreach (string file in mapPaths)
                {
                    archive.CreateEntryFromFile(file, Path.GetFileName(file));
                }
            }

            TaskLogs.AddLog($"Backed up all maps at {zipPath}");

            
        }
    }

    private List<string> GetMapPaths()
    {
        var mapPaths = new List<string>();
        var projectMapNames = MapLocator.GetMapList(Smithbox.ProjectRoot);

        foreach (var name in projectMapNames)
        {

        }

        return mapPaths;
    }
}
