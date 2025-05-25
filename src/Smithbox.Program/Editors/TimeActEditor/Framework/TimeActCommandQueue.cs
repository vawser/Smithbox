using Octokit;
using StudioCore.Core;
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
    public TimeActEditorScreen Editor;
    public ProjectEntry Project;

    public TimeActCommandQueue(TimeActEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
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
                Editor.Selection.ResetSelection();

                var binderType = initcmd[1];
                var binderName = initcmd[2];

                if (binderType == "chr")
                {
                    foreach (var entry in Project.TimeActData.PrimaryBank.Entries)
                    {
                        var file = entry.Key;
                        var binder = entry.Value;

                        if (file.Filename == binderName)
                        {
                            Editor.Selection.SelectedFileKey = file.Filename;
                            Editor.Selection.SelectedFileEntry = file;
                            Editor.Selection.SelectedBinder = binder;
                            Editor.Selection.FocusContainer = true;
                        }
                    }
                }

                if (initcmd.Length > 3)
                {
                    var timeActName = initcmd[3];
                    var index = int.Parse(timeActName);

                    for (int i = 0; i < Editor.Selection.SelectedBinder.Files.Count; i++)
                    {
                        var curTimeAct = Editor.Selection.SelectedBinder.Files.ElementAt(i);

                        if(curTimeAct.Key != null)
                        {
                            if(curTimeAct.Key.Name == timeActName)
                            {
                                Editor.Selection.CurrentTimeActKey = curTimeAct.Key.Name;
                                Editor.Selection.CurrentTimeActIndex = i;
                                Editor.Selection.CurrentTimeAct = curTimeAct.Value;

                                Editor.Selection.FocusTimeAct = true;
                            }
                        }
                    }
                }

                if (initcmd.Length > 4)
                {
                    var animationIndex = initcmd[4];
                    var index = int.Parse(animationIndex);

                    for (int i = 0; i < Editor.Selection.CurrentTimeAct.Animations.Count; i++)
                    {
                        if (i == index)
                        {
                            Editor.Selection.CurrentTimeActAnimationIndex = i;
                            Editor.Selection.CurrentTimeActAnimation = Editor.Selection.CurrentTimeAct.Animations[i];

                            if (!Editor.Selection.StoredAnimations.ContainsKey(i))
                            {
                                Editor.Selection.StoredAnimations.Add(i, Editor.Selection.CurrentTimeAct.Animations[i]);
                            }
                            else
                            {
                                Editor.Selection.StoredAnimations[i] = Editor.Selection.CurrentTimeAct.Animations[i];
                            }

                            Editor.Selection.FocusAnimation = true;
                        }
                    }
                }

                if (initcmd.Length > 5)
                {
                    var eventIndex = initcmd[5];
                    var index = int.Parse(eventIndex);

                    for (int i = 0; i < Editor.Selection.CurrentTimeActAnimation.Events.Count; i++)
                    {
                        if (i == index)
                        {
                            Editor.Selection.CurrentTimeActEventIndex = i;
                            Editor.Selection.CurrentTimeActEvent = Editor.Selection.CurrentTimeActAnimation.Events[i];

                            if (!Editor.Selection.StoredEvents.ContainsKey(i))
                            {
                                Editor.Selection.StoredEvents.Add(i, Editor.Selection.CurrentTimeActAnimation.Events[i]);
                            }
                            else
                            {
                                Editor.Selection.StoredEvents[i] = Editor.Selection.CurrentTimeActAnimation.Events[i];
                            }

                            Editor.Selection.FocusEvent = true;
                        }
                    }
                }
            }
        }
    }
}
