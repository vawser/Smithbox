using ImGuiNET;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace StudioCore.Help;

public class HelpBrowser
{
    private readonly HelpDex _helpDex;

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
    private AssetLocator _locator;
    private HelpEntry _selectedEntry_Article;
    private HelpEntry _selectedEntry_Glossary;
    private HelpEntry _selectedEntry_Tutorial;

    private bool MenuOpenState;

    public HelpBrowser(string id, AssetLocator locator)
    {
        _id = id;
        _locator = locator;

        _helpDex = new HelpDex();
    }

    public void ToggleMenuVisibility()
    {
        MenuOpenState = !MenuOpenState;
    }

    public void Display()
    {
        var scale = MapStudioNew.GetUIScale();

        if (!MenuOpenState)
        {
            return;
        }

        ImGui.SetNextWindowSize(new Vector2(600.0f, 600.0f) * scale, ImGuiCond.FirstUseEver);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0f, 0f, 0f, 0.98f));
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, new Vector4(0.25f, 0.25f, 0.25f, 1.0f));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(10.0f, 10.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(20.0f, 10.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.IndentSpacing, 20.0f * scale);

        if (ImGui.Begin("Help##Popup", ref MenuOpenState, ImGuiWindowFlags.NoDocking))
        {
            ImGui.BeginTabBar("#HelpMenuTabBar");
            ImGui.PushStyleColor(ImGuiCol.Header, new Vector4(0.3f, 0.3f, 0.6f, 0.4f));
            ImGui.PushItemWidth(300f);

            DisplayHelpSection("Article", _helpDex.GetArticles(), _inputStr_Article, _inputStrCache_Article,
                "Articles", "No title.", "No article selected.");
            DisplayHelpSection("Tutorial", _helpDex.GetTutorials(), _inputStr_Tutorial, _inputStrCache_Tutorial,
                "Tutorials", "No title.", "No tutorial selected.");
            DisplayHelpSection("Glossary", _helpDex.GetGlossaryEntries(), _inputStr_Glossary,
                _inputStrCache_Glossary, "Glossary", "No title.", "No term selected.");
            DisplayLinks();
            DisplayCredits();

            ImGui.PopItemWidth();
            ImGui.PopStyleColor();
            ImGui.EndTabBar();
        }

        ImGui.End();

        ImGui.PopStyleVar(3);
        ImGui.PopStyleColor(2);
    }

    private void DisplayHelpSection(string sectionType, List<HelpEntry> entries, string inputStr,
        string inputStrCache, string title, string noSelection_Title, string noSelection_Body)
    {
        if (entries.Count < 1)
        {
            return;
        }

        if (ImGui.BeginTabItem(title))
        {
            // Search Area
            ImGui.InputText("Search", ref inputStr, 255);

            // Selection Area
            ImGui.BeginChild($"{title}SectionList", new Vector2(600, 100), true, ImGuiWindowFlags.NoScrollbar);

            if (inputStr.ToLower() != inputStrCache.ToLower())
            {
                inputStrCache = inputStr.ToLower();
            }

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
                    List<string> segmentParts = segment.Split(" ").ToList();
                    foreach (var part in segmentParts)
                    {
                        sectionNameSegments.Add(part.ToLower());
                    }
                }

                // Content Segments
                List<string> coreSegments = new();

                foreach (var segment in contents)
                {
                    List<string> segmentParts = segment.Split(" ").ToList();
                    foreach (var part in segmentParts)
                    {
                        coreSegments.Add(part.ToLower());
                    }
                }

                // Tags
                List<string> tagList = new();

                foreach (var segment in tags)
                {
                    tagList.Add(segment.ToLower());
                }

                // Only show if input matches any title or content segments, or if it is blank
                if (lowercaseInput.ToLower() == "" || sectionNameSegments.Contains(lowercaseInput) ||
                    coreSegments.Contains(lowercaseInput) || tagList.Contains(lowercaseInput))
                {
                    if (ImGui.Selectable(sectionName))
                    {
                    }

                    if (ImGui.IsItemClicked() && ImGui.IsMouseDoubleClicked(0))
                    {
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
            {
                ImGui.Text(textSection);
            }
        }
    }

    private void DisplayLinks()
    {
        if (ImGui.BeginTabItem("Links"))
        {
            ImGui.Indent();

            ImGui.Text("Below are a set of community links. Clicking them will take you to the associated URL.");

            foreach (LinkEntry entry in _helpDex.GetLinks())
            {
                if (ImGui.Button($"{entry.Title}"))
                {
                    Process.Start(new ProcessStartInfo { FileName = $"{entry.URL}", UseShellExecute = true });
                }
            }

            ImGui.Unindent();
            ImGui.EndTabItem();
        }
    }

    private void DisplayCredits()
    {
        if (ImGui.BeginTabItem("Credits"))
        {
            ImGui.Indent();

            ImGui.Text(GetDisplayText(_helpDex.GetCredits().Text));

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
            {
                // If the current str already includes a separator, reset the count
                // As GetDisplayTextSections will handle the separator instead
                if (str.Contains(textSectionSplitter))
                {
                    charCount = 0;
                }
            }
        }

        return displayText;
    }

    private List<string> GetDisplayTextSections(List<string> stringList)
    {
        var displayTextFull = GetDisplayText(stringList);

        List<string> displayTextSegments = displayTextFull.Split(textSectionSplitter).ToList();

        return displayTextSegments;
    }
}
