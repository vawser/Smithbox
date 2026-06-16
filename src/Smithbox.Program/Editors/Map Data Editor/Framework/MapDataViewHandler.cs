using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.MapDataEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapDataEditor;


public class MapDataViewHandler
{
    public MapDataEditorScreen Editor;
    public ProjectEntry Project;

    public List<MapDataEditorView> MapDataViews = new();
    public MapDataEditorView ActiveView;

    public bool AddNewView = false;
    public MapDataEditorView ViewToClose = null;

    public MapDataViewHandler(MapDataEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;

        var initialView = new MapDataEditorView(Editor, Project, 0);

        MapDataViews = [initialView];
        ActiveView = initialView;
    }

    public void DisplayMenu()
    {
        if (ImGui.MenuItem("Add New View", false))
        {
            AddView();
        }

        var canClose = CountViews() > 1;
        if (ImGui.MenuItem("Close Current View", false, canClose))
        {
            if (CountViews() > 1)
            {
                RemoveView(ActiveView);
            }
        }
    }

    public MapDataEditorView AddView()
    {
        var index = 0;
        while (index < MapDataViews.Count)
        {
            if (MapDataViews[index] == null)
            {
                break;
            }

            index++;
        }

        MapDataEditorView view = new(Editor, Project, index);

        if (index < MapDataViews.Count)
        {
            MapDataViews[index] = view;
        }
        else
        {
            MapDataViews.Add(view);
        }

        ActiveView = view;

        return view;
    }

    public bool RemoveView(MapDataEditorView view)
    {
        if (!MapDataViews.Contains(view))
        {
            return false;
        }

        MapDataViews[view.ViewIndex] = null;

        if (view == ActiveView || ActiveView == null)
        {
            ActiveView = MapDataViews.FindLast(e => e != null);
        }

        return true;
    }

    public int CountViews()
    {
        return MapDataViews.Where(e => e != null).Count();
    }

    public void HandleViews(uint editorDockspaceId)
    {
        var activeView = ActiveView;

        foreach (var view in MapDataViews)
        {
            if (view == null)
            {
                continue;
            }

            var displayTitle = "Active View";

            if (view != activeView)
            {
                displayTitle = "Inactive View";
            }

            displayTitle = $"{displayTitle} [{view.ViewIndex}]";

            ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f), ImGuiCond.FirstUseEver);

            if (CountViews() == 1)
            {
                displayTitle = "Active View";
            }

            ImGui.SetNextWindowDockID(editorDockspaceId, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowClass(ref UIHelper.DockGroup_MapDataEditor);
            if (ImGui.Begin($@"{displayTitle}###MapDataEditorView##{view.ViewIndex}", UIHelper.GetInnerWindowFlags()))
            {
                if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                {
                    ActiveView = view;
                }

                if (ImGui.BeginPopupContextItem())
                {
                    if (ImGui.MenuItem("Add View"))
                    {
                        AddNewView = true;
                    }

                    // Don't let the user close if their is only 1 view
                    if (CountViews() > 1)
                    {
                        if (ImGui.MenuItem("Close View"))
                        {
                            ViewToClose = view;
                        }
                    }

                    ImGui.EndMenu();
                }
            }

            var dsid = ImGui.GetID($"DockSpace_MapDataEditor_View{view.ViewIndex}");
            ImGui.DockSpace(dsid, new Vector2(0, 0), ref UIHelper.DockGroup_MapDataEditorView);

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
