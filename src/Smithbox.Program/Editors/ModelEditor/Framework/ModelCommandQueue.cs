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
    private ModelEditorScreen Editor;

    public ModelCommandQueue(ModelEditorScreen screen)
    {
        Editor = screen;
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
                    Editor.FileSelection._searchInput = modelName;
                    Editor.ResManager.LoadCharacter(modelName);
                }

                if (assetType == "Enemy")
                {
                    Editor.FileSelection._searchInput = modelName;
                    Editor.ResManager.LoadEnemy(modelName);
                }

                if (assetType == "Asset")
                {
                    Editor.FileSelection._searchInput = modelName;
                    Editor.ResManager.LoadAsset(modelName);
                }

                if (assetType == "Part")
                {
                    Editor.FileSelection._searchInput = modelName;
                    Editor.ResManager.LoadPart(modelName);
                }

                if (initcmd.Length > 3)
                {
                    var mapId = initcmd[3];

                    if (assetType == "MapPiece")
                    {
                        var mapPieceName = modelName.Replace(mapId, "m");
                        Editor.FileSelection._searchInput = mapPieceName;
                        Editor.ResManager.LoadMapPiece(modelName, mapId);
                    }
                }
            }
        }
    }
}
