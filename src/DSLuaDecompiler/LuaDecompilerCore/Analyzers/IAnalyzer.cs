using System;
using LuaDecompilerCore.IR;

namespace LuaDecompilerCore.Analyzers;

/// <summary>
/// An analyzer does analysis on a function and provides the results of that analysis to functions that request it.
/// Analyses should not mutate the function at all, and can be invalidated by future passes which will trigger a
/// regeneration.
/// </summary>
public interface IAnalyzer : IDisposable
{
    public void Run(DecompilationContext decompilationContext, FunctionContext functionContext, Function function);
}