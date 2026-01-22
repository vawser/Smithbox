using Hexa.NET.ImGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;


public class AutoFillSearchEngine<A, B>
{
    public ParamView CurrentView;

    private readonly string[] _autoFillArgs;
    private readonly SearchEngine<A, B> engine;
    private readonly string id;
    private AutoFillSearchEngine<A, B> _additionalCondition;
    private bool _autoFillNotToggle;
    private bool _useAdditionalCondition;

    internal AutoFillSearchEngine(ParamView curView, string id, SearchEngine<A, B> searchEngine)
    {
        CurrentView = curView;
        this.id = id;
        engine = searchEngine;
        _autoFillArgs = Enumerable.Repeat("", engine.AllCommands().Sum(x => x.Item2.Length)).ToArray();
        _autoFillNotToggle = false;
        _useAdditionalCondition = false;
        _additionalCondition = null;
    }

    internal string Menu(bool enableComplexToggles, bool enableDefault, string suffix, string inheritedCommand,
        Func<string, string> subMenu)
    {
        return Menu<object, object>(enableComplexToggles, null, enableDefault, suffix, inheritedCommand, subMenu);
    }

    internal string Menu<C, D>(bool enableComplexToggles, AutoFillSearchEngine<C, D> multiStageSE,
        bool enableDefault, string suffix, string inheritedCommand, Func<string, string> subMenu)
    {
        var currentArgIndex = 0;
        if (enableComplexToggles)
        {
            ImGui.Checkbox("Invert selection?##meautoinputnottoggle" + id, ref _autoFillNotToggle);
            ImGui.SameLine();
            ImGui.Checkbox("Add another condition?##meautoinputadditionalcondition" + id,
                ref _useAdditionalCondition);
        }
        else if (multiStageSE != null)
        {
            ImGui.Checkbox("Add another condition?##meautoinputadditionalcondition" + id,
                ref _useAdditionalCondition);
        }

        if (_useAdditionalCondition && _additionalCondition == null)
        {
            _additionalCondition = new AutoFillSearchEngine<A, B>(CurrentView, id + "0", engine);
        }
        else if (!_useAdditionalCondition)
        {
            _additionalCondition = null;
        }

        foreach ((string, string[], string) cmd in enableDefault
                     ? engine.VisibleCommands().Append((null, engine.defaultFilter.args, engine.defaultFilter.wiki))
                         .ToList()
                     : engine.VisibleCommands())
        {
            var argIndices = new int[cmd.Item2.Length];
            var valid = true;
            for (var i = 0; i < argIndices.Length; i++)
            {
                argIndices[i] = currentArgIndex;
                currentArgIndex++;
                if (string.IsNullOrEmpty(_autoFillArgs[argIndices[i]]))
                {
                    valid = false;
                }
            }

            string subResult = null;
            var wiki = cmd.Item3;
            ParamEditorHints.AddImGuiHintButton(cmd.Item1 == null ? "hintdefault" : "hint" + cmd.Item1, ref wiki, false,
                true);
            if (subMenu != null || _additionalCondition != null)
            {
                if (ImGui.BeginMenu(cmd.Item1 == null ? "Default filter..." : cmd.Item1, valid))
                {
                    var curResult = inheritedCommand + getCurrentStepText(valid, cmd.Item1, argIndices,
                        _additionalCondition != null ? " && " : suffix);
                    if (_useAdditionalCondition && multiStageSE != null)
                    {
                        subResult = multiStageSE.Menu(enableComplexToggles, enableDefault, suffix, curResult,
                            subMenu);
                    }
                    else if (_additionalCondition != null)
                    {
                        subResult = _additionalCondition.Menu(enableComplexToggles, enableDefault, suffix,
                            curResult, subMenu);
                    }
                    else
                    {
                        subResult = subMenu(curResult);
                    }

                    ImGui.EndMenu();
                }
            }
            else
            {
                subResult = ImGui.Selectable(cmd.Item1 == null ? "Default filter..." : cmd.Item1, false,
                    valid ? ImGuiSelectableFlags.None : ImGuiSelectableFlags.Disabled)
                    ? suffix
                    : null;
            }

            //ImGui.Indent();
            for (var i = 0; i < argIndices.Length; i++)
            {
                if (i != 0)
                {
                    ImGui.SameLine();
                }

                ImGui.InputTextWithHint("##meautoinput" + argIndices[i], cmd.Item2[i],
                    ref _autoFillArgs[argIndices[i]], 256);

                if (CurrentView.MassEdit.AutoFill != null)
                {
                    var var = CurrentView.MassEdit.AutoFill.MassEditAutoFillForVars(argIndices[i]);

                    if (var != null)
                    {
                        _autoFillArgs[argIndices[i]] = var;
                    }
                }
            }

            //ImGui.Unindent();
            var currentResult = getCurrentStepText(valid, cmd.Item1, argIndices,
                _additionalCondition != null ? " && " : suffix);
            if (subResult != null && valid)
            {
                return currentResult + subResult;
            }
        }

        return null;
    }

    internal string getCurrentStepText(bool valid, string command, int[] argIndices, string suffixToUse)
    {
        if (!valid)
        {
            return null;
        }

        if (command != null)
        {
            var cmdText = _autoFillNotToggle ? '!' + command : command;
            for (var i = 0; i < argIndices.Length; i++)
            {
                cmdText += " " + _autoFillArgs[argIndices[i]];
            }

            return cmdText + suffixToUse;
        }

        if (argIndices.Length > 0)
        {
            var argText = _autoFillArgs[argIndices[0]];
            for (var i = 1; i < argIndices.Length; i++)
            {
                argText += " " + _autoFillArgs[argIndices[i]];
            }

            return argText + suffixToUse;
        }

        return null;
    }
}
