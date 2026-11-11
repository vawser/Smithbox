using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.MaterialEditor;

/// <summary>
/// Shader Information (F2): the selected material's shader, its texture slots, and Add Missing
/// </summary>
public class MatShaderTool
{
    public MaterialEditorView View;
    public ProjectEntry Project;

    private readonly MaterialShaderPanel Panel;

    public MatShaderTool(MaterialEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        Panel = new MaterialShaderPanel(view, project);
    }

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("MAT_ShaderTool_Header")}##materialShaderToolHeader"))
            return;

        ImGui.BeginChild("MaterialShaderToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        var material = View.Selection.CurrentMaterial;

        if (material == null)
        {
            GUI.WrappedText(LOC.Get("MAT_Tool_No_Material"));
            return;
        }

        Panel.Draw(View.Selection.SourceType, material);
    }
}
