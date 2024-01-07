using ImGuiNET;
using StudioCore.Scene;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.MsbEditor;

public class DisplayGroupsEditor
{
    private readonly ActionManager _actionManager;

    private readonly RenderScene _scene;
    private readonly Selection _selection;
    public readonly HashSet<string> HighlightedGroups = new();

    public DisplayGroupsEditor(RenderScene scene, Selection sel, ActionManager manager)
    {
        _scene = scene;
        _selection = sel;
        _actionManager = manager;
    }

    public void OnGui(int dispCount)
    {
        var scale = MapStudioNew.GetUIScale();
        ImGui.SetNextWindowSize(new Vector2(100, 100) * scale);

        uint[] sdrawgroups = null;
        uint[] sdispgroups = null;
        var sels = _selection.GetFilteredSelection<Entity>(e => e.HasRenderGroups);
        if (sels.Any())
        {
            sdrawgroups = sels.First().Drawgroups;
            sdispgroups = sels.First().Dispgroups;
        }

        if (InputTracker.GetKeyDown(KeyBindings.Current.Map_RenderGroup_GetDisp)
            || InputTracker.GetKeyDown(KeyBindings.Current.Map_RenderGroup_GetDraw)
            || InputTracker.GetKeyDown(KeyBindings.Current.Map_RenderGroup_GiveDisp)
            || InputTracker.GetKeyDown(KeyBindings.Current.Map_RenderGroup_GiveDraw)
            || InputTracker.GetKeyDown(KeyBindings.Current.Map_RenderGroup_ShowAll)
            || InputTracker.GetKeyDown(KeyBindings.Current.Map_RenderGroup_HideAll)
            || InputTracker.GetKeyDown(KeyBindings.Current.Map_RenderGroup_SelectHighlights))
        {
            ImGui.SetNextWindowFocus();
        }
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(4.0f, 2.0f) * scale);
        if (ImGui.Begin("Render Groups") && _scene != null)
        {
            DrawGroup dg = _scene.DisplayGroup;
            if (dg.AlwaysVisible || dg.RenderGroups.Length != dispCount)
            {
                dg.RenderGroups = new uint[dispCount];
                for (var i = 0; i < dispCount; i++)
                {
                    dg.RenderGroups[i] = 0xFFFFFFFF;
                }

                dg.AlwaysVisible = false;
            }

            if (ImGui.Button($"Show All <{KeyBindings.Current.Map_RenderGroup_ShowAll.HintText}>")
                || InputTracker.GetKeyDown(KeyBindings.Current.Map_RenderGroup_ShowAll))
            {
                for (var i = 0; i < dispCount; i++)
                {
                    dg.RenderGroups[i] = 0xFFFFFFFF;
                }
            }

            ImGui.SameLine(0.0f, 6.0f * scale);
            if (ImGui.Button($"Hide All <{KeyBindings.Current.Map_RenderGroup_HideAll.HintText}>")
                || InputTracker.GetKeyDown(KeyBindings.Current.Map_RenderGroup_HideAll))
            {
                for (var i = 0; i < dispCount; i++)
                {
                    dg.RenderGroups[i] = 0;
                }
            }

            ImGui.SameLine(0.0f, 12.0f * scale);
            if (sdispgroups == null)
            {
                ImGui.BeginDisabled();
            }

            if (ImGui.Button($"Get Disp <{KeyBindings.Current.Map_RenderGroup_GetDisp.HintText}>")
                || (InputTracker.GetKeyDown(KeyBindings.Current.Map_RenderGroup_GetDisp)
                    && sdispgroups != null))
            {
                for (var i = 0; i < dispCount; i++)
                {
                    dg.RenderGroups[i] = sdispgroups[i];
                }
            }

            ImGui.SameLine(0.0f, 6.0f * scale);
            if (ImGui.Button($"Get Draw <{KeyBindings.Current.Map_RenderGroup_GetDraw.HintText}>")
                || (InputTracker.GetKeyDown(KeyBindings.Current.Map_RenderGroup_GetDraw)
                    && sdispgroups != null))
            {
                for (var i = 0; i < dispCount; i++)
                {
                    dg.RenderGroups[i] = sdrawgroups[i];
                }
            }

            ImGui.SameLine(0.0f, 12.0f * scale);
            if (ImGui.Button($"Assign Draw <{KeyBindings.Current.Map_RenderGroup_GiveDraw.HintText}>")
                || (InputTracker.GetKeyDown(KeyBindings.Current.Map_RenderGroup_GiveDraw)
                    && sdispgroups != null))
            {
                IEnumerable<uint[]> selDrawGroups = sels.Select(s => s.Drawgroups);
                ArrayPropertyCopyAction action = new(dg.RenderGroups, selDrawGroups);
                _actionManager.ExecuteAction(action);
            }

            ImGui.SameLine(0.0f, 6.0f * scale);
            if (ImGui.Button($"Assign Disp <{KeyBindings.Current.Map_RenderGroup_GiveDisp.HintText}>")
                || (InputTracker.GetKeyDown(KeyBindings.Current.Map_RenderGroup_GiveDisp)
                    && sdispgroups != null))
            {
                IEnumerable<uint[]> selDispGroups = sels.Select(s => s.Dispgroups);
                ArrayPropertyCopyAction action = new(dg.RenderGroups, selDispGroups);
                _actionManager.ExecuteAction(action);
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
                || InputTracker.GetKeyDown(KeyBindings.Current.Map_RenderGroup_SelectHighlights))
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
                    "Also, CollisionName references will be targeted by DSMapStudio when using `Set Selection`/`Get Selection` instead of your actual selection.\n" +
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
            ImGui.BeginChild("##DispTicks", new Vector2(), false, ImGuiWindowFlags.AlwaysVerticalScrollbar);
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

                    var check = ((dg.RenderGroups[g] >> i) & 0x1) > 0;

                    // Add spacing every 4 columns
                    if (i % 4 == 0 && i > 0)
                    {
                        ImGui.SameLine();
                        ImGui.Spacing();
                    }

                    ImGui.SameLine();

                    var drawActive = sdrawgroups != null && ((sdrawgroups[g] >> i) & 0x1) > 0;
                    var dispActive = sdispgroups != null && ((sdispgroups[g] >> i) & 0x1) > 0;

                    if (drawActive && dispActive)
                    {
                        // Selection dispgroup and drawgroup is ticked
                        // Yellow
                        ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0.4f, 0.4f, 0.06f, 1.0f));
                        ImGui.PushStyleColor(ImGuiCol.CheckMark, new Vector4(1f, 1f, 0.02f, 1.0f));
                    }
                    else if (drawActive)
                    {
                        // Selection drawgroup is ticked
                        // Green
                        ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0.02f, 0.3f, 0.02f, 1.0f));
                        ImGui.PushStyleColor(ImGuiCol.CheckMark, new Vector4(0.2f, 1.0f, 0.2f, 1.0f));
                    }
                    else if (dispActive)
                    {
                        // Selection dispGroup is ticked
                        // Red
                        ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0.4f, 0.06f, 0.06f, 1.0f));
                        ImGui.PushStyleColor(ImGuiCol.CheckMark, new Vector4(1.0f, 0.2f, 0.2f, 1.0f));
                    }
                    if (HighlightedGroups.Contains(cellKey))
                    {
                        ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(1.0f, 0.2f, 0.2f, 1.0f));
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
