using HKLib.hk2018;
using HKLib.hk2018.hk;
using HKLib.Serialization.hk2018;
using HKLib.Serialization.hk2018.Binary;
using HKLib.Serialization.hk2018.Binary.Util;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Core.Project;
using StudioCore.Editors.HavokEditor.Enums;
using StudioCore.Resource.Locators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.HavokEditor.Framework;

public static class HavokFileBank
{
    public static bool IsLoaded { get; private set; }
    public static bool IsLoading { get; private set; }

    public static List<HavokContainerInfo> BehaviorContainerBank { get; private set; } = new();
    public static List<HavokContainerInfo> CollisionContainerBank { get; private set; } = new();

    public static void LoadAllHavokFiles()
    {
        // Behaviors
        List<string> fileNames = MiscLocator.GetHavokBehaviorBinders();
        foreach (var entry in fileNames)
        {
            var filename = Path.GetFileNameWithoutExtension(entry);
            SetupBehaviorContainer(filename);
        }

        // Collisions
        fileNames = MiscLocator.GetHavokCollisionBinders();
        foreach (var entry in fileNames)
        {
            var filename = Path.GetFileNameWithoutExtension(entry);

            var bhdPath = $"{entry}";
            var bdtPath = $"{entry.Replace("bhd", "bdt")}";

            var reader = new BXF4Reader(bhdPath, bdtPath);

            foreach (var file in reader.Files)
            {
                SetupCollisionContainer(bhdPath, bdtPath, file.Name, reader.ReadFile(file).ToArray());
            }
        }


        IsLoaded = true;
    }

    public static void SetupBehaviorContainer(string filename)
    {
        HavokContainerInfo containerInfo = new(filename, HavokContainerType.Behavior);
        BehaviorContainerBank.Add(containerInfo);
    }

    public static void SetupCollisionContainer(string bhdPath, string bdtPath, string filename, byte[] data)
    {
        HavokContainerInfo containerInfo = new(filename, HavokContainerType.Collision);
        containerInfo.BhdPath = bhdPath;
        containerInfo.BdtPath = bdtPath;
        containerInfo.IsBxf = true;
        containerInfo.LoadFromData = true;
        containerInfo.Data = data;

        CollisionContainerBank.Add(containerInfo);
    }

    public static void SaveHavokFiles()
    {
        // Behaviors
        foreach (var info in BehaviorContainerBank)
        {
            SaveHavokFile(info);
        }

        // Collisions
        foreach (var info in CollisionContainerBank)
        {
            SaveHavokFile(info);
        }
    }

    public static void SaveHavokFile(HavokContainerInfo info)
    {
        //if (!info.IsModified)
        //    return;

        if (info.IsBxf)
        {
            // TODO
        }
        else
        {
            info.CopyBinderToMod();

            foreach (var file in info.Container.Files)
            {
                var fileBytes = file.Bytes;

                foreach (var entry in info.LoadedHavokRoots)
                {
                    if (file.Name.ToLower() == entry.Key.ToLower())
                    {
                        HavokBinarySerializer deserializer = new HavokBinarySerializer();

                        // TODO: we should work out the extra capacity need beforehand, rather than using a fixed +1024
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
                    binderBytes = info.Container.Write(DCX.Type.DCX_KRAK);
                    break;
                default:
                    return;
            }

            if (Smithbox.ProjectRoot == null && !File.Exists($@"{info.ModBinderPath}.bak") && File.Exists(info.ModBinderPath))
            {
                File.Copy(info.ModBinderPath, $@"{info.ModBinderPath}.bak", true);
            }

            if (binderBytes != null)
            {
                var filename = Path.GetFileNameWithoutExtension(info.ModBinderPath);

                File.WriteAllBytes(info.ModBinderPath, binderBytes);
                TaskLogs.AddLog($"Saved HKX container file: {filename} at {info.ModBinderPath}");
            }
        }
    }
}
