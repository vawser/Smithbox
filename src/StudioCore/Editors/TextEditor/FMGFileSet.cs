using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SoapstoneLib.SoulsFmg;
using static SoulsFormats.HKXPWV;
using static StudioCore.TextEditor.FMGBank;

namespace StudioCore.Editors.TextEditor;

/*
 * FMGFileSet represents a grouped set of FMGInfos source from the same bnd or loose folder, within a single language.
 */
public class FMGFileSet
{
    internal FMGFileSet(FmgFileCategory category)
    {
        Lang = new FMGLanguage(Smithbox.BankHandler.FMGBank.LanguageFolder);
        FileCategory = category;
    }
    internal FMGLanguage Lang;
    internal FmgFileCategory FileCategory;
    internal bool IsLoaded = false;
    internal bool IsLoading = false;

    internal List<FMGInfo> FmgInfos = new();

    internal void InsertFmgInfo(FMGInfo info)
    {
        FmgInfos.Add(info);
    }

    /// <summary>
    ///     Loads MsgBnd from path, generates FMGInfo, and fills FmgInfoBank.
    /// </summary>
    /// <returns>True if successful; false otherwise.</returns>
    internal bool LoadMsgBnd(string path, string msgBndType = "UNDEFINED")
    {
        if (path == null)
        {
            if (Lang != null)
            {
                if (Lang.LanguageFolder != "")
                {
                    TaskLogs.AddLog(
                        $"Could locate text data files when looking for \"{msgBndType}\" in \"{Lang.LanguageFolder}\" folder",
                        LogLevel.Warning);
                }
                else
                {
                    TaskLogs.AddLog(
                        $"Could not locate text data files when looking for \"{msgBndType}\" in Default English folder",
                        LogLevel.Warning);
                }
            }

            IsLoaded = false;
            IsLoading = false;
            return false;
        }

        IBinder fmgBinder;

        if (Smithbox.ProjectType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R)
        {
            fmgBinder = BND3.Read(path);
        }
        else
        {
            fmgBinder = BND4.Read(path);
        }

        foreach (BinderFile file in fmgBinder.Files)
        {
            FmgInfos.Add(GenerateFMGInfo(file));
        }
        // I hate this 2 parse system. Solve game differences by including game in the get enum functions. Maybe parentage is solvable by pre-sorting binderfiles but that does seem silly. FMG patching sucks. 
        foreach (FMGInfo info in FmgInfos)
        {
            /* TODO sorting without modifying data
            if (CFG.Current.FMG_NoGroupedFmgEntries)
            {
                info.EntryType = FmgEntryTextType.TextBody;
            }*/
            SetFMGInfoPatchParent(info);
        }

        fmgBinder.Dispose();

        HandleDuplicateEntries();
        IsLoaded = true;
        IsLoading = false;
        return true;
    }

    internal bool LoadLooseMsgsDS2(IEnumerable<string> files)
    {
        foreach (var file in files)
        {
            FMGInfo info = GenerateFMGInfoDS2(file);
            InsertFmgInfo(info);
        }

        //TODO ordering
        //FmgInfoBank = FmgInfoBank.OrderBy(e => e.Name).ToList();
        HandleDuplicateEntries();
        IsLoaded = true;
        IsLoading = false;
        return true;
    }

    internal void HandleDuplicateEntries()
    {
        var askedAboutDupes = false;
        var ignoreDupes = true;
        foreach (FMGInfo info in FmgInfos)
        {
            IEnumerable<FMG.Entry> dupes = info.Fmg.Entries.GroupBy(e => e.ID).SelectMany(g => g.SkipLast(1));
            if (dupes.Any())
            {
                var dupeList = string.Join(", ", dupes.Select(dupe => dupe.ID));
                if (!askedAboutDupes && PlatformUtils.Instance.MessageBox(
                        $"Duplicate text entries have been found in FMG {Path.GetFileNameWithoutExtension(info.FileName)} for the following row IDs:\n\n{dupeList}\n\nRemove all duplicates? (Latest entries are kept)",
                        "Duplicate Text Entries", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    ignoreDupes = false;
                }

                askedAboutDupes = true;

                if (!ignoreDupes)
                {
                    foreach (FMG.Entry dupe in dupes)
                    {
                        info.Fmg.Entries.Remove(dupe);
                    }
                }
            }
        }
    }
    internal FMGInfo GenerateFMGInfo(BinderFile file)
    {
        FMG fmg = FMG.Read(file.Bytes);
        var name = Enum.GetName(typeof(FmgIDType), file.ID);
        FMGInfo info = new()
        {
            FileSet = this,
            FileName = file.Name.Split("\\").Last(),
            Name = name,
            FmgID = (FmgIDType)file.ID,
            Fmg = fmg,
            EntryType = FMGUtils.GetFmgTextType(file.ID),
            EntryCategory = FMGUtils.GetFmgCategory(file.ID)
        };
        info.FileCategory = FMGUtils.GetFMGUICategory(info.EntryCategory);

        ApplyGameDifferences(info);
        return info;
    }

    internal FMGInfo GenerateFMGInfoDS2(string file)
    {
        // TODO: DS2 FMG grouping & UI sorting (copy SetFMGInfo)
        FMG fmg = FMG.Read(file);
        var name = Path.GetFileNameWithoutExtension(file);
        FMGInfo info = new()
        {
            FileSet = this,
            FileName = file.Split("\\").Last(),
            Name = name,
            FmgID = FmgIDType.None,
            Fmg = fmg,
            EntryType = FmgEntryTextType.TextBody,
            EntryCategory = FmgEntryCategory.None,
            FileCategory = FmgFileCategory.Loose
        };

        return info;
    }
    private void SetFMGInfoPatchParent(FMGInfo info)
    {
        var strippedName = Smithbox.BankHandler.FMGBank.RemovePatchStrings(info.Name);
        if (strippedName != info.Name)
        {
            // This is a patch FMG, try to find parent FMG.
            foreach (FMGInfo parentInfo in FmgInfos)
            {
                if (parentInfo.Name == strippedName)
                {
                    info.AddParent(parentInfo);
                    return;
                }
            }

            TaskLogs.AddLog($"Couldn't find patch parent for FMG \"{info.Name}\" with ID {info.FmgID}",
                LogLevel.Error);
        }
    }

    /// <summary>
    ///     Checks and applies FMG info that differs per-game.
    /// </summary>
    private void ApplyGameDifferences(FMGInfo info)
    {
        ProjectType gameType = Smithbox.ProjectType;
        switch (info.FmgID)
        {
            case FmgIDType.ReusedFMG_32:
                if (gameType == ProjectType.BB)
                {
                    info.Name = "GemExtraInfo";
                    info.FileCategory = FmgFileCategory.Item;
                    info.EntryCategory = FmgEntryCategory.Gem;
                    info.EntryType = FmgEntryTextType.ExtraText;
                }
                else
                {
                    info.Name = "ActionButtonText";
                    info.EntryCategory = FmgEntryCategory.ActionButtonText;
                }

                break;
            case FmgIDType.ReusedFMG_35:
                if (gameType == ProjectType.AC6)
                {
                    info.Name = "TitleGenerator";
                    info.FileCategory = FmgFileCategory.Item;
                    info.EntryCategory = FmgEntryCategory.Generator;
                    info.EntryType = FmgEntryTextType.Title;
                }
                else
                {
                    info.Name = "TitleGem";
                    info.FileCategory = FmgFileCategory.Item;
                    info.EntryCategory = FmgEntryCategory.Gem;
                    info.EntryType = FmgEntryTextType.Title;
                }

                break;
            case FmgIDType.ReusedFMG_36:
                if (gameType == ProjectType.AC6)
                {
                    info.Name = "DescriptionGenerator";
                    info.FileCategory = FmgFileCategory.Item;
                    info.EntryCategory = FmgEntryCategory.Generator;
                    info.EntryType = FmgEntryTextType.Description;
                }
                else
                {
                    info.Name = "SummaryGem";
                    info.FileCategory = FmgFileCategory.Item;
                    info.EntryCategory = FmgEntryCategory.Gem;
                    info.EntryType = FmgEntryTextType.Summary;
                }

                break;
            case FmgIDType.ReusedFMG_41:
                if (gameType == ProjectType.AC6)
                {
                    info.Name = "TitleFCS";
                    info.FileCategory = FmgFileCategory.Item;
                    info.EntryCategory = FmgEntryCategory.FCS;
                    info.EntryType = FmgEntryTextType.Title;
                }
                else
                {
                    info.Name = "TitleMessage";
                    info.FileCategory = FmgFileCategory.Menu;
                    info.EntryCategory = FmgEntryCategory.Message;
                    info.EntryType = FmgEntryTextType.TextBody;
                }

                break;
            case FmgIDType.ReusedFMG_42:
                if (gameType == ProjectType.AC6)
                {
                    info.Name = "DescriptionFCS";
                    info.FileCategory = FmgFileCategory.Item;
                    info.EntryCategory = FmgEntryCategory.FCS;
                    info.EntryType = FmgEntryTextType.Description;
                }
                else
                {
                    info.Name = "TitleSwordArts";
                    info.FileCategory = FmgFileCategory.Item;
                    info.EntryCategory = FmgEntryCategory.SwordArts;
                    info.EntryType = FmgEntryTextType.Title;
                }

                break;
            case FmgIDType.Event:
            case FmgIDType.Event_Patch:
                if (gameType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R
                    or ProjectType.BB)
                {
                    info.EntryCategory = FmgEntryCategory.ActionButtonText;
                }

                break;
            case FmgIDType.ReusedFMG_205:
                if (gameType == ProjectType.ER)
                {
                    info.Name = "LoadingTitle";
                    info.EntryType = FmgEntryTextType.Title;
                    info.EntryCategory = FmgEntryCategory.LoadingScreen;
                }
                else if (gameType == ProjectType.SDT)
                {
                    info.Name = "LoadingText";
                    info.EntryType = FmgEntryTextType.Description;
                    info.EntryCategory = FmgEntryCategory.LoadingScreen;
                }
                else if (gameType == ProjectType.AC6)
                {
                    info.Name = "MenuContext";
                }
                else
                {
                    info.Name = "SystemMessage_PS4";
                }

                break;
            case FmgIDType.ReusedFMG_206:
                if (gameType == ProjectType.ER)
                {
                    info.Name = "LoadingText";
                    info.EntryType = FmgEntryTextType.Description;
                    info.EntryCategory = FmgEntryCategory.LoadingScreen;
                }
                else if (gameType == ProjectType.SDT)
                {
                    info.Name = "LoadingTitle";
                    info.EntryType = FmgEntryTextType.Title;
                    info.EntryCategory = FmgEntryCategory.LoadingScreen;
                }
                else
                {
                    info.Name = "SystemMessage_XboxOne";
                }

                break;
            case FmgIDType.ReusedFMG_210:
                if (gameType == ProjectType.ER)
                {
                    info.Name = "ToS_win64";
                    info.FileCategory = FmgFileCategory.Menu;
                    info.EntryType = FmgEntryTextType.TextBody;
                    info.EntryCategory = FmgEntryCategory.None;
                }
                else if (gameType == ProjectType.AC6)
                {
                    info.Name = "TextEmbeddedImageNames";
                    info.FileCategory = FmgFileCategory.Menu;
                    info.EntryType = FmgEntryTextType.TextBody;
                    info.EntryCategory = FmgEntryCategory.None;
                }
                else
                {
                    info.Name = "TitleGoods_DLC1";
                    info.FileCategory = FmgFileCategory.Item;
                    info.EntryType = FmgEntryTextType.Title;
                    info.EntryCategory = FmgEntryCategory.Goods;
                    FMGInfo parent = FmgInfos.FirstOrDefault(e => e.FmgID == FmgIDType.TitleGoods);
                    info.AddParent(parent);
                }

                break;
        }
    }
}
