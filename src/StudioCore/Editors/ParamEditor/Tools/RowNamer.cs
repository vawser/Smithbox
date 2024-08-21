using Andre.Formats;
using StudioCore.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor.Tools;
public static class RowNamer
{
    public static bool MayUseRowNamer()
    {
        var selectedParam = Smithbox.EditorHandler.ParamEditor._activeView._selection;
        var activeParam = selectedParam.GetActiveParam();

        if (Smithbox.ProjectType is ProjectType.ER)
        {
            if (activeParam == "BehaviorParam" || activeParam == "BehaviorParam_PC")
            {
                return true;
            }
        }

        return false;
    }

    public static void AddNameToReferencedRow(Param param, Param.Row baseRow, string rowIDstr, string postfix)
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
            TaskLogs.AddLog($"Failed to parse {rowIDstr}: {ex.Message}");
        }
    }

    public static void ApplyRowNamer()
    {
        var selectedParam = Smithbox.EditorHandler.ParamEditor._activeView._selection;

        if (selectedParam.ActiveParamExists())
        {
            if (ParamBank.PrimaryBank.Params != null)
            {
                var activeParam = selectedParam.GetActiveParam();
                var rows = selectedParam.GetSelectedRows();

                HandleBehaviorParam(activeParam, rows);
                HandleBulletParam(activeParam, rows);
            }
        }
    }

    public static void HandleBehaviorParam(string activeParam, List<Param.Row> rows)
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
                            var AtkParam_Npc = ParamBank.PrimaryBank.Params.Where(e => e.Key == "AtkParam_Npc").FirstOrDefault();

                            if (AtkParam_Npc.Value != null)
                            {
                                var param = AtkParam_Npc.Value;
                                AddNameToReferencedRow(param, row, refID, "Attack");
                            }
                        }
                        else if (activeParam == "BehaviorParam_PC")
                        {
                            var AtkParam_PC = ParamBank.PrimaryBank.Params.Where(e => e.Key == "AtkParam_Pc").FirstOrDefault();
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
                        var Bullet = ParamBank.PrimaryBank.Params.Where(e => e.Key == "Bullet").FirstOrDefault();
                        if (Bullet.Value != null)
                        {
                            var param = Bullet.Value;
                            AddNameToReferencedRow(param, row, refID, "Bullet");
                        }
                    }
                    // SpEffect 
                    if (equipType == "2")
                    {
                        var SpEffectParam = ParamBank.PrimaryBank.Params.Where(e => e.Key == "SpEffectParam").FirstOrDefault();
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
                        TaskLogs.AddLog(chrID);
                        var result = Smithbox.BankHandler.CharacterAliases.Aliases.list.Where(e => e.id == chrID).FirstOrDefault();
                        if(result != null)
                        {
                            if (row.Name == "")
                            {
                                if (result.tags.Contains("sote"))
                                {
                                    row.Name = $"[DLC - {result.name}]";
                                }
                                else
                                {
                                    row.Name = $"[{result.name}]";
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public static void HandleBulletParam(string activeParam, List<Param.Row> rows)
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
                    var AtkParam_PC = ParamBank.PrimaryBank.Params.Where(e => e.Key == "AtkParam_Pc").FirstOrDefault();
                    if (AtkParam_PC.Value != null)
                    {
                        var param = AtkParam_PC.Value;
                        AddNameToReferencedRow(param, row, atkId_Bullet, "Bullet");
                    }

                    var AtkParam_Npc = ParamBank.PrimaryBank.Params.Where(e => e.Key == "AtkParam_Npc").FirstOrDefault();

                    if (AtkParam_Npc.Value != null)
                    {
                        var param = AtkParam_Npc.Value;
                        AddNameToReferencedRow(param, row, atkId_Bullet, "Bullet");
                    }

                    // Bullet
                    var Bullet = ParamBank.PrimaryBank.Params.Where(e => e.Key == "Bullet").FirstOrDefault();
                    if (Bullet.Value != null)
                    {
                        var param = Bullet.Value;
                        AddNameToReferencedRow(param, row, HitBulletID, "");
                        AddNameToReferencedRow(param, row, intervalCreateBulletId, "");
                    }

                    // SpEffect
                    var SpEffectParam = ParamBank.PrimaryBank.Params.Where(e => e.Key == "SpEffectParam").FirstOrDefault();
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

    public static void ApplyWeaponDeepRowNamer()
    {

    }
    public static void ApplyEnemyDeepRowNamer()
    {

    }
}
