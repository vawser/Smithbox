using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class MEOperation<T, O>
{
    internal Dictionary<string, (string[], string, Func<T, string[], O>)> operations = new();

    internal MEOperation()
    {
        Setup();
    }

    internal virtual void Setup()
    {
    }

    internal bool HandlesCommand(string command)
    {
        return operations.ContainsKey(command);
    }

    public List<(string, string[], string)> AvailableCommands()
    {
        List<(string, string[], string)> options = new();
        foreach (var op in operations.Keys)
        {
            options.Add((op, operations[op].Item1, operations[op].Item2));
        }

        return options;
    }
}
