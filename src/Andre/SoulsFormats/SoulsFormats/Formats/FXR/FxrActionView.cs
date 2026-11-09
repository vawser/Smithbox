using System;
using System.Collections.Generic;
using System.Text.Json;

namespace SoulsFormats
{
    public enum FxrValueKind
    {
        None,
        Field,
        Property,
        Section10,
    }

    /// <summary>
    /// Named access to an action's raw data through the schema layout for one game.
    /// Values are the file's raw values (fxr-main's display conversions are Phase 3).
    /// Reading a name past the end of a short list gives the schema default; writing one pads the
    /// list with defaults up to it.
    /// </summary>
    public class FxrActionView
    {
        public FxrSchema Schema { get; }
        public FxrAction Action { get; }
        public FxrGame Game { get; }

        /// <summary>Null when the schema has no layout for this action type in this game.</summary>
        public FxrActionLayout Layout { get; }

        /// <summary>True when named access applies (fxr-main's fit rule); otherwise show raw words.</summary>
        public bool IsTyped => Layout != null && Layout.Fits(Action);

        public FxrActionDef Definition => Layout?.Action;

        public FxrActionView(FxrSchema schema, FxrAction action, FxrGame game)
        {
            Schema = schema;
            Action = action;
            Game = game;
            Layout = schema.GetLayout(action.Type, game);
        }

        /// <summary>Uses the file's ComponentHermite bounds style (see <see cref="FXR.ComponentBoundsFromLast"/>).</summary>
        public FxrActionView(FxrSchema schema, FxrAction action, FxrGame game, FXR file) : this(schema, action, game)
        {
            ComponentBoundsFromLast = file.ComponentBoundsFromLast;
        }

        /// <summary>Bounds rule for edited ComponentHermite curves; defaults to the common one (last value).</summary>
        public bool ComponentBoundsFromLast { get; set; } = true;

        private FxrCurve DecodeCurve(FxrProperty p)
        {
            var curve = FxrCurve.Decode(p);
            curve.ComponentBoundsFromLast = ComponentBoundsFromLast;
            return curve;
        }

        /// <summary>DS3 Tracer (10012) sometimes has an extra -2 word in front of fields2.</summary>
        public static bool HasDs3TracerQuirk(FxrAction a, FxrGame game) =>
            game == FxrGame.DS3 && a.Type == 10012 && a.Fields2.Count > 0 && (int)a.Fields2[0] == -2;

        private int Fields2Skip => HasDs3TracerQuirk(Action, Game) ? 1 : 0;

        /// <summary>Every name in layout order: fields1, fields2, properties1, properties2, section10s.</summary>
        public IEnumerable<string> Names
        {
            get
            {
                if (Layout == null) yield break;
                foreach (var f in Layout.Fields1) yield return f.Name;
                foreach (var f in Layout.Fields2) yield return f.Name;
                foreach (var p in Layout.Properties1) yield return p;
                foreach (var p in Layout.Properties2) yield return p;
                foreach (var s in Layout.Section10s) yield return s;
            }
        }

        public FxrPropDef Describe(string name) => Definition?.Props.GetValueOrDefault(name);

        public FxrValueKind KindOf(string name)
        {
            if (Layout == null) return FxrValueKind.None;
            if (FindField(name, out _, out _)) return FxrValueKind.Field;
            if (Layout.Properties1.Contains(name) || Layout.Properties2.Contains(name)) return FxrValueKind.Property;
            if (Layout.Section10s.Contains(name)) return FxrValueKind.Section10;
            return FxrValueKind.None;
        }

        /// <summary>True when the file actually stores this value (not only the schema default).</summary>
        public bool IsStored(string name)
        {
            switch (KindOf(name))
            {
                case FxrValueKind.Field:
                    FindField(name, out var slot, out var list);
                    return slot.Word + slot.Words + (list == 2 ? Fields2Skip : 0) <= Words(list).Count;
                case FxrValueKind.Property:
                    return PropertyIndex(name, out var pl, out var pi) && pi < Props(pl).Count;
                case FxrValueKind.Section10:
                    return Layout.Section10s.IndexOf(name) < Action.Section10s.Count;
                default:
                    return false;
            }
        }

        // ---------------------------------------------------------------- fields

        public bool FindField(string name, out FxrFieldSlot slot, out int list)
        {
            slot = null;
            list = 0;
            if (Layout == null) return false;
            foreach (var f in Layout.Fields1) if (f.Name == name) { slot = f; list = 1; return true; }
            foreach (var f in Layout.Fields2) if (f.Name == name) { slot = f; list = 2; return true; }
            return false;
        }

        private List<uint> Words(int list) => list == 1 ? Action.Fields1 : Action.Fields2;

        /// <summary>The field's raw words (1 for scalars, 2-4 for vectors).</summary>
        public uint[] GetWords(string name)
        {
            if (!FindField(name, out var slot, out var list))
                throw new KeyNotFoundException($"'{name}' is not a field of {Definition?.Name ?? Action.Type.ToString()} in {Game}.");
            var words = Words(list);
            int start = slot.Word + (list == 2 ? Fields2Skip : 0);
            var result = new uint[slot.Words];
            for (int i = 0; i < slot.Words; i++)
                result[i] = start + i < words.Count ? words[start + i] : Schema.DefaultWord(slot.Prop, slot.Type, i);
            return result;
        }

        public void SetWords(string name, uint[] value)
        {
            if (!FindField(name, out var slot, out var list))
                throw new KeyNotFoundException($"'{name}' is not a field of {Definition?.Name ?? Action.Type.ToString()} in {Game}.");
            if (value.Length != slot.Words)
                throw new ArgumentException($"'{name}' takes {slot.Words} words, got {value.Length}.");
            var words = Words(list);
            int skip = list == 2 ? Fields2Skip : 0;
            PadFields(list, slot.Word + slot.Words + skip);
            for (int i = 0; i < value.Length; i++) words[slot.Word + skip + i] = value[i];
        }

        /// <summary>Extends a field list with schema defaults up to <paramref name="wordCount"/> words.</summary>
        private void PadFields(int list, int wordCount)
        {
            var words = Words(list);
            var slots = list == 1 ? Layout.Fields1 : Layout.Fields2;
            int skip = list == 2 ? Fields2Skip : 0;
            foreach (var s in slots)
                for (int i = 0; i < s.Words; i++)
                {
                    int at = skip + s.Word + i;
                    if (at >= words.Count && at < wordCount) words.Add(Schema.DefaultWord(s.Prop, s.Type, i));
                }
        }

        public int GetInt(string name) => (int)GetWords(name)[0];
        public float GetFloat(string name) => BitConverter.UInt32BitsToSingle(GetWords(name)[0]);
        public bool GetBool(string name) => GetWords(name)[0] != 0;

        public float[] GetVector(string name)
        {
            var w = GetWords(name);
            var v = new float[w.Length];
            for (int i = 0; i < w.Length; i++) v[i] = BitConverter.UInt32BitsToSingle(w[i]);
            return v;
        }

        public void SetInt(string name, int value) => SetWords(name, [(uint)value]);
        public void SetFloat(string name, float value) => SetWords(name, [BitConverter.SingleToUInt32Bits(value)]);
        public void SetBool(string name, bool value) => SetWords(name, [value ? 1u : 0u]);

        public void SetVector(string name, float[] value)
        {
            var w = new uint[value.Length];
            for (int i = 0; i < value.Length; i++) w[i] = BitConverter.SingleToUInt32Bits(value[i]);
            SetWords(name, w);
        }

        // ---------------------------------------------------------------- properties

        private List<FxrProperty> Props(int list) => list == 1 ? Action.Properties1 : Action.Properties2;

        private bool PropertyIndex(string name, out int list, out int index)
        {
            list = 0;
            index = -1;
            if (Layout == null) return false;
            index = Layout.Properties1.IndexOf(name);
            if (index >= 0) { list = 1; return true; }
            index = Layout.Properties2.IndexOf(name);
            if (index >= 0) { list = 2; return true; }
            return false;
        }

        /// <summary>The stored property, or null when the list ends before it (the schema default applies).</summary>
        public FxrProperty GetProperty(string name)
        {
            if (!PropertyIndex(name, out var list, out var index))
                throw new KeyNotFoundException($"'{name}' is not a property of {Definition?.Name ?? Action.Type.ToString()} in {Game}.");
            var props = Props(list);
            return index < props.Count ? props[index] : null;
        }

        public void SetProperty(string name, FxrProperty value)
        {
            if (!PropertyIndex(name, out var list, out var index))
                throw new KeyNotFoundException($"'{name}' is not a property of {Definition?.Name ?? Action.Type.ToString()} in {Game}.");
            var props = Props(list);
            var names = list == 1 ? Layout.Properties1 : Layout.Properties2;
            while (props.Count <= index) props.Add(DefaultProperty(names[props.Count]));
            props[index] = value;
        }

        /// <summary>A Constant (or Zero/One) property holding the schema default.</summary>
        public FxrProperty DefaultProperty(string name)
        {
            var def = Describe(name);
            int components = Math.Clamp(def?.Components ?? 1, 1, 4);
            var words = new uint[components];
            for (int i = 0; i < components; i++) words[i] = Schema.DefaultWord(def, FxrFieldType.Float, i);
            return ConstantProperty(words, Game);
        }

        /// <summary>Builds a constant property the way the game files store one (Zero/One when it can).</summary>
        public static FxrProperty ConstantProperty(uint[] words, FxrGame game)
        {
            uint zero = 0, one = BitConverter.SingleToUInt32Bits(1f);
            bool allZero = Array.TrueForAll(words, w => w == zero);
            bool allOne = Array.TrueForAll(words, w => w == one);
            var prop = new FxrProperty
            {
                ValueType = (FxrValueType)(words.Length - 1),
                Function = allZero ? FxrPropertyFunction.Zero : allOne ? FxrPropertyFunction.One : FxrPropertyFunction.Constant,
            };
            if (prop.Function == FxrPropertyFunction.Constant) prop.Fields.AddRange(words);
            // DS3/SDT/ER pack typeEnumB; NR stores 0 and new AC6 files do too
            prop.TypeEnumB = game is FxrGame.DS3 or FxrGame.SDT or FxrGame.ER ? prop.PackTypeEnumB() : 0;
            return prop;
        }

        // ---------------------------------------------------------------- converted values (fxr-main's display units)

        /// <summary>fxr-main's display conversions for a value in this game (empty for most values).</summary>
        public IEnumerable<FxrConversion> ConversionsOf(string name) => FxrConversions.For(Definition?.Name, name, Game);

        /// <summary>A field in display units (fxr-main's values: seconds, degrees, mirrored X, ...).</summary>
        public double[] GetValue(string name)
        {
            FindField(name, out var slot, out _);
            var words = GetWords(name);
            var values = new double[words.Length];
            for (int i = 0; i < words.Length; i++)
                values[i] = slot.Type switch
                {
                    FxrFieldType.Int => (int)words[i],
                    FxrFieldType.Bool => words[i] != 0 ? 1 : 0,
                    _ => BitConverter.UInt32BitsToSingle(words[i]),
                };
            FxrConversions.ToDisplay(ConversionsOf(name), values);
            return values;
        }

        /// <summary>Sets a field from display units. Int and bool fields are rounded.</summary>
        public void SetValue(string name, double[] display)
        {
            if (!FindField(name, out var slot, out _))
                throw new KeyNotFoundException($"'{name}' is not a field of {Definition?.Name ?? Action.Type.ToString()} in {Game}.");
            var values = (double[])display.Clone();
            FxrConversions.FromDisplay(ConversionsOf(name), values);
            var words = new uint[values.Length];
            for (int i = 0; i < values.Length; i++)
                words[i] = slot.Type switch
                {
                    FxrFieldType.Int => (uint)(int)Math.Round(values[i]),
                    FxrFieldType.Bool => Math.Round(values[i]) != 0 ? 1u : 0u,
                    _ => BitConverter.SingleToUInt32Bits((float)values[i]),
                };
            SetWords(name, words);
        }

        /// <summary>A property decoded as a curve in display units (the schema default when not stored).</summary>
        public FxrCurve GetCurve(string name)
        {
            var curve = DecodeCurve(GetProperty(name) ?? DefaultProperty(name));
            var conversions = ConversionsOf(name);
            for (int i = 0; i < curve.Components; i++)
            {
                double f = FxrConversions.CurveFactor(conversions, i);
                if (f != 1) curve.ScaleComponent(i, f);
            }
            return curve;
        }

        /// <summary>
        /// Writes a curve given in display units back into the property (modifiers are kept). Values equal
        /// to what <see cref="GetCurve"/> returned keep their original raw words, so unchanged parts of a
        /// converted curve do not drift by float rounding.
        /// </summary>
        public void SetCurve(string name, FxrCurve displayCurve, bool? recomputeBounds = null)
        {
            var prop = GetProperty(name);
            if (prop == null)
            {
                prop = DefaultProperty(name);
                SetProperty(name, prop);
            }
            var conversions = ConversionsOf(name);
            var raw = Clone(displayCurve);
            var oldRaw = DecodeCurve(prop);
            var oldDisplay = GetCurve(name);
            for (int i = 0; i < raw.Components; i++)
            {
                double f = FxrConversions.CurveFactor(conversions, i);
                if (f == 1) continue;
                raw.ScaleComponent(i, 1 / f);
                KeepUnchanged(raw, displayCurve, oldRaw, oldDisplay, i);
            }

            bool packs = Game is FxrGame.DS3 or FxrGame.SDT or FxrGame.ER || (Game == FxrGame.AC6 && prop.TypeEnumB != 0);
            raw.ComponentBoundsFromLast = ComponentBoundsFromLast;
            // Automatic: keep the stored bounds while the curve is unchanged (vanilla Bezier bounds cannot be
            // rebuilt), recompute them once it changes
            raw.EncodeInto(prop, recomputeBounds ?? !raw.SameShape(oldRaw));
            prop.TypeEnumB = packs ? prop.PackTypeEnumB() : 0;
        }

        // Copies old raw values wherever the new display value equals the old display value
        private static void KeepUnchanged(FxrCurve raw, FxrCurve display, FxrCurve oldRaw, FxrCurve oldDisplay, int i)
        {
            if (raw.Function != oldRaw.Function || raw.ValueType != oldRaw.ValueType) return;
            if (raw.Constant != null && oldRaw.Constant != null && display.Constant[i] == oldDisplay.Constant[i])
                raw.Constant[i] = oldRaw.Constant[i];
            if (raw.Min != null && oldRaw.Min != null && display.Min[i] == oldDisplay.Min[i]) raw.Min[i] = oldRaw.Min[i];
            if (raw.Max != null && oldRaw.Max != null && display.Max[i] == oldDisplay.Max[i]) raw.Max[i] = oldRaw.Max[i];

            void Keys(List<FxrKeyframe> r, List<FxrKeyframe> d, List<FxrKeyframe> or, List<FxrKeyframe> od, int c)
            {
                if (r.Count != or.Count) return;
                for (int k = 0; k < r.Count; k++)
                {
                    if (d[k].Value[c] == od[k].Value[c]) r[k].Value[c] = or[k].Value[c];
                    if (r[k].In != null && or[k].In != null && d[k].In[c] == od[k].In[c]) r[k].In[c] = or[k].In[c];
                    if (r[k].Out != null && or[k].Out != null && d[k].Out[c] == od[k].Out[c]) r[k].Out[c] = or[k].Out[c];
                }
            }

            if (raw.Function == FxrPropertyFunction.ComponentHermite)
                Keys(raw.ComponentKeyframes[i], display.ComponentKeyframes[i], oldRaw.ComponentKeyframes[i], oldDisplay.ComponentKeyframes[i], 0);
            else
                Keys(raw.Keyframes, display.Keyframes, oldRaw.Keyframes, oldDisplay.Keyframes, i);
        }

        public static FxrCurve Clone(FxrCurve c)
        {
            static FxrKeyframe K(FxrKeyframe k) => new()
            {
                Position = k.Position,
                Value = (float[])k.Value.Clone(),
                In = (float[])k.In?.Clone(),
                Out = (float[])k.Out?.Clone(),
            };
            return new FxrCurve
            {
                Function = c.Function,
                ValueType = c.ValueType,
                Loop = c.Loop,
                Duration = c.Duration,
                Constant = (float[])c.Constant?.Clone(),
                Min = (float[])c.Min?.Clone(),
                Max = (float[])c.Max?.Clone(),
                Keyframes = c.Keyframes.ConvertAll(K),
                ComponentKeyframes = c.ComponentKeyframes == null ? null : Array.ConvertAll(c.ComponentKeyframes, l => l.ConvertAll(K)),
            };
        }

        // ---------------------------------------------------------------- section10s

        /// <summary>The stored int list, or null when the action has fewer section10 lists.</summary>
        public List<int> GetSection10(string name)
        {
            int index = Layout?.Section10s.IndexOf(name) ?? -1;
            if (index < 0) throw new KeyNotFoundException($"'{name}' is not a section10 list of {Definition?.Name ?? Action.Type.ToString()} in {Game}.");
            return index < Action.Section10s.Count ? Action.Section10s[index] : null;
        }

        public void SetSection10(string name, List<int> values)
        {
            int index = Layout?.Section10s.IndexOf(name) ?? -1;
            if (index < 0) throw new KeyNotFoundException($"'{name}' is not a section10 list of {Definition?.Name ?? Action.Type.ToString()} in {Game}.");
            while (Action.Section10s.Count <= index) Action.Section10s.Add(new List<int>());
            Action.Section10s[index] = values;
        }
    }
}
