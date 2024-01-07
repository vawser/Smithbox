using System;
using System.Collections.Generic;
using System.IO;

namespace StudioCore.Editor;

/// <summary>
///     Utilities for dealing with global params for a game
/// </summary>
public class AliasBank
{
    private static AssetLocator AssetLocator;

    private static Dictionary<string, string> _mapNames;
    public static bool IsLoadingAliases { get; private set; }

    public static IReadOnlyDictionary<string, string> MapNames
    {
        get
        {
            if (IsLoadingAliases)
            {
                return null;
            }

            return _mapNames;
        }
    }

    private static void LoadMapNames()
    {
        try
        {
            var dir = AssetLocator.GetAliasAssetsDir();
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

    public static void ReloadAliases()
    {
        TaskManager.Run(new TaskManager.LiveTask("Map - Load Names", TaskManager.RequeueType.WaitThenRequeue, false,
            () =>
            {
                _mapNames = new Dictionary<string, string>();
                IsLoadingAliases = true;
                if (AssetLocator.Type != GameType.Undefined)
                {
                    LoadMapNames();
                }

                IsLoadingAliases = false;
            }));
    }

    public static void SetAssetLocator(AssetLocator l)
    {
        AssetLocator = l;
    }
}
