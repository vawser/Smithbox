using StudioCore.Platform;
using StudioCore.Scene;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Drawing;

namespace StudioCore.Tools.Randomiser;

[JsonSourceGenerationOptions(WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata, IncludeFields = true)]
[JsonSerializable(typeof(RandomiserCFG))]
internal partial class RandomiserCfgSerializerContext : JsonSerializerContext
{
}

public class RandomiserCFG
{
    public const string FolderName = "Smithbox";
    public const string Config_FileName = "Randomiser_Config.json";
    public static RandomiserCFG Current { get; private set; }
    public static RandomiserCFG Default { get; } = new();

    public static bool IsEnabled = true;
    private static readonly object _lock_SaveLoadCFG = new();

    // CFG

    // ER - World Treasures
    public bool ER_IncludeWorldTreasures = true;

    public string ER_WorldTreasures = "[10000005,18000901]:[30000010,39200503],[99010000,99010010]:[900003000]:[942370060,944360200]:[1033400100,1054550800]";

    public bool ER_IncludeWorldTreasures_DLC = true;
    public string ER_WorldTreasures_DLC = "[20000000,28000110]:[40000000:43010901]:[2044410000,2054400000]";

    // ER - Key Items
    public bool ER_IgnoreKeyItems = true;
    public string ER_KeyItems = "100,101,102,103,104,105,106,107,108,109,110,111,112,113,114,115,130,135,150,166,170,171.172,174,175,178,179,180,181,182,183,184,191,192,193,194,195,196,250,251,1000,1001,1002,1003,1004,1005,1006,1007,1008,1009,1010,1011,1012,1013,1014,1015,1016,1017,1018,1019,1020,1021,1022,1023,1024,1025,1050,1051,1052,1053,1054,1055,1056,1057,1058,1059,1060,1061,1062,1063,1064,1065,1066,1067,1068,1069,1070,1071,1072,1073,1074,1075,2190,8010,8102,8105,8106,8107,8109,8111,8121,8126,8127,8128,8129,8130,8131,8132,8133,8134,8136,8137,8142,8143,8144,8146,8147,8148,8149,8150,8151,8152,8153,8154,8155,8156,8158,8159,8161,8162,8163,8164,8166,8167,8168,8169,8171,8172,8173,8174,8175,8176,8181,8182,8183,8184,8187,8188,8189,8190,8191,8192,8193,8194,8195,8196,8197,8198,8199,8200,8201,8202,8203,8204,8205,8206,8221,8222,8223,8224,8225,8226,8227,8500,8590,8600,8601,8602,8603,8604,8605,8606,8607,8608,8609,8610,8611,8612,8613,8614,8615,8616,8617,8618,8660,8700,8701,8702,8703,8704,8705,8706,8707,8708,8709,8710,8711,8712,8713,8714,8715,8716,8717,8750,8751,8752,8753,8754,8755,8756,8757,8758,8759,8760,8761,8762,8763,8765,8767,8975,8976,8977,8978,8979,8980,10080,2008000,2008003,2008004,2008005,2008006,2008007,2008008,2008011,2008012,2008013,2008014,2008015,2008016,2008017,2008018,2008019,2008021,2008023,2008025,2008027,2008028,2008029,2008030,2008031,2008032,2008034,2008036,2008037,2008131,2008200,2008201,2008202,2008600,2008601,2008602,2008603,2008604";

    public bool ER_IgnoreProgressionItems = false;
    public string ER_ProgressionItems = "8000,8186,8850,8851,8855,8856,8857,8858,8859,8860,8861,8862,8863,8864,8865,8866,8910,8911,8912,8913,8915,8916,8917,8918,8919,8920,8921,8922,8923,8924,8925,8926,8927,8928,8929,8930,8931,8932,8933,8934,8935,8936,8937,8938,8939,8940,8941,8942,8943,8944,8945,8946,8947,8948,8951,8952,8953,8954,8955,8956,8957,8958,8959,8960,8961,8962,8963,8964,8965,8970,8971,8972,8973,8974,9300,9301,9302,9303,9305,9306,9307,9308,9309,9310,9311,9312,9313,9320,9321,9322,9323,9325,9326,9327,9329,9328,9330,9331,9340,9341,9342,9343,9344,9345,9346,9347,9348,9360,9361,9363,9364,9365,9380,9383,9384,9385,9386,9387,9388,9389,9390,9391,9392,9400,9401,9402,9403,9420,9421,9422,9423,9440,9441,9500,9501,9510,10010,10020,10030,10040,10060,10070,2008900,2008901,2008902,2008903,2008904,2008905,2008906,2008907,2008908,2008909,2009301,2009302,2009303,2009304,2009305,2009306,2009307,2009308,2009309,2009310,2009311,2009312,2009313,2009314,2009315,2009316,2009317,2009318,2009319,2009320,2009321,2009322,2009323,2009324,2009325,2009326,2009327,2009328,2009329,2009330,2009331,2009332,2009333,2009334,2009335,2009336,2009337,2009338,2009339,2009340,2009341,2009342,2009343,2009344,2009345,2009500,2010000,2010100";

    public string ER_IgnoredItems = "9000,9001,9002,9003,9004,9005,9006,9007,9008,9009,9010,9011,9012,9013,9014,9015,9016,9017,9018,9019,9020,9021,9022,9023,9024,9026,9027,9028,9029,9030,9031,9032,9033,9034,9035,9036,9037,9039,9040,9041,9042,9043,9045,9046,9047,9048,9049,9050,9100,9101,9102,9103,9104,9105,9106,9107,9108,9109,9110,9111,9112,9113,9115,9116,9117,9118,9119,9120,9121,9122,9123,9124,9125,9126,9127,9128,9129,9130,9131,9132,9134,9135,9136,9137,9138,9140,9141,9142,9150,9151,9152,9153,9195,2009000,2009001,2009002,2009003,2009004,2009160,2009161,2009162";

    public static string GetConfigFilePath()
    {
        return $@"{GetConfigFolderPath()}\{Config_FileName}";
    }

    public static string GetConfigFolderPath()
    {
        return $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\{FolderName}";
    }

    private static void LoadConfig()
    {
        if (!File.Exists(GetConfigFilePath()))
        {
            Current = new RandomiserCFG();
        }
        else
        {
            try
            {
                var options = new JsonSerializerOptions();
                Current = JsonSerializer.Deserialize(File.ReadAllText(GetConfigFilePath()),
                    RandomiserCfgSerializerContext.Default.RandomiserCFG);

                if (Current == null)
                {
                    throw new Exception("JsonConvert returned null");
                }
            }
            catch (Exception e)
            {
                DialogResult result = PlatformUtils.Instance.MessageBox(
                    $"{e.Message}\n\nConfig could not be loaded. Reset settings?",
                    $"{Config_FileName} Load Error", MessageBoxButtons.YesNo);
                if (result == DialogResult.No)
                {
                    throw new Exception($"{Config_FileName} could not be loaded.\n\n{e.Message}");
                }

                Current = new RandomiserCFG();
                SaveConfig();
            }
        }
    }
    private static void SaveConfig()
    {
        var json = JsonSerializer.Serialize(
            Current, RandomiserCfgSerializerContext.Default.RandomiserCFG);

        File.WriteAllText(GetConfigFilePath(), json);
    }

    public static void Save()
    {
        if (IsEnabled)
        {
            lock (_lock_SaveLoadCFG)
            {
                if (!Directory.Exists(GetConfigFolderPath()))
                {
                    Directory.CreateDirectory(GetConfigFolderPath());
                }

                SaveConfig();
            }
        }
    }

    public static void AttemptLoadOrDefault()
    {
        if (IsEnabled)
        {
            lock (_lock_SaveLoadCFG)
            {
                if (!Directory.Exists(GetConfigFolderPath()))
                {
                    Directory.CreateDirectory(GetConfigFolderPath());
                }

                LoadConfig();
            }
        }
    }
}
