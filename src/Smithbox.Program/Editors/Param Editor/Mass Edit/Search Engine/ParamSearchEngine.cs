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
            LOC.Get("PARAM_PSE_Modified_TT"),
            noArgs(noContext(param =>
            {
                if (param.Item1 != bank)
                {
                    return false;
                }

                HashSet<int> cache = bank.GetVanillaDiffRows(bank.GetKeyForParam(param.Item2));
                return cache.Count > 0;
            }))));

        filterList.Add("param", newCmd(new[] { 
            LOC.Get("PARAM_PSE_Param_Hint_1")},
            LOC.Get("PARAM_PSE_Param_TT"), 
            (args, lenient) =>
            {
                Regex rx = lenient ? new Regex(args[0], RegexOptions.IgnoreCase) : new Regex($@"^{args[0]}$");
                return noContext(param =>
                    param.Item1 != bank
                        ? false
                        : rx.IsMatch(bank.GetKeyForParam(param.Item2) == null
                            ? ""
                            : bank.GetKeyForParam(param.Item2)));
            }));

        filterList.Add("auxparam", newCmd(new[] { 
            LOC.Get("PARAM_PSE_AuxParam_Hint_1"),
            LOC.Get("PARAM_PSE_AuxParam_Hint_2")},
            LOC.Get("PARAM_PSE_AuxParam_TT"),
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

        filterList.Add("paramtype", newCmd(new[] { 
            LOC.Get("PARAM_PSE_ParamType_Hint_1")},
            LOC.Get("PARAM_PSE_ParamType_TT"), (args, lenient) =>
            {
                Regex rx = lenient ? new Regex(args[0], RegexOptions.IgnoreCase) : new Regex($@"^{args[0]}$");
                return noContext(param =>
                    param.Item1 != bank
                        ? false
                        : rx.IsMatch(bank.GetTypeForParam(param.Item2) == null
                            ? ""
                            : bank.GetTypeForParam(param.Item2)));
            }));

        defaultFilter = newCmd(new[] { 
            LOC.Get("PARAM_PSE_Default_Hint_1")},
            LOC.Get("PARAM_PSE_Default_TT"), (args, lenient) =>
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