namespace LuaDecompilerCore;

public record struct FunctionDotGraphResult(int FunctionId, string DotGraph);

public record struct PassDotGraphResult(string Pass, FunctionDotGraphResult[] FunctionResults);

public record struct PassIrResult(string Pass, string Ir);

/// <summary>
/// Result of a decompilation
/// </summary>
public record DecompilationResult(
    string? DecompiledSource,
    string? ErrorMessage,
    PassIrResult[] IrResults,
    PassDotGraphResult[] DotGraphResults,
    int[] FunctionsWithWarnings);
