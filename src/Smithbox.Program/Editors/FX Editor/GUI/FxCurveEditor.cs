using Hexa.NET.ImGui;
using Hexa.NET.ImPlot;
using SoulsFormats;
using System.Numerics;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// The Curve Editor (FX Editor Plan, phase E3): the property picked in the Action Inspector, as a graph and a key
/// table, with its function, loop flag and modifiers.
///
/// It edits a working copy of the curve in display units (seconds, degrees, ...), except Hermite tangents, which
/// are angles and are kept as stored (see <see cref="FxCurveOps.ToEditable"/>). Changes go into the copy while a
/// value is typed or a key dragged, and are committed as one undo step once nothing is active. Any edit, undo or
/// redo rebuilds the copy from the file.
/// </summary>
public class FxCurveEditor
{
    private readonly FxEditorScreen Editor;

    // The working copy and what it was made from
    private FxrCurve Working;
    private float[] Factors;
    private (FxEntry Entry, FxrAction Action, string Name, int Version) WorkingFor;
    private bool Dirty;
    private bool DragHeld;
    private int Version;
    private int Component;

    private readonly Random Seeds = new();

    public FxCurveEditor(FxEditorScreen editor)
    {
        Editor = editor;
        editor.ActionManager.ActionPerformed += () => Version++;
    }

    private FxData Data => Editor.Data;

    public void Display()
    {
        GUI.TitleHeader(LOC.Get("FX_CurveEditor_Header"), LOC.Get("FX_CurveEditor_Header_TT"));

        ImGui.BeginChild("FxCurveEditorSection", ImGuiChildFlags.Borders);

        var entry = Editor.Selection.Entry;
        var action = Editor.Selection.Action;
        var name = Editor.Selection.Property;

        if (entry?.Fxr == null || action == null || name == null)
        {
            GUI.WrappedText(LOC.Get("FX_CurveEditor_No_Property"));
            ImGui.EndChild();
            return;
        }

        var view = Data.View(entry, action);
        if (!view.IsTyped || view.KindOf(name) != FxrValueKind.Property)
        {
            GUI.WrappedText(LOC.Get("FX_CurveEditor_Not_Property"));
            ImGui.EndChild();
            return;
        }

        Prepare(entry, action, name, view);

        DisplayHeader(view, name);
        DisplayFunction(view);
        DisplayGraph(view, name);
        DisplayKeys();
        DisplayModifiers(entry, action, view, name);

        CommitIfIdle(entry, action, name);

        ImGui.EndChild();
    }

    /// <summary>Remakes the working copy when the property, or the file under it, changed (and nothing is pending).</summary>
    private void Prepare(FxEntry entry, FxrAction action, string name, FxrActionView view)
    {
        var key = (entry, action, name, Version);
        if (Working != null && key.Equals(WorkingFor))
            return;
        if (Working != null && Dirty && (entry, action, name) == (WorkingFor.Entry, WorkingFor.Action, WorkingFor.Name))
            return;

        Factors = Enumerable.Range(0, 4).Select(i => (float)FxrConversions.CurveFactor(view.ConversionsOf(name), i)).ToArray();
        Working = FxCurveOps.ToEditable(view.GetCurve(name), Factors);
        WorkingFor = key;
        Dirty = false;
        Component = Math.Min(Component, Working.Components - 1);
    }

    /// <summary>Commits an edit still in progress, so a save includes it.</summary>
    public void FlushPending()
    {
        if (Dirty && Working != null && WorkingFor.Entry != null)
            Commit(WorkingFor.Entry, WorkingFor.Action, WorkingFor.Name);
    }

    private void CommitIfIdle(FxEntry entry, FxrAction action, string name)
    {
        if (!Dirty || ImGui.IsAnyItemActive() || DragHeld)
            return;
        Commit(entry, action, name);
    }

    private void Commit(FxEntry entry, FxrAction action, string name)
    {
        Dirty = false;
        FxCurveOps.SortKeys(Working);
        var display = FxCurveOps.ToDisplay(Working, Factors);
        FxCurveOps.Normalize(display);

        Editor.ActionManager.ExecuteAction(new FxEditPropertyAction(Data.Bank, entry, action, a =>
            Data.View(entry, a).SetCurve(name, display)));
    }

    // ---------------------------------------------------------------- header and function

    private void DisplayHeader(FxrActionView view, string name)
    {
        var def = view.Describe(name);

        // Property
        GUI.SimpleHeader(LOC.Get("FX_CurveEditor_Property_Header"), LOC.Get("FX_CurveEditor_Property_Header_TT"));
        GUI.WrappedText($"{view.Definition.Name}: {FxNames.Pretty(name)}");
        if (def?.Argument != null)
            GUI.WrappedText(LOC.Get("FX_CurveEditor_Argument", ArgumentName(def.Argument)));
        if (view.ConversionsOf(name).Any())
            GUI.WrappedTextColored(UI.Current.ImGui_Default_Text_Color with { W = 0.75f }, LOC.Get("FX_Inspector_Converted"));
        if (def?.Description != null && CFG.Current.FxEditor_Inspector_Show_Descriptions)
            GUI.WrappedTextColored(UI.Current.ImGui_Default_Text_Color with { W = 0.75f }, def.Description);
    }

    private static string ArgumentName(string argument) => argument switch
    {
        "ParticleAge" => LOC.Get("FX_Argument_ParticleAge"),
        "ActiveTime" => LOC.Get("FX_Argument_ActiveTime"),
        "EmissionTime" => LOC.Get("FX_Argument_EmissionTime"),
        "Constant0" => LOC.Get("FX_Argument_Constant0"),
        _ => argument,
    };

    private void DisplayFunction(FxrActionView view)
    {
        // Function
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("FX_CurveEditor_Function_Header"), LOC.Get("FX_CurveEditor_Function_Header_TT"));

        var current = Working.Function <= FxrPropertyFunction.Constant ? FxrPropertyFunction.Constant : Working.Function;
        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo("##fxCurveFunction", FunctionName(current)))
        {
            foreach (var f in FxCurveOps.Functions(Data.Game))
            {
                if (ImGui.Selectable($"{FunctionName(f)}##fxFunc{(int)f}", f == current) && f != current)
                {
                    Working = FxCurveOps.Convert(Working, f);
                    Component = 0;
                    Dirty = true;
                }
                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip(LOC.Get($"FX_Function_{f}_TT"));
            }
            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("FX_CurveEditor_Function_TT"));

        if (FxCurveOps.IsKeyed(Working.Function))
        {
            bool loop = Working.Loop;
            if (ImGui.Checkbox($"{LOC.Get("FX_CurveEditor_Loop")}##fxCurveLoop", ref loop))
            {
                Working.Loop = loop;
                Dirty = true;
            }
            GUI.Tooltip(LOC.Get("FX_CurveEditor_Loop_TT"));
        }

        if (Working.Function == FxrPropertyFunction.ComponentHermite)
        {
            float duration = Working.Duration;
            ImGui.SetNextItemWidth(-1);
            if (ImGui.InputFloat("##fxCurveDuration", ref duration, 0, 0, "%.4g s"))
            {
                Working.Duration = Math.Max(duration, 0);
                Dirty = true;
            }
            GUI.Tooltip(LOC.Get("FX_CurveEditor_Duration_TT"));
        }
    }

    private static string FunctionName(FxrPropertyFunction f) => LOC.Get($"FX_Function_{f}");

    // ---------------------------------------------------------------- graph

    private static readonly string[] AxisLabels = { "X", "Y", "Z", "W" };
    private static readonly string[] ColorLabels = { "R", "G", "B", "A" };

    private unsafe void DisplayGraph(FxrActionView view, string name)
    {
        // Graph
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("FX_CurveEditor_Graph_Header"), LOC.Get("FX_CurveEditor_Graph_Header_TT"));

        int c = Working.Components;
        bool keyed = FxCurveOps.IsKeyed(Working.Function);
        bool isColor = view.Describe(name)?.Color != null;
        var labels = isColor ? ColorLabels : AxisLabels;

        // Up to the last key plus some, or a full extra period of a looping curve
        float end = Math.Max(Working.SequenceDuration, Working.Function == FxrPropertyFunction.ComponentHermite ? Working.Duration : 0);
        float span = keyed ? Math.Max(end * (Working.Loop ? 2f : 1.25f), 0.1f) : 1f;

        const int samples = 256;
        var xs = new double[samples];
        var ys = new double[c][];
        for (int i = 0; i < c; i++) ys[i] = new double[samples];
        var value = new float[4];
        for (int s = 0; s < samples; s++)
        {
            float x = span * s / (samples - 1);
            xs[s] = x;
            if (keyed) Working.EvaluateInto(x, value);
            else for (int i = 0; i < c; i++) value[i] = Working.Constant?[i] ?? 0;
            for (int i = 0; i < c; i++) ys[i][s] = float.IsFinite(value[i]) ? value[i] : 0;
        }

        // The random modifiers' range around the curve
        var prop = view.GetProperty(name);
        var bands = new List<(string Label, double[] Low, double[] High, int Component)>();
        if (prop != null)
        {
            foreach (var m in prop.Modifiers.Where(m => FxCurveOps.IsRandom(m.ModifierType)))
            {
                var ranges = FxCurveOps.GetRanges(m, Factors);
                for (int i = 0; i < c; i++)
                {
                    var low = new double[samples];
                    var high = new double[samples];
                    for (int s = 0; s < samples; s++)
                    {
                        double v = ys[i][s];
                        (low[s], high[s]) = m.ModifierType switch
                        {
                            FxCurveOps.RandomRange => (v + ranges[0][i], v + ranges[1][i]),
                            FxCurveOps.RandomFraction => (v - Math.Abs(v * ranges[0][i]), v + Math.Abs(v * ranges[0][i])),
                            _ => (v - ranges[0][i], v + ranges[0][i]),
                        };
                    }
                    bands.Add(($"{FxCurveOps.ModifierName(m.ModifierType)} {labels[i]}", low, high, i));
                }
            }
        }

        DragHeld = false;
        var size = new Vector2(-1, 220 * DPI.UIScale());
        if (!ImPlot.BeginPlot("##fxCurvePlot", size))
            return;

        ImPlot.SetupAxes(LOC.Get("FX_CurveEditor_Graph_Time"), LOC.Get("FX_CurveEditor_Graph_Value"));
        ImPlot.SetupAxisLimits(ImAxis.X1, 0, span, ImPlotCond.Always);

        // Fit the values (and the random bands) with a margin, but don't follow a key while it is dragged
        double min = ys.SelectMany(a => a).DefaultIfEmpty(0).Min(), max = ys.SelectMany(a => a).DefaultIfEmpty(1).Max();
        foreach (var b in bands)
        {
            min = Math.Min(min, b.Low.Min());
            max = Math.Max(max, b.High.Max());
        }
        double pad = Math.Max((max - min) * 0.1, 0.05);
        ImPlot.SetupAxisLimits(ImAxis.Y1, min - pad, max + pad, ImPlotCond.Once);

        fixed (double* x = xs)
        {
            for (int i = 0; i < c; i++)
                fixed (double* y = ys[i])
                    ImPlot.PlotLine(c == 1 ? LOC.Get("FX_CurveEditor_Graph_Value") : labels[i], x, y, samples);

            foreach (var b in bands)
                fixed (double* lo = b.Low)
                fixed (double* hi = b.High)
                    ImPlot.PlotShaded(b.Label, x, lo, hi, samples);
        }

        // Keys, dragged in place: a shared key moves all components' positions together
        if (keyed)
        {
            int id = 0;
            for (int comp = 0; comp < c; comp++)
            {
                var keys = FxCurveOps.Keys(Working, comp);
                int at = Working.Function == FxrPropertyFunction.ComponentHermite ? 0 : comp;
                if (Working.Function == FxrPropertyFunction.ComponentHermite && comp != Component)
                    continue;

                foreach (var k in keys)
                {
                    double px = k.Position, py = k.Value[at];
                    bool clicked = false, hovered = false, held = false;
                    var colour = comp == Component ? new Vector4(1, 0.85f, 0.2f, 1) : new Vector4(0.8f, 0.8f, 0.8f, 1);
                    if (ImPlot.DragPoint(id++, &px, &py, colour, 5 * DPI.UIScale(), ImPlotDragToolFlags.None, &clicked, &hovered, &held))
                    {
                        k.Position = Math.Max((float)px, 0);
                        k.Value[at] = (float)py;
                        Dirty = true;
                    }
                    if (held)
                        DragHeld = true;
                }
            }

            // Double-click adds a key where clicked (to every component, or the shown ComponentHermite one)
            if (ImPlot.IsPlotHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            {
                var mouse = ImPlot.GetPlotMousePos(ImAxis.X1);
                FxCurveOps.AddKey(Working, Component, Math.Max((float)mouse.X, 0));
                Dirty = true;
            }
        }

        ImPlot.EndPlot();
        GUI.Tooltip(LOC.Get("FX_CurveEditor_Graph_TT"));
    }

    // ---------------------------------------------------------------- keys

    private void DisplayKeys()
    {
        // Keys
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("FX_CurveEditor_Keys_Header"), LOC.Get("FX_CurveEditor_Keys_Header_TT"));

        if (!FxCurveOps.IsKeyed(Working.Function))
        {
            // A constant: its value
            var v = Working.Constant ??= new float[Working.Components];
            for (int i = 0; i < v.Length; i++)
            {
                ImGui.SetNextItemWidth(-1);
                if (ImGui.InputFloat($"##fxCurveConstant{i}", ref v[i], 0, 0, "%.5g"))
                {
                    if (Working.Function != FxrPropertyFunction.Constant)
                        Working.Function = FxrPropertyFunction.Constant;
                    Dirty = true;
                }
                GUI.Tooltip(LOC.Get("FX_CurveEditor_Constant_TT"));
            }
            return;
        }

        bool component = Working.Function == FxrPropertyFunction.ComponentHermite;
        if (component && Working.Components > 1)
        {
            ImGui.SetNextItemWidth(-1);
            if (ImGui.SliderInt("##fxCurveComponent", ref Component, 0, Working.Components - 1, LOC.Get("FX_CurveEditor_Component")))
                Component = Math.Clamp(Component, 0, Working.Components - 1);
            GUI.Tooltip(LOC.Get("FX_CurveEditor_Component_TT"));
        }

        var keys = FxCurveOps.Keys(Working, Component);
        int width = component ? 1 : Working.Components;
        bool tangents = Working.Function is FxrPropertyFunction.Bezier or FxrPropertyFunction.Hermite or FxrPropertyFunction.ComponentHermite;
        int columns = 3 + (tangents ? 2 : 0);
        int remove = -1;

        if (ImGui.BeginTable("fxCurveKeys", columns, ImGuiTableFlags.SizingStretchProp | ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInnerV))
        {
            ImGui.TableSetupColumn(LOC.Get("FX_CurveEditor_Position"), ImGuiTableColumnFlags.WidthStretch, 0.6f);
            ImGui.TableSetupColumn(LOC.Get("FX_CurveEditor_Value"), ImGuiTableColumnFlags.WidthStretch, 1.0f);
            if (tangents)
            {
                bool bezier = Working.Function == FxrPropertyFunction.Bezier;
                ImGui.TableSetupColumn(LOC.Get(bezier ? "FX_CurveEditor_Bezier_Leave" : "FX_CurveEditor_Hermite_Angle1"), ImGuiTableColumnFlags.WidthStretch, 1.0f);
                ImGui.TableSetupColumn(LOC.Get(bezier ? "FX_CurveEditor_Bezier_Arrive" : "FX_CurveEditor_Hermite_Angle2"), ImGuiTableColumnFlags.WidthStretch, 1.0f);
            }
            ImGui.TableSetupColumn("##remove", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableHeadersRow();

            for (int k = 0; k < keys.Count; k++)
            {
                var key = keys[k];
                ImGui.TableNextRow();

                ImGui.TableSetColumnIndex(0);
                float position = key.Position;
                ImGui.SetNextItemWidth(-1);
                if (ImGui.InputFloat($"##fxKeyPos{k}", ref position, 0, 0, "%.4g"))
                {
                    key.Position = Math.Max(position, 0);
                    Dirty = true;
                }
                GUI.Tooltip(LOC.Get("FX_CurveEditor_Position_TT"));

                ImGui.TableSetColumnIndex(1);
                Floats($"##fxKeyVal{k}", key.Value, width, "FX_CurveEditor_Value_TT");

                if (tangents)
                {
                    key.In ??= new float[width];
                    key.Out ??= new float[width];
                    ImGui.TableSetColumnIndex(2);
                    Floats($"##fxKeyIn{k}", key.In, width, Working.Function == FxrPropertyFunction.Bezier ? "FX_CurveEditor_Bezier_Leave_TT" : "FX_CurveEditor_Hermite_Angle_TT");
                    ImGui.TableSetColumnIndex(3);
                    Floats($"##fxKeyOut{k}", key.Out, width, Working.Function == FxrPropertyFunction.Bezier ? "FX_CurveEditor_Bezier_Arrive_TT" : "FX_CurveEditor_Hermite_Angle_TT");
                }

                ImGui.TableSetColumnIndex(columns - 1);
                ImGui.BeginDisabled(keys.Count <= 1);
                if (ImGui.Button($"{Icons.Minus}##fxKeyRemove{k}"))
                    remove = k;
                ImGui.EndDisabled();
                GUI.Tooltip(LOC.Get("FX_CurveEditor_Remove_Key_TT"));
            }

            ImGui.EndTable();
        }

        if (remove >= 0 && FxCurveOps.RemoveKey(Working, Component, remove))
            Dirty = true;

        // Actions
        GUI.MultiButtonInput("fxCurveKeyButtons",
            "fxAddKey", LOC.Get("FX_CurveEditor_Add_Key"), LOC.Get("FX_CurveEditor_Add_Key_TT"), () =>
            {
                // After the last key, by the gap before it (or a tenth of a second)
                var list = FxCurveOps.Keys(Working, Component);
                float last = list.Count > 0 ? list[^1].Position : 0;
                float gap = list.Count > 1 ? last - list[^2].Position : 0.1f;
                FxCurveOps.AddKey(Working, Component, last + (gap > 0 ? gap : 0.1f));
                Dirty = true;
            });
    }

    private void Floats(string id, float[] values, int count, string tooltip)
    {
        ImGui.SetNextItemWidth(-1);
        bool changed = false;
        switch (count)
        {
            case 1: changed = ImGui.InputFloat(id, ref values[0], 0, 0, "%.4g"); break;
            case 2: { var v = new Vector2(values[0], values[1]); changed = ImGui.InputFloat2(id, ref v, "%.4g"); values[0] = v.X; values[1] = v.Y; break; }
            case 3: { var v = new Vector3(values[0], values[1], values[2]); changed = ImGui.InputFloat3(id, ref v, "%.4g"); values[0] = v.X; values[1] = v.Y; values[2] = v.Z; break; }
            default: { var v = new Vector4(values[0], values[1], values[2], values[3]); changed = ImGui.InputFloat4(id, ref v, "%.4g"); values[0] = v.X; values[1] = v.Y; values[2] = v.Z; values[3] = v.W; break; }
        }
        if (changed) Dirty = true;
        GUI.Tooltip(LOC.Get(tooltip));
    }

    // ---------------------------------------------------------------- modifiers

    // Modifier fields being typed, committed once the input lets go
    private readonly Dictionary<string, object> PendingModifier = new();

    private void DisplayModifiers(FxEntry entry, FxrAction action, FxrActionView view, string name)
    {
        // Modifiers
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("FX_CurveEditor_Modifiers_Header"), LOC.Get("FX_CurveEditor_Modifiers_Header_TT"));

        var prop = view.GetProperty(name);
        var modifiers = prop?.Modifiers ?? new List<FxrModifier>();
        int c = Working.Components;

        if (modifiers.Count == 0)
            GUI.WrappedText(LOC.Get("FX_CurveEditor_No_Modifiers"));

        for (int m = 0; m < modifiers.Count; m++)
        {
            var mod = modifiers[m];
            int index = m;
            ImGui.PushID($"fxMod{m}");

            ImGui.Separator();
            ImGui.TextUnformatted(FxCurveOps.ModifierName(mod.ModifierType));
            ImGui.SameLine();
            if (ImGui.SmallButton($"{Icons.Minus}##fxModRemove"))
                EditModifiers(entry, action, name, list => list.RemoveAt(index));
            GUI.Tooltip(LOC.Get("FX_CurveEditor_Remove_Modifier_TT"));
            GUI.WrappedTextColored(UI.Current.ImGui_Default_Text_Color with { W = 0.75f }, LOC.Get($"FX_Modifier_{FxCurveOps.ModifierName(mod.ModifierType)}_TT"));

            if (FxCurveOps.IsRandom(mod.ModifierType))
            {
                var ranges = FxCurveOps.GetRanges(mod, Factors);
                string[] labels = mod.ModifierType == FxCurveOps.RandomRange
                    ? new[] { "FX_CurveEditor_Modifier_Min", "FX_CurveEditor_Modifier_Max" }
                    : new[] { mod.ModifierType == FxCurveOps.RandomFraction ? "FX_CurveEditor_Modifier_Fraction" : "FX_CurveEditor_Modifier_Delta" };

                for (int g = 0; g < ranges.Count; g++)
                {
                    int group = g;
                    ImGui.TextDisabled(LOC.Get(labels[g]));
                    var key = $"range{m}_{g}";
                    var values = PendingModifier.TryGetValue(key, out var p) ? (float[])p : ranges[g];
                    if (EditFloats($"##fxModRange{g}", values, c))
                        PendingModifier[key] = values;
                    if (ImGui.IsItemDeactivatedAfterEdit() && PendingModifier.Remove(key))
                    {
                        var copy = (float[])values.Clone();
                        EditModifiers(entry, action, name, list => FxCurveOps.SetRange(list[index], group, copy, Factors));
                    }
                    GUI.Tooltip(LOC.Get("FX_CurveEditor_Modifier_Range_TT"));
                }

                // Seeds
                var seeds = FxCurveOps.GetSeeds(mod);
                ImGui.TextDisabled(LOC.Get("FX_CurveEditor_Modifier_Seeds"));
                for (int i = 0; i < seeds.Length; i++)
                {
                    int comp = i;
                    var key = $"seed{m}_{i}";
                    int seed = PendingModifier.TryGetValue(key, out var p) ? (int)p : (int)seeds[i];
                    ImGui.SetNextItemWidth(-1);
                    if (ImGui.InputInt($"##fxModSeed{i}", ref seed))
                        PendingModifier[key] = seed;
                    if (ImGui.IsItemDeactivatedAfterEdit() && PendingModifier.Remove(key))
                    {
                        uint value = (uint)seed;
                        EditModifiers(entry, action, name, list => FxCurveOps.SetSeed(list[index], comp, value));
                    }
                    GUI.Tooltip(LOC.Get("FX_CurveEditor_Modifier_Seed_TT"));
                }
            }
            else
            {
                // External value
                var key = $"ext{m}";
                int id = PendingModifier.TryGetValue(key, out var p) ? (int)p : FxCurveOps.GetExternalId(mod);
                ImGui.TextDisabled(LOC.Get("FX_CurveEditor_Modifier_External"));
                ImGui.SetNextItemWidth(-1);
                if (ImGui.InputInt("##fxModExternal", ref id))
                    PendingModifier[key] = id;
                if (ImGui.IsItemDeactivatedAfterEdit() && PendingModifier.Remove(key))
                {
                    int value = Math.Max(id, 0);
                    EditModifiers(entry, action, name, list => FxCurveOps.SetExternalId(list[index], value));
                }
                GUI.Tooltip(LOC.Get("FX_CurveEditor_Modifier_External_TT"));

                // The factor curve, by external value
                if (mod.Properties.Count > 0)
                {
                    var factor = FxrCurve.Decode(mod.Properties[0]);
                    ImGui.TextDisabled(LOC.Get("FX_CurveEditor_Modifier_Factor"));
                    DisplayFactorKeys(entry, action, name, index, factor);
                }
            }

            ImGui.PopID();
        }

        // Add
        ImGui.Separator();
        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo("##fxModAdd", LOC.Get("FX_CurveEditor_Add_Modifier")))
        {
            foreach (var type in FxCurveOps.ModifierTypes(Data.Game))
            {
                if (ImGui.Selectable($"{FxCurveOps.ModifierName(type)}##fxModType{type}"))
                {
                    var valueType = Working.ValueType;
                    var mod = FxCurveOps.CreateModifier(Data.Game, type, valueType, Seeds);
                    EditModifiers(entry, action, name, list => list.Add(mod));
                }
                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip(LOC.Get($"FX_Modifier_{FxCurveOps.ModifierName(type)}_TT"));
            }
            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("FX_CurveEditor_Add_Modifier_TT"));
    }

    /// <summary>An external value modifier's factor curve: its keys (or constant), edited in place.</summary>
    private void DisplayFactorKeys(FxEntry entry, FxrAction action, string name, int index, FxrCurve factor)
    {
        if (!FxCurveOps.IsKeyed(factor.Function))
        {
            var key = $"factorConst{index}";
            var values = PendingModifier.TryGetValue(key, out var p) ? (float[])p : (float[])(factor.Constant ?? new float[factor.Components]).Clone();
            if (EditFloats("##fxFactorConst", values, factor.Components))
                PendingModifier[key] = values;
            if (ImGui.IsItemDeactivatedAfterEdit() && PendingModifier.Remove(key))
            {
                var copy = (float[])values.Clone();
                EditFactor(entry, action, name, index, f => { f.Function = FxrPropertyFunction.Constant; f.Constant = copy; });
            }
            return;
        }

        if (factor.Function == FxrPropertyFunction.ComponentHermite)
        {
            ImGui.TextDisabled(LOC.Get("FX_Inspector_Curve_Summary", factor.Function, factor.ComponentKeyframes.Sum(k => k.Count), ""));
            return;
        }

        for (int k = 0; k < factor.Keyframes.Count; k++)
        {
            int keyIndex = k;
            var kf = factor.Keyframes[k];
            var key = $"factor{index}_{k}";
            var values = PendingModifier.TryGetValue(key, out var p) ? (float[])p : new[] { kf.Position }.Concat(kf.Value).ToArray();
            if (EditFloats($"##fxFactorKey{k}", values, values.Length))
                PendingModifier[key] = values;
            if (ImGui.IsItemDeactivatedAfterEdit() && PendingModifier.Remove(key))
            {
                var copy = (float[])values.Clone();
                EditFactor(entry, action, name, index, f =>
                {
                    f.Keyframes[keyIndex].Position = Math.Max(copy[0], 0);
                    f.Keyframes[keyIndex].Value = copy.Skip(1).ToArray();
                    f.Keyframes = f.Keyframes.OrderBy(x => x.Position).ToList();
                });
            }
            GUI.Tooltip(LOC.Get("FX_CurveEditor_Modifier_Factor_Key_TT"));
        }
    }

    private static bool EditFloats(string id, float[] values, int count)
    {
        ImGui.SetNextItemWidth(-1);
        switch (Math.Min(count, values.Length))
        {
            case 1: return ImGui.InputFloat(id, ref values[0], 0, 0, "%.4g");
            case 2: { var v = new Vector2(values[0], values[1]); bool r = ImGui.InputFloat2(id, ref v, "%.4g"); values[0] = v.X; values[1] = v.Y; return r; }
            case 3: { var v = new Vector3(values[0], values[1], values[2]); bool r = ImGui.InputFloat3(id, ref v, "%.4g"); values[0] = v.X; values[1] = v.Y; values[2] = v.Z; return r; }
            default: { var v = new Vector4(values[0], values[1], values[2], values[3]); bool r = ImGui.InputFloat4(id, ref v, "%.4g"); values[0] = v.X; values[1] = v.Y; values[2] = v.Z; values[3] = v.W; return r; }
        }
    }

    /// <summary>A change to the property's modifier list, as one undo step. A property not stored yet is stored first.</summary>
    private void EditModifiers(FxEntry entry, FxrAction action, string name, Action<List<FxrModifier>> change)
    {
        Editor.ActionManager.ExecuteAction(new FxEditPropertyAction(Data.Bank, entry, action, a =>
        {
            var v = Data.View(entry, a);
            var prop = v.GetProperty(name);
            if (prop == null)
            {
                v.SetCurve(name, v.GetCurve(name));
                prop = v.GetProperty(name);
            }
            change(prop.Modifiers);
        }));
    }

    private void EditFactor(FxEntry entry, FxrAction action, string name, int index, Action<FxrCurve> change)
    {
        EditModifiers(entry, action, name, list =>
        {
            var target = list[index].Properties[0];
            var curve = FxrCurve.Decode(target);
            change(curve);
            int typeB = target.TypeEnumB;
            curve.EncodeInto(target);
            target.TypeEnumB = typeB != 0 ? target.PackTypeEnumB() : 0;
        });
    }
}
