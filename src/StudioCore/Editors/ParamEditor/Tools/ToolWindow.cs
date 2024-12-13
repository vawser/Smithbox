using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.Core.Project;
using StudioCore.Editor;
using StudioCore.Editors.ParamEditor.Actions;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Resource.Locators;
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

    public void Shortcuts()
    {
        MassEditHandler.Shortcuts();

        if(InputTracker.GetKeyDown(KeyBindings.Current.PARAM_SortRows))
        {
            Handler.SortRowsHandler();
        }
    }

    public void OnProjectChanged()
    {
        PinGroupHandler.OnProjectChanged();
    }

    public void OnGui()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * DPI.GetUIScale(), ImGuiCond.FirstUseEver);

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
                UIHelper.WrappedText("Duplicate the selected rows.");
                UIHelper.WrappedText("");

                if (!Screen._activeView._selection.RowSelectionExists())
                {
                    UIHelper.WrappedText("You must select a row before you can use this action.");
                    UIHelper.WrappedText("");
                }
                else
                {
                    UIHelper.WrappedText("Amount to Duplicate:");

                    ImGui.SetNextItemWidth(defaultButtonSize.X);
                    ImGui.InputInt("##Amount", ref CFG.Current.Param_Toolbar_Duplicate_Amount);
                    UIHelper.ShowHoverTooltip("The number of times the current selection will be duplicated.");
                    UIHelper.WrappedText("");

                    UIHelper.WrappedText("Duplicate Offset:");

                    ImGui.SetNextItemWidth(defaultButtonSize.X);
                    ImGui.InputInt("##Offset", ref CFG.Current.Param_Toolbar_Duplicate_Offset);
                    UIHelper.ShowHoverTooltip("The ID offset to apply when duplicating.");
                    UIHelper.WrappedText("");

                    if (ImGui.Button("Duplicate##duplicateRow", defaultButtonSize))
                    {
                        Handler.DuplicateHandler();
                    }
                }
            }

            // Import Row Names
            if (ImGui.CollapsingHeader("Import Row Names"))
            {
                UIHelper.WrappedText("Import row names for the currently selected param, or for all params.");
                UIHelper.WrappedText("");

                if (!Screen._activeView._selection.ActiveParamExists())
                {
                    UIHelper.WrappedText("You must select a param before you can use this action.");
                    UIHelper.WrappedText("");
                }
                else
                {
                    Handler.ParamTargetElement(ref Handler.CurrentTargetCategory, "The target for the Row Name import.", defaultButtonSize);
                    Handler.ParamSourceElement(ref Handler.CurrentSourceCategory, "The source of the names used in by the Row Name import.", defaultButtonSize);

                    ImGui.Checkbox("Only replace unmodified row names", ref Handler._rowNameImporter_VanillaOnly);
                    UIHelper.ShowHoverTooltip("Row name import will only replace the name of unmodified rows.");

                    ImGui.Checkbox("Only replace empty row names", ref Handler._rowNameImporter_EmptyOnly);
                    UIHelper.ShowHoverTooltip("Row name import will only replace the name of un-named rows.");
                    UIHelper.WrappedText("");

                    if (ImGui.Button("Import##action_ImportRowNames", halfButtonSize))
                    {
                        Handler.ImportRowNameHandler();
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("Open Project Folder##action_Selection_OpenExportFolder", halfButtonSize))
                    {
                        if (Smithbox.ProjectType != ProjectType.Undefined)
                        {
                            var dir = $"{Smithbox.ProjectRoot}\\.smithbox\\Assets\\PARAM\\{MiscLocator.GetGameIDForDir()}\\Names";
                            Process.Start("explorer.exe", dir);
                        }
                    }
                    UIHelper.ShowHoverTooltip("Opens the project-specific Names folder that contains the Names to be imported.");
                }
            }

            // Export Row Names
            if (ImGui.CollapsingHeader("Export Row Names"))
            {
                UIHelper.WrappedText("Export row names for the currently selected param, or for all params.");
                UIHelper.WrappedText("");

                if (!Screen._activeView._selection.ActiveParamExists())
                {
                    UIHelper.WrappedText("You must select a param before you can use this action.");
                    UIHelper.WrappedText("");
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
                            var dir = $"{Smithbox.ProjectRoot}\\.smithbox\\Assets\\PARAM\\{MiscLocator.GetGameIDForDir()}\\Names";
                            Process.Start("explorer.exe", dir);
                        }
                    }
                    UIHelper.ShowHoverTooltip("Opens the project-specific Names folder that contains the exported Names.");
                }
            }

            // Trim Row Names
            if (ImGui.CollapsingHeader("Trim Row Names"))
            {
                UIHelper.WrappedText("Trim Carriage Return (\\r) characters from row names\nfor the currently selected param, or for all params.");
                UIHelper.WrappedText("");

                if (!Screen._activeView._selection.ActiveParamExists())
                {
                    UIHelper.WrappedText("You must select a param before you can use this action.");
                    UIHelper.WrappedText("");
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
                UIHelper.WrappedText("Sort the rows for the currently selected param by their ID.");
                UIHelper.WrappedText("");

                if (ImGui.Button("Sort##action_SortRows", defaultButtonSize))
                {
                    Handler.SortRowsHandler();
                }
            }

            // Merge Params
            if (ImGui.CollapsingHeader("Merge Params"))
            {
                UIHelper.WrappedText("Use this to merge a target regulation.bin into your current project.");
                UIHelper.WrappedText("");
                UIHelper.WrappedText("Merging will bring all modified param rows from the target regulation into your project.");
                UIHelper.WrappedText("");
                UIHelper.WrappedText("This process is 'simple', and thus may produce a broken mod if you attempt to merge complex mods.");
                UIHelper.WrappedText("");

                UIHelper.WrappedText("Target Regulation");
                UIHelper.ShowHoverTooltip("This is the target regulation.bin you wish to merge.");

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

                ImGui.Checkbox("Unique Only##targetUniqueOnly", ref Handler.targetUniqueOnly);
                UIHelper.ShowHoverTooltip("Only merge in unique param rows from the target regulation. If disabled, all modified rows, even if not unique, will be merged.");
                UIHelper.WrappedText("");

                if (Smithbox.ProjectType is ProjectType.DS2S or ProjectType.DS2)
                {
                    UIHelper.WrappedText("Target Loose Params");
                    UIHelper.ShowHoverTooltip("This is the target loose param folder you wish to merge.");

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
                    UIHelper.WrappedText("");

                    UIHelper.WrappedText("Target Regulation");
                    UIHelper.ShowHoverTooltip("This is the target enemy param you wish to merge.");

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
                    UIHelper.WrappedText("");
                }

                if (ImGui.Button("Merge##action_MergeParam", defaultButtonSize))
                {
                    Handler.MergeParamHandler();
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
                    UIHelper.WrappedText("WARNING: Param Reloader only works for existing row entries.\nGame must be restarted for new rows and modified row IDs.");
                    UIHelper.WrappedText("");

                    if (ImGui.Button("Reload Current Param", defaultButtonSize))
                    {
                        ParamMemoryTools.ReloadCurrentParam();
                    }
                    UIHelper.ShowHoverTooltip($"{KeyBindings.Current.PARAM_ReloadParam.HintText}");

                    if (ImGui.Button("Reload All Params", defaultButtonSize))
                    {
                        ParamMemoryTools.ReloadAllParams();
                    }
                    UIHelper.ShowHoverTooltip($"{KeyBindings.Current.PARAM_ReloadAllParams.HintText}");
                }
            }

            // Item Gib
            if (Smithbox.ProjectType is ProjectType.DS3)
            {
                if (ImGui.CollapsingHeader("Item Gib"))
                {
                    UIHelper.WrappedText("Use this tool to spawn an item in-game. First, select an EquipParam row within the Param Editor.");
                    UIHelper.WrappedText("");

                    var activeParam = Smithbox.EditorHandler.ParamEditor._activeView._selection.GetActiveParam();

                    if (activeParam == "EquipParamGoods")
                    {
                        UIHelper.WrappedText("Number of Spawned Items");
                        ImGui.InputInt("##spawnItemCount", ref ParamMemoryTools.SpawnedItemAmount);
                    }
                    if (activeParam == "EquipParamWeapon")
                    {
                        UIHelper.WrappedText("Reinforcement of Spawned Weapon");
                        ImGui.InputInt("##spawnWeaponLevel", ref ParamMemoryTools.SpawnWeaponLevel);

                        if (Smithbox.ProjectType is ProjectType.DS3)
                        {
                            if (ParamMemoryTools.SpawnWeaponLevel > 10)
                            {
                                ParamMemoryTools.SpawnWeaponLevel = 10;
                            }
                        }
                    }

                    UIHelper.WrappedText("");
                    if (ImGui.Button("Give Item", defaultButtonSize))
                    {
                        ParamMemoryTools.GiveItem();
                    }

                }
            }

            // Mass Edit Window
            if (ImGui.CollapsingHeader("Mass Edit - Window"))
            {
                var Size = ImGui.GetWindowSize();
                float EditX = (Size.X * 0.975f);
                float EditY = (Size.Y * 0.1f);

                UIHelper.WrappedText("Write and execute mass edit commands here.");
                UIHelper.WrappedText("");

                // Options
                ImGui.Checkbox("Retain Input", ref MassEditHandler.retainMassEditCommand);
                UIHelper.ShowWideHoverTooltip("Retain the mass edit command in the input text area after execution.");
                UIHelper.WrappedText("");

                // AutoFill
                var res = AutoFill.MassEditCompleteAutoFill();
                if (res != null)
                {
                    MassEditHandler._currentMEditRegexInput = MassEditHandler._currentMEditRegexInput + res;
                }

                // Input
                UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, "Input:");

                ImGui.InputTextMultiline("##MEditRegexInput", ref MassEditHandler._currentMEditRegexInput, 65536,
                new Vector2(EditX * DPI.GetUIScale(), EditY * DPI.GetUIScale()));

                if (ImGui.Button("Apply##action_Selection_MassEdit_Execute", halfButtonSize))
                {
                    MassEditHandler.ExecuteMassEdit();
                }
                UIHelper.ShowHoverTooltip($"{KeyBindings.Current.PARAM_ExecuteMassEdit.HintText}");


                ImGui.SameLine();
                if (ImGui.Button("Clear##action_Selection_MassEdit_Clear", halfButtonSize))
                {
                    MassEditHandler._currentMEditRegexInput = "";
                }

                ImGui.Text("");

                // Output
                UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, "Output:");
                UIHelper.ShowWideHoverTooltip("Success state of the Mass Edit command that was previously used.\n\nRemember to handle clipboard state between edits with the 'clear' command");
                ImGui.SameLine();
                UIHelper.WrappedText($"{MassEditHandler._mEditRegexResult}");

                ImGui.InputTextMultiline("##MEditRegexOutput", ref MassEditHandler._lastMEditRegexInput, 65536,
                    new Vector2(EditX * DPI.GetUIScale(), EditY * DPI.GetUIScale()), ImGuiInputTextFlags.ReadOnly);
                UIHelper.WrappedText("");
            }

            // Mass Edit Scripts
            if (ImGui.CollapsingHeader("Mass Edit - Scripts"))
            {
                MassEditHandler.MassEditScriptSetup();

                UIHelper.WrappedText("Load and edit mass edit scripts here.");
                UIHelper.WrappedText("");

                // Ignore the combo box if no files exist
                if (MassEditScript.scriptList.Count > 0)
                {
                    UIHelper.WrappedText("Existing Scripts:");

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

                UIHelper.WrappedText("");

                ImGui.SetNextItemWidth(defaultButtonSize.X);
                UIHelper.WrappedText("New Script:");
                ImGui.InputText("##scriptName", ref MassEditHandler._newScriptName, 255);
                UIHelper.ShowHoverTooltip("The file name used for this script.");
                UIHelper.WrappedText("");

                var Size = ImGui.GetWindowSize();
                float EditX = (Size.X / 100) * 975;
                float EditY = (Size.Y / 100) * 10;

                UIHelper.WrappedText("Script:");
                UIHelper.ShowHoverTooltip("The mass edit script.");
                ImGui.InputTextMultiline("##newMassEditScript", ref MassEditHandler._newScriptBody, 65536, new Vector2(EditX * DPI.GetUIScale(), EditY * DPI.GetUIScale()));
                UIHelper.WrappedText("");

                if (ImGui.Button("Save", halfButtonSize))
                {
                    MassEditHandler.SaveMassEditScript();
                }
                ImGui.SameLine();
                if (ImGui.Button("Open Script Folder", halfButtonSize))
                {
                    var projectScriptDir = $"{Smithbox.ProjectRoot}\\.smithbox\\Assets\\Scripts\\";

                    Process.Start("explorer.exe", projectScriptDir);
                }
            }

            ImGui.Separator(); 

            // Find Field Instances
            if (ImGui.CollapsingHeader("Find Field Instances"))
            {
                UIHelper.WrappedText("Display all fields and the respective params they appear in based on the search string.");
                UIHelper.WrappedText("");

                if (!Smithbox.EditorHandler.ParamEditor._activeView._selection.ActiveParamExists())
                {
                    UIHelper.WrappedText("You must select a param before you can use this action.");
                    UIHelper.WrappedText("");
                }
                else
                {
                    UIHelper.WrappedText("Search Text:");
                    ImGui.SetNextItemWidth(defaultButtonSize.X);
                    ImGui.InputText("##searchString", ref Handler._idFieldInstanceFinder_SearchString, 255);
                    UIHelper.ShowHoverTooltip("The field string to search for.");

                    ImGui.Checkbox("Include Descriptions in Search##matchDescriptions", ref Handler._idFieldInstanceFinder_matchWiki);
                    UIHelper.ShowHoverTooltip("Include the description text for a field in the search.");

                    ImGui.Checkbox("Display Community Names in Result##useCommunityNamesInResults", ref Handler._idFieldInstanceFinder_displayCommunityName);
                    UIHelper.ShowHoverTooltip("Display the community name for the field instead of the internal name.");

                    UIHelper.WrappedText("");

                    Handler.DisplayFieldInstances();

                    UIHelper.WrappedText("");
                }

                if (ImGui.Button("Search##action_SearchForFieldInstances", defaultButtonSize))
                {
                    Handler.FieldInstanceHandler();
                }
            }

            // Find Row ID Instances
            if (ImGui.CollapsingHeader("Find Row ID Instances"))
            {
                UIHelper.WrappedText("Display all instances of a specificed row ID.");
                UIHelper.WrappedText("");

                if (!Smithbox.EditorHandler.ParamEditor._activeView._selection.ActiveParamExists())
                {
                    UIHelper.WrappedText("You must select a param before you can use this action.");
                    UIHelper.WrappedText("");
                }
                else
                {
                    UIHelper.WrappedText("Row ID:");
                    ImGui.SetNextItemWidth(defaultButtonSize.X);
                    ImGui.InputInt("##searchRowId", ref Handler._idRowInstanceFinder_SearchID);
                    UIHelper.ShowHoverTooltip("The row ID to search for.");

                    UIHelper.WrappedText("Row Index:");
                    ImGui.SetNextItemWidth(defaultButtonSize.X);
                    ImGui.InputInt("##searchRowIndex", ref Handler._idRowInstanceFinder_SearchIndex);
                    UIHelper.ShowHoverTooltip("The row index to search for. -1 for any");

                    UIHelper.WrappedText("");

                    Handler.DisplayRowIDInstances();

                    UIHelper.WrappedText("");
                }

                if (ImGui.Button("Search##action_SearchForRowIDs", defaultButtonSize))
                {
                    Handler.RowIDInstanceHandler();
                }
            }

            // Find Row Name Instances
            if (ImGui.CollapsingHeader("Find Row Name Instances"))
            {
                UIHelper.WrappedText("Display all instances of a specificed row ID.");
                UIHelper.WrappedText("");

                if (!Smithbox.EditorHandler.ParamEditor._activeView._selection.ActiveParamExists())
                {
                    UIHelper.WrappedText("You must select a param before you can use this action.");
                    UIHelper.WrappedText("");
                }
                else
                {
                    UIHelper.WrappedText("Row ID:");
                    ImGui.SetNextItemWidth(defaultButtonSize.X);
                    ImGui.InputInt("##searchRowId", ref Handler._idRowInstanceFinder_SearchID);
                    UIHelper.ShowHoverTooltip("The row ID to search for.");

                    UIHelper.WrappedText("Row Index:");
                    ImGui.SetNextItemWidth(defaultButtonSize.X);
                    ImGui.InputInt("##searchRowIndex", ref Handler._idRowInstanceFinder_SearchIndex);
                    UIHelper.ShowHoverTooltip("The row index to search for. -1 for any");

                    UIHelper.WrappedText("");

                    Handler.DisplayRowIDInstances();

                    UIHelper.WrappedText("");
                }

                if (ImGui.Button("Search##action_SearchForRowIDs", defaultButtonSize))
                {
                    Handler.RowIDInstanceHandler();
                }
            }

            // Find Row Value Instances
            if (ImGui.CollapsingHeader("Find Row Value Instances"))
            {
                UIHelper.WrappedText("Display all instances of a specified value.");
                UIHelper.WrappedText("");

                if (!Smithbox.EditorHandler.ParamEditor._activeView._selection.ActiveParamExists())
                {
                    UIHelper.WrappedText("You must select a param before you can use this action.");
                    UIHelper.WrappedText("");
                }
                else
                {
                    UIHelper.WrappedText("Value:");
                    ImGui.SetNextItemWidth(defaultButtonSize.X);
                    ImGui.InputText("##searchValue", ref Handler._searchValue, 255);
                    UIHelper.ShowHoverTooltip("The value to search for.");

                    ImGui.Checkbox("Initial Match Only", ref CFG.Current.Param_Toolbar_FindValueInstances_InitialMatchOnly);
                    UIHelper.ShowHoverTooltip("Only display the first match within a param, instead of all matches.");
                    UIHelper.WrappedText("");

                    Handler.DisplayRowValueInstances();

                    if (ImGui.Button("Search##action_SearchForRowValues", defaultButtonSize))
                    {
                        Handler.RowValueInstanceHandler();
                    }
                }
            }

        }

        ImGui.End();
        ImGui.PopStyleColor(1);
    }

    

}
