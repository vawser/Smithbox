using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Editors.ModelEditor;

public static class ColMatConfig
{
    public static Dictionary<int, string> DS3_MatNames = new()
    {
        { 0, "Dummy" },
        { 1, "Cobblestone" },
        { 2, "Stone" },
        { 3, "Soil" },
        { 4, "Wood" },
        { 5, "Grassland" },
        { 6, "Gravel" },
        { 7, "Missing" },
        { 8, "Trees" },
        { 9, "Swamp" },
        { 10, "Nest" },
        { 11, "Iron" },
        { 12, "Flesh" },
        { 13, "Sand" },
        { 14, "Bones" },
        { 15, "Ash" },
        { 16, "RoofTiles" },
        { 17, "Cloth" },
        { 18, "FallenLeaves" },
        { 19, "Bell" },
        { 20, "WaterPuddle" },
        { 21, "WaterAnkles" },
        { 22, "WaterKnees" },
        { 23, "PoisonSwampShallow" },
        { 24, "PoisonSwampDeep" },
        { 25, "PoisonSwampKnees" },
        { 26, "PoisonSwampPuddle" },
        { 27, "WaterWaist" },
        { 28, "Carpet" },
        { 29, "Without" },
        { 30, "Eclipse" },
        { 31, "GodGraveNet" }
    };

    public static Dictionary<int, string> DS3_MatProperties = new()
    {
        { 100, "Safe1" },
        { 200, "Safe2" },
        { 300, "Safe3" },
        { 400, "Slide1" },
        { 500, "Slide2" },
    };

    public static Dictionary<int, string> ER_MatNames = new()
    {
        { 0, "None" },
        { 1, "Stock" },
        { 2, "Rock" },
        { 3, "Sand" },
        { 4, "Wood" },
        { 5, "Dirt" },
        { 6, "Ore" },
        { 7, "Lava" },
        { 8, "ScarletTree" },
        { 9, "9" },
        { 10, "10" },
        { 11, "IronGrate" },
        { 12, "ScarletMushroom" },
        { 13, "13" },
        { 14, "Bone" },
        { 15, "15" },
        { 16, "16" },
        { 17, "17" },
        { 18, "18" },
        { 19, "19" },
        { 20, "20" },
        { 21, "Water" },
        { 22, "22" },
        { 23, "ShallowPoisonSwamp" },
        { 24, "PoisonSwamp" },
        { 25, "25" },
        { 26, "26" },
        { 27, "27" },
        { 28, "28" },
        { 29, "29" },
        { 30, "30" },
        { 31, "31" },
        { 32, "32" },
        { 33, "33" },
        { 34, "34" },
        { 35, "35" },
        { 36, "36" },
        { 37, "37" },
        { 38, "38" },
        { 39, "39" },
        { 40, "40" },
        { 41, "41" },
        { 42, "42" },
        { 43, "43" },
        { 44, "RainDirt" },
        { 45, "45" },
        { 46, "ScarletSwamp" },
        { 47, "47" },
        { 48, "48" },
        { 49, "LakeofRot" }
    };

    public static Dictionary<int, string> ER_MatProperties = new()
    {
        { 100, "Safe1" },
        { 200, "Safe2" },
        { 300, "Safe3" },
        { 400, "Slide1" },
        { 500, "Slide2" },
    };
}

public class CollisionMaterialDef
{
    public string FlverMaterial { get; set; }
    public string CollisionMateral { get; set; }
}