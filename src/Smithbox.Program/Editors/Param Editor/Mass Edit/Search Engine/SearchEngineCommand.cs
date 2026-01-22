using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class SearchEngineCommand<A, B>
{
    public string[] args;
    internal Func<string[], bool, Func<A, Func<B, bool>>> func;
    internal Func<bool> shouldShow;
    public string wiki;

    internal SearchEngineCommand(string[] args, string wiki, Func<string[], bool, Func<A, Func<B, bool>>> func,
        Func<bool> shouldShow)
    {
        this.args = args;
        this.wiki = wiki;
        this.func = func;
        this.shouldShow = shouldShow;
    }
}