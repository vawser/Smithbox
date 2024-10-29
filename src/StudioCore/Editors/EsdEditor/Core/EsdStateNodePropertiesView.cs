using ImGuiNET;
using Org.BouncyCastle.Utilities;
using SoulsFormats;
using StudioCore.Editor;
using StudioCore.Editors.TextEditor;
using StudioCore.Interface;
using StudioCore.TalkEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.ESD;

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

                UIHelper.WrappedText("Command");

                ImGui.TableSetColumnIndex(1);

                DisplayCommandIdSection(node, commands, cmd);

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                DisplayerCommandParameterSection(node, commands, cmd);
            }

            ImGui.EndTable();
        }
    }

    private void DisplayCommandIdSection(ESD.State node, List<CommandCall> commands, CommandCall cmd)
    {
        var displayIdentifier = $"{cmd.CommandBank} [{cmd.CommandID}]";

        UIHelper.WrappedText(displayIdentifier);

        var cmdMeta = EsdMeta.GetCommandMeta(cmd.CommandBank, cmd.CommandID);
        if (cmdMeta != null)
        {
            var displayAlias = cmdMeta.displayName;
            UIHelper.DisplayAlias(displayAlias);
        }

        ImGui.SameLine();

        // Go to State Group
        if (cmd.CommandBank == 6)
        {
            if (ImGui.Button("Go To", new Vector2(80 * DPI.GetUIScale(), 18 * DPI.GetUIScale())))
            {
                var targetStateGroup = cmd.CommandID;
                var groups = Selection._selectedEsdScript.StateGroups;

                foreach(var (key, entry) in groups)
                {
                    if (key == targetStateGroup)
                    {
                        Selection.SetStateGroup(key, entry);
                    }
                }
            }
        }
    }

    private void DisplayerCommandParameterSection(ESD.State node, List<CommandCall> commands, CommandCall cmd)
    {
        UIHelper.WrappedText("Parameters");

        ImGui.TableSetColumnIndex(1);

        foreach (var arg in cmd.Arguments)
        {
            UIHelper.WrappedText(BitConverter.ToString(arg));
        }
    }

    public void DisplayConditions(ESD.State node, string imguiId)
    {

    }
}
