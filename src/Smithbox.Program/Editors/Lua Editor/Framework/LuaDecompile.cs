using LuaDecompilerCore;
using LuaDecompilerCore.LanguageDecompilers;
using LuaReader = LuaDecompilerCore.Utilities.BinaryReaderEx;

namespace StudioCore.Editors.LuaEditor;

/// <summary>
/// A binder's Lua entry as text: Lua 5.0 bytecode is decompiled (DSLuaDecompiler, vendored in src/DSLuaDecompiler)
/// and given readable local names; source text is read as Shift-JIS. LuaAiSurvey checks the decompiled text of every
/// vanilla ER script compiles back to the original bytecode (637 of 638; 451000_battle differs in one function).
/// </summary>
public static class LuaDecompile
{
    /// <summary>Whether the entry is compiled Lua (starts with ESC 'Lua'), not source text</summary>
    public static bool IsBytecode(ReadOnlySpan<byte> data) =>
        data.Length >= 4 && data[0] == 0x1B && data[1] == (byte)'L' && data[2] == (byte)'u' && data[3] == (byte)'a';

    /// <summary>The entry's source; throws if bytecode can't be decompiled</summary>
    public static string ToText(ReadOnlyMemory<byte> data, bool readableNames)
    {
        if (!IsBytecode(data.Span))
            return LuaLexer.ShiftJis.GetString(data.Span);

        var lua = new LuaFile(new LuaReader(false, new MemoryStream(data.ToArray())));
        if (lua.Version != LuaFile.LuaVersion.Lua50)
            throw new NotSupportedException(LOC.Get("LUA_Data_Not_Lua50", lua.Version));

        var main = new LuaDecompilerCore.IR.Function(lua.MainFunction.FunctionId);
        var result = new LuaDecompiler(new DecompilationOptions { CatchPassExceptions = true })
            .DecompileLuaFunction(new Lua50Decompiler(), main, lua.MainFunction);

        if (result.DecompiledSource == null)
            throw new InvalidOperationException(result.ErrorMessage ?? LOC.Get("LUA_Data_Decompile_Failed"));

        var text = result.DecompiledSource.Replace("\r\n", "\n");
        return readableNames ? LuaNames.Apply(text) : text;
    }

    /// <summary>
    /// The globals compiled Lua 5.0 assigns, in the order its .luagnl lists them: SETGLOBAL names, the main function
    /// first, then nested functions depth-first (467 of 469 vanilla ER binders; the other two ship lists with a few
    /// of these left out).
    /// </summary>
    public static List<string> BytecodeGlobals(ReadOnlyMemory<byte> data)
    {
        var lua = new LuaFile(new LuaReader(false, new MemoryStream(data.ToArray())));
        var names = new List<string>();
        Walk(lua.MainFunction, names);
        return names.Distinct().ToList();

        // Lua 5.0: opcode = low 6 bits, Bx = bits 6..23; SETGLOBAL = 7, Bx = the name's constant index
        static void Walk(LuaFile.Function f, List<string> names)
        {
            for (int i = 0; i + 4 <= f.Bytecode.Length; i += 4)
            {
                var ins = BitConverter.ToUInt32(f.Bytecode, i);
                var bx = (int)((ins >> 6) & 0x3FFFF);
                if ((ins & 0x3F) == 7 && bx < f.Constants.Length && f.Constants[bx].StringValue is { } s)
                    names.Add(s);
            }
            foreach (var c in f.ChildFunctions)
                Walk(c, names);
        }
    }
}
