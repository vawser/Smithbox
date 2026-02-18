using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.ModelEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.AnimEditor;

public class BehaviorFileList
{
    public BehaviorView View;
    public ProjectEntry Project;

    public bool ApplyAutoSelectPass = false;

    public BehaviorFileList(BehaviorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Display(float width, float height)
    {
        UIHelper.SimpleHeader("Files", "");

        DisplayMenubar();

        ImGui.BeginChild($"FileSection", new System.Numerics.Vector2(width, height), ImGuiChildFlags.Borders);

        DisplaySelectionList();

        ImGui.EndChild();
    }

    public void DisplayMenubar()
    {
        if (ImGui.BeginMenuBar())
        {

            ImGui.EndMenuBar();
        }
    }

    public void DisplaySelectionList()
    {
        if (View.Selection.SelectedContainer == null)
            return;

        var container = View.Selection.SelectedContainer;

        int i = 0;

        foreach (var wrapper in container.Entries)
        {
            bool selected = false;

            if (View.Selection.SelectedFile != null)
            {
                if (View.Selection.SelectedFile.Name == wrapper.Name)
                {
                    selected = true;
                }
            }

            var displayedName = $"{wrapper.Name}";

            if (ImGui.Selectable($"{displayedName}##behaviorSelectListEntry{i}", selected, ImGuiSelectableFlags.AllowDoubleClick))
            {
                if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                {
                    View.InvalidateContent();

                    View.Selection.SelectedFile = wrapper;

                    View.View.ViewportActionManager.Clear();
                    View.View.ActionManager.Clear();

                    wrapper.Load();
                }
            }

            // Context Menu
            DisplayContextMenu(wrapper);

            i++;
        }

        if (ApplyAutoSelectPass)
        {
            ApplyAutoSelectPass = false;

            if (container.Entries.Count == 1)
            {
                foreach (var wrapper in container.Entries)
                {
                    View.Selection.SelectedFile = wrapper;

                    View.View.ViewportActionManager.Clear();
                    View.View.ActionManager.Clear();

                    wrapper.Load();
                }
            }
        }
    }

    private void DisplayContextMenu(BehaviorWrapper wrapper)
    {
        if (ImGui.BeginPopupContextItem($@"modelSelectListEntryContext_{wrapper.Name}"))
        {
            if (ImGui.Selectable("Load"))
            {
                View.InvalidateContent();

                View.Selection.SelectedFile = wrapper;

                View.View.ViewportActionManager.Clear();
                View.View.ActionManager.Clear();

                wrapper.Load();
            }

            ImGui.EndPopup();
        }
    }
}
