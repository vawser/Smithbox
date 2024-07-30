using HKLib.hk2018;
using HKLib.hk2018.hkAsyncThreadPool;
using HKLib.Serialization.hk2018.Binary;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editors.ModelEditor;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// Represents the 'types' of containers so we can differentiate them within the editor.
/// </summary>
public enum HavokContainerType
{
    [Display(Name = "Behavior")] Behavior,
    [Display(Name = "Collision")] Collision,
    [Display(Name = "Animation")] Animation
}

public class HavokContainerInfo
{
    public HavokContainerType Type { get; set; }

    public BND4 ContainerBinder { get; set; }
    public Dictionary<string, hkRootLevelContainer> LoadedHavokFiles { get; set; }

    public string Filename { get; set; }
    public bool IsModified { get; set; }

    public string RootBinderPath { get; set; }
    public string ModBinderPath { get; set; }
    public string ModBinderDirectory { get; set; }

    public string BinderDirectory { get; set; }
    public string BinderPath { get; set; }
    public string BinderExtension { get; set; }

    public List<string> InternalFileList { get; set; }

    public hkRootLevelContainer LoadedFile { get; set; }

    public HavokContainerInfo(string name, HavokContainerType type)
    {
        Type = type;
        Filename = name;
        IsModified = false;
        LoadedHavokFiles = new();

        BinderDirectory = GetBinderDirectory();
        BinderExtension = GetBinderExtension();
        BinderPath = $"{BinderDirectory}{Filename}{BinderExtension}";

        RootBinderPath = $"{Smithbox.GameRoot}{BinderPath}";
        ModBinderPath = $"{Smithbox.ProjectRoot}{BinderPath}";
        ModBinderDirectory = $"{Smithbox.ProjectRoot}{BinderDirectory}";
    }

    /// <summary>
    /// Loads havok container, preferring project version if it exists.
    /// </summary>
    public void LoadBinder()
    {
        // Use mod if it exists
        if (File.Exists(ModBinderPath))
        {
            TaskLogs.AddLog($"Loaded: {ModBinderPath}");

            ContainerBinder = BND4.Read(DCX.Decompress(ModBinderPath));
            InternalFileList = new();
            foreach (var entry in ContainerBinder.Files)
            {
                InternalFileList.Add(entry.Name.ToLower());
            }
        }
        // Otherwise load root
        else
        {
            TaskLogs.AddLog($"Loaded: {RootBinderPath}");

            ContainerBinder = BND4.Read(DCX.Decompress(RootBinderPath));
            InternalFileList = new();
            foreach (var entry in ContainerBinder.Files)
            {
                InternalFileList.Add(entry.Name.ToLower());
            }
        }
    }

    /// <summary>
    /// Loads havok file (from within container) for usage with editor
    /// </summary>
    /// <param name="key"></param>
    public void LoadFile(string key)
    {
        if (LoadedHavokFiles.ContainsKey(key))
        {
            LoadedFile = LoadedHavokFiles[key];
        }
        else
        {
            foreach (var file in ContainerBinder.Files)
            {
                if (file.Name.ToLower() == key.ToLower())
                {
                    var fileName = file.Name.ToLower();
                    var fileBytes = file.Bytes;

                    HavokBinarySerializer serializer = new HavokBinarySerializer();
                    using (MemoryStream memoryStream = new MemoryStream(fileBytes.ToArray()))
                    {
                        var container = (hkRootLevelContainer)serializer.Read(memoryStream);

                        LoadedHavokFiles.Add(key, container);

                        LoadedFile = LoadedHavokFiles[key];
                    }
                }
            }
        }
    }

    /// <summary>
    /// Copies container to project directory if it does not already exist
    /// </summary>
    /// <returns></returns>
    public bool CopyBinderToMod()
    {
        if (!Directory.Exists(ModBinderDirectory))
        {
            Directory.CreateDirectory(ModBinderDirectory);
        }

        if (File.Exists(RootBinderPath))
        {
            if (!File.Exists(ModBinderPath))
            {
                File.Copy(RootBinderPath, ModBinderPath);
            }
        }
        // Mod-only model, no need to copy to mod
        else if (File.Exists(ModBinderPath))
        {
            return true;
        }
        else
        {
            TaskLogs.AddLog($"Container path does not exist: {RootBinderPath}");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Gets the relevant container directory depending on the Havok Container Type.
    /// </summary>
    /// <returns></returns>
    public string GetBinderDirectory()
    {
        switch (Type)
        {
            case HavokContainerType.Behavior:
                return @"\chr\";
            default: break;
        }

        return "";
    }

    /// <summary>
    /// Gets the relevant container extension depending on the Havok Container Type.
    /// </summary>
    /// <returns></returns>
    public string GetBinderExtension()
    {
        switch (Type)
        {
            case HavokContainerType.Behavior:
                string chrExt = @".behbnd.dcx";

                return chrExt;
            default: break;
        }

        return "";
    }
}