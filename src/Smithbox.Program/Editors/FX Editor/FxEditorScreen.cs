using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using System.Globalization;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// The FX Editor: browse and edit FXR effects (FX.md, milestone E1+E2), with a viewport (phase V1). Each
/// view (<see cref="FxEditorView"/>) has its own Effect List, Effect Tree, Action Inspector, Viewport and Tools
/// window; the screen keeps what the views share: the bank, saving, and the notes, favorites, templates and scans.
/// </summary>
public class FxEditorScreen : EditorScreen
{
    public ProjectEntry Project;

    public FxViewHandler ViewHandler;

    /// <summary>The view the menus, shortcuts and commands act on</summary>
    public FxEditorView ActiveView => ViewHandler.ActiveView;

    // Shared by every view: kept in the project's files (notes, favorites, templates), or scans of the whole project
    public FxNotes Notes;
    public FxFavorites Favorites;
    public FxTemplates Templates;
    public FxUsage Usage;
    public FxExternalReferences External;
    public FxProjectCheck ProjectCheck;

    /// <summary>The FX Reloader's game work, shared by every view</summary>
    public FxReloader Reloader;

    public FxEditorScreen(ProjectEntry project)
    {
        Project = project;

        Notes = new FxNotes(this);
        Favorites = new FxFavorites(this);
        Templates = new FxTemplates(this);
        Usage = new FxUsage(this);
        External = new FxExternalReferences(this);
        ProjectCheck = new FxProjectCheck(this);
        Reloader = new FxReloader(project);

        ViewHandler = new FxViewHandler(this, project);
    }

    public FxData Data => Project.Handler.FxData;

    public string EditorName => "FX Editor";
    public string CommandEndpoint => "fx";
    public string SaveType => "FX";
    public string WindowName => "";
    public bool HasDocked { get; set; }

    /// <summary>
    /// Set once the FX Editor window (or a window docked inside it) has had focus. The bank isn't indexed before
    /// then: the editor's window can draw while the user is elsewhere (startup, before the dock layout settles, or
    /// docked side by side), and indexing reads every FXR binder in the project.
    /// </summary>
    private bool HasBeenFocused;

    public void OnGUI(string[] commands)
    {
        // Called inside the FX Editor's host window, so this tests that window and the windows in its dockspace
        if (!HasBeenFocused && ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows | ImGuiFocusedFlags.DockHierarchy))
            HasBeenFocused = true;

        if (HasBeenFocused)
            Data.Bank.StartIndexing();
        Data.Bank.Update();

        HandleCommands(commands);

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
            ToolSections.EditorOptionsMenu();

            ImGui.EndMenuBar();
        }

        var dsid = ImGui.GetID("DockSpace_FxEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None, ref GUI.DockGroup_FxEditor);

        ViewHandler.HandleViews(dsid);
    }

    // ---------------------------------------------------------------- commands

    /// <summary>An effect id a command asked for, kept until the bank is indexed.</summary>
    private int? PendingJump;

    /// <summary>
    /// fx/select/&lt;id&gt;: select the effect f&lt;id&gt; in the active view and scroll the list to it (FxReferenceHelper
    /// sends this from the other editors). The bank indexes on first use, so a jump made before then waits for it.
    /// </summary>
    private void HandleCommands(string[] commands)
    {
        if (commands is { Length: >= 2 } && commands[0] == "select"
            && int.TryParse(commands[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
        {
            PendingJump = id;
        }

        if (PendingJump is not int jump || !Data.Bank.IsIndexed || ActiveView == null)
            return;

        PendingJump = null;
        ActiveView.JumpTo(jump);
    }

    // ---------------------------------------------------------------- viewport

    public void Update(float dt)
    {
        // Only the active view's viewport takes input and plays, as in the Time Act Editor
        var view = ActiveView;
        if (view == null)
            return;

        view.Update(dt);

        // The map lighting the viewport borrows, loaded once there is an effect to show: reading the map's volumes
        // and GPARAM on the editor's first frame would hold it up (as in the Time Act Editor)
        if (view.Selection.Entry?.Fxr != null)
            Project.Handler.ViewportEnvironment.Update();
    }

    public void Draw(GraphicsDevice device, CommandList cl)
    {
        if (ViewHandler.ViewToClose == null)
        {
            foreach (var view in ViewHandler.OpenViews)
                view.Draw(device, cl);
        }

        // Done here so the list isn't changed while the views are drawn
        if (ViewHandler.ViewToClose != null)
        {
            ViewHandler.RemoveView(ViewHandler.ViewToClose);
            ViewHandler.ViewToClose = null;
        }
    }

    public void EditorResized(Sdl2Window window, GraphicsDevice device)
    {
        ActiveView?.EditorResized(window, device);
    }

    public bool InputCaptured()
    {
        return ActiveView?.InputCaptured() ?? false;
    }

    private void Shortcuts()
    {
        if (!FocusManager.IsInFxEditor())
            return;

        var view = ActiveView;
        if (view == null)
            return;

        if (InputManager.IsPressed(KeybindID.Save))
            Save();

        // A value being edited is committed first, so undo never runs under a live change
        if (view.ActionManager.CanUndo() && InputManager.IsPressed(KeybindID.Undo))
        {
            view.FlushEdits();
            view.ActionManager.UndoAction();
        }

        if (view.ActionManager.CanRedo() && InputManager.IsPressed(KeybindID.Redo))
        {
            view.FlushEdits();
            view.ActionManager.RedoAction();
        }

        view.EffectTree.Shortcuts();
        view.EffectList.Shortcuts();
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
        var selected = ActiveView?.Selection.Entry;
        if (ImGui.MenuItem($"{LOC.Get("FX_Menu_Reload")}##fxReload", "", false, selected?.IsLoaded == true))
            ReloadEntry(selected);
        GUI.Tooltip(LOC.Get("FX_Menu_Reload_TT"));

        ImGui.Separator();

        // Allow Automatic Save
        if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Allow_Automatic_Save")}##fxAutoSave"))
            CFG.Current.FxEditor.FxEditor_Allow_Automatic_Save = !CFG.Current.FxEditor.FxEditor_Allow_Automatic_Save;
        GUI.Tooltip(LOC.Get("EDITOR_Menubar_Allow_Automatic_Save_TT"));
        GUI.ShowActiveStatus(CFG.Current.FxEditor.FxEditor_Allow_Automatic_Save);

        ImGui.EndMenu();
    }

    private void EditMenu()
    {
        if (!ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_Edit")}##fxEditMenu"))
            return;

        var view = ActiveView;
        if (view != null)
        {
            var actions = view.ActionManager;

            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Undo")}##fxUndo", InputManager.GetHint(KeybindID.Undo), false, actions.CanUndo()))
            {
                view.FlushEdits();
                actions.UndoAction();
            }

            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Undo_All")}##fxUndoAll", "", false, actions.CanUndo()))
            {
                view.FlushEdits();
                actions.UndoAllAction();
            }

            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Redo")}##fxRedo", InputManager.GetHint(KeybindID.Redo), false, actions.CanRedo()))
            {
                view.FlushEdits();
                actions.RedoAction();
            }

            ImGui.Separator();

            // The selected node in the Effect Tree
            var node = view.EffectTree.EditableNode;

            if (ImGui.MenuItem($"{LOC.Get("FX_EffectTree_Duplicate")}##fxDuplicateNode", InputManager.GetHint(KeybindID.Duplicate), false, node != null))
                view.EffectTree.DuplicateNode(node);
            GUI.Tooltip(LOC.Get("FX_EffectTree_Duplicate_TT"));

            if (ImGui.MenuItem($"{LOC.Get("FX_EffectTree_Delete")}##fxDeleteNode", InputManager.GetHint(KeybindID.Delete), false, node != null))
                view.EffectTree.DeleteNode(node);
            GUI.Tooltip(LOC.Get("FX_EffectTree_Delete_TT"));
        }

        ImGui.EndMenu();
    }

    private void ViewMenu()
    {
        if (!ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_View")}##fxViewMenu"))
            return;

        // Descriptions
        if (ImGui.MenuItem($"{LOC.Get("FX_View_Toggle_Descriptions")}##fxToggleDescriptions"))
            CFG.Current.FxEditor.FxEditor_Inspector_Show_Descriptions = !CFG.Current.FxEditor.FxEditor_Inspector_Show_Descriptions;
        GUI.ShowActiveStatus(CFG.Current.FxEditor.FxEditor_Inspector_Show_Descriptions);

        // Raw values
        if (ImGui.MenuItem($"{LOC.Get("FX_View_Toggle_Raw")}##fxToggleRaw"))
            CFG.Current.FxEditor.FxEditor_Inspector_Show_Raw = !CFG.Current.FxEditor.FxEditor_Inspector_Show_Raw;
        GUI.ShowActiveStatus(CFG.Current.FxEditor.FxEditor_Inspector_Show_Raw);

        // Auto-framing a newly selected effect
        if (ImGui.MenuItem($"{LOC.Get("FX_PreviewTool_Auto_Frame")}##fxToggleAutoFrame"))
            CFG.Current.FxEditor.FxEditor_Viewport_Auto_Frame = !CFG.Current.FxEditor.FxEditor_Viewport_Auto_Frame;
        GUI.Tooltip(LOC.Get("FX_PreviewTool_Auto_Frame_TT"));
        GUI.ShowActiveStatus(CFG.Current.FxEditor.FxEditor_Viewport_Auto_Frame);

        ImGui.Separator();

        // Curve Editor
        if (ImGui.MenuItem($"{LOC.Get("FX_View_Toggle_Curve_Editor")}##fxToggleCurveEditor"))
            CFG.Current.Interface.Interface_FxEditor_CurveEditor = !CFG.Current.Interface.Interface_FxEditor_CurveEditor;
        GUI.ShowActiveStatus(CFG.Current.Interface.Interface_FxEditor_CurveEditor);

        // Tools window
        if (ImGui.MenuItem($"{LOC.Get("FX_View_Toggle_Tools")}##fxToggleTools"))
            CFG.Current.Interface.Interface_FxEditor_ToolWindow = !CFG.Current.Interface.Interface_FxEditor_ToolWindow;
        GUI.ShowActiveStatus(CFG.Current.Interface.Interface_FxEditor_ToolWindow);

        ImGui.Separator();

        ViewHandler.DisplayMenu();

        ImGui.EndMenu();
    }

    /// <summary>
    /// Drops the effect's unsaved changes. Every view's undo history goes too, as it may point at the old objects, and
    /// every view showing the effect lets go of it.
    /// </summary>
    public void ReloadEntry(FxEntry entry)
    {
        if (entry == null)
            return;
        Data.Bank.Reload(entry);
        Notes.OnReloaded(entry);

        foreach (var view in ViewHandler.OpenViews)
        {
            view.ActionManager.Clear();
            if (view.Selection.Entry == entry)
                view.Selection.ClearItem();
        }
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
        if (autoSave && !CFG.Current.FxEditor.FxEditor_Allow_Automatic_Save)
            return;

        if (!autoSave)
        {
            foreach (var view in ViewHandler.OpenViews)
            {
                view.Inspector.FlushPending();
                view.CurveEditor.FlushPending();
            }
        }

        var dirty = Data.Bank.DirtyEntries.ToList();

        // Automatic saves stay quiet when there's nothing to save
        if (autoSave && !Data.Bank.HasChanges)
            return;

        await Data.Bank.SaveGuarded(dirty);

        // Node notes are keyed by position in the saved effect, so they're written once it is saved
        Notes.OnSaved(dirty);

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
            .Concat(bank.PendingRemovals().Select(x => $"{x.where.Binder.Path} ({LOC.Get("FX_Unsaved_Removal", x.entry.Stem)})"))
            .Distinct()
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

    /// <summary>Releases every view's viewport, for the project closing</summary>
    public void Dispose()
    {
        ViewHandler.DisposeViews();
    }
}
