using HKLib.hk2018;
using HKLib.hk2018.hk;
using HKLib.Serialization.hk2018;
using HKLib.Serialization.hk2018.Binary;
using HKLib.Serialization.hk2018.Binary.Util;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Locators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.HavokEditor;
public static class HavokFileBank
{
    public static bool IsLoaded { get; private set; }
    public static bool IsLoading { get; private set; }

    public static List<HavokContainerInfo> BehaviorContainerBank { get; private set; } = new();

    public static void LoadAllHavokFiles()
    {
        // Behaviors
        List<string> fileNames = MiscLocator.GetBehaviorBinders();
        foreach(var entry in fileNames)
        {
            var filename = Path.GetFileNameWithoutExtension(entry);
            SetupBehaviorContainer(filename);
        }

        IsLoaded = true;
    }

    public static void SetupBehaviorContainer(string filename)
    {
        HavokContainerInfo containerInfo = new(filename, HavokContainerType.Behavior);
        BehaviorContainerBank.Add(containerInfo);
    }

    public static void SaveHavokFiles()
    {
        // Behaviors
        foreach (var info in BehaviorContainerBank)
        {
            SaveHavokFile(info);
        }
    }

    public static void SaveHavokFile(HavokContainerInfo info)
    {
        if (!info.IsModified)
            return;

        info.CopyBinderToMod();

        foreach (var file in info.ContainerBinder.Files)
        {
            var fileBytes = file.Bytes;

            foreach (var entry in info.LoadedHavokFiles)
            {
                if (file.Name.ToLower() == entry.Key.ToLower())
                {
                    HavokBinarySerializer deserializer = new HavokBinarySerializer();

                    // TODO: we need to work out the extra capacity need beforehand, rather than using a fixed +1024
                    using (MemoryStream memoryStream = new MemoryStream(fileBytes.ToArray().Length + 1024))
                    {
                        deserializer.Write(entry.Value, memoryStream);
                        fileBytes = memoryStream.ToArray();
                    }
                }
            }

            file.Bytes = fileBytes;
        }

        byte[] binderBytes;

        switch (Smithbox.ProjectType)
        {
            case ProjectType.ER:
                binderBytes = info.ContainerBinder.Write(DCX.Type.DCX_KRAK);
                break;
            default:
                TaskLogs.AddLog($"Invalid Project Type during Save Havok File");
                return;
        }

        if (Smithbox.ProjectRoot == null && !File.Exists($@"{info.ModBinderPath}.bak") && File.Exists(info.ModBinderPath))
        {
            File.Copy(info.ModBinderPath, $@"{info.ModBinderPath}.bak", true);
        }

        if (binderBytes != null)
        {
            File.WriteAllBytes(info.ModBinderPath, binderBytes);
            TaskLogs.AddLog($"Saved at: {info.ModBinderPath}");
        }
    }
}
