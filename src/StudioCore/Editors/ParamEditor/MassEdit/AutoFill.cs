using Andre.Formats;
using Hexa.NET.ImGui;
using Octokit;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.ParamEditor.Data;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.ParamEditor;

public class AutoFillSearchEngine<A, B>
{
    public ProjectEntry Project;

    private readonly string[] _autoFillArgs;
    private readonly SearchEngine<A, B> engine;
    private readonly string id;
    private AutoFillSearchEngine<A, B> _additionalCondition;
    private bool _autoFillNotToggle;
    private bool _useAdditionalCondition;

    internal AutoFillSearchEngine(ProjectEntry project, string id, SearchEngine<A, B> searchEngine)
    {
        Project = project;
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
            _additionalCondition = new AutoFillSearchEngine<A, B>(Project, id + "0", engine);
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
            UIHints.AddImGuiHintButton(cmd.Item1 == null ? "hintdefault" : "hint" + cmd.Item1, ref wiki, false,
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

                if (Project.ParamEditor.MassEditHandler.AutoFill != null)
                {
                    var var = Project.ParamEditor.MassEditHandler.AutoFill.MassEditAutoFillForVars(argIndices[i]);

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

public class AutoFill
{
    public ProjectEntry Project;

    // Type hell. Can't omit the type.
    private readonly AutoFillSearchEngine<ParamSelection, (MassEditRowSource, Param.Row)>
        autoFillParse;

    private readonly AutoFillSearchEngine<bool, string> autoFillVse;

    private readonly AutoFillSearchEngine<bool, (ParamBank, Param)> autoFillPse;

    private readonly AutoFillSearchEngine<(ParamBank, Param), Param.Row> autoFillRse;

    private readonly AutoFillSearchEngine<(string, Param.Row), (PseudoColumn, Param.Column)> autoFillCse;

    private string[] _autoFillArgsGop;

    private string[] _autoFillArgsRop;

    private string[] _autoFillArgsCop;

    private string[] _autoFillArgsOa;

    private string _literalArg = "";

    internal Vector4 HINTCOLOUR = new(0.3f, 0.5f, 1.0f, 1.0f);
    internal Vector4 PREVIEWCOLOUR = new(0.65f, 0.75f, 0.65f, 1.0f);

    public AutoFill(ProjectEntry project)
    {
        Project = project;

        autoFillParse = new(Project, "parse", Project.ParamEditor.MassEditHandler.parse);
        autoFillVse = new(Project, "vse", Project.ParamEditor.MassEditHandler.vse);
        autoFillPse = new(Project, "pse", Project.ParamEditor.MassEditHandler.pse);
        autoFillRse = new(Project, "rse", Project.ParamEditor.MassEditHandler.rse);
        autoFillCse = new(Project, "cse", Project.ParamEditor.MassEditHandler.cse);

        _autoFillArgsGop = Enumerable
            .Repeat("", project.ParamEditor.MassEditHandler.GlobalOps.AvailableCommands().Sum(x => x.Item2.Length)).ToArray();

        _autoFillArgsRop = Enumerable
            .Repeat("", project.ParamEditor.MassEditHandler.RowOps.AvailableCommands().Sum(x => x.Item2.Length)).ToArray();

        _autoFillArgsCop = Enumerable
            .Repeat("", project.ParamEditor.MassEditHandler.FieldOps.AvailableCommands().Sum(x => x.Item2.Length)).ToArray();

        _autoFillArgsOa = Enumerable
            .Repeat("", project.ParamEditor.MassEditHandler.OperationArgs.AllArguments().Sum(x => x.Item2.Length)).ToArray();
    }

    public string ParamSearchBarAutoFill()
    {
        ImGui.Button($@"{Icons.CaretDown}##paramAutofill");
        if (ImGui.BeginPopupContextItem("##psbautoinputoapopup", ImGuiPopupFlags.MouseButtonLeft))
        {
            ImGui.TextColored(HINTCOLOUR, "Select params...");
            var result = autoFillPse.Menu(true, false, "", null, null);
            ImGui.EndPopup();
            return result;
        }

        return null;
    }

    public string RowSearchBarAutoFill()
    {
        ImGui.Button($@"{Icons.CaretDown}##rowAutofill");
        if (ImGui.BeginPopupContextItem("##rsbautoinputoapopup", ImGuiPopupFlags.MouseButtonLeft))
        {
            ImGui.TextColored(HINTCOLOUR, "Select rows...");
            var result = autoFillRse.Menu(true, false, "", null, null);
            ImGui.EndPopup();
            return result;
        }

        return null;
    }

    public string ColumnSearchBarAutoFill()
    {
        ImGui.Button($@"{Icons.CaretDown}##fieldAutofill");
        if (ImGui.BeginPopupContextItem("##csbautoinputoapopup", ImGuiPopupFlags.MouseButtonLeft))
        {
            ImGui.TextColored(HINTCOLOUR, "Select fields...");
            var result = autoFillCse.Menu(true, false, "", null, null);
            ImGui.EndPopup();
            return result;
        }

        return null;
    }

    public string MassEditCompleteAutoFill()
    {
        ImGui.TextUnformatted("Add command...");
        ImGui.SameLine();
        ImGui.Button($@"{Icons.CaretDown}##massEditAutofill");
        if (ImGui.BeginPopupContextItem("##meautoinputoapopup", ImGuiPopupFlags.MouseButtonLeft))
        {
            ImGui.PushID("paramrow");
            ImGui.TextColored(HINTCOLOUR, "Select param and rows...");
            var result1 = autoFillParse.Menu(false, autoFillRse, false, ": ", null, inheritedCommand =>
            {
                if (inheritedCommand != null)
                {
                    ImGui.TextColored(PREVIEWCOLOUR, inheritedCommand);
                }

                ImGui.TextColored(HINTCOLOUR, "Select fields...");
                var res1 = autoFillCse.Menu(true, true, ": ", inheritedCommand, inheritedCommand2 =>
                {
                    if (inheritedCommand2 != null)
                    {
                        ImGui.TextColored(PREVIEWCOLOUR, inheritedCommand2);
                    }

                    ImGui.TextColored(HINTCOLOUR, "Select field operation...");
                    return MassEditAutoFillForOperation(Project.ParamEditor.MassEditHandler.FieldOps, ref _autoFillArgsCop, ";", null);
                });
                ImGui.Separator();
                ImGui.TextColored(HINTCOLOUR, "Select row operation...");
                var res2 = MassEditAutoFillForOperation(Project.ParamEditor.MassEditHandler.RowOps, ref _autoFillArgsRop, ";", null);
                if (res1 != null)
                {
                    return res1;
                }

                return res2;
            });
            ImGui.PopID();
            ImGui.Separator();
            ImGui.PushID("param");
            ImGui.TextColored(HINTCOLOUR, "Select params...");
            var result2 = autoFillPse.Menu(true, false, ": ", null, inheritedCommand =>
            {
                if (inheritedCommand != null)
                {
                    ImGui.TextColored(PREVIEWCOLOUR, inheritedCommand);
                }

                ImGui.TextColored(HINTCOLOUR, "Select rows...");
                return autoFillRse.Menu(true, false, ": ", inheritedCommand, inheritedCommand2 =>
                {
                    if (inheritedCommand2 != null)
                    {
                        ImGui.TextColored(PREVIEWCOLOUR, inheritedCommand2);
                    }

                    ImGui.TextColored(HINTCOLOUR, "Select fields...");
                    var res1 = autoFillCse.Menu(true, true, ": ", inheritedCommand2, inheritedCommand3 =>
                    {
                        if (inheritedCommand3 != null)
                        {
                            ImGui.TextColored(PREVIEWCOLOUR, inheritedCommand3);
                        }

                        ImGui.TextColored(HINTCOLOUR, "Select field operation...");
                        return MassEditAutoFillForOperation(Project.ParamEditor.MassEditHandler.FieldOps, ref _autoFillArgsCop, ";",
                            null);
                    });
                    string res2 = null;
                    if (CFG.Current.Param_AdvancedMassedit)
                    {
                        ImGui.Separator();
                        ImGui.TextColored(HINTCOLOUR, "Select row operation...");
                        res2 = MassEditAutoFillForOperation(Project.ParamEditor.MassEditHandler.RowOps, ref _autoFillArgsRop, ";", null);
                    }

                    if (res1 != null)
                    {
                        return res1;
                    }

                    return res2;
                });
            });
            ImGui.PopID();
            string result3 = null;
            string result4 = null;
            if (CFG.Current.Param_AdvancedMassedit)
            {
                ImGui.Separator();
                ImGui.PushID("globalop");
                ImGui.TextColored(HINTCOLOUR, "Select global operation...");
                result3 = MassEditAutoFillForOperation(Project.ParamEditor.MassEditHandler.GlobalOps, ref _autoFillArgsGop, ";",
                    null);
                ImGui.PopID();
                if (MassParamEdit.massEditVars.Count != 0)
                {
                    ImGui.Separator();
                    ImGui.PushID("var");
                    ImGui.TextColored(HINTCOLOUR, "Select variables...");
                    result4 = autoFillVse.Menu(false, false, ": ", null, inheritedCommand =>
                    {
                        if (inheritedCommand != null)
                        {
                            ImGui.TextColored(PREVIEWCOLOUR, inheritedCommand);
                        }

                        ImGui.TextColored(HINTCOLOUR, "Select value operation...");
                        return MassEditAutoFillForOperation(Project.ParamEditor.MassEditHandler.FieldOps, ref _autoFillArgsCop, ";",
                            null);
                    });
                }
            }

            ImGui.EndPopup();
            if (result1 != null)
            {
                return result1;
            }

            if (result2 != null)
            {
                return result2;
            }

            if (result3 != null)
            {
                return result3;
            }

            return result4;
        }

        return null;
    }

    public string MassEditOpAutoFill()
    {
        return MassEditAutoFillForOperation(Project.ParamEditor.MassEditHandler.FieldOps, ref _autoFillArgsCop, ";", null);
    }

    private string MassEditAutoFillForOperation<A, B>(MEOperation<A, B> ops, ref string[] staticArgs,
        string suffix, Func<string> subMenu)
    {
        var currentArgIndex = 0;
        string result = null;
        foreach ((string, string[], string) cmd in ops.AvailableCommands())
        {
            var argIndices = new int[cmd.Item2.Length];
            var valid = true;
            for (var i = 0; i < argIndices.Length; i++)
            {
                argIndices[i] = currentArgIndex;
                currentArgIndex++;
                if (string.IsNullOrEmpty(staticArgs[argIndices[i]]))
                {
                    valid = false;
                }
            }

            var wiki = cmd.Item3;
            UIHints.AddImGuiHintButton(cmd.Item1, ref wiki, false, true);
            if (subMenu != null)
            {
                if (ImGui.BeginMenu(cmd.Item1, valid))
                {
                    result = subMenu();
                    ImGui.EndMenu();
                }
            }
            else
            {
                result = ImGui.Selectable(cmd.Item1, false,
                    valid ? ImGuiSelectableFlags.None : ImGuiSelectableFlags.Disabled)
                    ? suffix
                    : null;
            }

            ImGui.Indent();
            for (var i = 0; i < argIndices.Length; i++)
            {
                if (i != 0)
                {
                    ImGui.SameLine();
                }

                ImGui.InputTextWithHint("##meautoinputop" + argIndices[i], cmd.Item2[i],
                    ref staticArgs[argIndices[i]], 256);
                ImGui.SameLine();
                ImGui.Button($@"{Icons.CaretDown}");
                if (ImGui.BeginPopupContextItem("##meautoinputoapopup" + argIndices[i],
                        ImGuiPopupFlags.MouseButtonLeft))
                {
                    var opargResult = MassEditAutoFillForArguments(Project.ParamEditor.MassEditHandler.OperationArgs, ref _autoFillArgsOa);
                    if (opargResult != null)
                    {
                        staticArgs[argIndices[i]] = opargResult;
                    }

                    ImGui.EndPopup();
                }
            }

            ImGui.Unindent();
            if (result != null && valid)
            {
                var argText = argIndices.Length > 0 ? staticArgs[argIndices[0]] : null;
                for (var i = 1; i < argIndices.Length; i++)
                {
                    argText += ":" + staticArgs[argIndices[i]];
                }

                result = cmd.Item1 + (argText != null ? " " + argText + result : result);
                return result;
            }
        }

        return result;
    }

    private string MassEditAutoFillForArguments(MEOperationArgument oa, ref string[] staticArgs)
    {
        var currentArgIndex = 0;
        string result = null;
        foreach ((string, string, string[]) arg in oa.VisibleArguments())
        {
            var argIndices = new int[arg.Item3.Length];
            var valid = true;
            for (var i = 0; i < argIndices.Length; i++)
            {
                argIndices[i] = currentArgIndex;
                currentArgIndex++;
                if (string.IsNullOrEmpty(staticArgs[argIndices[i]]))
                {
                    valid = false;
                }
            }

            var selected = false;
            var wiki = arg.Item2;
            UIHints.AddImGuiHintButton(arg.Item1, ref wiki, false, true);
            if (ImGui.Selectable(arg.Item1, selected,
                    valid ? ImGuiSelectableFlags.None : ImGuiSelectableFlags.Disabled))
            {
                result = arg.Item1;
                var argText = "";
                for (var i = 0; i < argIndices.Length; i++)
                {
                    argText += " " + staticArgs[argIndices[i]];
                }

                return arg.Item1 + argText;
            }

            ImGui.Indent();
            for (var i = 0; i < argIndices.Length; i++)
            {
                if (i != 0)
                {
                    ImGui.SameLine();
                }

                ImGui.InputTextWithHint("##meautoinputoa" + argIndices[i], arg.Item3[i],
                    ref staticArgs[argIndices[i]], 256);
                var var = MassEditAutoFillForVars(argIndices[i]);
                if (var != null)
                {
                    staticArgs[argIndices[i]] = var;
                }
            }

            ImGui.Unindent();
        }

        if (MassParamEdit.massEditVars.Count != 0)
        {
            ImGui.Separator();
            ImGui.TextUnformatted("Defined variables...");
            foreach (KeyValuePair<string, object> pair in MassParamEdit.massEditVars)
            {
                if (ImGui.Selectable(pair.Key + "(" + pair.Value + ")"))
                {
                    return '$' + pair.Key;
                }
            }
        }

        ImGui.Separator();
        if (ImGui.Selectable("Exactly..."))
        {
            result = '"' + _literalArg + '"';
        }

        ImGui.InputTextWithHint("##meautoinputoaExact", "literal value...", ref _literalArg, 256);
        return result;
    }

    internal string MassEditAutoFillForVars(int id)
    {
        if (MassParamEdit.massEditVars.Count == 0)
        {
            return null;
        }

        ImGui.SameLine();
        ImGui.Button("$");
        if (ImGui.BeginPopupContextItem("##meautoinputvarpopup" + id, ImGuiPopupFlags.MouseButtonLeft))
        {
            ImGui.TextUnformatted("Defined variables...");
            ImGui.Separator();
            foreach (KeyValuePair<string, object> pair in MassParamEdit.massEditVars)
            {
                if (ImGui.Selectable(pair.Key + "(" + pair.Value + ")"))
                {
                    ImGui.EndPopup();
                    return '$' + pair.Key;
                }
            }

            ImGui.EndPopup();
        }

        return null;
    }
}
