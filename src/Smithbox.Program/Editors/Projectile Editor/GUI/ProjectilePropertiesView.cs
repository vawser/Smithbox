#nullable enable
using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.ParamEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StudioCore.Editors.ProjectileEditor;

/// <summary>
/// A tab for the selected row, then one for each attack row its attack id points at. The bullet tab also lists the
/// bullets it creates, as links. A spell row has one tab, with its own fields and the bullets in its slots.
/// <para>
/// Fields are drawn and edited with the Param Editor's own decorators and inputs, so they show the same community
/// names, descriptions, param references, enums, aliases and text references, and a value is applied the same way:
/// when its field is left after an edit (Enter, Tab or a click elsewhere, but not Escape), as an action of the Param
/// Editor's, so it can be undone in either editor and the simulation runs again with it.
/// </para>
/// </summary>
public class ProjectilePropertiesView
{
    private static readonly PropertyInfo CellValue = typeof(Param.Cell).GetProperty("Value")!;

    private readonly ProjectileEditorView View;

    private string FieldFilter = "";
    private bool ExactFieldFilter = false;

    // The Param Editor view the decorators and inputs work through, and this window's own copies of them, so an
    // edit in progress here never mixes with one in the Param Editor
    private ParamEditorView? FieldView;
    private ParamFieldDecorators? Decorators;
    private ParamFieldInput? FieldInput;

    public ProjectilePropertiesView(ProjectileEditorView view)
    {
        View = view;
    }

    private ProjectEntry Project => View.Project;

    public void Display()
    {
        var simulator = View.Simulator;

        if (simulator.Row is not { } row || simulator.ParamName is not { } paramName)
        {
            GUI.WrappedText(LOC.Get("PROJECTILE_Properties_None"));
            return;
        }

        var param = ProjectileParams.GetParam(Project, paramName);

        if (param == null)
            return;

        if (!UpdateFieldView())
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("PROJECTILE_Properties_No_Param_Editor"));
            return;
        }

        EditorFilters.DisplayFramedListFilter("projectileFieldFilter", ref FieldFilter, ref ExactFieldFilter);

        // The tabs are named by param, not by row, so the open tab stays open as other rows are selected
        if (!ImGui.BeginTabBar("projectilePropertiesTabs", ImGuiTabBarFlags.FittingPolicyScroll))
            return;

        if (ImGui.BeginTabItem($"{paramName}###projectileTab_{paramName}"))
        {
            DisplayRowTab(paramName, param, row, null, () =>
            {
                if (simulator.IsSpell)
                {
                    DisplaySpellBullets(row);
                }
                else
                {
                    DisplayAttackNote(row);
                    DisplayChildBullets(paramName, param, row);
                }
            });

            ImGui.EndTabItem();
        }

        if (!simulator.IsSpell)
            DisplayAttackTabs(row);

        ImGui.EndTabBar();
    }

    /// <summary>
    /// Points the decorators and inputs at the Param Editor's active view, making new ones when it changes.
    /// False if there is no Param Editor to edit through.
    /// </summary>
    private bool UpdateFieldView()
    {
        var paramEditor = View.Editor.ParamEditor;
        var activeView = paramEditor?.ViewHandler?.ActiveView;

        if (paramEditor == null || activeView == null)
            return false;

        if (FieldView != activeView)
        {
            FieldView = activeView;
            Decorators = new ParamFieldDecorators(paramEditor, Project, activeView);
            FieldInput = new ParamFieldInput(paramEditor, Project, activeView);
        }

        return true;
    }

    // ---- tabs

    /// <summary>The attack rows the bullet's attack id points at, a tab each: the one the simulation uses first</summary>
    private void DisplayAttackTabs(Param.Row row)
    {
        var attackId = ProjectileParams.GetAttackId(row);

        if (attackId <= 0)
            return;

        var used = ProjectileParams.FindAttackRow(Project, attackId, out var usedParam);

        if (used == null || usedParam == null || ProjectileParams.GetParam(Project, usedParam) is not { } usedParamData)
            return;

        if (ImGui.BeginTabItem($"{usedParam}###projectileTab_{usedParam}"))
        {
            DisplayRowTab(usedParam, usedParamData, used, LOC.Get("PROJECTILE_Properties_Attack_Used"), null);
            ImGui.EndTabItem();
        }
        GUI.Tooltip(LOC.Get("PROJECTILE_Properties_Attack_Tab_Used_TT"));

        // The same id in the other attack param, which the simulation uses with the other setting
        var otherParam = usedParam == ProjectileParams.PlayerAttackParam
            ? ProjectileParams.EnemyAttackParam
            : ProjectileParams.PlayerAttackParam;

        if (ProjectileParams.GetParam(Project, otherParam) is { } other && other[attackId] is { } otherRow)
        {
            if (ImGui.BeginTabItem($"{otherParam}###projectileTab_{otherParam}"))
            {
                DisplayRowTab(otherParam, other, otherRow, null, null);
                ImGui.EndTabItem();
            }
            GUI.Tooltip(LOC.Get("PROJECTILE_Properties_Attack_Tab_Other_TT"));
        }
    }

    /// <summary>
    /// A tab's content: the row's label (with a link to it in the Param Editor), its fields, then anything after them
    /// </summary>
    private void DisplayRowTab(string paramName, Param param, Param.Row row, string? note, Action? after)
    {
        var label = ProjectileParams.RowLabel(row.ID, row.Name);

        if (note != null)
            label += $"  ({note})";

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_AliasName_Text);
        ImGui.Selectable($"{label}##projectileRowLabel_{paramName}");
        ImGui.PopStyleColor();

        if (ImGui.BeginPopupContextItem($"projectileRowLabelContext_{paramName}"))
        {
            if (ImGui.Selectable($"{LOC.Get("PROJECTILE_Open_In_Param_Editor")}##openInParamEditor"))
                OpenInParamEditor(paramName, row.ID);

            ImGui.EndPopup();
        }
        GUI.Tooltip(LOC.Get("PROJECTILE_Properties_Row_TT"));

        if (ImGui.BeginChild($"projectileTabContent_{paramName}"))
        {
            DisplayFields(paramName, param, row);
            after?.Invoke();
        }

        ImGui.EndChild();
    }

    /// <summary>Why a bullet has no attack tab, when it has none</summary>
    private void DisplayAttackNote(Param.Row row)
    {
        var attackId = ProjectileParams.GetAttackId(row);

        if (attackId <= 0)
        {
            GUI.Spacer();
            ImGui.TextDisabled(LOC.Get("PROJECTILE_Properties_No_Attack"));
            return;
        }

        if (ProjectileParams.FindAttackRow(Project, attackId, out _) == null)
        {
            GUI.Spacer();
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("PROJECTILE_Properties_Missing_Attack", attackId));
        }
    }

    /// <summary>The bullets this one creates, each a link that shows it</summary>
    private void DisplayChildBullets(string paramName, Param param, Param.Row row)
    {
        var children = ProjectileParams.GetChildBullets(row).ToList();

        if (children.Count == 0)
            return;

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("PROJECTILE_Properties_Child_Bullets"), LOC.Get("PROJECTILE_Properties_Child_Bullets_TT"));

        foreach (var (field, id) in children)
            BulletLink(paramName, param, field, id);
    }

    private void DisplaySpellBullets(Param.Row row)
    {
        var bullets = BulletDefinition.GetSpellBullets(row);

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("PROJECTILE_Properties_Spell_Bullets"), LOC.Get("PROJECTILE_Properties_Child_Bullets_TT"));

        if (bullets.Length == 0)
        {
            ImGui.TextDisabled(LOC.Get("PROJECTILE_Spell_No_Bullets"));
            return;
        }

        var bulletParam = ProjectileParams.GetSpellBulletParam(Project);

        if (bulletParam == null)
            return;

        var paramName = ProjectileParams.GetParam(Project, "Bullet") != null ? "Bullet" : "BulletParam";

        foreach (var bullet in bullets)
            BulletLink(paramName, bulletParam, bullet.Slot.ToString(), bullet.Id);
    }

    private void BulletLink(string paramName, Param param, string field, int id)
    {
        var target = param[id];
        var label = $"{field}: {ProjectileParams.RowLabel(id, target?.Name)}";

        if (target == null)
        {
            ImGui.TextDisabled($"{label} ({LOC.Get("PROJECTILE_Properties_Missing_Row")})");
            return;
        }

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_AliasName_Text);
        if (ImGui.Selectable($"{label}##bulletLink_{field}_{id}"))
            View.Select(paramName, target);
        ImGui.PopStyleColor();
        GUI.Tooltip(LOC.Get("PROJECTILE_Properties_Bullet_Link_TT"));

        if (ImGui.BeginPopupContextItem($"bulletLinkContext_{field}_{id}"))
        {
            if (ImGui.Selectable($"{LOC.Get("PROJECTILE_Open_In_Param_Editor")}##openInParamEditor"))
                OpenInParamEditor(paramName, id);

            ImGui.EndPopup();
        }
    }

    private static void OpenInParamEditor(string paramName, int id) =>
        EditorCommandQueue.AddCommand($"param/select/-1/{paramName}/{id}");

    // ---- fields

    private void DisplayFields(string paramName, Param param, Param.Row row)
    {
        var paramData = Project.Handler.ParamData;
        var meta = paramData.GetParamMeta(param.AppliedParamdef);
        var annotations = paramData.GetParamAnnotations(param.ParamType);
        var filter = FieldFilter.Trim();

        var tableFlags = ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.Resizable;

        if (!ImGui.BeginTable($"projectileFields_{paramName}", 2, tableFlags))
            return;

        ImGui.TableSetupColumn("##name", ImGuiTableColumnFlags.WidthStretch, 0.5f);
        ImGui.TableSetupColumn("##value", ImGuiTableColumnFlags.WidthStretch, 0.5f);

        var index = 0;

        foreach (var cell in row.Cells)
        {
            var field = cell.Def;
            var internalName = field.InternalName;
            var fieldMeta = paramData.GetParamFieldMeta(meta, field);
            var annotation = paramData.GetFieldAnnotation(annotations, internalName);
            var context = new FieldMetaContext(FieldView!, meta, fieldMeta, annotation, paramName, internalName);

            if (context.IsPadding && !CFG.Current.ParamEditor_Field_List_Display_Padding)
                continue;

            var name = DisplayName(internalName, annotation);

            if (filter.Length > 0 && !EditorFilters.IsMatch(filter, internalName, ExactFieldFilter, name))
            {
                continue;
            }

            ImGui.PushID(index++);
            FieldRow(paramName, row, cell, context, name);
            ImGui.PopID();
        }

        ImGui.EndTable();
    }

    /// <summary>One field, as the Param Editor draws it: name, labels, input, hints, clicks and context menu</summary>
    private void FieldRow(string paramName, Param.Row row, Param.Cell cell, FieldMetaContext context, string name)
    {
        var decorators = Decorators!;
        var input = FieldInput!;
        var field = cell.Def;
        var oldval = cell.Value;
        object newval = null!;

        ImGui.TableNextRow();

        // Name, with the Param Editor's description tooltip and labels (param references, enums and the like)
        ImGui.TableSetColumnIndex(0);

        if (context.InjectSeparator)
            ImGui.Separator();

        FieldTooltipHelper.IconTooltip(FieldView!, context, field);

        // A field whose value differs from the vanilla row's takes the modified colour. Fields of an added row stay
        // uncoloured (the row list shows it as added), as in the Param Editor's field list
        var fieldStatus = ParamVanillaStatus.FieldStatus(Project, paramName, row, field.InternalName);
        var nameColoured = fieldStatus == VanillaStatus.Modified && VanillaMarkers.PushColour(fieldStatus);

        ImGui.AlignTextToFramePadding();
        ImGui.TextUnformatted(name);
        VanillaMarkers.PopColour(nameColoured);
        FieldTooltipHelper.HoverTooltip(FieldView!, context, field);

        decorators.HandleCache(context, row, oldval);
        decorators.HandleLabels(context, row, oldval);

        // The labels (param references, enums and the like), or the name when there are none, open the value's menu
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
            ImGui.OpenPopup("projectileFieldValueMenu");

        // Value, coloured as a reference when it points at something
        ImGui.TableSetColumnIndex(1);

        if (context.InjectSeparator)
            ImGui.Separator();

        var isReference = context.HasAnyReferenceElements();

        if (isReference)
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_IsRef_Text);

        input.DisplayFieldInput(context, oldval.GetType(), oldval, ref newval, field.BitSize);

        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
            ImGui.OpenPopup("projectileFieldValueMenu");

        decorators.HandleHints(context, row, paramName, field.InternalName, oldval);

        // So do the hints beside the value (the referenced row's name, the enum's name and the like)
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
            ImGui.OpenPopup("projectileFieldValueMenu");

        decorators.HandleClick(context, row, oldval);

        // Effect ids the metadata doesn't mark as effects still lead to the effect in the FX Editor
        var isUnmarkedEffect = !context.DisplayParticleEnum && oldval is IConvertible &&
            field.InternalName.Contains("sfx", StringComparison.OrdinalIgnoreCase);

        if (isUnmarkedEffect)
            FxReferenceHelper.Click(Project, oldval);

        if (isReference)
            ImGui.PopStyleColor();

        // The value's menu: the Param Editor's reference, enum and alias choices
        if (ImGui.BeginPopup("projectileFieldValueMenu"))
        {
            var anyShown = false;

            if (context.HasAnyDisplayedElements())
            {
                if (decorators.HandleContextMenu(context, row, oldval, ref newval))
                    input.SetLastPropertyManual(newval);

                anyShown = true;
            }

            if (CFG.Current.ParamEditor_Field_Context_Display_Reference_Search &&
                (context.DisplayVirtualReference || context.DisplayExternalReference))
            {
                VirtualParamReferenceHelper.ContextMenu(FieldView!, context.VirtualReference, oldval, row, field.InternalName);
                ExternalReferenceHelper.ContextMenu(FieldView!, context.VirtualReference, oldval, row, field.InternalName, context.ExternalReferences);
                anyShown = true;
            }

            if (isUnmarkedEffect)
            {
                FxReferenceHelper.ContextMenu(Project, oldval);
                anyShown = true;
            }

            if (!anyShown)
            {
                if (ImGui.Selectable($"{LOC.Get("PROJECTILE_Open_In_Param_Editor")}##openInParamEditor"))
                    OpenInParamEditor(paramName, row.ID);
            }

            ImGui.EndPopup();
        }

        if (input.UpdateProperty(cell, CellValue, oldval))
            Project.Handler.ParamData.PrimaryBank.RefreshParamRowDiffs(View.Editor.ParamEditor!, row, paramName);
    }

    /// <summary>
    /// The field's name: its community name, shown as the Param Editor's field name setting says, if the
    /// Projectile Editor's community names setting is on
    /// </summary>
    private static string DisplayName(string internalName, ParamAnnotationFieldEntry? annotation)
    {
        var altName = annotation?.Name;

        if (!CFG.Current.ProjectileEditor_Show_Community_Names || string.IsNullOrWhiteSpace(altName))
            return internalName;

        return CFG.Current.ParamEditor_FieldNameMode switch
        {
            ParamFieldNameMode.Source => internalName,
            ParamFieldNameMode.Source_Community => $"{internalName} ({altName})",
            ParamFieldNameMode.Community_Source => $"{altName} ({internalName})",
            _ => altName
        };
    }
}
