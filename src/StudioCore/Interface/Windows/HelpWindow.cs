using ImGuiNET;
using StudioCore.Banks.HelpBank;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.ParamEditor.Toolbar;
using StudioCore.Help;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace StudioCore.Interface.Windows;

public class HelpWindow
{
    private readonly HelpBank _helpBank;

    // Article
    private readonly string _inputStr_Article = "";

    // Glossary
    private readonly string _inputStr_Glossary = "";

    // Tutorial
    private readonly string _inputStr_Tutorial = "";
    private readonly string _inputStrCache_Article = "";
    private readonly string _inputStrCache_Glossary = "";
    private readonly string _inputStrCache_Tutorial = "";
    private readonly int textSectionForceSplitCharCount = 2760;

    private readonly string textSectionSplitter = "[-----]";
    private string _id;
    private HelpEntry _selectedEntry_Article;
    private HelpEntry _selectedEntry_Glossary;
    private HelpEntry _selectedEntry_Tutorial;

    private bool MenuOpenState;

    public HelpWindow()
    {
        _helpBank = new HelpBank();
    }

    public void ToggleMenuVisibility()
    {
        MenuOpenState = !MenuOpenState;
    }

    public void Display()
    {
        var scale = Smithbox.GetUIScale();

        if (!MenuOpenState)
            return;

        ImGui.SetNextWindowSize(new Vector2(600.0f, 600.0f) * scale, ImGuiCond.FirstUseEver);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, CFG.Current.Imgui_Moveable_MainBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBg, CFG.Current.Imgui_Moveable_TitleBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, CFG.Current.Imgui_Moveable_TitleBg_Active);
        ImGui.PushStyleColor(ImGuiCol.ChildBg, CFG.Current.Imgui_Moveable_ChildBg);
        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(10.0f, 10.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(20.0f, 10.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.IndentSpacing, 20.0f * scale);

        if (ImGui.Begin("Help##Popup", ref MenuOpenState, ImGuiWindowFlags.NoDocking))
        {
            ImGui.BeginTabBar("#HelpMenuTabBar");
            ImGui.PushStyleColor(ImGuiCol.Header, CFG.Current.Imgui_Moveable_Header);
            ImGui.PushItemWidth(300f);

            DisplayHelpSection("Article", _helpBank.GetArticles(), _inputStr_Article, _inputStrCache_Article,
                "Articles", "No title.", "No article selected.");
            DisplayHelpSection("Tutorial", _helpBank.GetTutorials(), _inputStr_Tutorial, _inputStrCache_Tutorial,
                "Tutorials", "No title.", "No tutorial selected.");
            DisplayHelpSection("Glossary", _helpBank.GetGlossaryEntries(), _inputStr_Glossary,
                _inputStrCache_Glossary, "Glossary", "No title.", "No term selected.");
            DisplayMassEditHelp();
            DisplayRegexHelp();
            DisplayLinks();
            DisplayCredits();

            ImGui.PopItemWidth();
            ImGui.PopStyleColor();
            ImGui.EndTabBar();
        }

        ImGui.End();

        ImGui.PopStyleVar(3);
        ImGui.PopStyleColor(5);
    }

    private void DisplayHelpSection(string sectionType, List<HelpEntry> entries, string inputStr,
        string inputStrCache, string title, string noSelection_Title, string noSelection_Body)
    {
        if (entries.Count < 1)
            return;

        if (ImGui.BeginTabItem(title))
        {
            // Search Area
            ImGui.InputText("Search", ref inputStr, 255);

            // Selection Area
            ImGui.BeginChild($"{title}SectionList", new Vector2(600, 100), true, ImGuiWindowFlags.NoScrollbar);

            if (inputStr.ToLower() != inputStrCache.ToLower())
                inputStrCache = inputStr.ToLower();

            var lowercaseInput = _inputStr_Article.ToLower();

            foreach (HelpEntry entry in entries)
            {
                var sectionName = entry.Title;
                List<string> contents = entry.Contents;
                List<string> tags = entry.Tags;

                // Section Title Segments
                List<string> sectionNameSegments = new();

                foreach (var segment in sectionName.Split(" ").ToList())
                {
                    var segmentParts = segment.Split(" ").ToList();
                    foreach (var part in segmentParts)
                        sectionNameSegments.Add(part.ToLower());
                }

                // Content Segments
                List<string> coreSegments = new();

                foreach (var segment in contents)
                {
                    var segmentParts = segment.Split(" ").ToList();
                    foreach (var part in segmentParts)
                        coreSegments.Add(part.ToLower());
                }

                // Tags
                List<string> tagList = new();

                foreach (var segment in tags)
                    tagList.Add(segment.ToLower());

                // Only show if input matches any title or content segments, or if it is blank
                if (lowercaseInput.ToLower() == "" || sectionNameSegments.Contains(lowercaseInput) ||
                    coreSegments.Contains(lowercaseInput) || tagList.Contains(lowercaseInput))
                {
                    if (ImGui.Selectable(sectionName))
                    {
                    }

                    if (ImGui.IsItemClicked() && ImGui.IsMouseDoubleClicked(0))
                        switch (sectionType)
                        {
                            case "Article":
                                _selectedEntry_Article = entry;
                                break;
                            case "Tutorial":
                                _selectedEntry_Tutorial = entry;
                                break;
                            case "Glossary":
                                _selectedEntry_Glossary = entry;
                                break;
                        }
                }
            }

            ImGui.EndChild();

            ImGui.Separator();

            switch (sectionType)
            {
                case "Article":
                    DisplayHelpSection(_selectedEntry_Article, noSelection_Title, noSelection_Body);
                    break;
                case "Tutorial":
                    DisplayHelpSection(_selectedEntry_Tutorial, noSelection_Title, noSelection_Body);
                    break;
                case "Glossary":
                    DisplayHelpSection(_selectedEntry_Glossary, noSelection_Title, noSelection_Body);
                    break;
            }

            ImGui.Separator();
            ImGui.EndTabItem();
        }
    }

    private void DisplayHelpSection(HelpEntry entry, string noSelection_Title, string noSelection_Body)
    {
        // No selection
        if (entry == null)
        {
            ImGui.Text(noSelection_Title);
            ImGui.Separator();
            ImGui.Text(noSelection_Body);
        }
        // Selection
        else
        {
            ImGui.Text(entry.Title);
            ImGui.Separator();
            foreach (var textSection in GetDisplayTextSections(entry.Contents))
                ImGui.Text(textSection);
        }
    }
    public void QuickAdd(string text)
    {
        if (ImGui.Button($"Add##{text}"))
        {
            ParamAction_MassEdit._currentMEditRegexInput = $"{ParamAction_MassEdit._currentMEditRegexInput}{text}";
        }
        ImGui.SameLine();
    }

    private void DisplayMassEditHelp()
    {
        if (ImGui.BeginTabItem("Mass Edit"))
        {
            if (ImGui.BeginTabBar("##MassEditWiki"))
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
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "%% <value>");
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

            ImGui.EndTabItem();
        }
    }
    private void DisplayRegexHelp()
    {
        if (ImGui.BeginTabItem("Regex"))
        {
            if (ImGui.BeginTabBar("##RegexWiki"))
            {
                if (ImGui.BeginTabItem("Character Classes"))
                {
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

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Anchors"))
                {
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

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Escaped Characters"))
                {
                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "\\.      ");
                    ImGui.SameLine();
                    ImGui.Text("Use to match a .");

                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "\\*      ");
                    ImGui.SameLine();
                    ImGui.Text("Use to match a *");

                    ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, "\\\\      ");
                    ImGui.SameLine();
                    ImGui.Text("Use to match a \\");

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Groups & Lookaround"))
                {
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

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Quantifiers & Alternation"))
                {
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

                ImGui.EndTabBar();
            }

            ImGui.EndTabItem();
        }
    }

    private void DisplayLinks()
    {
        if (ImGui.BeginTabItem("Links"))
        {
            ImGui.Indent();

            ImGui.Text("Below are a set of community links. Clicking them will take you to the associated URL.");

            foreach (LinkEntry entry in _helpBank.GetLinks())
                if (ImGui.Button($"{entry.Title}"))
                    Process.Start(new ProcessStartInfo { FileName = $"{entry.URL}", UseShellExecute = true });

            ImGui.Unindent();
            ImGui.EndTabItem();
        }
    }

    private void DisplayCredits()
    {
        if (ImGui.BeginTabItem("Credits"))
        {
            ImGui.Indent();

            ImGui.Text(GetDisplayText(_helpBank.GetCredits().Text));

            ImGui.Unindent();
            ImGui.EndTabItem();
        }
    }

    private string GetDisplayText(List<string> stringList)
    {
        var charCount = 0;

        var displayText = "";
        foreach (var str in stringList)
        {
            displayText = displayText + str + "\n";
            charCount = charCount + str.Length;

            // Force seperator if text length is close to the Imgui.Text character limit.
            if (charCount >= textSectionForceSplitCharCount)
            {
                charCount = 0;
                displayText = displayText + textSectionSplitter + "";
            }
            else
                // If the current str already includes a separator, reset the count
                // As GetDisplayTextSections will handle the separator instead
                if (str.Contains(textSectionSplitter))
                charCount = 0;
        }

        return displayText;
    }

    private List<string> GetDisplayTextSections(List<string> stringList)
    {
        var displayTextFull = GetDisplayText(stringList);

        var displayTextSegments = displayTextFull.Split(textSectionSplitter).ToList();

        return displayTextSegments;
    }
}
