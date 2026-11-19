using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LuaDecompilerCore.Analyzers;
using LuaDecompilerCore.IR;
using LuaDecompilerCore.Passes;
using LuaDecompilerCore.Utilities;

namespace LuaDecompilerCore;

/// <summary>
/// Stores contextual information of functions in between passes
/// </summary>
public class FunctionContext : IDisposable
{
    private readonly DecompilationContext _context;
    private readonly Function _function;
    private readonly List<IAnalyzer> _analyzers = new List<IAnalyzer>(5);

    public FunctionContext(DecompilationContext context, Function function)
    {
        _context = context;
        _function = function;
    }
    
    /// <summary>
    /// Gets an analyzer analysis. If one already exists the cached analysis will be returned. Otherwise a new one
    /// will be created and ran.
    /// </summary>
    /// <typeparam name="T">The analyzer to run</typeparam>
    /// <returns>The result of the analyzer</returns>
    public T GetAnalysis<T>() where T : IAnalyzer, new()
    {
        foreach (var analyzer in _analyzers)
        {
            if (analyzer is T a)
                return a;
        }

        var newAnalyzer = new T();
        newAnalyzer.Run(_context, this, _function);
        _analyzers.Add(newAnalyzer);
        return newAnalyzer;
    }

    /// <summary>
    /// Invalidates a cached analysis. Should be used if modifications occur such that the analysis is no longer
    /// valid.
    /// </summary>
    /// <typeparam name="T">The analyzer to invalidate</typeparam>
    public void InvalidateAnalysis<T>()
    {
        for (var i = 0; i < _analyzers.Count; i++)
        {
            if (_analyzers[i] is not T) continue;
            _analyzers[i].Dispose();
            _analyzers.RemoveAt(i);
            return;
        }
    }

    public void Dispose()
    {
        foreach (var analyzer in _analyzers)
        {
            analyzer.Dispose();
        }
    }
}

/// <summary>
/// Pass manager to run all the passes on functions and allow debugging of passes.
/// </summary>
public class PassManager
{
    public record struct PassRegistration(string Name, IPass Pass);

    private readonly List<PassRegistration> _passes;
    private readonly List<Interval> _loopsUntilUnchanged;
    private int _loopsUntilUnchangedIndex;
    private readonly IReadOnlySet<string> _dumpIrPasses;
    private readonly bool _dumpAllIrPasses;
    private readonly IReadOnlySet<string> _dumpDotGraphPasses;
    private readonly bool _dumpAllDotGraphPasses;
    private readonly bool _dumpCfgMutatingDotGraphPasses;

    public PassManager(DecompilationOptions options)
    {
        _passes = new List<PassRegistration>();
        _loopsUntilUnchanged = new List<Interval>();
        _loopsUntilUnchangedIndex = -1;
        _dumpIrPasses = options.DumpIrPasses;
        _dumpAllIrPasses = _dumpIrPasses.Contains("all");
        _dumpDotGraphPasses = options.DumpDotGraphPasses;
        _dumpAllDotGraphPasses = _dumpDotGraphPasses.Contains("all");
        _dumpCfgMutatingDotGraphPasses = _dumpDotGraphPasses.Contains("cfg-mutated");
    }

    public void AddPass(string name, IPass pass)
    {
        _passes.Add(new PassRegistration(name, pass));
    }

    /// <summary>
    /// Begins a set of passes that will be ran in a loop until there are no changes to the IR.
    /// </summary>
    public void PushLoopUntilUnchanged()
    {
        _loopsUntilUnchanged.Add(new Interval(_passes.Count));
        _loopsUntilUnchangedIndex++;
    }

    public void PopLoopUntilUnchanged()
    {
        _loopsUntilUnchanged[_loopsUntilUnchangedIndex] =
            new Interval(_loopsUntilUnchanged[_loopsUntilUnchangedIndex].Begin, _passes.Count);
        _loopsUntilUnchangedIndex--;
    }

    private struct LoopState
    {
        public bool Changed;
        public int IterationCount;
        public string BaseName;

        public LoopState(string baseName)
        {
            Changed = false;
            IterationCount = 0;
            BaseName = baseName;
        }
    }

    /// <summary>
    /// Run the registered passes on all the functions, printing out the root function after the specified passes
    /// execute. This runs a pass on all the functions before proceeding to the next pass.
    /// </summary>
    /// <param name="context">The decompilation context provided to every pass</param>
    /// <param name="functions">The functions to run all the passes on</param>
    /// <param name="catchExceptions"></param>
    /// <returns>String representation of the first function after each specified pass runs</returns>
    public DecompilationResult RunOnFunctions(DecompilationContext context,
        IReadOnlyList<Function> functions,
        bool catchExceptions)
    {
        var irResults = new List<PassIrResult>(_passes.Count);
        var dotGraphResults = new List<PassDotGraphResult>(_passes.Count);
        var functionContexts = ArrayPool<FunctionContext>.Shared.Rent(functions.Count);
        var loopStates = new List<LoopState>(_loopsUntilUnchanged.Count);
        
        for (var i = 0; i < functions.Count; i++)
        {
            functionContexts[i] = new FunctionContext(context, functions[i]);
        }
        
        var printer = new FunctionPrinter();
        string? error = null;
        for (var p = 0; p < _passes.Count; p++)
        {
            // See if we need to enter a loop
            if (_loopsUntilUnchanged.Count > loopStates.Count && 
                _loopsUntilUnchanged[loopStates.Count].Begin == p)
            {
                var baseName = loopStates.Count > 0 ? $"{loopStates[^1].BaseName}_{loopStates[^1].IterationCount}" : "";
                loopStates.Add(new LoopState(baseName));
                p--;
                continue;
            }
            
            // See if we need to leave or restart a loop
            if (loopStates.Count > 0 && _loopsUntilUnchanged[loopStates.Count - 1].End == p)
            {
                var loopState = loopStates[^1];
                if (loopState.Changed)
                {
                    loopState.IterationCount++;
                    loopState.Changed = false;
                    loopStates[^1] = loopState;
                    p = _loopsUntilUnchanged[loopStates.Count - 1].Begin - 1;
                    continue;
                }
                loopStates.RemoveAt(loopStates.Count - 1);
                p--;
                continue;
            }

            var passName = loopStates.Count > 0
                ? $"{_passes[p].Name}{loopStates[^1].BaseName}_{loopStates[^1].IterationCount}"
                : _passes[p].Name;

            bool changed = false;
            for (var i = 0; i < functions.Count; i++)
            {
                if (catchExceptions)
                {
                    try
                    {
                        changed |= _passes[p].Pass.RunOnFunction(context, functionContexts[i], functions[i]);
                    }
                    catch (Exception e)
                    {
                        error = $"Exception occurred!\nPass: {passName}\nFunction ID: {i}\n\n{e.Message}\n\n{e.StackTrace}";
                        goto Error;
                    }
                }
                else
                {
                    changed |= _passes[p].Pass.RunOnFunction(context, functionContexts[i], functions[i]);
                }
            }

            if (loopStates.Count > 0)
            {
                var state = loopStates[^1];
                state.Changed |= changed;
                loopStates[^1] = state;
            }

            // Dump pass IR
            if (_dumpAllIrPasses || _dumpIrPasses.Contains(_passes[p].Name))
            {
                var output = new StringBuilder(128 * 1024);
                if (catchExceptions)
                {
                    try
                    {
                        printer.PrintFunctionToStringBuilder(functions[0], output);
                        irResults.Add(new PassIrResult(passName, output.ToString()));
                    }
                    catch (Exception e)
                    {
                        error = $"Exception occurred printing IR!\nPass: {passName}\n\n{e.Message}\n\n{e.StackTrace}";
                        irResults.Add(new PassIrResult(passName, ""));
                        goto Error;
                    }
                }
                else
                {
                    printer.PrintFunctionToStringBuilder(functions[0], output);
                    irResults.Add(new PassIrResult(passName, output.ToString()));
                }
            }
            
            // Dump dot files
            if (_dumpAllDotGraphPasses || 
                (_dumpCfgMutatingDotGraphPasses && _passes[p].Pass.MutatesCfg) || 
                _dumpDotGraphPasses.Contains(_passes[p].Name))
            {
                var functionDotResults = new List<FunctionDotGraphResult>();
                foreach (var f in functions)
                {
                    functionDotResults.Add(
                        new FunctionDotGraphResult(f.FunctionId, printer.PrintDotGraphForFunction(f)));
                }
                dotGraphResults.Add(new PassDotGraphResult(passName, functionDotResults.ToArray()));
            }
        }
        
        Error:
        List<int> functionsWithWarnings = new List<int>(functions.Count);
        for (var i = 0; i < functions.Count; i++)
        {
            if (functions[i].Warnings.Count > 0)
                functionsWithWarnings.Add(i);
            functionContexts[i].Dispose();
        }
        ArrayPool<FunctionContext>.Shared.Return(functionContexts);
        return new DecompilationResult(
             error == null ? printer.PrintFunction(functions[0]) : null, 
             error, 
             irResults.ToArray(),
             dotGraphResults.ToArray(),
             functionsWithWarnings.ToArray());
    }
}