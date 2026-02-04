using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.TextureViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

public class TextViewHandler
{
    public TextEditorScreen Editor;
    public ProjectEntry Project;

    public List<TextEditorView> TexViews = new();
    public TextEditorView ActiveView;

    public TextEditorView ViewToClose = null;

    public TextViewHandler(TextEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;

        var initialView = new TextEditorView(Editor, Project, 0);

        TexViews = [initialView];
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

    public TextEditorView AddView()
    {
        var index = 0;
        while (index < TexViews.Count)
        {
            if (TexViews[index] == null)
            {
                break;
            }

            index++;
        }

        TextEditorView view = new(Editor, Project, index);

        if (index < TexViews.Count)
        {
            TexViews[index] = view;
        }
        else
        {
            TexViews.Add(view);
        }

        ActiveView = view;

        return view;
    }

    public bool RemoveView(TextEditorView view)
    {
        if (!TexViews.Contains(view))
        {
            return false;
        }

        TexViews[view.ViewIndex] = null;

        if (view == ActiveView || ActiveView == null)
        {
            ActiveView = TexViews.FindLast(e => e != null);
        }

        return true;
    }

    public int CountViews()
    {
        return TexViews.Where(e => e != null).Count();
    }

    public void HandleViews()
    {
        var activeView = ActiveView;

        foreach (var view in TexViews)
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

            if (ImGui.Begin($@"{displayTitle}###TextEditorView##{view.ViewIndex}", UIHelper.GetInnerWindowFlags()))
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
