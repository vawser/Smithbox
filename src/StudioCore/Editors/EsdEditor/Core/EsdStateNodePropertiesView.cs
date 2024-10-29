using Google.Protobuf.WellKnownTypes;
using ImGuiNET;
using Org.BouncyCastle.Utilities;
using SoulsFormats;
using StudioCore.Editor;
using StudioCore.Editors.EsdEditor.EsdLang;
using StudioCore.Editors.TextEditor;
using StudioCore.Interface;
using StudioCore.TalkEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static SoulsFormats.ESD;
using static StudioCore.Editors.EsdEditor.EsdLang.AST;

namespace StudioCore.Editors.EsdEditor;

/// <summary>
/// Handles the state node properties viewing and editing.
/// </summary>
public class EsdStateNodePropertyView
{
    private EsdEditorScreen Screen;
    private EsdPropertyDecorator Decorator;
    private EsdSelectionManager Selection;

    public EsdStateNodePropertyView(EsdEditorScreen screen)
    {
        Screen = screen;
        Decorator = screen.Decorator;
        Selection = screen.Selection;
    }

    /// <summary>
    /// Reset view state on project change
    /// </summary>
    public void OnProjectChanged()
    {

    }

    /// <summary>
    /// The main UI for the view
    /// </summary>
    public void Display()
    {
        ImGui.Begin("State Node##EsdStateNodePropertyView");

        var stateNode = Selection._selectedStateGroupNode;

        if (stateNode != null)
        {
            DisplayCommands(stateNode, stateNode.EntryCommands, "entry", "Entry Commands");

            ImGui.Separator();

            DisplayCommands(stateNode, stateNode.ExitCommands, "exit", "Exit Commands");

            ImGui.Separator();

            DisplayCommands(stateNode, stateNode.WhileCommands, "while", "While Commands");

            ImGui.Separator();

            DisplayConditions(stateNode, "conditions");

            ImGui.Separator();
        }

        ImGui.End();
    }

    public void DisplayCommands(ESD.State node, List<CommandCall> commands, string imguiId, string displayTitle)
    {
        ImGui.Separator();
        UIHelper.WrappedText(displayTitle);
        ImGui.Separator();

        if (ImGui.BeginTable($"esdCommandTable_{imguiId}", 2, ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableSetupColumn("CommandIdentifier", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("CommandParameters", ImGuiTableColumnFlags.WidthStretch);

            foreach (var cmd in commands)
            {
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                ImGui.AlignTextToFramePadding();
                UIHelper.WrappedText("Command");

                ImGui.TableSetColumnIndex(1);

                DisplayCommandIdSection(node, commands, cmd);

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                ImGui.AlignTextToFramePadding();
                UIHelper.WrappedText("Parameters");

                ImGui.TableSetColumnIndex(1);

                DisplayerCommandParameterSection(node, commands, cmd);
            }

            ImGui.EndTable();
        }
    }

    /// <summary>
    /// Command ID
    /// </summary>
    private void DisplayCommandIdSection(ESD.State node, List<CommandCall> commands, CommandCall cmd)
    {
        var displayIdentifier = $"{cmd.CommandBank} [{cmd.CommandID}]";

        ImGui.AlignTextToFramePadding();
        UIHelper.WrappedText(displayIdentifier);

        var cmdMeta = EsdMeta.GetCommandMeta(cmd.CommandBank, cmd.CommandID);
        if (cmdMeta != null)
        {
            var displayAlias = cmdMeta.displayName;
            ImGui.AlignTextToFramePadding();
            UIHelper.DisplayAlias(displayAlias);
        }

        // Go to State Group
        if (cmd.CommandBank == 6)
        {
            ImGui.SameLine();
            ImGui.AlignTextToFramePadding();
            if (ImGui.Button("Go To", new Vector2(150 * DPI.GetUIScale(), 24 * DPI.GetUIScale())))
            {
                var targetStateGroup = cmd.CommandID;
                var groups = Selection._selectedEsdScript.StateGroups;

                foreach(var (key, entry) in groups)
                {
                    if (key == targetStateGroup)
                    {
                        Selection.ResetStateGroupNode();
                        Selection.SetStateGroup(key, entry);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Command Parameters
    /// </summary>
    private void DisplayerCommandParameterSection(ESD.State node, List<CommandCall> commands, CommandCall cmd)
    {
        var cmdArgMeta = EsdMeta.GetCommandArgMeta(cmd.CommandBank, cmd.CommandID);

        for(int i = 0; i < cmd.Arguments.Count; i++)
        {
            var arg = cmd.Arguments[i];

            var expr = EzInfixor.BytecodeToInfix(arg);

            // Value expr
            if(expr is ConstExpr constExpr)
            {
                ImGui.AlignTextToFramePadding();
                UIHelper.WrappedText($"{constExpr}");

                if (cmdArgMeta.Count > i)
                {
                    var argMeta = cmdArgMeta[i];

                    var displayAlias = argMeta.displayName;
                    ImGui.AlignTextToFramePadding();
                    UIHelper.DisplayAlias(displayAlias);

                    // Display quick-link button if applicable
                    DisplayGoToButton(argMeta, constExpr);
                }
            }
        }
    }

    private void DisplayGoToButton(EsdMeta_Arg argMeta, ConstExpr expr)
    {
        if (argMeta.argLink != null)
        {
            ImGui.SameLine();
            ImGui.AlignTextToFramePadding();
            if (ImGui.Button("Go To", new Vector2(150 * DPI.GetUIScale(), 24 * DPI.GetUIScale())))
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
                        var groups = Selection._selectedEsdScript.StateGroups;

                        foreach (var (key, entry) in groups)
                        {
                            if (key == targetStateGroup)
                            {
                                Selection.ResetStateGroupNode();
                                Selection.SetStateGroup(key, entry);
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
            }
        }
    }


    public void DisplayConditions(ESD.State node, string imguiId)
    {

    }
}
