using HKLib.hk2018;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Editors.HavokEditor;

// Specific state when in Behavior View (for selected file)
public class BehaviorViewSelection
{
    public bool IsBehaviorGraph = false;

    public bool InBehaviorGraphTab = false;
    public bool InClipGeneratorTab = false;

    public hkbBehaviorGraph SelectedBehaviorGraph;
    public hkbClipGenerator SelectedClipGenerator;

    public void Reset()
    {
        IsBehaviorGraph = false;

        SelectedBehaviorGraph = null;
        SelectedClipGenerator = null;
    }
}