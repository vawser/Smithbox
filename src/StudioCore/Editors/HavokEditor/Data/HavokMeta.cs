using StudioCore.Resource.Locators;
using System;
using System.Collections.Generic;
using System.IO;

namespace StudioCore.Editors.HavokEditor.Data;

public static class HavokMeta
{
    private static Dictionary<string, HavokClassMeta> _meta = new();

    public static void Setup()
    {
        _meta = new();

        var metaPath = $"{AppContext.BaseDirectory}\\Assets\\HAVOK\\{MiscLocator.GetGameIDForDir()}\\Meta";

        //TaskLogs.AddLog($"metaPath: {metaPath}");

        if (Path.Exists(metaPath))
        {
            foreach (var file in Directory.EnumerateFiles(metaPath))
            {
                var currentPath = file;
                var classType = Path.GetFileNameWithoutExtension(file);

                //TaskLogs.AddLog($"currentPath: {currentPath}");

                var newMeta = new HavokClassMeta(currentPath);
                _meta.Add(classType, newMeta);
            }
        }
    }

    public static HavokClassMeta GetClassMeta(string classType)
    {
        if(_meta.ContainsKey(classType))
        {
            return _meta[classType];
        }
        else
        {
            return null;
        }
    }
}
