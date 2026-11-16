#nullable enable
using Andre.Formats;
using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.ParamEditor;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace StudioCore.Editors.ProjectileEditor;

/// <summary>
/// The selected row's fields, then the attack rows its attack id points at, then the bullets it creates, as links.
/// A spell row shows its own fields and the bullets in its slots.
/// <para>
/// Fields are edited as in the Param Editor: a value is applied when its field is left after an edit (Enter, Tab or a
/// click elsewhere, but not Escape), as an action of the Param Editor's, so it can be undone in either editor and the
/// simulation runs again with it.
/// </para>
/// </summary>
public class ProjectilePropertiesView
{
    private static readonly PropertyInfo CellValue = typeof(Param.Cell).GetProperty("Value")!;

    private readonly ProjectileEditorView View;

    private string FieldFilter = "";

    // The text field being edited, and what has been typed in it
    private string? EditKey;
    private string EditText = "";

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

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##projectileFieldFilter", LOC.Get("PROJECTILE_Properties_Filter_Hint"), ref FieldFilter, 256);

        if (ImGui.BeginChild("projectileProperties"))
        {
            DisplayRow(paramName, param, row, true, null);

            if (simulator.IsSpell)
            {
                DisplaySpellBullets(row);
            }
            else
            {
                DisplayAttacks(row);
                DisplayChildBullets(paramName, param, row);
            }
        }

        ImGui.EndChild();
    }

    // ---- sections

    /// <summary>A row's header (with a link to it in the Param Editor) and, when open, its fields</summary>
    private void DisplayRow(string paramName, Param param, Param.Row row, bool defaultOpen, string? note)
    {
        var label = $"{paramName}: {ProjectileParams.RowLabel(row.ID, row.Name)}";

        if (note != null)
            label += $"  ({note})";

        var flags = defaultOpen ? ImGuiTreeNodeFlags.DefaultOpen : ImGuiTreeNodeFlags.None;
        var open = ImGui.CollapsingHeader($"{label}###projectileRowHeader_{paramName}_{row.ID}", flags);

        if (ImGui.BeginPopupContextItem($"projectileRowHeaderContext_{paramName}_{row.ID}"))
        {
            if (ImGui.Selectable($"{LOC.Get("PROJECTILE_Open_In_Param_Editor")}##openInParamEditor"))
                OpenInParamEditor(paramName, row.ID);

            ImGui.EndPopup();
        }
        GUI.Tooltip(LOC.Get("PROJECTILE_Properties_Row_TT"));

        if (open)
            DisplayFields(paramName, param, row);
    }

    /// <summary>The attack rows the bullet's attack id points at: the one the simulation uses first</summary>
    private void DisplayAttacks(Param.Row row)
    {
        var attackId = ProjectileParams.GetAttackId(row);

        GUI.Spacer();

        if (attackId <= 0)
        {
            ImGui.TextDisabled(LOC.Get("PROJECTILE_Properties_No_Attack"));
            return;
        }

        var used = ProjectileParams.FindAttackRow(Project, attackId, out var usedParam);

        if (used == null || usedParam == null)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("PROJECTILE_Properties_Missing_Attack", attackId));
            return;
        }

        DisplayRow(usedParam, ProjectileParams.GetParam(Project, usedParam)!, used, true, LOC.Get("PROJECTILE_Properties_Attack_Used"));

        // The same id in the other attack param, which the simulation uses with the other setting
        var otherParam = usedParam == ProjectileParams.PlayerAttackParam
            ? ProjectileParams.EnemyAttackParam
            : ProjectileParams.PlayerAttackParam;

        if (ProjectileParams.GetParam(Project, otherParam) is { } other && other[attackId] is { } otherRow)
            DisplayRow(otherParam, other, otherRow, false, null);
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
        var filter = FieldFilter.Trim();

        var tableFlags = ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.Resizable;

        if (!ImGui.BeginTable($"projectileFields_{paramName}_{row.ID}", 2, tableFlags))
            return;

        ImGui.TableSetupColumn("##name", ImGuiTableColumnFlags.WidthStretch, 0.5f);
        ImGui.TableSetupColumn("##value", ImGuiTableColumnFlags.WidthStretch, 0.5f);

        foreach (var cell in row.Cells)
        {
            var field = cell.Def;
            var fieldMeta = paramData.GetParamFieldMeta(meta, field);

            if (fieldMeta?.IsPaddingField == true || cell.Value is byte[])
                continue;

            var name = DisplayName(field, fieldMeta);

            if (filter.Length > 0 &&
                !name.Contains(filter, StringComparison.OrdinalIgnoreCase) &&
                !field.InternalName.Contains(filter, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(0);
            ImGui.AlignTextToFramePadding();
            ImGui.TextUnformatted(name);

            if (ImGui.IsItemHovered())
                ImGui.SetTooltip(FieldTooltip(field, fieldMeta));

            ImGui.TableSetColumnIndex(1);
            DisplayFieldInput($"{paramName}/{row.ID}/{field.InternalName}", cell, fieldMeta);
        }

        ImGui.EndTable();
    }

    private static string DisplayName(PARAMDEF.Field field, ParamFieldMeta? meta)
    {
        if (CFG.Current.ProjectileEditor_Show_Community_Names && !string.IsNullOrEmpty(meta?.AltName))
            return meta.AltName;

        return field.InternalName;
    }

    private static string FieldTooltip(PARAMDEF.Field field, ParamFieldMeta? meta)
    {
        var tooltip = field.InternalName;

        if (!string.IsNullOrEmpty(meta?.AltName) && meta.AltName != field.InternalName)
            tooltip = $"{meta.AltName}\n{tooltip}";

        if (!string.IsNullOrEmpty(meta?.Wiki))
            tooltip += $"\n\n{meta.Wiki}";

        return tooltip;
    }

    private void DisplayFieldInput(string key, Param.Cell cell, ParamFieldMeta? meta)
    {
        var value = cell.Value;
        var id = $"##{key}";

        ImGui.SetNextItemWidth(-1);

        // A flag: a checkbox, applied when it is clicked
        if (meta?.IsBool == true && value is IConvertible)
        {
            var on = Convert.ToInt64(value, CultureInfo.InvariantCulture) != 0;

            if (ImGui.Checkbox(id, ref on))
                Commit(cell, on ? 1 : 0);

            return;
        }

        // An enum: its names, applied when one is picked
        if (meta?.EnumType is { Values.Count: > 0 } enumType)
        {
            var current = Convert.ToString(value, CultureInfo.InvariantCulture) ?? "";
            var preview = enumType.Values.TryGetValue(current, out var currentName) ? $"{currentName} ({current})" : current;

            if (ImGui.BeginCombo(id, preview))
            {
                foreach (var (optionValue, optionName) in enumType.Values)
                {
                    if (ImGui.Selectable($"{optionName} ({optionValue})", optionValue == current))
                        Commit(cell, optionValue);
                }

                ImGui.EndCombo();
            }

            return;
        }

        // Anything else: its text, applied when the field is left after an edit
        var text = EditKey == key ? EditText : Format(value);

        if (ImGui.InputText(id, ref text, 256))
        {
            EditKey = key;
            EditText = text;
        }

        if (GUI.InputCommitted())
        {
            if (EditKey == key)
                Commit(cell, EditText);

            EditKey = null;
        }
        else if (ImGui.IsItemDeactivated() && EditKey == key)
        {
            // Left with Escape: the edit is dropped
            EditKey = null;
        }

        // Effect ids lead to the effect in the FX Editor
        if (value is IConvertible && cell.Def.InternalName.Contains("sfx", StringComparison.OrdinalIgnoreCase))
        {
            FxReferenceHelper.Click(Project, value);

            if (ImGui.BeginPopupContextItem($"fxContext{key}"))
            {
                FxReferenceHelper.ContextMenu(Project, value);
                ImGui.EndPopup();
            }
        }
    }

    private static string Format(object value) => value switch
    {
        float f => f.ToString("R", CultureInfo.InvariantCulture),
        double d => d.ToString("R", CultureInfo.InvariantCulture),
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => value?.ToString() ?? ""
    };

    /// <summary>Sets a cell to a value (converted to the field's type) as an undoable Param Editor action</summary>
    private void Commit(Param.Cell cell, object input)
    {
        var current = cell.Value;
        object newValue;

        try
        {
            newValue = current is string
                ? Convert.ToString(input, CultureInfo.InvariantCulture) ?? ""
                : Convert.ChangeType(input is string s ? s.Trim() : input, current.GetType(), CultureInfo.InvariantCulture);
        }
        catch (Exception)
        {
            Smithbox.Log(this, LOC.Get("PARAM_FieldInput_Invalid_Value_Rejected", input, current), LogLevel.Warning);
            return;
        }

        if (Equals(newValue, current))
            return;

        if (View.Editor.ActionManager is not { } actions)
            return;

        var action = new PropertiesChangedAction(CellValue, cell, newValue)
        {
            DisplayName = LOC.Get("PARAM_Action_Name_Edit_Field", cell.Def.InternalName, cell.GetRow().ID)
        };

        actions.ExecuteAction(action);
    }
}
