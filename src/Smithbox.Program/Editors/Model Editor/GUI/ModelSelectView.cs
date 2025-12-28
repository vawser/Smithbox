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
public class ModelSelectView
{
    public ModelEditorScreen Editor;
    public ProjectEntry Project;

    public bool ApplyAutoSelectPass = false;

    public ModelSelectView(ModelEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void OnGui()
    {
        var scale = DPI.UIScale();

        if (CFG.Current.Interface_ModelEditor_ModelSelectList)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
            ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

            if (ImGui.Begin($@"Model List##modelSelectionList", ImGuiWindowFlags.MenuBar))
            {
                Editor.FocusManager.SwitchModelEditorContext(ModelEditorContext.ModelSelectList);

                DisplayMenubar();

                ImGui.BeginChild($"modelSelectListSection");

                DisplayModelSelectionList();

                ImGui.EndChild();
            }

            ImGui.End();
            ImGui.PopStyleColor();
        }
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
        if (Editor.Selection.SelectedModelContainerWrapper == null)
            return;

        var container = Editor.Selection.SelectedModelContainerWrapper;

        int i = 0;
        foreach(var wrapper in container.Models)
        {
            bool selected = false;
            if (Editor.Selection.SelectedModelWrapper != null)
            {
                if (Editor.Selection.SelectedModelWrapper.Name == wrapper.Name)
                {
                    selected = true;
                }
            }

            var displayedName = $"{wrapper.Name}";

            if (ImGui.Selectable($"{displayedName}##modelSelectListEntry{i}", selected, ImGuiSelectableFlags.AllowDoubleClick))
            {
                if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                {
                    if(Editor.Selection.SelectedModelWrapper != null)
                    {
                        Editor.Selection.SelectedModelWrapper.Unload();
                    }

                    Editor.Selection.SelectedModelWrapper = wrapper;

                    Editor.EditorActionManager.Clear();

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
                    if (Editor.Selection.SelectedModelWrapper != null)
                    {
                        Editor.Selection.SelectedModelWrapper.Unload();
                    }

                    Editor.Selection.SelectedModelWrapper = wrapper;

                    Editor.EditorActionManager.Clear();

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
                if (Editor.Selection.SelectedModelWrapper != null)
                {
                    Editor.Selection.SelectedModelWrapper.Unload();
                }

                Editor.Selection.SelectedModelWrapper = wrapper;

                Editor.EditorActionManager.Clear();

                wrapper.Load();
            }

            if (ImGui.Selectable("Unload"))
            {
                Editor.Selection.SelectedModelWrapper = null;

                wrapper.Unload();
            }

            ImGui.EndPopup();
        }
    }
}
