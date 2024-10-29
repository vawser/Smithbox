using Google.Protobuf.WellKnownTypes;
using ImGuiNET;
using Org.BouncyCastle.Utilities;
using SoulsFormats;
using StudioCore.Editor;
using StudioCore.Editors.EsdEditor.Enums;
using StudioCore.Editors.EsdEditor.EsdLang;
using StudioCore.Editors.TextEditor;
using StudioCore.Interface;
using StudioCore.TalkEditor;
using StudioCore.Utilities;
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
        Selection.SwitchWindowContext(EsdEditorContext.StateNodeContents);

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

        if (ImGui.BeginTable($"esdCommandTable_{imguiId}", 4, ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableSetupColumn("Column1", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Column2", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Column3", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Column4", ImGuiTableColumnFlags.WidthStretch);

            for(int i = 0; i < commands.Count; i++) 
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

    /// <summary>
    /// Command ID
    /// </summary>
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
            UIHelper.ShowHoverTooltip("View this state group.");
        }

        ImGui.TableSetColumnIndex(3);

        // Alias
        var cmdMeta = EsdMeta.GetCommandMeta(cmd.CommandBank, cmd.CommandID);
        if (cmdMeta != null)
        {
            var displayAlias = cmdMeta.displayName;
            ImGui.AlignTextToFramePadding();
            UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, displayAlias);
        }
    }

    /// <summary>
    /// Command Parameters
    /// </summary>
    private void DisplayerCommandParameterSection(ESD.State node, List<CommandCall> commands, CommandCall cmd, int imguiId)
    {
        var cmdArgMeta = EsdMeta.GetCommandArgMeta(cmd.CommandBank, cmd.CommandID);

        for(int i = 0; i < cmd.Arguments.Count; i++)
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

                // EMEVD
                if (source == "emevd")
                {
                    var mapName = Selection._selectedFileInfo.Name;
                    var eventId = target;

                    //EditorCommandQueue.AddCommand($@"param/select/-1/{paramName}/{rowID}");
                }
            }
            UIHelper.ShowHoverTooltip("View this in its associated editor.");
        }
    }

    public void DisplayConditions(ESD.State node, string imguiId)
    {

    }
}
