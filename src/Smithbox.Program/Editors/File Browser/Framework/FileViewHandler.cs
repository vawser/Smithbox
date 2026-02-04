using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.FileBrowser;

public class FileViewHandler
{
    public FileBrowserScreen Editor;
    public ProjectEntry Project;

    public List<FileEditorView> FileViews = new();
    public FileEditorView ActiveView;

    public FileEditorView ViewToClose = null;

    public FileViewHandler(FileBrowserScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;

        var initialView = new FileEditorView(Editor, Project, 0);

        FileViews = [initialView];
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

    public FileEditorView AddView()
    {
        var index = 0;
        while (index < FileViews.Count)
        {
            if (FileViews[index] == null)
            {
                break;
            }

            index++;
        }

        FileEditorView view = new(Editor, Project, index);

        if (index < FileViews.Count)
        {
            FileViews[index] = view;
        }
        else
        {
            FileViews.Add(view);
        }

        ActiveView = view;

        return view;
    }

    public bool RemoveView(FileEditorView view)
    {
        if (!FileViews.Contains(view))
        {
            return false;
        }

        FileViews[view.ViewIndex] = null;

        if (view == ActiveView || ActiveView == null)
        {
            ActiveView = FileViews.FindLast(e => e != null);
        }

        return true;
    }

    public int CountViews()
    {
        return FileViews.Where(e => e != null).Count();
    }

    public void HandleViews()
    {
        var activeView = ActiveView;

        foreach (var view in FileViews)
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

            if (ImGui.Begin($@"{displayTitle}###FileBrowserView##{view.ViewIndex}", UIHelper.GetInnerWindowFlags()))
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
