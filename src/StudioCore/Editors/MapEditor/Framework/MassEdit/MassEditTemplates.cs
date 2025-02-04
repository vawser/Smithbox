using ImGuiNET;
using StudioCore.Interface;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Framework.MassEdit;

public class MassEditTemplates
{
    private MapEditorScreen Screen;
    private MassEditHandler Handler;

    public bool ShowTemplateManager = true;

    public MassEditTemplates(MapEditorScreen screen, MassEditHandler handler)
    {
        Screen = screen;
        Handler = handler;
    }

    public void DisplayButton()
    {
        if (ImGui.Button($"{ForkAwesome.Database}##templateManagerView"))
        {
            Handler.EditLog.ShowMassEditLog = false;
            Handler.BackupManager.ShowBackupManager = false;
            ShowTemplateManager = true;
        }
        UIHelper.ShowHoverTooltip("Toggle visibility of the template manager.");

    }

    public void Display()
    {
        if (ShowTemplateManager)
        {
            ImGui.BeginChild("templateManagerSection");


            ImGui.EndChild();
        }
    }
}
