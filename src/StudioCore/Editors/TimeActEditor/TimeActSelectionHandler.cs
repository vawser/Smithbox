using HKLib.hk2018.hkHashMapDetail;
using SoulsFormats;
using StudioCore.Editor;
using StudioCore.Editors.HavokEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.DRB;
using static StudioCore.Editors.TimeActEditor.AnimationBank;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActSelectionHandler
{
    private ActionManager EditorActionManager;
    private TimeActEditorScreen Screen;

    public AnimationFileInfo ContainerInfo;
    public IBinder ContainerBinder;
    public string ContainerKey;
    public int ContainerIndex = -1;

    public TAE CurrentTimeAct;
    public int CurrentTimeActKey;
    public int CurrentTimeActIndex = -1;

    public TAE.Animation CurrentTimeActAnimation;
    public int CurrentTimeActAnimationIndex = -1;

    public TAE.Event CurrentTimeActEvent;
    public int CurrentTimeActEventIndex = -1;

    public HavokContainerInfo LoadedHavokContainer;

    public Multiselect TimeActMultiselect;
    public Multiselect TimeActAnimationMultiselect;
    public Multiselect TimeActEventMultiselect;

    public TimeActSelectionHandler(ActionManager editorActionManager, TimeActEditorScreen screen)
    {
        EditorActionManager = editorActionManager;
        Screen = screen;

        TimeActMultiselect = new();
        TimeActAnimationMultiselect = new();
        TimeActEventMultiselect = new();
    }

    public void FileContainerChange(AnimationFileInfo info, IBinder binder, int index)
    {
        ContainerIndex = index;
        ContainerKey = info.Name;
        ContainerInfo = info;
        ContainerBinder = binder;

        CurrentTimeActKey = -1;
        CurrentTimeAct = null;

        CurrentTimeActAnimation = null;
        CurrentTimeActAnimationIndex = -1;

        CurrentTimeActEvent = null;
        CurrentTimeActEventIndex = -1;

        TimeActMultiselect = new Multiselect();
        TimeActAnimationMultiselect = new Multiselect();
        TimeActEventMultiselect = new Multiselect();
    }

    public void TimeActChange(TAE entry, int index)
    {
        TimeActMultiselect.HandleMultiselect(CurrentTimeActKey, index);

        CurrentTimeActKey = index;
        CurrentTimeAct = entry;

        CurrentTimeActAnimation = null;
        CurrentTimeActAnimationIndex = -1;

        CurrentTimeActEvent = null;
        CurrentTimeActEventIndex = -1;

        TimeActAnimationMultiselect = new Multiselect();
        TimeActEventMultiselect = new Multiselect();

        TimeActUtils.ApplyTemplate(CurrentTimeAct);
    }

    public void TimeActAnimationChange(TAE.Animation entry, int index)
    {
        TimeActAnimationMultiselect.HandleMultiselect(CurrentTimeActAnimationIndex, index);

        CurrentTimeActAnimation = entry;
        CurrentTimeActAnimationIndex = index;

        CurrentTimeActEvent = null;
        CurrentTimeActEventIndex = -1;

        TimeActEventMultiselect = new Multiselect();
    }

    public void TimeActEventChange(TAE.Event entry, int index)
    {
        TimeActEventMultiselect.HandleMultiselect(CurrentTimeActEventIndex, index);

        CurrentTimeActEvent = entry;
        CurrentTimeActEventIndex = index;
    }

    public bool HasSelectedFileContainer()
    {
        return ContainerInfo != null;
    }

    public bool HasSelectedTimeAct()
    {
        return CurrentTimeAct != null;
    }

    public bool HasSelectedTimeActAnimation()
    {
        return CurrentTimeActAnimation != null;
    }
}
