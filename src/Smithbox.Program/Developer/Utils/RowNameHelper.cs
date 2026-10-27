using Octokit;
using StudioCore.Editors.ParamEditor;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace StudioCore.Application;

public static class RowNameHelper
{
    public static void TranslateFromCsvToJson()
    {
        var csvSrc = @"C:\Users\benja\Programming\Dump\gameparam_dlc02";

        foreach(var file in Directory.EnumerateFiles(csvSrc))
        {
            var filename = Path.GetFileNameWithoutExtension(file);

            var newRowNameEntry = new RowNameParam
            {
                Name = filename
            };

            try
            {
                var filestring = File.ReadAllText(file);

                var entries = filestring.Split("\n");

                for(int i = 1; i < entries.Length; i++)
                {
                    var curEntry = entries[i];

                    var parts = curEntry.Split(CFG.Current.Param_Import_Delimiter);
                    if (parts.Length > 1)
                    {
                        var id = parts[0];
                        var name = parts[1];

                        var newRowName = new RowNameEntry()
                        {
                            ID = int.Parse(id),
                            Entries = new()
                            {
                                name.Replace("\"", "").Trim()
                            }
                        };

                        newRowNameEntry.Entries.Add(newRowName);
                    }
                }

                // Write out row name json
                var options = new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = true,
                    IncludeFields = true
                };
                var json = JsonSerializer.Serialize(newRowNameEntry, typeof(RowNameParam), options);

                var outputDir = $@"C:\Users\benja\Programming\C#\Smithbox\src\Smithbox.Data\Assets\PARAM\ER\Param Row Names\Japanese\{filename}.json";

                File.WriteAllText(outputDir, json);
            }
            catch (Exception e)
            {
            }
        }
    }
}
