using System.Text.RegularExpressions;

/// <summary>
/// A pixel shader read from `dxc -dumpbin` text: its resources, cbuffer layouts and signatures, and for every
/// output component the leaves it depends on (texture channels, cbuffer fields, interpolants).
/// Data dependence only, found by walking the SSA operands back from each storeOutput, so a select's
/// condition counts as well as its values.
/// </summary>
class Dxil
{
    public record Resource(string Name, string Type, string Id, string Bind);
    public record Field(string Name, int Offset, int ArrayLength);

    public List<Resource> Resources = new();
    public Dictionary<string, List<Field>> Cbuffers = new();
    public List<string> Inputs = new();   // by signature element id
    public List<string> Outputs = new();  // by signature element id, e.g. SV_Target1

    /// <summary>Output component (RT1.x) to its leaves</summary>
    public SortedDictionary<string, SortedSet<string>> OutputDeps = new(StringComparer.Ordinal);

    /// <summary>Per sample call: the texture, which channels are used, and the leaves of its coordinates</summary>
    public List<Sample> Samples = new();

    public SortedSet<string> DiscardDeps = new(StringComparer.Ordinal);
    public bool HasDiscard;

    public class Sample
    {
        public string Texture = "";
        public string Op = "";
        public SortedSet<string> Coordinates = new(StringComparer.Ordinal);
    }

    record Instr(string Text, List<int> Operands);

    static readonly Regex Line = new(@"^\s+%(\d+) = (.*)$", RegexOptions.Compiled);
    static readonly Regex Operand = new(@"(?<!label )%(\d+)\b", RegexOptions.Compiled);
    static readonly Regex PhiPair = new(@"\[\s*([^,\]]+),\s*%\d+\s*\]", RegexOptions.Compiled);
    static readonly Regex Binding = new(@"^;\s+(\S+)\s+(cbuffer|texture|sampler|UAV)\s+\S+\s+\S+\s+(\S+)\s+(\S+)\s+\d+", RegexOptions.Compiled);
    // A long name runs into the comment: "float M_..._snp_0_float_1_0;; Offset:  144"
    static readonly Regex FieldLine = new(@"^;\s+(?:column_major |row_major )?[\w.]+\s+(\w+)(?:\[(\d+)\])?;\s*; Offset:\s+(\d+)", RegexOptions.Compiled);
    static readonly Regex SigRow = new(@"^;\s+(\w+)\s+(\d+)\s+[xyzw]+\s+", RegexOptions.Compiled);
    /// <summary>A row of the DXIL table: name, index, then the interpolation mode or nothing</summary>
    static readonly Regex DxilSigRow = new(@"^;\s+([A-Za-z_]\w*)\s+(\d+)(?:\s+[a-z]+)?\s*$", RegexOptions.Compiled);
    static readonly Regex Store = new(@"@dx\.op\.storeOutput\.\w+\(i32 5, i32 (\d+), i32 (\d+), i8 (\d+), \w+ ([^)]+)\)", RegexOptions.Compiled);
    static readonly Regex Discard = new(@"@dx\.op\.discard\(i32 82, i1 ([^)]+)\)", RegexOptions.Compiled);
    static readonly Regex CreateHandle = new(@"@dx\.op\.createHandle\(i32 57, i8 (\d+), i32 (\d+),", RegexOptions.Compiled);
    static readonly Regex CreateFromBinding = new(@"@dx\.op\.createHandleFromBinding\(i32 217, %dx\.types\.ResBind \{ i32 (\d+), i32 \d+, i32 (\d+), i8 (\d+) \}", RegexOptions.Compiled);
    static readonly Regex Annotate = new(@"@dx\.op\.annotateHandle\(i32 216, %dx\.types\.Handle %(\d+)", RegexOptions.Compiled);
    static readonly Regex ExtractValue = new(@"^extractvalue %dx\.types\.(ResRet|CBufRet)\.\w+ %(\d+), (\d+)", RegexOptions.Compiled);
    static readonly Regex SampleCall = new(@"@dx\.op\.(sample\w*|textureLoad|textureGather\w*)\.\w+\(i32 \d+, %dx\.types\.Handle %(\d+)", RegexOptions.Compiled);
    static readonly Regex CBufLoad = new(@"@dx\.op\.cbufferLoadLegacy\.\w+\(i32 59, %dx\.types\.Handle %(\d+), i32 (%?\d+)\)", RegexOptions.Compiled);
    static readonly Regex LoadInput = new(@"@dx\.op\.loadInput\.\w+\(i32 4, i32 (\d+), i32 [^,]+, i8 (\d+)", RegexOptions.Compiled);

    readonly Dictionary<int, Instr> _ssa = new();
    readonly Dictionary<int, SortedSet<string>> _memo = new();
    static readonly string[] Xyzw = ["x", "y", "z", "w"];
    static readonly string[] Rgba = ["r", "g", "b", "a"];

    public static Dxil Parse(string[] lines)
    {
        var d = new Dxil();
        d.ReadHeader(lines);

        var stores = new List<(string output, string value)>();
        var discards = new List<string>();
        foreach (var raw in lines)
        {
            var m = Line.Match(raw);
            if (m.Success)
            {
                var text = m.Groups[2].Value;
                List<int> ops;
                if (text.StartsWith("phi "))
                    ops = PhiPair.Matches(text).Select(p => p.Groups[1].Value.Trim()).Where(v => v.StartsWith('%'))
                        .Select(v => int.Parse(v[1..])).ToList();
                else
                    ops = Operand.Matches(text).Select(o => int.Parse(o.Groups[1].Value)).ToList();
                d._ssa[int.Parse(m.Groups[1].Value)] = new Instr(text, ops);
                continue;
            }

            var s = Store.Match(raw);
            if (s.Success)
            {
                var sig = int.Parse(s.Groups[1].Value);
                var name = sig < d.Outputs.Count ? d.Outputs[sig] : $"OUT{sig}";
                stores.Add(($"{name}.{Xyzw[int.Parse(s.Groups[3].Value)]}", s.Groups[4].Value.Trim()));
                continue;
            }

            var dc = Discard.Match(raw);
            if (dc.Success) { d.HasDiscard = true; discards.Add(dc.Groups[1].Value.Trim()); }
        }

        foreach (var (output, value) in stores)
        {
            // The last store is the one that counts; the first ones clear the target
            d._storeValues[output] = value;
            if (!d.OutputDeps.TryGetValue(output, out var set)) d.OutputDeps[output] = set = new(StringComparer.Ordinal);
            if (value.StartsWith('%')) set.UnionWith(d.Leaves(int.Parse(value[1..])));
        }

        foreach (var c in discards.Where(c => c.StartsWith('%'))) d.DiscardDeps.UnionWith(d.Leaves(int.Parse(c[1..])));

        foreach (var (id, instr) in d._ssa)
        {
            var sc = SampleCall.Match(instr.Text);
            if (!sc.Success) continue;
            var sample = new Sample { Texture = d.HandleName(int.Parse(sc.Groups[2].Value)), Op = sc.Groups[1].Value };
            // The texture and sampler handles are the first two operands; the rest are coordinates, offsets and the like
            foreach (var op in instr.Operands.Skip(2)) sample.Coordinates.UnionWith(d.Leaves(op));
            d.Samples.Add(sample);
        }

        return d;
    }

    void ReadHeader(string[] lines)
    {
        string section = "", cbuffer = null;
        var inputsDone = false;
        var outputsDone = false;
        // The disassembly prints each signature twice: first in register order (with masks), then as DXIL numbers its
        // elements, which is the order loadInput and storeOutput go by. The second wins when it is there
        var inputTables = 0;
        var outputTables = 0;
        foreach (var raw in lines)
        {
            if (!raw.StartsWith(';')) { if (raw.StartsWith("target") || raw.StartsWith("define")) break; continue; }
            var t = raw.TrimStart(';').Trim();
            if (t == "Input signature:")
            {
                if (++inputTables == 2) { Inputs.Clear(); inputsDone = false; }
                section = inputsDone ? "" : "in";
                continue;
            }
            if (t == "Output signature:")
            {
                if (++outputTables == 2) { Outputs.Clear(); outputsDone = false; }
                section = outputsDone ? "" : "out";
                continue;
            }
            if (t == "Buffer Definitions:") { section = "cb"; continue; }
            if (t == "Resource Bindings:") { section = "res"; continue; }
            if (t.StartsWith("ViewId state") || t.StartsWith("Pipeline Runtime")) { section = ""; continue; }

            switch (section)
            {
                case "in":
                case "out":
                    var row = SigRow.Match(raw);
                    if (!row.Success) row = DxilSigRow.Match(raw);
                    if (row.Success && row.Groups[1].Value != "Name")
                        (section == "in" ? Inputs : Outputs).Add(row.Groups[1].Value + row.Groups[2].Value);
                    else if (t.Length == 0 && (section == "in" ? Inputs : Outputs).Count > 0)
                    {
                        if (section == "in") inputsDone = true; else outputsDone = true;
                        section = "";
                    }
                    break;
                case "cb":
                    if (t.StartsWith("cbuffer ")) { cbuffer = t[8..].Trim(); Cbuffers[cbuffer] = new(); break; }
                    var f = FieldLine.Match(raw);
                    if (f.Success && cbuffer != null)
                        Cbuffers[cbuffer].Add(new Field(f.Groups[1].Value, int.Parse(f.Groups[3].Value),
                            f.Groups[2].Success ? int.Parse(f.Groups[2].Value) : 0));
                    break;
                case "res":
                    var b = Binding.Match(raw);
                    if (b.Success && b.Groups[1].Value != "Name") Resources.Add(new Resource(b.Groups[1].Value, b.Groups[2].Value, b.Groups[3].Value, b.Groups[4].Value));
                    break;
            }
        }

        // The output names are SV_Target0..n; call them RT0..n
        Outputs = Outputs.Select(o => o.StartsWith("SV_Target") ? "RT" + o["SV_Target".Length..] : o).ToList();
    }

    /// <summary>What a handle refers to, through createHandle (by range ID) or createHandleFromBinding (by register)</summary>
    string HandleName(int id)
    {
        if (!_ssa.TryGetValue(id, out var instr)) return $"%{id}";
        var a = Annotate.Match(instr.Text);
        if (a.Success) return HandleName(int.Parse(a.Groups[1].Value));

        var c = CreateHandle.Match(instr.Text);
        if (c.Success)
        {
            var prefix = c.Groups[1].Value switch { "0" => "T", "1" => "U", "2" => "CB", _ => "S" };
            var rid = prefix + c.Groups[2].Value;
            return Resources.FirstOrDefault(r => r.Id == rid)?.Name ?? rid;
        }

        var fb = CreateFromBinding.Match(instr.Text);
        if (fb.Success)
        {
            var prefix = fb.Groups[3].Value switch { "0" => "t", "1" => "u", "2" => "cb", _ => "s" };
            var reg = prefix + fb.Groups[1].Value;
            var space = fb.Groups[2].Value;
            return Resources.FirstOrDefault(r => r.Bind == reg || r.Bind == $"{reg},space{space}")?.Name ?? reg;
        }

        return $"%{id}";
    }

    string CbufferField(string cbuffer, string row, int component)
    {
        if (!Cbuffers.TryGetValue(cbuffer, out var fields) || fields.Count == 0) return $"{cbuffer}.?";
        if (row.StartsWith('%'))
        {
            // A computed row: an array indexed at run time, so only which cbuffer is known
            return $"{cbuffer}[dyn]";
        }

        var offset = int.Parse(row) * 16 + component * 4;
        var field = fields.Where(f => f.Offset <= offset).OrderByDescending(f => f.Offset).FirstOrDefault();
        if (field == null) return $"{cbuffer}.?";
        if (field.ArrayLength > 0) return $"{field.Name}[{(offset - field.Offset) / 16}].{Xyzw[(offset % 16) / 4]}";
        var within = (offset - field.Offset) / 4;
        return within == 0 && (offset - field.Offset) % 16 == 0 ? field.Name : $"{field.Name}.{Xyzw[Math.Min(3, within)]}";
    }

    readonly Dictionary<string, string> _storeValues = new();

    // ---- Structure, for the recipe generator ------------------------------------------------------------------

    /// <summary>An operand: an SSA value, or a constant</summary>
    public readonly record struct Arg(int Id, double Value)
    {
        public bool IsConst => Id < 0;
    }

    public enum Op { Leaf, Add, Sub, Mul, Div, Mad, Saturate, Min, Max, Dot, Other }

    /// <summary>One value: what it does, its operands, and for a leaf its name (tex:..., cb:..., in:...)</summary>
    public record Node(int Id, Op Op, Arg[] Args, string Leaf);

    static readonly Regex BinaryArgs = new(@"^(fadd|fsub|fmul|fdiv) (?:fast )?\w+ ([^,]+), (.+)$", RegexOptions.Compiled);

    public Arg ArgOf(string token)
    {
        token = token.Trim();
        if (token.StartsWith('%')) return new Arg(int.Parse(token[1..]), 0);
        if (token.StartsWith("0x") && token.Length == 18) return new Arg(-1, BitConverter.Int64BitsToDouble(Convert.ToInt64(token, 16)));
        return double.TryParse(token, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var d)
            ? new Arg(-1, d) : new Arg(-1, double.NaN);
    }

    /// <summary>The expression of any value, as <see cref="Expression"/> prints an output's</summary>
    public string ExpressionOf(int id, int maxDepth = 14) => Expr(id, maxDepth);

    /// <summary>The SSA value an output component is written from, or null</summary>
    public int? StoreId(string output) => _storeValues.TryGetValue(output, out var v) && v.StartsWith('%') ? int.Parse(v[1..]) : null;

    public Node NodeOf(int id)
    {
        if (!_ssa.TryGetValue(id, out var instr)) return new Node(id, Op.Other, [], "");
        var t = instr.Text;
        var leaves = Leaves(id);
        if (leaves.Count == 1 && (ExtractValue.IsMatch(t) || LoadInput.IsMatch(t)))
        {
            var leaf = leaves.First();
            return new Node(id, Op.Leaf, [], leaf.StartsWith("cb:") || leaf.StartsWith("in:") ? leaf : "tex:" + leaf);
        }

        var b = BinaryArgs.Match(t);
        if (b.Success)
        {
            var op = b.Groups[1].Value switch { "fadd" => Op.Add, "fsub" => Op.Sub, "fmul" => Op.Mul, _ => Op.Div };
            return new Node(id, op, [ArgOf(b.Groups[2].Value), ArgOf(b.Groups[3].Value)], "");
        }
        var fm = DxTertiary.Match(t);
        if (fm.Success) return new Node(id, Op.Mad, [ArgOf(fm.Groups[1].Value), ArgOf(fm.Groups[2].Value), ArgOf(fm.Groups[3].Value)], "");
        var u = DxUnary.Match(t);
        if (u.Success && u.Groups[1].Value == "7") return new Node(id, Op.Saturate, [ArgOf(u.Groups[2].Value)], "");
        var bi = DxBinary.Match(t);
        if (bi.Success && bi.Groups[1].Value is "35" or "36")
            return new Node(id, bi.Groups[1].Value == "35" ? Op.Max : Op.Min, [ArgOf(bi.Groups[2].Value), ArgOf(bi.Groups[3].Value)], "");
        // dot2/3/4: the a operands, then the b operands. The comment after the call isn't part of it
        var dot = DotCall.Match(t);
        if (dot.Success)
            return new Node(id, Op.Dot, dot.Groups[1].Value.Split(',').Select(a => ArgOf(a.Trim().Split(' ').Last())).ToArray(), "");
        return new Node(id, Op.Other, instr.Operands.Select(o => new Arg(o, 0)).ToArray(), "");
    }

    static readonly Regex DotCall = new(@"@dx\.op\.dot[234]\.\w+\(i32 \d+, ([^)]*)\)", RegexOptions.Compiled);

    /// <summary>The texture, cbuffer and input leaves a value depends on, as NodeOf names them</summary>
    public IEnumerable<string> LeafNames(int id) => Leaves(id).Select(l => l.StartsWith("cb:") || l.StartsWith("in:") ? l : "tex:" + l);

    /// <summary>
    /// The texture channels an output saturates on their own or with a cbuffer field added, as the metallic of
    /// M[AMSN] is sat(MetallicMap.r + snp_0_float_0): (texture, channel, field or null)
    /// </summary>
    public List<(string texture, string channel, string field)> SaturatedInputs(string output)
    {
        var result = new List<(string, string, string)>();
        if (!_storeValues.TryGetValue(output, out var v) || !v.StartsWith('%')) return result;

        var seen = new HashSet<int>();
        var stack = new Stack<int>();
        stack.Push(int.Parse(v[1..]));
        while (stack.Count > 0)
        {
            var id = stack.Pop();
            if (!seen.Add(id) || !_ssa.TryGetValue(id, out var instr)) continue;
            foreach (var op in instr.Operands) stack.Push(op);

            var u = DxUnary.Match(instr.Text);
            if (!u.Success || u.Groups[1].Value != "7" || !u.Groups[2].Value.Trim().StartsWith('%')) continue;
            var inner = int.Parse(u.Groups[2].Value.Trim()[1..]);

            if (TextureLeaf(inner) is { } alone) { result.Add((alone.texture, alone.channel, null)); continue; }
            if (!_ssa.TryGetValue(inner, out var add) || !add.Text.StartsWith("fadd") || add.Operands.Count != 2) continue;
            var a = TextureLeaf(add.Operands[0]) ?? TextureLeaf(add.Operands[1]);
            var other = TextureLeaf(add.Operands[0]) != null ? add.Operands[1] : add.Operands[0];
            var field = Leaves(other) is { Count: 1 } l && l.First().StartsWith("cb:") ? l.First()[3..] : null;
            if (a != null && field != null) result.Add((a.Value.texture, a.Value.channel, field));
        }
        return result;
    }

    (string texture, string channel)? TextureLeaf(int id)
    {
        if (!_ssa.TryGetValue(id, out var instr) || !ExtractValue.IsMatch(instr.Text)) return null;
        var leaves = Leaves(id);
        if (leaves.Count != 1 || leaves.First().StartsWith("cb:") || leaves.First().StartsWith("in:")) return null;
        var leaf = leaves.First();
        var dot = leaf.LastIndexOf('.');
        return (leaf[..dot], leaf[(dot + 1)..]);
    }

    /// <summary>
    /// The expression an output component is computed by, as text, down to its leaves: texture channels,
    /// cbuffer fields and interpolants. Deeper than maxDepth it stops with "…".
    /// </summary>
    public string Expression(string output, int maxDepth = 14)
    {
        if (!_storeValues.TryGetValue(output, out var v)) return "(not written)";
        return v.StartsWith('%') ? Expr(int.Parse(v[1..]), maxDepth) : v;
    }

    static readonly Regex Binary = new(@"^(fadd|fsub|fmul|fdiv|add|sub|mul|and|or|xor|shl|lshr|fcmp \w+|icmp \w+) (?:fast )?\w+ ([^,]+), (.+)$", RegexOptions.Compiled);
    static readonly Regex DxUnary = new(@"@dx\.op\.unary\.\w+\(i32 (\d+), \w+ ([^)]+)\)", RegexOptions.Compiled);
    static readonly Regex DxBinary = new(@"@dx\.op\.binary\.\w+\(i32 (\d+), \w+ ([^,]+), \w+ ([^)]+)\)", RegexOptions.Compiled);
    static readonly Regex DxTertiary = new(@"@dx\.op\.tertiary\.\w+\(i32 46, \w+ ([^,]+), \w+ ([^,]+), \w+ ([^)]+)\)", RegexOptions.Compiled);
    static readonly Regex DxDot = new(@"@dx\.op\.dot([234])\.\w+\(i32 \d+, (.+)\)", RegexOptions.Compiled);
    static readonly Regex Select = new(@"^select i1 ([^,]+), \w+ ([^,]+), \w+ (.+)$", RegexOptions.Compiled);

    string Operand2(string token, int depth)
    {
        token = token.Trim();
        if (token.StartsWith('%')) return Expr(int.Parse(token[1..]), depth);
        if (token.StartsWith("0x") && token.Length == 18)
            return BitConverter.Int64BitsToDouble(Convert.ToInt64(token, 16)).ToString("0.####", System.Globalization.CultureInfo.InvariantCulture);
        return double.TryParse(token, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var d)
            ? d.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture) : token;
    }

    string Expr(int id, int depth)
    {
        var leaves = Leaves(id);
        if (!_ssa.TryGetValue(id, out var instr)) return $"%{id}";
        var t = instr.Text;
        // A leaf is its own name
        if (leaves.Count == 1 && (ExtractValue.IsMatch(t) || LoadInput.IsMatch(t))) return leaves.First();
        if (depth <= 0) return "…";
        var d = depth - 1;

        var b = Binary.Match(t);
        if (b.Success)
        {
            var op = b.Groups[1].Value switch { "fadd" or "add" => "+", "fsub" or "sub" => "-", "fmul" or "mul" => "*", "fdiv" => "/", var o => o };
            return $"({Operand2(b.Groups[2].Value, d)} {op} {Operand2(b.Groups[3].Value, d)})";
        }
        var u = DxUnary.Match(t);
        if (u.Success)
        {
            var name = u.Groups[1].Value switch { "6" => "abs", "7" => "sat", "21" => "exp2", "23" => "log2", "24" => "sqrt", "25" => "rsqrt", "26" => "round", "22" => "frc", var o => $"op{o}" };
            return $"{name}({Operand2(u.Groups[2].Value, d)})";
        }
        var bi = DxBinary.Match(t);
        if (bi.Success)
        {
            var name = bi.Groups[1].Value switch { "35" => "max", "36" => "min", var o => $"op{o}" };
            return $"{name}({Operand2(bi.Groups[2].Value, d)}, {Operand2(bi.Groups[3].Value, d)})";
        }
        var fm = DxTertiary.Match(t);
        if (fm.Success) return $"({Operand2(fm.Groups[1].Value, d)} * {Operand2(fm.Groups[2].Value, d)} + {Operand2(fm.Groups[3].Value, d)})";
        var dot = DxDot.Match(t);
        if (dot.Success)
        {
            var args = dot.Groups[2].Value.Split(',').Select(x => x.Trim().Split(' ').Last()).ToArray();
            var half = args.Length / 2;
            return $"dot({string.Join(", ", args.Take(half).Select(a => Operand2(a, d)))} | {string.Join(", ", args.Skip(half).Select(a => Operand2(a, d)))})";
        }
        var s = Select.Match(t);
        if (s.Success) return $"({Operand2(s.Groups[1].Value, d)} ? {Operand2(s.Groups[2].Value, d)} : {Operand2(s.Groups[3].Value, d)})";
        if (t.StartsWith("phi ")) return $"phi({string.Join(", ", instr.Operands.Select(o => Expr(o, d)))})";
        // Anything else: its leaves
        return $"{t.Split(' ')[0]}[{string.Join(" ", leaves)}]";
    }

    /// <summary>The leaves a value depends on</summary>
    SortedSet<string> Leaves(int id)
    {
        if (_memo.TryGetValue(id, out var done)) return done;
        var result = new SortedSet<string>(StringComparer.Ordinal);
        _memo[id] = result; // breaks loops through phis

        if (!_ssa.TryGetValue(id, out var instr)) return result;

        var ev = ExtractValue.Match(instr.Text);
        if (ev.Success && _ssa.TryGetValue(int.Parse(ev.Groups[2].Value), out var src))
        {
            var component = int.Parse(ev.Groups[3].Value);
            var sc = SampleCall.Match(src.Text);
            if (ev.Groups[1].Value == "ResRet" && sc.Success)
            {
                result.Add($"{HandleName(int.Parse(sc.Groups[2].Value))}.{(component < 4 ? Rgba[component] : "status")}");
                return result;
            }

            var cb = CBufLoad.Match(src.Text);
            if (ev.Groups[1].Value == "CBufRet" && cb.Success)
            {
                result.Add("cb:" + CbufferField(HandleName(int.Parse(cb.Groups[1].Value)), cb.Groups[2].Value, component));
                return result;
            }
        }

        var li = LoadInput.Match(instr.Text);
        if (li.Success)
        {
            var sig = int.Parse(li.Groups[1].Value);
            result.Add($"in:{(sig < Inputs.Count ? Inputs[sig] : $"IN{sig}")}.{Xyzw[int.Parse(li.Groups[2].Value)]}");
            return result;
        }

        foreach (var op in instr.Operands) result.UnionWith(Leaves(op));
        return result;
    }
}
