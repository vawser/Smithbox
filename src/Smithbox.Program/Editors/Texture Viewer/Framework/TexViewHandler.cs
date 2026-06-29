using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.ParamEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextureViewer;

public class TexViewHandler
{
    public TextureViewerScreen Editor;
    public ProjectEntry Project;

    public List<TexEditorView> TexViews = new();
    public TexEditorView ActiveView;

    public bool AddNewView = false;
    public TexEditorView ViewToClose = null;

    public TexViewHandler(TextureViewerScreen editor, ProjectEntry project)
    {
        Editor = editor; 
        Project = project;

        var initialView = new TexEditorView(Editor, Project, 0);

        TexViews = [initialView];
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

    public TexEditorView AddView()
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

        TexEditorView view = new(Editor, Project, index);

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

    public bool RemoveView(TexEditorView view)
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

    public void HandleViews(uint editorDockspaceId)
    {
        var activeView = ActiveView;

        foreach (var view in TexViews)
        {
            if (view == null)
            {
                continue;
            }

            var name = view.Selection.SelectedFileEntry != null ? view.Selection.SelectedFileEntry.Filename : null;

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
            ImGui.SetNextWindowClass(ref UIHelper.DockGroup_TextureViewer);
            if (ImGui.Begin($@"{displayTitle}###TextureEditorView##{view.ViewIndex}", UIHelper.GetInnerWindowFlags()))
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

                    ImGui.EndMenu();
                }
            }

            var dsid = ImGui.GetID($"DockSpace_TextureViewer_View{view.ViewIndex}");
            ImGui.DockSpace(dsid, new Vector2(0, 0), ref UIHelper.DockGroup_TextureViewerView);

            view.Display(dsid, view.ViewIndex, Editor.CommandQueue.DoFocus && view == activeView, view == activeView);

            ImGui.End();
        }

        if(AddNewView)
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
