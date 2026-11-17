using Andre.Formats;
using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Logger;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace StudioCore.Editors.ParamEditor;

/// <summary>
/// Gives the selected EquipParam rows to the player in the running game.
/// Supported when the selected Param Reloader offsets define an item give function (itemGive* in the JSON)
/// and list the active param in itemIDCategories.
/// </summary>
public class ItemGib
{
    public ParamEditorView View;
    public ProjectEntry Project;

    public ItemGibProperties props = new();
    public ItemGibRelicDesigner Designer = new();

    private int lastSelectedParamID = -1;
    private string lastSelectedParam = null;

    public ItemGib(ParamEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    /// <summary>
    /// A row field as an int, or null when the param has no such field.
    /// </summary>
    private static int? GetRowInt(Param.Row row, string field)
    {
        var cell = row?[field];
        if (cell == null)
            return null;

        try
        {
            return Convert.ToInt32(cell.Value.Value);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// The offsets base that can give items of this param, or null.
    /// </summary>
    private static GameOffsetBaseEntry GetItemGiveBase(GameOffsetsEntry offsets, string paramType)
    {
        return offsets?.Bases.Find(x => x.SupportsItemGive && x.itemIDCategories.ContainsKey(paramType));
    }

    /// <summary>
    /// The designer slots for this param, when its prepare step defines any.
    /// </summary>
    private static ItemGivePrepare GetDesignerPrepare(GameOffsetBaseEntry itemGiveBase, string paramType)
    {
        return itemGiveBase?.ItemGivePrepares.FirstOrDefault(p => p.Param == paramType && p.HasSlots);
    }

    public void Display()
    {
        // The tool is listed for games whose offsets can give items at all.
        var offsets = GetGameOffsets();
        var gameBases = offsets?.Bases.Where(b => b.SupportsItemGive).ToList();
        if (gameBases == null || !gameBases.Any())
        {
            return;
        }

        if (!ToolSections.Header($"{LOC.Get("PARAM_Tools_ItemGib_Header")}##itemGibHeader"))
        {
            return;
        }

        ImGui.BeginChild("ItemGibSection", ImGuiChildFlags.Borders);

        DisplayContents(offsets, gameBases);

        ImGui.EndChild();
    }

    private void DisplayContents(GameOffsetsEntry offsets, List<GameOffsetBaseEntry> gameBases)
    {
        GUI.WrappedText(LOC.Get("PARAM_Tools_ItemGib_Hint"));

        var activeView = Project.Handler.ParamEditor.ViewHandler.ActiveView;
        var activeParam = activeView.Selection.GetActiveParam();

        var itemGiveBase = string.IsNullOrEmpty(activeParam) ? null : GetItemGiveBase(offsets, activeParam);
        if (itemGiveBase == null)
        {
            var supported = string.Join(", ", gameBases.SelectMany(b => b.itemIDCategories.Keys).Distinct());
            GUI.Spacer();
            GUI.WrappedText(LOC.Get("PARAM_Tools_ItemGib_Unsupported_Param", supported));
            return;
        }

        var activeRow = activeView.Selection.GetActiveRow();
        if (activeRow == null)
        {
            GUI.Spacer();
            GUI.WrappedText(LOC.Get("PARAM_Tools_ItemGib_No_Active_Row"));
            return;
        }

        var paramChanged = lastSelectedParamID != activeRow.ID || lastSelectedParam != activeParam;
        if (paramChanged)
        {
            lastSelectedParamID = activeRow.ID;
            lastSelectedParam = activeParam;
            props.Quantity = 1;
            props.ReinforceLvl = 0;
            props.GemId = -1;
            props.Durability = -1;
        }

        var hasDurability = itemGiveBase.ItemGiveLayout == ItemGiveLayout.Durability;
        var hasGem = itemGiveBase.ItemGiveLayout == ItemGiveLayout.Gem;
        var designerPrepare = GetDesignerPrepare(itemGiveBase, activeParam);
        var multiSelect = IsMultiSelectParam(activeParam);
        var selectedCount = multiSelect ? activeView.Selection.GetSelectedRows().Count : 1;

        // Item
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_Tools_ItemGib_Item_Header"),
            LOC.Get("PARAM_Tools_ItemGib_Item_Header_TT"));

        GUI.WrappedText(string.IsNullOrWhiteSpace(activeRow.Name) ? $"{activeRow.ID}" : $"{activeRow.ID} {activeRow.Name}");
        if (selectedCount > 1)
        {
            GUI.WrappedText(LOC.Get(designerPrepare != null && Designer.HasChoices
                ? "PARAM_Tools_ItemGib_Item_Designer_Active_Only"
                : "PARAM_Tools_ItemGib_Item_Selected_Count", selectedCount));
        }

        // Quantity
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_Tools_ItemGib_Number_of_Spawned_Items"),
            LOC.Get("PARAM_Tools_ItemGib_Number_of_Spawned_Items_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.InputInt("##itemGibQuantity", ref props.Quantity);
        GUI.Tooltip(LOC.Get("PARAM_Tools_ItemGib_Quantity_Input_TT"));
        props.Quantity = Math.Clamp(props.Quantity, 1, 999);

        switch (activeParam)
        {
            case "EquipParamWeapon":
                DisplayWeaponOptions(activeRow, hasGem, hasDurability);
                break;
            case "EquipParamProtector":
                props.ReinforceLvl = 0;
                props.GemId = -1;

                if (hasDurability)
                {
                    DisplayDurability();
                }
                break;
            case "EquipParamCustomWeapon":
                props.ReinforceLvl = GetRowInt(activeRow, "reinforceLv") ?? 0;
                props.GemId = GetRowInt(activeRow, "gemId") ?? -1;
                break;
            default:
                // Goods, Magic, Accessory, Gem, Antique: stackable, multi-select.
                props.ReinforceLvl = 0;
                props.GemId = -1;
                break;
        }

        // Designer (e.g. NR relic effects)
        if (designerPrepare != null)
        {
            Designer.Display(activeView, activeRow, activeParam, designerPrepare);
        }

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_Tools_ItemGib_Actions_Header"),
            LOC.Get("PARAM_Tools_ItemGib_Actions_Header_TT"));

        if (designerPrepare != null)
        {
            GUI.MultiButtonInput("itemGibActions",
                "giveItemAction", LOC.Get("PARAM_Tools_ItemGib_Action_Give_Item"), LOC.Get("PARAM_Tools_ItemGib_Action_Give_Item_TT"),
                GiveItem,

                "resetDesignerAction", LOC.Get("PARAM_Tools_ItemGib_Action_Reset_Designer"), LOC.Get("PARAM_Tools_ItemGib_Action_Reset_Designer_TT"),
                Designer.Reset);
        }
        else
        {
            GUI.MultiButtonInput("itemGibActions",
                "giveItemAction", LOC.Get("PARAM_Tools_ItemGib_Action_Give_Item"), LOC.Get("PARAM_Tools_ItemGib_Action_Give_Item_TT"),
                GiveItem);
        }
    }

    private void DisplayWeaponOptions(Param.Row activeRow, bool hasGem, bool hasDurability)
    {
        // Weapons that can mount a gem (ER gemMountType 2) reach +25, the others +10.
        var gemMountable = GetRowInt(activeRow, "gemMountType") == 2;

        var reinforceShopCategory = GetRowInt(activeRow, "reinforceShopCategory");
        if (reinforceShopCategory is not null and not 0)
        {
            // Reinforcement
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("PARAM_Tools_ItemGib_Reinforcement_Level"),
                LOC.Get("PARAM_Tools_ItemGib_Reinforcement_Level_TT"));

            ImGui.SetNextItemWidth(-1);
            ImGui.InputInt("##itemGibReinforce", ref props.ReinforceLvl);
            GUI.Tooltip(LOC.Get("PARAM_Tools_ItemGib_Reinforce_Input_TT", gemMountable ? 25 : 10));
            props.ReinforceLvl = Math.Clamp(props.ReinforceLvl, 0, gemMountable ? 25 : 10);
        }
        else
        {
            props.ReinforceLvl = 0;
        }

        if (hasGem && gemMountable)
        {
            DisplayGem(activeRow);
        }
        else
        {
            props.GemId = -1;
        }

        if (hasDurability)
        {
            DisplayDurability();
        }
    }

    private void DisplayGem(Param.Row row)
    {
        var activeView = Project.Handler.ParamEditor.ViewHandler.ActiveView;
        const string paramName = "EquipParamGem";
        List<ParamRef> refs = new() { new ParamRef(null, paramName) };

        // Gem
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_Tools_ItemGib_Param_ID", paramName),
            LOC.Get("PARAM_Tools_ItemGib_Gem_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.InputInt("##itemGibGem", ref props.GemId);
        GUI.Tooltip(LOC.Get("PARAM_Tools_ItemGib_Gem_Input_TT"));
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            ImGui.OpenPopup("ItemGibGemContextMenu");
        }

        ParamReferenceHelper.Label(activeView, refs, row);
        ImGui.SameLine();
        ParamReferenceHelper.Hint(activeView, refs, row, props.GemId);

        if (ImGui.BeginPopup("ItemGibGemContextMenu"))
        {
            object newValue = props.GemId;
            if (ParamReferenceHelper.ContextMenu(activeView, refs, row, props.GemId, ref newValue, activeView.Editor.ActionManager))
            {
                props.GemId = (int)newValue;
            }

            ImGui.EndPopup();
        }
    }

    private void DisplayDurability()
    {
        // Durability
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_Tools_ItemGib_Durability"),
            LOC.Get("PARAM_Tools_ItemGib_Durability_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.InputInt("##itemGibDurability", ref props.Durability);
        GUI.Tooltip(LOC.Get("PARAM_Tools_ItemGib_Durability_Input_TT"));
        props.Durability = Math.Max(props.Durability, -1);
    }

    private static bool IsMultiSelectParam(string param)
    {
        return param is not ("EquipParamWeapon" or "EquipParamProtector" or "EquipParamCustomWeapon");
    }

    public void GiveItem()
    {
        var activeView = Project.Handler.ParamEditor.ViewHandler.ActiveView;
        var activeParam = activeView.Selection.GetActiveParam();

        if (string.IsNullOrEmpty(activeParam))
        {
            Smithbox.Log(this, LOC.Get("PARAM_Tools_ItemGib_No_Param"));
            return;
        }

        var offsets = GetGameOffsets();
        var itemGiveBase = GetItemGiveBase(offsets, activeParam);
        if (itemGiveBase == null)
        {
            Smithbox.LogError(this, LOC.Get("PARAM_Tools_ItemGib_No_Offsets"), LogPriority.High);
            return;
        }

        var props = new ItemGibProperties(this.props);
        var designerPrepare = GetDesignerPrepare(itemGiveBase, activeParam);

        List<Param.Row> rowsToGib;
        switch (activeParam)
        {
            case "EquipParamProtector":
            case "EquipParamWeapon":
                rowsToGib = new() { activeView.Selection.GetActiveRow() };
                break;
            case "EquipParamCustomWeapon":
                var equipParamWeaponId = GetRowInt(activeView.Selection.GetActiveRow(), "baseWepId") ?? -1;
                if (equipParamWeaponId < 0)
                {
                    Smithbox.Log(this, LOC.Get("PARAM_Tools_ItemGib_No_Base_Weapon"));
                    return;
                }
                var equipParamWeapon = activeView.GetPrimaryBank().GetParamFromName("EquipParamWeapon")[equipParamWeaponId];
                if (equipParamWeapon == null)
                {
                    Smithbox.Log(this, LOC.Get("PARAM_Tools_ItemGib_No_EquipParamWeapon", equipParamWeaponId));
                    return;
                }
                rowsToGib = new() { equipParamWeapon };
                break;
            default:
                // The designer's choices belong to the active row's slots, so only that row is given with them.
                rowsToGib = designerPrepare != null && Designer.HasChoices
                    ? new() { activeView.Selection.GetActiveRow() }
                    : activeView.Selection.GetSelectedRows();
                break;
        }

        rowsToGib = rowsToGib.Where(r => r != null).ToList();
        if (!rowsToGib.Any())
        {
            Smithbox.Log(this, LOC.Get("PARAM_Tools_ItemGib_No_Rows"));
            return;
        }

        var categoryOffset = itemGiveBase.itemIDCategories.GetValueOrDefault(activeParam, 0);
        var items = rowsToGib
            .Select(row => new ItemGiveEntry(row.ID + categoryOffset + props.ReinforceLvl, props.Quantity, props.Durability, props.GemId, activeParam,
                designerPrepare != null ? Designer.GetOverrides(row, designerPrepare) : null))
            .ToList();

        GiveItems(offsets, itemGiveBase, items);
    }

    private void GiveItems(GameOffsetsEntry offsets, GameOffsetBaseEntry itemGiveBase, List<ItemGiveEntry> items)
    {
        var name = offsets.exeName.Replace(".exe", "");
        var process = Process.GetProcessesByName(name).FirstOrDefault();
        if (process == null)
        {
            Smithbox.LogError(this, LOC.Get("PARAM_Tools_Memory_No_Game_Process", offsets.exeName), LogPriority.High);
            return;
        }

        SoulsMemoryHandler memoryHandler = new(View.Editor, process);
        try
        {
            if (!memoryHandler.TryResolveItemGiveAddress("Func", itemGiveBase.ItemGiveFunc, out var funcOffset, out var funcFromAob) ||
                !memoryHandler.TryResolveItemGiveAddress("Manager", itemGiveBase.ItemGiveManager, out var managerOffset, out var managerFromAob))
            {
                Smithbox.LogError(this, LOC.Get("PARAM_Tools_ItemGib_Unresolved"), LogPriority.High);
                return;
            }

            // Prepare calls for the params being given (e.g. NR relics: roll their effects).
            var prepares = new Dictionary<string, (int Lookup, int Prepare)>();
            var allFromAob = funcFromAob && managerFromAob;
            foreach (var prepare in itemGiveBase.ItemGivePrepares)
            {
                if (!items.Any(i => i.Param == prepare.Param))
                    continue;

                if (!memoryHandler.TryResolveItemGiveAddress($"Lookup_{prepare.Param}", prepare.Lookup, out var lookupOffset, out var lookupFromAob) ||
                    !memoryHandler.TryResolveItemGiveAddress($"Prepare_{prepare.Param}", prepare.Prepare, out var prepareOffset, out var prepareFromAob))
                {
                    Smithbox.LogError(this, LOC.Get("PARAM_Tools_ItemGib_Unresolved"), LogPriority.High);
                    return;
                }

                prepares[prepare.Param] = (lookupOffset, prepareOffset);
                allFromAob &= lookupFromAob && prepareFromAob;
            }

            // A hard offset is only valid for the exe version it was taken from: calling it on another
            // version calls into the middle of unrelated code and crashes the game.
            if (!allFromAob)
            {
                var info = process.MainModule.FileVersionInfo;
                var runningVersion = $"{info.FileMajorPart}.{info.FileMinorPart}.{info.FileBuildPart}.{info.FilePrivatePart}";
                if (runningVersion != offsets.exeVersion)
                {
                    Smithbox.LogError(this, LOC.Get("PARAM_Tools_ItemGib_Version_Mismatch", runningVersion, offsets.exeVersion), LogPriority.High);
                    return;
                }
            }

            if (memoryHandler.ReadManager(managerOffset) == 0)
            {
                Smithbox.LogError(this, LOC.Get("PARAM_Tools_ItemGib_No_Manager"), LogPriority.High);
                return;
            }

            memoryHandler.PlayerItemGive(itemGiveBase, funcOffset, managerOffset, items, prepares);
        }
        finally
        {
            memoryHandler.Terminate();
        }
    }

    public GameOffsetsEntry GetGameOffsets()
    {
        ProjectType game = Project.Descriptor.ProjectType;
        if (!GameOffsetsEntry.GameOffsetBank.ContainsKey(game))
        {
            // Games without Param Reloader offsets: nothing to give with.
            if (Project.Handler.ParamData?.ParamReloaderOffsets?.Groups is not { Count: > 0 })
            {
                return null;
            }

            try
            {
                GameOffsetsEntry.GameOffsetBank.Add(game, new GameOffsetsEntry(Project));
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("PARAM_Tools_ItemGib_No_Game_Offset"),
                    LogPriority.High, e);
                return null;
            }
        }

        return GameOffsetsEntry.GameOffsetBank[game];
    }
}
