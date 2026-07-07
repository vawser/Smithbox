using Hexa.NET.ImGui;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MaterialEditor;
public class MaterialViewHandler
{
    public MaterialEditorScreen Editor;
    public ProjectEntry Project;

    public List<MaterialEditorView> MaterialViews = new();
    public MaterialEditorView ActiveView;

    public bool AddNewView = false;
    public MaterialEditorView ViewToClose = null;

    public MaterialViewHandler(MaterialEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;

        var initialView = new MaterialEditorView(Editor, Project, 0);

        MaterialViews = [initialView];
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

    public MaterialEditorView AddView()
    {
        var index = 0;
        while (index < MaterialViews.Count)
        {
            if (MaterialViews[index] == null)
            {
                break;
            }

            index++;
        }

        MaterialEditorView view = new(Editor, Project, index);

        if (index < MaterialViews.Count)
        {
            MaterialViews[index] = view;
        }
        else
        {
            MaterialViews.Add(view);
        }

        ActiveView = view;

        return view;
    }

    public bool RemoveView(MaterialEditorView view)
    {
        if (!MaterialViews.Contains(view))
        {
            return false;
        }

        MaterialViews[view.ViewIndex] = null;

        if (view == ActiveView || ActiveView == null)
        {
            ActiveView = MaterialViews.FindLast(e => e != null);
        }

        return true;
    }

    public int CountViews()
    {
        return MaterialViews.Where(e => e != null).Count();
    }

    public void HandleViews(uint editorDockspaceId)
    {
        var activeView = ActiveView;

        foreach (var view in MaterialViews)
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
            ImGui.SetNextWindowClass(ref GUI.DockGroup_MaterialEditor);
            if (ImGui.Begin($@"{displayTitle}###MaterialEditorView##{view.ViewIndex}", GUI.GetInnerWindowFlags()))
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

            var dsid = ImGui.GetID($"DockSpace_MaterialEditor_View{view.ViewIndex}");
            ImGui.DockSpace(dsid, new Vector2(0, 0), ref GUI.DockGroup_MaterialEditorView);

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
