using Hexa.NET.ImGui;
using System.Numerics;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// The EMEVD Editor's views: each has its own selection, windows and undo history, over the one shared bank
/// </summary>
public class EmevdViewHandler
{
    public EmevdEditorScreen Editor;
    public ProjectEntry Project;

    public List<EmevdEditorView> Views = new();
    public EmevdEditorView ActiveView;

    public bool AddNewView = false;
    public EmevdEditorView ViewToClose = null;

    public EmevdViewHandler(EmevdEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;

        var initialView = new EmevdEditorView(Editor, Project, 0);

        Views = [initialView];
        ActiveView = initialView;
    }

    /// <summary>The open views (closed views leave a null in <see cref="Views"/> so their index can be reused)</summary>
    public IEnumerable<EmevdEditorView> OpenViews => Views.Where(e => e != null);

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

    public EmevdEditorView AddView()
    {
        var index = 0;
        while (index < Views.Count)
        {
            if (Views[index] == null)
                break;

            index++;
        }

        EmevdEditorView view = new(Editor, Project, index);

        if (index < Views.Count)
            Views[index] = view;
        else
            Views.Add(view);

        ActiveView = view;

        return view;
    }

    public bool RemoveView(EmevdEditorView view)
    {
        if (!Views.Contains(view))
            return false;

        Views[view.ViewIndex] = null;

        if (view == ActiveView || ActiveView == null)
            ActiveView = Views.FindLast(e => e != null);

        return true;
    }

    public int CountViews()
    {
        return Views.Count(e => e != null);
    }

    public void HandleViews(uint editorDockspaceId)
    {
        var activeView = ActiveView;

        foreach (var view in Views)
        {
            if (view == null)
                continue;

            var displayTitle = LOC.Get("EDITOR_Active_View");

            if (view != activeView)
                displayTitle = LOC.Get("EDITOR_Inactive_View");

            displayTitle = $"{displayTitle} [{view.ViewIndex}]";

            ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * DPI.UIScale(), ImGuiCond.FirstUseEver);

            if (CountViews() == 1)
                displayTitle = LOC.Get("EDITOR_Active_View");

            ImGui.SetNextWindowDockID(editorDockspaceId, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowClass(ref GUI.DockGroup_EmevdEditor);
            if (ImGui.Begin($@"{displayTitle}###EmevdEditorView##{view.ViewIndex}", GUI.GetInnerWindowFlags()))
            {
                if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                    ActiveView = view;

                if (ImGui.BeginPopupContextItem())
                {
                    if (ImGui.MenuItem($"{LOC.Get("EDITOR_Add_View")}##addView"))
                        AddNewView = true;

                    // Don't let the user close the only view
                    if (CountViews() > 1)
                    {
                        if (ImGui.MenuItem($"{LOC.Get("EDITOR_Close_View")}##closeView"))
                            ViewToClose = view;
                    }

                    ImGui.EndPopup();
                }
            }

            var dsid = ImGui.GetID($"DockSpace_EmevdEditor_View{view.ViewIndex}");
            ImGui.DockSpace(dsid, new Vector2(0, 0), ref GUI.DockGroup_EmevdEditorView);

            view.Display(dsid);

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
