using DotNext.Collections.Generic;
using HKLib.hk2018;
using HKLib.hk2018.hkAsyncThreadPool;
using Org.BouncyCastle.Crypto;
using SoulsFormats;
using StudioCore.Banks.AliasBank;
using StudioCore.Banks.HavokAliasBank;
using StudioCore.Core.Project;
using StudioCore.Editors.HavokEditor.Enums;
using StudioCore.Editors.HavokEditor.Framework;
using StudioCore.Editors.TimeActEditor.Bank;
using StudioCore.Editors.TimeActEditor.Enums;
using StudioCore.Interface;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static SoulsFormats.DRB;
using static StudioCore.Editors.TimeActEditor.Bank.TimeActBank;

namespace StudioCore.Editors.TimeActEditor.Utils;

public static class TimeActUtils
{
    /// <summary>
    /// Get title string for Object/Asset differentiation based on project type. 
    /// </summary>
    public static string GetObjectTitle()
    {
        string title = "Object";

        if (Smithbox.ProjectType is ProjectType.ER or ProjectType.AC6)
        {
            title = "Asset";
        }

        return title;
    }
    public static void DisplayTimeActFileAlias(string name, TimeActAliasType type)
    {
        var referenceDict = Smithbox.AliasCacheHandler.AliasCache.Characters;

        if (type == TimeActAliasType.Asset)
        {
            referenceDict = Smithbox.AliasCacheHandler.AliasCache.Assets;
        }

        var lowerName = name.ToLower();

        if (referenceDict.ContainsKey(lowerName))
        {
            var aliasName = referenceDict[lowerName].name;

            UIHelper.DisplayAlias(aliasName);
        }
    }

    public static void DisplayTimeActAlias(TimeActContainerWrapper info, int id)
    {
        if (Smithbox.BankHandler.TimeActAliases.Aliases != null)
        {
            var idStr = id.ToString();
            if (idStr.Length > 3)
            {
                var idSection = idStr.Substring(idStr.Length - 3);

                var searchStr = $"{info.Name}_{idSection}";
                var alias = Smithbox.BankHandler.TimeActAliases.Aliases.list.Where(e => e.id == searchStr)
                    .FirstOrDefault();

                if (alias != null)
                {
                    var aliasStr = alias.name;
                    UIHelper.DisplayAlias(aliasStr);
                }
                else
                {
                    UIHelper.DisplayAlias("");
                }
            }
        }
    }

    public static void DisplayAnimationAlias(TimeActSelectionManager SelectionHandler, long id)
    {
        if (Smithbox.BankHandler.HavokGeneratorAliases != null)
        {
            List<string> aliasList = new();
            foreach (var entry in Smithbox.BankHandler.HavokGeneratorAliases.HavokAliases.List)
            {
                if (entry.ID == id.ToString())
                {
                    aliasList = entry.Generators;
                    break;
                }
            }
            if (aliasList.Count > 0)
            {
                if (CFG.Current.TimeActEditor_DisplayAllGenerators)
                {
                    UIHelper.DisplayAlias(string.Join(", ", aliasList));
                }
                else
                {
                    UIHelper.DisplayAlias(aliasList[0]);
                }
                AliasUtils.AliasTooltip(aliasList, "Generators that use this animation:");
            }
        }
    }

    public static HavokContainerInfo LoadHavokObjects(TimeActContainerWrapper info)
    {
        HavokContainerInfo newInfo = null;

        /*
        foreach (var entry in HavokFileBank.BehaviorContainerBank)
        {
            if (entry.Filename == info.Name)
            {
                entry.LoadBinder();

                foreach (var file in entry.InternalFileList)
                {
                    var name = file.Split("export")[1];
                    if (name.Contains("behaviors"))
                    {
                        newInfo = entry;
                        newInfo.LoadFile(file.ToLower(), HavokInternalType.Behavior.ToString());
                        newInfo.ReadHavokObjects(file.ToLower(), HavokInternalType.Behavior.ToString());
                        return newInfo;
                    }
                }
            }
        }
        */

        return newInfo;
    }

    public static string GetTimeActName(int id)
    {
        var displayName = "";

        var idStr = id.ToString();
        var idSection = idStr.Substring(idStr.Length - 3);
        displayName = $"a{idSection}";

        return displayName;
    }

    public static TAE.Template GetRelevantTemplate(TimeActTemplateType type)
    {
        switch (Smithbox.ProjectType)
        {
            case ProjectType.DES:
                return TimeActTemplates["TAE.Template.DES"];
            case ProjectType.DS1:
            case ProjectType.DS1R:
                if (type is TimeActTemplateType.Character)
                {
                    return TimeActTemplates["TAE.Template.DS1"];
                }
                else if (type is TimeActTemplateType.Object)
                {
                    return TimeActTemplates["TAE.Template.DS1.OBJ"];
                }
                else if (type is TimeActTemplateType.Cutscene)
                {
                    return TimeActTemplates["TAE.Template.DS1.REMO"];
                }
                break;
            case ProjectType.DS2:
            case ProjectType.DS2S:
                return TimeActTemplates["TAE.Template.SOTFS"];
            case ProjectType.DS3:
                return TimeActTemplates["TAE.Template.DS3"];
            case ProjectType.BB:
                return TimeActTemplates["TAE.Template.BB"];
            case ProjectType.SDT:
                return TimeActTemplates["TAE.Template.SDT"];
            case ProjectType.ER:
                return TimeActTemplates["TAE.Template.ER"];
            case ProjectType.AC6:
                return TimeActTemplates["TAE.Template.AC6"];
        }

        return null;
    }

    public static void ApplyTemplate(TAE entry, TimeActTemplateType type)
    {
        switch (Smithbox.ProjectType)
        {
            case ProjectType.DES:
                entry.ApplyTemplate(TimeActTemplates["TAE.Template.DES"]);
                break;
            case ProjectType.DS1:
            case ProjectType.DS1R:
                if (type is TimeActTemplateType.Character)
                {
                    entry.ApplyTemplate(TimeActTemplates["TAE.Template.DS1"]);
                }
                else if (type is TimeActTemplateType.Object)
                {
                    entry.ApplyTemplate(TimeActTemplates["TAE.Template.DS1.OBJ"]);
                }
                else if (type is TimeActTemplateType.Cutscene)
                {
                    entry.ApplyTemplate(TimeActTemplates["TAE.Template.DS1.REMO"]);
                }
                break;
            case ProjectType.DS2:
            case ProjectType.DS2S:
                entry.ApplyTemplate(TimeActTemplates["TAE.Template.SOTFS"]);
                break;
            case ProjectType.DS3:
                entry.ApplyTemplate(TimeActTemplates["TAE.Template.DS3"]);
                break;
            case ProjectType.BB:
                entry.ApplyTemplate(TimeActTemplates["TAE.Template.BB"]);
                break;
            case ProjectType.SDT:
                entry.ApplyTemplate(TimeActTemplates["TAE.Template.SDT"]);
                break;
            case ProjectType.ER:
                entry.ApplyTemplate(TimeActTemplates["TAE.Template.ER"]);
                break;
            case ProjectType.AC6:
                entry.ApplyTemplate(TimeActTemplates["TAE.Template.AC6"]);
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

    public static void SelectAdjustedAnimation(TAE.Animation targetAnim)
    {
        var Selection = Smithbox.EditorHandler.TimeActEditor.Selection;
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

    public static void SelectNewAnimation(int targetIndex)
    {
        var Selection = Smithbox.EditorHandler.TimeActEditor.Selection;
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

    public static void SelectNewEvent(int targetIndex)
    {
        var Selection = Smithbox.EditorHandler.TimeActEditor.Selection;
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
