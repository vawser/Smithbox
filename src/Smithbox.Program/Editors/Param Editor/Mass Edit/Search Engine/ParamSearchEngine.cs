using Andre.Formats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;


public class ParamSearchEngine : SearchEngine<bool, (ParamBank, Param)>
{
    public ParamEditorView CurrentView;

    public ParamSearchEngine(ParamEditorView curView)
    {
        CurrentView = curView;
        Setup();
    }

    internal void Setup()
    {
        var bank = CurrentView.GetPrimaryBank();
        var auxBanks = CurrentView.GetParamData().AuxBanks;

        unpacker = dummy =>
            auxBanks.Select((aux, i) => aux.Value.Params.Select((x, i) => (aux.Value, x.Value)))
                .Aggregate(bank.Params.Values.Select((x, i) => (bank, x)), (o, n) => o.Concat(n)).ToList();

        filterList.Add("modified", newCmd(new string[0],
            "Selects params where any rows do not match the vanilla version, or where any are added. Ignores row names",
            noArgs(noContext(param =>
            {
                if (param.Item1 != bank)
                {
                    return false;
                }

                HashSet<int> cache = bank.GetVanillaDiffRows(bank.GetKeyForParam(param.Item2));
                return cache.Count > 0;
            }))));

        filterList.Add("param", newCmd(new[] { "param name (regex)" },
            "Selects all params whose name matches the given regex", (args, lenient) =>
            {
                Regex rx = lenient ? new Regex(args[0], RegexOptions.IgnoreCase) : new Regex($@"^{args[0]}$");
                return noContext(param =>
                    param.Item1 != bank
                        ? false
                        : rx.IsMatch(bank.GetKeyForParam(param.Item2) == null
                            ? ""
                            : bank.GetKeyForParam(param.Item2)));
            }));

        filterList.Add("auxparam", newCmd(new[] { "parambank name", "param name (regex)" },
            "Selects params from the specified regulation or parambnd where the param name matches the given regex",
            (args, lenient) =>
            {
                var auxBanks = CurrentView.GetParamData().AuxBanks;

                ParamBank auxBank = auxBanks[args[0]];
                Regex rx = lenient ? new Regex(args[1], RegexOptions.IgnoreCase) : new Regex($@"^{args[1]}$");
                return noContext(param =>
                    param.Item1 != auxBank
                        ? false
                        : rx.IsMatch(auxBank.GetKeyForParam(param.Item2) == null
                            ? ""
                            : auxBank.GetKeyForParam(param.Item2)));
            }, () => auxBanks.Count > 0));

        defaultFilter = newCmd(new[] { "param name (regex)" },
            "Selects all params whose name matches the given regex", (args, lenient) =>
            {
                Regex rx = lenient ? new Regex(args[0], RegexOptions.IgnoreCase) : new Regex($@"^{args[0]}$");
                return noContext(param =>
                    param.Item1 != bank
                        ? false
                        : rx.IsMatch(bank.GetKeyForParam(param.Item2) == null
                            ? ""
                            : bank.GetKeyForParam(param.Item2)));
            });
    }
}