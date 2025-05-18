using Andre.Formats;
using Hexa.NET.ImGui;
using HKLib.hk2018.hkHashMapDetail;
using StudioCore.Core;
using StudioCore.Editors.ParamEditor.Decorators;
using StudioCore.Editors.TextEditor;
using StudioCore.Editors.TextEditor.Utils;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor.Tools;
public class ParamRowNamer
{
    public ParamEditorScreen Editor;

    public bool AffectItemNameOnly = false;

    public ParamRowNamer(ParamEditorScreen editor)
    {
        Editor = editor;
    }

    public void RowNamerMenu()
    {
        if (!Editor.Project.EnableTextEditor)
            return;

        if (!Editor._activeView.Selection.ActiveParamExists())
            return;

        if (Editor.Project.ProjectType is not ProjectType.ER)
            return;

        if (ImGui.BeginMenu("Row Namer"))
        {
            var selectedParam = Editor._activeView.Selection;
            var activeParam = selectedParam.GetActiveParam();

            if (activeParam == "BehaviorParam" || activeParam == "BehaviorParam_PC")
            {
                if (ImGui.MenuItem("Behavior"))
                {
                    if (Editor.Project.ParamData.PrimaryBank.Params != null)
                    {
                        var rows = selectedParam.GetSelectedRows();

                        HandleBehaviorParam(activeParam, rows);
                        HandleBulletParam(activeParam, rows);
                    }
                }
            }
            else if(activeParam == "ItemLotParam_map")
            {
                if (ImGui.MenuItem("Item Lots: Map"))
                {
                    if (Editor.Project.ParamData.PrimaryBank.Params != null)
                    {
                        var rows = selectedParam.GetSelectedRows();

                        HandleItemLotParamMap(activeParam, rows);
                    }
                }
            }
            else if (activeParam == "ItemLotParam_enemy")
            {
                if (ImGui.MenuItem("Item Lots: Enemy"))
                {
                    if (Editor.Project.ParamData.PrimaryBank.Params != null)
                    {
                        var rows = selectedParam.GetSelectedRows();

                        HandleItemLotParamEnemy(activeParam, rows);
                    }
                }
            }
            else
            {
                ImGui.Text("Current param is not supported in the row namer.");
            }

            ImGui.Separator();

            ImGui.Checkbox("Affect Item Name Only", ref AffectItemNameOnly);

            ImGui.EndMenu();
        }
    }

    public void AddNameToReferencedRow(Param param, Param.Row baseRow, string rowIDstr, string postfix)
    {
        try
        {
            var rowID = int.Parse(rowIDstr);
            var targetRow = param.Rows.Where(e => e.ID == rowID).FirstOrDefault();

            if (targetRow != null)
            {
                if (targetRow.Name == "")
                {
                    var baseName = baseRow.Name;

                    // Remove DLC aspect when adding name to referenced row
                    if(baseName.Contains("DLC -"))
                    {
                        baseName = baseName.Replace("DLC -", "");
                    }

                    targetRow.Name = $"{baseRow.Name} {postfix}";
                }
            }
        }
        catch (Exception ex)
        {
            TaskLogs.AddLog($"Failed to parse referenced row string: {rowIDstr}\n{ex.Message}");
        }
    }

    public void HandleBehaviorParam(string activeParam, List<Param.Row> rows)
    {
        if (activeParam == "BehaviorParam" || activeParam == "BehaviorParam_PC")
        {
            foreach (var row in rows)
            {
                var refID = $"{row["refId"].Value.Value}";
                var equipType = $"{row["refType"].Value.Value}";

                // Ignore if the source row name is empty
                if (row.Name != "")
                {
                    // Attack 
                    if (equipType == "0")
                    {
                        if (activeParam == "BehaviorParam")
                        {
                            var AtkParam_Npc = Editor.Project.ParamData.PrimaryBank.Params.Where(e => e.Key == "AtkParam_Npc").FirstOrDefault();

                            if (AtkParam_Npc.Value != null)
                            {
                                var param = AtkParam_Npc.Value;
                                AddNameToReferencedRow(param, row, refID, "Attack");
                            }
                        }
                        else if (activeParam == "BehaviorParam_PC")
                        {
                            var AtkParam_PC = Editor.Project.ParamData.PrimaryBank.Params.Where(e => e.Key == "AtkParam_Pc").FirstOrDefault();
                            if (AtkParam_PC.Value != null)
                            {
                                var param = AtkParam_PC.Value;
                                AddNameToReferencedRow(param, row, refID, "Attack");
                            }
                        }
                    }
                    // Bullet 
                    if (equipType == "1")
                    {
                        var Bullet = Editor.Project.ParamData.PrimaryBank.Params.Where(e => e.Key == "Bullet").FirstOrDefault();
                        if (Bullet.Value != null)
                        {
                            var param = Bullet.Value;
                            AddNameToReferencedRow(param, row, refID, "Bullet");
                        }
                    }
                    // SpEffect 
                    if (equipType == "2")
                    {
                        var SpEffectParam = Editor.Project.ParamData.PrimaryBank.Params.Where(e => e.Key == "SpEffectParam").FirstOrDefault();
                        if (SpEffectParam.Value != null)
                        {
                            var param = SpEffectParam.Value;
                            AddNameToReferencedRow(param, row, refID, "SpEffect");
                        }
                    }
                }
                else
                {
                    // If empty, try and determine suitable name
                    if (activeParam == "BehaviorParam" && row.ID > 100000)
                    {
                        var chrID = $"c{row.ID.ToString().Substring(1, 4)}";

                        var result = Editor.Project.Aliases.Characters.Where(e => e.ID == chrID).FirstOrDefault();
                        if(result != null)
                        {
                            if (row.Name == "")
                            {
                                if (result.Tags.Contains("sote"))
                                {
                                    row.Name = $"[DLC - {result.Name}]";
                                }
                                else
                                {
                                    row.Name = $"[{result.Name}]";
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public void HandleBulletParam(string activeParam, List<Param.Row> rows)
    {
        if (activeParam == "Bullet")
        {
            foreach (var row in rows)
            {
                var atkId_Bullet = $"{row["atkId_Bullet"].Value.Value}";

                var HitBulletID = $"{row["HitBulletID"].Value.Value}";
                var intervalCreateBulletId = $"{row["intervalCreateBulletId"].Value.Value}";

                var spEffectIDForShooter = $"{row["spEffectIDForShooter"].Value.Value}";
                var spEffectId0 = $"{row["spEffectId0"].Value.Value}";
                var spEffectId1 = $"{row["spEffectId1"].Value.Value}";
                var spEffectId2 = $"{row["spEffectId2"].Value.Value}";
                var spEffectId3 = $"{row["spEffectId3"].Value.Value}";
                var spEffectId4 = $"{row["spEffectId4"].Value.Value}";

                // Ignore if the source row name is empty
                if (row.Name != "")
                {
                    // AtkParam
                    var AtkParam_PC = Editor.Project.ParamData.PrimaryBank.Params.Where(e => e.Key == "AtkParam_Pc").FirstOrDefault();
                    if (AtkParam_PC.Value != null)
                    {
                        var param = AtkParam_PC.Value;
                        AddNameToReferencedRow(param, row, atkId_Bullet, "Bullet");
                    }

                    var AtkParam_Npc = Editor.Project.ParamData.PrimaryBank.Params.Where(e => e.Key == "AtkParam_Npc").FirstOrDefault();

                    if (AtkParam_Npc.Value != null)
                    {
                        var param = AtkParam_Npc.Value;
                        AddNameToReferencedRow(param, row, atkId_Bullet, "Bullet");
                    }

                    // Bullet
                    var Bullet = Editor.Project.ParamData.PrimaryBank.Params.Where(e => e.Key == "Bullet").FirstOrDefault();
                    if (Bullet.Value != null)
                    {
                        var param = Bullet.Value;
                        AddNameToReferencedRow(param, row, HitBulletID, "");
                        AddNameToReferencedRow(param, row, intervalCreateBulletId, "");
                    }

                    // SpEffect
                    var SpEffectParam = Editor.Project.ParamData.PrimaryBank.Params.Where(e => e.Key == "SpEffectParam").FirstOrDefault();
                    if (SpEffectParam.Value != null)
                    {
                        var param = Bullet.Value;
                        AddNameToReferencedRow(param, row, spEffectIDForShooter, "");
                        AddNameToReferencedRow(param, row, spEffectId0, "");
                        AddNameToReferencedRow(param, row, spEffectId1, "");
                        AddNameToReferencedRow(param, row, spEffectId2, "");
                        AddNameToReferencedRow(param, row, spEffectId3, "");
                        AddNameToReferencedRow(param, row, spEffectId4, "");
                    }
                }
            }
        }
    }

    public int GetIntValue(string fieldName, Param.Row row)
    {
        var intValue = -1;

        var strValue = $"{row[fieldName].Value.Value}";
        intValue = int.Parse(strValue);

        return intValue;
    }

    public void HandleItemLotParamMap(string activeParam, List<Param.Row> rows)
    {
        var fmgs = Editor.Project.TextData.PrimaryBank.Entries;

        if (activeParam == "ItemLotParam_map")
        {
            var selectedRows = Editor._activeView.Selection.GetSelectedRows();

            foreach (var row in selectedRows)
            {
                var newName = GetName_ItemLotParamMap(row);

                if (newName != "")
                {
                    row.Name = newName;
                }
            }
        }
    }

    public string GetName_ItemLotParamMap(Param.Row row)
    {
        var mapNames = Editor.Project.Aliases.MapNames;

        var newName = "";
        var prefix = "";

        var rowID = $"{row.ID}";
        var existingPrefix = "";

        var match = Regex.Match(row.Name, @"(\[.*?\])");
        if (match.Success)
        {
            // Ignore the brackets used for some items
            if (match.Value.Length > 1)
            {
                existingPrefix = match.Value;
            }
        }

        // Open World Tiles
        if (rowID.Length == 10)
        {
            var mapAA = rowID.Substring(0, 2);
            var mapBB = rowID.Substring(2, 2);
            var mapCC = rowID.Substring(4, 2);

            if (mapAA == "10")
                mapAA = "60";

            if (mapAA == "20")
                mapAA = "61";

            var mapID = $"m{mapAA}_{mapBB}_{mapCC}_00";

            var nameMatch = mapNames.FirstOrDefault(e => e.ID == mapID);
            if (nameMatch != null)
            {
                if(nameMatch.Name.Contains(","))
                {
                    var splitName = nameMatch.Name.Split(',')[0];
                    prefix = $"[{splitName}] ";
                }
                else if (nameMatch.Name.Contains(";"))
                {
                    var splitName = nameMatch.Name.Split(';')[0];
                    prefix = $"[{splitName}] ";
                }
                else
                {
                    prefix = $"[{nameMatch.Name}] ";
                }
            }
        }

        // Legacy Dungeons
        if (rowID.Length == 8)
        {
            var mapAA = rowID.Substring(0, 2);
            var mapBB = rowID.Substring(2, 2);

            var mapID = $"m{mapAA}_{mapBB}_00_00";

            var nameMatch = mapNames.FirstOrDefault(e => e.ID == mapID);
            if (nameMatch != null)
            {
                if (nameMatch.Name.Contains(","))
                {
                    var splitName = nameMatch.Name.Split(',')[0];
                    prefix = $"[{splitName}] ";
                }
                else if (nameMatch.Name.Contains(";"))
                {
                    var splitName = nameMatch.Name.Split(';')[0];
                    prefix = $"[{splitName}] ";
                }
                else
                {
                    prefix = $"[{nameMatch.Name}] ";
                }
            }
        }

        var itemLot_1 = GetIntValue("lotItemId01", row);
        var itemLot_2 = GetIntValue("lotItemId02", row);
        var itemLot_3 = GetIntValue("lotItemId03", row);
        var itemLot_4 = GetIntValue("lotItemId04", row);
        var itemLot_5 = GetIntValue("lotItemId05", row);
        var itemLot_6 = GetIntValue("lotItemId06", row);
        var itemLot_7 = GetIntValue("lotItemId07", row);
        var itemLot_8 = GetIntValue("lotItemId08", row);

        var itemType_1 = GetIntValue("lotItemCategory01", row);
        var itemType_2 = GetIntValue("lotItemCategory02", row);
        var itemType_3 = GetIntValue("lotItemCategory03", row);
        var itemType_4 = GetIntValue("lotItemCategory04", row);
        var itemType_5 = GetIntValue("lotItemCategory05", row);
        var itemType_6 = GetIntValue("lotItemCategory06", row);
        var itemType_7 = GetIntValue("lotItemCategory07", row);
        var itemType_8 = GetIntValue("lotItemCategory08", row);

        List<string> names = new();

        var name1 = GetItemName(itemLot_1, itemType_1);
        if(name1 != "")
        {
            names.Add(name1);
        }
        var name2 = GetItemName(itemLot_2, itemType_2);
        if (name2 != "")
        {
            names.Add(name2);
        }
        var name3 = GetItemName(itemLot_3, itemType_3);
        if (name3 != "")
        {
            names.Add(name3);
        }
        var name4 = GetItemName(itemLot_4, itemType_4);
        if (name4 != "")
        {
            names.Add(name4);
        }
        var name5 = GetItemName(itemLot_5, itemType_5);
        if (name5 != "")
        {
            names.Add(name5);
        }
        var name6 = GetItemName(itemLot_6, itemType_6);
        if (name6 != "")
        {
            names.Add(name6);
        }
        var name7 = GetItemName(itemLot_7, itemType_7);
        if (name7 != "")
        {
            names.Add(name7);
        }
        var name8 = GetItemName(itemLot_8, itemType_8);
        if (name8 != "")
        {
            names.Add(name8);
        }

        newName = string.Join(", ", names);

        var finalName = $"{prefix}{newName}";

        if (AffectItemNameOnly)
        {
            if (existingPrefix == "")
            {
                finalName = $"{newName}";
            }
            else
            {
                finalName = $"{existingPrefix} {newName}";
            }
        }

        return finalName;
    }

    public void HandleItemLotParamEnemy(string activeParam, List<Param.Row> rows)
    {
        var fmgs = Editor.Project.TextData.PrimaryBank.Entries;

        if (activeParam == "ItemLotParam_enemy")
        {
            var selectedRows = Editor._activeView.Selection.GetSelectedRows();

            foreach (var row in selectedRows)
            {
                var newName = GetName_ItemLotParamEnemy(row);

                if (newName != "")
                {
                    row.Name = newName;
                }
            }
        }
    }

    public string GetName_ItemLotParamEnemy(Param.Row row)
    {
        var chrNames = Editor.Project.Aliases.Characters;

        var newName = "";
        var prefix = "";

        var rowID = $"{row.ID}";
        var existingPrefix = "";

        var match = Regex.Match(row.Name, @"(\[.*?\])");
        if (match.Success)
        {
            // Ignore the brackets used for some items
            if (match.Value.Length > 1)
            {
                existingPrefix = match.Value;
            }
        }

        // Enemy Name
        if (rowID.Length > 4)
        {
            var chrStr = rowID.Substring(0, 4);

            var chrID = $"c{chrStr}";

            var nameMatch = chrNames.FirstOrDefault(e => e.ID == chrID);
            if (nameMatch != null)
            {
                prefix = $"[{nameMatch.Name}] ";
            }
        }

        var itemLot_1 = GetIntValue("lotItemId01", row);
        var itemLot_2 = GetIntValue("lotItemId02", row);
        var itemLot_3 = GetIntValue("lotItemId03", row);
        var itemLot_4 = GetIntValue("lotItemId04", row);
        var itemLot_5 = GetIntValue("lotItemId05", row);
        var itemLot_6 = GetIntValue("lotItemId06", row);
        var itemLot_7 = GetIntValue("lotItemId07", row);
        var itemLot_8 = GetIntValue("lotItemId08", row);

        var itemType_1 = GetIntValue("lotItemCategory01", row);
        var itemType_2 = GetIntValue("lotItemCategory02", row);
        var itemType_3 = GetIntValue("lotItemCategory03", row);
        var itemType_4 = GetIntValue("lotItemCategory04", row);
        var itemType_5 = GetIntValue("lotItemCategory05", row);
        var itemType_6 = GetIntValue("lotItemCategory06", row);
        var itemType_7 = GetIntValue("lotItemCategory07", row);
        var itemType_8 = GetIntValue("lotItemCategory08", row);

        List<string> names = new();

        var name1 = GetItemName(itemLot_1, itemType_1);
        if (name1 != "")
        {
            names.Add(name1);
        }
        var name2 = GetItemName(itemLot_2, itemType_2);
        if (name2 != "")
        {
            names.Add(name2);
        }
        var name3 = GetItemName(itemLot_3, itemType_3);
        if (name3 != "")
        {
            names.Add(name3);
        }
        var name4 = GetItemName(itemLot_4, itemType_4);
        if (name4 != "")
        {
            names.Add(name4);
        }
        var name5 = GetItemName(itemLot_5, itemType_5);
        if (name5 != "")
        {
            names.Add(name5);
        }
        var name6 = GetItemName(itemLot_6, itemType_6);
        if (name6 != "")
        {
            names.Add(name6);
        }
        var name7 = GetItemName(itemLot_7, itemType_7);
        if (name7 != "")
        {
            names.Add(name7);
        }
        var name8 = GetItemName(itemLot_8, itemType_8);
        if (name8 != "")
        {
            names.Add(name8);
        }

        newName = string.Join(", ", names);

        var finalName = $"{prefix}{newName}";

        if (AffectItemNameOnly)
        {
            if (existingPrefix == "")
            {
                finalName = $"{newName}";
            }
            else
            {
                finalName = $"{existingPrefix} {newName}";
            }
        }

        return finalName;
    }

    public string GetItemName(int itemID, int itemCategory)
    {
        var newName = "";

        foreach (var (fileEntry, wrapper) in Editor.Project.TextData.PrimaryBank.Entries)
        {
            if (wrapper.ContainerDisplayCategory is not TextContainerCategory.English)
                continue;

            if (wrapper.FmgWrappers.Count == 0)
                continue;

            foreach (var fmgWrapper in wrapper.FmgWrappers)
            {
                var id = fmgWrapper.ID;
                var fmgName = fmgWrapper.Name;
                var internalName = TextUtils.GetFmgInternalName(Editor.Project, wrapper, id, fmgName);

                // Goods
                if (itemCategory == 1 && (internalName == "Title_Goods" || internalName == "Title_Goods_DLC1" || internalName == "Title_Goods_DLC2"))
                {
                    var match = fmgWrapper.File.Entries.FirstOrDefault(e => e.ID == itemID);
                    if (match != null)
                    {
                        newName = match.Text;
                        break;
                    }
                }

                // Weapons
                if (itemCategory == 2 && (internalName == "Title_Weapons" || internalName == "Title_Weapons_DLC1" || internalName == "Title_Weapons_DLC2"))
                {
                    var match = fmgWrapper.File.Entries.FirstOrDefault(e => e.ID == itemID);
                    if (match != null)
                    {
                        newName = match.Text;
                        break;
                    }
                }

                // Armor
                if (itemCategory == 3 && (internalName == "Title_Armor" || internalName == "Title_Armor_DLC1" || internalName == "Title_Armor_DLC2"))
                {
                    var match = fmgWrapper.File.Entries.FirstOrDefault(e => e.ID == itemID);
                    if (match != null)
                    {
                        newName = match.Text;
                        break;
                    }
                }

                // Accessories
                if (itemCategory == 4 && (internalName == "Title_Accessories" || internalName == "Title_Accessories_DLC1" || internalName == "Title_Accessories_DLC2"))
                {
                    var match = fmgWrapper.File.Entries.FirstOrDefault(e => e.ID == itemID);
                    if (match != null)
                    {
                        newName = match.Text;
                        break;
                    }
                }

                // Ash of War
                if (itemCategory == 5 && (internalName == "Title_Ash_of_War" || internalName == "Title_Ash_of_War_DLC1" || internalName == "Title_Ash_of_War_DLC2"))
                {
                    var match = fmgWrapper.File.Entries.FirstOrDefault(e => e.ID == itemID);
                    if (match != null)
                    {
                        newName = match.Text;
                        break;
                    }
                }
            }
        }

        return newName;
    }
}
