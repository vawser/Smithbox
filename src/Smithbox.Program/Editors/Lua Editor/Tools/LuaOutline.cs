using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.LuaEditor;

/// <summary>
/// The Act Outline tool of the Tools window: what the selected script defines, its functions (and, optionally,
/// top-level values), in order. Click one to show it.
/// </summary>
public class LuaOutline
{
    private readonly LuaEditorScreen Editor;

    private string Filter = "";
    private bool ExactFilter = false;
    private bool ShowValues = false;

    public LuaOutline(LuaEditorScreen editor)
    {
        Editor = editor;
    }

    public void OnToolWindow()
    {
        if (ToolSections.Header($"{LOC.Get("LUA_Tool_Act_Outline")}##luaActOutlineTool"))
        {
            ImGui.BeginChild("LuaActOutlineToolSection", ImGuiChildFlags.Borders);
            Draw();
            ImGui.EndChild();
        }
    }

    private void Draw()
    {
        GUI.SimpleHeader(LOC.Get("LUA_Outline_Header"), LOC.Get("LUA_Outline_Header_TT"));

        ImGui.BeginChild("luaOutlineHeader", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);
        EditorFilters.DisplaySearchbar("luaEditor_Outline", ref Filter, ref ExactFilter);
        ImGui.SameLine();
        ImGui.Checkbox($"{LOC.Get("LUA_Outline_Show_Values")}##luaOutlineValues", ref ShowValues);
        GUI.Tooltip(LOC.Get("LUA_Outline_Show_Values_TT"));
        ImGui.EndChild();

        ImGui.BeginChild("luaOutline", Vector2.Zero, ImGuiChildFlags.Borders);

        var script = Editor.SelectedScript;
        var parse = script?.Text != null ? script.Parse : null;
        if (parse == null)
            ImGui.TextDisabled(LOC.Get("LUA_Script_None_Selected"));
        else if (!parse.IsValid)
            ImGui.TextDisabled(LOC.Get("LUA_Outline_Not_Valid"));
        else
        {
            var i = 0;
            foreach (var d in parse.Definitions)
            {
                i++;
                if (!d.IsFunction && !ShowValues)
                    continue;
                if (!string.IsNullOrEmpty(Filter) && !EditorFilters.IsMatch(Filter, d.Name, ExactFilter))
                    continue;

                var label = d.IsFunction ? d.Name : $"{d.Name} =";
                if (!d.IsTopLevel)
                    label = "    " + label;
                if (ImGui.Selectable($"{label}##luaOutline_{i}"))
                    Editor.ShowLine(script, d.Line);
                if (ImGui.IsItemHovered())
                    GUI.Tooltip(LOC.Get("LUA_Outline_Line", d.Line));
            }
        }

        ImGui.EndChild();
    }
}
