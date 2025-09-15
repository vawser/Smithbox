using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.MapEditor.Enums;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.Interface;
using StudioCore.Scene;
using StudioCore.Scene.Framework;
using StudioCore.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor.Tools.DisplayGroups;

public class DisplayGroupView
{
    private MapEditorScreen Editor;
    private ViewportActionManager EditorActionManager;
    private RenderScene RenderScene;
    private ViewportSelection Selection;

    private int _dispGroupCount = 8;
    private string _lastHoveredCheckbox;

    public readonly HashSet<string> HighlightedGroups = new();

    public bool selectHighlightsOperation = false;
    public bool DisplayHelpText = false;

    public DisplayGroupView(MapEditorScreen screen)
    {
        Editor = screen;
        EditorActionManager = screen.EditorActionManager;
        RenderScene = screen.MapViewportView.RenderScene;
        Selection = screen.ViewportSelection;
    }

    public void SetupDrawgroupCount()
    {
        switch (Editor.Project.ProjectType)
        {
            // imgui checkbox click seems to break at some point after 8 (8*32) checkboxes, so let's just hope that never happens, yeah?
            case ProjectType.DES:
            case ProjectType.DS1:
            case ProjectType.DS1R:
            case ProjectType.DS2:
            case ProjectType.DS2S:
            case ProjectType.AC4: // TODO unsure if this is correct
            case ProjectType.ACFA: // TODO unsure if this is correct
            case ProjectType.ACV: // TODO unsure if this is correct
            case ProjectType.ACVD: // TODO unsure if this is correct
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
                throw new Exception($"Error: Did not expect Gametype {Editor.Project.ProjectType}");
                //break;
        }
    }

    public void OnGui()
    {
        var scale = DPI.UIScale();

        if (!CFG.Current.Interface_MapEditor_RenderGroups)
            return;

        ImGui.SetNextWindowSize(new Vector2(100, 100) * scale);

        uint[] sdrawgroups = null;
        uint[] sdispgroups = null;
        var sels = Selection.GetFilteredSelection<Entity>(e => e.HasRenderGroups);
        if (sels.Any())
        {
            sdrawgroups = sels.First().Drawgroups;
            sdispgroups = sels.First().Dispgroups;
        }

        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_GetDisplayGroup)
            || InputTracker.GetKeyDown(KeyBindings.Current.MAP_GetDrawGroup)
            || InputTracker.GetKeyDown(KeyBindings.Current.MAP_SetDisplayGroup)
            || InputTracker.GetKeyDown(KeyBindings.Current.MAP_SetDrawGroup)
            || InputTracker.GetKeyDown(KeyBindings.Current.MAP_ShowAllDisplayGroups)
            || InputTracker.GetKeyDown(KeyBindings.Current.MAP_HideAllDisplayGroups)
            || InputTracker.GetKeyDown(KeyBindings.Current.MAP_SelectDisplayGroupHighlights))
        {
            ImGui.SetNextWindowFocus();
        }

        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(4.0f, 2.0f) * scale);
        if (ImGui.Begin("Render Groups") && RenderScene != null)
        {
            Editor.FocusManager.SwitchWindowContext(MapEditorContext.RenderGroups);

            DisplayGroupsGUI(sdrawgroups, sdispgroups, sels);
        }
        ImGui.PopStyleVar();
        ImGui.End();
    }

    public void DisplayGroupsGUI(uint[] sdrawgroups, uint[] sdispgroups, HashSet<Entity> sels)
    {
        DrawGroup dg = RenderScene.DisplayGroup;

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
    }

    public void DisplayFooterSection(DrawGroup dg, uint[] sdrawgroups, uint[] sdispgroups, HashSet<Entity> sels)
    {
        var scale = DPI.UIScale();

        // Show All
        if (ImGui.Button($"Show All", DPI.StandardButtonSize)
            || InputTracker.GetKeyDown(KeyBindings.Current.MAP_ShowAllDisplayGroups))
        {
            for (var i = 0; i < _dispGroupCount; i++)
            {
                dg.RenderGroups[i] = 0xFFFFFFFF;
            }
        }
        UIHelper.Tooltip($"Show all display groups.\n{KeyBindings.Current.MAP_ShowAllDisplayGroups.HintText}");

        ImGui.SameLine();

        // Hide All
        if (ImGui.Button($"Hide All", DPI.StandardButtonSize)
            || InputTracker.GetKeyDown(KeyBindings.Current.MAP_HideAllDisplayGroups))
        {
            for (var i = 0; i < _dispGroupCount; i++)
            {
                dg.RenderGroups[i] = 0;
            }
        }
        UIHelper.Tooltip($"Hide all display groups.\n{KeyBindings.Current.MAP_HideAllDisplayGroups.HintText}");

        // Get Display Group
        if (sdispgroups == null)
        {
            ImGui.BeginDisabled();
        }

        if (ImGui.Button($"Get Display Group", DPI.StandardButtonSize)
            || InputTracker.GetKeyDown(KeyBindings.Current.MAP_GetDisplayGroup)
                && sdispgroups != null)
        {
            for (var i = 0; i < _dispGroupCount; i++)
            {
                dg.RenderGroups[i] = sdispgroups[i];
            }
        }
        UIHelper.Tooltip($"Get display group for current selection.\n{KeyBindings.Current.MAP_GetDisplayGroup.HintText}");

        ImGui.SameLine();

        // Get Draw Group
        if (ImGui.Button($"Get Draw Group", DPI.StandardButtonSize)
            || InputTracker.GetKeyDown(KeyBindings.Current.MAP_GetDrawGroup)
                && sdispgroups != null)
        {
            for (var i = 0; i < _dispGroupCount; i++)
            {
                dg.RenderGroups[i] = sdrawgroups[i];
            }
        }
        UIHelper.Tooltip($"Get draw group for current selection.\n{KeyBindings.Current.MAP_GetDrawGroup.HintText}");

        // Assign Display Group
        if (ImGui.Button($"Assign Display Group", DPI.StandardButtonSize)
            || InputTracker.GetKeyDown(KeyBindings.Current.MAP_SetDisplayGroup)
                && sdispgroups != null)
        {
            IEnumerable<uint[]> selDispGroups = sels.Select(s => s.Dispgroups);
            ArrayPropertyCopyAction action = new(dg.RenderGroups, selDispGroups);
            EditorActionManager.ExecuteAction(action);
        }
        UIHelper.Tooltip($"Assign display group for current selection.\n{KeyBindings.Current.MAP_SetDisplayGroup.HintText}");

        ImGui.SameLine();

        // Assign Draw Group
        if (ImGui.Button($"Assign Draw Group",
            DPI.StandardButtonSize)
            || InputTracker.GetKeyDown(KeyBindings.Current.MAP_SetDrawGroup)
                && sdispgroups != null)
        {
            IEnumerable<uint[]> selDrawGroups = sels.Select(s => s.Drawgroups);
            ArrayPropertyCopyAction action = new(dg.RenderGroups, selDrawGroups);
            EditorActionManager.ExecuteAction(action);
        }
        UIHelper.Tooltip($"Assign draw group for current selection.\n{KeyBindings.Current.MAP_SetDrawGroup.HintText}");

        // Select Highlights
        if (sdispgroups == null)
        {
            ImGui.EndDisabled();
        }

        if (!HighlightedGroups.Any())
        {
            ImGui.BeginDisabled();
        }

        if (ImGui.Button("Select Highlights", DPI.StandardButtonSize)
            || InputTracker.GetKeyDown(KeyBindings.Current.MAP_SelectDisplayGroupHighlights))
        {
            selectHighlightsOperation = true;
        }
        UIHelper.Tooltip($"Select highlighted. Right-click to highlight a checkbox within the table section.\n{KeyBindings.Current.MAP_SelectDisplayGroupHighlights.HintText}");

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
