using StudioCore.Application;
using StudioCore.Editors.ParamEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace StudioCore.Developer;

public static class ParamMetadata
{
    public static void Apply()
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;

    }

    public static void UpdateAnnotations(ProjectEntry curProject)
    {
        if (curProject.Handler == null)
            return;

        var type = ProjectUtils.GetGameDirectory(curProject.Descriptor.ProjectType);

        var outputFolder = $@"C:\Users\benja\Programming\Reference\Annotations\{type}";

        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        var paramData = curProject.Handler.ParamData;

        var options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true,
            IncludeFields = true
        };

        foreach (var entry in paramData.PrimaryBank.Params)
        {
            var key = entry.Key;

            var def = entry.Value.AppliedParamdef;
            var meta = paramData.GetParamMeta(entry.Value.AppliedParamdef);

            var annotationEntry = paramData.GetParamAnnotations(entry.Value.AppliedParamdef.ParamType);

            annotationEntry.Param = entry.Value.AppliedParamdef.ParamType;
            annotationEntry.Type = entry.Value.AppliedParamdef.ParamType;
            annotationEntry.Name = entry.Value.AppliedParamdef.ParamType;

            var outputPath = Path.Combine(outputFolder, $"{entry.Value.AppliedParamdef.ParamType}.json");

            var jsonString = JsonSerializer.Serialize(annotationEntry, typeof(ParamAnnotationEntry), options);

            File.WriteAllText(outputPath, jsonString);
        }
    }

    public static void GenerateAnnotations(ProjectEntry curProject)
    {
        if (curProject.Handler == null)
            return;

        List<string> TypesConsumed = new();

        var type = ProjectUtils.GetGameDirectory(curProject.Descriptor.ProjectType);

        var outputFolder = $@"C:\Users\benja\Programming\Reference\Annotations\{type}";

        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        var paramData = curProject.Handler.ParamData;

        var options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true,
            IncludeFields = true
        };

        foreach (var entry in paramData.PrimaryBank.Params)
        {
            var key = entry.Key;

            var def = entry.Value.AppliedParamdef;
            var meta = paramData.GetParamMeta(entry.Value.AppliedParamdef);

            if (TypesConsumed.Contains(def.ParamType))
                continue;

            TypesConsumed.Add(def.ParamType);

            var outputPath = Path.Combine(outputFolder, $"{def.ParamType}.json");

            var annotationEntry = new ParamAnnotationEntry();

            annotationEntry.Param = def.ParamType;
            annotationEntry.Type = def.ParamType;
            annotationEntry.Name = def.ParamType;
            annotationEntry.Description = "";

            foreach (var field in def.Fields)
            {
                var currentEntry = new ParamAnnotationFieldEntry();

                var desc = "";
                if (field.Description != null)
                    desc = field.Description;

                currentEntry.Field = field.InternalName;
                currentEntry.Name = field.DisplayName;
                currentEntry.Description = desc;

                annotationEntry.Fields.Add(currentEntry);
            }

            var jsonString = JsonSerializer.Serialize(annotationEntry, typeof(ParamAnnotationEntry), options);

            File.WriteAllText(outputPath, jsonString);
        }
    }

    public static void GenerateFieldLayouts(ProjectEntry curProject)
    {
        if (curProject.Handler == null)
            return;

        var type = ProjectUtils.GetGameDirectory(curProject.Descriptor.ProjectType);

        var outputFolder = $@"C:\Users\benja\Programming\Reference\Meta\{type}";

        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        var paramData = curProject.Handler.ParamData;

        var options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true,
            IncludeFields = true
        };

        foreach (var entry in paramData.PrimaryBank.Params)
        {
            var key = entry.Key;

            var meta = paramData.GetParamMeta(entry.Value.AppliedParamdef);

            var parts = meta.AlternateOrder;

            if (meta.AlternateOrder == null)
                continue;

            var outputPath = Path.Combine(outputFolder, $"{key}.json");

            var newEntry = true;

            var layout = new FieldLayout();

            layout.Name = key;

            var currentEntry = new FieldLayoutEntry();

            foreach (var part in parts)
            {
                if (newEntry)
                {
                    layout.Groups.Add(currentEntry);

                    currentEntry = new FieldLayoutEntry();

                    var nameEntry = new FieldLayoutNameEntry();
                    nameEntry.Language = "English";
                    nameEntry.Name = "TODO";

                    currentEntry.Names.Add(nameEntry);

                    currentEntry.Key = $"TODO";
                    currentEntry.Fields = new();

                    newEntry = false;
                }

                if (part == "-")
                {
                    newEntry = true;
                    continue;
                }

                currentEntry.Fields.Add(part);
            }

            var jsonString = JsonSerializer.Serialize(layout, typeof(FieldLayout), options);
            File.WriteAllText(outputPath, jsonString);

        }
    }

    public static void ConvertEnums(ProjectEntry curProject)
    {
        if (curProject.Handler == null)
            return;

        var type = ProjectUtils.GetGameDirectory(curProject.Descriptor.ProjectType);

        var outputFolder = $@"C:\Users\benja\Programming\Reference\Meta\{type}";


        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        var paramData = curProject.Handler.ParamData;

        var options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true,
            IncludeFields = true
        };

        foreach (var entry in paramData.ParamMeta)
        {
            var meta = entry.Value;

            var enums = meta.ParamEnums.Values;

            foreach (var enumEntry in enums)
            {
                var outputPath = Path.Combine(outputFolder, $"{enumEntry.Name}.json");

                var newEnum = new ParamEnumEntry(enumEntry.Name, enumEntry.Name);

                foreach (var opt in enumEntry.Values)
                {
                    var newOpt = new ParamEnumOption(opt.Key, opt.Value);
                    newEnum.Options.Add(newOpt);
                }

                var jsonString = JsonSerializer.Serialize(newEnum, typeof(ParamEnumEntry), options);

                File.WriteAllText(outputPath, jsonString);
            }
        }
    }

    public static void ConvertParamMeta(ProjectEntry curProject)
    {
        if (curProject.Handler == null)
            return;

        var type = ProjectUtils.GetGameDirectory(curProject.Descriptor.ProjectType);

        var outputFolder = $@"C:\Users\benja\Programming\Reference\Meta\{type}";

        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        var paramData = curProject.Handler.ParamData;

        var options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true,
            IncludeFields = true
        };

        foreach (var entry in paramData.ParamMeta)
        {
            var meta = entry.Value;

            var outputPath = Path.Combine(outputFolder, $"{meta.Name}");

            var output = SerializeToXmlWithoutAltNameAndWiki(meta);

            File.WriteAllText(outputPath, output, Encoding.Unicode);
        }
    }

    public static string SerializeToXmlWithoutAltNameAndWiki(ParamMeta meta)
    {
        XmlDocument doc = new();
        XmlElement root = doc.CreateElement("PARAMMETA");
        root.SetAttribute("XmlVersion", ParamMeta.XML_VERSION.ToString());
        doc.AppendChild(root);

        // --- Self node ---
        XmlElement self = doc.CreateElement("Self");

        if (meta.BlockSize != 0)
            self.SetAttribute("BlockSize", meta.BlockSize.ToString());

        if (meta.BlockStart != 0)
            self.SetAttribute("BlockStart", meta.BlockStart.ToString());

        if (meta.ConsecutiveIDs)
            self.SetAttribute("ConsecutiveIDs", "");

        if (meta.OffsetSize != 0)
            self.SetAttribute("OffsetSize", meta.OffsetSize.ToString());

        if (meta.FixedOffset != 0)
            self.SetAttribute("FixedOffset", meta.FixedOffset.ToString());

        if (meta.Row0Dummy)
            self.SetAttribute("Row0Dummy", "");

        if (!string.IsNullOrEmpty(meta.FieldLayout))
            self.SetAttribute("FieldLayout", meta.FieldLayout);

        if (meta.AlternateOrder != null)
            self.SetAttribute("AlternativeOrder", string.Join(", ", meta.AlternateOrder));

        if (meta.CalcCorrectDef != null)
            self.SetAttribute("CalcCorrectDef", meta.CalcCorrectDef.getStringForm());

        if (meta.SoulCostDef != null)
            self.SetAttribute("SoulCostDef", meta.SoulCostDef.getStringForm());

        // Wiki intentionally omitted

        root.AppendChild(self);

        // --- Enums node ---
        //if (meta.ParamEnums.Any())
        //{
        //    XmlElement enums = doc.CreateElement("Enums");
        //    foreach (var (_, paramEnum) in meta.ParamEnums)
        //    {
        //        XmlElement enumEl = doc.CreateElement("Enum");
        //        enumEl.SetAttribute("Name", paramEnum.Name);
        //        foreach (var (value, name) in paramEnum.Values)
        //        {
        //            XmlElement option = doc.CreateElement("Option");
        //            option.SetAttribute("Value", value);
        //            option.SetAttribute("Name", name);
        //            enumEl.AppendChild(option);
        //        }
        //        enums.AppendChild(enumEl);
        //    }
        //    root.AppendChild(enums);
        //}

        // --- ColorEdit node ---
        if (meta.ColorEditors.Any())
        {
            XmlElement colorEdit = doc.CreateElement("ColorEdit");
            foreach (var ce in meta.ColorEditors)
            {
                XmlElement colorEditor = doc.CreateElement("ColorEditor");
                colorEditor.SetAttribute("Name", ce.Name);
                colorEditor.SetAttribute("Fields", ce.Fields);
                colorEditor.SetAttribute("PlacedField", ce.PlacedField);
                colorEdit.AppendChild(colorEditor);
            }
            root.AppendChild(colorEdit);
        }

        // --- DisplayNames node ---
        //if (meta.DisplayNames.Any())
        //{
        //    XmlElement displayNames = doc.CreateElement("DisplayNames");
        //    foreach (var dn in meta.DisplayNames)
        //    {
        //        XmlElement nameEntry = doc.CreateElement("NameEntry");
        //        nameEntry.SetAttribute("Param", dn.Param);
        //        nameEntry.SetAttribute("Name", dn.Name);
        //        displayNames.AppendChild(nameEntry);
        //    }
        //    root.AppendChild(displayNames);
        //}

        // --- Fields node ---
        XmlElement fields = doc.CreateElement("Field");
        foreach (var (paramdefField, fieldMeta) in meta.Fields)
        {
            XmlElement fieldEl = doc.CreateElement(SanitizeFieldName(paramdefField.InternalName));

            // AltName and Wiki intentionally omitted

            if (fieldMeta.DefaultValue != null)
                fieldEl.SetAttribute("DefaultValue", fieldMeta.DefaultValue);

            if (fieldMeta.TileRef != null)
                fieldEl.SetAttribute("TileRef", fieldMeta.TileRef);

            if (fieldMeta.RefTypes != null)
                fieldEl.SetAttribute("Refs", string.Join(",", fieldMeta.RefTypes.Select(r => r.getStringForm())));

            if (fieldMeta.VirtualRef != null)
                fieldEl.SetAttribute("VRef", fieldMeta.VirtualRef);

            if (fieldMeta.FmgRef != null)
                fieldEl.SetAttribute("FmgRef", string.Join(",", fieldMeta.FmgRef.Select(r => r.getStringForm())));

            if (fieldMeta.FmgRefRoleOverride != null)
                fieldEl.SetAttribute("FmgRefRoleOverride", fieldMeta.FmgRefRoleOverride);

            if (fieldMeta.MapFmgRef != null)
                fieldEl.SetAttribute("MapFmgRef", "");

            if (fieldMeta.IconConfig != null)
                fieldEl.SetAttribute("IconConfig", fieldMeta.IconConfig.getStringForm());

            if (fieldMeta.EnumType != null)
                fieldEl.SetAttribute("Enum", fieldMeta.EnumType.Name);

            if (fieldMeta.ShowProjectEnumList)
                fieldEl.SetAttribute("Enum", fieldMeta.ProjectEnumType);

            if (fieldMeta.IsBool)
                fieldEl.SetAttribute("IsBool", "");

            if (fieldMeta.ExtRefs != null)
                fieldEl.SetAttribute("ExtRefs", string.Join(";", fieldMeta.ExtRefs.Select(r => r.getStringForm())));

            if (fieldMeta.IsInvertedPercentage)
                fieldEl.SetAttribute("IsInvertedPercentage", "");

            if (fieldMeta.IsPaddingField)
                fieldEl.SetAttribute("Padding", "");

            if (fieldMeta.AddSeparatorNextLine)
                fieldEl.SetAttribute("Separator", "");

            if (fieldMeta.IsObsoleteField)
                fieldEl.SetAttribute("Obsolete", "");

            if (fieldMeta.ShowParticleEnumList)
                fieldEl.SetAttribute("ParticleAlias", "");

            if (fieldMeta.ShowSoundEnumList)
                fieldEl.SetAttribute("SoundAlias", "");

            if (fieldMeta.ShowFlagEnumList)
            {
                var flagVal = string.IsNullOrEmpty(fieldMeta.FlagAliasEnum_ConditionalField)
                    ? ""
                    : $"{fieldMeta.FlagAliasEnum_ConditionalField}={fieldMeta.FlagAliasEnum_ConditionalValue}";
                fieldEl.SetAttribute("FlagAlias", flagVal);
            }

            if (fieldMeta.ShowCutsceneEnumList)
                fieldEl.SetAttribute("CutsceneAlias", "");

            if (fieldMeta.ShowMovieEnumList)
            {
                var movieVal = string.IsNullOrEmpty(fieldMeta.MovieAliasEnum_ConditionalField)
                    ? ""
                    : $"{fieldMeta.MovieAliasEnum_ConditionalField}={fieldMeta.MovieAliasEnum_ConditionalValue}";
                fieldEl.SetAttribute("MovieAlias", movieVal);
            }

            if (fieldMeta.ShowParamFieldOffset)
                fieldEl.SetAttribute("ParamFieldOffset", fieldMeta.ParamFieldOffsetIndex);

            if (fieldMeta.DeepCopyTargetType != null)
                fieldEl.SetAttribute("DeepCopyTarget", string.Join(",", fieldMeta.DeepCopyTargetType));

            if (fieldMeta.ShowCharacterEnumList)
                fieldEl.SetAttribute("CharacterAlias", "");

            if (fieldMeta.RefGroup != null)
                fieldEl.SetAttribute("RefGroup", fieldMeta.RefGroup);

            fields.AppendChild(fieldEl);
        }
        root.AppendChild(fields);

        // --- AlternateRowOrders node ---
        if (meta.AlternateRowOrders != null)
        {
            XmlElement rowOrders = doc.CreateElement("AlternateRowOrders");
            foreach (var (name, order) in meta.AlternateRowOrders)
            {
                XmlElement orderEl = doc.CreateElement(name);
                orderEl.SetAttribute("Order", string.Join(", ", order));
                rowOrders.AppendChild(orderEl);
            }
            root.AppendChild(rowOrders);
        }

        // Pretty-print
        using var sw = new StringWriter();
        using var xw = XmlWriter.Create(sw, new XmlWriterSettings { Indent = true });
        doc.WriteTo(xw);
        xw.Flush();
        return sw.ToString();
    }

    private static string SanitizeFieldName(string internalName)
    {
        var name = Regex.Replace(internalName, @"[^a-zA-Z0-9_]", "");
        return Regex.IsMatch(name, @"^\d") ? "_" + name : name;
    }
}
