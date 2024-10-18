using SoulsFormats;
using StudioCore.Banks.AliasBank;
using StudioCore.Resource.Locators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;


public static class FmgImporter
{
    public static Dictionary<string, FmgWrapper> ImportSources = new();

    /// <summary>
    /// Get the FMG Wrapper sources on project load
    /// </summary>
    public static void OnProjectChanged()
    {
        LoadWrappers();
    }

    /// <summary>
    /// Load the wrappers into the FmgWrapper object and fill the ImportSources dictionary
    /// </summary>
    private static void LoadWrappers()
    {
        ImportSources = new();

        var wrapperPathList = TextLocator.GetFmgWrappers();

        if (wrapperPathList.Count > 0)
        {
            foreach (var path in wrapperPathList)
            {
                var filename = Path.GetFileName(path);
                var wrapper = new FmgWrapper();

                if (File.Exists(path))
                {
                    using (var stream = File.OpenRead(path))
                    {
                        wrapper = JsonSerializer.Deserialize(stream, FmgWrapperSerializationContext.Default.FmgWrapper);
                    }
                }

                if(!ImportSources.ContainsKey(filename))
                {
                    ImportSources.Add(filename, wrapper);
                }
                else
                {
                    TaskLogs.AddLog($"Attempted to add FmgWrapper with existing key!: {filename}");
                }
            }
        }
    }

    /// <summary>
    /// Display the possible import sources for the user to select from
    /// </summary>
    public static void DisplayImportList()
    {
        LoadWrappers();

        // Replace
        // Append
    }

    /// <summary>
    /// Replace the contents of the currently selected FMG with the contents of the selected source
    /// </summary>
    public static void ReplaceEntries()
    {

    }

    /// <summary>
    /// Append contents of the selected source to the contents of the currently selected FMG (respecting ID order)
    /// </summary>
    public static void AppendEntries()
    {

    }
}

