#nullable enable
using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.ParamEditor;
using StudioCore.Keybinds;
using System.Numerics;

namespace StudioCore.Editors.FaceGenEditor;

/// <summary>
/// The FaceGen Editor: the faces of player-like characters (NPC players, invaders, the creator's presets), one face
/// param row at a time, as grouped sliders, colours and part choices, with the rows that use each face.
/// <para>
/// It has no data of its own. It reads the face params of the Param Editor's primary bank, and edits them through the
/// Param Editor's action manager, so an edit here is undone in either editor and saved by the Param Editor.
/// </para>
/// </summary>
public class FaceGenEditorScreen : EditorScreen
{
    public ProjectEntry Project;

    public FaceGenEditorView View;

    public FaceGenEditorScreen(ProjectEntry project)
    {
        Project = project;

        View = new FaceGenEditorView(this, project);
    }

    /// <summary>The Param Editor, whose params and undo history this editor works on</summary>
    public ParamEditorScreen? ParamEditor => Project.Handler.ParamEditor;

    public ActionManager? ActionManager => ParamEditor?.ActionManager;

    public string EditorName => "FaceGen Editor";
    public string CommandEndpoint => "facegen";
    public string SaveType => "Params";
    public string WindowName => "";
    public bool HasDocked { get; set; }

    public void OnGUI(string[] commands)
    {
        HandleCommands(commands);
        Shortcuts();

        if (ImGui.BeginMenuBar())
        {
            FileMenu();
            EditMenu();
            ViewMenu();

            ToolSections.EditorOptionsMenu();

            ImGui.EndMenuBar();
        }

        var dsid = ImGui.GetID("DockSpace_FaceGenEditor");
        BuildDefaultLayout(dsid);
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None, ref GUI.DockGroup_FaceGenEditor);

        View.Display(dsid);
    }

    /// <summary>facegen/select/&lt;param&gt;/&lt;row id&gt;: shows a face row</summary>
    private void HandleCommands(string[] commands)
    {
        if (commands is not { Length: >= 3 } || commands[0] != "select")
            return;

        if (int.TryParse(commands[2], out var id) && !View.Select(commands[1], id))
            Smithbox.Log(this, LOC.Get("FACEGEN_Select_Missing_Row", commands[1], id), LogLevel.Warning);
    }

    private void Shortcuts()
    {
        if (!FocusManager.IsInFaceGenEditor() || ActionManager is not { } actions)
            return;

        // Not while a value is being typed, where the keys are the field's
        if (ImGui.GetIO().WantTextInput)
            return;

        if (InputManager.IsPressed(KeybindID.Save))
            ParamEditor?.Save();

        if (actions.CanUndo() && InputManager.IsPressed(KeybindID.Undo))
            actions.UndoAction();

        if (actions.CanRedo() && InputManager.IsPressed(KeybindID.Redo))
            actions.RedoAction();

        // Copy, paste, duplicate, delete, select all and stepping with the arrow keys, on the list's rows
        if (FocusManager.IsFocus(EditorFocusContext.FaceGenEditor_List))
            View.ListView.Shortcuts();
    }

    private void FileMenu()
    {
        if (!ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_File")}##faceGenFileMenu"))
            return;

        var saving = ParamEditor?.IsSaving == true;

        if (ImGui.MenuItem($"{LOC.Get("FACEGEN_Menu_Save_Params")}##faceGenSave", InputManager.GetHint(KeybindID.Save), false, ParamEditor != null && !saving))
            ParamEditor!.Save();
        GUI.Tooltip(LOC.Get("FACEGEN_Menu_Save_Params_TT"));

        ImGui.EndMenu();
    }

    private void EditMenu()
    {
        if (!ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_Edit")}##faceGenEditMenu"))
            return;

        if (ActionManager is { } actions)
        {
            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Undo")}##faceGenUndo", InputManager.GetHint(KeybindID.Undo), false, actions.CanUndo()))
                actions.UndoAction();

            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Redo")}##faceGenRedo", InputManager.GetHint(KeybindID.Redo), false, actions.CanRedo()))
                actions.RedoAction();
        }

        ImGui.EndMenu();
    }

    private void ViewMenu()
    {
        if (!ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_View")}##faceGenViewMenu"))
            return;

        Toggle("FACEGEN_Window_List", ref CFG.Current.Interface_FaceGenEditor_List);
        Toggle("FACEGEN_Window_Properties", ref CFG.Current.Interface_FaceGenEditor_Properties);
        Toggle("FACEGEN_Window_References", ref CFG.Current.Interface_FaceGenEditor_References);

        ImGui.Separator();

        Toggle("FACEGEN_Menu_Show_Internal_Names", ref CFG.Current.FaceGenEditor_Show_Internal_Names);
        GUI.Tooltip(LOC.Get("FACEGEN_Menu_Show_Internal_Names_TT"));

        ImGui.EndMenu();
    }

    private static void Toggle(string locKey, ref bool value)
    {
        if (ImGui.MenuItem($"{LOC.Get(locKey)}##{locKey}"))
            value = !value;

        GUI.ShowActiveStatus(value);
    }

    /// <summary>
    /// The list on the left, the properties in the middle and the references on the right, the first time the editor
    /// is shown. Once the layout exists it is left alone.
    /// </summary>
    private static unsafe void BuildDefaultLayout(uint dockspaceId)
    {
        if (ImGuiP.DockBuilderGetNode(dockspaceId).Handle != null)
            return;

        ImGuiP.DockBuilderAddNode(dockspaceId, (ImGuiDockNodeFlags)ImGuiDockNodeFlagsPrivate.Space);
        ImGuiP.DockBuilderSetNodeSize(dockspaceId, ImGui.GetContentRegionAvail());

        uint listId;
        uint restId;
        ImGuiP.DockBuilderSplitNode(dockspaceId, ImGuiDir.Left, 0.22f, &listId, &restId);

        uint referencesId;
        uint propertiesId;
        ImGuiP.DockBuilderSplitNode(restId, ImGuiDir.Right, 0.28f, &referencesId, &propertiesId);

        ImGuiP.DockBuilderDockWindow(FaceGenEditorView.ListWindowId, listId);
        ImGuiP.DockBuilderDockWindow(FaceGenEditorView.PropertiesWindowId, propertiesId);
        ImGuiP.DockBuilderDockWindow(FaceGenEditorView.ReferencesWindowId, referencesId);

        ImGuiP.DockBuilderFinish(dockspaceId);
    }

    // ---- saving: the Param Editor's

    public void Save() => ParamEditor?.Save();

    public void SaveAll() => ParamEditor?.SaveAll();
}
