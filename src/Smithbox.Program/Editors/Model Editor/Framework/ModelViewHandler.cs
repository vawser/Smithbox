using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

public class ModelViewHandler
{
    public ModelEditorScreen Editor;
    public ProjectEntry Project;

    public List<ModelEditorView> Views = new();
    public ModelEditorView ActiveView;

    public ModelEditorView ViewToClose = null;

    public ModelViewHandler(ModelEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;

        var initialView = new ModelEditorView(Editor, Project, 0);

        Views = [initialView];
        ActiveView = initialView;
    }

    public void DisplayMenu()
    {
        if (ImGui.MenuItem("New Editor View"))
        {
            AddView();
        }

        if (ImGui.MenuItem("Close Current Editor View"))
        {
            if (CountViews() > 1)
            {
                RemoveView(ActiveView);
            }
        }
    }

    public ModelEditorView AddView()
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

        ModelEditorView view = new(Editor, Project, index);

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

    public bool RemoveView(ModelEditorView view)
    {
        if (!Views.Contains(view))
        {
            return false;
        }

        Views[view.ViewIndex] = null;

        if (view == ActiveView || ActiveView == null)
        {
            ActiveView = Views.FindLast(e => e != null);
        }

        return true;
    }

    public int CountViews()
    {
        return Views.Where(e => e != null).Count();
    }

    public void HandleViews()
    {
        var activeView = ActiveView;

        foreach (var view in Views)
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

            if (ImGui.Begin($@"{displayTitle}###ModelEditorView##{view.ViewIndex}", UIHelper.GetDisplayViewWindowFlags()))
            {
                if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                {
                    ActiveView = view;
                }

                // Don't let the user close if their is only 1 view
                if (CountViews() > 1)
                {
                    if (ImGui.BeginPopupContextItem())
                    {
                        if (ImGui.MenuItem("Close View"))
                        {
                            ViewToClose = view;
                        }

                        ImGui.EndMenu();
                    }
                }
            }

            var dsid = ImGui.GetID($"DockSpace_ModelEdit_View{view.ViewIndex}");
            ImGui.DockSpace(dsid, new Vector2(0, 0));

            view.Display(Editor.CommandQueue.DoFocus && view == activeView, view == activeView);

            ImGui.End();
        }
    }
}
