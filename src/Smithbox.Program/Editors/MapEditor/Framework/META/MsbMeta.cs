using Octokit;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editors.ParamEditor;
using StudioCore.Resource.Locators;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace StudioCore.Editors.MapEditor.Framework.META;

public class MsbMeta
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public Dictionary<string, MapEntityPropertyMeta> _MsbMetas = new();

    public MsbMeta(Smithbox baseEditor, ProjectEntry project)
    {
        BaseEditor = baseEditor;
        Project = project;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        _MsbMetas = new();

        var metaPath = $"{AppContext.BaseDirectory}\\Assets\\MSB\\{ProjectUtils.GetGameDirectory(Project)}\\Meta";

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

                        var newMeta = new MapEntityPropertyMeta(currentPath);
                        _MsbMetas.Add($"{rootType}_{specificType}", newMeta);
                    }
                }
            }
        }

        return true;
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

