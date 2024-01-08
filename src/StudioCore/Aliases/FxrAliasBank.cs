using StudioCore.Editor;
using StudioCore.Settings;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;

namespace StudioCore.Aliases;

public class FxrAliasBank
{
    private static AssetLocator AssetLocator;

    public static FxrAliasContainer _loadedFxrAliasBank { get; set; }

    public static bool IsLoadingFxrAliases { get; private set; }

    public static FxrAliasContainer FxrNames
    {
        get
        {
            if (IsLoadingFxrAliases)
                return null;

            return _loadedFxrAliasBank;
        }
    }

    private static void LoadFxrNames()
    {
        try
        {
            _loadedFxrAliasBank = new FxrAliasContainer(AssetLocator.GetGameIDForDir());
        }
        catch (Exception e)
        {

        }
    }

    public static void ReloadFxrAliases()
    {
        TaskManager.Run(new TaskManager.LiveTask("FXR - Load Names", TaskManager.RequeueType.WaitThenRequeue, false,
            () =>
            {
                _loadedFxrAliasBank = new FxrAliasContainer();
                IsLoadingFxrAliases = true;

                if (AssetLocator.Type != GameType.Undefined)
                    LoadFxrNames();

                IsLoadingFxrAliases = false;
            }));
    }

    public static void SetAssetLocator(AssetLocator l)
    {
        AssetLocator = l;
    }
}
