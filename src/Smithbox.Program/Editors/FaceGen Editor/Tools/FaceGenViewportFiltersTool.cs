#nullable enable
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;

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

    private static void Display()
    {
        var cfg = CFG.Current;

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

        ImGui.Checkbox($"{LOC.Get("FACEGEN_Menu_Apply_Shape")}##faceGenApplyShape", ref cfg.FaceGenEditor_Apply_Shape);
        GUI.Tooltip(LOC.Get("FACEGEN_Menu_Apply_Shape_TT"));
    }

    private static void BodyOption(string locKey, int value)
    {
        if (ImGui.RadioButton($"{LOC.Get(locKey)}##{locKey}", CFG.Current.FaceGenEditor_Body == value))
            CFG.Current.FaceGenEditor_Body = value;

        GUI.Tooltip(LOC.Get($"{locKey}_TT"));
    }
}
