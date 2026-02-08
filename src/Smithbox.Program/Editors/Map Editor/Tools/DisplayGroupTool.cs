using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

public class DisplayGroupTool
{
    private MapEditorView View;
    public ProjectEntry Project;

    private int _dispGroupCount = 8;
    private string _lastHoveredCheckbox;

    public readonly HashSet<string> HighlightedGroups = new();

    public bool selectHighlightsOperation = false;
    public bool DisplayHelpText = false;

    public DisplayGroupTool(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void SetupDrawgroupCount()
    {
        switch (View.Project.Descriptor.ProjectType)
        {
            // imgui checkbox click seems to break at some point after 8 (8*32) checkboxes, so let's just hope that never happens, yeah?
            case ProjectType.DES:
            case ProjectType.DS1:
            case ProjectType.DS1R:
            case ProjectType.DS2:
            case ProjectType.DS2S:
                _dispGroupCount = 4;
                break;
            case ProjectType.BB:
            case ProjectType.DS3:
                _dispGroupCount = 8;
                break;
            case ProjectType.SDT:
            case ProjectType.ER:
            case ProjectType.AC6:
            case ProjectType.NR:
                _dispGroupCount = 8; //?
                break;
            default:
                throw new Exception($"Error: Did not expect Gametype {View.Project.Descriptor.ProjectType}");
                //break;
        }
    }

    /// <summary>
    /// Shortcut
    /// </summary>
    public void OnShortcut()
    {

    }

    /// <summary>
    /// Tool Window
    /// </summary>
    public void OnToolWindow()
    {
        var windowWidth = ImGui.GetWindowWidth();

        uint[] sdrawgroups = null;
        uint[] sdispgroups = null;
        var sels = View.ViewportSelection.GetFilteredSelection<Entity>(e => e.HasRenderGroups);
        if (sels.Any())
        {
            sdrawgroups = sels.First().Drawgroups;
            sdispgroups = sels.First().Dispgroups;
        }

        if (View.ViewportHandler.ActiveViewport.RenderScene != null)
        {
            if (ImGui.CollapsingHeader("Render Groups"))
            {
                DisplayGroupsGUI(sdrawgroups, sdispgroups, sels);
            }
        }
    }

    public void DisplayGroupsGUI(uint[] sdrawgroups, uint[] sdispgroups, HashSet<Entity> sels)
    {
        DrawGroup dg = View.ViewportHandler.ActiveViewport.RenderScene.DisplayGroup;

        if (dg.AlwaysVisible || dg.RenderGroups.Length != _dispGroupCount)
        {
            dg.RenderGroups = new uint[_dispGroupCount];
            for (var i = 0; i < _dispGroupCount; i++)
            {
                dg.RenderGroups[i] = 0xFFFFFFFF;
            }

            dg.AlwaysVisible = false;
        }

        // Targeted Entities
        DisplayHeaderSection(sels);

        // Render Group Table
        DisplayGroupTable(dg, sdrawgroups, sdispgroups);

        // Actions
        DisplayFooterSection(dg, sdrawgroups, sdispgroups, sels);

        HandleShortcuts(dg, sdrawgroups, sdispgroups, sels);
    }

    public void HandleShortcuts(DrawGroup dg, uint[] sdrawgroups, uint[] sdispgroups, HashSet<Entity> sels)
    {
        // Show All
        if (InputManager.IsPressed(KeybindID.MapEditor_Show_All_Display_Groups))
        {
            for (var i = 0; i < _dispGroupCount; i++)
            {
                dg.RenderGroups[i] = 0xFFFFFFFF;
            }
        }

        // Hide All
        if (InputManager.IsPressed(KeybindID.MapEditor_Hide_All_Display_Groups))
        {
            for (var i = 0; i < _dispGroupCount; i++)
            {
                dg.RenderGroups[i] = 0;
            }
        }

        if (InputManager.IsPressed(KeybindID.MapEditor_View_Display_Group) && sdispgroups != null)
        {
            for (var i = 0; i < _dispGroupCount; i++)
            {
                dg.RenderGroups[i] = sdispgroups[i];
            }
        }

        if (InputManager.IsPressed(KeybindID.MapEditor_View_Draw_Group) && sdispgroups != null)
        {
            for (var i = 0; i < _dispGroupCount; i++)
            {
                dg.RenderGroups[i] = sdrawgroups[i];
            }
        }

        if (InputManager.IsPressed(KeybindID.MapEditor_Apply_Display_Group) && sdispgroups != null)
        {
            IEnumerable<uint[]> selDispGroups = sels.Select(s => s.Dispgroups);
            ArrayPropertyCopyAction action = new(dg.RenderGroups, selDispGroups);

            View.ViewportActionManager.ExecuteAction(action);
        }

        if (InputManager.IsPressed(KeybindID.MapEditor_Apply_Draw_Group) && sdispgroups != null)
        {
            IEnumerable<uint[]> selDrawGroups = sels.Select(s => s.Drawgroups);
            ArrayPropertyCopyAction action = new(dg.RenderGroups, selDrawGroups);

            View.ViewportActionManager.ExecuteAction(action);
        }

        if (InputManager.IsPressed(KeybindID.MapEditor_Select_Display_Group_Highlights))
        {
            selectHighlightsOperation = true;
        }
    }

    public void DisplayFooterSection(DrawGroup dg, uint[] sdrawgroups, uint[] sdispgroups, HashSet<Entity> sels)
    {
        var scale = DPI.UIScale();

        // Show All
        if (ImGui.Button($"Show All", DPI.StandardButtonSize))
        {
            for (var i = 0; i < _dispGroupCount; i++)
            {
                dg.RenderGroups[i] = 0xFFFFFFFF;
            }
        }
        UIHelper.Tooltip($"Show all display groups.\n{InputManager.GetHint(KeybindID.MapEditor_Show_All_Display_Groups)}");

        ImGui.SameLine();

        // Hide All
        if (ImGui.Button($"Hide All", DPI.StandardButtonSize))
        {
            for (var i = 0; i < _dispGroupCount; i++)
            {
                dg.RenderGroups[i] = 0;
            }
        }
        UIHelper.Tooltip($"Hide all display groups.\n{InputManager.GetHint(KeybindID.MapEditor_Hide_All_Display_Groups)}");

        // Get Display Group
        if (sdispgroups == null)
        {
            ImGui.BeginDisabled();
        }

        if (ImGui.Button($"Get Display Group", DPI.StandardButtonSize) && sdispgroups != null)
        {
            for (var i = 0; i < _dispGroupCount; i++)
            {
                dg.RenderGroups[i] = sdispgroups[i];
            }
        }
        UIHelper.Tooltip($"Get display group for current selection.\n{InputManager.GetHint(KeybindID.MapEditor_View_Display_Group)}");

        ImGui.SameLine();

        // Get Draw Group
        if (ImGui.Button($"Get Draw Group", DPI.StandardButtonSize) && sdispgroups != null)
        {
            for (var i = 0; i < _dispGroupCount; i++)
            {
                dg.RenderGroups[i] = sdrawgroups[i];
            }
        }
        UIHelper.Tooltip($"Get draw group for current selection.\n{InputManager.GetHint(KeybindID.MapEditor_View_Draw_Group)}");

        // Assign Display Group
        if (ImGui.Button($"Assign Display Group", DPI.StandardButtonSize) && sdispgroups != null)
        {
            IEnumerable<uint[]> selDispGroups = sels.Select(s => s.Dispgroups);
            ArrayPropertyCopyAction action = new(dg.RenderGroups, selDispGroups);

            View.ViewportActionManager.ExecuteAction(action);
        }
        UIHelper.Tooltip($"Assign display group for current selection.\n{InputManager.GetHint(KeybindID.MapEditor_Apply_Display_Group)}");

        ImGui.SameLine();

        // Assign Draw Group
        if (ImGui.Button($"Assign Draw Group", DPI.StandardButtonSize) && sdispgroups != null)
        {
            IEnumerable<uint[]> selDrawGroups = sels.Select(s => s.Drawgroups);
            ArrayPropertyCopyAction action = new(dg.RenderGroups, selDrawGroups);

            View.ViewportActionManager.ExecuteAction(action);
        }
        UIHelper.Tooltip($"Assign draw group for current selection.\n{InputManager.GetHint(KeybindID.MapEditor_Apply_Draw_Group)}");

        // Select Highlights
        if (sdispgroups == null)
        {
            ImGui.EndDisabled();
        }

        if (!HighlightedGroups.Any())
        {
            ImGui.BeginDisabled();
        }

        if (ImGui.Button("Select Highlights", DPI.StandardButtonSize))
        {
            selectHighlightsOperation = true;
        }
        UIHelper.Tooltip($"Select highlighted. Right-click to highlight a checkbox within the table section.\n{InputManager.GetHint(KeybindID.MapEditor_Select_Display_Group_Highlights)}");

        ImGui.SameLine();

        // Clear Highlights
        if (ImGui.Button("Clear Highlights", DPI.StandardButtonSize))
        {
            HighlightedGroups.Clear();
        }
        else if (!HighlightedGroups.Any())
        {
            ImGui.EndDisabled();
        }

        if (DisplayHelpText)
        {
            ImGui.Separator();

            UIHelper.WrappedText(
                "Render Groups are used by the game to determine what should render and what shouldn't.\n" +
                "They consist of Display Groups and Draw Groups.\n" +
                //"Display Groups: Determines which DrawGroups should render.\n" +
                //"Draw Groups: Determines if things will render when a DispGroup is active.\n" +
                "When a Display Group is active, Map Objects with that Draw Group will render.\n" +
                "\n" +
                "If a Map Object uses the CollisionName field, they will inherit Draw Groups from the referenced Map Object.\n" +
                "Also, CollisionName references will be targeted by Smithbox when using `Set Selection`/`Get Selection` instead of your actual selection.\n" +
                "When a character walks on top of a piece of collision, they will use its DispGroups and DrawGroups.\n" +
                "\n" +
                "Color indicates which Render Groups selected Map Object is using.\n" +
                "Red = Selection uses this Display Group.\n" +
                "Green = Selection uses this Draw Group.\n" +
                "Yellow = Selection uses both.\n" +
                "\n" +
                "(Note: there are unknown differences in Sekiro/ER/AC6)");
        }
    }

    public void DisplayHeaderSection(HashSet<Entity> sels)
    {
        void DisplayTargetEntities()
        {
            // Help
            if (ImGui.Button($"{Icons.LightbulbO}##renderGroupHelpText", DPI.IconButtonSize))
            {
                DisplayHelpText = !DisplayHelpText;
            }
            UIHelper.Tooltip("Toggle the help section.");
            ImGui.SameLine();

            string affectedEntsStr = string.Join(", ", sels.Select(s => s.RenderGroupRefName != "" ? s.RenderGroupRefName : s.Name));

            ImGui.Text($"Targets: {Utils.ImGui_WordWrapString(affectedEntsStr, ImGui.GetWindowWidth(), 99)}");
            ImGui.Separator();
        }

        if (sels.Count <= 5)
        {
            DisplayTargetEntities();
        }
        else
        {
            if (ImGui.CollapsingHeader("Targets##DisplayGroup"))
            {
                DisplayTargetEntities();
            }
        }
    }

    public void DisplayGroupTable(DrawGroup dg, uint[] sdrawgroups, uint[] sdispgroups)
    {
        int groupCount = dg.RenderGroups.Length;
        int bitCount = 32;

        if (ImGui.BeginTable("RenderGroupsTable", groupCount + 1, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
        {
            ImGui.TableSetupColumn("Group");
            for (int g = 0; g < groupCount; g++)
            {
                ImGui.TableSetupColumn($"{g}");
            }
            ImGui.TableHeadersRow();

            for (int i = 0; i < bitCount; i++)
            {
                ImGui.TableNextRow();

                ImGui.TableSetColumnIndex(0);
                ImGui.Text($"Bit {i}");

                for (int g = 0; g < groupCount; g++)
                {
                    ImGui.TableSetColumnIndex(g + 1);

                    var cellKey = $"{g}_{i}";
                    if (selectHighlightsOperation)
                    {
                        if (HighlightedGroups.Contains(cellKey))
                        {
                            dg.RenderGroups[g] |= 1u << i;
                        }
                        else
                        {
                            dg.RenderGroups[g] &= ~(1u << i);
                        }
                    }

                    var check = (dg.RenderGroups[g] >> i & 0x1) > 0;

                    var drawActive = sdrawgroups != null && (sdrawgroups[g] >> i & 0x1) > 0;
                    var dispActive = sdispgroups != null && (sdispgroups[g] >> i & 0x1) > 0;

                    // --- Color Handling ---
                    if (drawActive && dispActive)
                    {
                        ImGui.PushStyleColor(ImGuiCol.FrameBg, UI.Current.DisplayGroupEditor_CombinedActive_Frame);
                        ImGui.PushStyleColor(ImGuiCol.CheckMark, UI.Current.DisplayGroupEditor_CombinedActive_Checkbox);
                    }
                    else if (drawActive)
                    {
                        ImGui.PushStyleColor(ImGuiCol.FrameBg, UI.Current.DisplayGroupEditor_DrawActive_Frame);
                        ImGui.PushStyleColor(ImGuiCol.CheckMark, UI.Current.DisplayGroupEditor_DrawActive_Checkbox);
                    }
                    else if (dispActive)
                    {
                        ImGui.PushStyleColor(ImGuiCol.FrameBg, UI.Current.DisplayGroupEditor_DisplayActive_Frame);
                        ImGui.PushStyleColor(ImGuiCol.CheckMark, UI.Current.DisplayGroupEditor_DisplayActive_Checkbox);
                    }

                    if (HighlightedGroups.Contains(cellKey))
                    {
                        ImGui.PushStyleColor(ImGuiCol.Border, UI.Current.DisplayGroupEditor_Border_Highlight);
                    }

                    if (ImGui.Checkbox($"##cell_{cellKey}", ref check))
                    {
                        if (check)
                            dg.RenderGroups[g] |= 1u << i;
                        else
                            dg.RenderGroups[g] &= ~(1u << i);
                    }

                    if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenBlockedByActiveItem) &&
                        ImGui.IsMouseDown(ImGuiMouseButton.Left))
                    {
                        if (_lastHoveredCheckbox != cellKey)
                        {
                            if (check)
                                dg.RenderGroups[g] &= ~(1u << i);
                            else
                                dg.RenderGroups[g] |= 1u << i;
                            _lastHoveredCheckbox = cellKey;
                        }
                    }

                    if (HighlightedGroups.Contains(cellKey))
                    {
                        ImGui.PopStyleColor(); // Border
                    }

                    if (ImGui.IsItemHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Right))
                    {
                        // Toggle render group highlights
                        if (HighlightedGroups.Contains(cellKey))
                            HighlightedGroups.Remove(cellKey);
                        else
                            HighlightedGroups.Add(cellKey);
                    }

                    if (drawActive || dispActive)
                    {
                        ImGui.PopStyleColor(2);
                    }
                }
            }

            ImGui.EndTable();
        }
    }
}
