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

    public List<ParamView> ParamViews;
    public ParamView ActiveView;

    public ParamView ViewToClose = null;

    public ParamViewHandler(ParamEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;

        var initialView = new ParamView(Editor, Project, 0);

        ParamViews = [initialView];
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

    public ParamView AddView()
    {
        var index = 0;
        while (index < ParamViews.Count)
        {
            if (ParamViews[index] == null)
            {
                break;
            }

            index++;
        }

        ParamView view = new(Editor, Project, index);

        if (index < ParamViews.Count)
        {
            ParamViews[index] = view;
        }
        else
        {
            ParamViews.Add(view);
        }

        ActiveView = view;

        return view;
    }

    public bool RemoveView(ParamView view)
    {
        if (!ParamViews.Contains(view))
        {
            return false;
        }

        ParamViews[view.ViewIndex] = null;

        if (view == ActiveView || ActiveView == null)
        {
            ActiveView = ParamViews.FindLast(e => e != null);
        }

        return true;
    }

    public int CountViews()
    {
        return ParamViews.Where(e => e != null).Count();
    }

    public void HandleViews()
    {
        var activeView = ActiveView;

        foreach (var view in ParamViews)
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
                displayTitle = "Param Editor";
            }

            if (ImGui.Begin($@"{displayTitle}###ParamEditorView##{view.ViewIndex}", UIHelper.GetInnerWindowFlags(false)))
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
