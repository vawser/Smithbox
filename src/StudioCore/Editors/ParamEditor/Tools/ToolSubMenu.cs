using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
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
    private ParamEditorScreen Editor;
    public ActionHandler Handler;

    public ToolSubMenu(ParamEditorScreen screen)
    {
        Editor = screen;
        Handler = new ActionHandler(screen);
    }

    public void Shortcuts()
    {
        if (InputTracker.GetKeyDown(KeyBindings.Current.PARAM_CreateParamPinGroup))
        {
            Editor.ToolWindow.PinGroupHandler.SetAutoGroupName("Param");
            Editor.ToolWindow.PinGroupHandler.CreateParamGroup();
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.PARAM_CreateRowPinGroup))
        {
            Editor.ToolWindow.PinGroupHandler.SetAutoGroupName("Row");
            Editor.ToolWindow.PinGroupHandler.CreateRowGroup();
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.PARAM_CreateFieldPinGroup))
        {
            Editor.ToolWindow.PinGroupHandler.SetAutoGroupName("Field");
            Editor.ToolWindow.PinGroupHandler.CreateFieldGroup();
        }

        if (InputTracker.GetKeyDown(KeyBindings.Current.PARAM_ClearCurrentPinnedParams))
        {
            Editor.Project.PinnedParams = new();
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.PARAM_ClearCurrentPinnedRows))
        {
            Editor.Project.PinnedRows = new();
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.PARAM_ClearCurrentPinnedFields))
        {
            Editor.Project.PinnedFields = new();
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
            Editor.RowNamer.ApplyRowNamer();
        }
    }

    public void OnProjectChanged()
    {

    }
    public void DisplayMenu()
    {
        if (ImGui.BeginMenu("Tools"))
        {
            if (ImGui.MenuItem("Color Picker"))
            {
                ColorPicker.ShowColorPicker = !ColorPicker.ShowColorPicker;
            }

            if (ImGui.MenuItem("Trim Row Names"))
            {
                if (Editor._activeView._selection.ActiveParamExists())
                {
                    Handler.RowNameTrimHandler();
                }
            }

            if (ImGui.MenuItem("Sort Rows"))
            {
                if (Editor._activeView._selection.ActiveParamExists())
                {
                    Handler.SortRowsHandler();
                }
            }

            ImGui.Separator();

            // Import
            if (ImGui.BeginMenu("Import Row Names"))
            {
                if (Editor.Project.ProjectType is ProjectType.BB or ProjectType.AC6)
                {
                    if (ImGui.BeginMenu("Developer"))
                    {
                        if (ImGui.MenuItem("Selected Rows"))
                        {
                            if (Editor._activeView._selection.RowSelectionExists())
                            {
                                Handler.CurrentSourceCategory = SourceType.Developer;
                                Handler.CurrentTargetCategory = TargetType.SelectedRows;
                                Handler.ImportRowNameHandler();
                            }
                        }
                        UIHelper.Tooltip("Import names for the specific rows currently selected.");

                        if (ImGui.MenuItem("Selected Param"))
                        {
                            if (Editor._activeView._selection.RowSelectionExists())
                            {
                                Handler.CurrentSourceCategory = SourceType.Developer;
                                Handler.CurrentTargetCategory = TargetType.SelectedParam;
                                Handler.ImportRowNameHandler();
                            }
                        }
                        UIHelper.Tooltip("Import names for the specific param currently selected.");

                        if (ImGui.MenuItem("All Params"))
                        {
                            Handler.CurrentSourceCategory = SourceType.Developer;
                            Handler.CurrentTargetCategory = TargetType.AllParams;
                            Handler.ImportRowNameHandler();
                        }
                        UIHelper.Tooltip("Import names for all params.");

                        ImGui.EndMenu();
                    }
                    UIHelper.Tooltip("Draw row names from the in-built Developer name lists. These are the Japanese and machine-translated english row names supplied with leaked paramdefs.");
                }

                if (ImGui.BeginMenu("Smithbox"))
                {
                    if (ImGui.MenuItem("Selected Rows"))
                    {
                        if (Editor._activeView._selection.RowSelectionExists())
                        {
                            Handler.CurrentSourceCategory = SourceType.Smithbox;
                            Handler.CurrentTargetCategory = TargetType.SelectedRows;
                            Handler.ImportRowNameHandler();
                        }
                    }
                    UIHelper.Tooltip("Import row names for the specific rows currently selected.");

                    if (ImGui.MenuItem("Selected Param"))
                    {
                        if (Editor._activeView._selection.RowSelectionExists())
                        {
                            Handler.CurrentSourceCategory = SourceType.Smithbox;
                            Handler.CurrentTargetCategory = TargetType.SelectedParam;
                            Handler.ImportRowNameHandler();
                        }
                    }
                    UIHelper.Tooltip("Import names for the specific param currently selected.");

                    if (ImGui.MenuItem("All Params"))
                    {
                        Handler.CurrentSourceCategory = SourceType.Smithbox;
                        Handler.CurrentTargetCategory = TargetType.AllParams;
                        Handler.ImportRowNameHandler();
                    }
                    UIHelper.Tooltip("Import names for all params.");

                    ImGui.EndMenu();
                }
                UIHelper.Tooltip("Draw names from the in-built Smithbox name lists. These are curated row names maintained by Smithbox.");

                if (ImGui.BeginMenu("Project"))
                {
                    if (ImGui.MenuItem("Selected Rows"))
                    {
                        if (Editor._activeView._selection.RowSelectionExists())
                        {
                            Handler.CurrentSourceCategory = SourceType.Smithbox;
                            Handler.CurrentTargetCategory = TargetType.SelectedRows;
                            Handler.ImportRowNameHandler();
                        }
                    }
                    UIHelper.Tooltip("Import names for the specific rows currently selected.");

                    if (ImGui.MenuItem("Selected Param"))
                    {
                        if (Editor._activeView._selection.RowSelectionExists())
                        {
                            Handler.CurrentSourceCategory = SourceType.Smithbox;
                            Handler.CurrentTargetCategory = TargetType.SelectedParam;
                            Handler.ImportRowNameHandler();
                        }
                    }
                    UIHelper.Tooltip("Import names for the specific param currently selected.");

                    if (ImGui.MenuItem("All Params"))
                    {
                        Handler.CurrentSourceCategory = SourceType.Smithbox;
                        Handler.CurrentTargetCategory = TargetType.AllParams;
                        Handler.ImportRowNameHandler();
                    }
                    UIHelper.Tooltip("Import names for all params.");

                    ImGui.EndMenu();
                }
                UIHelper.Tooltip("Draw names from your Project-specific name lists. These are curated row names you've exported for your project.");

                ImGui.EndMenu();
            }

            // Export
            if (ImGui.BeginMenu("Export Row Names"))
            {
                if (ImGui.MenuItem("Export Selected Rows"))
                {
                    Handler.CurrentTargetCategory = TargetType.SelectedRows;
                    if (Editor._activeView._selection.RowSelectionExists())
                    {
                        Handler.ExportRowNameHandler();
                    }
                }
                UIHelper.Tooltip("Export the row names for the currently selected rows.");

                if (ImGui.MenuItem("Export Selected Param"))
                {
                    Handler.CurrentTargetCategory = TargetType.SelectedParam;
                    if (Editor._activeView._selection.RowSelectionExists())
                    {
                        Handler.ExportRowNameHandler();
                    }
                }
                UIHelper.Tooltip("Export the row names for the currently selected param.");

                if (ImGui.MenuItem("Export All"))
                {
                    Handler.CurrentTargetCategory = TargetType.AllParams;
                    if (Editor._activeView._selection.RowSelectionExists())
                    {
                        Handler.ExportRowNameHandler();
                    }
                }
                UIHelper.Tooltip("Export all of the row names for all params.");

                ImGui.EndMenu();
            }

            // Param Reloader
            if (Editor.ParamReloader.GameIsSupported(Editor.Project.ProjectType))
            {
                if (ImGui.BeginMenu("Param Reloader"))
                {
                    if (ImGui.MenuItem("Current Param"))
                    {
                        Editor.ParamReloader.ReloadCurrentParam(Editor);
                    }
                    UIHelper.Tooltip($"WARNING: Param Reloader only works for existing row entries.\nGame must be restarted for new rows and modified row IDs.\n{KeyBindings.Current.PARAM_ReloadParam.HintText}");

                    if (ImGui.MenuItem("All Params"))
                    {
                        Editor.ParamReloader.ReloadAllParams(Editor);
                    }
                    UIHelper.Tooltip($"WARNING: Param Reloader only works for existing row entries.\nGame must be restarted for new rows and modified row IDs.\n{KeyBindings.Current.PARAM_ReloadAllParams.HintText}");

                    ImGui.EndMenu();
                }
            }

            // Item Gib
            if (Editor.Project.ProjectType == ProjectType.DS3)
            {
                if (ImGui.BeginMenu("Item Gib"))
                {
                    var activeParam = Editor._activeView._selection.GetActiveParam();

                    if (activeParam == "EquipParamGoods")
                    {
                        ImGui.InputInt("Number of Spawned Items##spawnItemCount", ref Editor.ParamReloader.SpawnedItemAmount);
                    }
                    if (activeParam == "EquipParamWeapon")
                    {
                        ImGui.InputInt("Reinforcement of Spawned Weapon##spawnWeaponLevel", ref Editor.ParamReloader.SpawnWeaponLevel);

                        if (Editor.ParamReloader.SpawnWeaponLevel > 10)
                        {
                            Editor.ParamReloader.SpawnWeaponLevel = 10;
                        }
                    }

                    if (ImGui.MenuItem("Give Selected Item"))
                    {
                        Editor.ParamReloader.GiveItem(Editor);
                    }
                    UIHelper.Tooltip("Spawns selected item in-game.");

                    ImGui.EndMenu();
                }
            }

            // Editor Mode
            if (ImGui.BeginMenu("Editor Mode"))
            {
                if (ImGui.MenuItem("Toggle"))
                {
                    Editor.EditorMode = !Editor.EditorMode;
                }
                UIHelper.Tooltip("Toggle Editor Mode, allowing you to edit the Param Meta within Smithbox.");
                UIHelper.ShowActiveStatus(Editor.EditorMode);

                if (ImGui.MenuItem("Save Changes"))
                {
                    ParamMetaData.SaveAll();
                    Editor.EditorMode = false;
                }
                UIHelper.Tooltip("Save current Param Meta changes.");

                if (ImGui.MenuItem("Discard Changes"))
                {
                    Editor.EditorMode = false;
                }
                UIHelper.Tooltip("Discard current Param Meta changes.");

                ImGui.EndMenu();
            }


            ImGui.EndMenu();
        }
    }
}
