using Hexa.NET.ImGui;
using System.Numerics;

namespace StudioCore.Editors.LuaEditor;

/// <summary>
/// The selected script. Reading: coloured text with line numbers, the syntax problem (if any) on top, Ctrl+click a
/// name to go to its definition. Editing: a text box over the whole script, applied when it is left (a click
/// elsewhere, a save, selecting another script; Tab types a tab, and Escape undoes the typing since the box was
/// entered). Each applied edit is one undo step.
/// </summary>
public class LuaScriptView
{
    private readonly LuaEditorScreen Editor;

    public bool Editing { get; private set; }

    // The script the edit box's text belongs to (it can differ from the selection for the frame a click elsewhere
    // selects another script: the box applies to its own script first)
    private LuaScript EditScript;
    private string EditBuffer = "";
    private int EditRevision = -1;

    private int? PendingJump;
    private int CurrentLine = -1;

    // Display cache: the lines of the shown revision, each split into coloured spans
    private LuaScript CachedScript;
    private int CachedRevision = -1;
    private List<List<Span>> Lines = new();

    private readonly record struct Span(string Text, LuaTokenKind Kind, bool IsMethod, bool IsConstant);

    public LuaScriptView(LuaEditorScreen editor)
    {
        Editor = editor;
    }

    /// <summary>The edit box holds text not yet applied to its script</summary>
    public bool HasPendingEdit => Editing && EditScript != null && EditScript.Text != null && EditBuffer != EditScript.Text;

    /// <summary>Applies the edit box's text to its script (before a save, undo, or another script is selected)</summary>
    public void CommitPending()
    {
        if (HasPendingEdit)
        {
            EditScript.Commit(EditBuffer);
            EditRevision = EditScript.Revision;
        }
    }

    public void SetEditing(bool editing)
    {
        if (!editing)
            CommitPending();
        Editing = editing;
        EditScript = null;
    }

    /// <summary>Shows a line (1-based); leaves editing, since the edit box can't scroll to a line</summary>
    public void JumpTo(int line)
    {
        if (Editing)
            SetEditing(false);
        PendingJump = line;
        CurrentLine = line;
    }

    public void Draw()
    {
        var script = Editor.SelectedScript;
        if (script == null)
        {
            ImGui.TextDisabled(LOC.Get("LUA_Script_None_Selected"));
            return;
        }

        var text = script.Text;
        if (text == null)
        {
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, script.LoadError ?? "");
            return;
        }

        DrawHeader(script);

        if (Editing)
            DrawEditor(script);
        else
            DrawReader(script);
    }

    private void DrawHeader(LuaScript script)
    {
        ImGui.Text($"{script.Container.Name} / {script.EntryName}");
        ImGui.SameLine();
        ImGui.TextDisabled(script.IsBytecode ? LOC.Get("LUA_Script_State_Bytecode") : LOC.Get("LUA_Script_State_Text"));
        GUI.Tooltip(script.IsBytecode ? LOC.Get("LUA_Script_State_Bytecode_TT") : LOC.Get("LUA_Script_State_Text_TT"));
        if (script.IsDirty)
        {
            ImGui.SameLine();
            ImGui.TextColored(UI.Current.ImGui_ModifiedFilter_Text, LOC.Get("LUA_Script_State_Unsaved"));
        }

        ImGui.SameLine();
        var label = Editing ? LOC.Get("LUA_Script_Button_Read") : LOC.Get("LUA_Script_Button_Edit");
        if (ImGui.SmallButton($"{label}##luaEditToggleButton"))
            SetEditing(!Editing);
        GUI.Tooltip(LOC.Get("LUA_Edit_Toggle_Editing_TT"));

        // The syntax problem Lua would report (checked on the applied text; it is also why a save is refused)
        var parse = script.Parse;
        if (parse != null && !parse.IsValid)
        {
            var p = parse.Problems[0];
            ImGui.TextColored(UI.Current.ImGui_Invalid_Text_Color, LOC.Get("LUA_Data_Problem_At", p.Line, p.Column, p.Message));
            if (ImGui.IsItemClicked())
                JumpTo(p.Line);
            GUI.Tooltip(LOC.Get("LUA_Script_Problem_TT"));
        }
        else if (LuaLexer.FindUnencodable(script.Text) is { } bad)
        {
            ImGui.TextColored(UI.Current.ImGui_Invalid_Text_Color, LOC.Get("LUA_Data_Unencodable", bad.Line, bad.Column, bad.Char));
            if (ImGui.IsItemClicked())
                JumpTo(bad.Line);
        }

        if (Editor.Definitions.Status is { } status)
            ImGui.TextDisabled(status);

        ImGui.Separator();
    }

    private void DrawEditor(LuaScript selected)
    {
        if (EditScript == null)
        {
            EditScript = selected;
            EditBuffer = selected.Text;
            EditRevision = selected.Revision;
        }

        var size = new Vector2(-1, -1);
        var bufferSize = (nuint)(System.Text.Encoding.UTF8.GetByteCount(EditBuffer) + 65536);
        ImGui.InputTextMultiline("##luaScriptEdit", ref EditBuffer, bufferSize, size, ImGuiInputTextFlags.AllowTabInput);

        if (GUI.InputCommitted())
        {
            EditScript.Commit(EditBuffer);
            EditRevision = EditScript.Revision;
        }

        if (!ImGui.IsItemActive())
        {
            // Another script was selected: the box follows it
            if (EditScript != selected)
            {
                CommitPending();
                EditScript = selected;
                EditBuffer = selected.Text;
                EditRevision = selected.Revision;
            }
            // The text changed outside the box (undo, redo)
            else if (EditRevision != selected.Revision)
            {
                EditBuffer = selected.Text;
                EditRevision = selected.Revision;
            }
        }
    }

    private void DrawReader(LuaScript script)
    {
        if (CachedScript != script || CachedRevision != script.Revision)
            Rebuild(script);

        ImGui.BeginChild("luaScriptRead", Vector2.Zero, ImGuiChildFlags.None, ImGuiWindowFlags.HorizontalScrollbar);

        var lineHeight = ImGui.GetTextLineHeightWithSpacing();
        if (PendingJump is { } jump)
        {
            ImGui.SetScrollY(Math.Max(0, (jump - 1) * lineHeight - ImGui.GetWindowHeight() / 3));
            PendingJump = null;
        }

        var numberWidth = ImGui.CalcTextSize(Lines.Count.ToString()).X + 12 * DPI.UIScale();
        var drawList = ImGui.GetWindowDrawList();
        var ctrl = ImGui.GetIO().KeyCtrl;

        var clipper = new ImGuiListClipper();
        clipper.Begin(Lines.Count, lineHeight);
        while (clipper.Step())
        {
            for (int l = clipper.DisplayStart; l < clipper.DisplayEnd; l++)
            {
                var lineNumber = l + 1;
                var start = ImGui.GetCursorScreenPos();

                if (lineNumber == CurrentLine)
                {
                    var width = Math.Max(ImGui.GetWindowWidth(), ImGui.GetContentRegionAvail().X);
                    drawList.AddRectFilled(start, start + new Vector2(width + ImGui.GetScrollX(), lineHeight),
                        ImGui.GetColorU32(ImGuiCol.FrameBg));
                }

                if (CFG.Current.LuaEditor_Show_Line_Numbers)
                {
                    ImGui.TextDisabled(lineNumber.ToString());
                    if (ImGui.IsItemClicked())
                        CurrentLine = lineNumber;
                    ImGui.SameLine(numberWidth);
                }

                var spans = Lines[l];
                if (spans.Count == 0)
                {
                    ImGui.TextUnformatted("");
                    continue;
                }

                for (int s = 0; s < spans.Count; s++)
                {
                    if (s > 0)
                        ImGui.SameLine(0, 0);
                    var span = spans[s];
                    ImGui.TextColored(ColorOf(span), span.Text);

                    if (span.Kind == LuaTokenKind.Name && ImGui.IsItemHovered())
                    {
                        if (ctrl)
                        {
                            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                            drawList.AddLine(ImGui.GetItemRectMin() with { Y = ImGui.GetItemRectMax().Y },
                                ImGui.GetItemRectMax(), ImGui.GetColorU32(ColorOf(span)));
                        }
                        if (ImGui.IsItemClicked() && ctrl)
                            Editor.Definitions.GoTo(script, span.Text);
                    }

                    // A name, number or string can be searched for in every script
                    if (span.Kind is LuaTokenKind.Name or LuaTokenKind.Number or LuaTokenKind.String &&
                        ImGui.IsItemClicked(ImGuiMouseButton.Right))
                    {
                        SearchToken = span.Kind == LuaTokenKind.String ? span.Text.Trim('"', '\'') : span.Text;
                        ImGui.OpenPopup("luaScriptTokenMenu");
                    }
                }

                if (ImGui.IsMouseClicked(ImGuiMouseButton.Left) && !ctrl && ImGui.IsWindowHovered() &&
                    ImGui.GetMousePos().Y >= start.Y && ImGui.GetMousePos().Y < start.Y + lineHeight)
                    CurrentLine = lineNumber;
            }
        }
        clipper.End();

        if (ImGui.BeginPopup("luaScriptTokenMenu"))
        {
            if (ImGui.Selectable($"{LOC.Get("LUA_Script_Search_All", SearchToken)}##luaScriptSearchAll"))
            {
                CFG.Current.Interface_LuaEditor_ToolWindow = true;
                Editor.ToolWindow.GlobalSearch.SearchFor(SearchToken, true);
            }
            GUI.Tooltip(LOC.Get("LUA_Script_Search_All_TT"));
            ImGui.EndPopup();
        }

        ImGui.EndChild();
    }

    // The token a right-click opened the menu on
    private string SearchToken = "";

    private void Rebuild(LuaScript script)
    {
        CachedScript = script;
        CachedRevision = script.Revision;
        Lines = new();

        var text = script.Text.Replace("\t", "    ");
        var tokens = LuaLexer.Tokenize(text, null, keepComments: true);

        var line = new List<Span>();
        var at = 0;

        void Emit(int end, LuaTokenKind kind, bool isMethod, bool isConstant)
        {
            // Split a piece of text at line breaks into spans of the current and following lines
            while (at < end)
            {
                var nl = text.IndexOf('\n', at, end - at);
                var stop = nl < 0 ? end : nl;
                if (stop > at)
                    line.Add(new Span(text[at..stop].TrimEnd('\r'), kind, isMethod, isConstant));
                if (nl < 0)
                {
                    at = end;
                    break;
                }
                Lines.Add(line);
                line = new List<Span>();
                at = nl + 1;
            }
        }

        for (int i = 0; i < tokens.Count; i++)
        {
            var t = tokens[i];
            if (t.Kind == LuaTokenKind.EndOfFile)
                break;
            Emit(t.Start, LuaTokenKind.Symbol, false, false);
            var isMethod = t.Kind == LuaTokenKind.Name && i > 0 && tokens[i - 1].Is(":");
            var isConstant = t.Kind == LuaTokenKind.Name && IsConstantName(t.Text);
            Emit(t.Start + t.Length, t.Kind, isMethod, isConstant);
        }
        Emit(text.Length, LuaTokenKind.Symbol, false, false);
        Lines.Add(line);
    }

    // As the situation reads them: ROLE_TYPE_Torimaki and INTERUPT_FindAttack are constants too, not only GOAL_…
    private static bool IsConstantName(string name) => LuaSituation.LooksLikeConstant(name);

    // Syntax colours (on the dark theme)
    private static readonly Vector4 KeywordColor = new(0.55f, 0.7f, 1.0f, 1.0f);
    private static readonly Vector4 StringColor = new(0.9f, 0.65f, 0.45f, 1.0f);
    private static readonly Vector4 NumberColor = new(0.65f, 0.9f, 0.6f, 1.0f);
    private static readonly Vector4 CommentColor = new(0.5f, 0.6f, 0.5f, 1.0f);
    private static readonly Vector4 MethodColor = new(0.9f, 0.85f, 0.6f, 1.0f);
    private static readonly Vector4 ConstantColor = new(0.8f, 0.6f, 0.9f, 1.0f);
    private static readonly Vector4 SymbolColor = new(0.75f, 0.75f, 0.75f, 1.0f);

    private static Vector4 ColorOf(Span s) => s.Kind switch
    {
        LuaTokenKind.Keyword => KeywordColor,
        LuaTokenKind.String => StringColor,
        LuaTokenKind.Number => NumberColor,
        LuaTokenKind.Comment => CommentColor,
        LuaTokenKind.Error => UI.Current.ImGui_Invalid_Text_Color,
        LuaTokenKind.Name when s.IsMethod => MethodColor,
        LuaTokenKind.Name when s.IsConstant => ConstantColor,
        LuaTokenKind.Name => UI.Current.ImGui_Default_Text_Color,
        _ => SymbolColor,
    };
}
