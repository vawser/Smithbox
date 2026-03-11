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
        ConvertParamMeta(curProject);
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

            var xml = SerializeToXml(meta);

            var outputPath = Path.Combine(outputFolder, $"{meta.Name}.json");

            File.WriteAllText(outputPath, xml);
        }
    }
    public static string SerializeToXml(object input)
    {
        XmlSerializer ser = new XmlSerializer(input.GetType(), "");
        string result = string.Empty;

        using (MemoryStream memStm = new MemoryStream())
        {
            ser.Serialize(memStm, input);

            memStm.Position = 0;
            result = new StreamReader(memStm).ReadToEnd();
        }

        return result;
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
