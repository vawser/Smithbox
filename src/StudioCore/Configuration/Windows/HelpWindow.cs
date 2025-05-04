using Hexa.NET.ImGui;
using StudioCore.Banks.HelpBank;
using StudioCore.Core;
using StudioCore.Help;
using StudioCore.Interface;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

namespace StudioCore.Configuration.Windows;

public class HelpWindow
{
    public Smithbox BaseEditor;

    private readonly HelpBank _helpBank;
    private bool MenuOpenState;

    // Articles
    private HelpEntry _selectedEntry_Article;
    private string _inputStr_Article = "";
    private string _inputStrCache_Article = "";

    // Tutorials
    private HelpEntry _selectedEntry_Tutorial;
    private string _inputStr_Tutorial = "";
    private string _inputStrCache_Tutorial = "";

    // Glossary
    private HelpEntry _selectedEntry_Glossary;
    private string _inputStr_Glossary = "";
    private string _inputStrCache_Glossary = "";

    // Mass Edit
    private HelpEntry _selectedEntry_MassEdit;
    private string _inputStr_MassEdit = "";
    private string _inputStrCache_MassEdit = "";

    // Regex
    private HelpEntry _selectedEntry_Regex;
    private string _inputStr_Regex = "";
    private string _inputStrCache_Regex = "";

    // Links
    private HelpEntry _selectedEntry_Link;
    private string _inputStr_Link = "";
    private string _inputStrCache_Link = "";

    // Credits
    private HelpEntry _selectedEntry_Credit;
    private string _inputStr_Credit = "";
    private string _inputStrCache_Credit = "";


    public HelpWindow(Smithbox baseEditor)
    {
        BaseEditor = baseEditor;
        _helpBank = new HelpBank();
    }

    public void ToggleMenuVisibility()
    {
        MenuOpenState = !MenuOpenState;
    }

    private SelectedHelpTab CurrentTab;

    public enum SelectedHelpTab
    {
        Articles = 0,
        Tutorials = 1,
        Glossary = 2,
        MassEdit = 3,
        Regex = 4,
        Links = 5,
        Credits = 6
    }

    public void ToggleWindow(SelectedHelpTab focusedTab, bool ignoreIfOpen = true)
    {
        CurrentTab = focusedTab;

        if (!ignoreIfOpen)
        {
            MenuOpenState = !MenuOpenState;
        }

        if (!MenuOpenState)
        {
            MenuOpenState = true;
        }
    }

    public void Display()
    {
        var scale = DPI.GetUIScale();

        if (!MenuOpenState)
            return;

        ImGui.SetNextWindowSize(new Vector2(600.0f, 600.0f) * scale, ImGuiCond.FirstUseEver);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, UI.Current.Imgui_Moveable_MainBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBg, UI.Current.Imgui_Moveable_TitleBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, UI.Current.Imgui_Moveable_TitleBg_Active);
        ImGui.PushStyleColor(ImGuiCol.ChildBg, UI.Current.Imgui_Moveable_ChildBg);
        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(10.0f, 10.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(5.0f, 5.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.IndentSpacing, 20.0f * scale);

        if (ImGui.Begin("Help##Popup", ref MenuOpenState, ImGuiWindowFlags.NoDocking))
        {
            if(CurrentTab is SelectedHelpTab.Articles)
            {
                DisplayHelpSection(_helpBank.GetArticles(), "Article", "Articles", "article", HelpSectionType.Article, _inputStr_Article, _inputStrCache_Article);
            }

            if (CurrentTab is SelectedHelpTab.Tutorials)
            {
                DisplayHelpSection(_helpBank.GetTutorials(), "Tutorial", "Tutorials", "tutorial", HelpSectionType.Tutorial, _inputStr_Tutorial, _inputStrCache_Tutorial);
            }

            if (CurrentTab is SelectedHelpTab.Glossary)
            {
                DisplayHelpSection(_helpBank.GetGlossaryEntries(), "Glossary", "Glossary", "glossary", HelpSectionType.Glossary, _inputStr_Glossary, _inputStrCache_Glossary);
            }

            if (CurrentTab is SelectedHelpTab.MassEdit)
            {
                DisplayHelpSection(_helpBank.GetMassEditHelp(), "Mass Edit", "Mass Edit", "mass edit", HelpSectionType.MassEdit, _inputStr_MassEdit, _inputStrCache_MassEdit);
            }

            if (CurrentTab is SelectedHelpTab.Regex)
            {
                DisplayHelpSection(_helpBank.GetRegexHelp(), "Regex", "Regexes", "regex", HelpSectionType.Regex, _inputStr_Regex, _inputStrCache_Regex);
            }

            if (CurrentTab is SelectedHelpTab.Links)
            {
                DisplayHelpSection(_helpBank.GetLinks(), "Link", "Links", "link", HelpSectionType.Link, _inputStr_Link, _inputStrCache_Link);
            }

            if (CurrentTab is SelectedHelpTab.Credits)
            {
                DisplayHelpSection(_helpBank.GetCredits(), "Credit", "Credits", "credit", HelpSectionType.Credit, _inputStr_Credit, _inputStrCache_Credit);
            }
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

    private enum HelpSectionType
    {
        Article,
        Tutorial,
        Glossary,
        Link,
        Credit,
        MassEdit,
        Regex
    }

    /// <summary>
    /// Articles
    /// </summary>
    private void DisplayHelpSection(List<HelpEntry> entries, string name, string tabTitle, string descName, HelpSectionType sectionType, string checkedInput, string checkedInputCache)
    {
        if (entries.Count < 1)
            return;

        ImGui.Columns(2);

        // Search Area
        ImGui.InputText("Search", ref checkedInput, 255);

        // Selection Area
        ImGui.BeginChild($"{name}SectionList");

        if (checkedInput.ToLower() != checkedInputCache.ToLower())
            checkedInputCache = checkedInput.ToLower();

        foreach (HelpEntry entry in entries)
        {
            var sectionName = entry.Title;

            ProjectType curProjectType = ProjectType.Undefined;

            if(BaseEditor.ProjectManager.SelectedProject != null)
            {
                curProjectType = BaseEditor.ProjectManager.SelectedProject.ProjectType;
            }

            if (entry.ProjectType == (int)curProjectType || entry.ProjectType == 0)
            {
                if (MatchEntry(entry, checkedInput))
                {
                    switch (sectionType)
                    {
                        case HelpSectionType.Article:
                            if (ImGui.Selectable(sectionName, _selectedEntry_Article == entry))
                            {
                                _selectedEntry_Article = entry;
                            }
                            break;
                        case HelpSectionType.Tutorial:
                            if (ImGui.Selectable(sectionName, _selectedEntry_Tutorial == entry))
                            {
                                _selectedEntry_Tutorial = entry;
                            }
                            break;
                        case HelpSectionType.Glossary:
                            if (ImGui.Selectable(sectionName, _selectedEntry_Glossary == entry))
                            {
                                _selectedEntry_Glossary = entry;
                            }
                            break;
                        case HelpSectionType.MassEdit:
                            if (ImGui.Selectable(sectionName, _selectedEntry_MassEdit == entry))
                            {
                                _selectedEntry_MassEdit = entry;
                            }
                            break;
                        case HelpSectionType.Regex:
                            if (ImGui.Selectable(sectionName, _selectedEntry_Regex == entry))
                            {
                                _selectedEntry_Regex = entry;
                            }
                            break;
                        case HelpSectionType.Link:
                            if (ImGui.Selectable(sectionName, _selectedEntry_Link == entry))
                            {
                                _selectedEntry_Link = entry;
                            }
                            break;
                        case HelpSectionType.Credit:
                            if (ImGui.Selectable(sectionName, _selectedEntry_Credit == entry))
                            {
                                _selectedEntry_Credit = entry;
                            }
                            break;
                    }
                }
            }
        }

        ImGui.EndChild();

        ImGui.NextColumn();

        ImGui.BeginChild($"{name}SectionView");

        foreach (HelpEntry entry in entries)
        {
            bool show = false;

            switch (sectionType)
            {
                case HelpSectionType.Article:
                    if (_selectedEntry_Article == entry)
                    {
                        show = true;
                    }
                    break;
                case HelpSectionType.Tutorial:
                    if (_selectedEntry_Tutorial == entry)
                    {
                        show = true;
                    }
                    break;
                case HelpSectionType.Glossary:
                    if (_selectedEntry_Glossary == entry)
                    {
                        show = true;
                    }
                    break;
                case HelpSectionType.MassEdit:
                    if (_selectedEntry_MassEdit == entry)
                    {
                        show = true;
                    }
                    break;
                case HelpSectionType.Regex:
                    if (_selectedEntry_Regex == entry)
                    {
                        show = true;
                    }
                    break;
                case HelpSectionType.Link:
                    if (_selectedEntry_Link == entry)
                    {
                        show = true;
                    }
                    break;
                case HelpSectionType.Credit:
                    if (_selectedEntry_Credit == entry)
                    {
                        show = true;
                    }
                    break;
            }

            if (show)
            {
                // No selection
                if (entry == null)
                {
                    UIHelper.WrappedText($"No {descName} selected");
                    ImGui.Separator();
                    UIHelper.WrappedText("");
                }
                // Selection
                else
                {
                    if (entry.Author != null)
                    {
                        ProcessText(entry, entry.Author);
                    }

                    foreach (var textSection in entry.Contents)
                    {
                        ProcessText(entry, textSection);
                    }
                }
            }
        }

        ImGui.EndChild();

        ImGui.Columns(1);
    }

    private void ProcessText(HelpEntry entry, string textLine)
    {
        var outputLine = textLine;

        // Header
        if (textLine.Contains("[Header]"))
        {
            outputLine = textLine.Replace("[Header]", "");
            ImGui.Separator();

            if (entry.HeaderColor != null)
            {
                Vector4 color = new Vector4(entry.HeaderColor[0], entry.HeaderColor[1], entry.HeaderColor[2], entry.HeaderColor[3]);
                UIHelper.WrappedTextColored(color, outputLine);
            }
            else
            {
                UIHelper.WrappedText(outputLine);
            }

            ImGui.Separator();
        }
        // Highlight
        else if (textLine.Contains("[Highlight@"))
        {
            var pattern = $@"\[Highlight\@(.*)\](.*)";
            var match = Regex.Match(textLine, pattern);

            if (match.Success && match.Groups.Count >= 2)
            {
                string highlightText = match.Groups[1].Value;
                string otherText = match.Groups[2].Value;

                if (entry.HighlightColor != null)
                {
                    Vector4 color = new Vector4(entry.HighlightColor[0], entry.HighlightColor[1], entry.HighlightColor[2], entry.HighlightColor[3]);
                    UIHelper.WrappedTextColored(color, highlightText);

                    var offset = highlightText.Length * 8.0f;
                    ImGui.SameLine(offset);
                    UIHelper.WrappedText(otherText);
                }
                else
                {
                    UIHelper.WrappedText(highlightText);
                    ImGui.SameLine();
                    UIHelper.WrappedText(otherText);
                }
            }
        }
        // Link
        else if (textLine.Contains("[Link@"))
        {
            var pattern = $@"\[Link\@(.*)@(.*)\]";
            var match = Regex.Match(textLine, pattern);

            if (match.Success && match.Groups.Count >= 3)
            {
                string url = match.Groups[1].Value;
                string linkName = match.Groups[2].Value;

                var width = ImGui.GetWindowWidth();
                if (ImGui.Button($"{linkName}", new Vector2(width, 32)))
                {
                    Process.Start(new ProcessStartInfo { FileName = $"{url}", UseShellExecute = true });
                }
            }
        }
        // Default
        else
        {
            UIHelper.WrappedText(textLine);
        }
    }
}
