using StudioCore.Editors.TimeActEditor.Bank;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// Handles the editor command queue
/// </summary>
public class TimeActCommandQueue
{
    private TimeActEditorScreen Screen;
    private TimeActSelectionManager Selection;
    private TimeActDecorator Decorator;

    public TimeActCommandQueue(TimeActEditorScreen screen)
    {
        Screen = screen;
        Selection = screen.Selection;
        Decorator = screen.Decorator;
    }

    /// <summary>
    /// Parse the passed commands for this editor.
    /// </summary>
    public void Parse(string[] initcmd)
    {
        if (initcmd != null && initcmd.Length > 2)
        {
            if (initcmd[0] == "select")
            {
                Selection.ResetSelection();

                var containerType = initcmd[1];
                var containerIndex = initcmd[2];

                if (containerType == "chr")
                {
                    for (int i = 0; i < TimeActBank.FileChrBank.Count; i++)
                    {
                        var container = TimeActBank.FileChrBank.ElementAt(i);
                        var index = int.Parse(containerIndex);

                        if (i == index)
                        {
                            Selection.ContainerIndex = index;
                            Selection.ContainerKey = container.Key.Name;
                            Selection.ContainerInfo = container.Key;
                            Selection.ContainerBinder = container.Value;
                            Selection.FocusContainer = true;
                        }
                    }
                }

                if (containerType == "obj")
                {
                    for (int i = 0; i < TimeActBank.FileObjBank.Count; i++)
                    {
                        var container = TimeActBank.FileObjBank.ElementAt(i);
                        var index = int.Parse(containerIndex);

                        if (i == index)
                        {
                            Selection.ContainerIndex = index;
                            Selection.ContainerKey = container.Key.Name;
                            Selection.ContainerInfo = container.Key;
                            Selection.ContainerBinder = container.Value;
                            Selection.FocusContainer = true;
                        }
                    }
                }

                if (initcmd.Length > 3)
                {
                    var timeActIndex = initcmd[3];
                    var index = int.Parse(timeActIndex);

                    for (int i = 0; i < Selection.ContainerInfo.InternalFiles.Count; i++)
                    {
                        if (i == index)
                        {
                            Selection.CurrentTimeActKey = i;
                            Selection.CurrentTimeAct = Selection.ContainerInfo.InternalFiles[i].TAE;

                            Selection.StoredTimeActs.Add(i, Selection.ContainerInfo.InternalFiles[i].TAE);

                            Selection.FocusTimeAct = true;
                        }
                    }
                }

                if (initcmd.Length > 4)
                {
                    var animationIndex = initcmd[4];
                    var index = int.Parse(animationIndex);

                    for (int i = 0; i < Selection.CurrentTimeAct.Animations.Count; i++)
                    {
                        if (i == index)
                        {
                            Selection.CurrentTimeActAnimationIndex = i;
                            Selection.CurrentTimeActAnimation = Selection.CurrentTimeAct.Animations[i];

                            Selection.StoredAnimations.Add(i, Selection.CurrentTimeAct.Animations[i]);

                            Selection.FocusAnimation = true;
                        }
                    }
                }

                if (initcmd.Length > 5)
                {
                    var eventIndex = initcmd[5];
                    var index = int.Parse(eventIndex);

                    for (int i = 0; i < Selection.CurrentTimeActAnimation.Events.Count; i++)
                    {
                        if (i == index)
                        {
                            Selection.CurrentTimeActEventIndex = i;
                            Selection.CurrentTimeActEvent = Selection.CurrentTimeActAnimation.Events[i];

                            Selection.StoredEvents.Add(i, Selection.CurrentTimeActAnimation.Events[i]);

                            Selection.FocusEvent = true;
                        }
                    }
                }
            }
        }
    }
}
