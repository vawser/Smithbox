using Smithbox.Core.MapEditorNS;
using StudioCore.Resource.Locators;
using System;
using System.Collections.Generic;
using System.IO;

namespace StudioCore.Editors.MapEditorNS;

public class MsbMeta
{
    public MapData DataParent;

    private Dictionary<string, MapEntityPropertyMeta> _MsbMetas = new();

    public MsbMeta(MapData data)
    {
        DataParent = data;
    }

    // TODO: async this
    public void Load()
    {
        _MsbMetas = new();

        var metaPath = $"{AppContext.BaseDirectory}\\Assets\\MSB\\{MiscLocator.GetGameIDForDir()}\\Meta";

        if (Path.Exists(metaPath))
        {
            foreach (var folder in Directory.EnumerateDirectories(metaPath))
            {
                var rootType = new DirectoryInfo(folder).Name;

                var typeMetaPath = $"{metaPath}\\{rootType}";

                if (Path.Exists(typeMetaPath))
                {
                    foreach (var file in Directory.EnumerateFiles(typeMetaPath))
                    {
                        var currentPath = file;
                        var specificType = Path.GetFileNameWithoutExtension(file);

                        //TaskLogs.AddLog($"currentPath: {currentPath}");

                        var newMeta = new MapEntityPropertyMeta(currentPath);
                        _MsbMetas.Add($"{rootType}_{specificType}", newMeta);
                    }
                }
            }
        }
    }

    public MapEntityPropertyMeta GetMeta(Type type, bool sharedMeta)
    {
        // Get the strings from the passed type
        var typeString = $"{type}";

        var typeParts = typeString.Split(".");

        if (typeParts.Length > 1)
        {
            var typeSegments = typeParts[1].Split("+");

            if (typeSegments.Length > 2)
            {
                var rootType = typeSegments[1];
                var specificType = typeSegments[2];

                if (sharedMeta)
                    specificType = rootType;

                var key = $"{rootType}_{specificType}";

                if (_MsbMetas.ContainsKey(key))
                    return _MsbMetas[key];
            }
            else if (typeSegments.Length > 1)
            {
                var rootType = typeSegments[1];

                var key = $"{rootType}_{rootType}";

                if (_MsbMetas.ContainsKey(key))
                    return _MsbMetas[key];
            }
        }

        return new MapEntityPropertyMeta();
    }

    /// <summary>
    /// For DS2 MSB params
    /// </summary>
    public MapEntityPropertyMeta GetParamMeta(string paramName)
    {
        if (_MsbMetas.ContainsKey(paramName))
            return _MsbMetas[paramName];

        return new MapEntityPropertyMeta();
    }

    public MapEntityPropertyFieldMeta GetFieldMeta(string field, Type type)
    {
        var rootMeta = GetMeta(type, true);
        var specificMeta = GetMeta(type, false);

        if (specificMeta != null)
        {
            if (specificMeta.Fields.ContainsKey(field))
            {
                return specificMeta.Fields[field];
            }
        }

        if (rootMeta != null)
        {
            if (rootMeta.Fields.ContainsKey(field))
            {
                return rootMeta.Fields[field];
            }
        }

        return new MapEntityPropertyFieldMeta();
    }

    /// <summary>
    /// For DS2 MSB params
    /// </summary>
    public MapEntityPropertyFieldMeta GetParamFieldMeta(string field, string paramName)
    {
        var rootMeta = GetParamMeta(paramName);

        if (rootMeta != null)
        {
            if (rootMeta.Fields.ContainsKey(field))
            {
                return rootMeta.Fields[field];
            }
        }

        return new MapEntityPropertyFieldMeta();
    }
}

