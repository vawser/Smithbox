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
    private FileSelectionView Selection;
    private ModelResourceManager ResourceManager;

    public ModelCommandQueue(ModelEditorScreen screen)
    {
        Screen = screen;
        Selection = screen.FileSelection;
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
                    Selection._searchInput = modelName;
                    ResourceManager.LoadCharacter(modelName);
                }

                if (assetType == "Asset")
                {
                    Selection._searchInput = modelName;
                    ResourceManager.LoadAsset(modelName);
                }

                if (assetType == "Part")
                {
                    Selection._searchInput = modelName;
                    ResourceManager.LoadPart(modelName);
                }

                if (initcmd.Length > 3)
                {
                    var mapId = initcmd[3];

                    if (assetType == "MapPiece")
                    {
                        var mapPieceName = modelName.Replace(mapId, "m");
                        Selection._searchInput = mapPieceName;
                        ResourceManager.LoadMapPiece(modelName, mapId);
                    }
                }
            }
        }
    }
}
