using SoulsFormats;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

public class MapQueryBank
{
    private MapEditorView View;

    private IMapQueryEngine Engine;

    public bool MapBankInitialized = false;

    public Dictionary<string, IMsb> MapList = new Dictionary<string, IMsb>();

    public MapQueryBank(MapEditorView view, IMapQueryEngine engine)
    {
        View = view;

        Engine = engine;

        if (View.Project.Descriptor.ProjectType is ProjectType.Undefined)
        {
            MapBankInitialized = true;
        }
    }

    public Dictionary<string, IMsb> GetMaps()
    {
        return MapList;
    }

    public async void SetupData()
    {
        // Load the maps async so the main thread isn't blocked
        Task<bool> loadMapsTask = ReadMaps();

        bool result = await loadMapsTask;
        MapBankInitialized = result;
    }

    public async Task<bool> ReadMaps()
    {
        await Task.Yield();

        List<FileDictionaryEntry> failedLoads = [];
        foreach (var map in View.Project.Locator.MapFiles.Entries)
        {
            IMsb msb = null;

            var msbData = View.Project.VFS.FS.ReadFile(map.Path);

            if (msbData == null)
                continue;

            try
            {
                if (View.Project.Descriptor.ProjectType == ProjectType.DES)
                {
                    msb = MSBD.Read((Memory<byte>)msbData);
                }
                if (View.Project.Descriptor.ProjectType == ProjectType.DS1 || View.Project.Descriptor.ProjectType == ProjectType.DS1R)
                {
                    msb = MSB1.Read((Memory<byte>)msbData);
                }
                if (View.Project.Descriptor.ProjectType == ProjectType.DS2 || View.Project.Descriptor.ProjectType == ProjectType.DS2S)
                {
                    msb = MSB2.Read((Memory<byte>)msbData);
                }
                if (View.Project.Descriptor.ProjectType == ProjectType.DS3)
                {
                    msb = MSB3.Read((Memory<byte>)msbData);
                }
                if (View.Project.Descriptor.ProjectType == ProjectType.BB)
                {
                    msb = MSBB.Read((Memory<byte>)msbData);
                }
                if (View.Project.Descriptor.ProjectType == ProjectType.SDT)
                {
                    msb = MSBS.Read((Memory<byte>)msbData);
                }
                if (View.Project.Descriptor.ProjectType == ProjectType.ER)
                {
                    msb = MSBE.Read((Memory<byte>)msbData);
                }
                if (View.Project.Descriptor.ProjectType == ProjectType.AC6)
                {
                    msb = MSB_AC6.Read((Memory<byte>)msbData);
                }
                if (View.Project.Descriptor.ProjectType == ProjectType.NR)
                {
                    msb = MSB_NR.Read((Memory<byte>)msbData);
                }
            }
            catch
            {
                failedLoads.Add(map);
            }

            if (msb != null && !MapList.ContainsKey(map.Filename))
            {
                MapList.Add(map.Filename, msb);
            }
        }

        if (failedLoads.Count > 0)
        {
            PlatformUtils.Instance.MessageBox(
                $"Failed to load {failedLoads.Count} maps, skipping.",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        return true;
    }

}
