using Hexa.NET.ImGui;
using System.Numerics;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using StudioCore.Utilities;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// Select the individual FLVER to load
/// </summary>
public class ModelSelectionList
{
    public ModelEditorView View;
    public ProjectEntry Project;

    public bool ApplyAutoSelectPass = false;

    public ModelSelectionList(ModelEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Display(float width, float height)
    {
        UIHelper.SimpleHeader("Files", "");

        DisplayMenubar();

        ImGui.BeginChild($"FileSection", new System.Numerics.Vector2(width, height), ImGuiChildFlags.Borders);

        DisplayModelSelectionList();

        ImGui.EndChild();
    }

    public void DisplayMenubar()
    {
        if (ImGui.BeginMenuBar())
        {
            if (ImGui.BeginMenu("Options"))
            {
                if (ImGui.MenuItem("Auto-Select Single Entries"))
                {
                    CFG.Current.ModelEditor_AutoLoadSingles = !CFG.Current.ModelEditor_AutoLoadSingles;
                }
                UIHelper.ShowActiveStatus(CFG.Current.ModelEditor_AutoLoadSingles);
                UIHelper.Tooltip($"If enabled, when the selection list is only one entry, it will be automatically selected and loaded.");

                ImGui.EndMenu();
            }


            ImGui.EndMenuBar();
        }
    }

    public void DisplayModelSelectionList()
    {
        if (View.Selection.SelectedModelContainerWrapper == null)
            return;

        var container = View.Selection.SelectedModelContainerWrapper;

        int i = 0;
        foreach(var wrapper in container.Models)
        {
            bool selected = false;
            if (View.Selection.SelectedModelWrapper != null)
            {
                if (View.Selection.SelectedModelWrapper.Name == wrapper.Name)
                {
                    selected = true;
                }
            }

            var displayedName = $"{wrapper.Name}";

            if (ImGui.Selectable($"{displayedName}##modelSelectListEntry{i}", selected, ImGuiSelectableFlags.AllowDoubleClick))
            {
                if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                {
                    if(View.Selection.SelectedModelWrapper != null)
                    {
                        View.Selection.SelectedModelWrapper.Unload();
                    }

                    View.Selection.SelectedModelWrapper = wrapper;

                    View.ViewportActionManager.Clear();
                    View.ActionManager.Clear();

                    wrapper.Load();
                }
            }

            // Context Menu
            DisplayContextMenu(wrapper);

            i++;
        }

        if(ApplyAutoSelectPass)
        {
            ApplyAutoSelectPass = false;

            if (container.Models.Count == 1)
            {
                foreach (var wrapper in container.Models)
                {
                    if (View.Selection.SelectedModelWrapper != null)
                    {
                        View.Selection.SelectedModelWrapper.Unload();
                    }

                    View.Selection.SelectedModelWrapper = wrapper;

                    View.ViewportActionManager.Clear();
                    View.ActionManager.Clear();

                    wrapper.Load();
                }
            }
        }
    }

    private void DisplayContextMenu(ModelWrapper wrapper)
    {
        if (ImGui.BeginPopupContextItem($@"modelSelectListEntryContext_{wrapper.Name}"))
        {
            if (ImGui.Selectable("Load"))
            {
                if (View.Selection.SelectedModelWrapper != null)
                {
                    View.Selection.SelectedModelWrapper.Unload();
                }

                View.Selection.SelectedModelWrapper = wrapper;

                View.ViewportActionManager.Clear();
                View.ActionManager.Clear();

                wrapper.Load();
            }

            if (ImGui.Selectable("Unload"))
            {
                View.Selection.SelectedModelWrapper = null;

                wrapper.Unload();
            }

            ImGui.EndPopup();
        }
    }
}
