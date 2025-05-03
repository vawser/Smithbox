using Andre.Formats;
using StudioCore.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor.Tools;
public class RowNamer
{
    public ParamEditorScreen Editor;

    public RowNamer(ParamEditorScreen editor)
    {
        Editor = editor;
    }

    public bool MayUseRowNamer()
    {
        var selectedParam = Editor._activeView._selection;
        var activeParam = selectedParam.GetActiveParam();

        if (Editor.Project.ProjectType is ProjectType.ER)
        {
            if (activeParam == "BehaviorParam" || activeParam == "BehaviorParam_PC")
            {
                return true;
            }
        }

        return false;
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

    public void ApplyRowNamer()
    {
        var selectedParam = Editor._activeView._selection;

        if (selectedParam.ActiveParamExists())
        {
            if (Editor.Project.ParamData.PrimaryBank.Params != null)
            {
                var activeParam = selectedParam.GetActiveParam();
                var rows = selectedParam.GetSelectedRows();

                HandleBehaviorParam(activeParam, rows);
                HandleBulletParam(activeParam, rows);
            }
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

    public void ApplyWeaponDeepRowNamer()
    {

    }
    public void ApplyEnemyDeepRowNamer()
    {

    }
}
