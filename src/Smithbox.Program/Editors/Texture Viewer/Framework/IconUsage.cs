using Andre.Formats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.ParamEditor;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.TextureViewer;

/// <summary>A param field that shows a layout cell as its icon, or would if another cell didn't hide it.</summary>
public record IconUse(string Param, int RowId, string RowName, string Field, string Group, int IconId, IconSource ShownInstead);

/// <summary>
/// Used By for layout cells (Texture Editor plan, T7): the param rows whose icon field the Param Editor shows with a
/// cell. A cell is read as (prefix, id) by each Icon Configuration group, exactly as the icon index reads it; a row's
/// field is listed when the group's index holds the cell for that key and the row's value is the id. When another
/// cell wins the lookup (an earlier sheet with the same id, or AC6's weapon prefixes tried in order), the row is
/// listed with the cell it shows instead.
///
/// The param fields with an icon configuration are found once and then on Refresh, as param edits don't notify the
/// Texture Editor; the rows are read when the selected cell changes. Low sheets are not looked up for icons.
/// </summary>
public class IconUsage
{
    private readonly ProjectEntry Project;

    public IconUsage(ProjectEntry project)
    {
        Project = project;
    }

    private record IconField(string ParamName, Param Param, Param.Column Column, string Group);

    private List<IconField> Fields;

    public bool ParamsAvailable => Project.Handler?.ParamData?.PrimaryBank?.Params != null;

    // The last answer, by cell, its name and the index it was found in
    private SubTexture ForCell;
    private string ForName;
    private IconIndex ForIndex;
    private List<IconUse> Uses = new();

    /// <summary>Forgets the fields and the last answer, so the params are read again.</summary>
    public void Refresh()
    {
        Fields = null;
        ForCell = null;
    }

    private void ScanFields()
    {
        Fields = new();

        var data = Project.Handler?.ParamData;
        var bank = data?.PrimaryBank;
        if (bank?.Params == null)
            return;

        foreach (var (name, param) in bank.Params)
        {
            if (param?.AppliedParamdef == null)
                continue;

            var meta = data.GetParamMeta(param.AppliedParamdef);

            foreach (var column in param.Columns)
            {
                var group = data.GetParamFieldMeta(meta, column.Def)?.IconConfig?.TargetConfiguration;
                if (!string.IsNullOrEmpty(group))
                    Fields.Add(new IconField(name, param, column, group));
            }
        }
    }

    /// <summary>
    /// The rows that show the cell (and those it would show but for another cell). Null when it can't be answered
    /// yet: no params, or the icon index is still being built.
    /// </summary>
    public List<IconUse> For(SubTexture cell)
    {
        if (cell == null || !ParamsAvailable)
            return null;

        var groups = Project.Handler.ParamData.IconConfigurations?.Groups;
        if (groups == null)
            return null;

        var index = Smithbox.TextureManager?.IconManager?.GetCurrentIndex(Project);
        if (index == null)
            return null;

        if (ReferenceEquals(cell, ForCell) && cell.Name == ForName && ReferenceEquals(index, ForIndex))
            return Uses;

        if (Fields == null)
            ScanFields();

        var uses = new List<IconUse>();

        foreach (var group in groups)
        {
            var fields = Fields.Where(f => f.Group == group.Name).ToList();
            if (fields.Count == 0)
                continue;

            foreach (var (prefix, id) in IconIndex.ReadCellName(group, cell.Name))
            {
                // Is this cell the group's entry for the key, or one it hides? Otherwise the group doesn't see it
                // (a restricted group, or a sheet only another group lists)
                var entry = index.Lookup(group, prefix, id);
                if (entry == null || (entry.SubTexture != cell && !entry.Shadowed.Any(s => s.SubTexture == cell)))
                    continue;

                foreach (var field in fields)
                {
                    foreach (var row in field.Param.Rows)
                    {
                        if (!int.TryParse($"{field.Column.GetValue(row)}", out var value) || value != id)
                            continue;

                        // AC6 armor reads the prefix from the row's slot flags; weapons try theirs in order
                        var prefixes = IconIndex.GetPrefixes(group, row);
                        if (!prefixes.Contains(prefix))
                            continue;

                        var shown = index.Find(group, prefixes, id);
                        uses.Add(new IconUse(field.ParamName, row.ID, row.Name ?? "", field.Column.Def.InternalName, group.Name, id,
                            shown == null || shown.SubTexture == cell ? null : shown));
                    }
                }
            }
        }

        Uses = uses
            .DistinctBy(u => (u.Param, u.RowId, u.Field))
            .OrderBy(u => u.ShownInstead != null)
            .ThenBy(u => u.Param)
            .ThenBy(u => u.RowId)
            .ToList();

        ForCell = cell;
        ForName = cell.Name;
        ForIndex = index;
        return Uses;
    }

    /// <summary>Opens the row in the Param Editor.</summary>
    public static void JumpToParam(IconUse use) =>
        EditorCommandQueue.AddCommand($@"param/select/-1/{use.Param}/{use.RowId}");
}
