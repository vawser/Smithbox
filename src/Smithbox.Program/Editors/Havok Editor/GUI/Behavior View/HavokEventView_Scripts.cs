using Hexa.NET.ImGui;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// The Events view's links to the action scripts (HavokScriptIndex): which events the shown file's paired scripts
/// name, where (call sites, opened in the Script window), and the W_ names they use that this file lacks.
/// A script not naming an event proves nothing: the game fires many itself (W_Damage, W_Death ...).
/// See Claude/Documentation/Behavior.md (Havok behavior views).
/// </summary>
public partial class HavokEventView
{
    // The paired scripts' string literals, rebuilt when a script finishes loading or another file is shown
    private HashSet<string> ScriptNames = new(StringComparer.Ordinal);
    private List<string> ScriptMissing = new();
    private int ScriptNamesVersion = -1;
    private string ScriptNamesFile;
    private HavokBehaviorSymbols ScriptNamesSymbols;
    private bool ScriptsReady;

    private const int MaxMentions = 60;

    /// <summary>
    /// Every string literal of the shown file's paired scripts (empty while they load)
    /// </summary>
    public HashSet<string> GetScriptNames()
    {
        var file = View.Selection.FilePath;
        var version = Owner.Scripts.Version;
        var symbols = Owner.Symbols;

        if (version == ScriptNamesVersion && file == ScriptNamesFile && ReferenceEquals(symbols, ScriptNamesSymbols) && ScriptsReady)
            return ScriptNames;

        var scripts = Owner.ScriptView.CurrentScripts();

        ScriptsReady = HavokScriptIndex.AllReady(scripts);
        ScriptNamesVersion = version;
        ScriptNamesFile = file;
        ScriptNamesSymbols = symbols;

        ScriptNames = scripts.Where(s => s.IsReady).SelectMany(s => s.LiteralNames).ToHashSet(StringComparer.Ordinal);

        // W_ names the scripts use that this file has no event for
        var events = symbols?.EventNames?.Cast<object>().Select(n => n as string ?? "").ToHashSet(StringComparer.Ordinal) ?? new();
        ScriptMissing = ScriptNames.Where(n => n.StartsWith("W_", StringComparison.Ordinal) && !events.Contains(n))
            .OrderBy(n => n, StringComparer.Ordinal).ToList();

        return ScriptNames;
    }

    /// <summary>
    /// The header line: the scripts being read, or the W_ names they use that this file lacks
    /// </summary>
    private void DisplayScriptStatus()
    {
        var scripts = Owner.ScriptView.CurrentScripts();

        if (scripts.Count == 0)
            return;

        GetScriptNames();

        if (!ScriptsReady)
        {
            ImGui.TextDisabled(LOC.Get("HAVOK_EventView_Scripts_Loading", string.Join(", ", scripts.Select(s => s.Name))));
            return;
        }

        if (ScriptMissing.Count == 0)
            return;

        ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("HAVOK_EventView_Scripts_Missing", ScriptMissing.Count));

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(LOC.Get("HAVOK_EventView_Scripts_Missing_TT") + "\n\n" +
                string.Join("\n", ScriptMissing.Take(40)) + (ScriptMissing.Count > 40 ? "\n..." : ""));
        }
    }

    /// <summary>
    /// The event's call sites in the paired scripts: each line that names it, with its function. Clicking one opens
    /// the Script window there.
    /// </summary>
    private void DisplayScriptMentions(string name)
    {
        var scripts = Owner.ScriptView.CurrentScripts();

        if (scripts.Count == 0)
            return;

        ImGui.Separator();

        if (!HavokScriptIndex.AllReady(scripts))
        {
            ImGui.TextDisabled(LOC.Get("HAVOK_EventView_Scripts_Loading", string.Join(", ", scripts.Select(s => s.Name))));
            return;
        }

        var mentions = HavokScriptIndex.FindMentions(scripts, name, MaxMentions + 1);

        ImGui.Text(LOC.Get("HAVOK_EventView_Script_Mentions", mentions.Count > MaxMentions ? $"{MaxMentions}+" : mentions.Count.ToString()));
        GUI.Tooltip(LOC.Get("HAVOK_EventView_Script_Mentions_TT", string.Join(", ", scripts.Select(s => s.Name))));

        if (mentions.Count == 0)
        {
            ImGui.TextDisabled(LOC.Get("HAVOK_EventView_Script_Not_Named"));
            return;
        }

        if (ImGui.BeginTable("eventScriptMentions", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable))
        {
            ImGui.TableSetupColumn(LOC.Get("HAVOK_EventView_Script_Column_Line"), ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn(LOC.Get("HAVOK_EventView_Script_Column_Function"), ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn(LOC.Get("HAVOK_EventView_Script_Column_Text"), ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableHeadersRow();

            for (var i = 0; i < Math.Min(mentions.Count, MaxMentions); i++)
            {
                var mention = mentions[i];

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                if (ImGui.Selectable($"{mention.Script.Name}:{mention.Line + 1}##mention{i}", false, ImGuiSelectableFlags.SpanAllColumns))
                    Owner.ScriptView.ShowLine(mention.Script, mention.Line);

                GUI.Tooltip(LOC.Get("HAVOK_EventView_Script_Mention_TT"));

                ImGui.TableSetColumnIndex(1);
                ImGui.TextDisabled(mention.Function ?? "-");

                ImGui.TableSetColumnIndex(2);
                ImGui.TextUnformatted(mention.Text);
            }

            ImGui.EndTable();
        }
    }

    /// <summary>
    /// The rename warning: the scripts that name the event, when any do
    /// </summary>
    private string RenameWarning(string name)
    {
        var named = Owner.ScriptView.CurrentScripts().Where(s => s.IsReady && s.Names(name)).Select(s => s.Name).ToList();

        return named.Count > 0
            ? LOC.Get("HAVOK_EventView_Rename_Script_TT", string.Join(", ", named))
            : LOC.Get("HAVOK_EventView_Rename_Warning_TT");
    }

    /// <summary>
    /// The scripts that name an event (for the Delete Event dialog), with how many lines in each
    /// </summary>
    public List<(string Script, int Lines)> ScriptsNaming(string name)
    {
        return Owner.ScriptView.CurrentScripts()
            .Where(s => s.IsReady && s.Names(name))
            .Select(s => (s.Name, s.LinesOf(name).Count))
            .ToList();
    }
}
