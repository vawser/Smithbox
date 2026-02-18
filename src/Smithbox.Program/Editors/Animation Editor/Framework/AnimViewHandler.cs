using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.ModelEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.AnimEditor;

public class AnimViewHandler
{
    public AnimEditorScreen Editor;
    public ProjectEntry Project;

    public List<AnimEditorView> Views = new();
    public AnimEditorView ActiveView;

    public AnimEditorView ViewToClose = null;

    public AnimViewHandler(AnimEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;

        var initialView = new AnimEditorView(Editor, Project, 0, AnimViewType.BEH);

        Views = [initialView];
        ActiveView = initialView;
    }

    public void DisplayMenu()
    {
        if (ImGui.BeginMenu("New Editor View"))
        {
            //if (ImGui.MenuItem("New Time Act View"))
            //{
            //    AddView(AnimViewType.TAE);
            //}

            if (ImGui.MenuItem("New Behavior View"))
            {
                AddView(AnimViewType.BEH);
            }

            ImGui.EndMenu();
        }

        if (ImGui.MenuItem("Close Current Editor View"))
        {
            if (CountViews() > 1)
            {
                RemoveView(ActiveView);
            }
        }
    }

    public AnimEditorView AddView(AnimViewType type)
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

        AnimEditorView view = new(Editor, Project, index, type);

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

    public bool RemoveView(AnimEditorView view)
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
            if (view.EditorType == AnimViewType.TAE)
            {
                displayTitle = "Active Time Act View";
            }
            if (view.EditorType == AnimViewType.BEH)
            {
                displayTitle = "Active Behavior View";
            }

            if (view != activeView)
            {
                displayTitle = "Inactive View";
                if (view.EditorType == AnimViewType.TAE)
                {
                    displayTitle = "Inactive Time Act View";
                }
                if (view.EditorType == AnimViewType.BEH)
                {
                    displayTitle = "Inactive Behavior View";
                }
            }

            displayTitle = $"{displayTitle} [{view.ViewIndex}]";

            ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f), ImGuiCond.FirstUseEver);

            if (CountViews() == 1)
            {
                displayTitle = "Active View";
            }

            if (ImGui.Begin($@"{displayTitle}###AnimEditorView##{view.ViewIndex}", UIHelper.GetDisplayViewWindowFlags()))
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

            var dsid = ImGui.GetID($"DockSpace_AnimEdit_View{view.ViewIndex}");
            ImGui.DockSpace(dsid, new Vector2(0, 0));

            view.Display(Editor.CommandQueue.DoFocus && view == activeView, view == activeView);

            ImGui.End();
        }
    }
}

public enum AnimViewType
{
    TAE,
    BEH
}