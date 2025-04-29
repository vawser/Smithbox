using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Core.ProjectNS;
using StudioCore.Editor;
using StudioCore.Editors.EsdEditor.EsdLang;
using StudioCore.Interface;
using StudioCore.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.ESD;
using static StudioCore.Editors.EsdEditor.EsdLang.AST;

namespace StudioCore.Editors.EzStateEditorNS;

public class EzStateFieldView
{
    public Project Project;
    public EzStateEditor Editor;
    public EzStateFieldView(Project curProject, EzStateEditor editor)
    {
        Project = curProject;
        Editor = editor;
    }

    public void Draw()
    {
        if (Editor.Selection.SelectedStateGroupNode == null)
            return;

        Editor.EditorFocus.SetFocusContext(EzStateEditorContext.StateNodeContents);

        var stateNode = Editor.Selection.SelectedStateGroupNode;

        DisplayCommands(stateNode, stateNode.EntryCommands, "entry", "Entry Commands");

        ImGui.Separator();

        DisplayCommands(stateNode, stateNode.ExitCommands, "exit", "Exit Commands");

        ImGui.Separator();

        DisplayCommands(stateNode, stateNode.WhileCommands, "while", "While Commands");

        ImGui.Separator();

        DisplayConditions(stateNode, stateNode.Conditions, "conditions", "Conditions");

        ImGui.Separator();
    }

    public void DisplayCommands(ESD.State node, List<CommandCall> commands, string imguiId, string displayTitle)
    {
        ImGui.Separator();
        UIHelper.WrappedText(displayTitle);
        ImGui.Separator();

        if (ImGui.BeginTable($"esdCommandTable_{imguiId}", 4, ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableSetupColumn("Column1", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Column2", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Column3", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Column4", ImGuiTableColumnFlags.WidthStretch);

            for (int i = 0; i < commands.Count; i++)
            {
                var cmd = commands[i];

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                ImGui.AlignTextToFramePadding();
                UIHelper.WrappedText("Command");

                ImGui.TableSetColumnIndex(1);

                DisplayCommandIdSection(node, commands, cmd, i);

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                ImGui.AlignTextToFramePadding();
                UIHelper.WrappedText("Parameters");

                DisplayerCommandParameterSection(node, commands, cmd, i);
            }

            ImGui.EndTable();
        }
    }

    private void DisplayCommandIdSection(ESD.State node, List<CommandCall> commands, CommandCall cmd, int imguiId)
    {
        var displayIdentifier = $"{cmd.CommandBank} [{cmd.CommandID}]";

        ImGui.AlignTextToFramePadding();
        UIHelper.WrappedText(displayIdentifier);

        ImGui.TableSetColumnIndex(2);

        // Go to State Group
        if (cmd.CommandBank == 6)
        {
            ImGui.AlignTextToFramePadding();
            if (ImGui.ArrowButton($"idJumpLinkButton{imguiId}", ImGuiDir.Right))
            {
                var targetStateGroup = cmd.CommandID;
                var groups = Editor.Selection.SelectedStateGroups;

                foreach (var (key, entry) in groups)
                {
                    if (key == targetStateGroup)
                    {
                        Editor.Selection.ClearStateGroupNode();
                        Editor.Selection.SelectStateGroupNode(key, entry);
                    }
                }
            }
            UIHelper.Tooltip("View this state group.");
        }

        ImGui.TableSetColumnIndex(3);

        // Alias
        var cmdMeta = Project.EzStateData.Meta.GetCommandMeta(cmd.CommandBank, cmd.CommandID);
        if (cmdMeta != null)
        {
            var displayAlias = cmdMeta.displayName;
            ImGui.AlignTextToFramePadding();
            UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, displayAlias);
        }
    }

    private void DisplayerCommandParameterSection(ESD.State node, List<CommandCall> commands, CommandCall cmd, int imguiId)
    {
        var cmdArgMeta = Project.EzStateData.Meta.GetCommandArgMeta(cmd.CommandBank, cmd.CommandID);

        for (int i = 0; i < cmd.Arguments.Count; i++)
        {
            var arg = cmd.Arguments[i];

            var expr = EzInfixor.BytecodeToInfix(arg);

            ImGui.TableSetColumnIndex(1);

            // Value expr
            if (expr is ConstExpr constExpr)
            {
                ImGui.AlignTextToFramePadding();
                UIHelper.WrappedText($"{constExpr}");

                ImGui.TableSetColumnIndex(2);

                // Display quick-link button if applicable
                if (cmdArgMeta.Count > i)
                {
                    var argMeta = cmdArgMeta[i];

                    DisplayGoToButton(argMeta, constExpr, $"{imguiId}{i}");
                }

                ImGui.TableSetColumnIndex(3);

                // Display alias
                if (cmdArgMeta.Count > i)
                {
                    var argMeta = cmdArgMeta[i];

                    var displayAlias = argMeta.displayName;
                    ImGui.AlignTextToFramePadding();
                    UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, displayAlias);
                }
            }

            ImGui.TableNextRow();
        }
    }

    private void DisplayGoToButton(EsdMeta_Arg argMeta, ConstExpr expr, string imguiId)
    {
        if (argMeta.argLink != null)
        {
            ImGui.AlignTextToFramePadding();
            if (ImGui.ArrowButton($"parameterJumpLinkButton{imguiId}", ImGuiDir.Right))
            {
                var linkExpression = argMeta.argLink.Split("/");
                var source = linkExpression[0];
                var target = linkExpression[1];

                // ESD
                if (source == "esd")
                {
                    if (target == "StateGroup")
                    {
                        var targetStateGroup = expr.AsInt();
                        var groups = Editor.Selection.SelectedStateGroups;

                        foreach (var (key, entry) in groups)
                        {
                            if (key == targetStateGroup)
                            {
                                Editor.Selection.ClearStateGroupNode();
                                Editor.Selection.SelectStateGroupNode(key, entry);
                            }
                        }
                    }
                }

                // PARAM
                if (source == "param")
                {
                    var paramName = target;
                    var rowID = expr.AsInt();

                    EditorCommandQueue.AddCommand($@"param/select/-1/{paramName}/{rowID}");
                }

                // EMEVD
                if (source == "emevd")
                {
                    var mapName = Editor.Selection.SelectedFilename;
                    var eventId = target;

                    //EditorCommandQueue.AddCommand($@"param/select/-1/{paramName}/{rowID}");
                }
            }
            UIHelper.Tooltip("View this in its associated editor.");
        }
    }

    public void DisplayConditions(ESD.State node, List<Condition> conditions, string imguiId, string displayTitle)
    {
        ImGui.Separator();
        UIHelper.WrappedText(displayTitle);
        ImGui.Separator();

        if (ImGui.BeginTable($"esdConditionTable_{imguiId}", 4, ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableSetupColumn("Column1", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Column2", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Column3", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Column4", ImGuiTableColumnFlags.WidthStretch);

            for (int i = 0; i < conditions.Count; i++)
            {
                var cond = conditions[i];

                DisplayCondition(node, conditions, cond, $"{imguiId}{i}");
            }

            ImGui.EndTable();
        }
    }

    private void DisplayCondition(ESD.State node, List<Condition> conditions, Condition cond, string imguiId)
    {
        ImGui.TableNextRow();

        DisplayCondition_TargetState(node, conditions, cond, imguiId);

        ImGui.TableNextRow();

        DisplayCondition_Evaluator(node, conditions, cond, imguiId);

        ImGui.TableNextRow();

        DisplayCondition_PassCommands(node, conditions, cond, imguiId);

        ImGui.TableNextRow();

        DisplayCondition_Subconditions(node, conditions, cond, imguiId);
    }

    private void DisplayCondition_TargetState(ESD.State node, List<Condition> conditions, Condition cond, string imguiId)
    {
        ImGui.TableSetColumnIndex(0);

        ImGui.AlignTextToFramePadding();
        UIHelper.WrappedText("Target State");

        ImGui.TableSetColumnIndex(1);

        ImGui.Text($"{cond.TargetState}");
    }

    private void DisplayCondition_Evaluator(ESD.State node, List<Condition> conditions, Condition cond, string imguiId)
    {
        ImGui.TableSetColumnIndex(0);

        ImGui.AlignTextToFramePadding();
        UIHelper.WrappedText("Evaluator");

        ImGui.TableSetColumnIndex(1);

        var expr = EzInfixor.BytecodeToInfix(cond.Evaluator);

        //ImGui.Text($"{expr.GetType()}");

        // Value expr
        if (expr is ConstExpr constExpr)
        {
            ImGui.AlignTextToFramePadding();
            UIHelper.WrappedText($"{constExpr}");

            ImGui.TableSetColumnIndex(2);

            // Display quick-link button if applicable

            ImGui.TableSetColumnIndex(3);

            // Display alias
        }

        // Binary expr
        if (expr is BinaryExpr binaryExpr)
        {
            ImGui.AlignTextToFramePadding();
            UIHelper.WrappedText($"{binaryExpr}");

            ImGui.TableSetColumnIndex(2);

            // Display quick-link button if applicable

            ImGui.TableSetColumnIndex(3);

            // Display alias
        }
    }
    private void DisplayCondition_PassCommands(ESD.State node, List<Condition> conditions, Condition cond, string imguiId)
    {
        ImGui.TableSetColumnIndex(0);

        ImGui.AlignTextToFramePadding();
        UIHelper.WrappedText("Pass Commands");

        ImGui.TableSetColumnIndex(1);

        foreach (var passCmd in cond.PassCommands)
        {
            var displayIdentifier = $"{passCmd.CommandBank} [{passCmd.CommandID}]";

            ImGui.AlignTextToFramePadding();
            UIHelper.WrappedText(displayIdentifier);

            ImGui.TableSetColumnIndex(2);

            // Go to State Group
            if (passCmd.CommandBank == 6)
            {
                ImGui.AlignTextToFramePadding();
                if (ImGui.ArrowButton($"idJumpLinkButton{imguiId}", ImGuiDir.Right))
                {
                    var targetStateGroup = passCmd.CommandID;
                    var groups = Editor.Selection.SelectedStateGroups;

                    foreach (var (key, entry) in groups)
                    {
                        if (key == targetStateGroup)
                        {
                            Editor.Selection.ClearStateGroupNode();
                            Editor.Selection.SelectStateGroupNode(key, entry);
                        }
                    }
                }
                UIHelper.Tooltip("View this state group.");
            }

            ImGui.TableSetColumnIndex(3);

            // Alias
            var cmdMeta = Project.EzStateData.Meta.GetCommandMeta(passCmd.CommandBank, passCmd.CommandID);
            if (cmdMeta != null)
            {
                var displayAlias = cmdMeta.displayName;
                ImGui.AlignTextToFramePadding();
                UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, displayAlias);
            }

            ImGui.TableNextRow();

            var cmdArgMeta = Project.EzStateData.Meta.GetCommandArgMeta(passCmd.CommandBank, passCmd.CommandID);

            for (int i = 0; i < passCmd.Arguments.Count; i++)
            {
                var arg = passCmd.Arguments[i];

                var expr = EzInfixor.BytecodeToInfix(arg);

                ImGui.TableSetColumnIndex(1);

                // Value expr
                if (expr is ConstExpr constExpr)
                {
                    ImGui.AlignTextToFramePadding();
                    UIHelper.WrappedText($"{constExpr}");

                    ImGui.TableSetColumnIndex(2);

                    // Display quick-link button if applicable
                    if (cmdArgMeta.Count > i)
                    {
                        var argMeta = cmdArgMeta[i];

                        DisplayGoToButton(argMeta, constExpr, $"{imguiId}{i}");
                    }

                    ImGui.TableSetColumnIndex(3);

                    // Display alias
                    if (cmdArgMeta.Count > i)
                    {
                        var argMeta = cmdArgMeta[i];

                        var displayAlias = argMeta.displayName;
                        ImGui.AlignTextToFramePadding();
                        UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, displayAlias);
                    }
                }

                ImGui.TableNextRow();
            }
        }
    }
    private void DisplayCondition_Subconditions(ESD.State node, List<Condition> conditions, Condition cond, string imguiId)
    {
        ImGui.TableSetColumnIndex(0);

        ImGui.AlignTextToFramePadding();
        UIHelper.WrappedText("Subconditions");

        ImGui.TableSetColumnIndex(1);

        for (int i = 0; i < cond.Subconditions.Count; i++)
        {
            var subCond = conditions[i];

            ImGui.Text($"Sub Condition {i}");

            //DisplayCondition(node, conditions, subCond, $"{imguiId}{i}");
        }
    }
}
