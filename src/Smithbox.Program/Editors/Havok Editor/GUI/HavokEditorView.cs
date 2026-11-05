using Hexa.NET.ImGui;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.HavokEditor;

public class HavokEditorView : IEditorView
{
    public HavokEditorScreen Editor;
    public ProjectEntry Project;

    /// <summary>
    /// The file bank, shared by every view so they all edit and save the same objects
    /// </summary>
    public HavokBank HavokBank => Editor.HavokBank;

    public ActionManager ActionManager;

    public HavokSelection Selection;

    public HavokPropertyCache PropertyCache = new();

    public HavokCategoryView CategoryView;
    public HavokBinderView BinderView;
    public HavokFileView FileView;
    public HavokPropertyView PropertyView;
    public HavokToolView Tools;

    public int ViewIndex;
    private int _imguiId = -1;

    public HavokEditorView(HavokEditorScreen editor, ProjectEntry project, int imguiId)
    {
        Editor = editor;
        Project = project;

        ViewIndex = imguiId;
        _imguiId = imguiId;

        Selection = new(this, project);
        ActionManager = new();

        // ActionPerformed is raised before the action is pushed onto the undo/redo stack,
        // so the stacks are checked on the next frame instead
        ActionManager.ActionPerformed += () =>
        {
            CheckForStructureChange = true;

            // A val: search matches values, so any edit can change its matches (in every view, as they share files)
            foreach (var view in Editor.ViewHandler?.Views ?? new())
            {
                if (view != null && view.PropertyView.PropFilter.StartsWith("val:"))
                {
                    view.PropertyView.InvalidateSearchCache();
                }
            }
        };

        CategoryView = new(this, project);
        BinderView = new(this, project);
        FileView = new(this, project);
        PropertyView = new(this, project);
        Tools = new(this, project);
    }

    /// <summary>
    /// Undo the last action, first showing the file (or binder) it changes if another one is shown
    /// </summary>
    public void Undo()
    {
        if (!ActionManager.CanUndo())
            return;

        ShowActionTarget(ActionManager.PeekUndoAction());
        ActionManager.UndoAction();
    }

    /// <summary>
    /// Redo the last undone action, first showing the file (or binder) it changes if another one is shown
    /// </summary>
    public void Redo()
    {
        if (!ActionManager.CanRedo())
            return;

        ShowActionTarget(ActionManager.PeekRedoAction());
        ActionManager.RedoAction();
    }

    /// <summary>
    /// Select the category, binder and file an action applies to, so an undo/redo is never applied out of view.
    /// Logs which file it switched to.
    /// </summary>
    private void ShowActionTarget(EditorAction action)
    {
        HavokCategoryMode category;
        FileDictionaryEntry binderEntry;
        string filePath = null;

        if (action is IHavokFileAction { FileWrapper: not null } fileAction)
        {
            if (!HavokBank.FindWrapper(fileAction.FileWrapper, out category, out binderEntry, out filePath))
                return;
        }
        else if (action is IHavokBinderAction binderAction)
        {
            binderEntry = binderAction.BinderEntry;

            if (!HavokBank.FindBinder(binderEntry, out category))
                return;
        }
        else
        {
            return;
        }

        var sameBinder = Selection.CategoryMode == category && Equals(Selection.BinderFileEntry, binderEntry);

        // Already shown (for a file action, the binder is enough)
        if (sameBinder && (filePath == null || filePath == Selection.FilePath))
            return;

        if (Selection.CategoryMode != category)
        {
            Selection.ClearSelection();
            Selection.CategoryMode = category;
            Selection.RebuildBinderAliasCache();
        }

        if (!sameBinder)
        {
            Selection.ClearFileSelection();
            Selection.BinderFileEntry = binderEntry;

            // No cache clear: the binder is already in the bank, and nothing needs evicting
            BinderView.PopulateFileList();
        }

        if (filePath != null)
        {
            FileView.SelectFile(filePath);

            Smithbox.Log(this, LOC.Get("HAVOK_Undo_Showing_File", Path.GetFileName(filePath), binderEntry.Filename));
        }
        else
        {
            Smithbox.Log(this, LOC.Get("HAVOK_Undo_Showing_Binder", binderEntry.Filename));
        }
    }

    private bool CheckForStructureChange = false;

    /// <summary>
    /// After an action that can add or remove objects (or its undo/redo), every view rebuilds
    /// the object lists it built from the file (the behavior tabs), since the views share the files
    /// </summary>
    private void HandleStructureChange()
    {
        if (!CheckForStructureChange)
            return;

        CheckForStructureChange = false;

        var changed = ActionManager.PeekUndoAction() is IHavokFileAction { ChangesStructure: true }
            || ActionManager.PeekRedoAction() is IHavokFileAction { ChangesStructure: true };

        if (!changed)
            return;

        foreach (var view in Editor.ViewHandler.Views)
        {
            if (view != null)
            {
                view.Selection.AppliedHavokTreeSearch = false;
                view.PropertyView.InvalidateSearchCache();
            }
        }
    }

    public void Display(uint dockspaceId, int viewIndex, bool doFocus, bool isActiveView)
    {
        HandleStructureChange();

        // Category
        // -> Type of hkx to populate/reference

        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_HavokEditorView);
        if (ImGui.Begin($@"{LOC.Get("HAVOK_Window_Category_List")}##havokeditor_CategoryList_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.HavokEditor_CategoryList);
                Editor.ViewHandler.ActiveView = this;
            }

            CategoryView.Draw();
        }

        ImGui.End();

        // Binder List
        // -> Binders that contain the specified type of hkx files

        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_HavokEditorView);
        if (ImGui.Begin($@"{LOC.Get("HAVOK_Window_Binder_List")}##havokeditor_BinderList_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.HavokEditor_BinderList);
                Editor.ViewHandler.ActiveView = this;
            }

            BinderView.Draw();
        }

        ImGui.End();

        // File List
        // -> The hkx files within a binder

        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_HavokEditorView);
        if (ImGui.Begin($@"{LOC.Get("HAVOK_Window_File_List")}##havokeditor_FileList_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.HavokEditor_FileList);
                Editor.ViewHandler.ActiveView = this;
            }

            FileView.Draw();
        }

        ImGui.End();

        // The behavior symbols are built from the shown file before the behavior windows draw
        // (the Properties window used to do this, but it now draws after them)
        if (Selection.CategoryMode is HavokCategoryMode.Behavior && Selection.SelectedFileWrapper?.Root is { } shownRoot)
        {
            Selection.ApplyFileSpecificTreeSearches(shownRoot);
        }

        var behaviorView = PropertyView.BehaviorView;

        // Behavior Categories
        // -> What the Contents window lists (Outline, Animations, State Machines ...)

        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_HavokEditorView);
        if (ImGui.Begin($@"{LOC.Get("HAVOK_Window_Behavior_Categories")}##havokeditor_BehaviorCategories_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.HavokEditor_BehaviorCategories);
                Editor.ViewHandler.ActiveView = this;
            }

            behaviorView.DisplayCategoryWindow();
        }

        ImGui.End();

        // Contents
        // -> The selected category's list (the Outline tree, the Animations table ...)

        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_HavokEditorView);
        if (ImGui.Begin($@"{LOC.Get("HAVOK_Window_Contents")}##havokeditor_Contents_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.HavokEditor_Contents);
                Editor.ViewHandler.ActiveView = this;

                // The property panel follows the window clicked last
                if (ImGui.IsMouseClicked(ImGuiMouseButton.Left) || ImGui.IsMouseClicked(ImGuiMouseButton.Right))
                    behaviorView.SelectionSource = HavokBehaviorSelectionSource.Contents;
            }

            behaviorView.DisplayContentsWindow();
        }

        ImGui.End();

        // Graph
        // -> The state machine of the Contents selection

        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_HavokEditorView);
        if (ImGui.Begin($@"{LOC.Get("HAVOK_Window_Graph")}##havokeditor_Graph_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.HavokEditor_Graph);
                Editor.ViewHandler.ActiveView = this;

                if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                    behaviorView.SelectionSource = HavokBehaviorSelectionSource.Graph;
            }

            behaviorView.DisplayGraphWindow();
        }

        ImGui.End();

        // Properties
        // -> Editor for the havok properties (copy MapHavokPropertyView)

        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_HavokEditorView);
        if (ImGui.Begin($@"{LOC.Get("HAVOK_Window_Properties")}##havokeditor_Properties_{viewIndex}", GUI.GetInnerWindowFlags()))
        {
            var width = ImGui.GetContentRegionAvail().X;
            var height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.HavokEditor_Properties);
                Editor.ViewHandler.ActiveView = this;
            }

            PropertyView.Draw();
        }

        ImGui.End();

        // Tools
        if (CFG.Current.Interface_HavokEditor_ToolWindow)
        {
            ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowClass(ref GUI.DockGroup_HavokEditorView);
            if (ImGui.Begin($@"{LOC.Get("HAVOK_Window_Tools")}##havokeditor_Tools_{viewIndex}", GUI.GetInnerWindowFlags()))
            {
                var width = ImGui.GetContentRegionAvail().X;
                var height = ImGui.GetContentRegionAvail().Y;

                if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                {
                    FocusManager.SetFocus(EditorFocusContext.HavokEditor_Tools);
                    Editor.ViewHandler.ActiveView = this;
                }

                Tools.Draw();
            }

            ImGui.End();
        }

        // The behavior dialogs (Add Animation, Clone State ...) can be opened from the Contents or Graph window,
        // so they're drawn here, outside both, where they're opened and drawn in the same place
        behaviorView.ActionsView.DisplayDialogs();

        Tools.CollisionGeneratorTool.DetectFlverSelectionMenu();
        Tools.CollisionGeneratorTool.DisplayFlverSelectionMenu();
    }
}
