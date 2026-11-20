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

        var label = container.IsDirty ? $"{container.Name}*" : container.Name;
        if (container.InProject)
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_PrimaryChanged_Text);
        var open = ImGui.TreeNodeEx($"{label}##luaBinder_{container.Path}", flags);
        if (container.InProject)
            ImGui.PopStyleColor();

        if (selected && ScrollToSelection)
            ImGui.SetScrollHereY();

        if (ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen())
            Editor.SelectContainer(container);
        if (container.InProject)
            GUI.Tooltip(LOC.Get("LUA_FileList_In_Project_TT"));

        var hasCharacter = CharacterText(container);
        UsersText(container, hasCharacter);

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
                var name = script.IsDirty ? $"{script.EntryName}*" : script.EntryName;
                if (ImGui.Selectable($"{name}##luaScript_{container.Path}_{script.EntryName}", Editor.SelectedScript == script))
                    Editor.SelectScript(script);
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
    private bool CharacterText(LuaContainer container)
    {
        if (container.CharacterId is not { } chr)
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
    /// The NpcThinkParam rows that use the binder's goal ID, after its name: dimmed when the character's alias is shown,
    /// so the two don't read as one
    /// </summary>
    private void UsersText(LuaContainer container, bool afterAlias)
    {
        if (!CFG.Current.LuaEditor_File_List_Display_Users || container.GoalId is not { } id)
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

    private bool Matches(LuaContainer container)
    {
        if (string.IsNullOrEmpty(Filter))
            return true;

        var users = CFG.Current.LuaEditor_File_List_Display_Users && container.GoalId is { } id
            ? string.Join(" ", Editor.Project.Handler.LuaData?.UsersOf(id).Select(u => u.Name) ?? Array.Empty<string>())
            : "";
        // The character's ID and alias match too
        var character = container.CharacterId is { } chr ? $"{chr} {AliasOf(chr)}" : "";
        return EditorFilters.IsMatch(Filter, container.Name, ExactFilter, $"{character} {users}");
    }

    private static string KindName(LuaBinderKind kind) => kind switch
    {
        LuaBinderKind.Battle => LOC.Get("LUA_FileList_Kind_Battle"),
        LuaBinderKind.Logic => LOC.Get("LUA_FileList_Kind_Logic"),
        LuaBinderKind.Common => LOC.Get("LUA_FileList_Kind_Common"),
        _ => LOC.Get("LUA_FileList_Kind_Other"),
    };
}
