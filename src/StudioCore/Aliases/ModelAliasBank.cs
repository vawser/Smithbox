using StudioCore.Editor;
using StudioCore.Settings;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;

namespace StudioCore.Aliases;

public class ModelAliasBank
{
    private static AssetLocator AssetLocator;

    public static ModelAliasContainer _loadedModelAliasBank { get; set; }

    public static bool IsLoadingModelAliases { get; private set; }

    public static ModelAliasContainer ModelNames
    {
        get
        {
            if (IsLoadingModelAliases)
                return null;

            return _loadedModelAliasBank;
        }
    }

    // TODO: add support for mod-local model aliases

    private static void LoadModelNames()
    {
        try
        {
            _loadedModelAliasBank = new ModelAliasContainer(AssetLocator.GetGameIDForDir());
        }
        catch (Exception e)
        {
            
        }
    }

    public static void ReloadModelAliases()
    {
        TaskManager.Run(new TaskManager.LiveTask("Models - Load Names", TaskManager.RequeueType.WaitThenRequeue, false,
            () =>
            {
                _loadedModelAliasBank = new ModelAliasContainer();
                IsLoadingModelAliases = true;

                if (AssetLocator.Type != GameType.Undefined)
                    LoadModelNames();

                IsLoadingModelAliases = false;
            }));
    }

    public static void SetAssetLocator(AssetLocator l)
    {
        AssetLocator = l;
    }
}
