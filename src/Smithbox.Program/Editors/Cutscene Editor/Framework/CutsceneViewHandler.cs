using Hexa.NET.ImGui;
using System.Numerics;

namespace StudioCore.Editors.CutsceneEditor;

/// <summary>
/// The Cutscene Editor's views: each has its own cut shown, playback, undo history and viewport, over the shared cutscene files
/// </summary>
public class CutsceneViewHandler
{
    public CutsceneEditorScreen Editor;
    public ProjectEntry Project;

    public List<CutsceneEditorView> Views = new();
    public CutsceneEditorView ActiveView;

    public bool AddNewView = false;
    public CutsceneEditorView ViewToClose = null;

    public CutsceneViewHandler(CutsceneEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;

        var initialView = new CutsceneEditorView(Editor, Project, 0);

        Views = [initialView];
        ActiveView = initialView;
    }

    /// <summary>The open views (closed views leave a null in <see cref="Views"/> so their index can be reused)</summary>
    public IEnumerable<CutsceneEditorView> OpenViews => Views.Where(e => e != null);

    public void DisplayMenu()
    {
        if (ImGui.MenuItem($"{LOC.Get("EDITOR_Add_New_View")}##addNewView", false))
        {
            AddView();
        }

        var canClose = CountViews() > 1;
        if (ImGui.MenuItem($"{LOC.Get("EDITOR_Close_Current_View")}##closeCurrentView", false, canClose))
        {
            if (CountViews() > 1)
            {
                // After the frame's draw (CutsceneEditorScreen.Draw), as the viewport may still be drawn this frame
                ViewToClose = ActiveView;
            }
        }
    }

    public CutsceneEditorView AddView()
    {
        var index = 0;
        while (index < Views.Count)
        {
            if (Views[index] == null)
                break;

            index++;
        }

        CutsceneEditorView view = new(Editor, Project, index);

        if (index < Views.Count)
            Views[index] = view;
        else
            Views.Add(view);

        ActiveView = view;

        return view;
    }

    public bool RemoveView(CutsceneEditorView view)
    {
        if (!Views.Contains(view))
            return false;

        Views[view.ViewIndex] = null;

        // Its render pipeline would otherwise keep its GPU targets, and its map and models stay loaded
        view.Dispose();

        if (view == ActiveView || ActiveView == null)
            ActiveView = Views.FindLast(e => e != null);

        return true;
    }

    /// <summary>
    /// Releases every view's viewport, for the project closing
    /// </summary>
    public void DisposeViews()
    {
        foreach (var view in OpenViews)
            view.Dispose();
    }

    public int CountViews()
    {
        return Views.Count(e => e != null);
    }

    public void HandleViews(uint editorDockspaceId)
    {
        var activeView = ActiveView;

        foreach (var view in Views)
        {
            if (view == null)
                continue;

            var displayTitle = LOC.Get("EDITOR_Active_View");

            if (view != activeView)
                displayTitle = LOC.Get("EDITOR_Inactive_View");

            displayTitle = $"{displayTitle} [{view.ViewIndex}]";

            ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * DPI.UIScale(), ImGuiCond.FirstUseEver);

            if (CountViews() == 1)
                displayTitle = LOC.Get("EDITOR_Active_View");

            ImGui.SetNextWindowDockID(editorDockspaceId, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowClass(ref GUI.DockGroup_CutsceneEditor);
            if (ImGui.Begin($@"{displayTitle}###CutsceneEditorView##{view.ViewIndex}", GUI.GetDisplayViewWindowFlags()))
            {
                if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                    ActiveView = view;

                if (ImGui.BeginPopupContextItem())
                {
                    if (ImGui.MenuItem($"{LOC.Get("EDITOR_Add_View")}##addView"))
                        AddNewView = true;

                    // Don't let the user close the only view
                    if (CountViews() > 1)
                    {
                        if (ImGui.MenuItem($"{LOC.Get("EDITOR_Close_View")}##closeView"))
                            ViewToClose = view;
                    }

                    ImGui.EndPopup();
                }
            }

            var dsid = ImGui.GetID($"DockSpace_CutsceneEditor_View{view.ViewIndex}");
            ImGui.DockSpace(dsid, new Vector2(0, 0), ref GUI.DockGroup_CutsceneEditorView);

            view.Display(dsid);

            ImGui.End();
        }

        if (AddNewView)
        {
            AddView();
            AddNewView = false;
        }

    }
}
