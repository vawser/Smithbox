using Andre.Formats;
using Grpc.Core;
using Octokit;
using SoapstoneLib;
using SoapstoneLib.Proto;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.MapEditor.Enums;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.TextEditor;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static SoapstoneLib.SoulsFmg;

#pragma warning disable CS1998 // Async method lacks 'await'. Return without Task is convenient, though

namespace StudioCore;

/// <summary>
///     Implementation of RPC server for cross-editor communication.
///     For the most part, this does not intentionally return errors when an operation cannot be
///     completed. This may change in the future.
/// </summary>
public class SoapstoneService : SoapstoneServiceV1
{
    private static readonly Dictionary<ProjectType, FromSoftGame> gameMapping = new()
    {
        [ProjectType.DES] = FromSoftGame.DemonsSouls,
        [ProjectType.DS1] = FromSoftGame.DarkSoulsPtde,
        [ProjectType.DS1R] = FromSoftGame.DarkSoulsRemastered,
        [ProjectType.DS2] = FromSoftGame.DarkSouls2,
        [ProjectType.DS2S] = FromSoftGame.DarkSouls2Sotfs,
        [ProjectType.DS3] = FromSoftGame.DarkSouls3,
        [ProjectType.BB] = FromSoftGame.Bloodborne,
        [ProjectType.SDT] = FromSoftGame.Sekiro,
        [ProjectType.ER] = FromSoftGame.EldenRing,
        [ProjectType.AC6] = FromSoftGame.ArmoredCore6
    };

    private static readonly Dictionary<MsbEntityType, KeyNamespace> mapNamespaces = new()
    {
        [MsbEntityType.Event] = KeyNamespace.MapEvent,
        [MsbEntityType.Region] = KeyNamespace.MapRegion,
        [MsbEntityType.Part] = KeyNamespace.MapPart
    };

    private static readonly Dictionary<KeyNamespace, MsbEntityType> revMapNamespaces =
        mapNamespaces.ToDictionary(e => e.Value, e => e.Key);

    private readonly Smithbox BaseEditor;

    private readonly string Version;

    public SoapstoneService(Smithbox baseEditor, string version)
    {
        Version = version;
        BaseEditor = baseEditor;
    }

    public override async Task<ServerInfoResponse> GetServerInfo(ServerCallContext context)
    {
        ServerInfoResponse response = new()
        {
            Id = "Smithbox",
            Version = Version,
            ServerPath = Process.GetCurrentProcess().MainModule?.FileName
        };

        if (BaseEditor.ProjectManager.SelectedProject != null)
        {
            var curProject = BaseEditor.ProjectManager.SelectedProject;

            if (curProject.DataPath != null
                && gameMapping.TryGetValue(curProject.ProjectType, out FromSoftGame gameType))
            {
                EditorResource projectResource = new()
                {
                    Type = EditorResourceType.Project,
                    ProjectJsonPath = Path.Combine(curProject.ProjectPath, "project.json"),
                    Game = gameType
                };
                response.Resources.Add(projectResource);

                if (curProject.MapEditor != null)
                {
                    if (curProject.MapEditor.IsAnyMapLoaded())
                    {
                        foreach(var entry in curProject.MapData.PrimaryBank.Maps)
                        {
                            if (entry.Value.MapContainer != null)
                            {
                                EditorResource mapResource = new()
                                {
                                    Type = EditorResourceType.Map,
                                    ProjectJsonPath = projectResource.ProjectJsonPath,
                                    Game = projectResource.Game,
                                    Name = entry.Key.Filename
                                };
                                response.Resources.Add(mapResource);
                            }
                        }
                    }
                }

                if (curProject.ParamEditor != null)
                {
                    if (curProject.ParamData.PrimaryBank?.Params != null && curProject.ParamData.PrimaryBank?.Params.Count > 0)
                    {
                        EditorResource paramResource = new()
                        {
                            Type = EditorResourceType.Param,
                            ProjectJsonPath = projectResource.ProjectJsonPath,
                            Game = projectResource.Game
                        };
                        response.Resources.Add(paramResource);
                    }
                }

                if (curProject.TextEditor != null)
                {
                    FmgLanguage lang;

                    SoulsFmg.TryGetFmgLanguageEnum(projectResource.Game, CFG.Current.TextEditor_PrimaryCategory.ToString(), out lang);

                    EditorResource textResource = new()
                    {
                        Type = EditorResourceType.Fmg,
                        ProjectJsonPath = projectResource.ProjectJsonPath,
                        Game = projectResource.Game,
                        // At the moment, this is limited to known languages, as otherwise FmgKey cannot be constructed.
                        Name = lang.ToString()
                    };
                    response.Resources.Add(textResource);
                }
            }
        }

        return response;
    }

    private static object AccessParamFile(SoulsKey.GameParamKey p, string key)
    {
        switch (key)
        {
            case "Param":
                return p.Param;
        }

        return null;
    }

    private static object AccessParamProperty(string paramName, Param.Row row, string key)
    {
        switch (key)
        {
            case "Param":
                return paramName;
            case "ID":
                return row.ID;
            case "Name":
                return row.Name;
        }

        // This should be safe to do even if key does not exist as a valid column name.
        // For more optimization, could precalculate a Column for a given param lookup.
        if (row[key] is Param.Cell cell)
        {
            return cell.Value;
        }

        return null;
    }

    private static object AccessMapFile(SoulsKey.MsbKey m, string key)
    {
        switch (key)
        {
            case "Map":
                return m.Map;
        }

        return null;
    }

    private static object AccessMapProperty(MsbEntity e, string key)
    {
        switch (key)
        {
            case "Map":
                return e.MapID;
            case "Type":
                return e.WrappedObject?.GetType()?.Name;
            case "Namespace":
                if (mapNamespaces.ContainsKey(e.Type))
                {
                    return e.Type.ToString();
                }

                return null;
        }

        var val = e.GetPropertyValue(key);
        if (val == null)
        {
            return null;
        }

        // It is okay if this is an invalid property type, as it will be filtered out in AddRequestedProperties.
        return val;
    }

    private static object AccessFmgFile(SoulsKey.FmgKey fmgKey, SoulsFmg.FmgKeyInfo keyInfo, string key)
    {
        switch (key)
        {
            case "Language":
                return fmgKey.Language.ToString();
            case "FMG":
                return keyInfo.Type.ToString();
            case "BaseFMG":
                return keyInfo.BaseType.ToString();
            case "Category":
                return keyInfo.Category.ToString();
        }

        return null;
    }

    private static object AccessFmgProperty(SoulsKey.FmgKey fmgKey, SoulsFmg.FmgKeyInfo keyInfo, FMG.Entry entry,
        string key)
    {
        switch (key)
        {
            case "ID":
                return entry.ID;
            case "Text":
                return entry.Text;
        }

        return AccessFmgFile(fmgKey, keyInfo, key);
    }

    /*
    private static bool GetFmgKey(
        FromSoftGame game,
        SoulsFmg.FmgLanguage lang,
        FMGInfo info,
        out SoulsKey.FmgKey key)
    {
        // FileCategory is used to distinguish between name-keyed FMGs (DS2) and binder-keyed FMGs (item/menu bnds)
        if (info.FileCategory == FmgFileCategory.Loose)
        {
            if (SoulsFmg.TryGetFmgNameType(game, info.Name, out List<SoulsFmg.FmgType> types))
            {
                key = new SoulsKey.FmgKey(lang, types[0]);
                return true;
            }
        }
        else
        {
            if (SoulsFmg.TryGetFmgBinderType(game, (int)info.FmgID, out List<SoulsFmg.FmgType> types))
            {
                key = new SoulsKey.FmgKey(lang, types[0]);
                return true;
            }
        }

        key = null;
        return false;
    }
    */

    private static bool MatchesResource(EditorResource resource, string key)
    {
        return string.IsNullOrEmpty(resource.Name) || key == resource.Name;
    }

    public override async Task<IEnumerable<SoulsObject>> GetObjects(
        ServerCallContext context,
        EditorResource resource,
        List<SoulsKey> keys,
        RequestedProperties properties)
    {
        List<SoulsObject> results = new();

        if (BaseEditor.ProjectManager.SelectedProject == null)
        {
            return results;
        }

        var curProject = BaseEditor.ProjectManager.SelectedProject;

        if (!gameMapping.TryGetValue(curProject.ProjectType, out FromSoftGame game) || resource.Game != game)
        {
            return results;
        }

        if (curProject.ParamEditor != null)
        {
            if (resource.Type == EditorResourceType.Param
                && curProject.ParamData.PrimaryBank?.Params is IReadOnlyDictionary<string, Param> paramDict)
            {
                foreach (SoulsKey getKey in keys)
                {
                    if (getKey.File is not SoulsKey.GameParamKey fileKey
                        || paramDict.TryGetValue(fileKey.Param, out Param param))
                    {
                        continue;
                    }

                    if (getKey is SoulsKey.GameParamKey gameParamKey)
                    {
                        SoulsObject obj = new(gameParamKey);
                        obj.AddRequestedProperties(properties, key => AccessParamFile(gameParamKey, key));
                        results.Add(obj);
                    }
                    else if (getKey is SoulsKey.GameParamRowKey gameParamRowKey)
                    {
                        // This ignores DataIndex for now
                        Param.Row row = param[gameParamRowKey.ID];
                        if (row != null)
                        {
                            SoulsObject obj = new(gameParamRowKey);
                            obj.AddRequestedProperties(properties, key => AccessParamProperty(fileKey.Param, row, key));
                            results.Add(obj);
                        }
                    }
                }
            }
        }

        /*
        if (resource.Type == EditorResourceType.Fmg
            && Locator.ActiveProject.FMGBank.IsLoaded
            && !string.IsNullOrEmpty(Locator.ActiveProject.FMGBank.LanguageFolder)
            && SoulsFmg.TryGetFmgLanguageEnum(game, Locator.ActiveProject.FMGBank.LanguageFolder, out SoulsFmg.FmgLanguage lang)
            && MatchesResource(resource, lang.ToString()))
        {
            foreach (SoulsKey getKey in keys)
            {
                if (getKey.File is not SoulsKey.FmgKey fileKey
                    || !SoulsFmg.TryGetFmgInfo(game, fileKey, out SoulsFmg.FmgKeyInfo keyInfo))
                {
                    continue;
                }

                FMGInfo info = Locator.ActiveProject.FMGBank.FmgInfoBank
                    .FirstOrDefault(info =>
                        GetFmgKey(game, lang, info, out SoulsKey.FmgKey infoKey) && infoKey.Equals(fileKey));
                if (info == null)
                {
                    continue;
                }

                if (getKey is SoulsKey.FmgKey fmgKey)
                {
                    SoulsObject obj = new(fmgKey);
                    obj.AddRequestedProperties(properties, key => AccessFmgFile(fmgKey, keyInfo, key));
                    results.Add(obj);
                }
                else if (getKey is SoulsKey.FmgEntryKey fmgEntryKey)
                {
                    FMG.Entry entry = info.Fmg.Entries.Find(entry => entry.ID == fmgEntryKey.ID);
                    if (entry != null)
                    {
                        SoulsObject obj = new(fmgEntryKey);
                        obj.AddRequestedProperties(properties,
                            key => AccessFmgProperty(fileKey, keyInfo, entry, key));
                        results.Add(obj);
                    }
                }
            }
        }
        */

        if (curProject.MapEditor != null)
        {
            if (resource.Type == EditorResourceType.Map)
            {
                foreach (SoulsKey getKey in keys)
                {
                    if (getKey.File is not SoulsKey.MsbKey fileKey ||
                        curProject.MapEditor.GetMapContainerFromMapID(fileKey.Map) == null || !MatchesResource(resource, fileKey.Map))
                    {
                        continue;
                    }

                    var targetContainer = curProject.MapEditor.GetMapContainerFromMapID(fileKey.Map);

                    if (getKey is SoulsKey.MsbKey msbKey)
                    {
                        SoulsObject obj = new(msbKey);
                        obj.AddRequestedProperties(properties, key => AccessMapFile(fileKey, key));
                        results.Add(obj);
                    }
                    else if (getKey is SoulsKey.MsbEntryKey msbEntryKey && targetContainer != null)
                    {
                        foreach (Entity ob in targetContainer.GetObjectsByName(msbEntryKey.Name))
                        {
                            if (ob is not MsbEntity e || !mapNamespaces.TryGetValue(e.Type, out KeyNamespace ns) ||
                                ns != msbEntryKey.Namespace)
                            {
                                continue;
                            }

                            SoulsObject obj = new(msbEntryKey);
                            obj.AddRequestedProperties(properties, key => AccessMapProperty(e, key));
                            results.Add(obj);
                        }
                    }
                }
            }
        }

        return results;
    }

    public override async Task<IEnumerable<SoulsObject>> SearchObjects(
        ServerCallContext context,
        EditorResource resource,
        SoulsKeyType resultType,
        PropertySearch search,
        RequestedProperties properties,
        SearchOptions options)
    {
        List<SoulsObject> results = new();

        if (BaseEditor.ProjectManager.SelectedProject == null)
        {
            return results;
        }

        var curProject = BaseEditor.ProjectManager.SelectedProject;

        if (!gameMapping.TryGetValue(curProject.ProjectType, out FromSoftGame game) || resource.Game != game)
        {
            return results;
        }

        var maxResults = options.MaxResults;

        bool addResult(SoulsObject obj)
        {
            results.Add(obj);
            return maxResults > 0 && results.Count >= maxResults;
        }

        // Some of the iterations below may have interference with concurrent modification issues,
        // but transient errors are basically acceptable here. If not, use concurrent collections instead.
        if(curProject.ParamEditor != null)
        {
            if (resource.Type == EditorResourceType.Param)
            {
                Predicate<object> fileFilter = search.GetKeyFilter("Param");
                foreach (KeyValuePair<string, Param> entry in curProject.ParamData.PrimaryBank?.Params ??
                                                              new Dictionary<string, Param>())
                {
                    if (!fileFilter(entry.Key))
                    {
                        continue;
                    }

                    SoulsKey.GameParamKey fileKey = new(entry.Key);
                    if (resultType.Matches(typeof(SoulsKey.GameParamKey)))
                    {
                        // The only property of GameParamKey has already been filtered, so no need to check IsMatch.
                        SoulsObject obj = new(fileKey);
                        obj.AddRequestedProperties(properties, key => AccessParamFile(fileKey, key));
                        if (addResult(obj))
                        {
                            goto limitReached;
                        }
                    }

                    if (resultType.Matches(typeof(SoulsKey.GameParamRowKey)))
                    {
                        foreach (Param.Row row in entry.Value.Rows)
                        {
                            if (search.IsMatch(key => AccessParamProperty(entry.Key, row, key)))
                            {
                                // Currently, Param doesn't expose DataIndex.
                                // If there is a way to do that without breaking abstractions, we should add it here,
                                // since Soapstone does support identical param rows having indices.
                                SoulsObject obj = new(new SoulsKey.GameParamRowKey(fileKey, row.ID));
                                obj.AddRequestedProperties(properties, key => AccessParamProperty(entry.Key, row, key));
                                if (addResult(obj))
                                {
                                    goto limitReached;
                                }
                            }
                        }
                    }
                }
            }
        }

        /*
        if (resource.Type == EditorResourceType.Fmg
            && Locator.ActiveProject.FMGBank.IsLoaded
            && !string.IsNullOrEmpty(Locator.ActiveProject.FMGBank.LanguageFolder)
            && SoulsFmg.TryGetFmgLanguageEnum(game, Locator.ActiveProject.FMGBank.LanguageFolder, out SoulsFmg.FmgLanguage lang)
            // Language applies to all FMGs at once currently, so filter it here if requested
            && MatchesResource(resource, lang.ToString())
            && search.GetKeyFilter("Language")(lang.ToString()))
        {
            Predicate<object> fmgFilter = search.GetKeyFilter("FMG");
            Predicate<object> baseFmgFilter = search.GetKeyFilter("BaseFMG");
            Predicate<object> categoryFilter = search.GetKeyFilter("Category");
            foreach (FMGInfo info in Locator.ActiveProject.FMGBank.FmgInfoBank)
            {
                if (!GetFmgKey(game, lang, info, out SoulsKey.FmgKey fileKey)
                    || !SoulsFmg.TryGetFmgInfo(game, fileKey, out SoulsFmg.FmgKeyInfo keyInfo))
                {
                    continue;
                }

                if (!fmgFilter(keyInfo.Type.ToString())
                    || !baseFmgFilter(keyInfo.BaseType.ToString())
                    || !categoryFilter(keyInfo.Category.ToString()))
                {
                    continue;
                }

                if (resultType.Matches(typeof(SoulsKey.FmgKey)))
                {
                    // All properties of FmgKey have already been filtered, so no need to check IsMatch.
                    SoulsObject obj = new(fileKey);
                    obj.AddRequestedProperties(properties, key => AccessFmgFile(fileKey, keyInfo, key));
                    if (addResult(obj))
                    {
                        goto limitReached;
                    }
                }

                if (resultType.Matches(typeof(SoulsKey.FmgEntryKey)))
                {
                    foreach (FMG.Entry entry in info.Fmg.Entries)
                    {
                        if (search.IsMatch(key => AccessFmgProperty(fileKey, keyInfo, entry, key)))
                        {
                            SoulsObject obj = new(new SoulsKey.FmgEntryKey(fileKey, entry.ID));
                            obj.AddRequestedProperties(properties,
                                key => AccessFmgProperty(fileKey, keyInfo, entry, key));
                            if (addResult(obj))
                            {
                                goto limitReached;
                            }
                        }
                    }
                }
            }
        }
        */

        if (curProject.MapEditor != null)
        {
            if (resource.Type == EditorResourceType.Map)
            {
                Predicate<object> fileFilter = search.GetKeyFilter("Map");
                // LoadedObjectContainers is never null, starts out an empty dictionary
                foreach (var entry in curProject.MapData.PrimaryBank.Maps)
                {
                    if (!fileFilter(entry.Key) || !MatchesResource(resource, entry.Key.Filename))
                    {
                        continue;
                    }

                    SoulsKey.MsbKey fileKey = new(entry.Key.Filename);
                    // For MsbKey, we don't care about the container actually being loaded or not.
                    // Just include it if it's not been filtered.
                    if (resultType.Matches(typeof(SoulsKey.MsbKey)))
                    {
                        // The only property of MsbKey has already been filtered, so no need to check IsMatch.
                        SoulsObject obj = new(fileKey);
                        // For now, don't include a map alias property here, as it's not part of ObjectContainer directly.
                        // We could also use a separate EditorResourceType.Alias for that.
                        obj.AddRequestedProperties(properties, key => AccessMapFile(fileKey, key));
                        if (addResult(obj))
                        {
                            goto limitReached;
                        }
                    }

                    if (resultType.Matches(typeof(SoulsKey.MsbEntryKey)) && entry.Value.MapContainer != null)
                    {
                        // Use similar enumeration as SearchProperties
                        foreach (Entity ob in entry.Value.MapContainer.Objects)
                        {
                            if (ob is not MsbEntity e || !mapNamespaces.TryGetValue(e.Type, out KeyNamespace ns))
                            {
                                continue;
                            }

                            if (search.IsMatch(key => AccessMapProperty(e, key)))
                            {
                                SoulsObject obj = new(new SoulsKey.MsbEntryKey(fileKey, ns, e.Name));
                                obj.AddRequestedProperties(properties, key => AccessMapProperty(e, key));
                                if (addResult(obj))
                                {
                                    goto limitReached;
                                }
                            }
                        }
                    }
                }
            }
        }

    limitReached:
        return results;
    }

    public override async Task OpenResource(ServerCallContext context, EditorResource resource)
    {
        var curProject = BaseEditor.ProjectManager.SelectedProject;

        // At the moment, only loading maps is supported.
        // This could be extended to switching FMG language, or adding a param view, or opening a model to view.
        if (!gameMapping.TryGetValue(curProject.ProjectType, out FromSoftGame game) || resource.Game != game)
        {
            return;
        }

        if (resource.Type == EditorResourceType.Map && resource.Name != null)
        {
            EditorCommandQueue.AddCommand(new[] { "map", "load", resource.Name });
            EditorCommandQueue.AddCommand("windowFocus");
        }
    }

    public override async Task OpenObject(
        ServerCallContext context,
        EditorResource resource,
        SoulsKey key)
    {
        if (BaseEditor.ProjectManager.SelectedProject == null)
            return;

        var curProject = BaseEditor.ProjectManager.SelectedProject;

        if (!gameMapping.TryGetValue(curProject.ProjectType, out FromSoftGame game) || resource.Game != game)
        {
            return;
        }

        if (resource.Type == EditorResourceType.Map)
        {
            if (key is SoulsKey.MsbKey msbKey && MatchesResource(resource, msbKey.Map))
            {
                EditorCommandQueue.AddCommand(new[] { "map", "select", msbKey.Map });
                EditorCommandQueue.AddCommand("windowFocus");
            }
            else if (key is SoulsKey.MsbEntryKey msbEntryKey && MatchesResource(resource, msbEntryKey.File.Map)
                                                             && revMapNamespaces.TryGetValue(msbEntryKey.Namespace,
                                                                 out MsbEntityType entityType))
            {
                EditorCommandQueue.AddCommand(new[]
                {
                    "map", "select", msbEntryKey.File.Map, msbEntryKey.Name, entityType.ToString()
                });
                EditorCommandQueue.AddCommand("windowFocus");
            }
        }
        else if (resource.Type == EditorResourceType.Param)
        {
            if (key is SoulsKey.GameParamKey paramKey)
            {
                EditorCommandQueue.AddCommand(new[] { "param", "select", "-1", paramKey.Param });
                EditorCommandQueue.AddCommand("windowFocus");
            }
            else if (key is SoulsKey.GameParamRowKey paramRowKey)
            {
                EditorCommandQueue.AddCommand(new[]
                {
                    "param", "select", "-1", paramRowKey.File.Param, paramRowKey.ID.ToString()
                });
                EditorCommandQueue.AddCommand("windowFocus");
            }
        }
        else if (resource.Type == EditorResourceType.Fmg
                 && key.File is SoulsKey.FmgKey fmgKey
                 && SoulsFmg.TryGetFmgInfo(game, fmgKey, out SoulsFmg.FmgKeyInfo fmgKeyInfo))
        {
            var commandKey = fmgKeyInfo.BinderID >= 0 ? fmgKeyInfo.BinderID.ToString() : fmgKeyInfo.FmgName;
            if (key is SoulsKey.FmgKey)
            {
                EditorCommandQueue.AddCommand(new[] { "text", "select", commandKey });
                EditorCommandQueue.AddCommand("windowFocus");
            }
            else if (key is SoulsKey.FmgEntryKey fmgEntryKey)
            {
                EditorCommandQueue.AddCommand(new[] { "text", "select", commandKey, fmgEntryKey.ID.ToString() });
                EditorCommandQueue.AddCommand("windowFocus");
            }
        }
    }

    public override async Task OpenSearch(
        ServerCallContext context,
        EditorResource resource,
        SoulsKeyType resultType,
        PropertySearch search,
        bool openFirstResult)
    {
        if (BaseEditor.ProjectManager.SelectedProject == null)
            return;

        var curProject = BaseEditor.ProjectManager.SelectedProject;

        // At the moment, just map properties, since there are some multi-keyed things like entity groups
        // Params are also possible; FMG might require a new command
        if (!gameMapping.TryGetValue(curProject.ProjectType, out FromSoftGame game) || resource.Game != game)
        {
            return;
        }

        if (resource.Type == EditorResourceType.Map)
        {
            if (resultType.Matches(typeof(SoulsKey.MsbEntryKey)))
            {
                // Single property equality is supported.
                // Just opening the search interface without a property name is currently not supported.
                PropertySearch.Condition cond = search.FirstCondition;
                if (cond != null && cond.Type == PropertyComparisonType.Equal)
                {
                    List<string> cmd = new() { "map", "propsearch", cond.Key, cond.Value.ToString() };
                    if (openFirstResult)
                    {
                        cmd.Add("selectFirstResult");
                    }

                    EditorCommandQueue.AddCommand(cmd);
                    EditorCommandQueue.AddCommand("windowFocus");
                }
            }
        }
    }
}
