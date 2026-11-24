#nullable enable
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;

namespace StudioCore.Editors.FaceGenEditor;

/// <summary>
/// How the viewport shows the active face: the body it is put on, and whether the sliders shape it. None of it
/// changes the face's data.
/// </summary>
public class FaceGenViewportFiltersTool
{
    private readonly FaceGenEditorView View;

    public FaceGenViewportFiltersTool(FaceGenEditorView view)
    {
        View = view;
    }

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("FACEGEN_Tool_Viewport_Filters")}##faceGenViewportFiltersToolHeader"))
            return;

        ImGui.BeginChild("FaceGenViewportFiltersToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        var cfg = CFG.Current;
        var viewport = View.ViewportWindow;

        // Camera: framing, preset angles and the turntable
        GUI.SimpleHeader(LOC.Get("FACEGEN_Camera_Header"), LOC.Get("FACEGEN_Camera_Header_TT"));

        if (ImGui.Button($"{LOC.Get("FACEGEN_Menu_Frame_Face")}##faceGenToolFrameFace"))
            viewport.FrameFace();

        ImGui.SameLine();

        if (ImGui.Button($"{LOC.Get("FACEGEN_Menu_Frame_Body")}##faceGenToolFrameBody"))
            viewport.FrameBody();

        var first = true;

        foreach (var angle in Enum.GetValues<FaceViewportWindow.CameraAngle>())
        {
            if (!first)
                ImGui.SameLine();
            first = false;

            if (ImGui.Button($"{LOC.Get($"FACEGEN_Camera_{angle}")}##faceGenToolCamera{angle}"))
                viewport.SetCameraAngle(angle);
        }

        ImGui.Checkbox($"{LOC.Get("FACEGEN_Camera_Turntable")}##faceGenToolTurntable", ref viewport.Turntable);
        GUI.Tooltip(LOC.Get("FACEGEN_Camera_Turntable_TT"));

        if (viewport.Turntable)
        {
            ImGui.SetNextItemWidth(-1);
            ImGui.SliderFloat("##faceGenToolTurntableSpeed", ref viewport.TurntableSpeed, 5f, 90f,
                LOC.Get("FACEGEN_Camera_Turntable_Speed"));
        }

        GUI.Spacer();

        GUI.WrappedText(LOC.Get("FACEGEN_ViewportFilters_Hint"));

        // Body
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("FACEGEN_Menu_Body"), LOC.Get("FACEGEN_Menu_Body_TT"));

        BodyOption("FACEGEN_Menu_Body_Wearer", 0);
        BodyOption("FACEGEN_Menu_Body_Male", 1);
        BodyOption("FACEGEN_Menu_Body_Female", 2);

        // Face
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("FACEGEN_ViewportFilters_Face_Header"), LOC.Get("FACEGEN_ViewportFilters_Face_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("FACEGEN_Menu_Apply_Shape")}##faceGenApplyShape", ref cfg.FaceGenEditor.FaceGenEditor_Apply_Shape);
        GUI.Tooltip(LOC.Get("FACEGEN_Menu_Apply_Shape_TT"));
    }

    private static void BodyOption(string locKey, int value)
    {
        if (ImGui.RadioButton($"{LOC.Get(locKey)}##{locKey}", CFG.Current.FaceGenEditor.FaceGenEditor_Body == value))
            CFG.Current.FaceGenEditor.FaceGenEditor_Body = value;

        GUI.Tooltip(LOC.Get($"{locKey}_TT"));
    }
}
