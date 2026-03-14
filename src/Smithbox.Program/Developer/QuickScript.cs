using Andre.Formats;
using DotNext;
using Microsoft.Extensions.Logging;
using Octokit;
using Org.BouncyCastle.Crypto;
using Pfim;
using SoulsFormats;
using StudioCore.Editors.ParamEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace StudioCore.Application;

public class QuickScript
{
    public static string BuildFolder = "";

    public static void ApplyQuickScript(ProjectEntry curProject)
    {
        UpdateAnnotations(curProject);
    }

    public static void UpdateAnnotations(ProjectEntry curProject)
    {
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

            var outputPath = Path.Combine(outputFolder, $"{key}.json");

            var annotationEntry = new ParamAnnotationEntry();

            annotationEntry.Param = entry.Key;
            annotationEntry.Type = def.ParamType;
            annotationEntry.Name = entry.Key;
            annotationEntry.Description = "";

            foreach (var field in def.Fields)
            {
                var currentEntry = new ParamAnnotationFieldEntry();

                currentEntry.Field = field.InternalName;
                currentEntry.Name = field.DisplayName;
                currentEntry.Description = "";

                annotationEntry.Fields.Add(currentEntry);
            }

            var jsonString = JsonSerializer.Serialize(annotationEntry, typeof(ParamAnnotationEntry), options);

            File.WriteAllText(outputPath, jsonString);
        }
    }

    public static void GenerateFieldLayouts(ProjectEntry curProject)
    {
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
                if(newEntry)
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

                foreach(var opt in enumEntry.Values)
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

    //public static void ConvertParamEnums(ProjectEntry curProject)
    //{
    //    var type = ProjectUtils.GetGameDirectory(curProject.Descriptor.ProjectType);

    //    var outputFolder = $@"C:\Users\benja\Programming\Reference\Meta\{type}";

    //    if (!Directory.Exists(outputFolder))
    //    {
    //        Directory.CreateDirectory(outputFolder);
    //    }

    //    var paramData = curProject.Handler.ParamData;

    //    var options = new JsonSerializerOptions
    //    {
    //        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    //        WriteIndented = true,
    //        IncludeFields = true
    //    };

    //    foreach (var entry in paramData.Enums.List)
    //    {
    //        var key = entry.Name;
    //        var title = entry.DisplayName;

    //        ParamEnumEntry newEntry = new(key, title);

    //        foreach (var opt in entry.Options)
    //        {
    //            var optKey = opt.ID;
    //            var optTitle = opt.Name;

    //            ParamEnumOption newOption = new(optKey, optTitle);

    //            newEntry.Options.Add(newOption);
    //        }

    //        var outputPath = Path.Combine(outputFolder, $"{key}.json");

    //        var jsonString = JsonSerializer.Serialize(newEntry, typeof(ParamEnumEntry), options);

    //        File.WriteAllText(outputPath, jsonString);
    //    }
    //}

    public void GenerateParamAnnotations(ProjectEntry curProject)
    {
        //var type = ProjectUtils.GetGameDirectory(curProject.Descriptor.ProjectType);

        //var outputFolder = $@"C:\Users\benja\Programming\C#\Smithbox\src\Smithbox.Data\Assets\PARAM\{type}\Param Annotations\English";

        //if(!Directory.Exists(outputFolder))
        //{
        //    Directory.CreateDirectory(outputFolder);
        //}

        //var meta = curProject.Handler.ParamData.ParamMeta;

        //foreach(var param in curProject.Handler.ParamData.PrimaryBank.Params)
        //{
        //    var curMeta = curProject.Handler.ParamData.GetParamMeta(param.Value.AppliedParamdef);

        //    var newAnnotatioEntry = new ParamAnnotationEntry();
        //    newAnnotatioEntry.Param = param.Key;
        //    newAnnotatioEntry.Type = param.Value.ParamType;
        //    newAnnotatioEntry.Name = param.Key;
        //    newAnnotatioEntry.Description = curMeta.Wiki;

        //    foreach(var field in curMeta.Fields)
        //    {
        //        var newFieldEntry = new ParamAnnotationFieldEntry();
        //        newFieldEntry.Field = field.Key.InternalName;

        //        if (field.Value.AltName != null && field.Value.AltName != "")
        //        {
        //            newFieldEntry.Name = field.Value.AltName;
        //        }
        //        else
        //        {
        //            newFieldEntry.Name = "";
        //        }

        //        if (field.Value.Wiki != null && field.Value.Wiki != "")
        //        {
        //            newFieldEntry.Description = field.Value.Wiki;
        //        }
        //        else
        //        {
        //            newFieldEntry.Description = "";
        //        }

        //        newAnnotatioEntry.Fields.Add(newFieldEntry);
        //    }

        //    var outputPath = Path.Combine(outputFolder, $"{param.Key}.json");

        //    var jsonString = JsonSerializer.Serialize(newAnnotatioEntry, ParamEditorJsonSerializerContext.Default.ParamAnnotationEntry);

        //    File.WriteAllText(outputPath, jsonString);
        //}
    }

    public static void GenerateIconLayouts_DS2()
    {
        TraverseDS2Icons("tex\\icon", "ic_", "/menu/tex/icon", 128, 256, "Item", true);

        TraverseDS2Icons("tex\\icon\\bonfire_area", "ic_area_", "/menu/tex/icon/bonfire_area", 256, 128, "Bonfire Area");

        TraverseDS2Icons("tex\\icon\\bonfire_list", "ic_list_", "/menu/tex/icon/bonfire_list", 256, 128, "Bonfire List");

        TraverseDS2Icons("tex\\icon\\charamaking", "ic_cm_", "/menu/tex/icon/charamaking", 128, 128, "Character Menu");

        TraverseDS2Icons("tex\\icon\\effect", "ei_", "/menu/tex/icon/effect", 64, 64, "Effect");

        TraverseDS2Icons("tex\\icon\\item_category", "ic_ca_", "/menu/tex/icon/item_category", 64, 64, "Item Category");

        // Assuming english here for simplicity
        TraverseDS2Icons("tex\\icon\\mapname\\english", "map_name_", "/menu/tex/icon/mapname/english", 1024, 64, "Map Names");

        TraverseDS2Icons("tex\\icon\\vow", "vi_", "/menu/tex/icon/vow", 64, 64, "Vow");
    }

    public static void TraverseDS2Icons(string path, string prefix, string imagePath, int width, int height, string type, bool special = false)
    {
        var searchPath = $"F:\\SteamLibrary\\steamapps\\common\\Dark Souls II Scholar of the First Sin\\Game\\menu\\{path}";

        foreach (var file in Directory.EnumerateFiles(searchPath))
        {
            var filename = Path.GetFileNameWithoutExtension(file);

            if (filename.StartsWith(prefix))
            {
                var idStr = filename.Replace(prefix, "");

                if (long.TryParse(idStr, out long id))
                {
                    // Adjust the width and height based on the ID (for the items, where the sizing is varied
                    if(special)
                    {
                        width = 128;
                        height = 256;

                        // Armor
                        if (id >= 20000000 && id < 30000000)
                        {
                            // Gaunlets
                            if(id % 100 == 2)
                            {
                                width = 64;
                                height = 128;
                            }
                        }

                        if (id >= 30000000 && id < 1000000000)
                        {
                            width = 64;
                            height = 128;
                        }

                        if (id >= 1800000000 && id < 4000000000)
                        {
                            width = 64;
                            height = 128;
                        }
                        if (id >= 4000000000)
                        {
                            width = 128;
                            height = 128;
                        }
                    }

                    GenerateDS2IconLayout(imagePath, filename, id, width, height, type);
                }
            }
        }
    }

    public static void GenerateDS2IconLayout(string imagePath, string filename, long id, int width, int height, string type)
    {
        if (!Path.Exists(BuildFolder))
        {
            Smithbox.Log<QuickScript>($"Folder doesn't exist: {BuildFolder}", LogLevel.Error);
            return;
        }

        var header = $@"<TextureAtlas imagePath=""{imagePath}/{filename}.tpf"" width=""{width}"" height=""{height}"" type=""{type}"">";
        var footer = @"</TextureAtlas>";

        List<string> lines = new();

        lines.Add(header);

        var line = $@"    <SubTexture name=""ICON_{id}.png"" x=""0"" y=""0"" width=""{width}""  height=""{height}"" originalWidth=""{width}"" originalHeight=""{height}"" half=""0""/>";

        lines.Add(line);

        lines.Add(footer);

        var outputFile = Path.Combine(BuildFolder, $"{filename}.layout");

        File.WriteAllLines(outputFile, lines);
    }

    public static void GenerateIconLayout(string filename, int idStart, int width, int height, int iconIncrement, int resolution)
    {
        if (!Path.Exists(BuildFolder))
        {
            Smithbox.Log<QuickScript>($"Folder doesn't exist: {BuildFolder}", LogLevel.Error);
            return;
        }

        var header = $@"<TextureAtlas imagePath=""{filename}.tif"" width=""{resolution}"" height=""{resolution}"">";
        var footer = @"</TextureAtlas>";

        List<string> lines = new();

        lines.Add(header);

        var curId = idStart;
        var curX = 0;
        var curY = 0;

        // Rows
        for (int i = 0; i < height; i++)
        {
            // Icon in Row
            for (int k = 0; k < width; k++)
            {
                var idStr = "";
                if (curId < 10)
                {
                    idStr = $"0000{curId}";
                }
                else if (curId >= 10 && curId < 100)
                {
                    idStr = $"000{curId}";
                }
                else if (curId >= 100 && curId < 1000)
                {
                    idStr = $"00{curId}";
                }
                else if (curId >= 1000 && curId < 10000)
                {
                    idStr = $"0{curId}";
                }
                else
                {
                    idStr = $"{curId}";
                }

                var line = $@"    <SubTexture name=""ICON_{idStr}.png"" x=""{curX}"" y=""{curY}"" width=""160""  height=""160"" originalWidth=""160"" originalHeight=""160"" half=""0""/>";

                lines.Add(line);

                curX = (iconIncrement * (k + 1));
                curId = curId + 1;
            }

            curX = 0;
            curY = (iconIncrement * (i + 1));
        }

        lines.Add(footer);

        var outputFile = Path.Combine(BuildFolder, $"{filename}.layout");

        File.WriteAllLines(outputFile, lines);
    }
}
