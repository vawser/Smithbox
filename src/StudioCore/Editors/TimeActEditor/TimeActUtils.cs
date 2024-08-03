using DotNext.Collections.Generic;
using HKLib.hk2018;
using HKLib.hk2018.hkAsyncThreadPool;
using SoulsFormats;
using StudioCore.Banks.AliasBank;
using StudioCore.Banks.HavokAliasBank;
using StudioCore.Core;
using StudioCore.Editors.HavokEditor;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static SoulsFormats.DRB;
using static StudioCore.Editors.TimeActEditor.AnimationBank;

namespace StudioCore.Editors.TimeActEditor;

public static class TimeActUtils
{
    public static void DisplayTimeActFileAlias(string name)
    {
        var referenceDict = Smithbox.AliasCacheHandler.AliasCache.Characters;
        var lowerName = name.ToLower();

        if (referenceDict.ContainsKey(lowerName))
        {
            var aliasName = referenceDict[lowerName].name;

            AliasUtils.DisplayAlias(aliasName);
        }
    }

    public static void DisplayTimeActAlias(AnimationFileInfo info, int id)
    {
        if (Smithbox.BankHandler.TimeActAliases.Aliases != null)
        {
            var idStr = id.ToString();
            var idSection = idStr.Substring(idStr.Length - 3);

            var searchStr = $"{info.Name}_{idSection}";
            var alias = Smithbox.BankHandler.TimeActAliases.Aliases.list.Where(e => e.id == searchStr)
                .FirstOrDefault();

            if (alias != null)
            {
                var aliasStr = alias.name;
                AliasUtils.DisplayAlias(aliasStr);
            }
            else
            {
                AliasUtils.DisplayAlias("");
            }
        }
    }

    public static void DisplayAnimationAlias(TimeActSelectionHandler SelectionHandler, long id)
    {
        if (Smithbox.BankHandler.HavokGeneratorAliases != null)
        {
            List<string> aliasList = new();
            foreach(var entry in Smithbox.BankHandler.HavokGeneratorAliases.HavokAliases.List)
            {
                if(entry.ID == id.ToString())
                {
                    aliasList = entry.Generators;
                    break;
                }
            }
            if (aliasList.Count > 0)
            {
                if (CFG.Current.TimeActEditor_DisplayAllGenerators)
                {
                    AliasUtils.DisplayAlias(string.Join(", ", aliasList));
                }
                else
                {
                    AliasUtils.DisplayAlias(aliasList[0]);
                }
                AliasUtils.AliasTooltip(aliasList, "Generators that use this animation:");
            }
        }
    }

    public static HavokContainerInfo LoadHavokObjects(AnimationFileInfo info)
    {
        HavokContainerInfo newInfo = null;

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
                        newInfo.LoadFile(file.ToLower());
                        newInfo.ReadHavokObjects(file.ToLower());
                        return newInfo;
                    }
                }
            }
        }

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

    public static void ApplyTemplate(TAE entry)
    {
        switch (Smithbox.ProjectType)
        {
            case ProjectType.DS1:
                entry.ApplyTemplate(AnimationBank.TimeActTemplates["TAE.Template.DS1"]);
                break;
            case ProjectType.DS2:
            case ProjectType.DS2S:
                entry.ApplyTemplate(AnimationBank.TimeActTemplates["TAE.Template.SOTFS"]);
                break;
            case ProjectType.DS3:
                entry.ApplyTemplate(AnimationBank.TimeActTemplates["TAE.Template.DS3"]);
                break;
            case ProjectType.BB:
                entry.ApplyTemplate(AnimationBank.TimeActTemplates["TAE.Template.BB"]);
                break;
            case ProjectType.SDT:
                entry.ApplyTemplate(AnimationBank.TimeActTemplates["TAE.Template.SDT"]);
                break;
            case ProjectType.ER:
                entry.ApplyTemplate(AnimationBank.TimeActTemplates["TAE.Template.ER"]);
                break;
            case ProjectType.AC6:
                entry.ApplyTemplate(AnimationBank.TimeActTemplates["TAE.Template.AC6"]);
                break;
        }
    }
}
