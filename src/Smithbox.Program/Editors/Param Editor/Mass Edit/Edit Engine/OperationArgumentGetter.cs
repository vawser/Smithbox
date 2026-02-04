using Andre.Formats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class OperationArgumentGetter
{
    public string[] args;

    internal Func<string[], Func<int, Param, Func<int, Param.Row, Func<int, (ParamEditorPseudoColumn, Param.Column), string>>>>
        func;

    internal Func<bool> shouldShow;
    public string wiki;

    internal OperationArgumentGetter(string[] args, string wiki,
        Func<string[], Func<int, Param, Func<int, Param.Row, Func<int, (ParamEditorPseudoColumn, Param.Column), string>>>>
            func, Func<bool> shouldShow)
    {
        this.args = args;
        this.wiki = wiki;
        this.func = func;
        this.shouldShow = shouldShow;
    }
}
