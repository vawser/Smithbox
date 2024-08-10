using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editors.ParamEditor.Actions;
using StudioCore.Editors.TextEditor.Tools;
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
    }

    public void OnProjectChanged()
    {

    }
    public void DisplayMenu()
    {
        if (ImGui.BeginMenu("Tools"))
        {
            if (ParamMemoryTools.IsParamReloaderSupported())
            {
                ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
                if (ImGui.BeginMenu("Param Reloader"))
                {
                    if (ImGui.MenuItem("Current Param", KeyBindings.Current.PARAM_ReloadParam.HintText))
                    {
                        ParamMemoryTools.ReloadCurrentParam();
                    }
                    ImguiUtils.ShowHoverTooltip("WARNING: Param Reloader only works for existing row entries.\nGame must be restarted for new rows and modified row IDs.");

                    if (ImGui.MenuItem("All Params", KeyBindings.Current.PARAM_ReloadAllParams.HintText))
                    {
                        ParamMemoryTools.ReloadAllParams();
                    }
                    ImguiUtils.ShowHoverTooltip("WARNING: Param Reloader only works for existing row entries.\nGame must be restarted for new rows and modified row IDs.");

                    ImGui.EndMenu();
                }
            }

            if (Smithbox.ProjectType == ProjectType.DS3)
            {
                ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
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

                    if (ImGui.MenuItem("Give Selected Item"))
                    {
                        ParamMemoryTools.GiveItem();
                    }
                    ImguiUtils.ShowHoverTooltip("Spawns selected item in-game.");

                    ImGui.EndMenu();
                }
            }

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Color Picker"))
            {
                ColorPicker.ShowColorPicker = !ColorPicker.ShowColorPicker;
            }

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.BeginMenu("Import Row Names"))
            {
                if (ImGui.BeginMenu("Smithbox"))
                {
                    if (ImGui.MenuItem("Selected Rows"))
                    {
                        if (Screen._activeView._selection.RowSelectionExists())
                        {
                            Handler.CurrentSourceCategory = SourceType.Smithbox;
                            Handler.CurrentTargetCategory = TargetType.SelectedRows;
                            Handler.ImportRowNameHandler();
                        }
                    }
                    ImguiUtils.ShowHoverTooltip("Import names for the specific rows currently selected.");

                    if (ImGui.MenuItem("Selected Param"))
                    {
                        if (Screen._activeView._selection.RowSelectionExists())
                        {
                            Handler.CurrentSourceCategory = SourceType.Smithbox;
                            Handler.CurrentTargetCategory = TargetType.SelectedParam;
                            Handler.ImportRowNameHandler();
                        }
                    }
                    ImguiUtils.ShowHoverTooltip("Import names for the specific param currently selected.");

                    if (ImGui.MenuItem("All Params"))
                    {
                        if (Screen._activeView._selection.RowSelectionExists())
                        {
                            Handler.CurrentSourceCategory = SourceType.Smithbox;
                            Handler.CurrentTargetCategory = TargetType.AllParams;
                            Handler.ImportRowNameHandler();
                        }
                    }
                    ImguiUtils.ShowHoverTooltip("Import names for all params.");

                    ImGui.EndMenu();
                }
                ImguiUtils.ShowHoverTooltip("Draw names from the in-built Smithbox name lists.");

                if (ImGui.BeginMenu("Project"))
                {
                    if (ImGui.MenuItem("Selected Rows"))
                    {
                        if (Screen._activeView._selection.RowSelectionExists())
                        {
                            Handler.CurrentSourceCategory = SourceType.Smithbox;
                            Handler.CurrentTargetCategory = TargetType.SelectedRows;
                            Handler.ImportRowNameHandler();
                        }
                    }
                    ImguiUtils.ShowHoverTooltip("Import names for the specific rows currently selected.");

                    if (ImGui.MenuItem("Selected Param"))
                    {
                        if (Screen._activeView._selection.RowSelectionExists())
                        {
                            Handler.CurrentSourceCategory = SourceType.Smithbox;
                            Handler.CurrentTargetCategory = TargetType.SelectedParam;
                            Handler.ImportRowNameHandler();
                        }
                    }
                    ImguiUtils.ShowHoverTooltip("Import names for the specific param currently selected.");

                    if (ImGui.MenuItem("All Params"))
                    {
                        if (Screen._activeView._selection.RowSelectionExists())
                        {
                            Handler.CurrentSourceCategory = SourceType.Smithbox;
                            Handler.CurrentTargetCategory = TargetType.AllParams;
                            Handler.ImportRowNameHandler();
                        }
                    }
                    ImguiUtils.ShowHoverTooltip("Import names for all params.");

                    ImGui.EndMenu();
                }
                ImguiUtils.ShowHoverTooltip("Draw names from your Project-specific name lists.");

                ImGui.EndMenu();
            }

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.BeginMenu("Export Row Names"))
            {
                if (ImGui.MenuItem("Export Selected Rows"))
                {
                    Handler.CurrentTargetCategory = TargetType.SelectedRows;
                    if (Screen._activeView._selection.RowSelectionExists())
                    {
                        Handler.ExportRowNameHandler();
                    }
                }
                ImguiUtils.ShowHoverTooltip("Export the row names for the currently selected rows.");

                if (ImGui.MenuItem("Export Selected Param"))
                {
                    Handler.CurrentTargetCategory = TargetType.SelectedParam;
                    if (Screen._activeView._selection.RowSelectionExists())
                    {
                        Handler.ExportRowNameHandler();
                    }
                }
                ImguiUtils.ShowHoverTooltip("Export the row names for the currently selected param.");

                if (ImGui.MenuItem("Export All"))
                {
                    Handler.CurrentTargetCategory = TargetType.AllParams;
                    if (Screen._activeView._selection.RowSelectionExists())
                    {
                        Handler.ExportRowNameHandler();
                    }
                }
                ImguiUtils.ShowHoverTooltip("Export all of the row names for all params.");

                ImGui.EndMenu();
            }

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Trim Row Names"))
            {
                if (Screen._activeView._selection.ActiveParamExists())
                {
                    Handler.RowNameTrimHandler();
                }
            }

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Sort Rows"))
            {
                if (Screen._activeView._selection.ActiveParamExists())
                {
                    Handler.SortRowsHandler();
                }
            }
            
            /*
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Check Params for Edits", null, false, !ParamBank.PrimaryBank.IsLoadingParams && !ParamBank.VanillaBank.IsLoadingParams))
            {
                ParamBank.RefreshAllParamDiffCaches(true);
            }
            */

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.BeginMenu("Editor Mode"))
            {
                if (ImGui.MenuItem("Toggle"))
                {
                    ParamEditorScreen.EditorMode = !ParamEditorScreen.EditorMode;
                }
                ImguiUtils.ShowHoverTooltip("Toggle Editor Mode, allowing you to edit the Param Meta within Smithbox.");
                ImguiUtils.ShowActiveStatus(ParamEditorScreen.EditorMode);

                if (ImGui.MenuItem("Save Changes"))
                {
                    ParamMetaData.SaveAll();
                    ParamEditorScreen.EditorMode = false;
                }
                ImguiUtils.ShowHoverTooltip("Save current Param Meta changes.");

                if (ImGui.MenuItem("Discard Changes"))
                {

                    ParamEditorScreen.EditorMode = false;
                }
                ImguiUtils.ShowHoverTooltip("Discard current Param Meta changes.");

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }
    }
}
