using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class VarSearchEngine : SearchEngine<bool, string>
{
    public ParamEditorView CurrentView;
    public VarSearchEngine(ParamEditorView curView)
    {
        CurrentView = curView;

        Setup();
    }

    internal void Setup()
    {
        unpacker = dummy =>
        {
            return MassParamEdit.massEditVars.Keys.ToList();
        };

        filterList.Add("vars", newCmd(new[] { "variable names (regex)" },
            "Selects variables whose name matches the given regex", (args, lenient) =>
            {
                if (args[0].StartsWith('$'))
                {
                    args[0] = args[0].Substring(1);
                }

                Regex rx = lenient ? new Regex(args[0], RegexOptions.IgnoreCase) : new Regex($@"^{args[0]}$");
                return noContext(name => rx.IsMatch(name));
            }));
    }
}
