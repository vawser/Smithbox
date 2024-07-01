using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.TextEditor;
using StudioCore.Locators;
using StudioCore.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static StudioCore.Editors.ParticleEditor.ParticleBank;

namespace StudioCore.TextEditor;

/// <summary>
///     Static class that stores all the strings for a Souls game.
/// </summary>
public class FMGBank
{
    public bool IsLoaded => fmgLangs.Count != 0 && fmgLangs.All((fs) => fs.Value.IsLoaded);
    public bool IsLoading => fmgLangs.Count != 0 && fmgLangs.Any((fs) => fs.Value.IsLoading);
    public string LanguageFolder { get; private set; } = "";

    public IEnumerable<FMGInfo> FmgInfoBank { get => fmgLangs[LanguageFolder]._FmgInfoBanks.SelectMany((x) => x.Value.FmgInfos); }
    public IEnumerable<FMGInfo> SortedFmgInfoBank
    {
        get
        {
            //This check shouldn't be here. Should do better housekeeping.
            if (IsLoading || !IsLoaded || !fmgLangs.ContainsKey(LanguageFolder))
            {
                return [];
            }
            if (CFG.Current.FMG_NoGroupedFmgEntries)
            {
                return FmgInfoBank.OrderBy(e => e.EntryCategory).ThenBy(e => e.FmgID);
            }
            else
            {
                return FmgInfoBank.OrderBy(e => e.Name);
            }
        }
    }
    public Dictionary<string, FMGLanguage> fmgLangs = new();
    public IEnumerable<FmgFileCategory> currentFmgInfoBanks
    {
        get
        {
            if (IsLoading || !IsLoaded || !fmgLangs.ContainsKey(LanguageFolder))
            {
                return [];
            }
            return fmgLangs[LanguageFolder]._FmgInfoBanks.Keys;
        }
    }

    /// <summary>
    /// List of strings to compare with "FmgIDType" name to identify patch FMGs.
    /// </summary>
    private static readonly List<string> patchStrings = new() { "_Patch", "_DLC1", "_DLC2", "_SOTE", "_SOTE_DLC2" };

    /// <summary>
    /// Removes patch/DLC identifiers from strings for the purposes of finding patch FMGs. Kinda dumb.
    /// </summary>
    public string RemovePatchStrings(string str)
    {
        foreach (var badString in patchStrings)
        {
            str = str.Replace(badString, "", StringComparison.CurrentCultureIgnoreCase);
        }

        return str;
    }

    public void LoadFMGs(string languageFolder = "")
    {
        TaskManager.Run(new TaskManager.LiveTask("FMG - Load Text - " + languageFolder, TaskManager.RequeueType.WaitThenRequeue, true,
            () =>
            {
                LanguageFolder = languageFolder;
                SetDefaultLanguagePath();
                if (fmgLangs.ContainsKey(LanguageFolder))
                {
                    return;
                }

                if (Smithbox.ProjectType == ProjectType.Undefined)
                {
                    return;
                }

                FMGLanguage lang = new FMGLanguage(LanguageFolder);
                bool success = false;

                if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
                {
                    success = lang.LoadDS2FMGs();
                }
                else
                {
                    success = lang.LoadNormalFmgs();
                }
                if (success)
                    fmgLangs.Add(lang.LanguageFolder, lang);
            }));
    }

    public void SaveFMGs()
    {
        foreach (FMGLanguage lang in fmgLangs.Values)
        {
            lang.SaveFMGs();
        }
    }


    private void SetDefaultLanguagePath()
    {
        if (LanguageFolder == "")
        {
            // By default, try to find path to English folder.
            foreach (KeyValuePair<string, string> lang in ResourceTextLocator.GetMsgLanguages())
            {
                var folder = lang.Value.Split("\\").Last();
                if (folder.Contains("eng", StringComparison.CurrentCultureIgnoreCase))
                {
                    LanguageFolder = folder;
                    break;
                }
            }
        }
    }

    /// <summary>
    ///     Get patched FMG Entries for the specified category, with TextType Title or TextBody.
    /// </summary>
    /// <param name="category">FMGEntryCategory. If "None", an empty list will be returned.</param>
    /// <returns>List of patched entries if found; empty list otherwise.</returns>
    public List<FMG.Entry> GetFmgEntriesByCategory(FmgEntryCategory category, bool sort = true)
    {
        if (category == FmgEntryCategory.None)
        {
            return new List<FMG.Entry>();
        }

        foreach (FMGInfo info in FmgInfoBank)
        {
            if (info.EntryCategory == category &&
                info.EntryType is FmgEntryTextType.Title or FmgEntryTextType.TextBody)
            {
                return info.GetPatchedEntries(sort);
            }
        }

        return new List<FMG.Entry>();
    }

    /// <summary>
    ///     Get patched FMG Entries for the specified category and text type.
    /// </summary>
    /// <param name="category">FMGEntryCategory . If "None", an empty list will be returned.</param>
    /// <returns>List of patched entries if found; empty list otherwise.</returns>
    public List<FMG.Entry> GetFmgEntriesByCategoryAndTextType(FmgEntryCategory category,
        FmgEntryTextType textType, bool sort = true)
    {
        if (category == FmgEntryCategory.None)
        {
            return new List<FMG.Entry>();
        }

        foreach (FMGInfo info in FmgInfoBank)
        {
            if (info.EntryCategory == category && info.EntryType == textType)
            {
                return info.GetPatchedEntries(sort);
            }
        }

        return new List<FMG.Entry>();
    }

    /// <summary>
    ///     Get patched FMG Entries for the specified FmgIDType.
    /// </summary>
    /// <returns>List of patched entries if found; empty list otherwise.</returns>
    public List<FMG.Entry> GetFmgEntriesByFmgIDType(FmgIDType fmgID, bool sort = true)
    {
        foreach (FMGInfo info in FmgInfoBank)
        {
            if (info.FmgID == fmgID)
            {
                return info.GetPatchedEntries(sort);
            }
        }

        return new List<FMG.Entry>();
    }

    /// <summary>
    ///     Generate a new EntryGroup using a given ID and FMGInfo.
    ///     Data is updated using FMGInfo PatchChildren.
    /// </summary>
    public FMGEntryGroup GenerateEntryGroup(int id, FMGInfo fmgInfo)
    {
        FMGEntryGroup eGroup = new() { ID = id };

        if (fmgInfo.EntryCategory == FmgEntryCategory.None || CFG.Current.FMG_NoGroupedFmgEntries)
        {
            List<EntryFMGInfoPair> entryPairs = fmgInfo.GetPatchedEntryFMGPairs();
            EntryFMGInfoPair pair = entryPairs.Find(e => e.Entry.ID == id);
            if (pair == null)
            {
                return eGroup;
            }

            eGroup.TextBody = pair.Entry;
            eGroup.TextBodyInfo = pair.FmgInfo;
            return eGroup;
        }

        foreach (FMGInfo info in FmgInfoBank)
        {
            if (info.EntryCategory == fmgInfo.EntryCategory && info.PatchParent == null)
            {
                List<EntryFMGInfoPair> entryPairs = info.GetPatchedEntryFMGPairs();
                EntryFMGInfoPair pair = entryPairs.Find(e => e.Entry.ID == id);
                if (pair != null)
                {
                    switch (info.EntryType)
                    {
                        case FmgEntryTextType.Title:
                            eGroup.Title = pair.Entry;
                            eGroup.TitleInfo = pair.FmgInfo;
                            break;
                        case FmgEntryTextType.Summary:
                            eGroup.Summary = pair.Entry;
                            eGroup.SummaryInfo = pair.FmgInfo;
                            break;
                        case FmgEntryTextType.Description:
                            eGroup.Description = pair.Entry;
                            eGroup.DescriptionInfo = pair.FmgInfo;
                            break;
                        case FmgEntryTextType.ExtraText:
                            eGroup.ExtraText = pair.Entry;
                            eGroup.ExtraTextInfo = pair.FmgInfo;
                            break;
                        case FmgEntryTextType.TextBody:
                            eGroup.TextBody = pair.Entry;
                            eGroup.TextBodyInfo = pair.FmgInfo;
                            break;
                    }
                }
            }
        }

        return eGroup;
    }
}
