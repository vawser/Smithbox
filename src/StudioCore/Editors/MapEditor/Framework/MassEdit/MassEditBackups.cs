using ImGuiNET;
using StudioCore.Interface;
using StudioCore.Resource.Locators;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Framework.MassEdit;

public class MassEditBackups
{
    private MapEditorScreen Screen;
    private MassEditHandler Handler;

    private string BackupDir = "";

    private bool ShowBackupManager = true;

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
            ShowBackupManager = !ShowBackupManager;
        }
        UIHelper.ShowHoverTooltip("Toggle visibility of the backup manager.");

    }

    public void Display()
    {
        ImGui.BeginChild("mapBackupManagerSection");

        if(ImGui.Button("Backup Maps"))
        {

        }
        UIHelper.ShowHoverTooltip("All maps as they currently exist will be backed up into a ZIP file within the .smithbox folder.");

        ImGui.EndChild();
    }
}
