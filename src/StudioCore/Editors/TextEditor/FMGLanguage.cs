using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editors.MapEditor;
using StudioCore.Locators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.HKXPWV;
using static StudioCore.TextEditor.FMGBank;

namespace StudioCore.Editors.TextEditor;

/*
 * FMGLanguage represents a grouped set of FMGFileSets containing FMGInfos sourced from the same language, within a project's FMGBank.
 */
public class FMGLanguage
{
    internal FMGLanguage(string language)
    {
        LanguageFolder = language;
    }

    internal readonly string LanguageFolder;
    internal bool IsLoaded => _FmgInfoBanks.Count != 0 && _FmgInfoBanks.All((fs) => fs.Value.IsLoaded);
    internal bool IsLoading => _FmgInfoBanks.Count != 0 && _FmgInfoBanks.Any((fs) => fs.Value.IsLoading);
    internal readonly Dictionary<FmgFileCategory, FMGFileSet> _FmgInfoBanks = new();

    /// <summary>
    ///     Loads item and menu MsgBnds from paths, generates FMGInfo, and fills FmgInfoBank.
    /// </summary>
    /// <returns>True if successful; false otherwise.</returns>
    internal bool LoadItemMenuMsgBnds(ResourceDescriptor itemMsgPath, ResourceDescriptor menuMsgPath)
    {
        FMGFileSet itemMsgBnd = new FMGFileSet(FmgFileCategory.Item);

        if (itemMsgBnd.LoadMsgBnd(itemMsgPath.AssetPath, "item.msgbnd"))
            _FmgInfoBanks.Add(itemMsgBnd.FileCategory, itemMsgBnd);

        FMGFileSet menuMsgBnd = new FMGFileSet(FmgFileCategory.Menu);

        if (menuMsgBnd.LoadMsgBnd(menuMsgPath.AssetPath, "menu.msgbnd"))
            _FmgInfoBanks.Add(menuMsgBnd.FileCategory, menuMsgBnd);

        if (_FmgInfoBanks.Count == 0)
            return false;

        return true;
    }

    internal bool LoadNormalFmgs()
    {
        ResourceDescriptor itemMsgPath = ResourceTextLocator.GetItemMsgbnd(LanguageFolder);
        ResourceDescriptor menuMsgPath = ResourceTextLocator.GetMenuMsgbnd(LanguageFolder);

        if (LoadItemMenuMsgBnds(itemMsgPath, menuMsgPath))
        {
            return true;
        }
        return false;

    }
    internal bool LoadDS2FMGs()
    {
        ResourceDescriptor desc = ResourceTextLocator.GetItemMsgbnd(LanguageFolder, true);

        if (desc.AssetPath == null)
        {
            if (LanguageFolder != "")
            {
                TaskLogs.AddLog($"Could not locate text data files when using \"{LanguageFolder}\" folder",
                    LogLevel.Warning);
            }
            else
            {
                TaskLogs.AddLog("Could not locate text data files when using Default English folder",
                    LogLevel.Warning);
            }
            return false;
        }

        List<string> files = Directory
            .GetFileSystemEntries($@"{Smithbox.GameRoot}\{desc.AssetPath}", @"*.fmg").ToList();

        FMGFileSet looseMsg = new FMGFileSet(FmgFileCategory.Loose);
        if (looseMsg.LoadLooseMsgsDS2(files))
        {
            _FmgInfoBanks.Add(looseMsg.FileCategory, looseMsg);
            return true;
        }
        return false;
    }

    public void SaveFMGs()
    {
        try
        {
            if (!IsLoaded)
            {
                return;
            }

            if (Smithbox.ProjectType == ProjectType.Undefined)
            {
                return;
            }

            if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
            {
                SaveFMGsDS2();
            }
            else
            {
                SaveFMGsNormal();
            }
            TaskLogs.AddLog("Saved FMG text");
        }
        catch (SavingFailedException e)
        {
            TaskLogs.AddLog(e.Wrapped.Message,
                LogLevel.Error, TaskLogs.LogPriority.High, e.Wrapped);
        }
    }

    private void SaveFMGsDS2()
    {
        foreach (FMGInfo info in _FmgInfoBanks.SelectMany((x) => x.Value.FmgInfos))
        {
            Utils.WriteWithBackup(Smithbox.GameRoot, Smithbox.ProjectRoot,
                $@"menu\text\{LanguageFolder}\{info.Name}.fmg", info.Fmg);
        }
    }
    private void SaveFMGsNormal()
    {
        // Load the fmg bnd, replace fmgs, and save
        IBinder fmgBinderItem;
        IBinder fmgBinderMenu;
        ResourceDescriptor itemMsgPath = ResourceTextLocator.GetItemMsgbnd(LanguageFolder);
        ResourceDescriptor menuMsgPath = ResourceTextLocator.GetMenuMsgbnd(LanguageFolder);

        if (Smithbox.ProjectType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R)
        {
            fmgBinderItem = BND3.Read(itemMsgPath.AssetPath);
            fmgBinderMenu = BND3.Read(menuMsgPath.AssetPath);
        }
        else
        {
            fmgBinderItem = BND4.Read(itemMsgPath.AssetPath);
            fmgBinderMenu = BND4.Read(menuMsgPath.AssetPath);
        }

        foreach (BinderFile file in fmgBinderItem.Files)
        {
            FMGInfo info = _FmgInfoBanks.SelectMany((x) => x.Value.FmgInfos).FirstOrDefault(e => e.FmgID == (FmgIDType)file.ID);
            if (info != null)
            {
                file.Bytes = info.Fmg.Write();
            }
        }

        foreach (BinderFile file in fmgBinderMenu.Files)
        {
            FMGInfo info = _FmgInfoBanks.SelectMany((x) => x.Value.FmgInfos).FirstOrDefault(e => e.FmgID == (FmgIDType)file.ID);
            if (info != null)
            {
                file.Bytes = info.Fmg.Write();
            }
        }

        ResourceDescriptor itemMsgPathDest = ResourceTextLocator.GetItemMsgbnd(LanguageFolder, true);
        ResourceDescriptor menuMsgPathDest = ResourceTextLocator.GetMenuMsgbnd(LanguageFolder, true);
        var parentDir = Smithbox.GameRoot;
        var modDir = Smithbox.ProjectRoot;
        if (fmgBinderItem is BND3 bnd3)
        {
            Utils.WriteWithBackup(parentDir, modDir, itemMsgPathDest.AssetPath, bnd3);
            Utils.WriteWithBackup(parentDir, modDir, menuMsgPathDest.AssetPath, (BND3)fmgBinderMenu);
            if (Smithbox.ProjectType is ProjectType.DES)
            {
                bnd3.Compression = DCX.Type.None;
                ((BND3)fmgBinderMenu).Compression = DCX.Type.None;
                Utils.WriteWithBackup(parentDir, modDir, itemMsgPathDest.AssetPath[..^4], bnd3);
                Utils.WriteWithBackup(parentDir, modDir, menuMsgPathDest.AssetPath[..^4], (BND3)fmgBinderMenu);
            }
        }
        else if (fmgBinderItem is BND4 bnd4)
        {
            Utils.WriteWithBackup(parentDir, modDir, itemMsgPathDest.AssetPath, bnd4);
            Utils.WriteWithBackup(parentDir, modDir, menuMsgPathDest.AssetPath, (BND4)fmgBinderMenu);
        }

        fmgBinderItem.Dispose();
        fmgBinderMenu.Dispose();
    }
}
