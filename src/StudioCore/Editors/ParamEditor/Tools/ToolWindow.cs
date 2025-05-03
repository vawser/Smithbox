using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editors.ParamEditor.Actions;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Resource.Locators;
using StudioCore.Utilities;
using System.Diagnostics;
using System.Numerics;

namespace StudioCore.Editors.ParamEditor.Tools;

public class ToolWindow
{
    private ParamEditorScreen Editor;
    public ActionHandler Handler;
    public MassEditHandler MassEditHandler;
    public PinGroups PinGroupHandler;

    public ToolWindow(ParamEditorScreen screen)
    {
        Editor = screen;
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
        if (Editor.Project.ProjectType == ProjectType.Undefined)
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

                if (!Editor._activeView._selection.RowSelectionExists())
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

                    UIHelper.WrappedText("Deep Copy:");
                    UIHelper.ShowHoverTooltip("If any of these options are enabled, then the tagged fields within the duplicated row will be affected by the duplication offset.\n\nThis lets you easily duplicate sets of rows where the fields tend to refer to other rows (e.g. bullets).");

                    // Specific toggles
                    ImGui.Checkbox("Affect Attack Field", ref CFG.Current.Param_Toolbar_Duplicate_AffectAttackField);
                    UIHelper.ShowHoverTooltip("Fields tagged as 'Attack' will have the offset applied to their value.\n\nExample: the Attack reference in a Bullet row.");

                    ImGui.Checkbox("Affect Bullet Field", ref CFG.Current.Param_Toolbar_Duplicate_AffectBulletField);
                    UIHelper.ShowHoverTooltip("Fields tagged as 'Bullet' will have the offset applied to their value.\n\nExample: the Bullet references in a Bullet row.");

                    ImGui.Checkbox("Affect Behavior Field", ref CFG.Current.Param_Toolbar_Duplicate_AffectBehaviorField);
                    UIHelper.ShowHoverTooltip("Fields tagged as 'Behavior' will have the offset applied to their value.\n\nExamples: the Reference ID field in a BehaviorParam row.");

                    ImGui.Checkbox("Affect SpEffect Field", ref CFG.Current.Param_Toolbar_Duplicate_AffectSpEffectField);
                    UIHelper.ShowHoverTooltip("Fields tagged as 'SpEffect' will have the offset applied to their value.\n\nExample: the SpEffect references in a Bullet row.");

                    ImGui.Checkbox("Affect Equipment Origin Field", ref CFG.Current.Param_Toolbar_Duplicate_AffectSourceField);
                    UIHelper.ShowHoverTooltip("Fields tagged as 'Source' will have the offset applied to their value.\n\nExamples: the Source ID references in an EquipParamProtector row.");


                    if (ImGui.Button("Duplicate##duplicateRow", defaultButtonSize))
                    {
                        Handler.DuplicateHandler();
                    }
                }
            }

            // Duplicate Row to Commutative Param
            if (ImGui.CollapsingHeader("Duplicate Row to Commutative Param"))
            {
                UIHelper.WrappedText("Duplicate the selected rows to another param that shares the same underlying structure.");
                UIHelper.WrappedText("");

                if (!Editor._activeView._selection.RowSelectionExists())
                {
                    UIHelper.WrappedText("You must select a row before you can use this action.");
                    UIHelper.WrappedText("");
                }
                else
                {
                    UIHelper.WrappedText("Offset:");

                    ImGui.SetNextItemWidth(defaultButtonSize.X);
                    ImGui.InputInt("##Offset", ref CFG.Current.Param_Toolbar_CommutativeDuplicate_Offset);
                    UIHelper.ShowHoverTooltip("The ID offset to apply when duplicating.");
                    UIHelper.WrappedText("");

                    ImGui.Checkbox("Replace Rows in Target Param", ref CFG.Current.Param_Toolbar_CommutativeDuplicate_ReplaceExistingRows);
                    UIHelper.ShowHoverTooltip("If enabled, rows in the target will be overwritten when duplicating into a commutative param.");

                    Handler.DisplayCommutativeDuplicateToolMenu();

                    if (ImGui.Button("Duplicate##duplicateRow", defaultButtonSize))
                    {
                        Handler.CommutativeDuplicateHandler();
                    }
                }
            }

            // Import Row Names
            if (ImGui.CollapsingHeader("Import Row Names"))
            {
                UIHelper.WrappedText("Import row names for the currently selected param, or for all params.");
                UIHelper.WrappedText("");

                if (!Editor._activeView._selection.ActiveParamExists())
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
                        if (Editor.Project.ProjectType != ProjectType.Undefined)
                        {
                            var dir = $"{Editor.Project.ProjectPath}\\.smithbox\\Assets\\PARAM\\{MiscLocator.GetGameIDForDir()}\\Names";
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

                if (!Editor._activeView._selection.ActiveParamExists())
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
                        if (Editor.Project.ProjectType != ProjectType.Undefined)
                        {
                            var dir = $"{Editor.Project.ProjectPath}\\.smithbox\\Assets\\PARAM\\{MiscLocator.GetGameIDForDir()}\\Names";
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

                if (!Editor._activeView._selection.ActiveParamExists())
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

            ImGui.Separator();

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
                        MassEditScript.ReloadScripts(Editor);
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
                    var projectScriptDir = $"{Editor.Project.ProjectPath}\\.smithbox\\Assets\\Scripts\\";

                    Process.Start("explorer.exe", projectScriptDir);
                }
            }

            ImGui.Separator();

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

                if (Editor.Project.ProjectType is ProjectType.DS2S or ProjectType.DS2)
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

            // Param Reloader
            if (ParamMemoryTools.IsParamReloaderSupported(Editor))
            {
                if (ImGui.CollapsingHeader("Param Reloader"))
                {
                    UIHelper.WrappedText("WARNING: Param Reloader only works for existing row entries.\nGame must be restarted for new rows and modified row IDs.");
                    UIHelper.WrappedText("");

                    if (ImGui.Button("Reload Current Param", defaultButtonSize))
                    {
                        ParamMemoryTools.ReloadCurrentParam(Editor);
                    }
                    UIHelper.ShowHoverTooltip($"{KeyBindings.Current.PARAM_ReloadParam.HintText}");

                    if (ImGui.Button("Reload All Params", defaultButtonSize))
                    {
                        ParamMemoryTools.ReloadAllParams(Editor);
                    }
                    UIHelper.ShowHoverTooltip($"{KeyBindings.Current.PARAM_ReloadAllParams.HintText}");
                }
            }

            // Item Gib
            if (Editor.Project.ProjectType is ProjectType.DS3)
            {
                if (ImGui.CollapsingHeader("Item Gib"))
                {
                    UIHelper.WrappedText("Use this tool to spawn an item in-game. First, select an EquipParam row within the Param Editor.");
                    UIHelper.WrappedText("");

                    var activeParam = Editor._activeView._selection.GetActiveParam();

                    if (activeParam == "EquipParamGoods")
                    {
                        UIHelper.WrappedText("Number of Spawned Items");
                        ImGui.InputInt("##spawnItemCount", ref ParamMemoryTools.SpawnedItemAmount);
                    }
                    if (activeParam == "EquipParamWeapon")
                    {
                        UIHelper.WrappedText("Reinforcement of Spawned Weapon");
                        ImGui.InputInt("##spawnWeaponLevel", ref ParamMemoryTools.SpawnWeaponLevel);

                        if (Editor.Project.ProjectType is ProjectType.DS3)
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
                        ParamMemoryTools.GiveItem(Editor);
                    }

                }
            }

            ImGui.Separator();

            // Find Field Instances
            if (ImGui.CollapsingHeader("Find Field Name Instances"))
            {
                Editor.FieldNameFinder.Display();
            }

            // Find Field Value Instances
            if (ImGui.CollapsingHeader("Find Field Value Instances"))
            {
                Editor.FieldValueFinder.Display();
            }

            // Find Row Name Instances
            if (ImGui.CollapsingHeader("Find Row Name Instances"))
            {
                Editor.RowNameFinder.Display();
            }

            // Find Row ID Instances
            if (ImGui.CollapsingHeader("Find Row ID Instances"))
            {
                Editor.RowIDFinder.Display();
            }

            ImGui.Separator();

            // Pin Groups
            if (ImGui.CollapsingHeader("Pin Groups"))
            {
                PinGroupHandler.Display();
            }

            if (ImGui.CollapsingHeader("Param Categories"))
            {
                ParamCategories.Display(Editor);
            }
        }

        ImGui.End();
        ImGui.PopStyleColor(1);
    }

    

}
