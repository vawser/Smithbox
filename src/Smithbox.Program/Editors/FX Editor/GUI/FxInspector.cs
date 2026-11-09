using Hexa.NET.ImGui;
using SoulsFormats;
using System.Globalization;
using System.Numerics;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// The selected tree item. For an action: its named fields, properties and section10 lists through the
/// schema, in fxr-main's display units (seconds, degrees, ...), with a raw view of the stored words.
/// Fields and constant properties can be edited here; a property's name (or a keyframed property's summary) opens
/// it in the Curve Editor, where its keys, function and modifiers are edited.
/// </summary>
public class FxInspector
{
    private readonly FxEditorScreen Editor;

    // Text inputs keep their value here while being typed in, and commit once when they lose focus,
    // so one edit is one undo step
    private readonly Dictionary<string, object> Pending = new();

    public FxInspector(FxEditorScreen editor)
    {
        Editor = editor;
    }

    private FxData Data => Editor.Data;

    public void Display()
    {
        GUI.TitleHeader(LOC.Get("FX_Inspector_Header"), LOC.Get("FX_Inspector_Header_TT"));

        ImGui.BeginChild("FxInspectorSection", ImGuiChildFlags.Borders);

        var entry = Editor.Selection.Entry;
        var item = Editor.Selection.Item;

        switch (item)
        {
            case FxrAction action when entry != null:
                DisplayAction(entry, action);
                break;
            case FxrState state:
                DisplayState(state);
                break;
            case FxrNode node:
                DisplayNode(node);
                break;
            case FxrConfig config:
                GUI.WrappedText(LOC.Get("FX_Inspector_Config", FxNames.ConfigType(config.Type), config.Actions.Count));
                break;
            default:
                GUI.WrappedText(LOC.Get("FX_Inspector_No_Selection"));
                break;
        }

        ImGui.EndChild();
    }

    private void DisplayState(FxrState state)
    {
        // Conditions
        GUI.SimpleHeader(LOC.Get("FX_Inspector_Conditions_Header"), LOC.Get("FX_Inspector_Conditions_Header_TT"));
        if (state.Conditions.Count == 0)
            GUI.WrappedText(LOC.Get("FX_Inspector_No_Conditions"));
        foreach (var c in state.Conditions)
            GUI.WrappedText(FxNames.Condition(c));
    }

    private void DisplayNode(FxrNode node)
    {
        GUI.WrappedText(LOC.Get("FX_Inspector_Node", FxNames.NodeType(node.Type), node.Configs.Count, node.Actions.Count, node.Children.Count));
    }

    // ---------------------------------------------------------------- actions

    private void DisplayAction(FxEntry entry, FxrAction action)
    {
        var view = Data.View(entry, action);
        var def = view.Definition;

        // Action
        GUI.SimpleHeader(LOC.Get("FX_Inspector_Action_Header"), LOC.Get("FX_Inspector_Action_Header_TT"));
        GUI.WrappedText(def != null ? $"{def.Name} ({action.Type})" : LOC.Get("FX_Action_Unknown", action.Type));
        if (def?.Slot != null)
            GUI.WrappedText(LOC.Get("FX_Inspector_Slot", def.Slot));
        if (def?.Description != null && CFG.Current.FxEditor_Inspector_Show_Descriptions)
            GUI.WrappedTextColored(UI.Current.ImGui_Default_Text_Color with { W = 0.75f }, def.Description);

        // Options
        GUI.Spacer();
        ImGui.Checkbox($"{LOC.Get("FX_Inspector_Show_Raw")}##fxShowRaw", ref CFG.Current.FxEditor_Inspector_Show_Raw);
        GUI.Tooltip(LOC.Get("FX_Inspector_Show_Raw_TT"));
        ImGui.Checkbox($"{LOC.Get("FX_Inspector_Show_Unknown")}##fxShowUnknown", ref CFG.Current.FxEditor_Inspector_Show_Unknown);
        GUI.Tooltip(LOC.Get("FX_Inspector_Show_Unknown_TT"));

        if (!view.IsTyped)
        {
            GUI.Spacer();
            GUI.WrappedText(view.Layout == null
                ? LOC.Get("FX_Inspector_No_Layout", Data.Game)
                : LOC.Get("FX_Inspector_Does_Not_Fit"));
            DisplayRaw(action);
            return;
        }

        if (CFG.Current.FxEditor_Inspector_Show_Raw)
        {
            DisplayRaw(action);
            return;
        }

        var names = view.Names.ToList();
        var known = names.Where(n => view.Describe(n)?.IsUnknown != true).ToList();
        var unknown = names.Where(n => view.Describe(n)?.IsUnknown == true).ToList();

        // Values
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("FX_Inspector_Values_Header"), LOC.Get("FX_Inspector_Values_Header_TT"));
        DisplayValueTable("fxKnownValues", entry, action, view, known);

        // Unknown values
        if (unknown.Count > 0)
        {
            GUI.Spacer();
            GUI.ConditionalHeader(LOC.Get("FX_Inspector_Unknown_Header"), LOC.Get("FX_Inspector_Unknown_Header_TT"),
                ref CFG.Current.FxEditor_Inspector_Show_Unknown);
            if (CFG.Current.FxEditor_Inspector_Show_Unknown)
                DisplayValueTable("fxUnknownValues", entry, action, view, unknown);
        }
    }

    private void DisplayValueTable(string id, FxEntry entry, FxrAction action, FxrActionView view, List<string> names)
    {
        if (!ImGui.BeginTable(id, 2, ImGuiTableFlags.SizingStretchProp | ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInnerV))
            return;

        ImGui.TableSetupColumn("##name", ImGuiTableColumnFlags.WidthStretch, 0.4f);
        ImGui.TableSetupColumn("##value", ImGuiTableColumnFlags.WidthStretch, 0.6f);

        foreach (var name in names)
        {
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            var def = view.Describe(name);
            bool stored = view.IsStored(name);
            var label = FxNames.Pretty(name);
            if (view.KindOf(name) == FxrValueKind.Property)
            {
                // Properties open in the Curve Editor
                if (!stored) ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color with { W = 0.5f });
                if (ImGui.Selectable($"{label}##fxPropSelect_{name}", Editor.Selection.Property == name))
                    Editor.Selection.SelectProperty(name);
                if (!stored) ImGui.PopStyleColor();
            }
            else if (stored)
                ImGui.TextUnformatted(label);
            else
                ImGui.TextDisabled(label);
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip(Tooltip(name, def, stored, view));

            ImGui.TableSetColumnIndex(1);
            ImGui.SetNextItemWidth(-1);
            switch (view.KindOf(name))
            {
                case FxrValueKind.Field:
                    DisplayField(entry, action, view, name, def);
                    break;
                case FxrValueKind.Property:
                    DisplayProperty(entry, action, view, name, def);
                    break;
                case FxrValueKind.Section10:
                    var list = view.GetSection10(name);
                    ImGui.TextUnformatted(list == null || list.Count == 0 ? "—" : string.Join(", ", list));
                    break;
            }
        }

        ImGui.EndTable();
    }

    private string Tooltip(string name, FxrPropDef def, bool stored, FxrActionView view)
    {
        var lines = new List<string> { name };
        if (def?.Description != null) lines.Add(def.Description);
        if (def?.Argument != null) lines.Add(LOC.Get("FX_Inspector_Argument", def.Argument));
        if (view.ConversionsOf(name).Any()) lines.Add(LOC.Get("FX_Inspector_Converted"));
        if (!stored) lines.Add(LOC.Get("FX_Inspector_Not_Stored"));
        return string.Join("\n\n", lines);
    }

    // ---------------------------------------------------------------- fields

    private void DisplayField(FxEntry entry, FxrAction action, FxrActionView view, string name, FxrPropDef def)
    {
        view.FindField(name, out var slot, out _);
        var values = view.GetValue(name);
        string id = $"##fxField_{name}";

        void Commit(double[] newValues)
        {
            var copy = (double[])newValues.Clone();
            Editor.ActionManager.ExecuteAction(new FxEditFieldAction(Data.Bank, entry, action, a => Data.View(entry, a).SetValue(name, copy)));
        }

        switch (slot.Type)
        {
            case FxrFieldType.Bool:
            {
                bool b = values[0] != 0;
                if (ImGui.Checkbox(id, ref b))
                    Commit(new[] { b ? 1.0 : 0.0 });
                break;
            }
            case FxrFieldType.Int when def?.Enum != null && Data.Schema.Enums.TryGetValue(def.Enum, out var en):
            {
                int cur = (int)values[0];
                var member = en.Members.FirstOrDefault(m => m.Value == cur);
                var preview = member != null ? $"{member.Name} ({cur})" : cur.ToString();
                if (ImGui.BeginCombo(id, preview))
                {
                    foreach (var m in en.Members)
                    {
                        if (ImGui.Selectable($"{m.Name} ({m.Value})##{m.Name}", m.Value == cur))
                            Commit(new[] { (double)m.Value });
                        if (m.Description != null && ImGui.IsItemHovered())
                            ImGui.SetTooltip(m.Description);
                    }
                    ImGui.EndCombo();
                }
                break;
            }
            case FxrFieldType.Int when def?.Resource != null:
                DisplayResource(id, def, (int)values[0], v => Commit(new[] { (double)v }));
                break;
            case FxrFieldType.Int:
            {
                int v = Pending.TryGetValue(id, out var p) ? (int)p : (int)values[0];
                if (ImGui.InputInt(id, ref v))
                    Pending[id] = v;
                CommitOnDeactivate(id, () => Commit(new[] { (double)v }));
                if (def?.Resource != null && v > 0)
                    ImGui.TextDisabled(ResourceName(def, v));
                break;
            }
            default:
                DisplayFloats(id, values, def, Commit);
                break;
        }
    }

    /// <summary>Float and vector inputs (colour picker for colour values) that commit on deactivation.</summary>
    private void DisplayFloats(string id, double[] values, FxrPropDef def, Action<double[]> commit)
    {
        var cur = Pending.TryGetValue(id, out var p) ? (float[])p : values.Select(v => (float)v).ToArray();
        bool changed = false;

        if (cur.Length == 4 && def?.Color != null)
        {
            var v = new Vector4(cur[0], cur[1], cur[2], cur[3]);
            changed = ImGui.ColorEdit4(id, ref v, ImGuiColorEditFlags.Float | ImGuiColorEditFlags.Hdr | ImGuiColorEditFlags.AlphaPreviewHalf);
            cur = new[] { v.X, v.Y, v.Z, v.W };
        }
        else
        {
            switch (cur.Length)
            {
                case 1: { float v = cur[0]; changed = ImGui.InputFloat(id, ref v, 0, 0, "%.4g"); cur = new[] { v }; break; }
                case 2: { var v = new Vector2(cur[0], cur[1]); changed = ImGui.InputFloat2(id, ref v, "%.4g"); cur = new[] { v.X, v.Y }; break; }
                case 3: { var v = new Vector3(cur[0], cur[1], cur[2]); changed = ImGui.InputFloat3(id, ref v, "%.4g"); cur = new[] { v.X, v.Y, v.Z }; break; }
                case 4: { var v = new Vector4(cur[0], cur[1], cur[2], cur[3]); changed = ImGui.InputFloat4(id, ref v, "%.4g"); cur = new[] { v.X, v.Y, v.Z, v.W }; break; }
            }
        }

        if (changed)
            Pending[id] = cur;

        var final = cur;
        CommitOnDeactivate(id, () => commit(final.Select(f => (double)f).ToArray()));
    }

    private void CommitOnDeactivate(string id, Action commit)
    {
        if (!Pending.ContainsKey(id))
            return;

        if (ImGui.IsItemDeactivatedAfterEdit())
        {
            commit();
            Pending.Remove(id);
            PendingCommits.Remove(id);
        }
        else if (!ImGui.IsItemActive() && !ImGui.IsItemDeactivated())
        {
            // Focus went elsewhere without an edit
            Pending.Remove(id);
            PendingCommits.Remove(id);
        }
        else
        {
            // Still being typed in: Save commits it first (Ctrl+S doesn't take focus off the field)
            PendingCommits[id] = commit;
        }
    }

    private readonly Dictionary<string, Action> PendingCommits = new();

    /// <summary>Commits any value still being typed, so a save right after typing includes it.</summary>
    public void FlushPending()
    {
        foreach (var commit in PendingCommits.Values.ToList())
            commit();
        PendingCommits.Clear();
        Pending.Clear();
    }

    /// <summary>
    /// A texture, model or anibnd reference as text. The file stores an id, so what is typed is read as one: a
    /// number (88300) or a file name (s88300, s88300_a, s88300_a.tpf, s88300.flver.dcx, with any folder). Below
    /// it, the file the game loads for that id, and a warning when no sfx binder holds it.
    /// </summary>
    private void DisplayResource(string id, FxrPropDef def, int current, Action<int> commit)
    {
        string text = Pending.TryGetValue(id, out var p) && p is string s ? s : current.ToString();
        if (ImGui.InputText(id, ref text, 255))
            Pending[id] = text;
        GUI.Tooltip(LOC.Get("FX_Inspector_Resource_TT"));

        int? parsed = ParseResourceId(text);
        if (parsed is int typed)
            CommitOnDeactivate(id, () => commit(typed));
        else
            CommitOnDeactivate(id, () => { }); // Not an id: dropped when the field is left

        if (parsed == null)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("FX_Inspector_Resource_Invalid"));
            return;
        }

        int shown = parsed.Value;
        if (shown <= 0)
            return;

        // The typed TPF first, then the plain one, as FxTextureLibrary looks them up
        var file = ResourceName(def, shown);
        bool known = true;
        if (def.Resource == "texture")
        {
            var plain = $"s{shown:D5}";
            known = Data.Bank.Textures.ContainsKey(Path.GetFileNameWithoutExtension(file));
            if (!known && Data.Bank.Textures.ContainsKey(plain))
            {
                file = $"{plain}.tpf";
                known = true;
            }
        }
        else if (def.Resource == "model")
            known = Data.Bank.Models.ContainsKey($"s{shown:D5}");

        if (known || !Data.Bank.IsIndexed)
            ImGui.TextDisabled(file);
        else
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("FX_Inspector_Resource_Missing", file));
    }

    /// <summary>88300, s88300, s88300_a, s88300_a.tpf, s88300.flver.dcx or a path to one: the id 88300. Null if not one.</summary>
    private static int? ParseResourceId(string text)
    {
        var t = text?.Trim();
        if (string.IsNullOrEmpty(t))
            return null;

        t = t.Replace('\\', '/');
        t = t[(t.LastIndexOf('/') + 1)..];

        // Extensions, any number of them (.flver.dcx)
        int dot;
        while ((dot = t.LastIndexOf('.')) > 0 && !char.IsDigit(t[dot + 1]))
            t = t[..dot];

        if (t.Length > 1 && (t[0] == 's' || t[0] == 'S') && char.IsDigit(t[1]))
            t = t[1..];

        // The texture type suffix (_a, _n, _3m): the slot decides the type the game loads
        int underscore = t.IndexOf('_');
        if (underscore > 0)
            t = t[..underscore];

        return int.TryParse(t, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) && id >= 0 ? id : null;
    }

    private string ResourceName(FxrPropDef def, int id) => def.Resource switch
    {
        "texture" => Data.Game is FxrGame.DS3 or FxrGame.SDT ? $"s{id:D5}.tpf" : $"s{id:D5}_{def.TextureType ?? "a"}.tpf",
        "model" => $"s{id:D5}.flver",
        "anibnd" => $"s{id:D5}.anibnd",
        _ => def.Resource,
    };

    // ---------------------------------------------------------------- properties

    private void DisplayProperty(FxEntry entry, FxrAction action, FxrActionView view, string name, FxrPropDef def)
    {
        var prop = view.GetProperty(name);
        var curve = view.GetCurve(name);
        string id = $"##fxProp_{name}";

        if (curve.Constant is { Length: 1 } && def?.Resource != null)
        {
            DisplayResource(id, def, (int)Math.Round(curve.Constant[0]), v =>
            {
                Editor.ActionManager.ExecuteAction(new FxEditPropertyAction(Data.Bank, entry, action, a =>
                {
                    var pv = Data.View(entry, a);
                    var c = pv.GetCurve(name);
                    c.Function = FxrPropertyFunction.Constant;
                    c.Constant = new[] { (float)v };
                    pv.SetCurve(name, c);
                }));
            });
        }
        else if (curve.Constant != null)
        {
            DisplayFloats(id, curve.Constant.Select(v => (double)v).ToArray(), def, values =>
            {
                var copy = values.Select(v => (float)v).ToArray();
                Editor.ActionManager.ExecuteAction(new FxEditPropertyAction(Data.Bank, entry, action, a =>
                {
                    var v = Data.View(entry, a);
                    var c = v.GetCurve(name);
                    c.Function = FxrPropertyFunction.Constant;
                    c.Constant = copy;
                    v.SetCurve(name, c);
                }));
            });
            if (def?.Resource != null && curve.Constant.Length == 1 && curve.Constant[0] > 0)
                ImGui.TextDisabled(ResourceName(def, (int)Math.Round(curve.Constant[0])));
        }
        else
        {
            // Keyframed: a summary, which opens the curve in the Curve Editor
            int keys = curve.Function == FxrPropertyFunction.ComponentHermite
                ? curve.ComponentKeyframes.Sum(k => k.Count)
                : curve.Keyframes.Count;
            var summary = LOC.Get("FX_Inspector_Curve_Summary", curve.Function, keys, curve.Loop ? LOC.Get("FX_Inspector_Curve_Loop") : "");
            if (ImGui.Selectable($"{summary}{id}", Editor.Selection.Property == name))
                Editor.Selection.SelectProperty(name);
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.TextUnformatted(LOC.Get("FX_Inspector_Curve_Open_TT"));
                DisplayKeys(curve);
                ImGui.EndTooltip();
            }
        }

        if (prop?.Modifiers.Count > 0)
            ImGui.TextDisabled(LOC.Get("FX_Inspector_Modifiers", string.Join(", ", prop.Modifiers.Select(ModifierName))));
    }

    private static string ModifierName(FxrModifier m) => m.ModifierType switch
    {
        21 => "RandomDelta",
        24 => "RandomRange",
        38 => "ExternalValue1",
        39 => "ExternalValue2",
        53 => "RandomFraction",
        _ => m.ModifierType.ToString(),
    };

    private static void DisplayKeys(FxrCurve curve)
    {
        static string F(float[] v) => v == null ? "" : string.Join(", ", v.Select(x => x.ToString("G5", CultureInfo.InvariantCulture)));

        if (curve.Function == FxrPropertyFunction.ComponentHermite)
        {
            for (int c = 0; c < curve.ComponentKeyframes.Length; c++)
            {
                ImGui.TextDisabled(LOC.Get("FX_Inspector_Component", c));
                foreach (var k in curve.ComponentKeyframes[c])
                    ImGui.TextUnformatted($"{k.Position:G5}: {F(k.Value)}");
            }
            return;
        }
        foreach (var k in curve.Keyframes)
            ImGui.TextUnformatted($"{k.Position:G5}: {F(k.Value)}");
    }

    // ---------------------------------------------------------------- raw

    private static void DisplayRaw(FxrAction action)
    {
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("FX_Inspector_Raw_Header"), LOC.Get("FX_Inspector_Raw_Header_TT"));
        ImGui.TextUnformatted($"Unk02 {action.Unk02}  Unk03 {action.Unk03}  Unk04 {action.Unk04}");

        RawWords("fxRawFields1", LOC.Get("FX_Inspector_Raw_Fields1"), action.Fields1);
        RawWords("fxRawFields2", LOC.Get("FX_Inspector_Raw_Fields2"), action.Fields2);

        var props = action.Properties1.Select((p, i) => (p, $"P1[{i}]")).Concat(action.Properties2.Select((p, i) => (p, $"P2[{i}]")));
        foreach (var (p, label) in props)
        {
            var curve = FxrCurve.Decode(p);
            var text = curve.Constant != null
                ? string.Join(", ", curve.Constant.Select(v => v.ToString("G6", CultureInfo.InvariantCulture)))
                : $"{p.Function}, {p.Fields.Count} words";
            ImGui.TextUnformatted($"{label}  {p.ValueType} {p.Function}{(p.Loop ? " loop" : "")}: {text}{(p.Modifiers.Count > 0 ? $"  (+{p.Modifiers.Count} mod)" : "")}");
        }
        for (int i = 0; i < action.Section10s.Count; i++)
            ImGui.TextUnformatted($"S10[{i}]  {string.Join(", ", action.Section10s[i])}");
    }

    private static void RawWords(string id, string title, List<uint> words)
    {
        if (words.Count == 0)
            return;
        ImGui.TextDisabled(title);
        if (!ImGui.BeginTable(id, 4, ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit))
            return;
        for (int i = 0; i < words.Count; i++)
        {
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0); ImGui.TextDisabled(i.ToString());
            ImGui.TableSetColumnIndex(1); ImGui.TextUnformatted($"0x{words[i]:X8}");
            ImGui.TableSetColumnIndex(2); ImGui.TextUnformatted(((int)words[i]).ToString());
            ImGui.TableSetColumnIndex(3); ImGui.TextUnformatted(BitConverter.UInt32BitsToSingle(words[i]).ToString("G6", CultureInfo.InvariantCulture));
        }
        ImGui.EndTable();
    }
}
