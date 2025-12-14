using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Formats.JSON;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid.MetalBindings;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// Select the individual FLVER to load
/// </summary>
public class ModelSelectView
{
    public ModelEditorScreen Editor;
    public ProjectEntry Project;

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


            ImGui.EndMenuBar();
        }
    }

    public void DisplayModelSelectionList()
    {
        if (Editor.Selection.SelectedModelContainerWrapper == null)
            return;

        var container = Editor.Selection.SelectedModelContainerWrapper;

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

            if (ImGui.Selectable($"{displayedName}##modelSelectListEntry{displayedName}", selected, ImGuiSelectableFlags.AllowDoubleClick))
            {
                if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                {
                    if(Editor.Selection.SelectedModelWrapper != null)
                    {
                        Editor.Selection.SelectedModelWrapper.Unload();
                    }

                    Editor.Selection.SelectedModelWrapper = wrapper;
                    wrapper.Load();
                }
            }

            // Context Menu
            DisplayContextMenu(wrapper);
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
