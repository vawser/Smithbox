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
        GUI.TitleHeader(
            LOC.Get("MODEL_FileList_Header"),
            LOC.Get("MODEL_FileList_Header_TT"));

        EditorFilters.DisplayFramedListFilter("modelEditor_fileList",
            ref FileListFilter, ref ExactFileListFilter);

        ImGui.BeginChild($"FileSection", new Vector2(0, 0), ImGuiChildFlags.Borders);

        DisplayModelSelectionList();

        ImGui.EndChild();
    }

    /// <summary>
    /// Loads the model requested by picking a container in the Source List. This runs every frame, as the File List
    /// itself may not be drawn (such as when it is a hidden tab).
    /// </summary>
    public void ProcessPendingLoad()
    {
        if (!ApplyAutoLoadFirst && !ApplyAutoSelectPass)
            return;

        var autoLoadFirst = ApplyAutoLoadFirst && CFG.Current.ModelEditor_Files_AutoLoadFirstEntry;
        var autoSelect = ApplyAutoSelectPass;

        ApplyAutoLoadFirst = false;
        ApplyAutoSelectPass = false;

        var container = View.Selection.SelectedModelContainerWrapper;

        if (container == null || container.Models.Count == 0)
            return;

        // A container with a single model always loads it; otherwise only if the first is to be loaded automatically
        if (autoLoadFirst || (autoSelect && container.Models.Count == 1))
        {
            View.LoadModel(container.Models.First());
        }
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

            // Mark models with unsaved changes
            if (curWrapper.IsDirty)
            {
                displayedName = $"{displayedName}*";
            }

            // Mark models loaded in another view, as they can't be loaded here as well
            var ownerView = curWrapper.Container?.View;

            if (ownerView != null && ownerView != View)
            {
                displayedName = $"{displayedName} {LOC.Get("MODEL_FileList_Open_In_View", ownerView.ViewIndex)}";
            }

            // Mark models switched away from that keep their undo history in this view
            if (curWrapper.Parked?.View == View)
            {
                displayedName = $"{displayedName} {LOC.Get("MODEL_FileList_History_Kept", curWrapper.Parked.History.Count)}";
            }

            // Loads the same way as the Source List: on a single click, or a double click if that is required there
            var requireDoubleClick = CFG.Current.ModelEditor_ModelSourceList_RequireDoubleClick;

            if (ImGui.Selectable($"{displayedName}##modelSelectListEntry{i}", selected, ImGuiSelectableFlags.AllowDoubleClick))
            {
                var isLoadClick = requireDoubleClick
                    ? ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left)
                    : !selected || curWrapper.Container == null;

                if (isLoadClick)
                {
                    View.LoadModel(curWrapper);
                }
            }

            DisplayContextMenu(curWrapper);
        }
    }

    private void DisplayContextMenu(ModelWrapper wrapper)
    {
        if (ImGui.BeginPopupContextItem($@"modelSelectListEntryContext_{wrapper.Name}"))
        {
            // Load
            if (ImGui.Selectable($"{LOC.Get("MODEL_FileList_Context_Load_Action")}##loadAction"))
            {
                View.LoadModel(wrapper);
            }

            // Unload
            if (ImGui.Selectable($"{LOC.Get("MODEL_FileList_Context_Unload_Action")}##unloadAction"))
            {
                View.UnloadModel(wrapper);
            }

            ImGui.EndPopup();
        }
    }
}
