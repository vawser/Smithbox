using ImGuiNET;
using StudioCore.Banks.HelpBank;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.ParamEditor.Toolbar;
using StudioCore.Help;
using StudioCore.UserProject;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

namespace StudioCore.Interface.Windows;

public class HelpWindow
{
    private readonly HelpBank _helpBank;

    // Article
    private string _inputStr_Article = "";

    // Glossary
    private string _inputStr_Glossary = "";

    // Tutorial
    private string _inputStr_Tutorial = "";
    private string _inputStrCache_Article = "";
    private string _inputStrCache_Glossary = "";
    private string _inputStrCache_Tutorial = "";
    private int textSectionForceSplitCharCount = 2760;

    private string textSectionSplitter = "[-----]";
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

            DisplayArticles();
            DisplayTutorials();
            DisplayGlossary();

            if (ImGui.BeginTabItem("Mass Edit"))
            {
                DisplayMassEditHelp();

                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("Regex"))
            {
                DisplayRegexHelp();

                ImGui.EndTabItem();
            }

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

    private bool MatchEntry(HelpEntry entry, string inputStr)
    {
        bool isMatch = false;

        var lowercaseInput = inputStr.ToLower();

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
        {
            tagList.Add(segment.ToLower());
        }

        // Only show if input matches any title or content segments, or if it is blank
        if (lowercaseInput.ToLower() == "" || sectionNameSegments.Contains(lowercaseInput) || coreSegments.Contains(lowercaseInput) || tagList.Contains(lowercaseInput))
        {
            isMatch = true;
        }

        return isMatch;
    }

    /// <summary>
    /// Articles
    /// </summary>
    private void DisplayArticles()
    {
        if (_helpBank.GetArticles().Count < 1)
            return;

        if (ImGui.BeginTabItem("Articles"))
        {
            ImGui.Columns(2);

            // Search Area
            ImGui.InputText("Search", ref _inputStr_Article, 255);

            // Selection Area
            ImGui.BeginChild($"ArticleSectionList");

            if (_inputStr_Article.ToLower() != _inputStrCache_Article.ToLower())
                _inputStrCache_Article = _inputStr_Article.ToLower();

            foreach (HelpEntry entry in _helpBank.GetArticles())
            {
                var sectionName = entry.Title;

                if (entry.ProjectType == (int)Project.Type || entry.ProjectType == 0)
                {
                    if (MatchEntry(entry, _inputStr_Article))
                    {
                        if (ImGui.Selectable(sectionName, _selectedEntry_Article == entry))
                        {
                            _selectedEntry_Article = entry;
                        }
                    }
                }
            }

            ImGui.EndChild();

            ImGui.NextColumn();

            ImGui.BeginChild($"ArticleSectionView");

            foreach (HelpEntry entry in _helpBank.GetArticles())
            {
                if (_selectedEntry_Article == entry)
                {
                    // No selection
                    if (entry == null)
                    {
                        ImguiUtils.WrappedText("No article selected");
                        ImGui.Separator();
                        ImguiUtils.WrappedText("");
                    }
                    // Selection
                    else
                    {
                        foreach (var textSection in entry.Contents)
                        {
                            ProcessText(entry, textSection);
                        }
                    }
                }
            }

            ImGui.EndChild();

            ImGui.Columns(1);

            ImGui.EndTabItem();
        }
    }

    /// <summary>
    /// Tutorials
    /// </summary>
    private void DisplayTutorials()
    {
        if (_helpBank.GetArticles().Count < 1)
            return;

        if (ImGui.BeginTabItem("Tutorials"))
        {
            ImGui.Columns(2);

            // Search Area
            ImGui.InputText("Search", ref _inputStr_Tutorial, 255);

            // Selection Area
            ImGui.BeginChild($"TutorialSectionList");

            if (_inputStr_Tutorial.ToLower() != _inputStrCache_Tutorial.ToLower())
                _inputStrCache_Tutorial = _inputStr_Tutorial.ToLower();

            foreach (HelpEntry entry in _helpBank.GetTutorials())
            {
                var sectionName = entry.Title;

                if (entry.ProjectType == (int)Project.Type || entry.ProjectType == 0)
                {
                    if (MatchEntry(entry, _inputStr_Tutorial))
                    {
                        if (ImGui.Selectable(sectionName, _selectedEntry_Tutorial == entry))
                        {
                            _selectedEntry_Tutorial = entry;
                        }
                    }
                }
            }

            ImGui.EndChild();

            ImGui.NextColumn();

            ImGui.BeginChild($"TutorialSectionView");

            foreach (HelpEntry entry in _helpBank.GetTutorials())
            {
                if (_selectedEntry_Tutorial == entry)
                {
                    // No selection
                    if (entry == null)
                    {
                        ImguiUtils.WrappedText("No tutorial selected");
                        ImGui.Separator();
                        ImguiUtils.WrappedText("");
                    }
                    // Selection
                    else
                    {
                        foreach (var textSection in entry.Contents)
                        {
                            ProcessText(entry, textSection);
                        }
                    }
                }
            }

            ImGui.EndChild();

            ImGui.Columns(1);

            ImGui.EndTabItem();
        }
    }

    /// <summary>
    /// Glossary
    /// </summary>
    private void DisplayGlossary()
    {
        if (_helpBank.GetGlossaryEntries().Count < 1)
            return;

        if (ImGui.BeginTabItem("Glossary"))
        {
            ImGui.Columns(2);

            // Search Area
            ImGui.InputText("Search", ref _inputStr_Glossary, 255);

            // Selection Area
            ImGui.BeginChild($"GlossarySectionList");

            if (_inputStr_Glossary.ToLower() != _inputStrCache_Glossary.ToLower())
                _inputStrCache_Glossary = _inputStr_Glossary.ToLower();

            foreach (HelpEntry entry in _helpBank.GetGlossaryEntries())
            {
                var sectionName = entry.Title;

                if (entry.ProjectType == (int)Project.Type || entry.ProjectType == 0)
                {
                    if (MatchEntry(entry, _inputStr_Glossary))
                    {
                        if (ImGui.Selectable(sectionName, _selectedEntry_Glossary == entry))
                        {
                            _selectedEntry_Glossary = entry;
                        }
                    }
                }
            }

            ImGui.EndChild();

            ImGui.NextColumn();

            ImGui.BeginChild($"GlossarySectionView");

            foreach (HelpEntry entry in _helpBank.GetGlossaryEntries())
            {
                if (_selectedEntry_Glossary == entry)
                {
                    // No selection
                    if (entry == null)
                    {
                        ImguiUtils.WrappedText("No glossary entry selected");
                        ImGui.Separator();
                        ImguiUtils.WrappedText("");
                    }
                    // Selection
                    else
                    {
                        foreach (var textSection in entry.Contents)
                        {
                            ProcessText(entry, textSection);
                        }
                    }
                }
            }

            ImGui.EndChild();

            ImGui.Columns(1);

            ImGui.EndTabItem();
        }
    }

    private void ProcessText(HelpEntry entry, string textLine)
    {
        var outputLine = textLine;

        if(textLine.Contains("[Header]"))
        {
            outputLine = textLine.Replace("[Header]", "");
            ImGui.Separator();

            if (entry.HeaderColor != null)
            {
                Vector4 color = new Vector4(entry.HeaderColor[0], entry.HeaderColor[1], entry.HeaderColor[2], entry.HeaderColor[3]);
                ImguiUtils.WrappedTextColored(color, outputLine);
            }
            else
            {
                ImguiUtils.WrappedText(outputLine);
            }

            ImGui.Separator();
        }
        else if(textLine.Contains("[Highlight@"))
        {
            var pattern = $@"\[Highlight\@(.*)\](.*)";
            var match = Regex.Match(textLine, pattern);

            if(match.Success && match.Groups.Count >= 2)
            {
                string highlightText = match.Groups[1].Value;
                string otherText = match.Groups[2].Value;

                if (entry.HighlightColor != null)
                {
                    Vector4 color = new Vector4(entry.HighlightColor[0], entry.HighlightColor[1], entry.HighlightColor[2], entry.HighlightColor[3]);
                    ImguiUtils.WrappedTextColored(color, highlightText);

                    var offset = highlightText.Length * 8.0f;
                    ImGui.SameLine(offset);
                    ImguiUtils.WrappedText(otherText);
                }
                else
                {
                    ImguiUtils.WrappedText(highlightText);
                    ImGui.SameLine();
                    ImguiUtils.WrappedText(otherText);
                }
            }
        }
        else
        {

            ImguiUtils.WrappedText(textLine);
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

    private List<string> MassEditTopics = new List<string>()
    {
        "Overview",
        "Param Selectors",
        "Row Selectors",
        "Field Selectors",
        "Global Operations",
        "Row Operations",
        "Field Operations",
        "Operation Arguments"
    };

    private int currentMassEditTopic = 0;

    private void DisplayMassEditHelp()
    {
        ImGui.Columns(2);

        ImGui.BeginChild($"MassEditSectionList");

        for(int i = 0; i < MassEditTopics.Count; i++)
        {
            var topic = MassEditTopics[i];

            if (ImGui.Selectable(topic))
            {
                currentMassEditTopic = i;
            }
        }

        ImGui.EndChild();

        ImGui.NextColumn();

        ImGui.BeginChild($"MassEditSectionView");

        if (currentMassEditTopic == 0)
        {
            ImguiUtils.WrappedText("Mass edit exists to make large batch-edits according to a simple scheme.");
            ImguiUtils.WrappedText("\n");
            ImguiUtils.WrappedText("A basic mass edit command is comprised of three selectors and an operation:");
            ImguiUtils.WrappedText("1. Param Selection");
            ImguiUtils.WrappedText("2. Row Selection");
            ImguiUtils.WrappedText("3. Field Selection");
            ImguiUtils.WrappedText("4. Field Value Operation");
            ImguiUtils.WrappedText("\n");
            ImguiUtils.WrappedText("A more advanced mass edit command can instead perform a row operation:");
            ImguiUtils.WrappedText("1. Param Selection");
            ImguiUtils.WrappedText("2. Row Selection");
            ImguiUtils.WrappedText("3. Row Operation");

            ImguiUtils.WrappedText("return a value that can then be operated on.");
            ImguiUtils.WrappedText("\n");
            ImguiUtils.WrappedText("Selection in each category can be controlled more precisely by");
            ImguiUtils.WrappedText("using the && characters to combine multiple selection criteria.");
            ImguiUtils.WrappedText("\n");
            ImguiUtils.WrappedText("Row and field operations can make use of operation arguments, which");
            ImguiUtils.WrappedText("return a value that can then be operated on.");
            ImguiUtils.WrappedText("\n");
            ImguiUtils.WrappedText("Variables can be used to hold a value.");
        }

        if (currentMassEditTopic == 1)
        {
            QuickAdd("selection: ");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "selection: ");
            ImguiUtils.WrappedText("Selects the current param selection and selected rows in that param.");
            ImguiUtils.WrappedText("\n");

            QuickAdd("clipboard: ");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "clipboard: ");
            ImguiUtils.WrappedText("Selects the param of the clipboard and the rows in the clipboard.");
            ImguiUtils.WrappedText("\n");

            QuickAdd("param: ");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "param: <param>");
            ImguiUtils.WrappedText("Selects all params whose name matches the given regex.");
            ImguiUtils.WrappedText("\n");

            QuickAdd("modified: ");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "modified: ");
            ImguiUtils.WrappedText("Selects params where any rows do not match the vanilla version,");
            ImguiUtils.WrappedText("or where any are added. Ignores row names.");
            ImguiUtils.WrappedText("\n");

            QuickAdd("auxparam: ");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "auxparam: <parambank>");
            ImguiUtils.WrappedText("Selects params from the specified regulation or");
            ImguiUtils.WrappedText("parambnd where the param name matches the given regex.");
            ImguiUtils.WrappedText("\n");

            QuickAdd("vars: ");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "vars: <string>");
            ImguiUtils.WrappedText("Selects variables whose name matches the given regex.");
            ImguiUtils.WrappedText("\n");
        }

        if (currentMassEditTopic == 2)
        {
            QuickAdd("selection: ");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "selection: ");
            ImguiUtils.WrappedText("Selects the current param selection and selected rows in that param.");
            ImguiUtils.WrappedText("\n");

            QuickAdd("clipboard: ");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "clipboard: ");
            ImguiUtils.WrappedText("Selects the param of the clipboard and the rows in the clipboard.");
            ImguiUtils.WrappedText("\n");

            QuickAdd("modified: ");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "modified: ");
            ImguiUtils.WrappedText("Selects rows which do not match the vanilla version, or are added.");
            ImguiUtils.WrappedText("Ignores row name");
            ImguiUtils.WrappedText("\n");

            QuickAdd("added: ");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "added: ");
            ImguiUtils.WrappedText("Selects rows where the ID is not found in the vanilla param.");
            ImguiUtils.WrappedText("\n");

            QuickAdd("id: ");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "id: <string>");
            ImguiUtils.WrappedText("Selects rows whose ID matches the given regex.");
            ImguiUtils.WrappedText("\n");

            QuickAdd("idrange: ");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "idrange: <min> <max>");
            ImguiUtils.WrappedText("Selects rows whose ID falls in the given numerical range.");
            ImguiUtils.WrappedText("Minimum and maximum are inclusive.");
            ImguiUtils.WrappedText("\n");

            QuickAdd("name: ");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "name: <string>");
            ImguiUtils.WrappedText("Selects rows whose Name matches the given regex.");
            ImguiUtils.WrappedText("\n");

            QuickAdd("prop: ");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "prop: <string> <value>");
            ImguiUtils.WrappedText("Selects rows where the specified field has a value that matches the given regex.");
            ImguiUtils.WrappedText("\n");

            QuickAdd("proprange: ");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "proprange: <string> <min> <max>");
            ImguiUtils.WrappedText("Selects rows where the specified field has a value that falls in the given numerical range.");
            ImguiUtils.WrappedText("Minimum and maximum are inclusive.");
            ImguiUtils.WrappedText("\n");

            QuickAdd("propref: ");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "propref: <field> <name>");
            ImguiUtils.WrappedText("Selects rows where the specified field that references another param");
            ImguiUtils.WrappedText("has a value referencing a row whose name matches the given regex.");
            ImguiUtils.WrappedText("\n");

            QuickAdd("propwhere: ");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "propref: <field> <selector>");
            ImguiUtils.WrappedText("Selects rows where the specified field appears when");
            ImguiUtils.WrappedText("the given cell or field search is given");
            ImguiUtils.WrappedText("\n");

            QuickAdd("mergeable: ");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "mergeable: ");
            ImguiUtils.WrappedText("Selects rows which are not modified in the primary regulation or");
            ImguiUtils.WrappedText("parambnd and there is exactly one equivalent row in another regulation ");
            ImguiUtils.WrappedText("or parambnd that is modified.");
            ImguiUtils.WrappedText("\n");

            QuickAdd("conflicts: ");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "conflicts: ");
            ImguiUtils.WrappedText("Selects rows which, among all equivalents in the primary and additional");
            ImguiUtils.WrappedText("regulations or parambnds, there is more than 1 row which is modified");
            ImguiUtils.WrappedText("\n");

            QuickAdd("fmg:");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "fmg: <string>");
            ImguiUtils.WrappedText("Selects rows which have an attached FMG and that FMG's text matches the given regex.");
            ImguiUtils.WrappedText("\n");

            QuickAdd("vanillaprop: ");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "vanillaprop: <field> <value>");
            ImguiUtils.WrappedText("Selects rows where the vanilla equivilent of that row has a value");
            ImguiUtils.WrappedText("for the given field that matches the given regex");
            ImguiUtils.WrappedText("\n");

            QuickAdd("vanillaproprange: ");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "vanillaproprange: <string> <min> <max>");
            ImguiUtils.WrappedText("Selects rows where the vanilla equivilent of that row has a value for");
            ImguiUtils.WrappedText("the given field that falls in the given numerical range");
            ImguiUtils.WrappedText("\n");

            QuickAdd("auxprop: ");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "name: <parambank> <field> <value>");
            ImguiUtils.WrappedText("Selects rows where the equivilent of that row in the given regulation or parambnd");
            ImguiUtils.WrappedText("has a value for the given field that matches the given regex.");
            ImguiUtils.WrappedText("Can be used to determine if an aux row exists.");
            ImguiUtils.WrappedText("\n");

            QuickAdd("auxproprange: ");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "auxproprange: <parambank> <field> <min> <max>");
            ImguiUtils.WrappedText("Selects rows where the equivilent of that row in the given regulation or parambnd");
            ImguiUtils.WrappedText("has a value for the given field that falls in the given range");
            ImguiUtils.WrappedText("\n");

            QuickAdd("semijoin: ");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "semijoin: <field> <parambank> <field> <row selector>");
            ImguiUtils.WrappedText("Selects all rows where the value of a given field is any of the values in");
            ImguiUtils.WrappedText("the second given field found in the given param using the given row selector.");
            ImguiUtils.WrappedText("\n");

            QuickAdd("unique: ");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "unique: <field>");
            ImguiUtils.WrappedText("Selects all rows where the value in the given field is unique.");
            ImguiUtils.WrappedText("\n");

            QuickAdd("vars: ");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "vars: <string>");
            ImguiUtils.WrappedText("Selects variables whose name matches the given regex.");
            ImguiUtils.WrappedText("\n");
        }

        if (currentMassEditTopic == 3)
        {
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "<field>");
            ImguiUtils.WrappedText("Selects cells/fields where the internal name of that field matches the given regex.");
            ImguiUtils.WrappedText("\n");

            QuickAdd("modified: ");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "modified: ");
            ImguiUtils.WrappedText("Selects cells/fields where the equivalent cell in the vanilla regulation");
            ImguiUtils.WrappedText("or parambnd has a different value");
            ImguiUtils.WrappedText("\n");

            QuickAdd("auxmodified: ");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "auxmodified: <parambank>");
            ImguiUtils.WrappedText("Selects cells/fields where the equivalent cell in the specified regulation");
            ImguiUtils.WrappedText("or parambnd has a different value.");
            ImguiUtils.WrappedText("\n");

            QuickAdd("sftype: ");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "sftype: <type>");
            ImguiUtils.WrappedText("Selects cells/fields where the field's data type, as enumerated by ");
            ImguiUtils.WrappedText("soulsformats, matches the given regex.");
            ImguiUtils.WrappedText("\n");

            QuickAdd("vars: ");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "vars: <string>");
            ImguiUtils.WrappedText("Selects variables whose name matches the given regex.");
            ImguiUtils.WrappedText("\n");
        }

        if (currentMassEditTopic == 4)
        {
            QuickAdd("clear");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "clear");
            ImguiUtils.WrappedText("Clears clipboard param and rows.");
            ImguiUtils.WrappedText("\n");

            QuickAdd("newvar");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "newvar <name> <value>");
            ImguiUtils.WrappedText("Creates a variable with the given value, and the type of that value.");
            ImguiUtils.WrappedText("\n");

            QuickAdd("clearvars");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "clearvars");
            ImguiUtils.WrappedText("Deletes all variables.");
            ImguiUtils.WrappedText("\n");
        }

        if (currentMassEditTopic == 5)
        {
            QuickAdd("copy");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "copy");
            ImguiUtils.WrappedText("Adds the selected rows into clipboard.");
            ImguiUtils.WrappedText("If the clipboard param is different, the clipboard is emptied first");
            ImguiUtils.WrappedText("\n");

            QuickAdd("copyN");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "copyN <value>");
            ImguiUtils.WrappedText("Adds the selected rows into clipboard the given number of times.");
            ImguiUtils.WrappedText("If the clipboard param is different, the clipboard is emptied first");
            ImguiUtils.WrappedText("\n");

            QuickAdd("paste");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "paste");
            ImguiUtils.WrappedText("Adds the selected rows to the primary regulation or parambnd in");
            ImguiUtils.WrappedText("the selected param");
            ImguiUtils.WrappedText("\n");
        }

        if (currentMassEditTopic == 6)
        {
            QuickAdd("=");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "= <value>");
            ImguiUtils.WrappedText("Assigns the given value to the selected values.");
            ImguiUtils.WrappedText("Will attempt conversion to the value's data type");
            ImguiUtils.WrappedText("\n");

            QuickAdd("+");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "+ <value>");
            ImguiUtils.WrappedText("Adds the number to the selected values, or appends text");
            ImguiUtils.WrappedText("if that is the data type of the values.");
            ImguiUtils.WrappedText("\n");

            QuickAdd("-");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "- <value>");
            ImguiUtils.WrappedText("Subtracts the number from the selected values.");
            ImguiUtils.WrappedText("\n");

            QuickAdd("*");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "* <value>");
            ImguiUtils.WrappedText("Multiplies selected values by the number.");
            ImguiUtils.WrappedText("\n");

            QuickAdd("/");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "/ <value>");
            ImguiUtils.WrappedText("Divides the selected values by the number");
            ImguiUtils.WrappedText("\n");

            QuickAdd("%");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "%% <value>");
            ImguiUtils.WrappedText("Gives the remainder when the selected values are divided by the number.");
            ImguiUtils.WrappedText("\n");

            QuickAdd("scale");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "scale <factor> <center>");
            ImguiUtils.WrappedText("Multiplies the difference between the selected values and the center number by the factor number.");
            ImguiUtils.WrappedText("\n");

            QuickAdd("replace");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "replace <search string> <replace string>");
            ImguiUtils.WrappedText("Interprets the selected values as text and replaces all occurances");
            ImguiUtils.WrappedText("of the text to replace with the new text");
            ImguiUtils.WrappedText("\n");

            QuickAdd("replacex");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "replacex <search string> <replace string>");
            ImguiUtils.WrappedText("Interprets the selected values as text and replaces all occurances");
            ImguiUtils.WrappedText("of the given regex with the replacement, supporting regex groups");
            ImguiUtils.WrappedText("\n");

            QuickAdd("max");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "max <value>");
            ImguiUtils.WrappedText("Returns the larger of the current value and number.");
            ImguiUtils.WrappedText("\n");

            QuickAdd("min");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "min <value>");
            ImguiUtils.WrappedText("Returns the smaller of the current value and number.");
            ImguiUtils.WrappedText("\n");
        }

        if (currentMassEditTopic == 7)
        {
            QuickAdd("self");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "self");
            ImguiUtils.WrappedText("Gives the value of the currently selected value.");
            ImguiUtils.WrappedText("\n");

            QuickAdd("field");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "field <field>");
            ImguiUtils.WrappedText("Gives the value of the given cell/field for the currently selected row and param");
            ImguiUtils.WrappedText("\n");

            QuickAdd("vanilla");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "vanilla");
            ImguiUtils.WrappedText("Gives the value of the equivalent cell/field in the vanilla regulation or parambnd");
            ImguiUtils.WrappedText("for the currently selected cell/field, row and param.");
            ImguiUtils.WrappedText("Will fail if a row does not have a vanilla equivilent.");
            ImguiUtils.WrappedText("Consider using && !added");
            ImguiUtils.WrappedText("\n");

            QuickAdd("aux");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "aux <parambank>");
            ImguiUtils.WrappedText("Gives the value of the equivalent cell/field in the specified regulation or parambnd");
            ImguiUtils.WrappedText("for the currently selected cell/field, row and param.");
            ImguiUtils.WrappedText("Will fail if a row does not have a aux equivilent.");
            ImguiUtils.WrappedText("Consider using && !added");
            ImguiUtils.WrappedText("\n");

            QuickAdd("vanillafield");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "vanillafield <field>");
            ImguiUtils.WrappedText("Gives the value of the specified cell/field in the vanilla regulation or parambnd");
            ImguiUtils.WrappedText("for the currently selected cell/field, row and param.");
            ImguiUtils.WrappedText("Will fail if a row does not have a vanilla equivilent.");
            ImguiUtils.WrappedText("Consider using && !added");
            ImguiUtils.WrappedText("\n");

            QuickAdd("auxfield");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "auxfield <parambank> <field>");
            ImguiUtils.WrappedText("Gives the value of the specified cell/field in the aux regulation or parambnd");
            ImguiUtils.WrappedText("for the currently selected cell/field, row and param.");
            ImguiUtils.WrappedText("Will fail if a row does not have an aux equivilent.");
            ImguiUtils.WrappedText("Consider using && !added");
            ImguiUtils.WrappedText("\n");

            QuickAdd("paramlookup");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "paramlookup <param> <row> <field>");
            ImguiUtils.WrappedText("Returns the specific value specified by the exact param, row and field.");
            ImguiUtils.WrappedText("\n");

            QuickAdd("average");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "average <field> <row selector>");
            ImguiUtils.WrappedText("Gives the mean value of the cells/fields found using the given selector,");
            ImguiUtils.WrappedText("for the currently selected param");
            ImguiUtils.WrappedText("\n");

            QuickAdd("median");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "median <field> <row selector>");
            ImguiUtils.WrappedText("Gives the median value of the cells/fields found using the given selector,");
            ImguiUtils.WrappedText("for the currently selected param");
            ImguiUtils.WrappedText("\n");

            QuickAdd("mode");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "mode <field> <row selector>");
            ImguiUtils.WrappedText("Gives the mode value of the cells/fields found using the given selector,");
            ImguiUtils.WrappedText("for the currently selected param");
            ImguiUtils.WrappedText("\n");

            QuickAdd("min");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "min <field> <row selector>");
            ImguiUtils.WrappedText("Gives the smallest value of the cells/fields found using the given selector,");
            ImguiUtils.WrappedText("for the currently selected param");
            ImguiUtils.WrappedText("\n");

            QuickAdd("max");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "max <field> <row selector>");
            ImguiUtils.WrappedText("Gives the largest value of the cells/fields found using the given selector,");
            ImguiUtils.WrappedText("for the currently selected param");
            ImguiUtils.WrappedText("\n");

            QuickAdd("random");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "random <min> <max>");
            ImguiUtils.WrappedText("Gives a random decimal number between the given values for each selected value.");
            ImguiUtils.WrappedText("Minimum and maximum are inclusive.");
            ImguiUtils.WrappedText("\n");

            QuickAdd("randint");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "randint <min> <max>");
            ImguiUtils.WrappedText("Gives a random integer number between the given values for each selected value.");
            ImguiUtils.WrappedText("Minimum and maximum are inclusive.");
            ImguiUtils.WrappedText("\n");

            QuickAdd("randFrom");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "randFrom <param> <field> <row selector>");
            ImguiUtils.WrappedText("Gives a random value from the cells/fields found using the given param,");
            ImguiUtils.WrappedText("row selector and field, for each selected value.");
            ImguiUtils.WrappedText("\n");

            QuickAdd("paramIndex");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "paramIndex");
            ImguiUtils.WrappedText("Gives an integer for the current selected param,");
            ImguiUtils.WrappedText("beginning at 0 and increasing by 1 for each param selected.");
            ImguiUtils.WrappedText("\n");

            QuickAdd("rowIndex");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "rowIndex");
            ImguiUtils.WrappedText("Gives an integer for the current selected row,");
            ImguiUtils.WrappedText("beginning at 0 and increasing by 1 for each row selected.");
            ImguiUtils.WrappedText("\n");

            QuickAdd("fieldIndex");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "fieldIndex");
            ImguiUtils.WrappedText("Gives an integer for the current selected field,");
            ImguiUtils.WrappedText("beginning at 0 and increasing by 1 for each field selected.");
            ImguiUtils.WrappedText("\n");
        }

        ImGui.EndChild();

        ImGui.Columns(1);
    }

    private List<string> RegexTopics = new List<string>()
    {
        "Character Classes",
        "Anchors",
        "Escaped Characters",
        "Groups & Lookaround",
        "Quantifiers & Alternation"
    };

    private int currentRegexTopic = 0;

    private void DisplayRegexHelp()
    {
        ImGui.Columns(2);

        ImGui.BeginChild($"RegexSectionList");

        for (int i = 0; i < RegexTopics.Count; i++)
        {
            var topic = RegexTopics[i];

            if (ImGui.Selectable(topic))
            {
                currentRegexTopic = i;
            }
        }

        ImGui.EndChild();

        ImGui.NextColumn();

        ImGui.BeginChild($"RegexSectionView");

        if (currentRegexTopic == 0)
        {
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, ".       ");
            ImGui.SameLine();
            ImguiUtils.WrappedText("Any character except new line.");

            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "\\w      ");
            ImGui.SameLine();
            ImguiUtils.WrappedText("Any word.");

            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "\\d      ");
            ImGui.SameLine();
            ImguiUtils.WrappedText("Any digit.");

            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "\\s      ");
            ImGui.SameLine();
            ImguiUtils.WrappedText("Any whitespace.");

            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "[abc]   ");
            ImGui.SameLine();
            ImguiUtils.WrappedText("Any of the listed characters.");

            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "[^abc]  ");
            ImGui.SameLine();
            ImguiUtils.WrappedText("Not any of the listed characters.");

            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "[a-g]   ");
            ImGui.SameLine();
            ImguiUtils.WrappedText("Any characters between the two specified characters.");
        }

        if (currentRegexTopic == 1)
        {
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "^abc    ");
            ImGui.SameLine();
            ImguiUtils.WrappedText("Start of the string.");

            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "abc$    ");
            ImGui.SameLine();
            ImguiUtils.WrappedText("End of the string.");

            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "\\b      ");
            ImGui.SameLine();
            ImguiUtils.WrappedText("Word.");

            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "\\B      ");
            ImGui.SameLine();
            ImguiUtils.WrappedText("Not-word boundary.");
        }

        if (currentRegexTopic == 2)
        {
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "\\.      ");
            ImGui.SameLine();
            ImguiUtils.WrappedText("Use to match a .");

            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "\\*      ");
            ImGui.SameLine();
            ImguiUtils.WrappedText("Use to match a *");

            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "\\\\      ");
            ImGui.SameLine();
            ImguiUtils.WrappedText("Use to match a \\");
        }

        if (currentRegexTopic == 3)
        {
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "(abc)   ");
            ImGui.SameLine();
            ImguiUtils.WrappedText("Capture group.");

            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "\\1      ");
            ImGui.SameLine();
            ImguiUtils.WrappedText("Backreference to first capture group.");

            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "(?:abc) ");
            ImGui.SameLine();
            ImguiUtils.WrappedText("Non-capturing group.");

            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "(?=abc) ");
            ImGui.SameLine();
            ImguiUtils.WrappedText("Positive lookahead.");

            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "(?!abc) ");
            ImGui.SameLine();
            ImguiUtils.WrappedText("Negative lookahead.");
        }

        if (currentRegexTopic == 4)
        {
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "a*      ");
            ImGui.SameLine();
            ImguiUtils.WrappedText("Zero or more.");

            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "a+      ");
            ImGui.SameLine();
            ImguiUtils.WrappedText("One or more.");

            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "a?      ");
            ImGui.SameLine();
            ImguiUtils.WrappedText("One or zero.");

            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "a{5}    ");
            ImGui.SameLine();
            ImguiUtils.WrappedText("Exactly the specified number.");

            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "a{2,}   ");
            ImGui.SameLine();
            ImguiUtils.WrappedText("Two or more.");

            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "a{1,3}  ");
            ImGui.SameLine();
            ImguiUtils.WrappedText("Between one and three.");

            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "?       ");
            ImGui.SameLine();
            ImguiUtils.WrappedText("Match as few as possible.");

            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "ab|cd   ");
            ImGui.SameLine();
            ImguiUtils.WrappedText("Match either.");
        }

        ImGui.EndChild();

        ImGui.Columns(1);
    }

    private void DisplayLinks()
    {
        if (ImGui.BeginTabItem("Links"))
        {
            ImGui.Indent();

            ImGui.BeginChild($"LinksList");

            ImguiUtils.WrappedText("Below are a set of community links. Clicking them will take you to the associated URL.");

            foreach (LinkEntry entry in _helpBank.GetLinks())
            {
                if (ImGui.Button($"{entry.Title}"))
                {
                    Process.Start(new ProcessStartInfo { FileName = $"{entry.URL}", UseShellExecute = true });
                }
            }

            ImGui.EndChild();

            ImGui.Unindent();
            ImGui.EndTabItem();
        }
    }

    private void DisplayCredits()
    {
        if (ImGui.BeginTabItem("Credits"))
        {
            ImGui.Indent();

            ImGui.BeginChild($"CreditsList");

            foreach (var textSection in _helpBank.GetCredits().Text)
            {
                ImguiUtils.WrappedText(textSection);
            }

            ImGui.EndChild();

            ImGui.Unindent();
            ImGui.EndTabItem();
        }
    }
}
