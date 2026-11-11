using Hexa.NET.ImGui;
using System.Numerics;

namespace StudioCore.Editors.BehaviorEditor;

/// <summary>A found record: where it is, and the line to show for it</summary>
public record BhvToolResult(BhvFileWrapper File, BhvRecordKind Kind, int State, int Transition, int Condition, int Index,
    string Tag, Vector4 TagColor, string Text);

/// <summary>
/// A tool's results: a clipped list of "file  tag  text" rows; clicking a row jumps to the record
/// </summary>
public static class BhvResultList
{
    /// <summary>Most rows a tool keeps; a search past this says so</summary>
    public const int MaxResults = 5000;

    public static void Draw(BhvEditorView view, string id, List<BhvToolResult> results)
    {
        if (results.Count == 0)
            return;

        ImGui.BeginChild(id, ImGuiChildFlags.Borders);

        var clipper = new ImGuiListClipper();
        clipper.Begin(results.Count);

        while (clipper.Step())
        {
            for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var r = results[i];

                if (ImGui.Selectable($"##{id}_row{i}", false, ImGuiSelectableFlags.AllowOverlap))
                    view.Selection.JumpTo(r.File, r.Kind, r.State, r.Transition, r.Condition, r.Index);
                GUI.Tooltip(LOC.Get("BHV_Tool_Result_Jump_TT"));

                ImGui.SameLine(0, 0);
                ImGui.TextDisabled($"{r.File.Binder.Name}/{r.File.Name}");

                if (r.Tag.Length > 0)
                {
                    ImGui.SameLine();
                    ImGui.TextColored(r.TagColor, r.Tag);
                }

                ImGui.SameLine();
                ImGui.Text(r.Text);
            }
        }

        clipper.End();

        ImGui.EndChild();
    }
}
