using Hexa.NET.ImGui;
using Microsoft.AspNetCore.Components;
using StudioCore.Core;
using StudioCore.Core.ProjectNS;
using StudioCore.Editor;
using StudioCore.Editors.ParamEditor.Actions;
using StudioCore.Utilities;
using System.Numerics;

namespace StudioCore.Editors.GparamEditorNS;

public class GparamEditor
{
    public BaseEditor BaseEditor;
    public Project Project;

    public int ID = 0;

    public ActionManager ActionManager;

    public 

    private bool DetectShortcuts = false;

    public GparamEditor(int id, BaseEditor editor, Project projectOwner)
    {
        BaseEditor = editor;
        Project = projectOwner;
        ID = id;

        ActionManager = new();
    }

    public void Display(float dt, string[] cmd)
    {
        ImGui.Begin($"Graphics Param Editor##GparamEditor{ID}", Project.BaseEditor.MainWindowFlags);

        DetectShortcuts = ShortcutUtils.UpdateShortcutDetection();

        uint dockspaceID = ImGui.GetID($"GparamEditorDockspace{ID}");
        ImGui.DockSpace(dockspaceID, Vector2.Zero, ImGuiDockNodeFlags.PassthruCentralNode);

        Menubar();
        Shortcuts();

        ImGui.End();

        ImGui.Begin($"Graphics Param List##GparamFileList", Project.BaseEditor.SubWindowFlags);

        FileList.Draw();

        ImGui.End();

        ImGui.Begin($"Groups##GparamGroupView", Project.BaseEditor.SubWindowFlags);

        GroupView.Draw();

        ImGui.End();

        ImGui.Begin($"Fields##GparamFieldView", Project.BaseEditor.SubWindowFlags);

        FieldView.Draw();

        ImGui.End();

        ImGui.Begin($"Entires##GparamFieldEntryView", Project.BaseEditor.SubWindowFlags);

        FieldEntryView.Draw();

        ImGui.End();

        ImGui.Begin($"Tools##GparamTools", Project.BaseEditor.SubWindowFlags);

        ToolView.Draw();

        ImGui.End();

        EditorFocus.Update();
    }
}
