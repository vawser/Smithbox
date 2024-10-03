using ImGuiNET;
using StudioCore.Core.Project;
using StudioCore.Editors.EmevdEditor.Actions;
using StudioCore.Editors.ModelEditor.Tools;
using StudioCore.EmevdEditor;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Tools;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.EMEVD;

namespace StudioCore.Editors.EmevdEditor.Tools;

public class ToolWindow
{
    private EmevdEditorScreen Screen;
    private ActionHandler Handler;

    public ToolWindow(EmevdEditorScreen screen)
    {
        Screen = screen;
        Handler = new ActionHandler(screen);
    }


    public void Shortcuts()
    {

    }

    public void OnProjectChanged()
    {

    }


    public void OnGui()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * DPI.GetUIScale(), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Tool Window##ToolConfigureWindow_EmevdEditor"))
        {
            var windowWidth = ImGui.GetWindowWidth();
            var defaultButtonSize = new Vector2(windowWidth, 32);

            List<string> loggedInstructions = new List<string>();

            if (ImGui.CollapsingHeader("Debug Tool"))
            {
                if (ImGui.Button("Log Unknowns", defaultButtonSize))
                {
                    foreach (var (info, binder) in EmevdBank.ScriptBank)
                    {
                        foreach (var evt in binder.Events)
                        {
                            var eventName = evt.Name;

                            foreach (var ins in evt.Instructions)
                            {
                                var insName = $"{ins.Bank}[{ins.ID}]";

                                if (!EmevdUtils.HasArgDoc(ins))
                                {
                                    if (!loggedInstructions.Contains(insName))
                                    {
                                        loggedInstructions.Add(insName);
                                        var output = EmevdUtils.DetermineUnknownParameters(ins, false);
                                        TaskLogs.AddLog($"{insName}{output}\n");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        ImGui.End();
        ImGui.PopStyleColor(1);
    }
}
