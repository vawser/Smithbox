using System.Text;
using LuaDecompilerCore.IR;

namespace LuaDecompilerCore;

public partial class FunctionPrinter
{
    /// <summary>
    /// Dumps the control flow graph of a function as a DOT file
    /// </summary>
    /// <param name="function"></param>
    /// <returns></returns>
    public string PrintDotGraphForFunction(Function function)
    {
        _builder = new StringBuilder();
        
        Append("digraph {");
        NewLine();
        PushIndent();
        foreach (var source in function.BlockList)
        {
            Append($@"{source.Name}");
            if (source.Successors.Count > 0)
            {
                Append(" -> { ");
                foreach (var dest in source.Successors)
                {
                    Append($@"{dest.Name} ");
                }
                Append("}");
            }
            NewLine();
        }
        PopIndent();
        Append('}');
        
        return _builder.ToString();
    }
}