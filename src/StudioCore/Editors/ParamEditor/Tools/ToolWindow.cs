using ImGuiNET;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.ParamEditor.Actions;
using StudioCore.Interface;
using StudioCore.Locators;
using StudioCore.Platform;
using StudioCore.Utilities;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor.Tools;

public class ToolWindow
{
    private ParamEditorScreen Screen;
    public ActionHandler Handler;
    public MassEditHandler MassEditHandler;
    public PinGroups PinGroupHandler;

    public ToolWindow(ParamEditorScreen screen)
    {
        Screen = screen;
        Handler = new ActionHandler(screen);
        MassEditHandler = new MassEditHandler(screen);
        PinGroupHandler = new PinGroups(screen);
    }

    public void OnProjectChanged()
    {
        PinGroupHandler.OnProjectChanged();
    }

    public void OnGui()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * Smithbox.GetUIScale(), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Tool Window##ToolConfigureWindow_ParamEditor"))
        {
            var windowWidth = ImGui.GetWindowWidth();
            var defaultButtonSize = new Vector2(windowWidth * 0.975f, 32);
            var halfButtonSize = new Vector2(windowWidth * 0.975f / 2, 32);
            var thirdButtonSize = new Vector2(windowWidth * 0.975f / 3, 32);
            var inputBoxSize = new Vector2((windowWidth * 0.725f), 32);
            var inputButtonSize = new Vector2((windowWidth * 0.225f), 32);

            // Duplicate Row
            if (ImGui.CollapsingHeader("Duplicate Row"))
            {
                ImguiUtils.WrappedText("Duplicate the selected rows.");
                ImguiUtils.WrappedText("");

                if (!Screen._activeView._selection.RowSelectionExists())
                {
                    ImguiUtils.WrappedText("You must select a row before you can use this action.");
                    ImguiUtils.WrappedText("");
                }
                else
                {
                    ImguiUtils.WrappedText("Amount to Duplicate:");

                    ImGui.SetNextItemWidth(defaultButtonSize.X);
                    ImGui.InputInt("##Amount", ref CFG.Current.Param_Toolbar_Duplicate_Amount);
                    ImguiUtils.ShowHoverTooltip("The number of times the current selection will be duplicated.");
                    ImguiUtils.WrappedText("");

                    if (ImGui.Button("Duplicate##duplicateRow", defaultButtonSize))
                    {
                        Handler.DuplicateHandler();
                    }
                }
            }

            // Import Row Names
            if (ImGui.CollapsingHeader("Import Row Names"))
            {
                ImguiUtils.WrappedText("Import row names for the currently selected param, or for all params.");
                ImguiUtils.WrappedText("");

                if (!Screen._activeView._selection.ActiveParamExists())
                {
                    ImguiUtils.WrappedText("You must select a param before you can use this action.");
                    ImguiUtils.WrappedText("");
                }
                else
                {
                    Handler.ParamTargetElement(ref Handler.CurrentTargetCategory, "The target for the Row Name import.", defaultButtonSize);
                    Handler.ParamSourceElement(ref Handler.CurrentSourceCategory, "The source of the names used in by the Row Name import.", defaultButtonSize);

                    ImGui.Checkbox("Only replace unmodified row names", ref Handler._rowNameImporter_VanillaOnly);
                    ImguiUtils.ShowHoverTooltip("Row name import will only replace the name of unmodified rows.");

                    ImGui.Checkbox("Only replace empty row names", ref Handler._rowNameImporter_EmptyOnly);
                    ImguiUtils.ShowHoverTooltip("Row name import will only replace the name of un-named rows.");
                    ImguiUtils.WrappedText("");

                    if (ImGui.Button("Import##action_ImportRowNames", halfButtonSize))
                    {
                        Handler.ImportRowNameHandler();
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("Open Project Folder##action_Selection_OpenExportFolder", halfButtonSize))
                    {
                        if (Smithbox.ProjectType != ProjectType.Undefined)
                        {
                            var dir = $"{Smithbox.ProjectRoot}\\.smithbox\\Assets\\Paramdex\\{MiscLocator.GetGameIDForDir()}\\Names";
                            Process.Start("explorer.exe", dir);
                        }
                    }
                    ImguiUtils.ShowHoverTooltip("Opens the project-specific Names folder that contains the Names to be imported.");
                }
            }

            // Export Row Names
            if (ImGui.CollapsingHeader("Export Row Names"))
            {
                ImguiUtils.WrappedText("Export row names for the currently selected param, or for all params.");
                ImguiUtils.WrappedText("");

                if (!Screen._activeView._selection.ActiveParamExists())
                {
                    ImguiUtils.WrappedText("You must select a param before you can use this action.");
                    ImguiUtils.WrappedText("");
                }
                else
                {
                    Handler.ParamTargetElement(ref Handler.CurrentTargetCategory, "The target for the Row Name export.", defaultButtonSize);

                    if (ImGui.Button("Export##action_Selection_ExportRowNames", halfButtonSize))
                    {
                        Handler.ExportRowNameHandler();
                    }

                    ImGui.SameLine();
                    if (ImGui.Button("Open Project Folder##action_Selection_OpenExportFolder", halfButtonSize))
                    {
                        if (Smithbox.ProjectType != ProjectType.Undefined)
                        {
                            var dir = $"{Smithbox.ProjectRoot}\\.smithbox\\Assets\\Paramdex\\{MiscLocator.GetGameIDForDir()}\\Names";
                            Process.Start("explorer.exe", dir);
                        }
                    }
                    ImguiUtils.ShowHoverTooltip("Opens the project-specific Names folder that contains the exported Names.");
                }
            }

            // Trim Row Names
            if (ImGui.CollapsingHeader("Trim Row Names"))
            {
                ImguiUtils.WrappedText("Trim Carriage Return (\\r) characters from row names\nfor the currently selected param, or for all params.");
                ImguiUtils.WrappedText("");

                if (!Screen._activeView._selection.ActiveParamExists())
                {
                    ImguiUtils.WrappedText("You must select a param before you can use this action.");
                    ImguiUtils.WrappedText("");
                }
                else
                {
                    Handler.ParamTargetElement(ref Handler.CurrentTargetCategory, "The target for the Row Name trimming.", defaultButtonSize);

                    if (ImGui.Button("Trim##action_TrimRowNames", defaultButtonSize))
                    {
                        Handler.RowNameTrimHandler();
                    }
                }
            }

            // Sort Rows
            if (ImGui.CollapsingHeader("Sort Rows"))
            {
                ImguiUtils.WrappedText("Sort the rows for the currently selected param by their ID.");
                ImguiUtils.WrappedText("");

                if (ImGui.Button("Sort##action_SortRows", defaultButtonSize))
                {
                    Handler.SortRowsHandler();
                }
            }

            // Merge Params
            if (ImGui.CollapsingHeader("Merge Params"))
            {
                ImguiUtils.WrappedText("Use this to merge a target regulation.bin into your current project.");
                ImguiUtils.WrappedText("");
                ImguiUtils.WrappedText("Merging will bring all unique param rows from the target regulation into your project.");
                ImguiUtils.WrappedText("");
                ImguiUtils.WrappedText("This process is 'simple', and thus may produce a broken mod if you attempt to merge complex mods.");
                ImguiUtils.WrappedText("");

                ImguiUtils.WrappedText("Target Regulation");
                ImguiUtils.ShowHoverTooltip("This is the target regulation.bin you wish to merge.");

                ImGui.SetNextItemWidth(inputBoxSize.X);
                ImGui.InputText("##targetRegulationPath", ref Handler.targetRegulationPath, 255);
                ImGui.SameLine();
                if (ImGui.Button($@"{ForkAwesome.FileO}"))
                {
                    if (PlatformUtils.Instance.OpenFileDialog("Select target regulation.bin...", Handler.allParamTypes, out var path))
                    {
                        Handler.targetRegulationPath = path;
                    }
                }
                ImguiUtils.WrappedText("");

                if (Smithbox.ProjectType is ProjectType.DS2S or ProjectType.DS2)
                {
                    ImguiUtils.WrappedText("Target Loose Params");
                    ImguiUtils.ShowHoverTooltip("This is the target loose param folder you wish to merge.");

                    ImGui.SetNextItemWidth(inputBoxSize.X);
                    ImGui.InputText("##targetLooseParamPath", ref Handler.targetLooseParamPath, 255);
                    ImGui.SameLine();
                    if (ImGui.Button($@"{ForkAwesome.FileO}"))
                    {
                        if (PlatformUtils.Instance.OpenFileDialog("Select target loose param folder...", Handler.allParamTypes, out var path))
                        {
                            Handler.targetLooseParamPath = path;
                        }
                    }
                    ImguiUtils.WrappedText("");

                    ImguiUtils.WrappedText("Target Regulation");
                    ImguiUtils.ShowHoverTooltip("This is the target enemy param you wish to merge.");

                    ImGui.SetNextItemWidth(inputBoxSize.X);
                    ImGui.InputText("##targetEnemyParamPath", ref Handler.targetEnemyParamPath, 255);
                    ImGui.SameLine();
                    if (ImGui.Button($@"{ForkAwesome.FileO}"))
                    {
                        if (PlatformUtils.Instance.OpenFileDialog("Select target loose param folder...", Handler.allParamTypes, out var path))
                        {
                            Handler.targetEnemyParamPath = path;
                        }
                    }
                    ImguiUtils.WrappedText("");
                }

                if (ImGui.Button("Merge##action_MergeParam", defaultButtonSize))
                {
                    Handler.MergeParamHandler();
                }
            }

            // Find Row ID Instances
            if (ImGui.CollapsingHeader("Find Row ID Instances"))
            {
                ImguiUtils.WrappedText("Display all instances of a specificed row ID.");
                ImguiUtils.WrappedText("");

                if (!Smithbox.EditorHandler.ParamEditor._activeView._selection.ActiveParamExists())
                {
                    ImguiUtils.WrappedText("You must select a param before you can use this action.");
                    ImguiUtils.WrappedText("");
                }
                else
                {
                    ImguiUtils.WrappedText("Row ID:");
                    ImGui.SetNextItemWidth(defaultButtonSize.X);
                    ImGui.InputInt("##searchRowId", ref Handler._idRowInstanceFinder_SearchID);
                    ImguiUtils.ShowHoverTooltip("The row ID to search for.");

                    ImguiUtils.WrappedText("Row Index:");
                    ImGui.SetNextItemWidth(defaultButtonSize.X);
                    ImGui.InputInt("##searchRowIndex", ref Handler._idRowInstanceFinder_SearchIndex);
                    ImguiUtils.ShowHoverTooltip("The row index to search for. -1 for any");

                    ImguiUtils.WrappedText("");

                    Handler.DisplayRowIDInstances();

                    ImguiUtils.WrappedText("");
                }

                if (ImGui.Button("Search##action_SearchForRowIDs", defaultButtonSize))
                {
                    Handler.RowIDInstanceHandler();
                }
            }

            // Find Row Value Instances
            if (ImGui.CollapsingHeader("Find Row Value Instances"))
            {
                ImguiUtils.WrappedText("Display all instances of a specificed value.");
                ImguiUtils.WrappedText("");

                if (!Smithbox.EditorHandler.ParamEditor._activeView._selection.ActiveParamExists())
                {
                    ImguiUtils.WrappedText("You must select a param before you can use this action.");
                    ImguiUtils.WrappedText("");
                }
                else
                {
                    ImguiUtils.WrappedText("Value:");
                    ImGui.SetNextItemWidth(defaultButtonSize.X);
                    ImGui.InputText("##searchValue", ref Handler._searchValue, 255);
                    ImguiUtils.ShowHoverTooltip("The value to search for.");

                    ImGui.Checkbox("Initial Match Only", ref CFG.Current.Param_Toolbar_FindValueInstances_InitialMatchOnly);
                    ImguiUtils.ShowHoverTooltip("Only display the first match within a param, instead of all matches.");
                    ImguiUtils.WrappedText("");

                    Handler.DisplayRowValueInstances();

                    if (ImGui.Button("Search##action_SearchForRowValues", defaultButtonSize))
                    {
                        Handler.RowValueInstanceHandler();
                    }
                }
            }

            // Mass Edit Window
            if (ImGui.CollapsingHeader("Mass Edit - Window"))
            {
                var Size = ImGui.GetWindowSize();
                float EditX = (Size.X * 0.975f);
                float EditY = (Size.Y * 0.1f);

                ImguiUtils.WrappedText("Write and execute mass edit commands here.");
                ImguiUtils.WrappedText("");

                // Options
                ImGui.Checkbox("Retain Input", ref MassEditHandler.retainMassEditCommand);
                ImguiUtils.ShowWideHoverTooltip("Retain the mass edit command in the input text area after execution.");
                ImguiUtils.WrappedText("");

                // AutoFill
                var res = AutoFill.MassEditCompleteAutoFill();
                if (res != null)
                {
                    MassEditHandler._currentMEditRegexInput = MassEditHandler._currentMEditRegexInput + res;
                }

                // Input
                ImguiUtils.WrappedTextColored(CFG.Current.ImGui_AliasName_Text, "Input:");

                ImGui.InputTextMultiline("##MEditRegexInput", ref MassEditHandler._currentMEditRegexInput, 65536,
                new Vector2(EditX * Smithbox.GetUIScale(), EditY * Smithbox.GetUIScale()));


                if (ImGui.Button("Apply##action_Selection_MassEdit_Execute", halfButtonSize))
                {
                    var command = MassEditHandler._currentMEditRegexInput;
                    MassEditHandler.ExecuteMassEdit();
                    if (MassEditHandler.retainMassEditCommand)
                    {
                        MassEditHandler._currentMEditRegexInput = command;
                    }
                }
                ImGui.SameLine();
                if (ImGui.Button("Clear##action_Selection_MassEdit_Clear", halfButtonSize))
                {
                    MassEditHandler._currentMEditRegexInput = "";
                }

                ImGui.Text("");

                // Output
                ImguiUtils.WrappedTextColored(CFG.Current.ImGui_AliasName_Text, "Output:");
                ImguiUtils.ShowWideHoverTooltip("Success state of the Mass Edit command that was previously used.\n\nRemember to handle clipboard state between edits with the 'clear' command");
                ImGui.SameLine();
                ImguiUtils.WrappedText($"{MassEditHandler._mEditRegexResult}");

                ImGui.InputTextMultiline("##MEditRegexOutput", ref MassEditHandler._lastMEditRegexInput, 65536,
                    new Vector2(EditX * Smithbox.GetUIScale(), EditY * Smithbox.GetUIScale()), ImGuiInputTextFlags.ReadOnly);
                ImguiUtils.WrappedText("");
            }

            // Mass Edit Scripts
            if (ImGui.CollapsingHeader("Mass Edit - Scripts"))
            {
                MassEditHandler.MassEditScriptSetup();

                ImguiUtils.WrappedText("Load and edit mass edit scripts here.");
                ImguiUtils.WrappedText("");

                // Ignore the combo box if no files exist
                if (MassEditScript.scriptList.Count > 0)
                {
                    ImguiUtils.WrappedText("Existing Scripts:");

                    // Scripts
                    ImGui.SetNextItemWidth(defaultButtonSize.X);
                    if (ImGui.BeginCombo("##massEditScripts", MassEditHandler._selectedMassEditScript.name))
                    {
                        foreach (var script in MassEditScript.scriptList)
                        {
                            if (ImGui.Selectable(script.name, MassEditHandler._selectedMassEditScript.name == script.name))
                            {
                                MassEditHandler._selectedMassEditScript = script;
                            }
                        }

                        ImGui.EndCombo();
                    }
                    if (MassEditHandler._selectedMassEditScript != null)
                    {
                        if (ImGui.Button("Load", thirdButtonSize))
                        {
                            MassEditHandler._currentMEditRegexInput = MassEditHandler._selectedMassEditScript.GenerateMassedit();
                        }
                        ImGui.SameLine();
                        if (ImGui.Button("Edit", thirdButtonSize))
                        {
                            MassEditHandler._newScriptName = MassEditHandler._selectedMassEditScript.name;
                            MassEditHandler._newScriptBody = MassEditHandler._selectedMassEditScript.GenerateMassedit();
                        }
                        ImGui.SameLine();
                    }

                    if (ImGui.Button("Reload", thirdButtonSize))
                    {
                        MassEditScript.ReloadScripts();
                    }
                }

                ImguiUtils.WrappedText("");

                ImGui.SetNextItemWidth(defaultButtonSize.X);
                ImguiUtils.WrappedText("New Script:");
                ImGui.InputText("##scriptName", ref MassEditHandler._newScriptName, 255);
                ImguiUtils.ShowHoverTooltip("The file name used for this script.");
                ImguiUtils.WrappedText("");

                var Size = ImGui.GetWindowSize();
                float EditX = (Size.X / 100) * 975;
                float EditY = (Size.Y / 100) * 10;

                ImguiUtils.WrappedText("Script:");
                ImguiUtils.ShowHoverTooltip("The mass edit script.");
                ImGui.InputTextMultiline("##newMassEditScript", ref MassEditHandler._newScriptBody, 65536, new Vector2(EditX * Smithbox.GetUIScale(), EditY * Smithbox.GetUIScale()));
                ImguiUtils.WrappedText("");

                if (ImGui.Button("Save", halfButtonSize))
                {
                    MassEditHandler.SaveMassEditScript();
                }
                ImGui.SameLine();
                if (ImGui.Button("Open Script Folder", halfButtonSize))
                {
                    var projectScriptDir = $"{Smithbox.ProjectRoot}\\.smithbox\\Assets\\MassEditScripts\\";

                    Process.Start("explorer.exe", projectScriptDir);
                }
            }

            // Pin Groups
            if (ImGui.CollapsingHeader("Pin Groups"))
            {
                PinGroupHandler.Display();
            }

            // Param Reloader
            if (ParamMemoryTools.IsParamReloaderSupported())
            {
                if (ImGui.CollapsingHeader("Param Reloader"))
                {
                    ImguiUtils.WrappedText("WARNING: Param Reloader only works for existing row entries.\nGame must be restarted for new rows and modified row IDs.");
                    ImguiUtils.WrappedText("");

                    if (ImGui.Button("Reload Current Param", defaultButtonSize))
                    {
                        ParamMemoryTools.ReloadCurrentParam();
                    }

                    if (ImGui.Button("Reload All Params", defaultButtonSize))
                    {
                        ParamMemoryTools.ReloadAllParams();
                    }
                }
            }

            // Item Gib
            if (Smithbox.ProjectType == ProjectType.DS3)
            {
                if (ImGui.CollapsingHeader("Item Gib"))
                {
                    ImguiUtils.WrappedText("Use this tool to spawn an item in-game. First, select an EquipParam row within the Param Editor.");
                    ImguiUtils.WrappedText("");

                    var activeParam = Smithbox.EditorHandler.ParamEditor._activeView._selection.GetActiveParam();

                    if (activeParam == "EquipParamGoods")
                    {
                        ImguiUtils.WrappedText("Number of Spawned Items");
                        ImGui.InputInt("##spawnItemCount", ref ParamMemoryTools.SpawnedItemAmount);
                    }
                    if (activeParam == "EquipParamWeapon")
                    {
                        ImguiUtils.WrappedText("Reinforcement of Spawned Weapon");
                        ImGui.InputInt("##spawnWeaponLevel", ref ParamMemoryTools.SpawnWeaponLevel);
                        if(ParamMemoryTools.SpawnWeaponLevel > 10)
                        {
                            ParamMemoryTools.SpawnWeaponLevel = 10;
                        }
                    }

                    ImguiUtils.WrappedText("");
                    if (ImGui.Button("Give Item", defaultButtonSize))
                    {
                        ParamMemoryTools.GiveItem();
                    }

                }
            }
        }

        ImGui.End();
        ImGui.PopStyleColor(1);
    }

    

}
