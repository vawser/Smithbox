using SoulsFormats;
using StudioCore.CutsceneEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.Editors.CutsceneEditor.CutsceneBank;

namespace StudioCore.Editors.CutsceneEditor;

public class CutsceneSelectionManager
{
    private CutsceneEditorScreen Screen;

    public CutsceneFileInfo _selectedFileInfo;
    public IBinder _selectedBinder;
    public string _selectedBinderKey;

    public MQB _selectedCutscene;
    public int _selectedCutsceneKey;

    public MQB.Resource _selectedResource;
    public int _selectedResourceKey;

    public MQB.Cut _selectedCut;
    public int _selectedCutKey;

    public MQB.Timeline _selectedTimeline;
    public int _selectedTimelineKey;

    public MQB.Disposition _selectedDisposition;
    public int _selectedDispositionKey;

    public MQB.CustomData _selectedTimelineCustomData;
    public int _selectedTimelineCustomDataKey;

    public MQB.CustomData.Sequence _selectedTimelineSequence;
    public int _selectedTimelineSequenceKey;

    public MQB.CustomData.Sequence.Point _selectedTimelineSequencePoint;
    public int _selectedTimelineSequencePointKey;

    public MQB.CustomData _selectedDispositionCustomData;
    public int _selectedDispositionCustomDataKey;

    public MQB.Transform _selectedDispositionTransform;
    public int _selectedDispositionTransformKey;

    public MQB.CustomData.Sequence _selectedDispositionSequence;
    public int _selectedDispositionSequenceKey;

    public MQB.CustomData.Sequence.Point _selectedDispositionSequencePoint;
    public int _selectedDispositionSequencePointKey;

    public CutsceneSelectionManager(CutsceneEditorScreen screen)
    {
        Screen = screen;
    }
    public void OnProjectChanged()
    {

    }

    /// <summary>
    /// Set selected file
    /// </summary>
    public void SetFile(CutsceneFileInfo info, IBinder binder)
    {
        _selectedBinderKey = info.Name;
        _selectedFileInfo = info;
        _selectedBinder = binder;
    }

    /// <summary>
    /// Set selected cutscene
    /// </summary>
    public void SetCutscene(int key, MQB entry)
    {
        _selectedCutsceneKey = key;
        _selectedCutscene = entry;
    }

    /// <summary>
    /// Clear selected cutscene
    /// </summary>
    public void ResetCutscene()
    {
        _selectedCutsceneKey = -1;
        _selectedCutscene = null;
    }

    /// <summary>
    /// Set selected resource
    /// </summary>
    public void SetResource(int key, MQB.Resource entry)
    {
        _selectedResourceKey = key;
        _selectedResource = entry;
    }

    /// <summary>
    /// Clear selected resource
    /// </summary>
    public void ResetResource()
    {
        _selectedResourceKey = -1;
        _selectedResource = null;
    }

    /// <summary>
    /// Set selected cut
    /// </summary>
    public void SetCut(int key, MQB.Cut entry)
    {
        _selectedCutKey = key;
        _selectedCut = entry;
    }

    /// <summary>
    /// Clear selected cut
    /// </summary>
    public void ResetCut()
    {
        _selectedCutKey = -1;
        _selectedCut = null;
    }

    /// <summary>
    /// Set selected timeline
    /// </summary>
    public void SetTimeline(int key, MQB.Timeline entry)
    {
        _selectedTimelineKey = key;
        _selectedTimeline = entry;
    }

    /// <summary>
    /// Clear selected timeline
    /// </summary>
    public void ResetTimeline()
    {
        _selectedTimelineKey = -1;
        _selectedTimeline = null;
    }

    /// <summary>
    /// Set selected timeline custom data
    /// </summary>
    public void SetTimelineCustomData(int key, MQB.CustomData entry)
    {
        _selectedTimelineCustomDataKey = key;
        _selectedTimelineCustomData = entry;
    }

    /// <summary>
    /// Clear selected timeline custom data
    /// </summary>
    public void ResetTimelineCustomData()
    {
        _selectedTimelineCustomDataKey = -1;
        _selectedTimelineCustomData = null;
    }

    /// <summary>
    /// Set selected timeline sequence
    /// </summary>
    public void SetTimelineSequence(int key, MQB.CustomData.Sequence entry)
    {
        _selectedTimelineSequenceKey = key;
        _selectedTimelineSequence = entry;
    }

    /// <summary>
    /// Clear selected timeline sequence
    /// </summary>
    public void ResetTimelineSequence()
    {
        _selectedTimelineSequenceKey = -1;
        _selectedTimelineSequence = null;
    }

    /// <summary>
    /// Set selected timeline sequence point
    /// </summary>
    public void SetTimelineSequencePoint(int key, MQB.CustomData.Sequence.Point entry)
    {
        _selectedTimelineSequencePointKey = key;
        _selectedTimelineSequencePoint = entry;
    }

    /// <summary>
    /// Clear selected timeline sequence point
    /// </summary>
    public void ResetTimelineSequencePoint()
    {
        _selectedTimelineSequencePointKey = -1;
        _selectedTimelineSequencePoint = null;
    }

    /// <summary>
    /// Set selected disposition
    /// </summary>
    public void SetDisposition(int key, MQB.Disposition entry)
    {
        _selectedDispositionKey = key;
        _selectedDisposition = entry;
    }

    /// <summary>
    /// Clear selected disposition
    /// </summary>
    public void ResetDisposition()
    {
        _selectedDispositionKey = -1;
        _selectedDisposition = null;
    }

    /// <summary>
    /// Set selected disposition custom data
    /// </summary>
    public void SetDispositionCustomData(int key, MQB.CustomData entry)
    {
        _selectedDispositionCustomDataKey = key;
        _selectedDispositionCustomData = entry;
    }

    /// <summary>
    /// Clear selected disposition custom data
    /// </summary>
    public void ResetDispositionCustomData()
    {
        _selectedDispositionCustomDataKey = -1;
        _selectedDispositionCustomData = null;
    }

    /// <summary>
    /// Set selected disposition transform
    /// </summary>
    public void SetDispositionTransform(int key, MQB.Transform entry)
    {
        _selectedDispositionTransformKey = key;
        _selectedDispositionTransform = entry;
    }

    /// <summary>
    /// Clear selected disposition transform
    /// </summary>
    public void ResetDispositionTransform()
    {
        _selectedDispositionTransformKey = -1;
        _selectedDispositionTransform = null;
    }

    /// <summary>
    /// Set selected disposition sequence
    /// </summary>
    public void SetDispositionSequence(int key, MQB.CustomData.Sequence entry)
    {
        _selectedDispositionSequenceKey = key;
        _selectedDispositionSequence = entry;
    }

    /// <summary>
    /// Clear selected disposition sequence
    /// </summary>
    public void ResetDispositionSequence()
    {
        _selectedDispositionSequenceKey = -1;
        _selectedDispositionSequence = null;
    }

    /// <summary>
    /// Clear selected disposition sequence point
    /// </summary>
    public void SetDispositionSequencePoint(int key, MQB.CustomData.Sequence.Point entry)
    {
        _selectedDispositionSequencePointKey = key;
        _selectedDispositionSequencePoint = entry;
    }

    /// <summary>
    /// Clear selected disposition sequence point
    /// </summary>
    public void ResetDispositionSequencePoint()
    {
        _selectedDispositionSequencePointKey = -1;
        _selectedDispositionSequencePoint = null;
    }
}
