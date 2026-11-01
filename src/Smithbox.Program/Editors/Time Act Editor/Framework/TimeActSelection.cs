using SoulsFormats;
using StudioCore.Editors.ModelEditor;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActSelection
{
    public TimeActContainerWrapper SelectedTimeActContainerWrapper;
    public TimeActModelWrapper SelectedTimeActModelWrapper;
    public TimeActEntry SelectedTimeAct;
    public TAE.Animation SelectedAnimation;
    public TAE.Event SelectedEvent;

    public bool IsAnyModelLoaded()
    {
        if (SelectedTimeActModelWrapper != null)
        {
            return true;
        }

        return false;
    }
}
