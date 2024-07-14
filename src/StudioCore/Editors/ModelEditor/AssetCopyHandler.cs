using HKLib.hk2018.hkAsyncThreadPool;
using ImGuiNET;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Interface;
using StudioCore.Locators;
using StudioCore.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor
{
    public class AssetCopyHandler
    {
        private ModelEditorScreen Screen;

        public AssetCopyHandler(ModelEditorScreen screen)
        {
            Screen = screen;
        }

        public bool IsSupportedProjectType()
        {
            if(Smithbox.ProjectType is ProjectType.ER or ProjectType.AC6)
            {
                return true;
            }

            return false;
        }

        private string SourceCharacterName = "";
        private int NewCharacterID = -1;
        private bool ShowNewCharacterMenu = false;

        public void OpenCharacterCopyMenu(string entry)
        {
            SourceCharacterName = entry;
            ShowNewCharacterMenu = true;
        }

        public void CharacterCopyMenu()
        {
            if (ShowNewCharacterMenu)
            {
                ImGui.OpenPopup("Copy as New Character");
            }

            if (ImGui.BeginPopupModal("Copy as New Character", ref ShowNewCharacterMenu, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove))
            {
                ImGui.Text("Source Character");
                var name = $"{SourceCharacterName} : {AliasUtils.GetCharacterAlias(SourceCharacterName)}";
                ImGui.InputText("sourceName", ref name, 255, ImGuiInputTextFlags.ReadOnly);
                ImGui.Text("");

                ImGui.Text("New Character ID");
                ImGui.InputInt("##newChrId", ref NewCharacterID, 1);

                if (ImGui.Button("Create"))
                {
                    bool createChr = true;

                    if (NewCharacterID > 0 && NewCharacterID < 9999)
                    {
                        var matchChr = $"c{NewCharacterID}";

                        if (Smithbox.BankHandler.CharacterAliases.Aliases.list.Any(x => x.id == matchChr))
                        {
                            createChr = false;
                            PlatformUtils.Instance.MessageBox($"{matchChr} already exists.", "Warning", MessageBoxButtons.OK);
                        }
                    }
                    else
                    {
                        createChr = false;
                        PlatformUtils.Instance.MessageBox($"{NewCharacterID} is not valid.", "Warning", MessageBoxButtons.OK);
                    }

                    if (createChr)
                    {
                        CreateCharacter(SourceCharacterName, $"c{NewCharacterID}");
                        ShowNewCharacterMenu = false;
                    }
                }
                ImGui.SameLine();
                if (ImGui.Button("Close"))
                {
                    ShowNewCharacterMenu = false;
                }

                ImGui.EndPopup();
            }
        }

        public void CreateCharacter(string copyChr, string newChr)
        {
            if (Smithbox.ProjectType is ProjectType.ER or ProjectType.AC6)
            {
                // ChrBND
                ResourceDescriptor chrBnd = AssetLocator.GetCharacterBinder(copyChr);
                if(chrBnd.AssetPath != null)
                    SaveContainer(chrBnd.AssetPath, copyChr, newChr);

                // AniBND
                ResourceDescriptor aniBnd = AssetLocator.GetCharacterAnimationBinder(copyChr);

                if (aniBnd.AssetPath != null)
                    SaveContainer(aniBnd.AssetPath, copyChr, newChr);

                // AniBND _div0X
                for(int i = 0; i < 6; i++)
                {
                    ResourceDescriptor aniBnd_div0X = AssetLocator.GetCharacterAnimationBinder(copyChr, $"_div0{i}");

                    if (aniBnd_div0X.AssetPath != null)
                        SaveContainer(aniBnd_div0X.AssetPath, copyChr, newChr);
                }

                // BehBND
                ResourceDescriptor behBnd = AssetLocator.GetCharacterBehaviorBinder(copyChr);

                if (behBnd.AssetPath != null)
                    SaveContainer(behBnd.AssetPath, copyChr, newChr);

                // TexBND _l
                ResourceDescriptor texBnd_l = AssetLocator.GetCharacterTextureBinder(copyChr, "_l");

                if (texBnd_l.AssetPath != null)
                    SaveContainer(texBnd_l.AssetPath, copyChr, newChr);

                // TexBND _h
                ResourceDescriptor texBnd_h = AssetLocator.GetCharacterTextureBinder(copyChr, "_h");

                if (texBnd_h.AssetPath != null)
                    SaveContainer(texBnd_h.AssetPath, copyChr, newChr);
            }

            // Reload banks so the addition appears in the lists
            Smithbox.BankHandler.ReloadAliasBanks = true;
            Smithbox.AliasCacheHandler.ReloadAliasCaches = true;
        }


        private string SourceAssetName = "";
        private int NewAssetID = -1;
        private bool ShowNewAssetMenu = false;

        public void OpenAssetCopyMenu(string entry)
        {
            SourceAssetName = entry;
            ShowNewAssetMenu = true;
        }

        public void AssetCopyMenu()
        {
            if (ShowNewAssetMenu)
            {
                ImGui.OpenPopup("Copy as New Asset");
            }

            if (ImGui.BeginPopupModal("Copy as New Asset", ref ShowNewAssetMenu, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove))
            {
                ImGui.Text("Source Asset");
                var name = $"{SourceAssetName} : {AliasUtils.GetAssetAlias(SourceAssetName)}";
                ImGui.InputText("sourceName", ref name, 255, ImGuiInputTextFlags.ReadOnly);
                ImGui.Text("");

                ImGui.Text("New Asset ID");
                ImGui.InputInt("##newAssetId", ref NewAssetID, 1);

                if (ImGui.Button("Create"))
                {
                    bool createAsset = true;

                    string newAssetIdStr = $"{NewAssetID}";
                    if (NewAssetID < 100)
                        newAssetIdStr = $"0{NewAssetID}";
                    if (NewAssetID < 10)
                        newAssetIdStr = $"00{NewAssetID}";

                    var matchAsset = $"{SourceAssetName.Substring(0, 6)}_{newAssetIdStr}";

                    if (NewAssetID > 0 && NewAssetID < 999)
                    {
                        if (Smithbox.BankHandler.AssetAliases.Aliases.list.Any(x => x.id == matchAsset))
                        {
                            createAsset = false;
                            PlatformUtils.Instance.MessageBox($"{matchAsset} already exists.", "Warning", MessageBoxButtons.OK);
                        }
                    }
                    else
                    {
                        createAsset = false;
                        PlatformUtils.Instance.MessageBox($"{matchAsset} is not valid.", "Warning", MessageBoxButtons.OK);
                    }

                    if (createAsset)
                    {
                        CreateAsset(SourceAssetName, $"{matchAsset}");
                        ShowNewAssetMenu = false;
                    }
                }
                ImGui.SameLine();
                if (ImGui.Button("Close"))
                {
                    ShowNewAssetMenu = false;
                }

                ImGui.EndPopup();
            }
        }

        public void CreateAsset(string copyAsset, string newAsset)
        {
            if (Smithbox.ProjectType is ProjectType.ER or ProjectType.AC6)
            {
                // GeomBND
                ResourceDescriptor assetGeom = AssetLocator.GetAssetGeomBinder(copyAsset);
                if (assetGeom.AssetPath != null)
                    SaveContainer(assetGeom.AssetPath, copyAsset, newAsset, true);

                // GeomHKXBND _l
                ResourceDescriptor assetGeomHKX_l = AssetLocator.GetAssetGeomHKXBinder(copyAsset, "_l");

                if (assetGeomHKX_l.AssetPath != null)
                    SaveContainer(assetGeomHKX_l.AssetPath, copyAsset, newAsset, true);

                // GeomHKXBND _h
                ResourceDescriptor assetGeomHKX_h = AssetLocator.GetAssetGeomHKXBinder(copyAsset, "_h");

                if (assetGeomHKX_h.AssetPath != null)
                    SaveContainer(assetGeomHKX_h.AssetPath, copyAsset, newAsset, true);
            }

            // Reload banks so the addition appears in the lists
            Smithbox.BankHandler.ReloadAliasBanks = true;
            Smithbox.AliasCacheHandler.ReloadAliasCaches = true;
        }

        private string SourcePartName = "";
        private int NewPartID = -1;
        private bool ShowNewPartMenu = false;

        public void OpenPartCopyMenu(string entry)
        {
            SourcePartName = entry;
            ShowNewPartMenu = true;
        }

        public void PartCopyMenu()
        {
            if (ShowNewPartMenu)
            {
                ImGui.OpenPopup("Copy as New Part");
            }

            if (ImGui.BeginPopupModal("Copy as New Part", ref ShowNewPartMenu, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove))
            {
                ImGui.Text("Source Part");
                var name = $"{SourcePartName} : {AliasUtils.GetPartAlias(SourcePartName)}";
                ImGui.InputText("sourceName", ref name, 255, ImGuiInputTextFlags.ReadOnly);
                ImGui.Text("");

                ImGui.Text("New Part ID");
                ImGui.InputInt("##newPartId", ref NewPartID, 1);

                if (ImGui.Button("Create"))
                {
                    bool createPart = true;

                    var oldPartId = Regex.Match(SourcePartName, @"[0-9]{4}").Groups[0].Value;
                    var matchPart = $"{SourcePartName.Replace(oldPartId, NewPartID.ToString())}";

                    if (NewPartID > 0 && NewPartID < 9999)
                    {
                        if (Smithbox.BankHandler.PartAliases.Aliases.list.Any(x => x.id == matchPart))
                        {
                            createPart = false;
                            PlatformUtils.Instance.MessageBox($"{matchPart} already exists.", "Warning", MessageBoxButtons.OK);
                        }
                    }
                    else
                    {
                        createPart = false;
                        PlatformUtils.Instance.MessageBox($"{matchPart} is not valid.", "Warning", MessageBoxButtons.OK);
                    }

                    if (createPart)
                    {
                        CreatePart(SourcePartName, $"{matchPart}");
                        ShowNewPartMenu = false;
                    }
                }
                ImGui.SameLine();
                if (ImGui.Button("Close"))
                {
                    ShowNewPartMenu = false;
                }

                ImGui.EndPopup();
            }
        }

        public void CreatePart(string copyPart, string newPart)
        {
            if (Smithbox.ProjectType is ProjectType.ER or ProjectType.AC6)
            {
                // PartBND
                ResourceDescriptor partBnd = AssetLocator.GetPartBinder(copyPart);
                if (partBnd.AssetPath != null)
                    SaveContainer(partBnd.AssetPath, copyPart, newPart, true);

                // PartBND _l
                ResourceDescriptor partBnd_l = AssetLocator.GetPartBinder(copyPart, "_l");
                if (partBnd_l.AssetPath != null)
                    SaveContainer(partBnd_l.AssetPath, copyPart, newPart, true);

                // PartBND _u
                ResourceDescriptor partBnd_u = AssetLocator.GetPartBinder(copyPart, "_u");
                if (partBnd_u.AssetPath != null)
                    SaveContainer(partBnd_u.AssetPath, copyPart, newPart, true);
            }

            if(Smithbox.ProjectType is ProjectType.AC6)
            {
                // TPF
                ResourceDescriptor partTpf = AssetLocator.GetPartTpf(copyPart, "");
                if (partTpf.AssetPath != null)
                    SaveFile(partTpf.AssetPath, copyPart, newPart, true);

                // TPF _l
                ResourceDescriptor partTpf_l = AssetLocator.GetPartTpf(copyPart, "_l");
                if (partTpf_l.AssetPath != null)
                    SaveFile(partTpf_l.AssetPath, copyPart, newPart, true);

                // TPF _u
                ResourceDescriptor partTpf_u = AssetLocator.GetPartTpf(copyPart, "_u");
                if (partTpf_u.AssetPath != null)
                    SaveFile(partTpf_u.AssetPath, copyPart, newPart, true);
            }

            // Reload banks so the addition appears in the lists
            Smithbox.BankHandler.ReloadAliasBanks = true;
            Smithbox.AliasCacheHandler.ReloadAliasCaches = true;
        }

        public void CreateMapPiece(string copyMapPiece, string newMapPiece, string mapId)
        {
        }

        private void SaveFile(string binderPath, string oldId, string newId, bool uppercaseReplace = false)
        {
            var rootFilePath = binderPath;
            var newFilePath = binderPath.Replace(oldId, newId);
            newFilePath = newFilePath.Replace(Smithbox.GameRoot, Smithbox.ProjectRoot);

            var newBinderDirectory = Path.GetDirectoryName(newFilePath);

            if (!Directory.Exists(newBinderDirectory))
            {
                Directory.CreateDirectory(newBinderDirectory);
            }

            File.Copy(rootFilePath, newFilePath, true);
        }

        private void SaveContainer(string binderPath, string oldId, string newId, bool uppercaseReplace = false)
        {
            var newBinderPath = binderPath.Replace(oldId, newId);
            newBinderPath = newBinderPath.Replace(Smithbox.GameRoot, Smithbox.ProjectRoot);

            var newBinderDirectory = Path.GetDirectoryName(newBinderPath);

            if(!Directory.Exists(newBinderDirectory))
            {
                Directory.CreateDirectory(newBinderDirectory);
            }

            if (Smithbox.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
            {
                byte[] fileBytes = null;

                using (IBinder binder = BND3.Read(DCX.Decompress(binderPath)))
                {
                    foreach (var file in binder.Files)
                    {
                        if (file.Name.ToLower().Contains(oldId.ToLower()))
                        {
                            if (uppercaseReplace)
                            {
                                file.Name = file.Name.Replace(oldId.ToUpper(), newId.ToUpper());
                            }
                            else
                            {
                                file.Name = file.Name.Replace(oldId, newId);
                            }
                        }
                    }

                    // Then write those bytes to file
                    BND3 writeBinder = binder as BND3;

                    switch (Smithbox.ProjectType)
                    {
                        case ProjectType.DS1:
                        case ProjectType.DS1R:
                            fileBytes = writeBinder.Write(DCX.Type.DCX_DFLT_10000_24_9);
                            break;
                        default:
                            TaskLogs.AddLog($"Invalid ProjectType during AssetCopy");
                            return;
                    }
                }

                if (fileBytes != null)
                {
                    File.WriteAllBytes(newBinderPath, fileBytes);
                }
            }
            else
            {
                byte[] fileBytes = null;

                using (IBinder binder = BND4.Read(DCX.Decompress(binderPath)))
                {
                    foreach (var file in binder.Files)
                    {
                        if (file.Name.ToLower().Contains(oldId.ToLower()))
                        {
                            if (uppercaseReplace)
                            {
                                file.Name = file.Name.Replace(oldId.ToUpper(), newId.ToUpper());
                            }
                            else
                            {
                                file.Name = file.Name.Replace(oldId, newId);
                            }
                        }
                    }

                    // Then write those bytes to file
                    BND4 writeBinder = binder as BND4;

                    switch (Smithbox.ProjectType)
                    {
                        case ProjectType.DS3:
                            fileBytes = writeBinder.Write(DCX.Type.DCX_DFLT_10000_44_9);
                            break;
                        case ProjectType.SDT:
                            fileBytes = writeBinder.Write(DCX.Type.DCX_KRAK);
                            break;
                        case ProjectType.ER:
                            fileBytes = writeBinder.Write(DCX.Type.DCX_KRAK);
                            break;
                        case ProjectType.AC6:
                            fileBytes = writeBinder.Write(DCX.Type.DCX_KRAK_MAX);
                            break;
                        default:
                            TaskLogs.AddLog($"Invalid ProjectType during AssetCopy");
                            return;
                    }
                }

                if (fileBytes != null)
                {
                    File.WriteAllBytes(newBinderPath, fileBytes);
                }
            }
        }
    }
}
