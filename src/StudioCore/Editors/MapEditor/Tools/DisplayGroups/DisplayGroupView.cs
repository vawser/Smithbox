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
                _dispGroupCount = 8; //?
                break;
            default:
                throw new Exception($"Error: Did not expect Gametype {Editor.Project.ProjectType}");
                //break;
        }
    }

    public void OnGui()
    {
        var scale = DPI.GetUIScale();

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

            if (ImGui.Button($"Show All <{KeyBindings.Current.MAP_ShowAllDisplayGroups.HintText}>")
                || InputTracker.GetKeyDown(KeyBindings.Current.MAP_ShowAllDisplayGroups))
            {
                for (var i = 0; i < _dispGroupCount; i++)
                {
                    dg.RenderGroups[i] = 0xFFFFFFFF;
                }
            }

            ImGui.SameLine(0.0f, 6.0f * scale);
            if (ImGui.Button($"Hide All <{KeyBindings.Current.MAP_HideAllDisplayGroups.HintText}>")
                || InputTracker.GetKeyDown(KeyBindings.Current.MAP_HideAllDisplayGroups))
            {
                for (var i = 0; i < _dispGroupCount; i++)
                {
                    dg.RenderGroups[i] = 0;
                }
            }

            ImGui.SameLine(0.0f, 12.0f * scale);
            if (sdispgroups == null)
            {
                ImGui.BeginDisabled();
            }

            if (ImGui.Button($"Get Disp <{KeyBindings.Current.MAP_GetDisplayGroup.HintText}>")
                || InputTracker.GetKeyDown(KeyBindings.Current.MAP_GetDisplayGroup)
                    && sdispgroups != null)
            {
                for (var i = 0; i < _dispGroupCount; i++)
                {
                    dg.RenderGroups[i] = sdispgroups[i];
                }
            }

            ImGui.SameLine(0.0f, 6.0f * scale);
            if (ImGui.Button($"Get Draw <{KeyBindings.Current.MAP_GetDrawGroup.HintText}>")
                || InputTracker.GetKeyDown(KeyBindings.Current.MAP_GetDrawGroup)
                    && sdispgroups != null)
            {
                for (var i = 0; i < _dispGroupCount; i++)
                {
                    dg.RenderGroups[i] = sdrawgroups[i];
                }
            }

            ImGui.SameLine(0.0f, 12.0f * scale);
            if (ImGui.Button($"Assign Draw <{KeyBindings.Current.MAP_SetDrawGroup.HintText}>")
                || InputTracker.GetKeyDown(KeyBindings.Current.MAP_SetDrawGroup)
                    && sdispgroups != null)
            {
                IEnumerable<uint[]> selDrawGroups = sels.Select(s => s.Drawgroups);
                ArrayPropertyCopyAction action = new(dg.RenderGroups, selDrawGroups);
                EditorActionManager.ExecuteAction(action);
            }

            ImGui.SameLine(0.0f, 6.0f * scale);
            if (ImGui.Button($"Assign Disp <{KeyBindings.Current.MAP_SetDisplayGroup.HintText}>")
                || InputTracker.GetKeyDown(KeyBindings.Current.MAP_SetDisplayGroup)
                    && sdispgroups != null)
            {
                IEnumerable<uint[]> selDispGroups = sels.Select(s => s.Dispgroups);
                ArrayPropertyCopyAction action = new(dg.RenderGroups, selDispGroups);
                EditorActionManager.ExecuteAction(action);
            }

            if (sdispgroups == null)
            {
                ImGui.EndDisabled();
            }

            ImGui.SameLine(0.0f, 12.0f * scale);
            if (!HighlightedGroups.Any())
            {
                ImGui.BeginDisabled();
            }

            bool selectHighlightsOperation = false;
            if (ImGui.Button("Select Highlights")
                || InputTracker.GetKeyDown(KeyBindings.Current.MAP_SelectDisplayGroupHighlights))
            {
                selectHighlightsOperation = true;
            }

            ImGui.SameLine(0.0f, 8.0f * scale);
            if (ImGui.Button("Clear Highlights"))
            {
                HighlightedGroups.Clear();
            }
            else if (!HighlightedGroups.Any())
            {
                ImGui.EndDisabled();
            }

            ImGui.SameLine(0.0f, 8.0f * scale);
            if (ImGui.Button("Help"))
            {
                ImGui.OpenPopup("##RenderHelp");
            }

            if (ImGui.BeginPopup("##RenderHelp"))
            {
                ImGui.Text(
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
                ImGui.EndPopup();
            }


            void DisplayTargetEntities()
            {
                string affectedEntsStr = string.Join(", ", sels.Select(s => s.RenderGroupRefName != "" ? s.RenderGroupRefName : s.Name));
                ImGui.Text($"Targets: {Utils.ImGui_WordWrapString(affectedEntsStr, ImGui.GetWindowWidth(), 99)}");
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

            ImGui.Separator();
            ImGui.BeginChild("##DispTicks", new Vector2());
            for (var g = 0; g < dg.RenderGroups.Length; g++)
            {
                // Row (groups)

                // Add spacing every 4 rows
                if (g % 4 == 0 && g > 0)
                {
                    ImGui.Spacing();
                }

                ImGui.Text($@"Group {g}:");
                for (var i = 0; i < 32; i++)
                {
                    // Column
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

                    // Add spacing every 4 columns
                    if (i % 4 == 0 && i > 0)
                    {
                        ImGui.SameLine();
                        ImGui.Spacing();
                    }

                    ImGui.SameLine();

                    var drawActive = sdrawgroups != null && (sdrawgroups[g] >> i & 0x1) > 0;
                    var dispActive = sdispgroups != null && (sdispgroups[g] >> i & 0x1) > 0;

                    if (drawActive && dispActive)
                    {
                        // Selection dispgroup and drawgroup is ticked
                        // Yellow
                        ImGui.PushStyleColor(ImGuiCol.FrameBg, UI.Current.DisplayGroupEditor_CombinedActive_Frame);
                        ImGui.PushStyleColor(ImGuiCol.CheckMark, UI.Current.DisplayGroupEditor_CombinedActive_Checkbox);
                    }
                    else if (drawActive)
                    {
                        // Selection drawgroup is ticked
                        // Green
                        ImGui.PushStyleColor(ImGuiCol.FrameBg, UI.Current.DisplayGroupEditor_DrawActive_Frame);
                        ImGui.PushStyleColor(ImGuiCol.CheckMark, UI.Current.DisplayGroupEditor_DrawActive_Checkbox);
                    }
                    else if (dispActive)
                    {
                        // Selection dispGroup is ticked
                        // Red
                        ImGui.PushStyleColor(ImGuiCol.FrameBg, UI.Current.DisplayGroupEditor_DisplayActive_Frame);
                        ImGui.PushStyleColor(ImGuiCol.CheckMark, UI.Current.DisplayGroupEditor_DisplayActive_Checkbox);
                    }
                    if (HighlightedGroups.Contains(cellKey))
                    {
                        ImGui.PushStyleColor(ImGuiCol.Border, UI.Current.DisplayGroupEditor_Border_Highlight);
                    }

                    if (ImGui.Checkbox($@"##cell_{cellKey}", ref check))
                    {
                        if (check)
                        {
                            dg.RenderGroups[g] |= 1u << i;
                        }
                        else
                        {
                            dg.RenderGroups[g] &= ~(1u << i);
                        }
                    }

                    if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenBlockedByActiveItem) && ImGui.IsMouseDown(ImGuiMouseButton.Left))
                    {
                        if (_lastHoveredCheckbox != cellKey)
                        {
                            if (check)
                            {
                                dg.RenderGroups[g] &= ~(1u << i);
                            }
                            else
                            {
                                dg.RenderGroups[g] |= 1u << i;
                            }
                            _lastHoveredCheckbox = cellKey;
                        }
                    }

                    if (HighlightedGroups.Contains(cellKey))
                    {
                        ImGui.PopStyleColor(1);
                    }

                    if (ImGui.IsItemHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Right))
                    {
                        // Toggle render group highlights
                        if (HighlightedGroups.Contains(cellKey))
                        {
                            HighlightedGroups.Remove(cellKey);
                        }
                        else
                        {
                            HighlightedGroups.Add(cellKey);
                        }
                    }

                    if (drawActive || dispActive)
                    {
                        ImGui.PopStyleColor(2);
                    }
                }
            }

            ImGui.EndChild();
        }
        ImGui.PopStyleVar();
        ImGui.End();
    }
}
