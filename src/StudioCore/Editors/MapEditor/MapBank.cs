using SoulsFormats;
using StudioCore.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.MapEditorNS;

public class MapBank
{
    private MapData DataParent;

    public string BankName = "Undefined";

    public Dictionary<string, IMsb> Maps = new();

    public MapBank(MapData parent, string bankName)
    {
        DataParent = parent;
        BankName = bankName;
    }

    public void LoadMap(string filename, string filepath)
    {
        var editor = DataParent.Project.MapEditor;
        var mapData = DataParent.Project.FS.ReadFileOrThrow(filepath);

        switch (DataParent.Project.ProjectType)
        {
            case ProjectType.DES:
                editor.Selection._selectedMap = MSBD.Read(mapData);
                break;
            case ProjectType.DS1:
            case ProjectType.DS1R:
                editor.Selection._selectedMap = MSB1.Read(mapData);
                break;
            case ProjectType.DS2:
            case ProjectType.DS2S:
                editor.Selection._selectedMap = MSB2.Read(mapData);
                break;
            case ProjectType.DS3:
                editor.Selection._selectedMap = MSB3.Read(mapData);
                break;
            case ProjectType.BB:
                editor.Selection._selectedMap = MSBB.Read(mapData);
                break;
            case ProjectType.SDT:
                editor.Selection._selectedMap = MSBS.Read(mapData);
                break;
            case ProjectType.ER:
                editor.Selection._selectedMap = MSBE.Read(mapData);
                break;
            case ProjectType.AC6:
                editor.Selection._selectedMap = MSB_AC6.Read(mapData);
                break;
            case ProjectType.ERN:
            default: break;
        }
    }

    public async Task<bool> Save()
    {
        await Task.Delay(1000);

        var successfulSave = false;

        switch (DataParent.Project.ProjectType)
        {
            case ProjectType.DES:
            case ProjectType.DS1:
            case ProjectType.DS1R:
            case ProjectType.DS2:
            case ProjectType.DS2S:
            case ProjectType.DS3:
            case ProjectType.BB:
            case ProjectType.SDT:
            case ProjectType.ER:
            case ProjectType.AC6:
            case ProjectType.ERN:
            default: break;
        }

        return successfulSave;
    }
}

