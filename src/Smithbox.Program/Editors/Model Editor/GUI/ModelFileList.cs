using Hexa.NET.ImGui;
using System.Numerics;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System.Linq;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// Select the individual FLVER to load
/// </summary>
public class ModelFileList
{
    public ModelEditorView View;
    public ProjectEntry Project;

    public bool ApplyAutoSelectPass = false;
    public bool ApplyAutoLoadFirst = false;

    private string FileListFilter = "";
    private bool ExactFileListFilter = false;

    public ModelFileList(ModelEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Display(float width, float height)
    {
        UIHelper.SimpleHeader("Files", "");

        EditorFilters.DisplayFramedListFilter("modelEditor_fileList",
            ref FileListFilter, ref ExactFileListFilter);

        ImGui.BeginChild($"FileSection", new Vector2(0, 0), ImGuiChildFlags.Borders);

        DisplayModelSelectionList();

        ImGui.EndChild();
    }

    public void DisplayModelSelectionList()
    {
        if (View.Selection.SelectedModelContainerWrapper == null)
            return;

        var container = View.Selection.SelectedModelContainerWrapper;

        for(int i = 0; i < container.Models.Count; i++)
        {
            var curWrapper = container.Models[i];

            bool selected = false;

            if (View.Selection.SelectedModelWrapper != null)
            {
                if (View.Selection.SelectedModelWrapper.Name == curWrapper.Name)
                {
                    selected = true;
                }
            }

            var displayedName = $"{curWrapper.Name}";

            var isMatch = EditorFilters.IsMatch(
                FileListFilter, displayedName, ExactFileListFilter);

            if (!isMatch)
                continue;

            if (ImGui.Selectable($"{displayedName}##modelSelectListEntry{i}", selected, ImGuiSelectableFlags.AllowDoubleClick))
            {
                if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                {
                    if(View.Selection.SelectedModelWrapper != null)
                    {
                        View.Selection.SelectedModelWrapper.Unload();
                    }

                    View.Selection.SelectedModelWrapper = curWrapper;

                    View.ViewportActionManager.Clear();
                    View.ActionManager.Clear();

                    curWrapper.Load();
                }
            }

            DisplayContextMenu(curWrapper);
        }

        if (ApplyAutoLoadFirst)
        {
            ApplyAutoLoadFirst = false;

            if (CFG.Current.ModelEditor_Files_AutoLoadFirstEntry)
            {
                var first = container.Models.First();

                if (View.Selection.SelectedModelWrapper != null)
                {
                    View.Selection.SelectedModelWrapper.Unload();
                }

                View.Selection.SelectedModelWrapper = first;

                View.ViewportActionManager.Clear();
                View.ActionManager.Clear();

                first.Load();
            }
        }

        if (ApplyAutoSelectPass)
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
