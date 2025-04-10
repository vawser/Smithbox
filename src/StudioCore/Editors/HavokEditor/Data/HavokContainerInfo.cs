using HKLib.hk2018;
using HKLib.Serialization.hk2018.Binary;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Editors.HavokEditor.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StudioCore.Editors.HavokEditor.Framework;


public class HavokContainerInfo
{
    public HavokContainerType Type { get; set; }

    public BND4 Container { get; set; }
    public Dictionary<string, hkRootLevelContainer> LoadedHavokRoots { get; set; }
    public Dictionary<string, List<IHavokObject>> LoadHavokObjects { get; set; }

    public string Filename { get; set; }
    public bool IsModified { get; set; }

    public string RootBinderPath { get; set; }
    public string ModBinderPath { get; set; }
    public string ModBinderDirectory { get; set; }

    public string BinderDirectory { get; set; }
    public string BinderPath { get; set; }
    public string BinderExtension { get; set; }

    public string BhdPath { get; set; }
    public string BdtPath { get; set; }
    public bool IsBxf { get; set; }


    public bool LoadFromData { get; set; }

    public byte[] Data { get; set; }

    public List<string> InternalFileList { get; set; }

    public HavokContainerInfo(string name, HavokContainerType type)
    {
        Type = type;
        Filename = name;
        IsModified = false;
        LoadedHavokRoots = new();
        LoadHavokObjects = new();

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
        if (LoadFromData)
        {
            // Use mod if it exists
            if (File.Exists(ModBinderPath))
            {
                //TaskLogs.AddLog($"Loaded: {ModBinderPath}");

                try
                {
                    Container = BND4.Read(Data);
                    InternalFileList = new();
                    foreach (var entry in Container.Files)
                    {
                        InternalFileList.Add(entry.Name.ToLower());
                    }
                }
                catch (Exception ex)
                {
                    var filename = Path.GetFileNameWithoutExtension(ModBinderPath);
                    TaskLogs.AddLog($"Failed to read HKX file: {filename} at {ModBinderPath}\n{ex}", LogLevel.Error);
                }
            }
            // Otherwise load root
            else
            {
                //TaskLogs.AddLog($"Loaded: {RootBinderPath}");

                try
                {
                    Container = BND4.Read(Data);
                    InternalFileList = new();
                    foreach (var entry in Container.Files)
                    {
                        InternalFileList.Add(entry.Name.ToLower());
                    }
                }
                catch (Exception ex)
                {
                    var filename = Path.GetFileNameWithoutExtension(ModBinderPath);
                    TaskLogs.AddLog($"Failed to read HKX file: {filename} at {ModBinderPath}\n{ex}", LogLevel.Error);
                }
            }
        }
        else
        {
            // Use mod if it exists
            if (File.Exists(ModBinderPath))
            {
                //TaskLogs.AddLog($"Loaded: {ModBinderPath}");

                try
                {
                    Container = BND4.Read(DCX.Decompress(ModBinderPath));
                    InternalFileList = new();
                    foreach (var entry in Container.Files)
                    {
                        InternalFileList.Add(entry.Name.ToLower());
                    }
                }
                catch (Exception ex)
                {
                    var filename = Path.GetFileNameWithoutExtension(ModBinderPath);
                    TaskLogs.AddLog($"Failed to read HKX file: {filename} at {ModBinderPath}\n{ex}", LogLevel.Error);
                }
            }
            // Otherwise load root
            else
            {
                //TaskLogs.AddLog($"Loaded: {RootBinderPath}");

                try
                {
                    Container = BND4.Read(DCX.Decompress(RootBinderPath));
                    InternalFileList = new();
                    foreach (var entry in Container.Files)
                    {
                        InternalFileList.Add(entry.Name.ToLower());
                    }
                }
                catch (Exception ex)
                {
                    var filename = Path.GetFileNameWithoutExtension(ModBinderPath);
                    TaskLogs.AddLog($"Failed to read HKX file: {filename} at {ModBinderPath}\n{ex}", LogLevel.Error);
                }
            }
        }
    }

    /// <summary>
    /// Loads havok file (from within container) for usage with editor
    /// </summary>
    /// <param name="key"></param>
    public hkRootLevelContainer ReadHavokRoot(string fileKey, string internalType)
    {
        var loadedFileKey = $"{internalType}--{fileKey}";

        if (LoadedHavokRoots.ContainsKey(loadedFileKey))
        {
            return LoadedHavokRoots[loadedFileKey];
        }
        else
        {
            foreach (var file in Container.Files)
            {
                if (file.Name.ToLower().Contains(fileKey.ToLower()))
                {
                    var fileName = file.Name.ToLower();
                    var fileBytes = file.Bytes;

                    HavokBinarySerializer serializer = new HavokBinarySerializer();
                    using (MemoryStream memoryStream = new MemoryStream(fileBytes.ToArray()))
                    {
                        var container = (hkRootLevelContainer)serializer.Read(memoryStream);

                        if (LoadedHavokRoots.ContainsKey(loadedFileKey))
                        {
                            LoadedHavokRoots[loadedFileKey] = container;
                        }
                        else
                        {
                            LoadedHavokRoots.Add(loadedFileKey, container);
                        }

                        return LoadedHavokRoots[loadedFileKey];
                    }
                }
            }
        }
        return null;
    }

    public List<IHavokObject> ReadHavokObjects(string fileKey, string internalType)
    {
        var loadedFileKey = $"{internalType}--{fileKey}";

        if (LoadHavokObjects.ContainsKey(loadedFileKey))
        {
            return LoadHavokObjects[loadedFileKey];
        }
        else
        {
            foreach (var file in Container.Files)
            {
                if (file.Name.ToLower().Contains(fileKey.ToLower()))
                {
                    var fileName = file.Name.ToLower();
                    var fileBytes = file.Bytes;

                    HavokBinarySerializer serializer = new HavokBinarySerializer();
                    using (MemoryStream memoryStream = new MemoryStream(fileBytes.ToArray()))
                    {
                        var objects = serializer.ReadAllObjects(memoryStream).ToList();

                        if (LoadHavokObjects.ContainsKey(loadedFileKey))
                        {
                            LoadHavokObjects[loadedFileKey] = objects;
                        }
                        else
                        {
                            LoadHavokObjects.Add(loadedFileKey, objects);
                        }

                        return LoadHavokObjects[loadedFileKey];
                    }
                }
            }
        }

        return null;
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
            TaskLogs.AddLog($"HKX container filepath does not exist: {RootBinderPath}", LogLevel.Error);
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