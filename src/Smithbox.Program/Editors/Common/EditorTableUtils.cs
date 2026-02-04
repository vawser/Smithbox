using Hexa.NET.ImGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public static class EditorTableUtils
{
    public static bool ImGuiTableStdColumns(string id, int cols, bool fixVerticalPadding)
    {
        Vector2 oldPad = ImGui.GetStyle().CellPadding;
        if (fixVerticalPadding)
        {
            ImGui.GetStyle().CellPadding = new Vector2(oldPad.X, 0);
        }

        var v = ImGui.BeginTable(id, cols,
            ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.SizingStretchSame |
            ImGuiTableFlags.ScrollY);

        if (fixVerticalPadding)
        {
            ImGui.GetStyle().CellPadding = oldPad;
        }

        return v;
    }
    public static bool ImguiTableSeparator()
    {
        var lastCol = false;
        var cols = ImGui.TableGetColumnCount();
        ImGui.TableNextRow();
        for (var i = 0; i < cols; i++)
        {
            if (ImGui.TableNextColumn())
            {
                ImGui.Separator();
                lastCol = true;
            }
        }

        return lastCol;
    }

    public static bool HelpIcon(string id, ref string hint, bool canEdit)
    {
        if (hint == null)
        {
            return false;
        }

        return ParamEditorHints.AddImGuiHintButton(id, ref hint, canEdit, true);
    }
}
