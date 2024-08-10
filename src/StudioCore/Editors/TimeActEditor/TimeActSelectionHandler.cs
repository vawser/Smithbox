using HKLib.hk2018.hkAsyncThreadPool;
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
using static StudioCore.Editors.TimeActEditor.TimeActUtils;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActSelectionHandler
{
    private ActionManager EditorActionManager;
    private TimeActEditorScreen Screen;

    public HavokContainerInfo LoadedHavokContainer;

    public ContainerFileInfo ContainerInfo;
    public BinderInfo ContainerBinder;
    public string ContainerKey;
    public int ContainerIndex = -1;

    public TAE CurrentTimeAct;
    public int CurrentTimeActKey;

    public TAE.Animation CurrentTimeActAnimation;
    public TemporaryAnimHeader CurrentTemporaryAnimHeader;
    public int CurrentTimeActAnimationIndex = -1;

    public TAE.Event CurrentTimeActEvent;
    public int CurrentTimeActEventIndex = -1;

    public string CurrentTimeActEventProperty;
    public int CurrentTimeActEventPropertyIndex = -1;

    public Multiselect TimeActMultiselect;
    public Multiselect TimeActAnimationMultiselect;
    public Multiselect TimeActEventMultiselect;

    public ContextMenu ContextMenu;

    public TemplateType CurrentTimeActType = TemplateType.Character;

    public SelectionContext CurrentSelectionContext = SelectionContext.None;
    public FileContainerType CurrentFileContainerType = FileContainerType.None;

    public enum SelectionContext
    {
        None,
        File,
        TimeAct,
        Animation,
        Event,
        Property
    }

    public enum FileContainerType
    {
        None,
        Character,
        Object
    }

    public TimeActSelectionHandler(ActionManager editorActionManager, TimeActEditorScreen screen)
    {
        EditorActionManager = editorActionManager;
        Screen = screen;

        TimeActMultiselect = new();
        TimeActAnimationMultiselect = new();
        TimeActEventMultiselect = new();

        ContextMenu = new(screen, this);
    }

    public void OnProjectChanged()
    {
        ResetSelection();
    }

    public void ResetSelection()
    {
        ContainerIndex = -1;
        ContainerKey = null;
        ContainerInfo = null;
        ContainerBinder = null;

        CurrentTimeActKey = -1;
        CurrentTimeAct = null;

        CurrentTimeActAnimation = null;
        CurrentTimeActAnimationIndex = -1;
        CurrentTemporaryAnimHeader = null;

        CurrentTimeActEvent = null;
        CurrentTimeActEventIndex = -1;

        CurrentTimeActEventProperty = null;
        CurrentTimeActEventPropertyIndex = -1;

        TimeActMultiselect = new Multiselect();
        TimeActAnimationMultiselect = new Multiselect();
        TimeActEventMultiselect = new Multiselect();
    }

    public void FileContainerChange(ContainerFileInfo info, BinderInfo binderInfo, int index, FileContainerType containerType)
    {
        CurrentFileContainerType = containerType;
        CurrentSelectionContext = SelectionContext.File;

        ContainerIndex = index;
        ContainerKey = info.Name;
        ContainerInfo = info;
        ContainerBinder = binderInfo;

        CurrentTimeActKey = -1;
        CurrentTimeAct = null;

        CurrentTimeActAnimation = null;
        CurrentTimeActAnimationIndex = -1;
        CurrentTemporaryAnimHeader = null;

        CurrentTimeActEvent = null;
        CurrentTimeActEventIndex = -1;

        CurrentTimeActEventProperty = null;
        CurrentTimeActEventPropertyIndex = -1;

        TimeActMultiselect = new Multiselect();
        TimeActAnimationMultiselect = new Multiselect();
        TimeActEventMultiselect = new Multiselect();
    }

    public void ResetOnTimeActChange()
    {
        CurrentTimeActKey = -1;
        CurrentTimeAct = null;

        CurrentTimeActAnimation = null;
        CurrentTimeActAnimationIndex = -1;
        CurrentTemporaryAnimHeader = null;

        CurrentTimeActEvent = null;
        CurrentTimeActEventIndex = -1;

        CurrentTimeActEventProperty = null;
        CurrentTimeActEventPropertyIndex = -1;

        TimeActMultiselect = new Multiselect();
        TimeActAnimationMultiselect = new Multiselect();
        TimeActEventMultiselect = new Multiselect();
    }

    public void TimeActChange(TAE entry, int index)
    {
        CurrentSelectionContext = SelectionContext.TimeAct;

        TimeActMultiselect.HandleMultiselect(CurrentTimeActKey, index);

        CurrentTimeActKey = index;
        CurrentTimeAct = entry;

        CurrentTimeActAnimation = null;
        CurrentTemporaryAnimHeader = null;
        CurrentTimeActAnimationIndex = -1;

        CurrentTimeActEvent = null;
        CurrentTimeActEventIndex = -1;

        CurrentTimeActEventProperty = null;
        CurrentTimeActEventPropertyIndex = -1;

        TimeActAnimationMultiselect = new Multiselect();
        TimeActEventMultiselect = new Multiselect();

        TimeActUtils.ApplyTemplate(CurrentTimeAct, CurrentTimeActType);
    }

    public void ResetOnTimeActAnimationChange()
    {
        CurrentTimeActAnimation = null;
        CurrentTimeActAnimationIndex = -1;
        CurrentTemporaryAnimHeader = null;

        CurrentTimeActEvent = null;
        CurrentTimeActEventIndex = -1;

        CurrentTimeActEventProperty = null;
        CurrentTimeActEventPropertyIndex = -1;

        TimeActAnimationMultiselect = new Multiselect();
        TimeActEventMultiselect = new Multiselect();
    }

    public void TimeActAnimationChange(TAE.Animation entry, int index)
    {
        CurrentSelectionContext = SelectionContext.Animation;

        TimeActAnimationMultiselect.HandleMultiselect(CurrentTimeActAnimationIndex, index);

        CurrentTimeActAnimation = entry;
        CurrentTimeActAnimationIndex = index;
        CurrentTemporaryAnimHeader = null;

        CurrentTimeActEvent = null;
        CurrentTimeActEventIndex = -1;

        CurrentTimeActEventProperty = null;
        CurrentTimeActEventPropertyIndex = -1;

        // If a filter is active, auto-select first result (if any), since this is more user-friendly
        if(TimeActFilters._timeActEventFilterString != "")
        {
            Screen.SelectFirstEvent = true;
        }

        TimeActEventMultiselect = new Multiselect();
    }

    public void ResetOnTimeActEventChange()
    {
        CurrentTimeActEvent = null;
        CurrentTimeActEventIndex = -1;

        CurrentTimeActEventProperty = null;
        CurrentTimeActEventPropertyIndex = -1;

        TimeActEventMultiselect = new Multiselect();
    }

    public void TimeActEventChange(TAE.Event entry, int index)
    {
        CurrentSelectionContext = SelectionContext.Event;

        TimeActEventMultiselect.HandleMultiselect(CurrentTimeActEventIndex, index);

        CurrentTimeActEvent = entry;
        CurrentTimeActEventIndex = index;

        CurrentTimeActEventProperty = null;
        CurrentTimeActEventPropertyIndex = -1;
    }

    public void TimeActEventPropertyChange(string entry, int index)
    {
        CurrentSelectionContext = SelectionContext.Property;

        CurrentTimeActEventProperty = entry;
        CurrentTimeActEventPropertyIndex = index;
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

    public bool HasSelectedTimeActEvent()
    {
        return CurrentTimeActEvent != null;
    }
}
