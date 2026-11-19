using LuaDecompilerCore.IR;

namespace LuaDecompilerCore.Passes;

/// <summary>
/// Interface for a pass that operates on the IR for a function
/// </summary>
public interface IPass
{
    public bool MutatesCfg => false;
    
    public bool RunOnFunction(DecompilationContext decompilationContext, FunctionContext functionContext, Function f);
}