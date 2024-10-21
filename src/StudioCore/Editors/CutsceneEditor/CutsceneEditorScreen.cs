using ImGuiNET;
using SoulsFormats;
using StudioCore.Core.Project;
using StudioCore.Editor;
using StudioCore.Editors.CutsceneEditor;
using StudioCore.Editors.CutsceneEditor.Framework;
using StudioCore.Interface;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.CutsceneEditor;

public class CutsceneEditorScreen : EditorScreen
{
    public ActionManager EditorActionManager = new();

    public CutsceneSelectionManager Selection;
    public CutscenePropertyDecorator Decorator;

    public CutsceneFilters Filters;

    public CutsceneActionHandler ActionHandler;
    public CutsceneActionMenubar ActionMenubar;
    public CutsceneToolMenubar ToolMenubar;
    public CutsceneToolView ToolView;

    public CutsceneFileView CutsceneFileList;
    public CutsceneListView CutsceneList;
    public CutscenePropertyView CutsceneProperties;

    public CutListView CutList;
    public CutPropertyView CutProperties;

    public TimelineListView Timelines;
    public TimelineCustomDataListView TimelineCustomDataList;
    public TimelineCustomDataPropertyView TimelineCustomDataProperties;
    public TimelineSequenceListView TimelineSequenceList;
    public TimelineSequencePropertyView TimelineSequenceProperties;
    public TimelineSequencePointListView TimelineSequencePointList;
    public TimelineSequencePointPropertyView TimelineSequencePointProperties;

    public DispositionListView DispositionList;
    public DispositionPropertyView DispositionProperties;
    public DispositionTransformListView DispositionTransformList;
    public DispositionTransformPropertyView DispositionTransformProperties;
    public DispositionSequenceListView DispositionSequenceList;
    public DispositionSequencePropertyView DispositionSequenceProperties;
    public DispositionSequencePointListView DispositionSequencePointList;
    public DispositionSequencePointPropertyView DispositionSequencePointProperties;

    public ResourceListView ResourceList;

    public CutsceneEditorScreen(Sdl2Window window, GraphicsDevice device)
    {
        Selection = new CutsceneSelectionManager(this);
        Decorator = new CutscenePropertyDecorator(this);
        Filters = new CutsceneFilters(this);

        ActionHandler = new CutsceneActionHandler(this);
        ActionMenubar = new CutsceneActionMenubar(this);
        ToolMenubar = new CutsceneToolMenubar(this);
        ToolView = new CutsceneToolView(this);

        CutsceneFileList = new CutsceneFileView(this);
        CutsceneList = new CutsceneListView(this);
        CutsceneProperties = new CutscenePropertyView(this);

        CutList = new CutListView(this);
        CutProperties = new CutPropertyView(this);

        Timelines = new TimelineListView(this);
        TimelineCustomDataList = new TimelineCustomDataListView(this);
        TimelineCustomDataProperties = new TimelineCustomDataPropertyView(this);
        TimelineSequenceList = new TimelineSequenceListView(this);
        TimelineSequenceProperties = new TimelineSequencePropertyView(this);
        TimelineSequencePointList = new TimelineSequencePointListView(this);
        TimelineSequencePointProperties = new TimelineSequencePointPropertyView(this);

        DispositionList = new DispositionListView(this);
        DispositionProperties = new DispositionPropertyView(this);
        DispositionTransformList = new DispositionTransformListView(this);
        DispositionTransformProperties = new DispositionTransformPropertyView(this);
        DispositionSequenceList = new DispositionSequenceListView(this);
        DispositionSequenceProperties = new DispositionSequencePropertyView(this);
        DispositionSequencePointList = new DispositionSequencePointListView(this);
        DispositionSequencePointProperties = new DispositionSequencePointPropertyView(this);

        ResourceList = new ResourceListView(this);
    }

    public string EditorName => "Cutscene Editor##CutsceneEditor";
    public string CommandEndpoint => "cutscene";
    public string SaveType => "Cutscene";

    public void EditDropdown()
    {

    }

    public void ViewDropdown()
    {

    }

    public void EditorUniqueDropdowns()
    {

    }

    public void OnGUI(string[] initcmd)
    {
        var scale = DPI.GetUIScale();

        // Docking setup
        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(4, 4) * scale);
        Vector2 wins = ImGui.GetWindowSize();
        Vector2 winp = ImGui.GetWindowPos();
        winp.Y += 20.0f * scale;
        wins.Y -= 20.0f * scale;
        ImGui.SetNextWindowPos(winp);
        ImGui.SetNextWindowSize(wins);

        var dsid = ImGui.GetID("DockSpace_CutsceneEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None);

        if (Smithbox.ProjectType is ProjectType.DS1 or ProjectType.DS1R or ProjectType.BB or ProjectType.DS2S or ProjectType.DS2 or ProjectType.AC4 or ProjectType.ACFA)
        {
            ImGui.Begin("Editor##InvalidCutsceneEditor");

            ImGui.Text($"This editor does not support {Smithbox.ProjectType}.");

            ImGui.End();
        }
        else
        {
            if (!CutsceneBank.IsLoaded)
            {
                if (!CFG.Current.AutoLoadBank_Cutscene)
                {
                    if (ImGui.Button("Load Cutscene Editor"))
                    {
                        CutsceneBank.LoadCutscenes();
                    }
                }
            }

            if (CutsceneBank.IsLoaded)
            {
                CutsceneFileList.Display();
                CutsceneList.Display();
                CutsceneProperties.Display();

                CutList.Display();
                CutProperties.Display();

                Timelines.Display();
                TimelineCustomDataList.Display();
                TimelineCustomDataProperties.Display();
                TimelineSequenceList.Display();
                TimelineSequenceProperties.Display();
                TimelineSequencePointList.Display();
                TimelineSequencePointProperties.Display();

                DispositionList.Display();
                DispositionProperties.Display();
                DispositionTransformList.Display();
                DispositionTransformProperties.Display();
                DispositionSequenceList.Display();
                DispositionSequenceProperties.Display();
                DispositionSequencePointList.Display();
                DispositionSequencePointProperties.Display();

                ResourceList.Display();
            }
        }

        ImGui.PopStyleVar();
        ImGui.PopStyleColor(1);
    }

    public void OnProjectChanged()
    {
        CutsceneFileList.OnProjectChanged();
        CutsceneList.OnProjectChanged();
        CutsceneProperties.OnProjectChanged();

        CutList.OnProjectChanged();
        CutProperties.OnProjectChanged();

        Timelines.OnProjectChanged();
        TimelineCustomDataList.OnProjectChanged();
        TimelineCustomDataProperties.OnProjectChanged();
        TimelineSequenceList.OnProjectChanged();
        TimelineSequenceProperties.OnProjectChanged();
        TimelineSequencePointList.OnProjectChanged();
        TimelineSequencePointProperties.OnProjectChanged();

        DispositionList.OnProjectChanged();
        DispositionProperties.OnProjectChanged();
        DispositionTransformList.OnProjectChanged();
        DispositionTransformProperties.OnProjectChanged();
        DispositionSequenceList.OnProjectChanged();
        DispositionSequenceProperties.OnProjectChanged();
        DispositionSequencePointList.OnProjectChanged();
        DispositionSequencePointProperties.OnProjectChanged();

        ResourceList.OnProjectChanged();

        if (CFG.Current.AutoLoadBank_Cutscene)
            CutsceneBank.LoadCutscenes();

        ResetActionManager();
    }

    public void Save()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if (CutsceneBank.IsLoaded)
            CutsceneBank.SaveCutscene(Selection._selectedFileInfo, Selection._selectedBinder);
    }

    public void SaveAll()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if (CutsceneBank.IsLoaded)
            CutsceneBank.SaveCutscenes();
    }

    private void ResetActionManager()
    {
        EditorActionManager.Clear();
    }
}
