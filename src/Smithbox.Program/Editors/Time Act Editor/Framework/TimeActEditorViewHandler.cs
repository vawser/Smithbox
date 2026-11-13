using Hexa.NET.ImGui;
using StudioCore.Editors.ModelEditor;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActEditorViewHandler
{
    public TimeActEditorScreen Editor;
    public ProjectEntry Project;

    public List<TimeActEditorView> Views = new();
    public TimeActEditorView ActiveView;

    public bool AddNewView = false;
    public TimeActEditorView ViewToClose = null;

    public TimeActEditorViewHandler(TimeActEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;

        var initialView = new TimeActEditorView(Editor, Project, 0);

        Views = [initialView];
        ActiveView = initialView;
    }

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
                RemoveView(ActiveView);
            }
        }
    }

    public TimeActEditorView AddView()
    {
        var index = 0;
        while (index < Views.Count)
        {
            if (Views[index] == null)
            {
                break;
            }

            index++;
        }

        TimeActEditorView view = new(Editor, Project, index);

        if (index < Views.Count)
        {
            Views[index] = view;
        }
        else
        {
            Views.Add(view);
        }

        ActiveView = view;

        return view;
    }

    /// <summary>
    /// The open view other than the given one that has the source selected, or null if there is none.
    /// A source is only ever open in one view, as the views share its time acts and model.
    /// </summary>
    public TimeActEditorView FindViewWithSource(TimeActContainerWrapper source, TimeActEditorView except = null)
    {
        if (source == null)
            return null;

        return Views.FirstOrDefault(e => e != null && e != except && e.Selection.SelectedTimeActContainerWrapper == source);
    }

    /// <summary>
    /// Makes the view the active one, and brings its window to the front the next time it is drawn
    /// </summary>
    public void FocusView(TimeActEditorView view)
    {
        ActiveView = view;
        view.RequestFocus = true;
    }

    public bool RemoveView(TimeActEditorView view)
    {
        if (!Views.Contains(view))
        {
            return false;
        }

        // Its source is let go, so it can be opened in another view, and its model doesn't stay in this one
        view.SourceWindow.ReleaseSource();

        Views[view.ViewIndex] = null;

        // Its render pipeline's queues would otherwise keep running every frame, and its GPU targets stay allocated
        view.Dispose();

        if (view == ActiveView || ActiveView == null)
        {
            ActiveView = Views.FindLast(e => e != null);
        }

        return true;
    }

    /// <summary>
    /// Releases every view's viewport, for the project closing
    /// </summary>
    public void DisposeViews()
    {
        foreach (var view in Views)
        {
            view?.Dispose();
        }
    }

    public int CountViews()
    {
        return Views.Where(e => e != null).Count();
    }

    public void HandleViews(uint editorDockspaceId)
    {
        var activeView = ActiveView;

        foreach (var view in Views)
        {
            if (view == null)
            {
                continue;
            }

            var displayTitle = LOC.Get("EDITOR_Active_View");

            if (view != activeView)
            {
                displayTitle = LOC.Get("EDITOR_Inactive_View");
            }

            displayTitle = $"{displayTitle} [{view.ViewIndex}]";

            ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f), ImGuiCond.FirstUseEver);

            if (CountViews() == 1)
            {
                displayTitle = LOC.Get("EDITOR_Active_View");
            }

            ImGui.SetNextWindowDockID(editorDockspaceId, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowClass(ref GUI.DockGroup_TimeActEditor);

            if (view.RequestFocus)
            {
                view.RequestFocus = false;
                ImGui.SetNextWindowFocus();
            }

            if (ImGui.Begin($@"{displayTitle}###TimeActEditorView##{view.ViewIndex}", GUI.GetDisplayViewWindowFlags()))
            {
                if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                {
                    ActiveView = view;
                }

                if (ImGui.BeginPopupContextItem())
                {
                    if (ImGui.MenuItem($"{LOC.Get("EDITOR_Add_View")}##addView"))
                    {
                        AddNewView = true;
                    }

                    // Don't let the user close if their is only 1 view
                    if (CountViews() > 1)
                    {
                        if (ImGui.MenuItem($"{LOC.Get("EDITOR_Close_View")}##closeView"))
                        {
                            ViewToClose = view;
                        }
                    }

                    // Closes BeginPopupContextItem (was EndMenu, which unbalanced ImGui's stack)
                    ImGui.EndPopup();
                }
            }

            var dsid = ImGui.GetID($"DockSpace_TimeActEdit_View{view.ViewIndex}");
            ImGui.DockSpace(dsid, new Vector2(0, 0), ref GUI.DockGroup_TimeActEditorView);

            view.Display(dsid, view.ViewIndex, Editor.CommandQueue.DoFocus && view == activeView, view == activeView);

            ImGui.End();
        }

        if (AddNewView)
        {
            AddView();

            AddNewView = false;
        }

        if (ViewToClose != null)
        {
            RemoveView(ViewToClose);

            ViewToClose = null;
        }
    }
}
