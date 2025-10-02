using SoulsFormats;
using SoulsFormats.KF4;
using StudioCore.Core;
using StudioCore.Platform;
using StudioCore.Resource.Locators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Tools.MapQuery;

public class MapQueryBank
{
    private MapEditorScreen Editor;

    private IMapQueryEngine Engine;

    public bool MapBankInitialized = false;

    public Dictionary<string, IMsb> MapList = new Dictionary<string, IMsb>();

    public MapQueryBank(MapEditorScreen editor, IMapQueryEngine engine)
    {
        Editor = editor;

        Engine = engine;

        if (Editor.Project.ProjectType is ProjectType.Undefined)
        {
            MapBankInitialized = true;
        }
    }

    public void OnProjectChanged()
    {
        MapBankInitialized = false;
        MapList = new Dictionary<string, IMsb>();

        if (Editor.Project.ProjectType is ProjectType.Undefined)
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
        if (!CFG.Current.MapEditor_LoadMapQueryData)
            return;

        // Load the maps async so the main thread isn't blocked
        Task<bool> loadMapsTask = ReadMaps();

        bool result = await loadMapsTask;
        MapBankInitialized = result;
    }

    public async Task<bool> ReadMaps()
    {
        await Task.Yield();

        List<Formats.JSON.FileDictionaryEntry> failedLoads = [];
        foreach (var map in Editor.Project.MapData.MapFiles.Entries)
        {
            IMsb msb = null;

            var msbData = Editor.Project.FS.ReadFile(map.Path);

            if (msbData == null)
                continue;

            try
            {
                if (Editor.Project.ProjectType == ProjectType.DES)
                {
                    msb = MSBD.Read((Memory<byte>)msbData);
                }
                if (Editor.Project.ProjectType == ProjectType.DS1 || Editor.Project.ProjectType == ProjectType.DS1R)
                {
                    msb = MSB1.Read((Memory<byte>)msbData);
                }
                if (Editor.Project.ProjectType == ProjectType.DS2 || Editor.Project.ProjectType == ProjectType.DS2S)
                {
                    msb = MSB2.Read((Memory<byte>)msbData);
                }
                if (Editor.Project.ProjectType == ProjectType.DS3)
                {
                    msb = MSB3.Read((Memory<byte>)msbData);
                }
                if (Editor.Project.ProjectType == ProjectType.BB)
                {
                    msb = MSBB.Read((Memory<byte>)msbData);
                }
                if (Editor.Project.ProjectType == ProjectType.SDT)
                {
                    msb = MSBS.Read((Memory<byte>)msbData);
                }
                if (Editor.Project.ProjectType == ProjectType.ER)
                {
                    msb = MSBE.Read((Memory<byte>)msbData);
                }
                if (Editor.Project.ProjectType == ProjectType.AC6)
                {
                    msb = MSB_AC6.Read((Memory<byte>)msbData);
                }
                if (Editor.Project.ProjectType == ProjectType.NR)
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
