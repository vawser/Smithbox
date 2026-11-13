using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// Vanilla Compare: how the loaded model differs from the game's own. Counts per list, the selected object's fields
/// against vanilla with per-field and whole-object revert, lists of modified, added and removed objects (removed
/// ones can be restored), and Revert Model to Vanilla. Plan: Documentation/Model Editor Second Review.md, R6.
/// </summary>
public class ModelVanillaCompareTool
{
    private readonly ModelEditorView View;
    private readonly ProjectEntry Project;

    // Rebuilt when the diff changes (a new one is made after every edit)
    private FlverDiff ListedFor;
    private List<Entity> Modified = new();
    private List<Entity> Added = new();
    private List<(string Kind, int Current, int Vanilla, int Modified, int Added, int Removed)> Counts = new();

    // The selected object's differences, rebuilt when the diff or the object changes
    private (FlverDiff Diff, Entity Entity) FieldsFor;
    private List<FlverFieldDiff> Fields = new();

    // Set while drawing, run after, as they change the model
    private ViewportAction PendingAction;
    private bool PendingRevertModel;

    private bool OpenRevertModelConfirm;

    public ModelVanillaCompareTool(ModelEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    private ModelVanillaCompare Compare => View.VanillaCompare;

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("MODEL_Vanilla_Tool_Header")}##modelVanillaToolHeader"))
            return;

        ImGui.BeginChild("ModelVanillaToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();

        DisplayRevertModelConfirm();

        // After the section, as they change the model
        if (PendingAction != null)
        {
            var action = PendingAction;
            PendingAction = null;

            View.ViewportActionManager.ExecuteAction(action);

            // Reverts and restores hand vanilla's own objects to the model, so vanilla is read again
            Compare.Invalidate();
        }

        if (PendingRevertModel)
        {
            PendingRevertModel = false;
            RevertModel();
        }
    }

    private void Display()
    {
        GUI.WrappedText(LOC.Get("MODEL_Vanilla_Tool_Hint"));

        var wrapper = View.Selection.SelectedModelWrapper;

        if (wrapper?.Container?.View != View)
        {
            GUI.WrappedText(LOC.Get("MODEL_Vanilla_Tool_No_Model"));
            return;
        }

        var diff = Compare.GetDiff(out var hasVanilla);

        if (!hasVanilla || diff == null)
        {
            GUI.WrappedText(LOC.Get("MODEL_Vanilla_Tool_No_Vanilla", wrapper.Name));
            return;
        }

        RebuildLists(diff);

        DisplaySummary();
        DisplaySelected(diff);
        DisplayLists(diff);
        DisplayActions();
    }

    #region Lists
    private void RebuildLists(FlverDiff diff)
    {
        if (ReferenceEquals(ListedFor, diff))
            return;

        ListedFor = diff;

        var container = View.Selection.SelectedModelWrapper.Container;

        Modified = new();
        Added = new();

        foreach (var entity in container.Objects)
        {
            if (entity.WrappedObject == null || !diff.Pairs.TryGetValue(entity.WrappedObject, out var vanilla))
                continue;

            if (vanilla == null)
                Added.Add(entity);
            else if (diff.Differs(entity.WrappedObject))
                Modified.Add(entity);
        }

        (string, int, int, int, int, int) Count<T>(string kind, List<T> current, List<T> vanilla)
        {
            return (kind, current.Count, vanilla.Count,
                Modified.Count(e => e.WrappedObject is T), Added.Count(e => e.WrappedObject is T), diff.Removed.Count(r => r is T));
        }

        Counts = new()
        {
            Count(LOC.Get("MODEL_Vanilla_Kind_Materials"), diff.Current.Materials, diff.Vanilla.Materials),
            Count(LOC.Get("MODEL_Vanilla_Kind_Nodes"), diff.Current.Nodes, diff.Vanilla.Nodes),
            Count(LOC.Get("MODEL_Vanilla_Kind_Dummies"), diff.Current.Dummies, diff.Vanilla.Dummies),
            Count(LOC.Get("MODEL_Vanilla_Kind_Meshes"), diff.Current.Meshes, diff.Vanilla.Meshes)
        };
    }

    private void DisplaySummary()
    {
        // Summary
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MODEL_Vanilla_Tool_Summary_Header"), LOC.Get("MODEL_Vanilla_Tool_Summary_Header_TT"));

        if (ImGui.BeginTable("modelVanillaCounts", 6, ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.SizingStretchProp))
        {
            ImGui.TableSetupColumn(LOC.Get("MODEL_Vanilla_Column_List"));
            ImGui.TableSetupColumn(LOC.Get("MODEL_Vanilla_Column_Current"));
            ImGui.TableSetupColumn(LOC.Get("MODEL_Vanilla_Column_Vanilla"));
            ImGui.TableSetupColumn(LOC.Get("MODEL_Vanilla_Column_Modified"));
            ImGui.TableSetupColumn(LOC.Get("MODEL_Vanilla_Column_Added"));
            ImGui.TableSetupColumn(LOC.Get("MODEL_Vanilla_Column_Removed"));
            ImGui.TableHeadersRow();

            foreach (var row in Counts)
            {
                ImGui.TableNextRow();
                ImGui.TableNextColumn(); ImGui.Text(row.Kind);
                ImGui.TableNextColumn(); ImGui.Text($"{row.Current}");
                ImGui.TableNextColumn(); ImGui.Text($"{row.Vanilla}");
                ImGui.TableNextColumn(); ImGui.Text($"{row.Modified}");
                ImGui.TableNextColumn(); ImGui.Text($"{row.Added}");
                ImGui.TableNextColumn(); ImGui.Text($"{row.Removed}");
            }

            ImGui.EndTable();
        }
    }

    private void DisplayLists(FlverDiff diff)
    {
        // Changes
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MODEL_Vanilla_Tool_Changes_Header"), LOC.Get("MODEL_Vanilla_Tool_Changes_Header_TT"));

        ImGui.BeginChild("modelVanillaLists", new Vector2(0, 260 * DPI.UIScale()), ImGuiChildFlags.Borders);

        if (ImGui.BeginTabBar("##modelVanillaTabs"))
        {
            if (ImGui.BeginTabItem($"{LOC.Get("MODEL_Vanilla_Tab_Modified", Modified.Count)}##modelVanillaTabModified"))
            {
                DisplayEntities(Modified, "modified", UI.Current.ImGui_MapEditor_VanillaModified_Text);
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem($"{LOC.Get("MODEL_Vanilla_Tab_Added", Added.Count)}##modelVanillaTabAdded"))
            {
                DisplayEntities(Added, "added", UI.Current.ImGui_MapEditor_VanillaAdded_Text);
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem($"{LOC.Get("MODEL_Vanilla_Tab_Removed", diff.Removed.Count)}##modelVanillaTabRemoved"))
            {
                DisplayRemoved(diff);
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }

        ImGui.EndChild();
    }

    private void DisplayEntities(List<Entity> entities, string id, Vector4 colour)
    {
        if (entities.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MODEL_Vanilla_Tool_None"));
            return;
        }

        var clipper = new ImGuiListClipper();
        clipper.Begin(entities.Count);

        while (clipper.Step())
        {
            for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var entity = entities[i];

                ImGui.PushStyleColor(ImGuiCol.Text, colour);
                var clicked = ImGui.Selectable($"{KindName(entity.WrappedObject)}: {ModelHistory.NameObject(entity)}##modelVanilla_{id}_{i}",
                    View.ViewportSelection.GetSelection().Contains(entity));
                ImGui.PopStyleColor();

                if (clicked)
                {
                    View.ViewportSelection.ClearSelection();
                    View.ViewportSelection.AddSelection(entity);
                    View.ViewportSelection.GotoTreeTarget = entity;
                }

                GUI.Tooltip(LOC.Get("MODEL_Vanilla_Tool_Select_TT"));
            }
        }

        clipper.End();
    }

    private void DisplayRemoved(FlverDiff diff)
    {
        if (diff.Removed.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MODEL_Vanilla_Tool_None"));
            return;
        }

        var clipper = new ImGuiListClipper();
        clipper.Begin(diff.Removed.Count);

        while (clipper.Step())
        {
            for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var removed = diff.Removed[i];

                // Meshes can't be added one at a time; Revert Model to Vanilla brings them back
                if (removed is FLVER2.Mesh)
                {
                    ImGui.TextDisabled(LOC.Get("MODEL_Vanilla_Tool_No_Restore"));
                    GUI.Tooltip(LOC.Get("MODEL_Vanilla_Restore_Mesh"));
                }
                else
                {
                    if (ImGui.Button($"{LOC.Get("MODEL_Vanilla_Tool_Restore")}##modelVanillaRestore{i}"))
                    {
                        var action = Compare.BuildRestore(diff, removed, out var reason);

                        if (action != null)
                            PendingAction = action;
                        else
                            Smithbox.LogError(this, LOC.Get("MODEL_Vanilla_Restore_Failed", reason));
                    }
                    GUI.Tooltip(LOC.Get("MODEL_Vanilla_Tool_Restore_TT"));
                }

                ImGui.SameLine();
                ImGui.TextUnformatted($"{KindName(removed)}: {VanillaName(diff, removed)}");
            }
        }

        clipper.End();
    }

    private static string KindName(object obj)
    {
        return obj switch
        {
            FLVER2.Material => LOC.Get("MODEL_Vanilla_Kind_Material"),
            FLVER.Node => LOC.Get("MODEL_Vanilla_Kind_Node"),
            FLVER.Dummy => LOC.Get("MODEL_Vanilla_Kind_Dummy"),
            FLVER2.Mesh => LOC.Get("MODEL_Vanilla_Kind_Mesh"),
            _ => obj?.GetType().Name ?? ""
        };
    }

    private static string VanillaName(FlverDiff diff, object obj)
    {
        return obj switch
        {
            FLVER2.Material m => m.Name,
            FLVER.Node n => n.Name,
            FLVER.Dummy d => LOC.Get("MODEL_History_Dummy", d.ReferenceID),
            FLVER2.Mesh mesh => LOC.Get("MODEL_History_Mesh", diff.Vanilla.Meshes.IndexOf(mesh)),
            _ => ""
        };
    }
    #endregion

    #region Selected
    private void DisplaySelected(FlverDiff diff)
    {
        var container = View.Selection.SelectedModelWrapper.Container;

        var entity = View.ViewportSelection.GetFilteredSelection<Entity>()
            .FirstOrDefault(e => e.WrappedObject is FLVER2.Material or FLVER.Node or FLVER.Dummy or FLVER2.Mesh ||
                                 e == container.RootObject);

        // Selected Object
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MODEL_Vanilla_Tool_Selected_Header"), LOC.Get("MODEL_Vanilla_Tool_Selected_Header_TT"));

        if (entity == null)
        {
            GUI.WrappedText(LOC.Get("MODEL_Vanilla_Tool_Select_Something"));
            return;
        }

        var isModel = entity == container.RootObject;

        if (!ReferenceEquals(FieldsFor.Diff, diff) || FieldsFor.Entity != entity)
        {
            Fields = isModel ? diff.CompareModel() : diff.Compare(entity.WrappedObject);
            FieldsFor = (diff, entity);
        }

        var name = isModel ? LOC.Get("MODEL_SaveCheck_Object_Model") : $"{KindName(entity.WrappedObject)}: {ModelHistory.NameObject(entity)}";

        if (!isModel && diff.Pairs.TryGetValue(entity.WrappedObject, out var vanilla) && vanilla == null)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_MapEditor_VanillaAdded_Text, LOC.Get("MODEL_Vanilla_Tool_Added", name));
            return;
        }

        if (Fields.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MODEL_Vanilla_Tool_Unchanged", name));
            return;
        }

        GUI.WrappedTextColored(UI.Current.ImGui_MapEditor_VanillaModified_Text, LOC.Get("MODEL_Vanilla_Tool_Fields", name, Fields.Count));

        if (ImGui.BeginTable("modelVanillaFields", 4, ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.SizingStretchProp))
        {
            ImGui.TableSetupColumn(LOC.Get("MODEL_Vanilla_Column_Field"), ImGuiTableColumnFlags.WidthStretch, 1.0f);
            ImGui.TableSetupColumn(LOC.Get("MODEL_Vanilla_Column_Current"), ImGuiTableColumnFlags.WidthStretch, 1.5f);
            ImGui.TableSetupColumn(LOC.Get("MODEL_Vanilla_Column_Vanilla"), ImGuiTableColumnFlags.WidthStretch, 1.5f);
            ImGui.TableSetupColumn("##revert", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableHeadersRow();

            for (var i = 0; i < Fields.Count; i++)
            {
                var field = Fields[i];

                ImGui.TableNextRow();

                ImGui.TableNextColumn();
                ImGui.TextUnformatted(field.Path);

                var current = ModelVanillaCompare.Format(field.Current);
                var vanillaText = ModelVanillaCompare.Format(field.Vanilla);

                ImGui.TableNextColumn();
                ImGui.TextUnformatted(current);
                GUI.Tooltip(current);

                ImGui.TableNextColumn();
                ImGui.TextUnformatted(vanillaText);
                GUI.Tooltip(vanillaText);

                ImGui.TableNextColumn();

                if (field.Kind is FlverDiffKind.Info)
                    continue;

                if (ImGui.Button($"{LOC.Get("MODEL_Vanilla_Tool_Revert")}##modelVanillaRevertField{i}"))
                {
                    var action = Compare.BuildFieldRevert(diff, entity, field, out var reason);

                    if (action != null)
                        PendingAction = action;
                    else
                        Smithbox.LogError(this, LOC.Get("MODEL_Vanilla_Revert_Failed", field.Path, reason));
                }
                GUI.Tooltip(field.Kind is FlverDiffKind.Geometry
                    ? LOC.Get("MODEL_Vanilla_Tool_Revert_Geometry_TT")
                    : LOC.Get("MODEL_Vanilla_Tool_Revert_TT"));
            }

            ImGui.EndTable();
        }

        // Revert Object
        GUI.MultiButtonInput("modelVanillaRevertObject",
            "modelVanillaRevertObjectButton", LOC.Get("MODEL_Vanilla_Tool_Revert_Object"), LOC.Get("MODEL_Vanilla_Tool_Revert_Object_TT"),
            () =>
            {
                var action = Compare.BuildObjectRevert(diff, entity, isModel, out var skipped, out var reason);

                if (action != null)
                    PendingAction = action;

                if (skipped > 0)
                    Smithbox.LogError(this, LOC.Get("MODEL_Vanilla_Revert_Skipped", skipped, reason));
            });
    }
    #endregion

    #region Revert Model
    private void DisplayActions()
    {
        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MODEL_Vanilla_Tool_Actions_Header"), LOC.Get("MODEL_Vanilla_Tool_Actions_Header_TT"));

        GUI.MultiButtonInput("modelVanillaActions",
            "modelVanillaRevertModel", LOC.Get("MODEL_Vanilla_Tool_Revert_Model"), LOC.Get("MODEL_Vanilla_Tool_Revert_Model_TT"),
            () => OpenRevertModelConfirm = true);
    }

    private void DisplayRevertModelConfirm()
    {
        var title = $"{LOC.Get("MODEL_Vanilla_Revert_Model_Title")}###modelVanillaRevertModelConfirm";

        if (OpenRevertModelConfirm)
        {
            OpenRevertModelConfirm = false;
            ImGui.OpenPopup(title);
        }

        ImGui.SetNextWindowSize(new Vector2(460, 0) * DPI.UIScale(), ImGuiCond.Always);

        var open = true;

        if (ImGui.BeginPopupModal(title, ref open, ImGuiWindowFlags.AlwaysAutoResize))
        {
            GUI.WrappedText(LOC.Get("MODEL_Vanilla_Revert_Model_Text", View.Selection.SelectedModelWrapper?.Name ?? ""));

            GUI.MultiButtonInput("modelVanillaRevertModelButtons",
                "modelVanillaRevertModelYes", LOC.Get("MODEL_Vanilla_Revert_Model_Yes"), LOC.Get("MODEL_Vanilla_Revert_Model_Yes_TT"),
                () =>
                {
                    PendingRevertModel = true;
                    ImGui.CloseCurrentPopup();
                },

                "modelVanillaRevertModelNo", LOC.Get("MODEL_Vanilla_Revert_Model_No"), LOC.Get("MODEL_Vanilla_Revert_Model_No_TT"),
                ImGui.CloseCurrentPopup);

            ImGui.EndPopup();
        }
    }

    /// <summary>
    /// Replaces the model's FLVER with a fresh copy of vanilla's and loads it again. It is marked unsaved, so nothing is
    /// written until a save. The other files (cloth, collision) are left as they are.
    /// </summary>
    private void RevertModel()
    {
        var wrapper = View.Selection.SelectedModelWrapper;

        if (wrapper == null)
            return;

        var vanilla = wrapper.ReadVanillaFlver();

        if (vanilla == null)
        {
            Smithbox.LogError(this, LOC.Get("MODEL_Vanilla_Tool_No_Vanilla", wrapper.Name));
            return;
        }

        View.RevertModelToVanilla(wrapper, vanilla);

        Compare.Invalidate();

        Smithbox.Log(this, LOC.Get("MODEL_Vanilla_Revert_Model_Done", wrapper.Name));
    }
    #endregion
}
