#nullable enable
using System.Collections.Generic;

namespace LuaDecompilerCore;

/// <summary>
/// Options to control decompilation. Largely for debugging purposes.
/// </summary>
public class DecompilationOptions
{
    /// <summary>
    /// Extra validation passes and checks will be run to help catch decompilation issues
    /// </summary>
    public bool ExtraValidation = false;
    
    /// <summary>
    /// Catch any exceptions that may occur in the decompilation in the pass manager to ensure
    /// a result can still be returned
    /// </summary>
    public bool CatchPassExceptions = true;
    
    /// <summary>
    /// Function Ids that are included in the decompilation. For closures, the parent function must also be included for
    /// the closure to be included in the decompilation. Null means all function Ids are compiled.
    /// </summary>
    public IReadOnlySet<int>? IncludedFunctionIds = null;

    /// <summary>
    /// Function Ids that are explicitly excluded from the decompilation. Excluding a function will exclude any closures
    /// that are contained in the function.
    /// </summary>
    public IReadOnlySet<int> ExcludedFunctionIds = new HashSet<int>();

    /// <summary>
    /// Debug option to dump intermediate representation to the output after each named pass. Passing "all" as a pass
    /// name will dump the IR for all the passes.
    /// </summary>
    public IReadOnlySet<string> DumpIrPasses = new HashSet<string>();
    
    /// <summary>
    /// Debug option to dump a DOT file representing the control flow graph of each function after each named pass.
    /// Passing "all" as a pass name will dump the CFG for all the passes.
    /// </summary>
    public IReadOnlySet<string> DumpDotGraphPasses = new HashSet<string>();
    
    /// <summary>
    /// Add comments to the decompiled output with various debugging info.
    /// </summary>
    public bool OutputDebugComments = false;

    /// <summary>
    /// Ignore debug info in the Lua file if it exists. Useful for testing local variable detection.
    /// </summary>
    public bool IgnoreDebugInfo = false;
}