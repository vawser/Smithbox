using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System.Numerics;

namespace StudioCore.Editors.LuaEditor;

/// <summary>
/// The Lua Editor: the Lua scripts in a project's binders (ER's AI, *.luabnd), as text. Bytecode is decompiled when a
/// script is first shown; an edited script is saved as Shift-JIS source, which the game loads. See
/// Claude/Documentation/ER AI Editor Analysis.md.
/// </summary>
public class LuaEditorScreen : EditorScreen
{
    public ProjectEntry Project;

    public LuaFileList FileList;
    public LuaScriptView ScriptView;
    public LuaOutline Outline;
    public LuaDefinitionIndex Definitions;

    public LuaContainer SelectedContainer { get; private set; }
    public LuaScript SelectedScript { get; private set; }

    public LuaEditorScreen(ProjectEntry project)
    {
        Project = project;
        FileList = new(this);
        ScriptView = new(this);
        Outline = new(this);
        Definitions = new(this);
    }

    public string EditorName => "Lua Editor##LuaEditor";
    public string CommandEndpoint => "lua";
    public string SaveType => "Lua";
    public string WindowName => "";
    public bool HasDocked { get; set; }

    public LuaBank Bank => Project.Handler.LuaData?.PrimaryBank;

    public void OnGUI(string[] commands)
    {
        MonitorShortcuts();
        ParseCommands(commands);

        if (ImGui.BeginMenuBar())
        {
            FileMenu();
            EditMenu();
            ViewMenu();
            ToolSections.EditorOptionsMenu();

            ImGui.EndMenuBar();
        }

        var dsid = ImGui.GetID("DockSpace_LuaEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None, ref GUI.DockGroup_LuaEditor);

        if (Bank == null)
            return;

        Definitions.Update();

        Window("LUA_Window_Files", "luaEditor_Files", EditorFocusContext.LuaEditor_FileList, dsid, FileList.Draw);
        Window("LUA_Window_Script", "luaEditor_Script", EditorFocusContext.LuaEditor_Script, dsid, ScriptView.Draw,
            GUI.GetInnerWindowFlags() | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
        Window("LUA_Window_Outline", "luaEditor_Outline", EditorFocusContext.LuaEditor_Outline, dsid, Outline.Draw);
    }

    private void Window(string titleKey, string id, EditorFocusContext focus, uint dockspaceId, Action draw, ImGuiWindowFlags? flags = null)
    {
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_LuaEditorView);

        if (ImGui.Begin($"{LOC.Get(titleKey)}###{id}", flags ?? GUI.GetInnerWindowFlags()))
        {
            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                FocusManager.SetFocus(focus);

            draw();
        }

        ImGui.End();
    }

    public void SelectContainer(LuaContainer container)
    {
        SelectedContainer = container;
        if (container == null)
        {
            SelectScript(null);
            return;
        }
        if (container.EnsureLoaded() && (SelectedScript == null || SelectedScript.Container != container))
            SelectScript(container.Scripts.FirstOrDefault());
    }

    public void SelectScript(LuaScript script)
    {
        if (script == SelectedScript)
            return;
        ScriptView.CommitPending();
        SelectedScript = script;
        if (script != null)
            SelectedContainer = script.Container;
    }

    /// <summary>Selects a script and shows a line (1-based) in it</summary>
    public void ShowLine(LuaScript script, int line)
    {
        SelectScript(script);
        ScriptView.JumpTo(line);
        FileList.ScrollToSelection = true;
    }

    /// <summary>lua/select/&lt;binder&gt;[/&lt;entry&gt;[/&lt;line&gt;]], e.g. lua/select/430000_battle/430000_battle.lua/120</summary>
    private void ParseCommands(string[] commands)
    {
        if (commands == null || commands.Length < 2 || commands[0] != "select" || Bank == null)
            return;

        var container = Bank.FindContainer(commands[1]);
        if (container == null || !container.EnsureLoaded())
            return;

        SelectContainer(container);
        FileList.ScrollToSelection = true;

        if (commands.Length > 2 && !string.IsNullOrEmpty(commands[2]))
        {
            var script = container.Scripts.FirstOrDefault(s => s.EntryName.Equals(commands[2], StringComparison.OrdinalIgnoreCase));
            if (script != null)
                SelectScript(script);
        }

        if (commands.Length > 3 && int.TryParse(commands[3], out var line))
            ScriptView.JumpTo(line);
    }

    private void MonitorShortcuts()
    {
        if (!FocusManager.IsInLuaEditor())
            return;

        if (InputManager.IsPressed(KeybindID.Save))
            Save();

        // While typing, the text box has its own undo
        if (ImGui.GetIO().WantTextInput)
            return;

        if (InputManager.IsPressed(KeybindID.Undo))
            Undo();
        if (InputManager.IsPressed(KeybindID.Redo))
            Redo();
    }

    private void Undo()
    {
        ScriptView.CommitPending();
        if (SelectedScript?.CanUndo == true)
            SelectedScript.Undo();
    }

    private void Redo()
    {
        ScriptView.CommitPending();
        if (SelectedScript?.CanRedo == true)
            SelectedScript.Redo();
    }

    public void FileMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_File")}##fileMenuHeader"))
        {
            var dirty = Bank?.GetDirtyFiles().Count ?? 0;
            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Save")}##saveAction", $"{InputManager.GetHint(KeybindID.Save)}", false, dirty > 0 || ScriptView.HasPendingEdit))
                Save();
            GUI.Tooltip(LOC.Get("LUA_Menubar_Save_TT", dirty));

            ImGui.EndMenu();
        }
    }

    public void EditMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_Edit")}##editMenuHeader"))
        {
            var script = SelectedScript;
            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Undo")}##undoAction", $"{InputManager.GetHint(KeybindID.Undo)}", false, script?.CanUndo == true || ScriptView.HasPendingEdit))
                Undo();

            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Redo")}##redoAction", $"{InputManager.GetHint(KeybindID.Redo)}", false, script?.CanRedo == true))
                Redo();

            ImGui.Separator();

            if (ImGui.MenuItem($"{LOC.Get("LUA_Edit_Toggle_Editing")}##luaEditToggle", "", ScriptView.Editing, script?.Text != null))
                ScriptView.SetEditing(!ScriptView.Editing);
            GUI.Tooltip(LOC.Get("LUA_Edit_Toggle_Editing_TT"));

            ImGui.EndMenu();
        }
    }

    public void ViewMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_View")}##viewMenuHeader"))
        {
            if (ImGui.MenuItem($"{LOC.Get("LUA_View_Line_Numbers")}##luaLineNumbers"))
                CFG.Current.LuaEditor_Show_Line_Numbers = !CFG.Current.LuaEditor_Show_Line_Numbers;
            GUI.ShowActiveStatus(CFG.Current.LuaEditor_Show_Line_Numbers);

            if (ImGui.MenuItem($"{LOC.Get("LUA_View_Users")}##luaUsers"))
                CFG.Current.LuaEditor_File_List_Display_Users = !CFG.Current.LuaEditor_File_List_Display_Users;
            GUI.Tooltip(LOC.Get("LUA_View_Users_TT"));
            GUI.ShowActiveStatus(CFG.Current.LuaEditor_File_List_Display_Users);

            if (ImGui.MenuItem($"{LOC.Get("LUA_View_Readable_Names")}##luaReadableNames"))
                CFG.Current.LuaEditor_Readable_Names = !CFG.Current.LuaEditor_Readable_Names;
            GUI.Tooltip(LOC.Get("LUA_View_Readable_Names_TT"));
            GUI.ShowActiveStatus(CFG.Current.LuaEditor_Readable_Names);

            ImGui.EndMenu();
        }
    }

    public List<string> GetUnsavedFiles()
    {
        ScriptView.CommitPending();
        return Bank?.GetDirtyFiles() ?? new();
    }

    public void SaveUnsaved()
    {
        Save();
    }

    public void SaveAll()
    {
        Save();
    }

    /// <summary>Saves every binder with unsaved changes into the project</summary>
    public void Save()
    {
        var bank = Bank;
        if (bank == null)
            return;

        ScriptView.CommitPending();

        if (bank.GetDirtyFiles().Count == 0)
            Smithbox.Log(this, LOC.Get("EDITOR_Data_Skip_File_Save", SelectedContainer?.Name ?? LOC.Get("LUA_Editor_Stub")));
        else
            bank.SaveAllDirty();

        Smithbox.Instance.SaveConfiguration();
    }

    public void OnDefocus()
    {
        ScriptView.CommitPending();
    }
}
