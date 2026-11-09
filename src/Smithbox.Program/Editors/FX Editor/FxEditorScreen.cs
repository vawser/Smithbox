using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// The FX Editor: browse and edit FXR effects (FX Editor Plan, milestone E1+E2), with a viewport (phase V1). The
/// windows in the editor's dockspace: the Effect List, the Effect Tree, the Action Inspector, the Viewport and the
/// Tools window.
/// </summary>
public class FxEditorScreen : EditorScreen
{
    public ProjectEntry Project;

    public ActionManager ActionManager = new();

    public FxSelection Selection;

    public FxEffectList EffectList;
    public FxEffectTree EffectTree;
    public FxInspector Inspector;

    // Viewport
    public RenderScene RenderScene;
    public ViewportSelection ViewportSelection = new();
    public ViewportActionManager ViewportActionManager = new();
    public FxUniverse Universe;
    public FxViewportWindow ViewportWindow;
    public FxPlayback Playback = new();

    // Tools
    public FxToolWindow ToolWindow;
    public FxPreviewTool PreviewTool;
    public ViewportGridTool GridTool;
    public ViewportRenderingTool RenderingTool;

    public FxEditorScreen(ProjectEntry project)
    {
        Project = project;

        Selection = new FxSelection(this);
        EffectList = new FxEffectList(this);
        EffectTree = new FxEffectTree(this);
        Inspector = new FxInspector(this);

        RenderScene = Smithbox.Instance._context.Device != null ? new RenderScene() : null;
        Universe = new FxUniverse(this, project);
        ViewportWindow = new FxViewportWindow(this, project);

        PreviewTool = new FxPreviewTool(this);
        GridTool = new ViewportGridTool(ViewportGridEditor.Fx, () => ViewportWindow?.Viewport as VulkanViewport);
        RenderingTool = new ViewportRenderingTool(() => ViewportWindow?.Viewport as VulkanViewport);
        ToolWindow = new FxToolWindow(this);
    }

    public FxData Data => Project.Handler.FxData;

    public string EditorName => "";
    public string CommandEndpoint => "fx";
    public string SaveType => "FX";
    public string WindowName => "";
    public bool HasDocked { get; set; }

    public void OnGUI(string[] commands)
    {
        Data.Bank.StartIndexing();
        Data.Bank.Update();

        Shortcuts();

        if (ImGui.BeginMenuBar())
        {
            FileMenu();
            EditMenu();
            ViewMenu();

            // Saving can take a while (large binders are recompressed whole), so say what it's doing
            var bank = Data.Bank;
            if (bank.IsSaving)
            {
                ImGui.Separator();
                ImGui.TextColored(UI.Current.ImGui_Benefit_Text_Color,
                    LOC.Get("FX_Menu_Saving", bank.SaveCurrent ?? "", Math.Min(bank.SaveDone + 1, Math.Max(bank.SaveTotal, 1)), Math.Max(bank.SaveTotal, 1)));
            }
            ImGui.EndMenuBar();
        }

        var dsid = ImGui.GetID("DockSpace_FxEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None, ref GUI.DockGroup_FxEditor);

        Window($"{LOC.Get("FX_Window_Effect_List")}###fxEditor_EffectList", dsid, EditorFocusContext.FxEditor_EffectList, EffectList.Display);
        Window($"{LOC.Get("FX_Window_Effect_Tree")}###fxEditor_EffectTree", dsid, EditorFocusContext.FxEditor_EffectTree, EffectTree.Display);
        Window($"{LOC.Get("FX_Window_Inspector")}###fxEditor_Inspector", dsid, EditorFocusContext.FxEditor_Inspector, Inspector.Display);

        // Viewport: the shared viewport window sets the focus itself
        ViewportWindow.Display(dsid);

        if (CFG.Current.Interface_FxEditor_ToolWindow)
            Window($"{LOC.Get("FX_Window_Tools")}###fxEditor_Tools", dsid, EditorFocusContext.FxEditor_Tools, ToolWindow.Display, GUI.GetMainWindowFlags());
    }

    // ---------------------------------------------------------------- viewport

    public void Update(float dt)
    {
        // A new effect starts playing from the beginning
        if (Selection.Entry != PlaybackEntry)
        {
            PlaybackEntry = Selection.Entry;
            Playback.Restart();
        }

        Playback.Update(dt);

        ViewportWindow.Update(dt);

        // The map lighting the viewport borrows, loaded once there is an effect to show: reading the map's volumes
        // and GPARAM on the editor's first frame would hold it up (as in the Time Act Editor)
        if (Selection.Entry?.Fxr != null)
            Project.Handler.ViewportEnvironment.Update();
    }

    private FxEntry PlaybackEntry;

    public void Draw(GraphicsDevice device, CommandList cl)
    {
        if (ViewportWindow.Viewport is VulkanViewport { Visible: true })
            ViewportWindow.Draw(device, cl);
    }

    public void EditorResized(Sdl2Window window, GraphicsDevice device)
    {
        ViewportWindow.EditorResized(window, device);
    }

    public bool InputCaptured()
    {
        return ViewportWindow.InputCaptured();
    }

    private static void Window(string title, uint dockspace, EditorFocusContext focus, Action draw, ImGuiWindowFlags? flags = null)
    {
        ImGui.SetNextWindowDockID(dockspace, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_FxEditor);
        if (ImGui.Begin(title, flags ?? GUI.GetInnerWindowFlags()))
        {
            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                FocusManager.SetFocus(focus);
            draw();
        }
        ImGui.End();
    }

    private void Shortcuts()
    {
        if (!FocusManager.IsInFxEditor())
            return;

        if (InputManager.IsPressed(KeybindID.Save))
            Save();

        if (ActionManager.CanUndo() && InputManager.IsPressed(KeybindID.Undo))
            ActionManager.UndoAction();

        if (ActionManager.CanRedo() && InputManager.IsPressed(KeybindID.Redo))
            ActionManager.RedoAction();
    }

    private void FileMenu()
    {
        if (!ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_File")}##fxFileMenu"))
            return;

        // Save
        if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Save")}##fxSave", InputManager.GetHint(KeybindID.Save)))
            Save();
        GUI.Tooltip(LOC.Get("FX_Menu_Save_TT"));

        // Save All
        if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_SaveAll")}##fxSaveAll"))
            SaveAll();
        GUI.Tooltip(LOC.Get("FX_Menu_SaveAll_TT"));

        ImGui.Separator();

        // Reload the selected effect, dropping its unsaved changes
        if (ImGui.MenuItem($"{LOC.Get("FX_Menu_Reload")}##fxReload", "", false, Selection.Entry?.IsLoaded == true))
            ReloadEntry(Selection.Entry);
        GUI.Tooltip(LOC.Get("FX_Menu_Reload_TT"));

        ImGui.Separator();

        // Allow Automatic Save
        if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Allow_Automatic_Save")}##fxAutoSave"))
            CFG.Current.FxEditor_Allow_Automatic_Save = !CFG.Current.FxEditor_Allow_Automatic_Save;
        GUI.Tooltip(LOC.Get("EDITOR_Menubar_Allow_Automatic_Save_TT"));
        GUI.ShowActiveStatus(CFG.Current.FxEditor_Allow_Automatic_Save);

        ImGui.EndMenu();
    }

    private void EditMenu()
    {
        if (!ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_Edit")}##fxEditMenu"))
            return;

        if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Undo")}##fxUndo", InputManager.GetHint(KeybindID.Undo), false, ActionManager.CanUndo()))
            ActionManager.UndoAction();

        if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Undo_All")}##fxUndoAll", "", false, ActionManager.CanUndo()))
            ActionManager.UndoAllAction();

        if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Redo")}##fxRedo", InputManager.GetHint(KeybindID.Redo), false, ActionManager.CanRedo()))
            ActionManager.RedoAction();

        ImGui.EndMenu();
    }

    private void ViewMenu()
    {
        if (!ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_View")}##fxViewMenu"))
            return;

        // Descriptions
        if (ImGui.MenuItem($"{LOC.Get("FX_View_Toggle_Descriptions")}##fxToggleDescriptions"))
            CFG.Current.FxEditor_Inspector_Show_Descriptions = !CFG.Current.FxEditor_Inspector_Show_Descriptions;
        GUI.ShowActiveStatus(CFG.Current.FxEditor_Inspector_Show_Descriptions);

        // Raw values
        if (ImGui.MenuItem($"{LOC.Get("FX_View_Toggle_Raw")}##fxToggleRaw"))
            CFG.Current.FxEditor_Inspector_Show_Raw = !CFG.Current.FxEditor_Inspector_Show_Raw;
        GUI.ShowActiveStatus(CFG.Current.FxEditor_Inspector_Show_Raw);

        ImGui.Separator();

        // Tools window
        if (ImGui.MenuItem($"{LOC.Get("FX_View_Toggle_Tools")}##fxToggleTools"))
            CFG.Current.Interface_FxEditor_ToolWindow = !CFG.Current.Interface_FxEditor_ToolWindow;
        GUI.ShowActiveStatus(CFG.Current.Interface_FxEditor_ToolWindow);

        ImGui.EndMenu();
    }

    /// <summary>Drops the effect's unsaved changes. Its undo history goes too, as it points at the old objects.</summary>
    public void ReloadEntry(FxEntry entry)
    {
        if (entry == null)
            return;
        Data.Bank.Reload(entry);
        ActionManager.Clear();
        if (Selection.Entry == entry)
            Selection.ClearItem();
    }

    // ---------------------------------------------------------------- saving

    /// <summary>
    /// Saves every modified effect to every binder that holds it. A value still being typed is committed first,
    /// since Ctrl+S doesn't take focus off the field. (Saving only the selected effect lost edits made to
    /// another effect before it was selected.)
    /// </summary>
    public void Save(bool autoSave = false) => SaveAll(autoSave);

    public async void SaveAll(bool autoSave = false)
    {
        if (autoSave && !CFG.Current.FxEditor_Allow_Automatic_Save)
            return;

        if (!autoSave)
            Inspector.FlushPending();

        var dirty = Data.Bank.DirtyEntries.ToList();

        // Automatic saves stay quiet when there's nothing to save
        if (autoSave && dirty.Count == 0)
            return;

        await Data.Bank.SaveGuarded(dirty);

        Smithbox.Instance.SaveConfiguration();
    }

    // Matches what SaveAll writes: every binder holding a modified effect
    public List<string> GetUnsavedFiles()
    {
        var bank = Data?.Bank;
        if (bank == null)
            return new();

        return bank.DirtyEntries
            .SelectMany(e => e.Binders.Select(b => $"{b.Binder.Path} ({e.Stem})"))
            .OrderBy(e => e)
            .ToList();
    }

    private Task _unsavedSave;

    public void SaveUnsaved()
    {
        var bank = Data?.Bank;
        if (bank == null)
            return;

        // On the main thread, before the files are written on a worker thread
        Smithbox.Instance.SaveConfiguration();
        _unsavedSave = SaveUnsavedCore(bank);
    }

    private async Task SaveUnsavedCore(FxBank bank)
    {
        try
        {
            await bank.SaveAll();
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, e.Message, e);
        }
    }

    public bool IsSavingUnsaved => _unsavedSave is { IsCompleted: false };
}
