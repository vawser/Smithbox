using CommunityToolkit.HighPerformance.Buffers;
using StudioCore.Editors.ModelEditor.Core;
using StudioCore.Editors.ModelEditor.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

public class ModelCommandQueue
{
    private ModelEditorScreen Screen;

    public ModelCommandQueue(ModelEditorScreen screen)
    {
        Screen = screen;
    }

    public void Parse(string[] initcmd)
    {
        if (initcmd != null && initcmd.Length > 1)
        {
            if (initcmd[0] == "load")
            {
                var modelName = initcmd[1];
                var assetType = initcmd[2];

                if (assetType == "Character")
                {
                    Screen.FileSelection._searchInput = modelName;
                    Screen.ResManager.LoadCharacter(modelName);
                }

                if (assetType == "Asset")
                {
                    Screen.FileSelection._searchInput = modelName;
                    Screen.ResManager.LoadAsset(modelName);
                }

                if (assetType == "Part")
                {
                    Screen.FileSelection._searchInput = modelName;
                    Screen.ResManager.LoadPart(modelName);
                }

                if (initcmd.Length > 3)
                {
                    var mapId = initcmd[3];

                    if (assetType == "MapPiece")
                    {
                        var mapPieceName = modelName.Replace(mapId, "m");
                        Screen.FileSelection._searchInput = mapPieceName;
                        Screen.ResManager.LoadMapPiece(modelName, mapId);
                    }
                }
            }
        }
    }
}
