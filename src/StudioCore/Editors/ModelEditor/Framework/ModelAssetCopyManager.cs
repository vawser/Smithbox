using HKLib.hk2018.hkAsyncThreadPool;
using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Resource.Locators;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using StudioCore.Core;

namespace StudioCore.Editors.ModelEditor;

public class ModelAssetCopyManager
{
    private ModelEditorScreen Editor;

    public ModelAssetCopyManager(ModelEditorScreen screen)
    {
        Editor = screen;
    }

    public bool IsSupportedProjectType()
    {
        if (Editor.Project.ProjectType is ProjectType.ER or ProjectType.AC6)
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
        Vector2 buttonSize = new Vector2(200, 24);

        if (ShowNewCharacterMenu)
        {
            ImGui.OpenPopup("Copy as New Character");
        }

        if (ImGui.BeginPopupModal("Copy as New Character", ref ShowNewCharacterMenu, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove))
        {
            ImGui.Text("Target Character:");
            UIHelper.DisplayAlias(SourceCharacterName);

            ImGui.Separator();

            ImGui.Text("New Character ID");
            ImGui.InputInt("##newChrId", ref NewCharacterID, 1);
            UIHelper.Tooltip("" +
                "The new ID the copied asset will have.\n\n" +
                "Character IDs must be between 0 and 9999 and not already exist.");

            if (ImGui.Button("Create", buttonSize))
            {
                bool createChr = true;

                string newChrIdStr = $"{NewCharacterID}";

                if (NewCharacterID < 1000)
                    newChrIdStr = $"0{NewCharacterID}";
                if (NewCharacterID < 100)
                    newChrIdStr = $"00{NewCharacterID}";
                if (NewCharacterID < 10)
                    newChrIdStr = $"000{NewCharacterID}";

                if (NewCharacterID >= 0 && NewCharacterID <= 9999)
                {
                    var matchChr = $"c{newChrIdStr}";

                    if (Editor.Project.Aliases.Characters.Any(x => x.ID == matchChr))
                    {
                        createChr = false;
                        PlatformUtils.Instance.MessageBox($"{matchChr} already exists.", "Warning", MessageBoxButtons.OK);
                    }
                }
                else
                {
                    createChr = false;
                    PlatformUtils.Instance.MessageBox($"{newChrIdStr} is not valid.", "Warning", MessageBoxButtons.OK);
                }

                if (createChr)
                {
                    CreateCharacter(SourceCharacterName, $"c{newChrIdStr}");
                    ShowNewCharacterMenu = false;
                }
            }

            ImGui.SameLine();

            if (ImGui.Button("Close", buttonSize))
            {
                ShowNewCharacterMenu = false;
            }

            ImGui.EndPopup();
        }
    }

    public void CreateCharacter(string copyChr, string newChr)
    {
        if (Editor.Project.ProjectType is ProjectType.ER or ProjectType.AC6)
        {
            // ChrBND
            ResourceDescriptor chrBnd = AssetLocator.GetCharacterBinder(Editor.Project, copyChr);
            if (chrBnd.AssetPath != null)
                SaveContainer(chrBnd.AssetPath, copyChr, newChr);

            // AniBND
            ResourceDescriptor aniBnd = AssetLocator.GetCharacterAnimationBinder(Editor.Project, copyChr);

            if (aniBnd.AssetPath != null)
                SaveContainer(aniBnd.AssetPath, copyChr, newChr);

            // AniBND _div0X
            for (int i = 0; i < 6; i++)
            {
                ResourceDescriptor aniBnd_div0X = AssetLocator.GetCharacterAnimationBinder(Editor.Project, copyChr, $"_div0{i}");

                if (aniBnd_div0X.AssetPath != null)
                    SaveContainer(aniBnd_div0X.AssetPath, copyChr, newChr);
            }

            // BehBND
            ResourceDescriptor behBnd = AssetLocator.GetCharacterBehaviorBinder(Editor.Project, copyChr);

            if (behBnd.AssetPath != null)
                SaveContainer(behBnd.AssetPath, copyChr, newChr);

            // TexBND _l
            ResourceDescriptor texBnd_l = AssetLocator.GetCharacterTextureBinder(Editor.Project, copyChr, "_l");

            if (texBnd_l.AssetPath != null)
                SaveContainer(texBnd_l.AssetPath, copyChr, newChr);

            // TexBND _h
            ResourceDescriptor texBnd_h = AssetLocator.GetCharacterTextureBinder(Editor.Project, copyChr, "_h");

            if (texBnd_h.AssetPath != null)
                SaveContainer(texBnd_h.AssetPath, copyChr, newChr);
        }
    }


    private string SourceAssetName = "";
    private int NewAssetCategoryID = -1;
    private int NewAssetID = -1;
    private bool ShowNewAssetMenu = false;

    public void OpenAssetCopyMenu(string entry)
    {
        SourceAssetName = entry;

        var assetNameSegments = SourceAssetName.Split("_");
        if (assetNameSegments.Length > 1)
        {
            try
            {
                NewAssetCategoryID = int.Parse(assetNameSegments[0]);
            }
            catch(Exception e)
            {
                TaskLogs.AddLog("Failed to convert NewAssetCategoryID string to int.", LogLevel.Error, Tasks.LogPriority.High, e);
            }
        }

        ShowNewAssetMenu = true;
    }

    public void AssetCopyMenu()
    {
        Vector2 buttonSize = new Vector2(200, 24);

        if (ShowNewAssetMenu)
        {
            ImGui.OpenPopup("Copy as New Asset");
        }

        if (ImGui.BeginPopupModal("Copy as New Asset", ref ShowNewAssetMenu, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove))
        {
            ImGui.Text("Target Asset:");
            UIHelper.DisplayAlias(SourceAssetName);

            ImGui.Separator();

            if (Editor.Project.ProjectType is ProjectType.ER or ProjectType.AC6)
            {
                ImGui.Text("New Asset Category ID");
                ImGui.InputInt("##newAssetCategoryId", ref NewAssetCategoryID, 1);
                UIHelper.Tooltip("" +
                    "The category ID the copied asset will have.\n\n" +
                    "Asset category IDs must be between 0 and 999.");
            }

            ImGui.Text("New Asset ID");
            ImGui.InputInt("##newAssetId", ref NewAssetID, 1);
            UIHelper.Tooltip("" +
                "The asset ID the copied asset will have.\n\n" +
                "Asset IDs must be between 0 and 999.");

            if (ImGui.Button("Create", buttonSize))
            {
                bool createAsset = true;

                var prefix = "";
                var matchAsset = "";

                if (Editor.Project.ProjectType is ProjectType.ER or ProjectType.AC6)
                {
                    prefix = "aeg";

                    string newAssetCategoryIdStr = $"{NewAssetCategoryID}";
                    if (NewAssetCategoryID < 100)
                        newAssetCategoryIdStr = $"0{NewAssetCategoryID}";
                    if (NewAssetCategoryID < 10)
                        newAssetCategoryIdStr = $"00{NewAssetCategoryID}";

                    string newAssetIdStr = $"{NewAssetID}";
                    if (NewAssetID < 100)
                        newAssetIdStr = $"0{NewAssetID}";
                    if (NewAssetID < 10)
                        newAssetIdStr = $"00{NewAssetID}";

                    matchAsset = $"{newAssetCategoryIdStr}_{newAssetIdStr}";

                    if (matchAsset != "" &&
                        NewAssetID >= 0 && NewAssetID <= 999 &&
                        NewAssetCategoryID >= 0 && NewAssetCategoryID <= 999)
                    {
                        if (Editor.Project.Aliases.Assets.Any(x => x.ID == matchAsset))
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
                }
                else
                {
                    prefix = "o";

                    string newAssetIdStr = $"{NewAssetID}";
                    if (NewAssetID < 100000)
                        newAssetIdStr = $"0{NewAssetID}";
                    if (NewAssetID < 10000)
                        newAssetIdStr = $"00{NewAssetID}";
                    if (NewAssetID < 1000)
                        newAssetIdStr = $"000{NewAssetID}";
                    if (NewAssetID < 100)
                        newAssetIdStr = $"0000{NewAssetID}";
                    if (NewAssetID < 10)
                        newAssetIdStr = $"00000{NewAssetID}";

                    matchAsset = $"{newAssetIdStr}";

                    if (matchAsset != "" &&
                        NewAssetID >= 0 && NewAssetID <= 999999)
                    {
                        if (Editor.Project.Aliases.Assets.Any(x => x.ID == matchAsset))
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
                }

                if (createAsset)
                {
                    CreateAsset(SourceAssetName, $"{prefix}{matchAsset}");
                    ShowNewAssetMenu = false;
                }
            }

            ImGui.SameLine();

            if (ImGui.Button("Close", buttonSize))
            {
                ShowNewAssetMenu = false;
            }

            ImGui.EndPopup();
        }
    }

    public void CreateAsset(string copyAsset, string newAsset)
    {
        if (Editor.Project.ProjectType is ProjectType.ER or ProjectType.AC6)
        {
            // GeomBND
            ResourceDescriptor assetGeom = AssetLocator.GetAssetGeomBinder(Editor.Project, copyAsset);
            if (assetGeom.AssetPath != null)
                SaveContainer(assetGeom.AssetPath, copyAsset, newAsset, true);

            // GeomHKXBND _l
            ResourceDescriptor assetGeomHKX_l = AssetLocator.GetAssetGeomHKXBinder(Editor.Project, copyAsset, "_l");

            if (assetGeomHKX_l.AssetPath != null)
                SaveContainer(assetGeomHKX_l.AssetPath, copyAsset, newAsset, true);

            // GeomHKXBND _h
            ResourceDescriptor assetGeomHKX_h = AssetLocator.GetAssetGeomHKXBinder(Editor.Project, copyAsset, "_h");

            if (assetGeomHKX_h.AssetPath != null)
                SaveContainer(assetGeomHKX_h.AssetPath, copyAsset, newAsset, true);
        }
    }

    private string SourcePartName = "";
    private string NewPartType = "";
    private string NewPartGender = "";
    private int NewPartID = -1;
    private bool ShowNewPartMenu = false;

    public void OpenPartCopyMenu(string entry)
    {
        SourcePartName = entry;

        var partNameSegments = SourcePartName.Split("_");
        if(partNameSegments.Length > 2)
        {
            NewPartType = partNameSegments[0];
            NewPartGender = partNameSegments[1];
        }

        ShowNewPartMenu = true;
    }

    public void PartCopyMenu()
    {
        Vector2 buttonSize = new Vector2(200, 24);

        if (ShowNewPartMenu)
        {
            ImGui.OpenPopup("Copy as New Part");
        }

        if (ImGui.BeginPopupModal("Copy as New Part", ref ShowNewPartMenu, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove))
        {
            ImGui.Text("Target Part:");
            UIHelper.DisplayAlias(SourcePartName);

            ImGui.Separator();

            ImGui.Text("New Part Type");
            ImGui.InputText("##newPartType", ref NewPartType, 255);
            UIHelper.Tooltip("" +
                "The part type string the copied part will have.\n\n" +
                "Part Type string should be hd, fc, bd, am or lg in most cases.");

            ImGui.Text("New Part Gender");
            ImGui.InputText("##newPartGender", ref NewPartGender, 255);
            UIHelper.Tooltip("" +
                "The part gender string the copied part will have.\n\n" +
                "Part Gender string should be m, f or a most cases.");

            ImGui.Text("New Part ID");
            ImGui.InputInt("##newPartId", ref NewPartID, 1);
            UIHelper.Tooltip("" +
                "The part ID the copied part will have.\n\n" +
                "Part IDs must be between 0 and 9999.");

            if (ImGui.Button("Create", buttonSize))
            {
                bool createPart = true;

                string newPartIdStr = $"{NewPartID}";

                if (NewPartID < 1000)
                    newPartIdStr = $"0{NewPartID}";
                if (NewPartID < 100)
                    newPartIdStr = $"00{NewPartID}";
                if (NewPartID < 10)
                    newPartIdStr = $"000{NewPartID}";

                var matchPart = $"{NewPartType}_{NewPartGender}_{newPartIdStr}";

                if (NewPartID >= 0 && NewPartID <= 9999)
                {
                    if (Editor.Project.Aliases.Parts.Any(x => x.ID == matchPart))
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

            if (ImGui.Button("Close", buttonSize))
            {
                ShowNewPartMenu = false;
            }

            ImGui.EndPopup();
        }
    }

    public void CreatePart(string copyPart, string newPart)
    {
        if (Editor.Project.ProjectType is ProjectType.ER or ProjectType.AC6)
        {
            // PartBND
            ResourceDescriptor partBnd = AssetLocator.GetPartBinder(Editor.Project, copyPart);
            if (partBnd.AssetPath != null)
                SaveContainer(partBnd.AssetPath, copyPart, newPart, true);

            // PartBND _l
            ResourceDescriptor partBnd_l = AssetLocator.GetPartBinder(Editor.Project, copyPart, "_l");
            if (partBnd_l.AssetPath != null)
                SaveContainer(partBnd_l.AssetPath, copyPart, newPart, true);

            // PartBND _u
            ResourceDescriptor partBnd_u = AssetLocator.GetPartBinder(Editor.Project, copyPart, "_u");
            if (partBnd_u.AssetPath != null)
                SaveContainer(partBnd_u.AssetPath, copyPart, newPart, true);
        }

        if (Editor.Project.ProjectType is ProjectType.AC6)
        {
            // TPF
            ResourceDescriptor partTpf = AssetLocator.GetPartTpf(Editor.Project, copyPart, "");
            if (partTpf.AssetPath != null)
                SaveFile(partTpf.AssetPath, copyPart, newPart, true);

            // TPF _l
            ResourceDescriptor partTpf_l = AssetLocator.GetPartTpf(Editor.Project, copyPart, "_l");
            if (partTpf_l.AssetPath != null)
                SaveFile(partTpf_l.AssetPath, copyPart, newPart, true);

            // TPF _u
            ResourceDescriptor partTpf_u = AssetLocator.GetPartTpf(Editor.Project, copyPart, "_u");
            if (partTpf_u.AssetPath != null)
                SaveFile(partTpf_u.AssetPath, copyPart, newPart, true);
        }
    }


    private string SourceMapPieceName = "";
    private string NewMapId = "";
    private int NewMapPieceID = -1;
    private bool ShowNewMapPieceMenu = false;

    public void OpenMapPieceCopyMenu(string entry)
    {
        SourceMapPieceName = entry;

        var nameSegments = SourcePartName.Split("_");
        if (nameSegments.Length > 2)
        {
            SourceMapPieceName = nameSegments.Last();
        }

        ShowNewMapPieceMenu = true;
    }

    public void MapPieceCopyMenu()
    {
        Vector2 buttonSize = new Vector2(200, 24);

        if (ShowNewMapPieceMenu)
        {
            ImGui.OpenPopup("Copy as New Map Piece");
        }

        if (ImGui.BeginPopupModal("Copy as New Map Piece", ref ShowNewMapPieceMenu, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove))
        {
            ImGui.Text("Target Map Piece:");
            UIHelper.DisplayAlias(SourceMapPieceName);

            ImGui.Separator();

            ImGui.Text("New Map ID");
            ImGui.InputText("##newMapId", ref NewMapId, 255);
            UIHelper.Tooltip("" +
                "The map ID string the copied map piece will have.\n\n" +
                "This should match the map ID of the map you want the map piece to work in.");

            ImGui.Text("New Map Piece ID");
            ImGui.InputInt("##newMapPieceID", ref NewMapPieceID, 1);
            UIHelper.Tooltip("" +
                "The map piece ID the copied map piece will have.\n\n" +
                "Map Piece IDs must be between 0 and 999999.");

            if (ImGui.Button("Create", buttonSize))
            {
                bool createMapPiece = true;

                string newMapPieceStr = $"{NewMapPieceID}";

                if (NewMapPieceID < 10000)
                    newMapPieceStr = $"0{NewMapPieceID}";
                if (NewMapPieceID < 10000)
                    newMapPieceStr = $"00{NewMapPieceID}";
                if (NewMapPieceID < 1000)
                    newMapPieceStr = $"000{NewMapPieceID}";
                if (NewMapPieceID < 100)
                    newMapPieceStr = $"0000{NewMapPieceID}";
                if (NewMapPieceID < 10)
                    newMapPieceStr = $"00000{NewMapPieceID}";

                var matchMapPiece = $"{NewMapId}_{newMapPieceStr}";

                if (NewMapPieceID >= 0 && NewMapPieceID <= 999999)
                {
                    if (Editor.Project.Aliases.MapPieces.Any(x => x.ID == matchMapPiece))
                    {
                        createMapPiece = false;
                        PlatformUtils.Instance.MessageBox($"{matchMapPiece} already exists.", "Warning", MessageBoxButtons.OK);
                    }
                }
                else if (NewMapId.Length != 12)
                {
                    createMapPiece = false;
                    PlatformUtils.Instance.MessageBox($"{NewMapId} is not in the correct format: mXX_XX_XX_XX", "Warning", MessageBoxButtons.OK);
                }
                else
                {
                    createMapPiece = false;
                    PlatformUtils.Instance.MessageBox($"{matchMapPiece} is not valid.", "Warning", MessageBoxButtons.OK);
                }

                if (createMapPiece)
                {
                    CreateMapPiece(SourceMapPieceName, $"{matchMapPiece}", NewMapId);
                    ShowNewMapPieceMenu = false;
                }
            }

            ImGui.SameLine();

            if (ImGui.Button("Close", buttonSize))
            {
                ShowNewMapPieceMenu = false;
            }

            ImGui.EndPopup();
        }
    }

    public void CreateMapPiece(string copyMapPiece, string newMapPiece, string mapId)
    {
        // Get the first section of a map id, e.g. m10
        var topMapId = mapId.Substring(0, 3);

        var dir = $"{mapId}";

        if(Editor.Project.ProjectType is ProjectType.ER or ProjectType.AC6)
            dir = $@"{topMapId}\{mapId}";

        ResourceDescriptor partBnd = AssetLocator.GetMapPiece(Editor.Project, dir, copyMapPiece);
        if (partBnd.AssetPath != null)
            SaveContainer(partBnd.AssetPath, copyMapPiece, newMapPiece, true);

    }

    private void SaveFile(string binderPath, string oldId, string newId, bool uppercaseReplace = false)
    {
        var rootFilePath = binderPath;
        var newFilePath = binderPath.Replace(oldId, newId);
        newFilePath = newFilePath.Replace(Editor.Project.DataPath, Editor.Project.ProjectPath);

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
        newBinderPath = newBinderPath.Replace(Editor.Project.DataPath, Editor.Project.ProjectPath);

        var newBinderDirectory = Path.GetDirectoryName(newBinderPath);

        if (!Directory.Exists(newBinderDirectory))
        {
            Directory.CreateDirectory(newBinderDirectory);
        }

        if (Editor.Project.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
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

                switch (Editor.Project.ProjectType)
                {
                    case ProjectType.DS1:
                    case ProjectType.DS1R:
                        fileBytes = writeBinder.Write(DCX.Type.DCX_DFLT_10000_24_9);
                        break;
                    default:
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

                switch (Editor.Project.ProjectType)
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
