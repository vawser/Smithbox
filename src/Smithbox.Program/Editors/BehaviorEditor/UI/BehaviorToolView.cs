using Hexa.NET.ImGui;
using StudioCore;
using StudioCore.Core;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BehaviorEditorNS;

public class BehaviorToolView
{
    public BehaviorEditorScreen Editor;
    public ProjectEntry Project;

    public BehaviorToolView(BehaviorEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void OnGui()
    {
        if (ImGui.BeginMenuBar())
        {
            ViewMenu();

            ImGui.EndMenuBar();
        }

        if (CFG.Current.Interface_BehaviorEditor_Tool_PowerEdit)
        {
            //Editor.PowerEdit.Display();
        }

        if (CFG.Current.Interface_BehaviorEditor_Tool_VariableAssist)
        {
            //Editor.VariableAssist.Display();
        }

        if (CFG.Current.Interface_BehaviorEditor_Tool_ClipAssist)
        {
            //Editor.ClipAssist.Display();
        }
    }

    public void ViewMenu()
    {
        if (ImGui.BeginMenu("View"))
        {
            if (ImGui.MenuItem("Power Edit"))
            {
                CFG.Current.Interface_BehaviorEditor_Tool_PowerEdit = !CFG.Current.Interface_BehaviorEditor_Tool_PowerEdit;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_BehaviorEditor_Tool_PowerEdit);

            if (ImGui.MenuItem("Variable Assist"))
            {
                CFG.Current.Interface_BehaviorEditor_Tool_VariableAssist = !CFG.Current.Interface_BehaviorEditor_Tool_VariableAssist;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_BehaviorEditor_Tool_VariableAssist);

            if (ImGui.MenuItem("Clip Assist"))
            {
                CFG.Current.Interface_BehaviorEditor_Tool_ClipAssist = !CFG.Current.Interface_BehaviorEditor_Tool_ClipAssist;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_BehaviorEditor_Tool_ClipAssist);

            ImGui.EndMenu();
        }
    }
}