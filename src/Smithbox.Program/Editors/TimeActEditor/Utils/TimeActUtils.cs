using Google.Protobuf.Reflection;
using Microsoft.AspNetCore.Components.Forms;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editors.TimeActEditor.Bank;
using StudioCore.Editors.TimeActEditor.Enums;
using StudioCore.Interface;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Linq;
using static StudioCore.Editors.TimeActEditor.Bank.TimeActBank;

namespace StudioCore.Editors.TimeActEditor.Utils;

public static class TimeActUtils
{
    /// <summary>
    /// Get title string for Object/Asset differentiation based on project type. 
    /// </summary>
    public static string GetObjectTitle(ProjectEntry project)
    {
        string title = "Object";

        if (project.ProjectType is ProjectType.ER or ProjectType.AC6)
        {
            title = "Asset";
        }

        return title;
    }

    public static void DisplayTimeActFileAlias(TimeActEditorScreen editor, string name, TimeActAliasType type)
    {
        if(type is TimeActAliasType.Character)
        {
            var aliasEntry = editor.Project.Aliases.Characters.Where(e => e.ID == name).FirstOrDefault();
            if(aliasEntry != null)
            {
                UIHelper.DisplayAlias(aliasEntry.Name);
            }
        }

        if (type is TimeActAliasType.Asset)
        {
            var aliasEntry = editor.Project.Aliases.Assets.Where(e => e.ID == name).FirstOrDefault();
            if (aliasEntry != null)
            {
                UIHelper.DisplayAlias(aliasEntry.Name);
            }
        }
    }

    public static void DisplayTimeActAlias(TimeActEditorScreen editor, string filename, int id)
    {
        var idStr = id.ToString();
        if (idStr.Length > 3)
        {
            var idSection = idStr.Substring(idStr.Length - 3);

            var searchStr = $"{filename}_{idSection}";
            var alias = editor.Project.Aliases.TimeActs.Where(e => e.ID == searchStr)
                .FirstOrDefault();

            if (alias != null)
            {
                var aliasStr = alias.Name;
                UIHelper.DisplayAlias(aliasStr);
            }
            else
            {
                UIHelper.DisplayAlias("");
            }
        }
    }

    public static TAE.Template GetRelevantTemplate(TimeActEditorScreen editor, TimeActTemplateType type)
    {
        switch (editor.Project.ProjectType)
        {
            case ProjectType.DES:
                return editor.Project.TimeActData.TimeActTemplates["TAE.Template.DES"];
            case ProjectType.DS1:
            case ProjectType.DS1R:
                if (type is TimeActTemplateType.Character)
                {
                    return editor.Project.TimeActData.TimeActTemplates["TAE.Template.DS1"];
                }
                else if (type is TimeActTemplateType.Object)
                {
                    return editor.Project.TimeActData.TimeActTemplates["TAE.Template.DS1.OBJ"];
                }
                else if (type is TimeActTemplateType.Cutscene)
                {
                    return editor.Project.TimeActData.TimeActTemplates["TAE.Template.DS1.REMO"];
                }
                break;
            case ProjectType.DS2:
            case ProjectType.DS2S:
                return editor.Project.TimeActData.TimeActTemplates["TAE.Template.SOTFS"];
            case ProjectType.DS3:
                return editor.Project.TimeActData.TimeActTemplates["TAE.Template.DS3"];
            case ProjectType.BB:
                return editor.Project.TimeActData.TimeActTemplates["TAE.Template.BB"];
            case ProjectType.SDT:
                return editor.Project.TimeActData.TimeActTemplates["TAE.Template.SDT"];
            case ProjectType.ER:
                return editor.Project.TimeActData.TimeActTemplates["TAE.Template.ER"];
            case ProjectType.AC6:
                return editor.Project.TimeActData.TimeActTemplates["TAE.Template.AC6"];
        }

        return null;
    }

    public static void ApplyTemplate(TimeActEditorScreen editor, TAE entry, TimeActTemplateType type)
    {
        switch (editor.Project.ProjectType)
        {
            case ProjectType.DES:
                entry.ApplyTemplate(editor.Project.TimeActData.TimeActTemplates["TAE.Template.DES"]);
                break;
            case ProjectType.DS1:
            case ProjectType.DS1R:
                if (type is TimeActTemplateType.Character)
                {
                    entry.ApplyTemplate(editor.Project.TimeActData.TimeActTemplates["TAE.Template.DS1"]);
                }
                else if (type is TimeActTemplateType.Object)
                {
                    entry.ApplyTemplate(editor.Project.TimeActData.TimeActTemplates["TAE.Template.DS1.OBJ"]);
                }
                else if (type is TimeActTemplateType.Cutscene)
                {
                    entry.ApplyTemplate(editor.Project.TimeActData.TimeActTemplates["TAE.Template.DS1.REMO"]);
                }
                break;
            case ProjectType.DS2:
            case ProjectType.DS2S:
                entry.ApplyTemplate(editor.Project.TimeActData.TimeActTemplates["TAE.Template.SOTFS"]);
                break;
            case ProjectType.DS3:
                entry.ApplyTemplate(editor.Project.TimeActData.TimeActTemplates["TAE.Template.DS3"]);
                break;
            case ProjectType.BB:
                entry.ApplyTemplate(editor.Project.TimeActData.TimeActTemplates["TAE.Template.BB"]);
                break;
            case ProjectType.SDT:
                entry.ApplyTemplate(editor.Project.TimeActData.TimeActTemplates["TAE.Template.SDT"]);
                break;
            case ProjectType.ER:
                entry.ApplyTemplate(editor.Project.TimeActData.TimeActTemplates["TAE.Template.ER"]);
                break;
            case ProjectType.AC6:
                entry.ApplyTemplate(editor.Project.TimeActData.TimeActTemplates["TAE.Template.AC6"]);
                break;
        }
    }

    public static TAE.Animation CloneAnimation(TAE.Animation sourceAnim)
    {
        TAE.Animation newAnim = new TAE.Animation(sourceAnim.ID, sourceAnim.MiniHeader.GetClone(), sourceAnim.AnimFileName);

        List<TAE.EventGroup> newEventGroups = new();
        foreach (var eventGrp in sourceAnim.EventGroups)
        {
            newEventGroups.Add(eventGrp.GetClone());
        }
        List<TAE.Event> newEvents = new();
        foreach (var evt in sourceAnim.Events)
        {
            newEvents.Add(evt.GetClone(false));
        }

        newAnim.EventGroups = newEventGroups;
        newAnim.Events = newEvents;

        return newAnim;
    }

    public static void SelectAdjustedAnimation(TimeActEditorScreen editor, TAE.Animation targetAnim)
    {
        var Selection = editor.Selection;
        Selection.StoredAnimations.Clear();

        Selection.CurrentTimeAct.Animations.Sort();
        for (int i = 0; i < Selection.CurrentTimeAct.Animations.Count; i++)
        {
            var serAnim = Selection.CurrentTimeAct.Animations[i];
            if (serAnim.ID == targetAnim.ID)
            {
                Selection.CurrentTimeActAnimation = serAnim;
                Selection.CurrentTimeActAnimationIndex = i;
                Selection.StoredAnimations.Add(i, Selection.CurrentTimeActAnimation);
                break;
            }
        }
    }

    public static void SelectNewAnimation(TimeActEditorScreen editor, int targetIndex)
    {
        var Selection = editor.Selection;
        Selection.StoredAnimations.Clear();

        for (int i = 0; i < Selection.CurrentTimeAct.Animations.Count; i++)
        {
            var curAnim = Selection.CurrentTimeAct.Animations[i];

            if (i == targetIndex)
            {
                Selection.CurrentTimeActAnimation = curAnim;
                Selection.CurrentTimeActAnimationIndex = i;
                Selection.StoredAnimations.Add(i, Selection.CurrentTimeActAnimation);
                break;
            }
        }
    }

    public static void SelectNewEvent(TimeActEditorScreen editor, int targetIndex)
    {
        var Selection = editor.Selection;
        Selection.StoredEvents.Clear();

        for (int i = 0; i < Selection.CurrentTimeActAnimation.Events.Count; i++)
        {
            var curEvent = Selection.CurrentTimeActAnimation.Events[i];

            if (i == targetIndex)
            {
                Selection.CurrentTimeActEvent = curEvent;
                Selection.CurrentTimeActEventIndex = i;
                Selection.StoredEvents.Add(i, Selection.CurrentTimeActEvent);
                break;
            }
        }
    }
}
