using Hexa.NET.ImGui;
using System.Numerics;

namespace StudioCore.Editors.HavokEditor;

public class HavokViewHandler
{
    public HavokEditorScreen Editor;
    public ProjectEntry Project;

    public List<HavokEditorView> Views = new();
    public HavokEditorView ActiveView;

    public bool AddNewView = false;
    public HavokEditorView ViewToClose = null;

    public HavokViewHandler(HavokEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;

        var initialView = new HavokEditorView(Editor, Project, 0);

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

    public HavokEditorView AddView()
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

        HavokEditorView view = new(Editor, Project, index);

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

    public bool RemoveView(HavokEditorView view)
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
            ImGui.SetNextWindowClass(ref GUI.DockGroup_HavokEditor);
            if (ImGui.Begin($@"{displayTitle}###HavokEditorView##{view.ViewIndex}", GUI.GetDisplayViewWindowFlags()))
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

            var dsid = ImGui.GetID($"DockSpace_HavokEdit_View{view.ViewIndex}");
            ImGui.DockSpace(dsid, new Vector2(0, 0), ref GUI.DockGroup_HavokEditorView);

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
