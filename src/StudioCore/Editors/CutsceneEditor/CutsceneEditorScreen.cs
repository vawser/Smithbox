using DotNext.Collections.Generic;
using ImGuiNET;
using SoulsFormats;
using StudioCore.Editor;
using StudioCore.Editors.CutsceneEditor;
using StudioCore.UserProject;
using System.Collections.Generic;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using static StudioCore.Editors.CutsceneEditor.CutsceneBank;

namespace StudioCore.CutsceneEditor;

public class CutsceneEditorScreen : EditorScreen
{
    private readonly PropertyEditor _propEditor;
    private ProjectSettings _projectSettings;

    public ActionManager EditorActionManager = new();

    private CutsceneFileInfo _selectedFileInfo;
    private IBinder _selectedBinder;
    private string _selectedBinderKey;

    private MQB _selectedCutscene;
    private int _selectedCutsceneKey;

    private MQB.Resource _selectedResource;
    private int _selectedResourceKey;

    private MQB.Cut _selectedCut;
    private int _selectedCutKey;

    private MQB.Timeline _selectedTimeline;
    private int _selectedTimelineKey;

    private MQB.Disposition _selectedDisposition;
    private int _selectedDispositionKey;

    private MQB.CustomData _selectedTimelineCustomData;
    private int _selectedTimelineCustomDataKey;

    private MQB.CustomData.Sequence _selectedTimelineSequence;
    private int _selectedTimelineSequenceKey;

    private MQB.CustomData.Sequence.Point _selectedTimelineSequencePoint;
    private int _selectedTimelineSequencePointKey;

    private MQB.CustomData _selectedDispositionCustomData;
    private int _selectedDispositionCustomDataKey;

    private MQB.Transform _selectedDispositionTransform;
    private int _selectedDispositionTransformKey;

    private MQB.CustomData.Sequence _selectedDispositionSequence;
    private int _selectedDispositionSequenceKey;

    private MQB.CustomData.Sequence.Point _selectedDispositionSequencePoint;
    private int _selectedDispositionSequencePointKey;

    public CutsceneEditorScreen(Sdl2Window window, GraphicsDevice device)
    {
        _propEditor = new PropertyEditor(EditorActionManager);
    }

    public string EditorName => "Cutscene Editor##CutsceneEditor";
    public string CommandEndpoint => "cutscene";
    public string SaveType => "Cutscene";

    public void Init()
    {

    }
    public void DrawEditorMenu()
    {
    }

    public void OnGUI(string[] initcmd)
    {
        var scale = Smithbox.GetUIScale();

        // Docking setup
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(4, 4) * scale);
        Vector2 wins = ImGui.GetWindowSize();
        Vector2 winp = ImGui.GetWindowPos();
        winp.Y += 20.0f * scale;
        wins.Y -= 20.0f * scale;
        ImGui.SetNextWindowPos(winp);
        ImGui.SetNextWindowSize(wins);

        if (Project.Type is ProjectType.DS1 or ProjectType.DS1R or ProjectType.BB or ProjectType.DS2S)
        {
            ImGui.Text($"This editor does not support {Project.Type}.");
            ImGui.PopStyleVar();
            return;
        }
        else if (_projectSettings == null)
        {
            ImGui.Text("No project loaded. File -> New Project");
        }

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

        var dsid = ImGui.GetID("DockSpace_CutsceneEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None);

        if (CutsceneBank.IsLoaded)
        {
            // Cutscene
            CutsceneFileView();
            CutsceneListView();
            CutscenePropertiesView();

            CutListView();
            CutPropertiesView();

            TimelineListView();
            TimelineCustomDataListView();
            TimelineCustomDataPropertiesView();
            TimelineCustomDataSequencesView();
            TimelineCustomDataSequencePropertiesView();
            TimelineSequencePointsListView();
            TimelineSequencePointPropertiesView();

            DispositionListView();
            DispositionPropertiesView();
            DispositionCustomDataListView();
            DispositionTransformListView();
            DispositionTransformPropertiesView();
            DispositionCustomDataPropertiesView();
            DispositionCustomDataSequencesView();
            DispositionCustomDataSequencePropertiesView();
            DispositionSequencePointsListView();
            DispositionSequencePointPropertiesView();

            ResourceListView();
        }

        ImGui.PopStyleVar();
    }

    public void CutsceneFileView()
    {
        // File List
        ImGui.Begin("Files##CutsceneFileList");

        ImGui.Text($"File");
        ImGui.Separator();

        foreach (var (info, binder) in CutsceneBank.FileBank)
        {
            if (ImGui.Selectable($@"{info.Name}", info.Name == _selectedBinderKey))
            {
                _selectedCutsceneKey = -1;
                _selectedCutKey = -1;
                _selectedResourceKey = -1;
                _selectedDispositionKey = -1;
                _selectedDispositionTransformKey = -1;
                _selectedDispositionCustomDataKey = -1;
                _selectedTimelineCustomDataKey = -1;

                _selectedBinderKey = info.Name;
                _selectedFileInfo = info;
                _selectedBinder = binder;
            }
        }

        ImGui.End();
    }

    public void CutsceneListView()
    {
        ImGui.Begin("Cutscenes##CutsceneList");

        if (_selectedFileInfo != null)
        {
            for (int i = 0; i < _selectedFileInfo.CutsceneFiles.Count; i++)
            {
                MQB entry = _selectedFileInfo.CutsceneFiles[i];

                if (ImGui.Selectable($@"{entry.Name}##{entry.Name}{i}", i == _selectedCutsceneKey))
                {
                    _selectedCutKey = -1;
                    _selectedResourceKey = -1;
                    _selectedDispositionKey = -1;
                    _selectedDispositionTransformKey = -1;
                    _selectedDispositionCustomDataKey = -1;
                    _selectedTimelineCustomDataKey = -1;
                    _selectedDispositionSequenceKey = -1;
                    _selectedDispositionSequencePointKey = -1;

                    _selectedCutsceneKey = i;
                    _selectedCutscene = entry;
                }
            }
        }

        ImGui.End();
    }

    public void CutscenePropertiesView()
    {
        ImGui.Begin("Cutscene Properties##CutsceneProperties");

        if (_selectedCutsceneKey != -1 && _selectedCutscene != null)
        {
            ImGui.Columns(2);

            ImGui.AlignTextToFramePadding();
            ImGui.Text($"Framerate");
            ImGui.Text($"Resource Directory");

            ImGui.NextColumn();

            ImGui.AlignTextToFramePadding();
            ImGui.Text($"{_selectedCutscene.Framerate}");
            ImGui.Text($"{_selectedCutscene.ResourceDirectory}");

            ImGui.Columns(1);
        }

        ImGui.End();
    }

    public void CutListView()
    {
        ImGui.Begin("Cuts##CutList");

        // Cuts
        if (_selectedCutsceneKey != -1 && _selectedCutscene != null)
        {
            for (int i = 0; i < _selectedCutscene.Cuts.Count; i++)
            {
                MQB.Cut entry = _selectedCutscene.Cuts[i];

                if (ImGui.Selectable($@"{entry.Name}##{entry.Name}{i}", i == _selectedCutKey))
                {
                    _selectedTimelineKey = -1;
                    _selectedResourceKey = -1;
                    _selectedDispositionKey = -1;
                    _selectedDispositionTransformKey = -1;
                    _selectedDispositionCustomDataKey = -1;
                    _selectedTimelineCustomDataKey = -1;
                    _selectedDispositionSequenceKey = -1;
                    _selectedDispositionSequencePointKey = -1;

                    _selectedCutKey = i;
                    _selectedCut = entry;
                }
            }
        }

        ImGui.End();
    }

    public void CutPropertiesView()
    {
        ImGui.Begin("Cut Properties##CutProperties");

        if (_selectedCutKey != -1 && _selectedCut != null)
        {
            ImGui.Columns(2);

            ImGui.AlignTextToFramePadding();
            ImGui.Text($"Duration");

            ImGui.NextColumn();

            ImGui.AlignTextToFramePadding();
            ImGui.Text($"{_selectedCut.Duration}");

            ImGui.Columns(1);
        }

        ImGui.End();
    }

    public void TimelineListView()
    {
        ImGui.Begin("Timelines##TimelineList");

        if (_selectedCutKey != -1 && _selectedCut != null)
        {
            // Timelines
            for (int i = 0; i < _selectedCut.Timelines.Count; i++)
            {
                MQB.Timeline entry = _selectedCut.Timelines[i];

                if (ImGui.Selectable($@"ID {i}##{i}", i == _selectedTimelineKey))
                {
                    _selectedDispositionKey = -1;
                    _selectedDispositionTransformKey = -1;
                    _selectedDispositionCustomDataKey = -1;
                    _selectedTimelineCustomDataKey = -1;
                    _selectedDispositionSequenceKey = -1;
                    _selectedDispositionSequencePointKey = -1;

                    _selectedTimelineKey = i;
                    _selectedTimeline = entry;
                }
            }
        }

        ImGui.End();
    }

    public void DispositionListView()
    {
        ImGui.Begin("Dispositions##DispositionList");

        if (_selectedTimelineKey != -1 && _selectedTimeline != null)
        {
            for (int i = 0; i < _selectedTimeline.Dispositions.Count; i++)
            {
                MQB.Disposition entry = _selectedTimeline.Dispositions[i];

                if (ImGui.Selectable($@"ID {entry.ID}##{entry.ID}{i}", i == _selectedDispositionKey))
                {
                    _selectedTimelineCustomDataKey = -1;
                    _selectedDispositionSequenceKey = -1;
                    _selectedDispositionSequencePointKey = -1;

                    _selectedDispositionKey = i;
                    _selectedDisposition = entry;
                }
            }
        }

        ImGui.End();
    }

    public void TimelineCustomDataListView()
    {
        ImGui.Begin("Timeline - Custom Data##TimelineCustomDataList");

        // Custom Data
        if (_selectedTimelineKey != -1 && _selectedTimeline != null)
        {
            for (int i = 0; i < _selectedTimeline.CustomData.Count; i++)
            {
                MQB.CustomData entry = _selectedTimeline.CustomData[i];

                if (ImGui.Selectable($@"{entry.Name}##{entry.Name}{i}", i == _selectedTimelineCustomDataKey))
                {
                    _selectedDispositionKey = -1;
                    _selectedDispositionTransformKey = -1;
                    _selectedDispositionCustomDataKey = -1;
                    _selectedDispositionSequenceKey = -1;
                    _selectedDispositionSequencePointKey = -1;

                    _selectedTimelineCustomDataKey = i;
                    _selectedTimelineCustomData = entry;
                }
            }
        }

        ImGui.End();
    }

    public void TimelineCustomDataPropertiesView()
    {
        ImGui.Begin("Timeline - Custom Data Properties##TimelineCustomDataProperties");

        if (_selectedTimelineCustomDataKey != -1 && _selectedTimelineCustomData != null)
        {
            ImGui.Columns(2);

            ImGui.AlignTextToFramePadding();
            ImGui.Text($"Type");
            ImGui.Text($"Unk44");
            ImGui.Text($"Value");

            ImGui.NextColumn();

            ImGui.AlignTextToFramePadding();
            ImGui.Text($"{_selectedTimelineCustomData.Type}");
            ImGui.Text($"{_selectedTimelineCustomData.Unk44}");
            ImGui.Text($"{_selectedTimelineCustomData.Value}");

            ImGui.Columns(1);
        }

        ImGui.End();
    }

    public void TimelineCustomDataSequencesView()
    {
        ImGui.Begin("Timeline - Sequences##TimelineSequencesProperties");

        if (_selectedTimelineCustomDataKey != -1 && _selectedTimelineCustomData != null)
        {
            for (int i = 0; i < _selectedTimelineCustomData.Sequences.Count; i++)
            {
                MQB.CustomData.Sequence entry = _selectedTimelineCustomData.Sequences[i];

                if (ImGui.Selectable($@"ID {i}##{i}", i == _selectedTimelineSequenceKey))
                {
                    _selectedTimelineSequencePointKey = -1;

                    _selectedTimelineSequenceKey = i;
                    _selectedTimelineSequence = entry;
                }
            }
        }

        ImGui.End();
    }

    public void TimelineCustomDataSequencePropertiesView()
    {
        ImGui.Begin("Timeline - Sequence Properties##TimelineSequenceProperties");

        if (_selectedTimelineCustomData != null && _selectedTimelineCustomData.Sequences.Count > 0)
        {
            if (_selectedTimelineSequenceKey != -1 && _selectedTimelineSequence != null)
            {
                ImGui.Columns(2);

                ImGui.AlignTextToFramePadding();
                ImGui.Text($"Value Type");
                ImGui.Text($"Point Type");
                ImGui.Text($"Value Index");

                ImGui.NextColumn();

                ImGui.AlignTextToFramePadding();
                ImGui.Text($"{_selectedTimelineSequence.ValueType}");
                ImGui.Text($"{_selectedTimelineSequence.PointType}");
                ImGui.Text($"{_selectedTimelineSequence.ValueIndex}");

                ImGui.Columns(1);
            }
        }

        ImGui.End();
    }

    public void TimelineSequencePointsListView()
    {
        ImGui.Begin("Timeline - Sequence - Points##TimelineSequencePointsList");

        if (_selectedTimelineCustomData != null && _selectedTimelineCustomData.Sequences.Count > 0)
        {
            if (_selectedTimelineSequenceKey != -1 && _selectedTimelineSequence != null)
            {
                for (int i = 0; i < _selectedTimelineSequence.Points.Count; i++)
                {
                    MQB.CustomData.Sequence.Point entry = _selectedTimelineSequence.Points[i];

                    if (ImGui.Selectable($@"ID {i}##{i}", i == _selectedTimelineSequencePointKey))
                    {
                        _selectedTimelineSequencePointKey = i;
                        _selectedTimelineSequencePoint = entry;
                    }
                }
            }
        }

        ImGui.End();
    }

    public void TimelineSequencePointPropertiesView()
    {
        ImGui.Begin("Timeline - Sequence - Point Properties##TimelineSequencePointProperties");

        if (_selectedTimelineSequence != null && _selectedTimelineSequence.Points.Count > 0)
        {
            if (_selectedTimelineSequencePointKey != -1 && _selectedTimelineSequencePoint != null)
            {
                ImGui.Columns(2);

                ImGui.AlignTextToFramePadding();
                ImGui.Text($"Value");
                ImGui.Text($"Unk08");
                ImGui.Text($"Unk10");
                ImGui.Text($"Unk14");

                ImGui.NextColumn();

                ImGui.AlignTextToFramePadding();
                ImGui.Text($"{_selectedTimelineSequencePoint.Value}");
                ImGui.Text($"{_selectedTimelineSequencePoint.Unk08}");
                ImGui.Text($"{_selectedTimelineSequencePoint.Unk10}");
                ImGui.Text($"{_selectedTimelineSequencePoint.Unk14}");

                ImGui.Columns(1);
            }
        }

        ImGui.End();
    }

    public void DispositionPropertiesView()
    {
        ImGui.Begin("Disposition Properties##DispositionPropertiesView");

        if (_selectedDispositionKey != -1 && _selectedDisposition != null)
        {
            ImGui.Columns(2);

            ImGui.AlignTextToFramePadding();
            ImGui.Text($"ResourceIndex");
            ImGui.Text($"Unk08");
            ImGui.Text($"StartFrame");
            ImGui.Text($"Duration");
            ImGui.Text($"Unk14");
            ImGui.Text($"Unk18");
            ImGui.Text($"Unk1C");
            ImGui.Text($"Unk20");
            ImGui.Text($"Unk28");

            ImGui.NextColumn();

            ImGui.AlignTextToFramePadding();
            ImGui.Text($"{_selectedDisposition.ResourceIndex}");
            ImGui.Text($"{_selectedDisposition.Unk08}");
            ImGui.Text($"{_selectedDisposition.StartFrame}");
            ImGui.Text($"{_selectedDisposition.Duration}");
            ImGui.Text($"{_selectedDisposition.Unk14}");
            ImGui.Text($"{_selectedDisposition.Unk18}");
            ImGui.Text($"{_selectedDisposition.Unk1C}");
            ImGui.Text($"{_selectedDisposition.Unk20}");
            ImGui.Text($"{_selectedDisposition.Unk28}");

            ImGui.Columns(1);
        }

        ImGui.End();
    }

    public void DispositionCustomDataListView()
    {
        ImGui.Begin("Disposition - Custom Data##DispositionCustomDataListView");

        if (_selectedDispositionKey != -1 && _selectedDisposition != null)
        {
            for (int i = 0; i < _selectedDisposition.CustomData.Count; i++)
            {
                MQB.CustomData entry = _selectedDisposition.CustomData[i];

                if (ImGui.Selectable($@"{entry.Name}##{i}", i == _selectedDispositionCustomDataKey))
                {
                    _selectedDispositionSequenceKey = -1;
                    _selectedDispositionSequencePointKey = -1;

                    _selectedDispositionCustomDataKey = i;
                    _selectedDispositionCustomData = entry;
                }
            }
        }

        ImGui.End();
    }

    public void DispositionTransformListView()
    {
        ImGui.Begin("Disposition - Transforms##DispositionTransformListView");

        if (_selectedDispositionKey != -1 && _selectedDisposition != null)
        {
            for (int i = 0; i < _selectedDisposition.Transforms.Count; i++)
            {
                MQB.Transform entry = _selectedDisposition.Transforms[i];

                if (ImGui.Selectable($@"ID {i}##{i}", i == _selectedDispositionTransformKey))
                {
                    _selectedDispositionSequenceKey = -1;
                    _selectedDispositionSequencePointKey = -1;

                    _selectedDispositionTransformKey = i;
                    _selectedDispositionTransform = entry;
                }
            }
        }

        ImGui.End();
    }

    public void DispositionTransformPropertiesView()
    {
        ImGui.Begin("Disposition - Transform Properties##DispositionTransformProperties");

        if (_selectedDispositionTransformKey != -1 && _selectedDispositionTransform != null)
        {
            ImGui.Columns(2);

            ImGui.AlignTextToFramePadding();
            ImGui.Text($"Frame");
            ImGui.Text($"Translation");
            ImGui.Text($"Unk10");
            ImGui.Text($"Unk1C");
            ImGui.Text($"Rotation");
            ImGui.Text($"Unk34");
            ImGui.Text($"Unk40");
            ImGui.Text($"Scale");
            ImGui.Text($"Unk58");
            ImGui.Text($"Unk64");

            ImGui.NextColumn();

            ImGui.AlignTextToFramePadding();
            ImGui.Text($"{_selectedDispositionTransform.Frame}");
            ImGui.Text($"{_selectedDispositionTransform.Translation}");
            ImGui.Text($"{_selectedDispositionTransform.Unk10}");
            ImGui.Text($"{_selectedDispositionTransform.Unk1C}");
            ImGui.Text($"{_selectedDispositionTransform.Rotation}");
            ImGui.Text($"{_selectedDispositionTransform.Unk34}");
            ImGui.Text($"{_selectedDispositionTransform.Unk40}");
            ImGui.Text($"{_selectedDispositionTransform.Scale}");
            ImGui.Text($"{_selectedDispositionTransform.Unk58}");
            ImGui.Text($"{_selectedDispositionTransform.Unk64}");

            ImGui.Columns(1);
        }

        ImGui.End();
    }

    public void DispositionCustomDataPropertiesView()
    {
        ImGui.Begin("Disposition - Custom Data Properties##DispositionCustomDataProperties");

        if (_selectedDispositionCustomDataKey != -1 && _selectedDispositionCustomData != null)
        {
            ImGui.Columns(2);

            ImGui.AlignTextToFramePadding();
            ImGui.Text($"Type");
            ImGui.Text($"Unk44");
            ImGui.Text($"Value");

            ImGui.NextColumn();

            ImGui.AlignTextToFramePadding();
            ImGui.Text($"{_selectedDispositionCustomData.Type}");
            ImGui.Text($"{_selectedDispositionCustomData.Unk44}");
            ImGui.Text($"{_selectedDispositionCustomData.Value}");

            ImGui.Columns(1);
        }

        ImGui.End();
    }

    public void DispositionCustomDataSequencesView()
    {
        ImGui.Begin("Disposition - Sequences##DispositionSequencesProperties");

        if (_selectedDispositionCustomDataKey != -1 && _selectedDispositionCustomData != null)
        {
            for (int i = 0; i < _selectedDispositionCustomData.Sequences.Count; i++)
            {
                MQB.CustomData.Sequence entry = _selectedDispositionCustomData.Sequences[i];

                if (ImGui.Selectable($@"ID {i}##{i}", i == _selectedDispositionSequenceKey))
                {
                    _selectedDispositionSequencePointKey = -1;

                    _selectedDispositionSequenceKey = i;
                    _selectedDispositionSequence = entry;
                }
            }
        }

        ImGui.End();
    }

    public void DispositionCustomDataSequencePropertiesView()
    {
        ImGui.Begin("Disposition - Sequence Properties##DispositionSequenceProperties");

        if (_selectedDispositionCustomData != null && _selectedDispositionCustomData.Sequences.Count > 0)
        {
            if (_selectedDispositionSequenceKey != -1 && _selectedDispositionSequence != null)
            {
                ImGui.Columns(2);

                ImGui.AlignTextToFramePadding();
                ImGui.Text($"Value Type");
                ImGui.Text($"Point Type");
                ImGui.Text($"Value Index");

                ImGui.NextColumn();

                ImGui.AlignTextToFramePadding();
                ImGui.Text($"{_selectedDispositionSequence.ValueType}");
                ImGui.Text($"{_selectedDispositionSequence.PointType}");
                ImGui.Text($"{_selectedDispositionSequence.ValueIndex}");

                ImGui.Columns(1);
            }
        }

        ImGui.End();
    }

    public void DispositionSequencePointsListView()
    {
        ImGui.Begin("Disposition - Sequence - Points##DispositionSequencePointsList");

        if (_selectedDispositionCustomData != null && _selectedDispositionCustomData.Sequences.Count > 0)
        {
            if (_selectedDispositionSequenceKey != -1 && _selectedDispositionSequence != null)
            {
                for (int i = 0; i < _selectedDispositionSequence.Points.Count; i++)
                {
                    MQB.CustomData.Sequence.Point entry = _selectedDispositionSequence.Points[i];

                    if (ImGui.Selectable($@"ID {i}##{i}", i == _selectedDispositionSequencePointKey))
                    {
                        _selectedDispositionSequencePointKey = i;
                        _selectedDispositionSequencePoint = entry;
                    }
                }
            }
        }

        ImGui.End();
    }

    public void DispositionSequencePointPropertiesView()
    {
        ImGui.Begin("Disposition - Sequence - Point Properties##DispositionSequencePointProperties");

        if (_selectedDispositionSequence != null && _selectedDispositionSequence.Points.Count > 0)
        {
            if (_selectedDispositionSequencePointKey != -1 && _selectedDispositionSequencePoint != null)
            {
                ImGui.Columns(2);

                ImGui.AlignTextToFramePadding();
                ImGui.Text($"Value");
                ImGui.Text($"Unk08");
                ImGui.Text($"Unk10");
                ImGui.Text($"Unk14");

                ImGui.NextColumn();

                ImGui.AlignTextToFramePadding();
                ImGui.Text($"{_selectedDispositionSequencePoint.Value}");
                ImGui.Text($"{_selectedDispositionSequencePoint.Unk08}");
                ImGui.Text($"{_selectedDispositionSequencePoint.Unk10}");
                ImGui.Text($"{_selectedDispositionSequencePoint.Unk14}");

                ImGui.Columns(1);
            }
        }

        ImGui.End();
    }

    public void ResourceListView()
    {
        ImGui.Begin("Resources##ResourceList");

        // Resources
        if (_selectedCutsceneKey != -1 && _selectedCutscene != null)
        {
            for (int i = 0; i < _selectedCutscene.Resources.Count; i++)
            {
                MQB.Resource entry = _selectedCutscene.Resources[i];

                if (ImGui.Selectable($@"{entry.Name}##{entry.Name}{i}", i == _selectedResourceKey))
                {
                    _selectedTimelineKey = -1;
                    _selectedCutKey = -1;
                    _selectedDispositionKey = -1;
                    _selectedDispositionTransformKey = -1;
                    _selectedDispositionCustomDataKey = -1;
                    _selectedTimelineCustomDataKey = -1;
                    _selectedDispositionSequenceKey = -1;
                    _selectedDispositionSequencePointKey = -1;

                    _selectedResourceKey = i;
                    _selectedResource = entry;
                }
            }
        }

        ImGui.End();
    }
    public void OnProjectChanged(ProjectSettings newSettings)
    {
        _projectSettings = newSettings;

        if(CFG.Current.AutoLoadBank_Cutscene)
            CutsceneBank.LoadCutscenes();

        ResetActionManager();
    }

    public void Save()
    {
        if (CutsceneBank.IsLoaded)
            CutsceneBank.SaveCutscene(_selectedFileInfo, _selectedBinder);
    }

    public void SaveAll()
    {
        if (CutsceneBank.IsLoaded)
            CutsceneBank.SaveCutscenes();
    }

    private void ResetActionManager()
    {
        EditorActionManager.Clear();
    }
}
