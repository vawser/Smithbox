using ImGuiNET;
using StudioCore.AssetLocator;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Interface;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Veldrid;

namespace StudioCore.Editors.ParamEditor
{
    public class ParamMassEditView
    {
        private ActionManager EditorActionManager;

        public static string _currentMEditRegexInput = "";
        private string _lastMEditRegexInput = "";
        private string _mEditRegexResult = "";

        private MassEditScript _selectedMassEditScript;

        public static bool SelectMassEditTab = false;
        private bool massEditTab = true;

        public ParamMassEditView(ActionManager actionManager)
        {
            EditorActionManager = actionManager;
        }

        public void QuickAdd(string text)
        {
            if (CFG.Current.Param_MassEdit_ShowAddButtons)
            {
                if (ImGui.Button($"Add##{text}"))
                {
                    _currentMEditRegexInput = $"{_currentMEditRegexInput}{text}";
                }
                ImGui.SameLine();
            }
        }

        public void ExecuteMassEdit()
        {
            ParamEditorScreen._activeView._selection.SortSelection();
            (MassEditResult r, ActionManager child) = MassParamEditRegex.PerformMassEdit(ParamBank.PrimaryBank,
                _currentMEditRegexInput, ParamEditorScreen._activeView._selection);

            if (child != null)
            {
                EditorActionManager.PushSubManager(child);
            }

            if (r.Type == MassEditResultType.SUCCESS)
            {
                _lastMEditRegexInput = _currentMEditRegexInput;
                _currentMEditRegexInput = "";
                TaskManager.Run(new TaskManager.LiveTask("Param - Check Differences",
                    TaskManager.RequeueType.Repeat,
                    true, TaskLogs.LogPriority.Low,
                    () => ParamBank.RefreshAllParamDiffCaches(false)));
            }

            _mEditRegexResult = r.Information;
        }

        public void OnGui()
        {
            if (Project.Type == ProjectType.Undefined)
                return;

            if (MassEditScript.scriptList.Count > 0)
            {
                if (_selectedMassEditScript == null)
                {
                    _selectedMassEditScript = MassEditScript.scriptList[0];
                }
            }

            if (ImGui.BeginTabBar("##massEditTools"))
            {
                // This is done so the Scripts tab can force user focus to the Execution tab
                if (SelectMassEditTab)
                {
                    SelectMassEditTab = false;
                    if (ImGui.BeginTabItem("Mass Edit##massEditExecution", ref massEditTab, ImGuiTabItemFlags.SetSelected))
                    {
                        DisplayMassEdit();

                        ImGui.EndTabItem();
                    }
                }
                // But we don't want to allow the tab to be closed, so switch back
                // to the flagless version as soon as possible
                else
                {
                    if (ImGui.BeginTabItem("Edit##massEditExecution"))
                    {
                        DisplayMassEdit();

                        ImGui.EndTabItem();
                    }
                }

                if (ImGui.BeginTabItem("Scripts##massEditScripts"))
                {
                    DisplayMassEditScripts();

                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }

            // Wiki
            if (CFG.Current.Param_MassEdit_ShowWiki)
            {
                ImGui.Separator();

                DisplayMassEditWiki();
            }
        }

        public void DisplayMassEdit()
        {
            var scale = Smithbox.GetUIScale();

            // Input
            ImGui.AlignTextToFramePadding();
            ImGui.Text("Input:");
            ImguiUtils.ShowWideHoverTooltip("Input your mass edit command here.");

            ImGui.InputTextMultiline("##MEditRegexInput", ref _currentMEditRegexInput, 65536,
                new Vector2(1024, ImGui.GetTextLineHeightWithSpacing() * 4) * scale);

            // Execute 
            if (ImGui.Button("Execute"))
            {
                ExecuteMassEdit();
            }
            ImGui.SameLine();
            // Clear
            if (ImGui.Button("Clear"))
            {
                _currentMEditRegexInput = "";
            }

            // Output
            ImGui.Separator();
            ImGui.Text($"Output: {_mEditRegexResult}");
            ImguiUtils.ShowWideHoverTooltip("Success state of the Mass Edit command that was previously used.\n\nRemember to handle clipboard state between edits with the 'clear' command");

            ImGui.InputTextMultiline("##MEditRegexOutput", ref _lastMEditRegexInput, 65536,
                new Vector2(1024, ImGui.GetTextLineHeightWithSpacing() * 4) * scale, ImGuiInputTextFlags.ReadOnly);
        }

        public void DisplayMassEditScripts()
        {
            var scale = Smithbox.GetUIScale();

            ImGui.Separator();
            ImGui.Text("Configured Scripts:");
            ImGui.Separator();

            // Ignore the combo box if no files exist
            if (MassEditScript.scriptList.Count > 0)
            {
                // Scripts
                if (ImGui.BeginCombo("##massEditScripts", _selectedMassEditScript.name))
                {
                    foreach (var script in MassEditScript.scriptList)
                    {
                        if (ImGui.Selectable(script.name, _selectedMassEditScript.name == script.name))
                        {
                            _selectedMassEditScript = script;
                        }
                    }

                    ImGui.EndCombo();
                }

                ImGui.SameLine();
                if (ImGui.Button("Reload"))
                {
                    MassEditScript.ReloadScripts();
                }

                if (_selectedMassEditScript != null)
                {
                    if (ImGui.Button("Load"))
                    {
                        _currentMEditRegexInput = _selectedMassEditScript.GenerateMassedit();
                        SelectMassEditTab = true;
                    }


                }
            }

            ImGui.Separator();
            ImGui.Text("New Script:");
            ImGui.Separator();

            ImGui.Text("Name:");
            ImGui.InputText("##scriptName", ref _newScriptName, 255);
            ImguiUtils.ShowHoverTooltip("The file name used for this script.");

            ImGui.Text("Script:");
            ImguiUtils.ShowHoverTooltip("The mass edit script.");
            ImGui.InputTextMultiline("##newMassEditScript", ref _newScriptBody, 65536, new Vector2(1024, ImGui.GetTextLineHeightWithSpacing() * 24) * scale);

            if (ImGui.Button("Save"))
            {
                var dir = ParamAssetLocator.GetMassEditScriptGameDir();
                var path = Path.Combine(dir, $"{_newScriptName}.txt");

                if(!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                if (!File.Exists(path))
                {
                    File.WriteAllText(path, _newScriptBody);
                    _newScriptName = "";
                    _newScriptBody = "";
                }

                MassEditScript.ReloadScripts();
            }
            ImGui.SameLine();
            if(ImGui.Button("Open Script Folder"))
            {
                var dir = ParamAssetLocator.GetMassEditScriptGameDir();
                Process.Start("explorer.exe", dir);
            }
        }

        private string _newScriptName = "";
        private string _newScriptBody = "";

        public void DisplayMassEditWiki()
        {
            if (ImGui.BeginTabBar("##massEditWiki"))
            {
                if (ImGui.BeginTabItem("Overview"))
                {
                    ImGui.Text("Mass edit exists to make large batch-edits according to a simple scheme.");
                    ImGui.Text("\n");
                    ImGui.Text("A basic mass edit command is comprised of three selectors and an operation:");
                    ImGui.Text("1. Param Selection");
                    ImGui.Text("2. Row Selection");
                    ImGui.Text("3. Field Selection");
                    ImGui.Text("4. Field Value Operation");
                    ImGui.Text("\n");
                    ImGui.Text("A more advanced mass edit command can instead perform a row operation:");
                    ImGui.Text("1. Param Selection");
                    ImGui.Text("2. Row Selection");
                    ImGui.Text("3. Row Operation");

                    ImGui.Text("return a value that can then be operated on.");
                    ImGui.Text("\n");
                    ImGui.Text("Selection in each category can be controlled more precisely by");
                    ImGui.Text("using the && characters to combine multiple selection criteria.");
                    ImGui.Text("\n");
                    ImGui.Text("Row and field operations can make use of operation arguments, which");
                    ImGui.Text("return a value that can then be operated on.");
                    ImGui.Text("\n");
                    ImGui.Text("Variables can be used to hold a value.");

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Regex"))
                {
                    // Character Classes
                    ImGui.Separator();
                    ImGui.Text("Character Classes:");
                    ImGui.Separator();

                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, ".       ");
                    ImGui.SameLine();
                    ImGui.Text("Any character except new line.");

                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "\\w      ");
                    ImGui.SameLine();
                    ImGui.Text("Any word.");

                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "\\d      ");
                    ImGui.SameLine();
                    ImGui.Text("Any digit.");

                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "\\s      ");
                    ImGui.SameLine();
                    ImGui.Text("Any whitespace.");

                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "[abc]   ");
                    ImGui.SameLine();
                    ImGui.Text("Any of the listed characters.");

                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "[^abc]  ");
                    ImGui.SameLine();
                    ImGui.Text("Not any of the listed characters.");

                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "[a-g]   ");
                    ImGui.SameLine();
                    ImGui.Text("Any characters between the two specified characters.");

                    // Anchors
                    ImGui.Separator();
                    ImGui.Text("Anchors:");
                    ImGui.Separator();

                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "^abc    ");
                    ImGui.SameLine();
                    ImGui.Text("Start of the string.");

                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "abc$    ");
                    ImGui.SameLine();
                    ImGui.Text("End of the string.");

                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "\\b      ");
                    ImGui.SameLine();
                    ImGui.Text("Word.");

                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "\\B      ");
                    ImGui.SameLine();
                    ImGui.Text("Not-word boundary.");

                    // Escaped Characters
                    ImGui.Separator();
                    ImGui.Text("Escaped Characters:");
                    ImGui.Separator();

                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "\\.      ");
                    ImGui.SameLine();
                    ImGui.Text("Use to match a .");

                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "\\*      ");
                    ImGui.SameLine();
                    ImGui.Text("Use to match a *");

                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "\\\\      ");
                    ImGui.SameLine();
                    ImGui.Text("Use to match a \\");

                    // Groups & Lookaround
                    ImGui.Separator();
                    ImGui.Text("Groups & Lookaround:");
                    ImGui.Separator();

                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "(abc)   ");
                    ImGui.SameLine();
                    ImGui.Text("Capture group.");

                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "\\1      ");
                    ImGui.SameLine();
                    ImGui.Text("Backreference to first capture group.");

                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "(?:abc) ");
                    ImGui.SameLine();
                    ImGui.Text("Non-capturing group.");

                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "(?=abc) ");
                    ImGui.SameLine();
                    ImGui.Text("Positive lookahead.");

                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "(?!abc) ");
                    ImGui.SameLine();
                    ImGui.Text("Negative lookahead.");

                    // Quantifiers & Alternation
                    ImGui.Separator();
                    ImGui.Text("Quantifiers & Alternation:");
                    ImGui.Separator();

                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "a*      ");
                    ImGui.SameLine();
                    ImGui.Text("Zero or more.");

                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "a+      ");
                    ImGui.SameLine();
                    ImGui.Text("One or more.");

                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "a?      ");
                    ImGui.SameLine();
                    ImGui.Text("One or zero.");

                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "a{5}    ");
                    ImGui.SameLine();
                    ImGui.Text("Exactly the specified number.");

                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "a{2,}   ");
                    ImGui.SameLine();
                    ImGui.Text("Two or more.");

                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "a{1,3}  ");
                    ImGui.SameLine();
                    ImGui.Text("Between one and three.");

                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "?       ");
                    ImGui.SameLine();
                    ImGui.Text("Match as few as possible.");

                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "ab|cd   ");
                    ImGui.SameLine();
                    ImGui.Text("Match either.");

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Param Selectors"))
                {
                    QuickAdd("selection: ");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "selection: ");
                    ImGui.Text("Selects the current param selection and selected rows in that param.");
                    ImGui.Text("\n");

                    QuickAdd("clipboard: ");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "clipboard: ");
                    ImGui.Text("Selects the param of the clipboard and the rows in the clipboard.");
                    ImGui.Text("\n");

                    QuickAdd("param: ");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "param: <param>");
                    ImGui.Text("Selects all params whose name matches the given regex.");
                    ImGui.Text("\n");

                    QuickAdd("modified: ");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "modified: ");
                    ImGui.Text("Selects params where any rows do not match the vanilla version,");
                    ImGui.Text("or where any are added. Ignores row names.");
                    ImGui.Text("\n");

                    QuickAdd("auxparam: ");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "auxparam: <parambank>");
                    ImGui.Text("Selects params from the specified regulation or");
                    ImGui.Text("parambnd where the param name matches the given regex.");
                    ImGui.Text("\n");

                    QuickAdd("vars: ");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "vars: <string>");
                    ImGui.Text("Selects variables whose name matches the given regex.");
                    ImGui.Text("\n");

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Row Selectors"))
                {
                    QuickAdd("selection: ");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "selection: ");
                    ImGui.Text("Selects the current param selection and selected rows in that param.");
                    ImGui.Text("\n");

                    QuickAdd("clipboard: ");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "clipboard: ");
                    ImGui.Text("Selects the param of the clipboard and the rows in the clipboard.");
                    ImGui.Text("\n");

                    QuickAdd("modified: ");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "modified: ");
                    ImGui.Text("Selects rows which do not match the vanilla version, or are added.");
                    ImGui.Text("Ignores row name");
                    ImGui.Text("\n");

                    QuickAdd("added: ");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "added: ");
                    ImGui.Text("Selects rows where the ID is not found in the vanilla param.");
                    ImGui.Text("\n");

                    QuickAdd("id: ");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "id: <string>");
                    ImGui.Text("Selects rows whose ID matches the given regex.");
                    ImGui.Text("\n");

                    QuickAdd("idrange: ");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "idrange: <min> <max>");
                    ImGui.Text("Selects rows whose ID falls in the given numerical range.");
                    ImGui.Text("Minimum and maximum are inclusive.");
                    ImGui.Text("\n");

                    QuickAdd("name: ");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "name: <string>");
                    ImGui.Text("Selects rows whose Name matches the given regex.");
                    ImGui.Text("\n");

                    QuickAdd("prop: ");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "prop: <string> <value>");
                    ImGui.Text("Selects rows where the specified field has a value that matches the given regex.");
                    ImGui.Text("\n");

                    QuickAdd("proprange: ");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "proprange: <string> <min> <max>");
                    ImGui.Text("Selects rows where the specified field has a value that falls in the given numerical range.");
                    ImGui.Text("Minimum and maximum are inclusive.");
                    ImGui.Text("\n");

                    QuickAdd("propref: ");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "propref: <field> <name>");
                    ImGui.Text("Selects rows where the specified field that references another param");
                    ImGui.Text("has a value referencing a row whose name matches the given regex.");
                    ImGui.Text("\n");

                    QuickAdd("propwhere: ");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "propref: <field> <selector>");
                    ImGui.Text("Selects rows where the specified field appears when");
                    ImGui.Text("the given cell or field search is given");
                    ImGui.Text("\n");

                    QuickAdd("mergeable: ");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "mergeable: ");
                    ImGui.Text("Selects rows which are not modified in the primary regulation or");
                    ImGui.Text("parambnd and there is exactly one equivalent row in another regulation ");
                    ImGui.Text("or parambnd that is modified.");
                    ImGui.Text("\n");

                    QuickAdd("conflicts: ");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "conflicts: ");
                    ImGui.Text("Selects rows which, among all equivalents in the primary and additional");
                    ImGui.Text("regulations or parambnds, there is more than 1 row which is modified");
                    ImGui.Text("\n");

                    QuickAdd("fmg:");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "fmg: <string>");
                    ImGui.Text("Selects rows which have an attached FMG and that FMG's text matches the given regex.");
                    ImGui.Text("\n");

                    QuickAdd("vanillaprop: ");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "vanillaprop: <field> <value>");
                    ImGui.Text("Selects rows where the vanilla equivilent of that row has a value");
                    ImGui.Text("for the given field that matches the given regex");
                    ImGui.Text("\n");

                    QuickAdd("vanillaproprange: ");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "vanillaproprange: <string> <min> <max>");
                    ImGui.Text("Selects rows where the vanilla equivilent of that row has a value for");
                    ImGui.Text("the given field that falls in the given numerical range");
                    ImGui.Text("\n");

                    QuickAdd("auxprop: ");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "name: <parambank> <field> <value>");
                    ImGui.Text("Selects rows where the equivilent of that row in the given regulation or parambnd");
                    ImGui.Text("has a value for the given field that matches the given regex.");
                    ImGui.Text("Can be used to determine if an aux row exists.");
                    ImGui.Text("\n");

                    QuickAdd("auxproprange: ");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "auxproprange: <parambank> <field> <min> <max>");
                    ImGui.Text("Selects rows where the equivilent of that row in the given regulation or parambnd");
                    ImGui.Text("has a value for the given field that falls in the given range");
                    ImGui.Text("\n");

                    QuickAdd("semijoin: ");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "semijoin: <field> <parambank> <field> <row selector>");
                    ImGui.Text("Selects all rows where the value of a given field is any of the values in");
                    ImGui.Text("the second given field found in the given param using the given row selector.");
                    ImGui.Text("\n");

                    QuickAdd("unique: ");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "unique: <field>");
                    ImGui.Text("Selects all rows where the value in the given field is unique.");
                    ImGui.Text("\n");

                    QuickAdd("vars: ");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "vars: <string>");
                    ImGui.Text("Selects variables whose name matches the given regex.");
                    ImGui.Text("\n");

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Field Selectors"))
                {
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "<field>");
                    ImGui.Text("Selects cells/fields where the internal name of that field matches the given regex.");
                    ImGui.Text("\n");

                    QuickAdd("modified: ");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "modified: ");
                    ImGui.Text("Selects cells/fields where the equivalent cell in the vanilla regulation");
                    ImGui.Text("or parambnd has a different value");
                    ImGui.Text("\n");

                    QuickAdd("auxmodified: ");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "auxmodified: <parambank>");
                    ImGui.Text("Selects cells/fields where the equivalent cell in the specified regulation");
                    ImGui.Text("or parambnd has a different value.");
                    ImGui.Text("\n");

                    QuickAdd("sftype: ");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "sftype: <type>");
                    ImGui.Text("Selects cells/fields where the field's data type, as enumerated by ");
                    ImGui.Text("soulsformats, matches the given regex.");
                    ImGui.Text("\n");

                    QuickAdd("vars: ");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "vars: <string>");
                    ImGui.Text("Selects variables whose name matches the given regex.");
                    ImGui.Text("\n");

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Global Operations"))
                {
                    QuickAdd("clear");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "clear");
                    ImGui.Text("Clears clipboard param and rows.");
                    ImGui.Text("\n");

                    QuickAdd("newvar");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "newvar <name> <value>");
                    ImGui.Text("Creates a variable with the given value, and the type of that value.");
                    ImGui.Text("\n");

                    QuickAdd("clearvars");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "clearvars");
                    ImGui.Text("Deletes all variables.");
                    ImGui.Text("\n");

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Row Operations"))
                {
                    QuickAdd("copy");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "copy");
                    ImGui.Text("Adds the selected rows into clipboard.");
                    ImGui.Text("If the clipboard param is different, the clipboard is emptied first");
                    ImGui.Text("\n");

                    QuickAdd("copyN");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "copyN <value>");
                    ImGui.Text("Adds the selected rows into clipboard the given number of times.");
                    ImGui.Text("If the clipboard param is different, the clipboard is emptied first");
                    ImGui.Text("\n");

                    QuickAdd("paste");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "paste");
                    ImGui.Text("Adds the selected rows to the primary regulation or parambnd in");
                    ImGui.Text("the selected param");
                    ImGui.Text("\n");

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Field Operations"))
                {
                    QuickAdd("=");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "= <value>");
                    ImGui.Text("Assigns the given value to the selected values.");
                    ImGui.Text("Will attempt conversion to the value's data type");
                    ImGui.Text("\n");

                    QuickAdd("+");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "+ <value>");
                    ImGui.Text("Adds the number to the selected values, or appends text");
                    ImGui.Text("if that is the data type of the values.");
                    ImGui.Text("\n");

                    QuickAdd("-");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "- <value>");
                    ImGui.Text("Subtracts the number from the selected values.");
                    ImGui.Text("\n");

                    QuickAdd("*");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "* <value>");
                    ImGui.Text("Multiplies selected values by the number.");
                    ImGui.Text("\n");

                    QuickAdd("/");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "/ <value>");
                    ImGui.Text("Divides the selected values by the number");
                    ImGui.Text("\n");

                    QuickAdd("%");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "% <value>");
                    ImGui.Text("Gives the remainder when the selected values are divided by the number.");
                    ImGui.Text("\n");

                    QuickAdd("scale");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "scale <factor> <center>");
                    ImGui.Text("Multiplies the difference between the selected values and the center number by the factor number.");
                    ImGui.Text("\n");

                    QuickAdd("replace");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "replace <search string> <replace string>");
                    ImGui.Text("Interprets the selected values as text and replaces all occurances");
                    ImGui.Text("of the text to replace with the new text");
                    ImGui.Text("\n");

                    QuickAdd("replacex");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "replacex <search string> <replace string>");
                    ImGui.Text("Interprets the selected values as text and replaces all occurances");
                    ImGui.Text("of the given regex with the replacement, supporting regex groups");
                    ImGui.Text("\n");

                    QuickAdd("max");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "max <value>");
                    ImGui.Text("Returns the larger of the current value and number.");
                    ImGui.Text("\n");

                    QuickAdd("min");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "min <value>");
                    ImGui.Text("Returns the smaller of the current value and number.");
                    ImGui.Text("\n");

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Operation Arguments"))
                {
                    QuickAdd("self");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "self");
                    ImGui.Text("Gives the value of the currently selected value.");
                    ImGui.Text("\n");

                    QuickAdd("field");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "field <field>");
                    ImGui.Text("Gives the value of the given cell/field for the currently selected row and param");
                    ImGui.Text("\n");

                    QuickAdd("vanilla");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "vanilla");
                    ImGui.Text("Gives the value of the equivalent cell/field in the vanilla regulation or parambnd");
                    ImGui.Text("for the currently selected cell/field, row and param.");
                    ImGui.Text("Will fail if a row does not have a vanilla equivilent.");
                    ImGui.Text("Consider using && !added");
                    ImGui.Text("\n");

                    QuickAdd("aux");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "aux <parambank>");
                    ImGui.Text("Gives the value of the equivalent cell/field in the specified regulation or parambnd");
                    ImGui.Text("for the currently selected cell/field, row and param.");
                    ImGui.Text("Will fail if a row does not have a aux equivilent.");
                    ImGui.Text("Consider using && !added");
                    ImGui.Text("\n");

                    QuickAdd("vanillafield");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "vanillafield <field>");
                    ImGui.Text("Gives the value of the specified cell/field in the vanilla regulation or parambnd");
                    ImGui.Text("for the currently selected cell/field, row and param.");
                    ImGui.Text("Will fail if a row does not have a vanilla equivilent.");
                    ImGui.Text("Consider using && !added");
                    ImGui.Text("\n");

                    QuickAdd("auxfield");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "auxfield <parambank> <field>");
                    ImGui.Text("Gives the value of the specified cell/field in the aux regulation or parambnd");
                    ImGui.Text("for the currently selected cell/field, row and param.");
                    ImGui.Text("Will fail if a row does not have an aux equivilent.");
                    ImGui.Text("Consider using && !added");
                    ImGui.Text("\n");

                    QuickAdd("paramlookup");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "paramlookup <param> <row> <field>");
                    ImGui.Text("Returns the specific value specified by the exact param, row and field.");
                    ImGui.Text("\n");

                    QuickAdd("average");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "average <field> <row selector>");
                    ImGui.Text("Gives the mean value of the cells/fields found using the given selector,");
                    ImGui.Text("for the currently selected param");
                    ImGui.Text("\n");

                    QuickAdd("median");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "median <field> <row selector>");
                    ImGui.Text("Gives the median value of the cells/fields found using the given selector,");
                    ImGui.Text("for the currently selected param");
                    ImGui.Text("\n");

                    QuickAdd("mode");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "mode <field> <row selector>");
                    ImGui.Text("Gives the mode value of the cells/fields found using the given selector,");
                    ImGui.Text("for the currently selected param");
                    ImGui.Text("\n");

                    QuickAdd("min");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "min <field> <row selector>");
                    ImGui.Text("Gives the smallest value of the cells/fields found using the given selector,");
                    ImGui.Text("for the currently selected param");
                    ImGui.Text("\n");

                    QuickAdd("max");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "max <field> <row selector>");
                    ImGui.Text("Gives the largest value of the cells/fields found using the given selector,");
                    ImGui.Text("for the currently selected param");
                    ImGui.Text("\n");

                    QuickAdd("random");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "random <min> <max>");
                    ImGui.Text("Gives a random decimal number between the given values for each selected value.");
                    ImGui.Text("Minimum and maximum are inclusive.");
                    ImGui.Text("\n");

                    QuickAdd("randint");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "randint <min> <max>");
                    ImGui.Text("Gives a random integer number between the given values for each selected value.");
                    ImGui.Text("Minimum and maximum are inclusive.");
                    ImGui.Text("\n");

                    QuickAdd("randFrom");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "randFrom <param> <field> <row selector>");
                    ImGui.Text("Gives a random value from the cells/fields found using the given param,");
                    ImGui.Text("row selector and field, for each selected value.");
                    ImGui.Text("\n");

                    QuickAdd("paramIndex");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "paramIndex");
                    ImGui.Text("Gives an integer for the current selected param,");
                    ImGui.Text("beginning at 0 and increasing by 1 for each param selected.");
                    ImGui.Text("\n");

                    QuickAdd("rowIndex");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "rowIndex");
                    ImGui.Text("Gives an integer for the current selected row,");
                    ImGui.Text("beginning at 0 and increasing by 1 for each row selected.");
                    ImGui.Text("\n");

                    QuickAdd("fieldIndex");
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "fieldIndex");
                    ImGui.Text("Gives an integer for the current selected field,");
                    ImGui.Text("beginning at 0 and increasing by 1 for each field selected.");
                    ImGui.Text("\n");

                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }
        }
    }
}
