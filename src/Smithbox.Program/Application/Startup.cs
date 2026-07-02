using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace StudioCore.Application;

// CFG but before anything has occured.
public class Startup
{
    public static Startup Current { get; set; }
    public static Startup Default { get; } = new();

    #region Parameters

    public bool SetProgramLanguage = false;

    public string Program_Language = "English";

    public RenderingBackend System_RenderingBackend = RenderingBackend.Vulkan;

    public bool System_Check_Program_Update = true;

    public bool System_Enable_Soapstone_Server = true;

    #endregion

    #region Load / Save

    public static void Setup()
    {
        Current = new Startup();
    }

    public static void Load()
    {
        string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var startupDir = Path.Join(localAppDataPath, "Smithbox", "Configuration");
        var startupFile = Path.Combine(startupDir, "Startup.json");

        if (!File.Exists(startupFile))
        {
            Current = new Startup();
            Save();
        }
        else
        {
            try
            {
                var filestring = File.ReadAllText(startupFile);
                Current = JsonSerializer.Deserialize(filestring, ProjectJsonSerializerContext.Default.Startup);
            }
            catch (Exception)
            {
                //Smithbox.LogError<Startup>("Startup configuration failed to load, default configuration has been restored.", e);

                Current = new Startup();
                Save();
            }
        }
    }
    public static void Save()
    {
        string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var startupDir = Path.Join(localAppDataPath, "Smithbox", "Configuration");
        var startupFile = Path.Combine(startupDir, "Startup.json");

        if (!Directory.Exists(startupDir))
        {
            Directory.CreateDirectory(startupDir);
        }

        var json = JsonSerializer.Serialize(Current, ProjectJsonSerializerContext.Default.Startup);

        File.WriteAllText(startupFile, json);
    }

    #endregion
}
