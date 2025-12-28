using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.MapEditor;

public static class PropInfo_Region_Connection
{
    public static void Display(MapEditorScreen editor, Entity firstEnt)
    {
        if (firstEnt.IsRegionConnection())
        {
            sbyte[] mapIds = (sbyte[])PropFinderUtil.FindPropertyValue("TargetMapID", firstEnt.WrappedObject);

            string mapString = "m";

            if (mapIds != null)
            {
                for (int i = 0; i < mapIds.Length; i++)
                {
                    var num = mapIds[i];
                    var str = "";
                    if (num < 10)
                    {
                        str = $"0{num}";
                    }
                    else
                    {
                        str = num.ToString();
                    }

                    if (i < mapIds.Length - 1)
                    {
                        mapString = mapString + $"{str}_";
                    }
                    else
                    {
                        mapString = mapString + $"{str}";
                    }
                }
            }

            ImGui.Separator();
            ImGui.Text($"Target Map ID:");
            ImGui.Separator();

            ImGui.Text(mapString);
            UIHelper.DisplayAlias(AliasHelper.GetMapNameAlias(editor.Project, mapString));
            ImGui.Text("");
        }
    }
}
