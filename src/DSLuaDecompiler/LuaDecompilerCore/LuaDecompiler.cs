using System;
using System.Collections.Generic;
using System.Linq;
using LuaDecompilerCore.IR;
using LuaDecompilerCore.Utilities;

namespace LuaDecompilerCore
{
    public class LuaDecompiler
    {
        private DecompilationOptions DecompilationOptions { get; set; }

        public LuaDecompiler(DecompilationOptions options)
        {
            DecompilationOptions = options;
        }

        public DecompilationResult DecompileLuaFunction(
            ILanguageDecompiler languageDecompiler,
            Function main,
            LuaFile.Function luaMain)
        {
            var functions = new List<Function>();
            
            void Visit(LuaFile.Function luaFunction, Function irFunction)
            {
                // Language specific initialization
                languageDecompiler.InitializeFunction(luaFunction, irFunction);
                
                // Enable debug comments if set
                irFunction.InsertDebugComments = DecompilationOptions.OutputDebugComments;

                // Initialize function parameters
                irFunction.ParameterCount = (int)luaFunction.NumParams;
                irFunction.RegisterCount = (int)luaFunction.NumParams;

                // Give constant table
                irFunction.Constants = luaFunction.Constants;

                // Now generate IR from the bytecode using language specific decompiler
                languageDecompiler.GenerateIr(luaFunction, irFunction);
                
                // Add function to run decompilation passes on
                functions.Add(irFunction);
                
                // Strip local information from the function if specified
                if (DecompilationOptions.IgnoreDebugInfo)
                {
                    foreach (var i in irFunction.BeginBlock.Instructions)
                    {
                        if (i is Assignment a)
                            a.LocalAssignments = null;
                    }
                }
                
                // Now visit all the child closures unless they are excluded
                for (var i = 0; i < luaFunction.ChildFunctions.Length; i++)
                {
                    if (DecompilationOptions.IncludedFunctionIds != null && 
                        !DecompilationOptions.IncludedFunctionIds.Contains(luaFunction.ChildFunctions[i].FunctionId))
                        continue;
                    if (DecompilationOptions.ExcludedFunctionIds.Contains(luaFunction.ChildFunctions[i].FunctionId))
                        continue;
                    Visit(luaFunction.ChildFunctions[i], irFunction.LookupClosure((uint)i));
                }
            }
            
            // Visit all the functions and generate the initial IR from the bytecode
            if (DecompilationOptions.CatchPassExceptions)
            {
                try
                {
                    Visit(luaMain, main);
                }
                catch (Exception e)
                {
                    return new DecompilationResult(
                        null,
                        $"Exception occurred in building IR!\n\n{e.Message}\n\n{e.StackTrace}",
                        Array.Empty<PassIrResult>(),
                        Array.Empty<PassDotGraphResult>(),
                        Array.Empty<int>()
                    );
                }
            }
            else
            {
                Visit(luaMain, main);
            }

            // Create pass manager, add language specific passes, and run
            var passManager = new PassManager(DecompilationOptions);
            languageDecompiler.AddDecompilePasses(passManager);
            return passManager.RunOnFunctions(
                new DecompilationContext(), 
                functions,
                DecompilationOptions.CatchPassExceptions);
        }

        public string? DisassembleLuaFunction(
            ILanguageDecompiler languageDecompiler,
            LuaFile.Function luaFunction)
        {
            return languageDecompiler.Disassemble(luaFunction);
        }
    }
}
