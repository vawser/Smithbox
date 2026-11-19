using System.Diagnostics.CodeAnalysis;
using LuaDecompilerCore.IR;

namespace LuaDecompilerCore;

/// <summary>
/// Interface to implement language specific support for a decompiler.
/// </summary>
public interface ILanguageDecompiler
{
    /// <summary>
    /// The mode for a Lua OpCode that specifies how it is decoded
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    public enum OpMode
    {
        IABC,
        IABx,
        IAsBx,
        IAx,
    }

    /// <summary>
    /// Properties for a Lua OpCode
    /// </summary>
    public class OpProperties
    {
        public string OpName;
        public OpMode OpMode;

        public OpProperties (string name)
        {
            OpName = name;
        }

        public OpProperties(string name, OpMode mode)
        {
            OpName = name;
            OpMode = mode;
        }
    }

    /// <summary>
    /// Initializes an IR function from a bytecode function and creates all the child closures
    /// </summary>
    /// <param name="function">The lua function to process</param>
    /// <param name="irFunction">The IR function to initialize</param>
    public void InitializeFunction(LuaFile.Function function, Function irFunction);
    
    /// <summary>
    /// Disassembles a function into Lua opcodes, which may be useful for debugging purposes.
    /// </summary>
    /// <param name="function">The function to disassemble</param>
    /// <returns>The disassembled function in text form, or null if disassembly is not supported</returns>
    public string? Disassemble(LuaFile.Function function);

    /// <summary>
    /// Decode the bytecode function and generate IR into the irFunction
    /// </summary>
    /// <param name="function">The lua function to process</param>
    /// <param name="irFunction">The IR function to insert IR instructions into</param>
    public void GenerateIr(LuaFile.Function function, Function irFunction);

    /// <summary>
    /// Add all the decompiler passes to the pass manager to decompile this language
    /// </summary>
    /// <param name="passManager"></param>
    public void AddDecompilePasses(PassManager passManager);
}