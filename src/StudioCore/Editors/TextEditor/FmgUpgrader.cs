using SoulsFormats;
using StudioCore.Core;
using StudioCore.Locators;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor
{
    public class FmgUpgrader
    {
        private TextEditorScreen Screen;

        public bool DisplayFmgUpdate = false;
        private Dictionary<FmgIDType, List<FMG.Entry>> itemEntriesToUpdate;
        private Dictionary<FmgIDType, List<FMG.Entry>> menuEntriesToUpdate;

        public FmgUpgrader(TextEditorScreen screen)
        {
            Screen = screen;
        }

        public bool IsSupportedProjectType_FmgUpdate()
        {
            if (Smithbox.ProjectType is ProjectType.ER)
                return true;

            return false;
        }

        public void SetupFmgUpdate()
        {
            // Only support ER for now, although technically DS3 should be covered
            if (Smithbox.ProjectType is ProjectType.ER)
            {
                var langFolder = Smithbox.BankHandler.FMGBank.LanguageFolder;

                Dictionary<FmgFileCategory, FMGFileSet> Project_Item_VanillaFmgInfoBanks = new();
                Dictionary<FmgFileCategory, FMGFileSet> Project_DLC_Item_VanillaFmgInfoBanks = new();
                Dictionary<FmgFileCategory, FMGFileSet> Base_Item_VanillaFmgInfoBanks = new();
                Dictionary<FmgFileCategory, FMGFileSet> Base_DLC_Item_VanillaFmgInfoBanks = new();

                Dictionary<FmgFileCategory, FMGFileSet> Project_Menu_VanillaFmgInfoBanks = new();
                Dictionary<FmgFileCategory, FMGFileSet> Project_DLC_Menu_VanillaFmgInfoBanks = new();
                Dictionary<FmgFileCategory, FMGFileSet> Base_Menu_VanillaFmgInfoBanks = new();
                Dictionary<FmgFileCategory, FMGFileSet> Base_DLC_Menu_VanillaFmgInfoBanks = new();

                ResourceDescriptor projectItemMsgPath = TextLocator.GetMsgbnd_Project_Upgrader("item", "", langFolder);
                ResourceDescriptor projectDlcItemMsgPath = TextLocator.GetMsgbnd_Project_Upgrader("item", "_dlc02", langFolder);
                ResourceDescriptor baseItemMsgPath = TextLocator.GetMsgbnd_Vanilla_Upgrader("item", "", langFolder);
                ResourceDescriptor baseDlcItemMsgPath = TextLocator.GetMsgbnd_Vanilla_Upgrader("item", "_dlc02", langFolder);

                ResourceDescriptor projectMenuMsgPath = TextLocator.GetMsgbnd_Project_Upgrader("menu", "", langFolder);
                ResourceDescriptor projectDlcMenuMsgPath = TextLocator.GetMsgbnd_Project_Upgrader("menu", "_dlc02", langFolder);
                ResourceDescriptor baseMenuMsgPath = TextLocator.GetMsgbnd_Vanilla_Upgrader("menu", "", langFolder);
                ResourceDescriptor baseDlcMenuMsgPath = TextLocator.GetMsgbnd_Vanilla_Upgrader("menu", "_dlc02", langFolder);

                // If the asset paths do not exist, return early to stop a failed msgbnd load
                if (!File.Exists(projectItemMsgPath.AssetPath) ||
                    !File.Exists(projectDlcItemMsgPath.AssetPath) ||
                    !File.Exists(baseItemMsgPath.AssetPath) ||
                    !File.Exists(baseDlcItemMsgPath.AssetPath) ||
                    !File.Exists(projectMenuMsgPath.AssetPath) ||
                    !File.Exists(projectDlcMenuMsgPath.AssetPath) ||
                    !File.Exists(baseMenuMsgPath.AssetPath) ||
                    !File.Exists(baseDlcMenuMsgPath.AssetPath))
                    return;

                FMGFileSet projectItemMsgBnd = new FMGFileSet(FmgFileCategory.Item);
                FMGFileSet projectDlcItemMsgBnd = new FMGFileSet(FmgFileCategory.Item);
                FMGFileSet baseItemMsgBnd = new FMGFileSet(FmgFileCategory.Item);
                FMGFileSet baseDlcItemMsgBnd = new FMGFileSet(FmgFileCategory.Item);
                FMGFileSet projectMenuMsgBnd = new FMGFileSet(FmgFileCategory.Menu);
                FMGFileSet projectDlcMenuMsgBnd = new FMGFileSet(FmgFileCategory.Menu);
                FMGFileSet baseMenuMsgBnd = new FMGFileSet(FmgFileCategory.Menu);
                FMGFileSet baseDlcMenuMsgBnd = new FMGFileSet(FmgFileCategory.Menu);

                if (projectItemMsgBnd.LoadMsgBnd(projectItemMsgPath.AssetPath, "item.msgbnd"))
                    Project_Item_VanillaFmgInfoBanks.Add(projectItemMsgBnd.FileCategory, projectItemMsgBnd);

                if (projectDlcItemMsgBnd.LoadMsgBnd(projectDlcItemMsgPath.AssetPath, "item.msgbnd"))
                    Project_DLC_Item_VanillaFmgInfoBanks.Add(projectDlcItemMsgBnd.FileCategory, projectDlcItemMsgBnd);

                if (baseItemMsgBnd.LoadMsgBnd(baseItemMsgPath.AssetPath, "item.msgbnd"))
                    Base_Item_VanillaFmgInfoBanks.Add(baseItemMsgBnd.FileCategory, baseItemMsgBnd);

                if (baseDlcItemMsgBnd.LoadMsgBnd(baseDlcItemMsgPath.AssetPath, "item.msgbnd"))
                    Base_DLC_Item_VanillaFmgInfoBanks.Add(baseDlcItemMsgBnd.FileCategory, baseDlcItemMsgBnd);

                if (projectMenuMsgBnd.LoadMsgBnd(projectMenuMsgPath.AssetPath, "menu.msgbnd"))
                    Project_Menu_VanillaFmgInfoBanks.Add(projectMenuMsgBnd.FileCategory, projectMenuMsgBnd);

                if (projectDlcMenuMsgBnd.LoadMsgBnd(projectDlcMenuMsgPath.AssetPath, "menu.msgbnd"))
                    Project_DLC_Menu_VanillaFmgInfoBanks.Add(projectDlcMenuMsgBnd.FileCategory, projectDlcMenuMsgBnd);

                if (baseMenuMsgBnd.LoadMsgBnd(baseMenuMsgPath.AssetPath, "menu.msgbnd"))
                    Base_Menu_VanillaFmgInfoBanks.Add(baseMenuMsgBnd.FileCategory, baseMenuMsgBnd);

                if (baseDlcMenuMsgBnd.LoadMsgBnd(baseDlcMenuMsgPath.AssetPath, "menu.msgbnd"))
                    Base_DLC_Menu_VanillaFmgInfoBanks.Add(baseDlcMenuMsgBnd.FileCategory, baseDlcMenuMsgBnd);

                itemEntriesToUpdate = new Dictionary<FmgIDType, List<FMG.Entry>>();
                menuEntriesToUpdate = new Dictionary<FmgIDType, List<FMG.Entry>>();

                itemEntriesToUpdate = GetEntriesToUpdate(Project_Item_VanillaFmgInfoBanks, Project_DLC_Item_VanillaFmgInfoBanks, Base_Item_VanillaFmgInfoBanks, Base_DLC_Item_VanillaFmgInfoBanks);
                menuEntriesToUpdate = GetEntriesToUpdate(Project_Menu_VanillaFmgInfoBanks, Project_DLC_Menu_VanillaFmgInfoBanks, Base_Menu_VanillaFmgInfoBanks, Base_DLC_Menu_VanillaFmgInfoBanks);

                foreach (var entry in itemEntriesToUpdate)
                {
                    foreach (var fmgEntry in entry.Value)
                    {
                        DisplayFmgUpdate = true;
                        //TaskLogs.AddLog($"New Item Entry: {entry.Key} - {fmgEntry.ID} {fmgEntry.Text}");
                    }
                }
                foreach (var entry in menuEntriesToUpdate)
                {
                    foreach (var fmgEntry in entry.Value)
                    {
                        DisplayFmgUpdate = true;
                        //TaskLogs.AddLog($"New Menu Entry: {entry.Key} - {fmgEntry.ID} {fmgEntry.Text}");
                    }
                }
            }
        }

        public void UpdateFmgs()
        {
            ApplyFmgUpdate(itemEntriesToUpdate);
            ApplyFmgUpdate(menuEntriesToUpdate);

            Smithbox.BankHandler.FMGBank.SaveFMGs();

            DisplayFmgUpdate = false;

            SetupFmgUpdate(); // Re-run this to check FMG update state and set to false if update was successful
        }

        private void ApplyFmgUpdate(Dictionary<FmgIDType, List<FMG.Entry>> entries)
        {
            var fmgBank = Smithbox.BankHandler.FMGBank;

            foreach (var updateEntry in entries)
            {
                foreach (var entry in fmgBank.FmgInfoBank)
                {
                    if (updateEntry.Key == entry.FmgID)
                    {
                        foreach (var fmgEntry in updateEntry.Value)
                        {
                            entry.Fmg.Entries.Add(fmgEntry);
                        }
                    }
                }
            }
        }

        private Dictionary<FmgIDType, List<FMG.Entry>> GetEntriesToUpdate(Dictionary<FmgFileCategory, FMGFileSet> projectInfoBanks, Dictionary<FmgFileCategory, FMGFileSet> projectDlcInfoBanks, Dictionary<FmgFileCategory, FMGFileSet> baseInfoBanks, Dictionary<FmgFileCategory, FMGFileSet> baseDlcInfoBanks)
        {
            var entries = new Dictionary<FmgIDType, List<FMG.Entry>>();

            // Populate entries with the project vanilla entries
            foreach (var bank in projectInfoBanks)
            {
                foreach (var fmgInfo in bank.Value.FmgInfos)
                {
                    foreach (var entry in fmgInfo.Fmg.Entries)
                    {
                        if (!entries.ContainsKey(fmgInfo.FmgID))
                        {
                            entries.Add(fmgInfo.FmgID, new List<FMG.Entry>());
                        }

                        entries[fmgInfo.FmgID].Add(entry);
                    }
                }
            }

            // Remove any matches from the entries if they are present in the vanilla FMGs
            foreach (var bank in baseInfoBanks)
            {
                foreach (var pEntry in entries)
                {
                    foreach (var fmgInfo in bank.Value.FmgInfos)
                    {
                        if (pEntry.Key == fmgInfo.FmgID)
                        {
                            foreach (var entry in fmgInfo.Fmg.Entries)
                            {
                                foreach (var pFmgEntry in pEntry.Value)
                                {
                                    if (entry.ID == pFmgEntry.ID)
                                    {
                                        entries[pEntry.Key].Remove(pFmgEntry);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Remove any matches from the entries if they are present in the vanilla DLC2 FMGs
            foreach (var bank in baseDlcInfoBanks)
            {
                foreach (var pEntry in entries)
                {
                    foreach (var fmgInfo in bank.Value.FmgInfos)
                    {
                        if (pEntry.Key == fmgInfo.FmgID)
                        {
                            foreach (var entry in fmgInfo.Fmg.Entries)
                            {
                                foreach (var pFmgEntry in pEntry.Value)
                                {
                                    if (entry.ID == pFmgEntry.ID)
                                    {
                                        entries[pEntry.Key].Remove(pFmgEntry);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Remove any matches from the entries if they are present in the project DLC2 FMGs
            foreach (var bank in projectDlcInfoBanks)
            {
                foreach (var pEntry in entries)
                {
                    foreach (var fmgInfo in bank.Value.FmgInfos)
                    {
                        if (pEntry.Key == fmgInfo.FmgID)
                        {
                            foreach (var entry in fmgInfo.Fmg.Entries)
                            {
                                foreach (var pFmgEntry in pEntry.Value)
                                {
                                    if (entry.ID == pFmgEntry.ID)
                                    {
                                        entries[pEntry.Key].Remove(pFmgEntry);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }


            return entries;
        }
    }
}
