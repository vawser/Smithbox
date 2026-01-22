using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;


public static class ParamDebugTools
{
    public static string ProjectFolder = @"C:\Users\benja\Programming\C#\Smithbox";

    public static void DisplayQuickTableNameExport(ParamEditorScreen editor, ProjectEntry project)
    {
        var activeView = editor.ViewHandler.ActiveView;

        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Copyright}##tableNameExport"))
        {
            var dir = Path.Combine(ProjectFolder,
                "src", "Smithbox.Data", "Assets", "PARAM",
                ProjectUtils.GetGameDirectory(project), "Community Table Names");

            activeView.ParamTableWindow.WriteTableGroupNames(dir);

            TaskLogs.AddLog($"Exported table names to {dir}");
        }
        UIHelper.Tooltip("Export the current table names for the current param directly to the Smithbox.Data folder.");
    }

    public static void DisplayQuickRowNameExport(ParamEditorScreen editor, ProjectEntry project)
    {
        var activeView = editor.ViewHandler.ActiveView;

        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Copyright}"))
        {
            var dir = Path.Combine(ProjectFolder,
                "src", "Smithbox.Data", "Assets", "PARAM",
                ProjectUtils.GetGameDirectory(project), "Community Row Names");

            var curParam = activeView.Selection.GetActiveParam();

            var store = new RowNameStore();
            store.Params = new();

            foreach (KeyValuePair<string, Param> p in activeView.GetPrimaryBank().Params)
            {
                if (curParam != "")
                {
                    if (p.Key != curParam)
                        continue;
                }

                var paramEntry = new RowNameParam();
                paramEntry.Name = p.Key;
                paramEntry.Entries = new();

                var groupedRows = p.Value.Rows
                    .GroupBy(r => r.ID)
                    .ToDictionary(g => g.Key, g => g.Select(r => r.Name ?? "").ToList());

                paramEntry.Entries = groupedRows.Select(kvp => new RowNameEntry
                {
                    ID = kvp.Key,
                    Entries = kvp.Value
                }).ToList();

                var fullPath = Path.Combine(dir, $"{p.Key}.json");

                var options = new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = true,
                    IncludeFields = true
                };
                var json = JsonSerializer.Serialize(paramEntry, typeof(RowNameParam), options);

                File.WriteAllText(fullPath, json);

                TaskLogs.AddLog($"Exported row names to {fullPath}");
            }
        }
        UIHelper.Tooltip("Export the current row names for the current param directly to the Smithbox.Data folder.");
    }


    public static void OutputParamTableInformation(ParamEditorScreen editor, ProjectEntry curProject)
    {
        var output = "^ Param ^ Description ^\n";

        foreach (var param in curProject.Handler.ParamData.PrimaryBank.Params)
        {
            var targetParamMeta = editor.Project.Handler.ParamData.GetParamMeta(param.Value.AppliedParamdef);

            var sanitizedWiki = $"{targetParamMeta.Wiki}".Replace("\n", ", ").Replace("|", "-");

            output = output + $"| [[XXX-refmat:param:{param.Key}]] | {sanitizedWiki} |\n";
        }

        PlatformUtils.Instance.SetClipboardText(output);
    }

    public static void OutputParamInformation(ParamEditorScreen editor, ProjectEntry curProject, string paramKey)
    {
        var namespacePrefix = "XXX";

        switch (curProject.Descriptor.ProjectType)
        {
            case ProjectType.DES: namespacePrefix = "des"; break;
            case ProjectType.DS1: namespacePrefix = "ds1"; break;
            case ProjectType.DS1R: namespacePrefix = "ds1"; break;
            case ProjectType.DS2: namespacePrefix = "ds2"; break;
            case ProjectType.DS2S: namespacePrefix = "ds2"; break;
            case ProjectType.DS3: namespacePrefix = "ds3"; break;
            case ProjectType.SDT: namespacePrefix = "sdt"; break;
            case ProjectType.BB: namespacePrefix = "bb"; break;
            case ProjectType.ER: namespacePrefix = "er"; break;
            case ProjectType.AC6: namespacePrefix = "ac6"; break;
        }

        var output = $"====== {paramKey} ======\n" +
            $"===== Fields =====\n" +
            $"^ Field ^ Type ^ Offset ^ Description ^ Notes ^\n";

        var targetParamDef = curProject.Handler.ParamData.PrimaryBank.GetParamFromName(paramKey);
        var targetParamMeta = editor.Project.Handler.ParamData.GetParamMeta(targetParamDef.AppliedParamdef);

        // Fields
        foreach (var field in targetParamDef.AppliedParamdef.Fields)
        {
            var col = targetParamDef.Columns.Where(e => e.Def == field).FirstOrDefault();

            var fieldMeta = editor.Project.Handler.ParamData.GetParamFieldMeta(targetParamMeta, field);

            var notes = "";

            // Param Ref
            if (fieldMeta.RefTypes != null && fieldMeta.RefTypes.Count > 0)
            {
                var references = "";
                foreach (var entry in fieldMeta.RefTypes)
                {
                    if (references == "")
                    {
                        references = $"[[{namespacePrefix}-refmat:param:{entry.ParamName}]]";
                    }
                    else
                    {
                        references = $"{references}, [[{namespacePrefix}-refmat:param:{entry.ParamName}]]";
                    }
                }

                if (notes == "")
                {
                    notes = $"This field refers to the following params: {references}";
                }
                else
                {
                    notes = $"{notes}\n\nThis field refers to the following params: {references}";
                }
            }

            // Text Ref
            if (fieldMeta.FmgRef != null && fieldMeta.FmgRef.Count > 0)
            {
                var references = "";
                foreach (var entry in fieldMeta.FmgRef)
                {
                    if (references == "")
                    {
                        references = $"{entry.fmg}";
                    }
                    else
                    {
                        references = $"{references}, {entry.fmg}";
                    }
                }

                if (notes == "")
                {
                    notes = $"This field refers to the following text files: {references}";
                }
                else
                {
                    notes = $"{notes}\n\nThis field refers to the following text files: {references}";
                }
            }

            // Enum
            if (fieldMeta.EnumType != null)
            {
                if (notes == "")
                {
                    notes = $"This field uses the following enum: {fieldMeta.EnumType.Name}";
                }
                else
                {
                    notes = $"{notes}\n\nThis field uses the following enum: {fieldMeta.EnumType.Name}";
                }
            }

            if (fieldMeta.ProjectEnumType != null)
            {
                if (notes == "")
                {
                    notes = $"This field uses the following enum: {fieldMeta.ProjectEnumType}";
                }
                else
                {
                    notes = $"{notes}\n\nThis field uses the following enum: {fieldMeta.ProjectEnumType}";
                }
            }

            // Bool
            if (fieldMeta.IsBool)
            {
                if (notes == "")
                {
                    notes = $"This field is a boolean.";
                }
                else
                {
                    notes = $"{notes}\n\nThis field is a boolean.";
                }
            }

            // Padding
            if (fieldMeta.IsPaddingField)
            {
                if (notes == "")
                {
                    notes = $"This field is padding.";
                }
                else
                {
                    notes = $"{notes}\n\nThis field is padding.";
                }
            }

            // Event Flag
            if (fieldMeta.ShowFlagEnumList)
            {
                if (notes == "")
                {
                    notes = $"This field takes an [[{namespacePrefix}-refmat:event-flag-list|Event Flag ID]].";
                }
                else
                {
                    notes = $"{notes}\n\nThis field takes an [[{namespacePrefix}-refmat:event-flag-list|Event Flag ID]].";
                }
            }

            // Particle
            if (fieldMeta.ShowParticleEnumList)
            {
                if (notes == "")
                {
                    notes = $"This field takes an [[{namespacePrefix}-refmat:particle-list|Particle ID]].";
                }
                else
                {
                    notes = $"{notes}\n\nThis field takes an [[{namespacePrefix}-refmat:particle-list|Particle ID]].";
                }
            }

            var sanitizedWiki = $"{fieldMeta.Wiki}".Replace("\n", " ").Replace("|", "-").Replace("^", "<nowiki>^</nowiki>");

            var colString = "";

            if (col != null)
            {
                var offS = col.GetBitOffset();

                if (col.Def.BitSize == -1)
                {
                    colString = col.GetByteOffset().ToString("x");
                }
                else if (col.Def.BitSize == 1)
                {
                    colString = $"{col.GetByteOffset().ToString("x")} [{offS}]";
                }
                else
                {
                    colString = $"{col.GetByteOffset().ToString("x")} [{offS}-{offS + col.Def.BitSize - 1}]";
                }
            }

            output = output + $"| {field.InternalName} | ''{field.DisplayType}'' | ''0x{colString}'' | {sanitizedWiki} | {notes} |\n";
        }

        if (targetParamMeta.ParamEnums.Count > 0)
        {
            output = $"{output}\n\n\n===== Enums =====\n";

            // Enums
            foreach (var entry in targetParamMeta.ParamEnums)
            {
                output = $"{output}\n\n==== {entry.Key} ====\n" +
                    $"^ Option ^ Description ^ Notes ^\n";

                foreach (var opt in entry.Value.Values)
                {
                    output = output + $"| ''{opt.Key}'' | {opt.Value} | |\n";
                }
            }
        }

        PlatformUtils.Instance.SetClipboardText(output);
    }
}
