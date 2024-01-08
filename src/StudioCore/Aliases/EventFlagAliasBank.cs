using StudioCore.Editor;
using StudioCore.Settings;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;

namespace StudioCore.Aliases;

public class EventFlagAliasBank
{
    private static AssetLocator AssetLocator;

    public static EventFlagAliasContainer _loadedEventFlagAliasBank { get; set; }

    public static bool IsLoadingEventFlagAliases { get; private set; }

    public static EventFlagAliasContainer EventFlagNames
    {
        get
        {
            if (IsLoadingEventFlagAliases)
                return null;

            return _loadedEventFlagAliasBank;
        }
    }

    private static void LoadEventFlagNames()
    {
        try
        {
            _loadedEventFlagAliasBank = new EventFlagAliasContainer(AssetLocator.GetGameIDForDir());
        }
        catch (Exception e)
        {

        }
    }

    public static void ReloadEventFlagAliases()
    {
        TaskManager.Run(new TaskManager.LiveTask("Event Flags - Load Names", TaskManager.RequeueType.WaitThenRequeue, false,
            () =>
            {
                _loadedEventFlagAliasBank = new EventFlagAliasContainer();
                IsLoadingEventFlagAliases = true;

                if (AssetLocator.Type != GameType.Undefined)
                    LoadEventFlagNames();

                IsLoadingEventFlagAliases = false;
            }));
    }

    public static void SetAssetLocator(AssetLocator l)
    {
        AssetLocator = l;
    }
}
