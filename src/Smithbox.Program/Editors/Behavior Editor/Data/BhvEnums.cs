using System.Text;

namespace StudioCore.Editors.BehaviorEditor;

/// <summary>
/// The AC6 TDF enums the BHV fields are typed by (Assets/PARAM/AC6/Tdfs, Shift-JIS). Matched by value in P0's
/// enummatch survey; see Claude/Documentation/BHV Format.md.
/// </summary>
public class BhvEnums
{
    public const string ActionType = "ActionTypeEnum";
    public const string ActionAiType = "ActionAiTypeEnum";
    public const string ActionSoundType = "ActionSoundTypeEnum";
    public const string AtkActionType = "AtkActionTypeEnum";
    public const string LocalActionType = "LocalActionTypeEnum";

    private static readonly string[] Names = [ActionType, ActionAiType, ActionSoundType, AtkActionType, LocalActionType];

    private readonly Dictionary<string, Dictionary<int, string>> Enums = new();

    /// <summary>
    /// The name of a value, or null if the enum or the value is unknown
    /// </summary>
    public string GetName(string enumName, int value)
    {
        if (enumName == null || !Enums.TryGetValue(enumName, out var values))
            return null;

        return values.TryGetValue(value, out var name) ? name : null;
    }

    /// <summary>
    /// Every named value of an enum, or null if the enum wasn't loaded
    /// </summary>
    public IReadOnlyDictionary<int, string> GetValues(string enumName)
    {
        return enumName != null && Enums.TryGetValue(enumName, out var values) ? values : null;
    }

    public static BhvEnums Load(ProjectEntry project)
    {
        var result = new BhvEnums();
        var folder = Path.Join(AppContext.BaseDirectory, "Assets", "PARAM", "AC6", "Tdfs");

        if (!Directory.Exists(folder))
            return result;

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var sjis = Encoding.GetEncoding(932);

        foreach (var name in Names)
        {
            var path = Path.Join(folder, $"{name}.tdf");

            if (!File.Exists(path))
                continue;

            try
            {
                var values = new Dictionary<int, string>();

                // "ENUM_NAME", "u8", then "label","value" per line
                foreach (var line in File.ReadAllLines(path, sjis).Skip(2))
                {
                    var cut = line.LastIndexOf("\",\"", StringComparison.Ordinal);
                    if (cut < 1)
                        continue;

                    var label = line[1..cut];
                    var valueText = line[(cut + 3)..].TrimEnd('"');

                    if (int.TryParse(valueText, out var value))
                        values.TryAdd(value, label);
                }

                result.Enums[name] = values;
            }
            catch (Exception e)
            {
                Smithbox.LogError(result, LOC.Get("BHV_Data_Log_Failed_to_Read_Enum", path), e);
            }
        }

        return result;
    }
}
