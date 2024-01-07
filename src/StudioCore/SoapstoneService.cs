using Andre.Formats;
using Grpc.Core;
using SoapstoneLib;
using SoapstoneLib.Proto;
using SoulsFormats;
using StudioCore.MsbEditor;
using StudioCore.ParamEditor;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

#pragma warning disable CS1998 // Async method lacks 'await'. Return without Task is convenient, though

namespace StudioCore;

/// <summary>
///     Implementation of RPC server for cross-editor communication.
///     For the most part, this does not intentionally return errors when an operation cannot be
///     completed. This may change in the future.
/// </summary>
public class SoapstoneService : SoapstoneServiceV1
{
    private static readonly Dictionary<GameType, FromSoftGame> gameMapping = new()
    {
        [GameType.DemonsSouls] = FromSoftGame.DemonsSouls,
        [GameType.DarkSoulsPTDE] = FromSoftGame.DarkSoulsPtde,
        [GameType.DarkSoulsRemastered] = FromSoftGame.DarkSoulsRemastered,
        [GameType.DarkSoulsIISOTFS] = FromSoftGame.DarkSouls2Sotfs,
        [GameType.DarkSoulsIII] = FromSoftGame.DarkSouls3,
        [GameType.Bloodborne] = FromSoftGame.Bloodborne,
        [GameType.Sekiro] = FromSoftGame.Sekiro,
        [GameType.EldenRing] = FromSoftGame.EldenRing
    };

    private static readonly Dictionary<MapEntity.MapEntityType, KeyNamespace> mapNamespaces = new()
    {
        [MapEntity.MapEntityType.Event] = KeyNamespace.MapEvent,
        [MapEntity.MapEntityType.Region] = KeyNamespace.MapRegion,
        [MapEntity.MapEntityType.Part] = KeyNamespace.MapPart
    };

    private static readonly Dictionary<KeyNamespace, MapEntity.MapEntityType> revMapNamespaces =
        mapNamespaces.ToDictionary(e => e.Value, e => e.Key);

    private readonly AssetLocator assetLocator;
    private readonly MsbEditorScreen msbEditor;

    private readonly string version;

    public SoapstoneService(string version, AssetLocator assetLocator, MsbEditorScreen msbEditor)
    {
        this.version = version;
        this.assetLocator = assetLocator;
        this.msbEditor = msbEditor;
    }

    public override async Task<ServerInfoResponse> GetServerInfo(ServerCallContext context)
    {
        ServerInfoResponse response = new()
        {
            Id = "DSMapStudio", Version = version, ServerPath = Process.GetCurrentProcess().MainModule?.FileName
        };

        if (assetLocator.GameModDirectory != null
            && gameMapping.TryGetValue(assetLocator.Type, out FromSoftGame gameType))
        {
            EditorResource projectResource = new()
            {
                Type = EditorResourceType.Project,
                ProjectJsonPath = Path.Combine(assetLocator.GameModDirectory, "project.json"),
                Game = gameType
            };
            response.Resources.Add(projectResource);
            if (msbEditor.Universe.LoadedObjectContainers.Count > 0)
            {
                foreach (KeyValuePair<string, ObjectContainer> entry in msbEditor.Universe.LoadedObjectContainers)
                {
                    if (entry.Value != null)
                    {
                        EditorResource mapResource = new()
                        {
                            Type = EditorResourceType.Map,
                            ProjectJsonPath = projectResource.ProjectJsonPath,
                            Game = projectResource.Game,
                            Name = entry.Key
                        };
                        response.Resources.Add(mapResource);
                    }
                }
            }

            if (ParamBank.PrimaryBank?.Params != null && ParamBank.PrimaryBank?.Params.Count > 0)
            {
                EditorResource paramResource = new()
                {
                    Type = EditorResourceType.Param,
                    ProjectJsonPath = projectResource.ProjectJsonPath,
                    Game = projectResource.Game
                };
                response.Resources.Add(paramResource);
            }

            if (FMGBankLoaded(projectResource.Game, out SoulsFmg.FmgLanguage lang))
            {
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

        return response;
    }

    private static bool FMGBankLoaded(FromSoftGame game, out SoulsFmg.FmgLanguage lang)
    {
        lang = default;
        return FMGBank.IsLoaded
               && !string.IsNullOrEmpty(FMGBank.LanguageFolder)
               && SoulsFmg.TryGetFmgLanguageEnum(game, FMGBank.LanguageFolder, out lang);
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

    private static object AccessMapProperty(MapEntity e, string key)
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

    private static bool GetFmgKey(
        FromSoftGame game,
        SoulsFmg.FmgLanguage lang,
        FMGBank.FMGInfo info,
        out SoulsKey.FmgKey key)
    {
        // UICategory is used to distinguish between name-keyed FMGs (DS2) and binder-keyed FMGs (item/menu bnds)
        if (info.UICategory == FmgUICategory.Text)
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
        if (!gameMapping.TryGetValue(assetLocator.Type, out FromSoftGame game) || resource.Game != game)
        {
            return results;
        }

        if (resource.Type == EditorResourceType.Param
            && ParamBank.PrimaryBank?.Params is IReadOnlyDictionary<string, Param> paramDict)
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

        if (resource.Type == EditorResourceType.Fmg
            && FMGBank.IsLoaded
            && !string.IsNullOrEmpty(FMGBank.LanguageFolder)
            && SoulsFmg.TryGetFmgLanguageEnum(game, FMGBank.LanguageFolder, out SoulsFmg.FmgLanguage lang)
            && MatchesResource(resource, lang.ToString()))
        {
            foreach (SoulsKey getKey in keys)
            {
                if (getKey.File is not SoulsKey.FmgKey fileKey
                    || !SoulsFmg.TryGetFmgInfo(game, fileKey, out SoulsFmg.FmgKeyInfo keyInfo))
                {
                    continue;
                }

                FMGBank.FMGInfo info = FMGBank.FmgInfoBank
                    .Find(info =>
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

        if (resource.Type == EditorResourceType.Map)
        {
            foreach (SoulsKey getKey in keys)
            {
                if (getKey.File is not SoulsKey.MsbKey fileKey
                    || !msbEditor.Universe.LoadedObjectContainers.TryGetValue(fileKey.Map,
                        out ObjectContainer container)
                    || !MatchesResource(resource, fileKey.Map))
                {
                    continue;
                }

                if (getKey is SoulsKey.MsbKey msbKey)
                {
                    SoulsObject obj = new(msbKey);
                    obj.AddRequestedProperties(properties, key => AccessMapFile(fileKey, key));
                    results.Add(obj);
                }
                else if (getKey is SoulsKey.MsbEntryKey msbEntryKey && container is Map m)
                {
                    foreach (Entity ob in m.GetObjectsByName(msbEntryKey.Name))
                    {
                        if (ob is not MapEntity e || !mapNamespaces.TryGetValue(e.Type, out KeyNamespace ns) ||
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
        if (!gameMapping.TryGetValue(assetLocator.Type, out FromSoftGame game) || resource.Game != game)
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
        if (resource.Type == EditorResourceType.Param)
        {
            Predicate<object> fileFilter = search.GetKeyFilter("Param");
            foreach (KeyValuePair<string, Param> entry in ParamBank.PrimaryBank?.Params ??
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

        if (resource.Type == EditorResourceType.Fmg
            && FMGBank.IsLoaded
            && !string.IsNullOrEmpty(FMGBank.LanguageFolder)
            && SoulsFmg.TryGetFmgLanguageEnum(game, FMGBank.LanguageFolder, out SoulsFmg.FmgLanguage lang)
            // Language applies to all FMGs at once currently, so filter it here if requested
            && MatchesResource(resource, lang.ToString())
            && search.GetKeyFilter("Language")(lang.ToString()))
        {
            Predicate<object> fmgFilter = search.GetKeyFilter("FMG");
            Predicate<object> baseFmgFilter = search.GetKeyFilter("BaseFMG");
            Predicate<object> categoryFilter = search.GetKeyFilter("Category");
            foreach (FMGBank.FMGInfo info in FMGBank.FmgInfoBank)
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

        if (resource.Type == EditorResourceType.Map)
        {
            Predicate<object> fileFilter = search.GetKeyFilter("Map");
            // LoadedObjectContainers is never null, starts out an empty dictionary
            foreach (KeyValuePair<string, ObjectContainer> entry in msbEditor.Universe.LoadedObjectContainers)
            {
                if (!fileFilter(entry.Key) || !MatchesResource(resource, entry.Key))
                {
                    continue;
                }

                SoulsKey.MsbKey fileKey = new(entry.Key);
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

                if (resultType.Matches(typeof(SoulsKey.MsbEntryKey)) && entry.Value is Map m)
                {
                    // Use similar enumeration as SearchProperties
                    foreach (Entity ob in m.Objects)
                    {
                        if (ob is not MapEntity e || !mapNamespaces.TryGetValue(e.Type, out KeyNamespace ns))
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

        limitReached:
        return results;
    }

    public override async Task OpenResource(ServerCallContext context, EditorResource resource)
    {
        // At the moment, only loading maps is supported.
        // This could be extended to switching FMG language, or adding a param view, or opening a model to view.
        if (!gameMapping.TryGetValue(assetLocator.Type, out FromSoftGame game) || resource.Game != game)
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
        if (!gameMapping.TryGetValue(assetLocator.Type, out FromSoftGame game) || resource.Game != game)
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
                                                                 out MapEntity.MapEntityType entityType))
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
                 && FMGBankLoaded(game, out SoulsFmg.FmgLanguage lang)
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
        // At the moment, just map properties, since there are some multi-keyed things like entity groups
        // Params are also possible; FMG might require a new command
        if (!gameMapping.TryGetValue(assetLocator.Type, out FromSoftGame game) || resource.Game != game)
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
