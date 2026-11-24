#nullable enable
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.LuaEditor;

/// <summary>
/// The Viewport Filters tool of the Lua Editor's Tools window: what the simulation viewport shows of the battle
/// simulation. The effects the TAE events and the bullets spawn, the lines drawn for the bullets (as the Projectile
/// Editor draws them), and the target character. None of it changes the simulation: bullets still fly and hit, and the
/// target still stands where distances and aiming are measured from. Saved in the CFG.
/// </summary>
public class LuaViewportFiltersTool
{
    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("LUA_Tool_Viewport_Filters")}##luaViewportFiltersTool"))
            return;

        ImGui.BeginChild("LuaViewportFiltersToolSection", ImGuiChildFlags.Borders);
        Display();
        ImGui.EndChild();
    }

    private static void Display()
    {
        var cfg = CFG.Current;

        GUI.WrappedText(LOC.Get("LUA_Filters_Hint"));

        // Effects: FX turns them all on or off; the kinds below pick which play while it is on
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("LUA_Filters_Effects_Header"), LOC.Get("LUA_Filters_Effects_Header_TT"));

        Toggle("LUA_Filters_Fx", ref cfg.LuaEditor.LuaEditor_TimeAct_Effects);
        ImGui.BeginDisabled(!cfg.LuaEditor.LuaEditor_TimeAct_Effects);
        Toggle("LUA_Filters_Tae_Effects", ref cfg.LuaEditor.LuaEditor_Display_Tae_Effects);
        Toggle("LUA_Filters_Bullet_Effects", ref cfg.LuaEditor.LuaEditor_Display_Bullet_Effects);
        Toggle("LUA_Filters_Hit_Effects", ref cfg.LuaEditor.LuaEditor_Display_Hit_Effects);
        ImGui.EndDisabled();

        // Bullet overlays
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("LUA_Filters_Overlays_Header"), LOC.Get("LUA_Filters_Overlays_Header_TT"));

        Toggle("LUA_Filters_Bullets", ref cfg.LuaEditor.LuaEditor_Display_Bullets);
        Toggle("LUA_Filters_Reach", ref cfg.LuaEditor.LuaEditor_Display_Reach);
        Toggle("LUA_Filters_Trails", ref cfg.LuaEditor.LuaEditor_Display_Trails);
        Toggle("LUA_Filters_Impacts", ref cfg.LuaEditor.LuaEditor_Display_Impacts);

        // Characters
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("LUA_Filters_Characters_Header"), "");

        Toggle("LUA_Filters_Target", ref cfg.LuaEditor.LuaEditor_Display_Target);

        // Animation
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("LUA_Filters_Animation_Header"), "");

        Toggle("LUA_Filters_Blend", ref cfg.LuaEditor.LuaEditor_Blend_Animations);
        ImGui.BeginDisabled(!cfg.LuaEditor.LuaEditor_Blend_Animations);
        ImGui.SetNextItemWidth(160 * DPI.UIScale());
        ImGui.DragFloat($"{LOC.Get("LUA_Filters_Blend_Default")}##luaBlendDefault", ref cfg.LuaEditor.LuaEditor_Blend_Default_Time, 0.01f, 0f, 2f, "%.2f s");
        GUI.Tooltip(LOC.Get("LUA_Filters_Blend_Default_TT"));
        ImGui.EndDisabled();
    }

    private static void Toggle(string locKey, ref bool value)
    {
        ImGui.Checkbox($"{LOC.Get(locKey)}##{locKey}", ref value);
        GUI.Tooltip(LOC.Get($"{locKey}_TT"));
    }
}
