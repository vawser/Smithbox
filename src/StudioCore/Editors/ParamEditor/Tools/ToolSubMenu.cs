using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.Core.Project;
using StudioCore.Editors.ParamEditor.Actions;
using StudioCore.Interface;
using StudioCore.Tools;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor.Tools;

public class ToolSubMenu
{
    private ParamEditorScreen Screen;
    public ActionHandler Handler;

    public ToolSubMenu(ParamEditorScreen screen)
    {
        Screen = screen;
        Handler = new ActionHandler(screen);
    }

    public void Shortcuts()
    {
        if (InputTracker.GetKeyDown(KeyBindings.Current.PARAM_CreateParamPinGroup))
        {
            Screen.ToolWindow.PinGroupHandler.SetAutoGroupName("Param");
            Screen.ToolWindow.PinGroupHandler.CreateParamGroup();
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.PARAM_CreateRowPinGroup))
        {
            Screen.ToolWindow.PinGroupHandler.SetAutoGroupName("Row");
            Screen.ToolWindow.PinGroupHandler.CreateRowGroup();
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.PARAM_CreateFieldPinGroup))
        {
            Screen.ToolWindow.PinGroupHandler.SetAutoGroupName("Field");
            Screen.ToolWindow.PinGroupHandler.CreateFieldGroup();
        }

        if (InputTracker.GetKeyDown(KeyBindings.Current.PARAM_ClearCurrentPinnedParams))
        {
            Smithbox.ProjectHandler.CurrentProject.Config.PinnedParams = new();
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.PARAM_ClearCurrentPinnedRows))
        {
            Smithbox.ProjectHandler.CurrentProject.Config.PinnedRows = new();
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.PARAM_ClearCurrentPinnedFields))
        {
            Smithbox.ProjectHandler.CurrentProject.Config.PinnedFields = new();
        }

        if (InputTracker.GetKeyDown(KeyBindings.Current.PARAM_OnlyShowPinnedParams))
        {
            CFG.Current.Param_PinGroups_ShowOnlyPinnedParams = !CFG.Current.Param_PinGroups_ShowOnlyPinnedParams;
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.PARAM_OnlyShowPinnedRows))
        {
            CFG.Current.Param_PinGroups_ShowOnlyPinnedRows = !CFG.Current.Param_PinGroups_ShowOnlyPinnedRows;
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.PARAM_OnlyShowPinnedFields))
        {
            CFG.Current.Param_PinGroups_ShowOnlyPinnedFields = !CFG.Current.Param_PinGroups_ShowOnlyPinnedFields;
        }

        if (InputTracker.GetKeyDown(KeyBindings.Current.PARAM_ApplyRowNamer))
        {
            RowNamer.ApplyRowNamer();
        }
    }

    public void OnProjectChanged()
    {

    }
    public void DisplayMenu()
    {
        if (ImGui.BeginMenu("Tools"))
        {
            if (ImGui.Button("Color Picker", UI.MenuButtonSize))
            {
                ColorPicker.ShowColorPicker = !ColorPicker.ShowColorPicker;
            }

            if (ImGui.Button("Trim Row Names", UI.MenuButtonSize))
            {
                if (Screen._activeView._selection.ActiveParamExists())
                {
                    Handler.RowNameTrimHandler();
                }
            }

            if (ImGui.Button("Sort Rows", UI.MenuButtonSize))
            {
                if (Screen._activeView._selection.ActiveParamExists())
                {
                    Handler.SortRowsHandler();
                }
            }

            ImGui.Separator();

            // Import
            if (ImGui.BeginMenu("Import Row Names"))
            {
                if (ImGui.BeginMenu("Smithbox"))
                {
                    if (ImGui.Button("Selected Rows", UI.MenuButtonSize))
                    {
                        if (Screen._activeView._selection.RowSelectionExists())
                        {
                            Handler.CurrentSourceCategory = SourceType.Smithbox;
                            Handler.CurrentTargetCategory = TargetType.SelectedRows;
                            Handler.ImportRowNameHandler();
                        }
                    }
                    UIHelper.ShowHoverTooltip("Import names for the specific rows currently selected.");

                    if (ImGui.Button("Selected Param", UI.MenuButtonSize))
                    {
                        if (Screen._activeView._selection.RowSelectionExists())
                        {
                            Handler.CurrentSourceCategory = SourceType.Smithbox;
                            Handler.CurrentTargetCategory = TargetType.SelectedParam;
                            Handler.ImportRowNameHandler();
                        }
                    }
                    UIHelper.ShowHoverTooltip("Import names for the specific param currently selected.");

                    if (ImGui.Button("All Params", UI.MenuButtonSize))
                    {
                        if (Screen._activeView._selection.RowSelectionExists())
                        {
                            Handler.CurrentSourceCategory = SourceType.Smithbox;
                            Handler.CurrentTargetCategory = TargetType.AllParams;
                            Handler.ImportRowNameHandler();
                        }
                    }
                    UIHelper.ShowHoverTooltip("Import names for all params.");

                    ImGui.EndMenu();
                }
                UIHelper.ShowHoverTooltip("Draw names from the in-built Smithbox name lists.");

                if (ImGui.BeginMenu("Project"))
                {
                    if (ImGui.Button("Selected Rows", UI.MenuButtonSize))
                    {
                        if (Screen._activeView._selection.RowSelectionExists())
                        {
                            Handler.CurrentSourceCategory = SourceType.Smithbox;
                            Handler.CurrentTargetCategory = TargetType.SelectedRows;
                            Handler.ImportRowNameHandler();
                        }
                    }
                    UIHelper.ShowHoverTooltip("Import names for the specific rows currently selected.");

                    if (ImGui.Button("Selected Param", UI.MenuButtonSize))
                    {
                        if (Screen._activeView._selection.RowSelectionExists())
                        {
                            Handler.CurrentSourceCategory = SourceType.Smithbox;
                            Handler.CurrentTargetCategory = TargetType.SelectedParam;
                            Handler.ImportRowNameHandler();
                        }
                    }
                    UIHelper.ShowHoverTooltip("Import names for the specific param currently selected.");

                    if (ImGui.Button("All Params", UI.MenuButtonSize))
                    {
                        if (Screen._activeView._selection.RowSelectionExists())
                        {
                            Handler.CurrentSourceCategory = SourceType.Smithbox;
                            Handler.CurrentTargetCategory = TargetType.AllParams;
                            Handler.ImportRowNameHandler();
                        }
                    }
                    UIHelper.ShowHoverTooltip("Import names for all params.");

                    ImGui.EndMenu();
                }
                UIHelper.ShowHoverTooltip("Draw names from your Project-specific name lists.");

                ImGui.EndMenu();
            }

            // Export
            if (ImGui.BeginMenu("Export Row Names"))
            {
                if (ImGui.Button("Export Selected Rows", UI.MenuButtonSize))
                {
                    Handler.CurrentTargetCategory = TargetType.SelectedRows;
                    if (Screen._activeView._selection.RowSelectionExists())
                    {
                        Handler.ExportRowNameHandler();
                    }
                }
                UIHelper.ShowHoverTooltip("Export the row names for the currently selected rows.");

                if (ImGui.Button("Export Selected Param", UI.MenuButtonSize))
                {
                    Handler.CurrentTargetCategory = TargetType.SelectedParam;
                    if (Screen._activeView._selection.RowSelectionExists())
                    {
                        Handler.ExportRowNameHandler();
                    }
                }
                UIHelper.ShowHoverTooltip("Export the row names for the currently selected param.");

                if (ImGui.Button("Export All", UI.MenuButtonSize))
                {
                    Handler.CurrentTargetCategory = TargetType.AllParams;
                    if (Screen._activeView._selection.RowSelectionExists())
                    {
                        Handler.ExportRowNameHandler();
                    }
                }
                UIHelper.ShowHoverTooltip("Export all of the row names for all params.");

                ImGui.EndMenu();
            }

            // Param Reloader
            if (ParamMemoryTools.IsParamReloaderSupported())
            {
                if (ImGui.BeginMenu("Param Reloader"))
                {
                    if (ImGui.Button("Current Param", UI.MenuButtonSize))
                    {
                        ParamMemoryTools.ReloadCurrentParam();
                    }
                    UIHelper.ShowHoverTooltip($"WARNING: Param Reloader only works for existing row entries.\nGame must be restarted for new rows and modified row IDs.\n{KeyBindings.Current.PARAM_ReloadParam.HintText}");

                    if (ImGui.Button("All Params", UI.MenuButtonSize))
                    {
                        ParamMemoryTools.ReloadAllParams();
                    }
                    UIHelper.ShowHoverTooltip($"WARNING: Param Reloader only works for existing row entries.\nGame must be restarted for new rows and modified row IDs.\n{KeyBindings.Current.PARAM_ReloadAllParams.HintText}");

                    ImGui.EndMenu();
                }
            }

            // Item Gib
            if (Smithbox.ProjectType == ProjectType.DS3)
            {
                if (ImGui.BeginMenu("Item Gib"))
                {
                    var activeParam = Smithbox.EditorHandler.ParamEditor._activeView._selection.GetActiveParam();

                    if (activeParam == "EquipParamGoods")
                    {
                        ImGui.InputInt("Number of Spawned Items##spawnItemCount", ref ParamMemoryTools.SpawnedItemAmount);
                    }
                    if (activeParam == "EquipParamWeapon")
                    {
                        ImGui.InputInt("Reinforcement of Spawned Weapon##spawnWeaponLevel", ref ParamMemoryTools.SpawnWeaponLevel);
                        if (ParamMemoryTools.SpawnWeaponLevel > 10)
                        {
                            ParamMemoryTools.SpawnWeaponLevel = 10;
                        }
                    }

                    if (ImGui.Button("Give Selected Item", UI.MenuButtonSize))
                    {
                        ParamMemoryTools.GiveItem();
                    }
                    UIHelper.ShowHoverTooltip("Spawns selected item in-game.");

                    ImGui.EndMenu();
                }
            }

            // Editor Mode
            if (ImGui.BeginMenu("Editor Mode"))
            {
                if (ImGui.Button("Toggle", UI.MenuButtonSize))
                {
                    ParamEditorScreen.EditorMode = !ParamEditorScreen.EditorMode;
                }
                UIHelper.ShowHoverTooltip("Toggle Editor Mode, allowing you to edit the Param Meta within Smithbox.");
                UIHelper.ShowActiveStatus(ParamEditorScreen.EditorMode);

                if (ImGui.Button("Save Changes", UI.MenuButtonSize))
                {
                    ParamMetaData.SaveAll();
                    ParamEditorScreen.EditorMode = false;
                }
                UIHelper.ShowHoverTooltip("Save current Param Meta changes.");

                if (ImGui.Button("Discard Changes", UI.MenuButtonSize))
                {
                    ParamEditorScreen.EditorMode = false;
                }
                UIHelper.ShowHoverTooltip("Discard current Param Meta changes.");

                ImGui.EndMenu();
            }


            ImGui.EndMenu();
        }
    }
}
