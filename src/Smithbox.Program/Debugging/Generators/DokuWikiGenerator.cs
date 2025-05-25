using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Interface;
using StudioCore.Platform;
using System.Linq;

namespace StudioCore.Debug.Generators;

public static class DokuWikiGenerator
{
    public static void Display(Smithbox baseEditor, ProjectEntry project)
    {
        if (project == null)
            return;

        if (project.ParamEditor == null)
            return;

        if (ImGui.Button("Output Param Table Information"))
        {
            OutputParamTableInformation(baseEditor, project);
        }

        UIHelper.SimpleHeader("paramList", "Specific Param Information", "", UI.Current.ImGui_AliasName_Text);

        foreach (var param in project.ParamData.PrimaryBank.Params)
        {
            var paramKey = param.Key;

            if (ImGui.Selectable($"{paramKey}"))
            {
                OutputParamInformation(baseEditor, project, paramKey);
            }
        }
    }

    public static void OutputParamTableInformation(Smithbox baseEditor, ProjectEntry curProject)
    {
        var editor = baseEditor.ProjectManager.SelectedProject.ParamEditor;

        var output = "^ Param ^ Description ^\n";

        foreach(var param in curProject.ParamData.PrimaryBank.Params)
        {
            var targetParamMeta = editor.Project.ParamData.GetParamMeta(param.Value.AppliedParamdef);

            var sanitizedWiki = $"{targetParamMeta.Wiki}".Replace("\n", ", ").Replace("|", "-");

            output = output + $"| [[XXX-refmat:param:{param.Key}]] | {sanitizedWiki} |\n";
        }

        PlatformUtils.Instance.SetClipboardText(output);
    }

    public static void OutputParamInformation(Smithbox baseEditor, ProjectEntry project, string paramKey)
    {
        var editor = baseEditor.ProjectManager.SelectedProject.ParamEditor;

        var namespacePrefix = "XXX";

        switch(project.ProjectType)
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

        var targetParamDef = project.ParamData.PrimaryBank.GetParamFromName(paramKey);
        var targetParamMeta = editor.Project.ParamData.GetParamMeta(targetParamDef.AppliedParamdef);

        // Fields
        foreach (var field in targetParamDef.AppliedParamdef.Fields)
        {
            var col = targetParamDef.Columns.Where(e => e.Def == field).FirstOrDefault();

            var fieldMeta = editor.Project.ParamData.GetParamFieldMeta(targetParamMeta, field);

            var notes = "";

            // Param Ref
            if(fieldMeta.RefTypes != null && fieldMeta.RefTypes.Count > 0)
            {
                var references = "";
                foreach(var entry in fieldMeta.RefTypes)
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

                if(notes == "")
                {
                    notes = $"This field refers to the following params: {references}";
                }
                else
                {
                    notes = $"{notes}\n\nThis field refers to the following params: {references}";
                }
            }

            // Text Ref
            if(fieldMeta.FmgRef != null && fieldMeta.FmgRef.Count > 0)
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

            if(fieldMeta.ProjectEnumType != null)
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
            if(fieldMeta.IsBool)
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
            if(fieldMeta.ShowParticleEnumList)
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
