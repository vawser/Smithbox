using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Editors.ModelEditor;
using System.Numerics;

namespace StudioCore.Editors.LuaEditor;

/// <summary>
/// The project's Lua binders by kind (battle goals, logic, the shared aicommon, other), each with the NpcThinkParam
/// rows that use its goal ID. A binder opens to its scripts.
/// </summary>
public class LuaFileList
{
    private readonly LuaEditorScreen Editor;

    private string Filter = "";
    private bool ExactFilter = false;

    public bool ScrollToSelection;

    public LuaFileList(LuaEditorScreen editor)
    {
        Editor = editor;
    }

    public void Draw()
    {
        GUI.TitleHeader(LOC.Get("LUA_FileList_Header"), LOC.Get("LUA_FileList_Header_TT"));

        ImGui.BeginChild("luaFileListHeader", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);
        EditorFilters.DisplaySearchbar("luaEditor_FileList", ref Filter, ref ExactFilter);
        ImGui.EndChild();

        ImGui.BeginChild("luaFileList", Vector2.Zero, ImGuiChildFlags.Borders);

        var bank = Editor.Bank;
        if (bank != null)
        {
            if (bank.Containers.Count == 0)
                ImGui.TextDisabled(LOC.Get("LUA_FileList_No_Files"));

            foreach (var group in bank.Containers.GroupBy(c => c.Kind))
            {
                var visible = group.Where(Matches).ToList();
                if (visible.Count == 0)
                    continue;

                if (!ImGui.CollapsingHeader($"{KindName(group.Key)} ({visible.Count})##luaKind_{group.Key}", ImGuiTreeNodeFlags.DefaultOpen))
                    continue;

                foreach (var container in visible)
                    DrawContainer(container);
            }
        }

        ScrollToSelection = false;
        ImGui.EndChild();
    }

    private void DrawContainer(LuaContainer container)
    {
        var selected = Editor.SelectedContainer == container;
        var flags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanAvailWidth;
        if (selected)
            flags |= ImGuiTreeNodeFlags.Selected;

        // A binder with one script selects it directly; one with several opens to them
        var single = container.IsLoaded && container.Scripts.Count == 1;
        if (single)
            flags |= ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen;

        // A map's binder (or aicommon) found by one of its scripts opens to show it
        var byScript = !single && !string.IsNullOrEmpty(Filter) && !MatchesBinder(container);
        if (byScript)
            ImGui.SetNextItemOpen(true);

        var label = container.IsDirty ? $"{container.Name}*" : container.Name;
        var status = container.VanillaStatus;
        var coloured = VanillaMarkers.PushColour(status);
        var open = ImGui.TreeNodeEx($"{label}##luaBinder_{container.Path}", flags);
        VanillaMarkers.PopColour(coloured);

        if (selected && ScrollToSelection)
            ImGui.SetScrollHereY();

        if (ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen())
            Editor.SelectContainer(container);
        if (coloured)
            VanillaMarkers.Tooltip(status);

        // A binder of one script: that script's actions; a map's binder (or aicommon): a new script in it
        if (ImGui.BeginPopupContextItem($"##luaBinderMenu_{container.Path}"))
        {
            if (container.EnsureLoaded() && !container.HoldsManyGoals && container.Scripts.Count == 1)
                Editor.Commands.ScriptMenuItems(container.Scripts[0]);
            else if (container.IsLoaded)
                Editor.Commands.BinderMenuItems(container);
            ImGui.EndPopup();
        }

        if (container.PendingRevert)
        {
            ImGui.SameLine();
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("LUA_FileList_Reverting"));
            GUI.Tooltip(LOC.Get("LUA_FileList_Reverting_TT"));
        }

        // A binder of one goal shows its character and users on its own row; a map's binder on each script's
        if (!container.HoldsManyGoals)
        {
            var hasCharacter = CharacterText(container.CharacterId);
            UsersText(container.GoalId, hasCharacter);
        }

        if (container.LoadError != null)
        {
            ImGui.SameLine();
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, Icons.ExclamationTriangle);
            GUI.Tooltip(container.LoadError);
        }

        if (!open || single)
            return;

        if (container.EnsureLoaded())
        {
            foreach (var script in container.Scripts)
            {
                if (byScript && !MatchesScript(script))
                    continue;

                var name = script.IsDirty ? $"{script.EntryName}*" : script.EntryName;
                var scriptStatus = container.ScriptStatus(script);
                var scriptColoured = VanillaMarkers.PushColour(scriptStatus);
                var clicked = ImGui.Selectable($"{name}##luaScript_{container.Path}_{script.EntryName}", Editor.SelectedScript == script);
                VanillaMarkers.PopColour(scriptColoured);
                if (scriptColoured)
                    VanillaMarkers.Tooltip(scriptStatus);

                if (clicked)
                    Editor.SelectScript(script);
                if (ImGui.BeginPopupContextItem($"##luaScriptMenu_{container.Path}_{script.EntryName}"))
                {
                    Editor.Commands.ScriptMenuItems(script);
                    ImGui.EndPopup();
                }

                if (container.HoldsManyGoals)
                {
                    var hasCharacter = CharacterText(script.CharacterId);
                    UsersText(script.GoalId, hasCharacter);
                }
            }
        }
        ImGui.TreePop();
    }

    // Character aliases by chr ID (looked up once each)
    private readonly Dictionary<string, string> Aliases = new();

    private string AliasOf(string chr)
    {
        if (!Aliases.TryGetValue(chr, out var alias))
            Aliases[chr] = alias = ModelEditorUtils.GetAliasForSourceListEntry(Editor.Project, chr, ModelListType.Character) ?? "";
        return alias;
    }

    /// <summary>A battle goal's character (from its goal ID) and its alias, after its name; true if one was shown</summary>
    private bool CharacterText(string chr)
    {
        if (chr == null)
            return false;

        var alias = AliasOf(chr);
        ImGui.SameLine();
        ImGui.TextDisabled(chr);
        GUI.Tooltip(LOC.Get("LUA_FileList_Character_TT", chr));
        if (alias.Length == 0)
            return false;
        GUI.DisplayAlias(alias, CFG.Current.Interface_Alias_Wordwrap_Model_Editor);
        return true;
    }

    /// <summary>
    /// The NpcThinkParam rows that use the goal ID, after the binder's or script's name: dimmed when the character's
    /// alias is shown, so the two don't read as one
    /// </summary>
    private void UsersText(int? goalId, bool afterAlias)
    {
        if (!CFG.Current.LuaEditor_File_List_Display_Users || goalId is not { } id)
            return;

        var users = Editor.Project.Handler.LuaData?.UsersOf(id);
        if (users == null || users.Count == 0)
            return;

        var first = users.FirstOrDefault(u => !string.IsNullOrEmpty(u.Name));
        var text = first.Name ?? users[0].Id.ToString();
        if (users.Count > 1)
            text += $" (+{users.Count - 1})";

        ImGui.SameLine();
        if (afterAlias)
            ImGui.TextDisabled(text);
        else
            ImGui.TextColored(UI.Current.ImGui_AliasName_Text, text);
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text(LOC.Get("LUA_FileList_Users_TT"));
            foreach (var u in users.Take(30))
                ImGui.Text($"{u.Id}  {u.Name}");
            if (users.Count > 30)
                ImGui.TextDisabled(LOC.Get("LUA_FileList_Users_More", users.Count - 30));
            ImGui.EndTooltip();
        }
    }

    /// <summary>The binder matches, or (a map's binder, or aicommon, once read) one of its scripts does</summary>
    private bool Matches(LuaContainer container)
    {
        if (string.IsNullOrEmpty(Filter) || MatchesBinder(container))
            return true;
        return container.HoldsManyGoals && container.IsLoaded && container.Scripts.Any(MatchesScript);
    }

    private bool MatchesBinder(LuaContainer container) =>
        IsMatch(container.Name, container.HoldsManyGoals ? null : container.GoalId, container.HoldsManyGoals ? null : container.CharacterId);

    private bool MatchesScript(LuaScript script) =>
        string.IsNullOrEmpty(Filter) || IsMatch(script.EntryName, script.GoalId, script.CharacterId);

    private bool IsMatch(string name, int? goalId, string chr)
    {
        var users = CFG.Current.LuaEditor_File_List_Display_Users && goalId is { } id
            ? string.Join(" ", Editor.Project.Handler.LuaData?.UsersOf(id).Select(u => u.Name) ?? Array.Empty<string>())
            : "";
        // The character's ID and alias match too
        var character = chr != null ? $"{chr} {AliasOf(chr)}" : "";
        return EditorFilters.IsMatch(Filter, name, ExactFilter, $"{character} {users}");
    }

    private static string KindName(LuaBinderKind kind) => kind switch
    {
        LuaBinderKind.Battle => LOC.Get("LUA_FileList_Kind_Battle"),
        LuaBinderKind.Logic => LOC.Get("LUA_FileList_Kind_Logic"),
        LuaBinderKind.Common => LOC.Get("LUA_FileList_Kind_Common"),
        LuaBinderKind.Map => LOC.Get("LUA_FileList_Kind_Map"),
        _ => LOC.Get("LUA_FileList_Kind_Other"),
    };
}
