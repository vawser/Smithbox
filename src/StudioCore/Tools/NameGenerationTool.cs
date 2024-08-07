using Andre.Formats;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.HavokEditor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.TimeActEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Tools;

public static class NameGenerationTool
{
    private static AnimationBank.ContainerFileInfo PlayerTAEContainer;

    private static Param EquipWeaponParam;
    private static Param SwordArtsParam;
    private static Param BehaviorParam_PC;
    private static Param SpEffectParam;
    private static Param Bullet;
    private static Param AtkParam_Pc;

    public static void GenerateRowNames()
    {
        if (!AnimationBank.IsLoaded)
        {
            TaskManager.Run(
                    new TaskManager.LiveTask($"Setup Time Act Editor: Templates", TaskManager.RequeueType.None, false,
                () =>
                {
                    AnimationBank.LoadTimeActTemplates();
                }));

            TaskManager.Run(
                new TaskManager.LiveTask($"Setup Time Act Editor: Characters", TaskManager.RequeueType.None, false,
            () =>
            {
                AnimationBank.LoadProjectCharacterTimeActs();
            }));

            TaskManager.Run(
                new TaskManager.LiveTask($"Setup Time Act Editor: Objects", TaskManager.RequeueType.None, false,
            () =>
            {
                AnimationBank.LoadProjectObjectTimeActs();
            }));
        }
        if (!HavokFileBank.IsLoaded)
        {
            HavokFileBank.LoadAllHavokFiles();
        }

        PlayerTAEContainer = AnimationBank.FileChrBank.Where(e => e.Key.Name == "c0000").FirstOrDefault().Key;
        var playerContainer = HavokFileBank.BehaviorContainerBank.Where(e => e.Filename == "c0000.behbnd.dcx").FirstOrDefault();

        // Read TAE: get animations (behavior judge -> animation ID)
        // Read HKX: get animations names (clipGen -> CMSG list)

        EquipWeaponParam = ParamBank.PrimaryBank.Params.Where(e => e.Key == "EquipParamWeapon").First().Value;
        SwordArtsParam = ParamBank.PrimaryBank.Params.Where(e => e.Key == "SwordArtsParam").First().Value;
        BehaviorParam_PC = ParamBank.PrimaryBank.Params.Where(e => e.Key == "BehaviorParam_PC").First().Value;
        SpEffectParam = ParamBank.PrimaryBank.Params.Where(e => e.Key == "BehaviorParam_PC").First().Value;
        Bullet = ParamBank.PrimaryBank.Params.Where(e => e.Key == "Bullet").First().Value;
        AtkParam_Pc = ParamBank.PrimaryBank.Params.Where(e => e.Key == "AtkParam_Pc").First().Value;

        foreach(var entry in EquipWeaponParam.Rows)
        {
            DeriveRowNames(entry);
        }
    }

    public static void DeriveRowNames(Param.Row row)
    {
        // Test with the 1000000 Dagger row
        if (row.ID != 1000000)
        {
            return;
        }

        var weaponName = row.Name;
        var behaviorVariationId = row["behaviorVariationId"].Value.Value.ToString();

        if (behaviorVariationId != "0")
        {
            List<Param.Row> behaviorRows;
            List<Param.Row> atkRows;
            List<Param.Row> bulletRows;
            List<Param.Row> spEffectRows;
            (behaviorRows, atkRows, bulletRows, spEffectRows) = GetReferencedBehaviorRows(row);

            var infoLine = GetBehaviorInfoLine(row);

            // Behavior
            foreach (var bRow in behaviorRows)
            {
                bRow.Name = $"{infoLine}";
            }

            infoLine = $"{weaponName}";

            // AtkParam
            foreach (var aRow in atkRows)
            {
                aRow.Name = $"{infoLine}";
            }

            infoLine = $"{weaponName}";

            // Bullet
            foreach (var blRow in bulletRows)
            {
                blRow.Name = $"{infoLine}";
            }

            infoLine = $"{weaponName}";

            // SpEffect
            foreach (var sRow in spEffectRows)
            {
                sRow.Name = $"[Weapon] {infoLine}";
            }
        }
    }

    public static string GetBehaviorInfoLine(Param.Row row)
    {
        var infoLine = "";

        var baseMotionCategory = (byte)row["wepmotionCategory"].Value.Value;

        // Find relevant TAE
        var baseTAE = PlayerTAEContainer.InternalFiles.Where(e => (e.TAE.ID-2000) == baseMotionCategory).First().TAE;

        switch (Smithbox.ProjectType)
        {
            case ProjectType.DS1:
                baseTAE.ApplyTemplate(AnimationBank.TimeActTemplates["TAE.Template.DS1"]);
                break;
            case ProjectType.DS2:
            case ProjectType.DS2S:
                baseTAE.ApplyTemplate(AnimationBank.TimeActTemplates["TAE.Template.SOTFS"]);
                break;
            case ProjectType.DS3:
                baseTAE.ApplyTemplate(AnimationBank.TimeActTemplates["TAE.Template.DS3"]);
                break;
            case ProjectType.BB:
                baseTAE.ApplyTemplate(AnimationBank.TimeActTemplates["TAE.Template.BB"]);
                break;
            case ProjectType.SDT:
                baseTAE.ApplyTemplate(AnimationBank.TimeActTemplates["TAE.Template.SDT"]);
                break;
            case ProjectType.ER:
                baseTAE.ApplyTemplate(AnimationBank.TimeActTemplates["TAE.Template.ER"]);
                break;
            case ProjectType.AC6:
                baseTAE.ApplyTemplate(AnimationBank.TimeActTemplates["TAE.Template.AC6"]);
                break;
        }

        var anims = baseTAE.Animations;

        foreach (var anim in anims)
        {
            foreach(var evt in anim.Events)
            {
                foreach (var entry in evt.Parameters.Values)
                {
                    TaskLogs.AddLog($"event: {entry.Key} {entry.Value}");
                }
            }
        }

        var overrideMotionCategory = row["spAtkcategory"].Value.Value.ToString();

        return infoLine;
    }

    public static (List<Param.Row>, List<Param.Row>, List<Param.Row>, List<Param.Row>) GetReferencedBehaviorRows(Param.Row row)
    {
        List<Param.Row> behaviorRows = new List<Param.Row>();
        List<Param.Row> atkParamRows = new List<Param.Row>();
        List<Param.Row> bulletRows = new List<Param.Row>();
        List<Param.Row> spEffectRows = new List<Param.Row>();

        var behaviorVariationId = row["behaviorVariationId"].Value.Value.ToString();

        behaviorRows = BehaviorParam_PC.Rows.Where(e => e["variationId"].Value.Value.ToString() == behaviorVariationId).ToList();

        foreach (var bRow in behaviorRows)
        {
            var refId = bRow["refId"].Value.Value.ToString();
            var bRefType = bRow["refType"].Value.Value.ToString();

            // Attack
            if (bRefType == "0")
            {
                if (AtkParam_Pc.Rows.Where(e => e.ID.ToString() == refId).Any())
                {
                    var atkRow = AtkParam_Pc.Rows.Where(e => e.ID.ToString() == refId).First();
                    atkParamRows.Add(atkRow);
                }
            }
            // Bullet
            if (bRefType == "1")
            {
                if (Bullet.Rows.Where(e => e.ID.ToString() == refId).Any())
                {
                    var bulletRow = Bullet.Rows.Where(e => e.ID.ToString() == refId).First();
                    bulletRows.Add(bulletRow);
                }
            }
            // SpEffect
            if (bRefType == "2")
            {
                if (SpEffectParam.Rows.Where(e => e.ID.ToString() == refId).Any())
                {
                    var effectRow = SpEffectParam.Rows.Where(e => e.ID.ToString() == refId).First();
                    spEffectRows.Add(effectRow);
                }
            }
        }

        return (behaviorRows, atkParamRows, bulletRows, spEffectRows);
    }
}
