using Hexa.NET.ImGui;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// The Script window: the shown behavior file's paired action scripts (HavokScriptIndex), read only, with line
/// numbers, a function list, find, and Back. Call sites and callbacks elsewhere jump here (ShowLine).
/// See Claude/Documentation/Havok Behavior HKS Cross-Reference Plan.md.
/// </summary>
public class HavokScriptView
{
    private readonly HavokEditorView View;
    private readonly HavokBehaviorView Owner;

    private HavokScript Script;
    private int HighlightLine = -1;
    private bool ScrollToHighlight;

    private readonly Stack<(HavokScript Script, int Line)> History = new();

    private string Find = "";
    private string FindFor = null;
    private HavokScript FindIn;
    private List<int> FindMatches = new();
    private int FindIndex = -1;

    private string FunctionFilter = "";

    /// <summary>Set by a jump, so the window is brought to the front</summary>
    public bool FocusRequested;

    public HavokScriptView(HavokEditorView view, HavokBehaviorView owner)
    {
        View = view;
        Owner = owner;
    }

    /// <summary>The scripts paired with the shown behavior file</summary>
    public List<HavokScript> CurrentScripts()
        => Owner.Scripts.ScriptsFor(View.Selection.BinderFileEntry, View.Selection.FilePath);

    /// <summary>
    /// Shows a script at a line (0-based), remembering where it was for Back
    /// </summary>
    public void ShowLine(HavokScript script, int line)
    {
        if (script == null)
            return;

        if (Script != null)
            History.Push((Script, HighlightLine));

        Script = script;
        HighlightLine = line;
        ScrollToHighlight = true;
        FocusRequested = true;
    }

    public void DisplayWindow()
    {
        var scripts = CurrentScripts();

        if (scripts.Count == 0)
        {
            GUI.WrappedText(LOC.Get("HAVOK_Script_None"));
            return;
        }

        // Another behavior file shown: its first script
        if (Script == null || !scripts.Any(s => ReferenceEquals(s, Script)))
        {
            Script = scripts[0];
            History.Clear();
            HighlightLine = -1;
        }

        if (!DisplayHeader(scripts))
            return;

        if (!Script.IsReady)
        {
            ImGui.TextDisabled(LOC.Get("HAVOK_Script_Loading", Script.Name));
            return;
        }

        if (Script.Error != null)
        {
            ImGui.TextColored(UI.Current.ImGui_Invalid_Text_Color, LOC.Get("HAVOK_Script_Failed", Script.Name, Script.Error));
            return;
        }

        DisplayLines();
    }

    /// <summary>False when Reload dropped the scripts this frame</summary>
    private bool DisplayHeader(List<HavokScript> scripts)
    {
        ImGui.BeginChild("havokScriptHeader", EditorFilters.GetHeaderSize() * new System.Numerics.Vector2(1, 2), ImGuiChildFlags.Borders);

        // Back
        ImGui.BeginDisabled(History.Count == 0);
        if (ImGui.ArrowButton("##scriptBack", ImGuiDir.Left))
        {
            var (script, line) = History.Pop();
            Script = script;
            HighlightLine = line;
            ScrollToHighlight = true;
        }
        ImGui.EndDisabled();
        GUI.Tooltip(LOC.Get("HAVOK_Script_Back_TT"));

        // Script
        ImGui.SameLine();
        ImGui.SetNextItemWidth(200f * DPI.UIScale());

        if (ImGui.BeginCombo("##scriptPick", Script.Name))
        {
            foreach (var script in scripts)
            {
                if (ImGui.Selectable($"{script.Name}##pick_{script.Path}", ReferenceEquals(script, Script)))
                {
                    if (!ReferenceEquals(script, Script))
                        ShowLine(script, 0);
                }
            }

            ImGui.EndCombo();
        }

        GUI.Tooltip(LOC.Get("HAVOK_Script_Pick_TT"));

        ImGui.SameLine();
        ImGui.TextDisabled(LOC.Get(Script.FromProject ? "HAVOK_Script_From_Project" : "HAVOK_Script_From_Game") +
            (Script.IsText ? "" : "  " + LOC.Get("HAVOK_Script_Decompiled")));
        GUI.Tooltip(LOC.Get("HAVOK_Script_Source_TT"));

        ImGui.SameLine();
        if (ImGui.SmallButton($"{LOC.Get("HAVOK_Script_Reload")}##scriptReload"))
        {
            Owner.Scripts.Reload();
            Script = null;
            History.Clear();
            FindFor = null;
            ImGui.EndChild();
            return false;
        }
        GUI.Tooltip(LOC.Get("HAVOK_Script_Reload_TT"));

        if (Script.IsReady && Script.Error == null)
        {
            // Functions
            ImGui.SetNextItemWidth(200f * DPI.UIScale());

            if (ImGui.BeginCombo("##scriptFunctions", LOC.Get("HAVOK_Script_Functions", Script.Functions.Count), ImGuiComboFlags.HeightLarge))
            {
                if (ImGui.IsWindowAppearing())
                {
                    FunctionFilter = "";
                    ImGui.SetKeyboardFocusHere();
                }

                ImGui.SetNextItemWidth(-1);
                ImGui.InputTextWithHint("##scriptFunctionFilter", LOC.Get("HAVOK_BehaviorIndex_Search_Hint"), ref FunctionFilter, 255);

                var shown = Script.Functions
                    .Where(f => FunctionFilter.Length == 0 || f.Name.Contains(FunctionFilter, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var clipper = new ImGuiListClipper();
                clipper.Begin(shown.Count);

                while (clipper.Step())
                {
                    for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                    {
                        if (ImGui.Selectable($"{shown[i].Name}##fn{i}"))
                            ShowLine(Script, shown[i].Line);
                    }
                }

                clipper.End();
                ImGui.EndCombo();
            }

            GUI.Tooltip(LOC.Get("HAVOK_Script_Functions_TT"));

            // Find: the matches follow the text; the arrows step through them
            ImGui.SameLine();
            ImGui.SetNextItemWidth(220f * DPI.UIScale());
            ImGui.InputTextWithHint("##scriptFind", LOC.Get("HAVOK_Script_Find_Hint"), ref Find, 255);

            UpdateFind();

            ImGui.SameLine();
            ImGui.BeginDisabled(FindMatches.Count == 0);
            if (ImGui.ArrowButton("##scriptFindPrev", ImGuiDir.Up))
                StepFind(-1);
            ImGui.SameLine();
            if (ImGui.ArrowButton("##scriptFindNext", ImGuiDir.Down))
                StepFind(1);
            ImGui.EndDisabled();

            ImGui.SameLine();
            ImGui.AlignTextToFramePadding();

            if (Find.Length > 0)
                ImGui.TextDisabled(LOC.Get("HAVOK_Script_Find_Count", FindIndex + 1, FindMatches.Count));
        }

        ImGui.EndChild();
        return true;
    }

    private void UpdateFind()
    {
        if (Find == FindFor && ReferenceEquals(FindIn, Script))
            return;

        FindFor = Find;
        FindIn = Script;
        FindMatches = new List<int>();
        FindIndex = -1;

        if (Find.Length == 0)
            return;

        for (var i = 0; i < Script.Lines.Length; i++)
        {
            if (Script.Lines[i].Contains(Find, StringComparison.OrdinalIgnoreCase))
                FindMatches.Add(i);
        }
    }

    private void StepFind(int step)
    {
        if (FindMatches.Count == 0)
            return;

        FindIndex = ((FindIndex + step) % FindMatches.Count + FindMatches.Count) % FindMatches.Count;
        HighlightLine = FindMatches[FindIndex];
        ScrollToHighlight = true;
    }

    private void DisplayLines()
    {
        ImGui.BeginChild("havokScriptLines", new System.Numerics.Vector2(0, 0), ImGuiChildFlags.None, ImGuiWindowFlags.HorizontalScrollbar);

        var lines = Script.Lines;
        var lineHeight = ImGui.GetTextLineHeightWithSpacing();

        if (ScrollToHighlight && HighlightLine >= 0)
        {
            ImGui.SetScrollY(Math.Max(0, HighlightLine * lineHeight - ImGui.GetWindowHeight() * 0.4f));
            ScrollToHighlight = false;
        }

        var digits = lines.Length.ToString().Length;
        var clipper = new ImGuiListClipper();
        clipper.Begin(lines.Length, lineHeight);

        while (clipper.Step())
        {
            for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                if (ImGui.Selectable($"##scriptLine{i}", i == HighlightLine, ImGuiSelectableFlags.AllowOverlap))
                    HighlightLine = i;

                ImGui.SameLine(0, 0);
                ImGui.TextDisabled((i + 1).ToString().PadLeft(digits));
                ImGui.SameLine();
                ImGui.TextUnformatted(lines[i]);
            }
        }

        clipper.End();

        ImGui.EndChild();
    }
}
