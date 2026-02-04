using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class ParamViewHandler
{
    public ParamEditorScreen Editor;
    public ProjectEntry Project;

    public List<ParamEditorView> ParamEditorViews;
    public ParamEditorView ActiveView;

    public ParamEditorView ViewToClose = null;

    public ParamViewHandler(ParamEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;

        var initialView = new ParamEditorView(Editor, Project, 0);

        ParamEditorViews = [initialView];
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

    public ParamEditorView AddView()
    {
        var index = 0;
        while (index < ParamEditorViews.Count)
        {
            if (ParamEditorViews[index] == null)
            {
                break;
            }

            index++;
        }

        ParamEditorView view = new(Editor, Project, index);

        if (index < ParamEditorViews.Count)
        {
            ParamEditorViews[index] = view;
        }
        else
        {
            ParamEditorViews.Add(view);
        }

        ActiveView = view;

        return view;
    }

    public bool RemoveView(ParamEditorView view)
    {
        if (!ParamEditorViews.Contains(view))
        {
            return false;
        }

        ParamEditorViews[view.ViewIndex] = null;

        if (view == ActiveView || ActiveView == null)
        {
            ActiveView = ParamEditorViews.FindLast(e => e != null);
        }

        return true;
    }

    public int CountViews()
    {
        return ParamEditorViews.Where(e => e != null).Count();
    }

    public void HandleViews()
    {
        var activeView = ActiveView;

        foreach (var view in ParamEditorViews)
        {
            if (view == null)
            {
                continue;
            }

            var name = view.Selection.GetActiveRow() != null ? view.Selection.GetActiveRow().Name : null;

            var displayTitle = "Active View";

            if (view != activeView)
            {
                displayTitle = "Inactive View";
            }

            var rowName = Utils.ImGuiEscape(name, "null");

            if(view.Selection.ActiveParamExists())
            {
                rowName = $"{view.Selection.GetActiveParam()}";
            }

            displayTitle = $"{displayTitle} [{rowName}]";

            ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f), ImGuiCond.FirstUseEver);

            if (CountViews() == 1)
            {
                displayTitle = "Active View";
            }

            if (ImGui.Begin($@"{displayTitle}###ParamEditorView##{view.ViewIndex}", UIHelper.GetInnerWindowFlags()))
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

            view.Display(Editor.CommandQueue.DoFocus && view == activeView, view == activeView);

            ImGui.End();
        }

        if (ViewToClose != null)
        {
            RemoveView(ViewToClose);

            ViewToClose = null;
        }
    }
}
