#nullable enable
using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.ParamEditor;
using StudioCore.Keybinds;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;

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

        // Before the viewport loads any face part, so they keep the vertices their shape is written over
        FaceMorpher.RetainFaceParts();

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

        // A new ID with the viewport (Phase 3), so a layout saved without it is made again
        var dsid = ImGui.GetID("DockSpace_FaceGenEditor_Viewport");
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

        ImGui.Separator();

        // The body the face is shown on
        if (ImGui.BeginMenu($"{LOC.Get("FACEGEN_Menu_Body")}##faceGenBodyMenu"))
        {
            BodyOption("FACEGEN_Menu_Body_Wearer", 0);
            BodyOption("FACEGEN_Menu_Body_Male", 1);
            BodyOption("FACEGEN_Menu_Body_Female", 2);
            ImGui.EndMenu();
        }
        GUI.Tooltip(LOC.Get("FACEGEN_Menu_Body_TT"));

        Toggle("FACEGEN_Menu_Apply_Shape", ref CFG.Current.FaceGenEditor_Apply_Shape);
        GUI.Tooltip(LOC.Get("FACEGEN_Menu_Apply_Shape_TT"));

        if (ImGui.MenuItem($"{LOC.Get("FACEGEN_Menu_Frame_Face")}##faceGenFrameFace"))
            View.ViewportWindow.FrameFace();

        if (ImGui.MenuItem($"{LOC.Get("FACEGEN_Menu_Frame_Body")}##faceGenFrameBody"))
            View.ViewportWindow.FrameBody();

        ImGui.EndMenu();
    }

    private static void BodyOption(string locKey, int value)
    {
        if (ImGui.MenuItem($"{LOC.Get(locKey)}##{locKey}"))
            CFG.Current.FaceGenEditor_Body = value;

        GUI.ShowActiveStatus(CFG.Current.FaceGenEditor_Body == value);
    }

    private static void Toggle(string locKey, ref bool value)
    {
        if (ImGui.MenuItem($"{LOC.Get(locKey)}##{locKey}"))
            value = !value;

        GUI.ShowActiveStatus(value);
    }

    /// <summary>
    /// The list over the references on the left, the properties on the right, and the viewport between them, the
    /// first time the editor is shown. Once the layout exists it is left alone.
    /// </summary>
    private static unsafe void BuildDefaultLayout(uint dockspaceId)
    {
        if (ImGuiP.DockBuilderGetNode(dockspaceId).Handle != null)
            return;

        ImGuiP.DockBuilderAddNode(dockspaceId, (ImGuiDockNodeFlags)ImGuiDockNodeFlagsPrivate.Space);
        ImGuiP.DockBuilderSetNodeSize(dockspaceId, ImGui.GetContentRegionAvail());

        uint leftId;
        uint restId;
        ImGuiP.DockBuilderSplitNode(dockspaceId, ImGuiDir.Left, 0.2f, &leftId, &restId);

        uint listId;
        uint referencesId;
        ImGuiP.DockBuilderSplitNode(leftId, ImGuiDir.Up, 0.65f, &listId, &referencesId);

        uint propertiesId;
        uint centerId;
        ImGuiP.DockBuilderSplitNode(restId, ImGuiDir.Right, 0.38f, &propertiesId, &centerId);

        ImGuiP.DockBuilderDockWindow(FaceGenEditorView.ListWindowId, listId);
        ImGuiP.DockBuilderDockWindow(FaceGenEditorView.ReferencesWindowId, referencesId);
        ImGuiP.DockBuilderDockWindow(FaceGenEditorView.PropertiesWindowId, propertiesId);
        ImGuiP.DockBuilderDockWindow(FaceViewportWindow.WindowId, centerId);

        ImGuiP.DockBuilderFinish(dockspaceId);
    }

    // ---- viewport

    public void Update(float dt)
    {
        View.Update(dt);
    }

    public void Draw(GraphicsDevice device, CommandList cl)
    {
        View.Draw(device, cl);
    }

    public void EditorResized(Sdl2Window window, GraphicsDevice device)
    {
        View.EditorResized(window, device);
    }

    public bool InputCaptured()
    {
        return View.InputCaptured();
    }

    public void Dispose()
    {
        View.Dispose();
    }

    // ---- saving: the Param Editor's

    public void Save() => ParamEditor?.Save();

    public void SaveAll() => ParamEditor?.SaveAll();
}
