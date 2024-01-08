using StudioCore.Editor;
using StudioCore.Settings;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;

namespace StudioCore.Aliases;

/// <summary>
///     Utilities for dealing with global params for a game
/// </summary>
public class MapAliasBank
{
    private static AssetLocator AssetLocator;

    private static Dictionary<string, string> _mapNames;
    public static bool IsLoadingMapAliases { get; private set; }

    public static IReadOnlyDictionary<string, string> MapNames
    {
        get
        {
            if (IsLoadingMapAliases)
                return null;

            return _mapNames;
        }
    }

    private static void LoadMapNames()
    {
        try
        {
            var dir = AssetLocator.GetMapAliasAssetsDir();
            var mapNames = File.ReadAllLines(dir + "/MapNames.txt");
            foreach (var pair in mapNames)
            {
                var parts = pair.Split(' ', 2);
                _mapNames[parts[0]] = parts[1];
            }
        }
        catch (Exception e)
        {
            //should log the error really. Or just fill in the missing alias files
        }
    }

    public static void ReloadMapAliases()
    {
        TaskManager.Run(new TaskManager.LiveTask("Map - Load Names", TaskManager.RequeueType.WaitThenRequeue, false,
            () =>
            {
                _mapNames = new Dictionary<string, string>();
                IsLoadingMapAliases = true;
                if (AssetLocator.Type != GameType.Undefined)
                    LoadMapNames();

                IsLoadingMapAliases = false;
            }));
    }

    public static void SetAssetLocator(AssetLocator l)
    {
        AssetLocator = l;
    }
}
