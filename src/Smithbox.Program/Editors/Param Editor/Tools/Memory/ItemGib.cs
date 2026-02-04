using Andre.Formats;
using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;


public class ItemGib
{
    public ParamEditorScreen Editor;
    public ProjectEntry Project;

    public ItemGibProperties props = new();

    private int lastSelectedParamID = -1;
    private string lastSelectedParam = null;

    private readonly List<ProjectType> _itemGibSupportedGames = new()
    {
        ProjectType.DS3,
        ProjectType.ER,
    };

    public ItemGib(ParamEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public bool ItemGibSupported(ProjectType gameType)
    {
        return _itemGibSupportedGames.Contains(gameType);
    }

    private void ParamFieldDecoratorRegisterRightClick(string popupName = "ItemGibContextMenu")
    {
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            ImGui.OpenPopup(popupName);
        }
    }

    private void ParamReferenceField(Param.Row row, string paramName, ref int value)
    {
        if (row == null)
        {
            return;
        }

        var activeView = Project.Handler.ParamEditor.ViewHandler.ActiveView;

        List<ParamRef> refs = new() { new ParamRef(null, paramName) };
        UIHelper.WrappedText($"{paramName} ID");

        ImGui.InputInt($"##{paramName}", ref value);
        ParamFieldDecoratorRegisterRightClick($"ItemGib{paramName}ContextMenu");
        ImGui.NextColumn();

        ParamReferenceHelper.Label(activeView, refs, row);

        ParamFieldDecoratorRegisterRightClick($"ItemGib{paramName}ContextMenu");

        ImGui.SameLine();

        ParamReferenceHelper.Hint(activeView, refs, row, value);

        ParamFieldDecoratorRegisterRightClick($"ItemGib{paramName}ContextMenu");

        if (ImGui.BeginPopup($"ItemGib{paramName}ContextMenu"))
        {
            object newValue = value;

            if (ParamReferenceHelper.ContextMenu(activeView, refs, row, value, ref newValue, activeView.Editor.ActionManager))
            {
                value = (int)newValue;
            }

            ImGui.EndPopup();
        }
    }

    public void Display()
    {
        var windowWidth = ImGui.GetWindowWidth();

        if (!ItemGibSupported(Editor.Project.Descriptor.ProjectType))
        {
            return;
        }

        var activeView = Project.Handler.ParamEditor.ViewHandler.ActiveView;
        var activeParam = activeView.Selection.GetActiveParam();

        var gameOffsets = GetGameOffsets();
        if (string.IsNullOrEmpty(activeParam) || gameOffsets == null || !gameOffsets.Bases.Any(item => item.itemIDCategories.ContainsKey(activeParam)))
        {
            return;
        }

        if (ImGui.CollapsingHeader("Item Gib"))
        {
            UIHelper.WrappedText("Use this tool to spawn an item in-game.");
            UIHelper.WrappedText("");

            var activeRow = activeView.Selection.GetActiveRow();
            if (activeRow == null)
            {
                UIHelper.WrappedText("No active row selected. Please select a row in the Param Editor.");
                return;
            }
            var paramChanged = lastSelectedParamID != activeRow.ID || lastSelectedParam != activeParam;
            if (paramChanged)
            {
                lastSelectedParamID = activeRow.ID;
                lastSelectedParam = activeParam;
                props.Quantity = 1;
            }
            props.Quantity = Math.Clamp(props.Quantity, 1, 999);
            props.ReinforceLvl = Math.Clamp(props.ReinforceLvl, 0, 25);
            switch (activeParam)
            {
                case "EquipParamGoods":
                case "Magic":
                case "EquipParamAccessory":
                    UIHelper.WrappedText("Number of Spawned Items");
                    ImGui.InputInt("##spawnItemCount", ref props.Quantity);

                    if (paramChanged)
                    {
                        props.ReinforceLvl = 0;
                        props.GemId = -1;
                    }
                    break;
                case "EquipParamWeapon":
                    UIHelper.WrappedText("Number of Spawned Items");
                    ImGui.InputInt("##spawnItemCount", ref props.Quantity);

                    if (paramChanged)
                    {
                        props.ReinforceLvl = 0;
                        props.GemId = -1;
                    }
                    // check if reinforcement can be changed
                    if (activeRow["reinforceShopCategory"].Value.Value as byte? != 0)
                    {
                        UIHelper.WrappedText("Reinforcement Level");
                        ImGui.InputInt("##durability", ref props.ReinforceLvl);
                    }

                    // check if gem can be changed
                    if (Project.Descriptor.ProjectType == ProjectType.ER && activeRow["gemMountType"].Value.Value as byte? == 2)
                    {
                        ParamReferenceField(activeRow, "EquipParamGem", ref props.GemId);
                    }
                    else
                    {
                        props.ReinforceLvl = Math.Clamp(props.ReinforceLvl, 0, 10);
                    }

                    if (Project.Descriptor.ProjectType == ProjectType.DS3)
                    {
                        UIHelper.WrappedText("Durability");
                        ImGui.InputInt("##durability", ref props.Durability);
                    }
                    break;
                case "EquipParamProtector":
                    UIHelper.WrappedText("Number of Spawned Items");
                    ImGui.InputInt("##spawnItemCount", ref props.Quantity);

                    if (paramChanged) props.ReinforceLvl = 0;

                    if (Project.Descriptor.ProjectType == ProjectType.DS3)
                    {
                        UIHelper.WrappedText("Reinforcement Level");
                        ImGui.InputInt("##durability", ref props.ReinforceLvl);

                        UIHelper.WrappedText("Durability");
                        ImGui.InputInt("##durability", ref props.Durability);
                    }
                    break;
                case "EquipParamCustomWeapon":
                    UIHelper.WrappedText("Number of Spawned Items");
                    ImGui.InputInt("##spawnItemCount", ref props.Quantity);

                    props.ReinforceLvl = Convert.ToInt32(activeRow["reinforceLv"].Value.Value);
                    props.GemId = activeRow["gemId"].Value.Value as int? ?? -1;
                    break;
                default:
                    return; // Unsupported param type for Item Gib
            }


            UIHelper.WrappedText("");
            if (ImGui.Button("Give Item"))
            {
                GiveItem();
            }
        }
    }

    private void GiveItem(GameOffsetsEntry offsets, List<Param.Row> rowsToGib, string paramType, ItemGibProperties props)
    {
        if (!rowsToGib.Any())
            return;

        var name = offsets.exeName.Replace(".exe", "");
        Process[] processArray = Process.GetProcessesByName(name);
        if (!processArray.Any())
        {
            TaskLogs.AddLog($"No game process found for {offsets.exeName}. Please start the game first.", LogLevel.Error, LogPriority.High);
            return;
        }
        var gameOffsets = offsets.Bases.Find(x =>
        {
            var result = x.itemIDCategories.ContainsKey(paramType);
            if (Project.Descriptor.ProjectType == ProjectType.ER)
            {
                result = result && x.ERItemGiveFuncOffset.HasValue && x.ERMapItemManOffset.HasValue;
            }
            return result;
        });
        if (gameOffsets == null)
        {
            TaskLogs.AddLog($"No ItemGib offsets found.", LogLevel.Error, LogPriority.High);
            return;
        }

        SoulsMemoryHandler memoryHandler = new(Editor, processArray.First());

        List<int> finalItemIds = new();
        foreach (var row in rowsToGib)
        {
            int baseId = row.ID;
            int categoryOffset = gameOffsets.itemIDCategories.GetValueOrDefault(paramType, 0);
            int finalId = baseId + categoryOffset + props.ReinforceLvl;

            finalItemIds.Add(finalId);
        }


        if (Project.Descriptor.ProjectType == ProjectType.DS3)
        {
            memoryHandler.PlayerItemGive_DS3(finalItemIds, props.Quantity, props.Durability);
        }
        else if (Project.Descriptor.ProjectType == ProjectType.ER)
        {
            memoryHandler.PlayerItemGive_ER(gameOffsets, finalItemIds, props.Quantity, props.GemId);
        }

        memoryHandler.Terminate();
    }

    public void GiveItem()
    {
        var activeView = Project.Handler.ParamEditor.ViewHandler.ActiveView;
        var activeParam = activeView.Selection.GetActiveParam();

        if (string.IsNullOrEmpty(activeParam))
        {
            TaskLogs.AddLog("No param selected yet for Item Gib.");
            return;
        }

        GameOffsetsEntry offsets = GetGameOffsets();
        if (offsets == null)
            return;



        var props = new ItemGibProperties(this.props);
        List<Param.Row> rowsToGib = new List<Param.Row>();
        switch (activeParam)
        {
            case "EquipParamGoods":
            case "EquipParamAccessory":
                rowsToGib = activeView.Selection.GetSelectedRows();
                if (!rowsToGib.Any())
                {
                    TaskLogs.AddLog("No rows selected for Item Gib.");
                    return;
                }
                break;

            case "EquipParamProtector":
            case "EquipParamWeapon":
                rowsToGib = new()
                {
                    activeView.Selection.GetActiveRow()
                };
                break;
            case "EquipParamCustomWeapon":
                var equipParamWeaponId = activeView.Selection.GetActiveRow()["baseWepId"].Value.Value as int? ?? -1;
                if (equipParamWeaponId < 0)
                {
                    TaskLogs.AddLog("No base weapon ID found for EquipParamCustomWeapon.");
                    return;
                }
                var equipParamWeapon = activeView.GetPrimaryBank().GetParamFromName("EquipParamWeapon")[equipParamWeaponId];
                if (equipParamWeapon == null)
                {
                    TaskLogs.AddLog($"EquipParamWeapon with ID {equipParamWeaponId} not found.");
                    return;
                }
                rowsToGib = new()
                {
                    equipParamWeapon
                };
                break;
        }

        GiveItem(offsets, rowsToGib, activeParam, props);
    }

    public GameOffsetsEntry GetGameOffsets()
    {
        ProjectType game = Project.Descriptor.ProjectType;
        if (!GameOffsetsEntry.GameOffsetBank.ContainsKey(game))
        {
            try
            {
                GameOffsetsEntry.GameOffsetBank.Add(game, new GameOffsetsEntry(Project));
            }
            catch (Exception e)
            {
                TaskLogs.AddLog("Unable to create GameOffsets for Item Gibber.", LogLevel.Error,
                    LogPriority.High, e);
                return null;
            }
        }

        return GameOffsetsEntry.GameOffsetBank[game];
    }
}
