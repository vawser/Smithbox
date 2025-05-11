using SoulsFormats;
using StudioCore.Core;
using System.Text.RegularExpressions;
using static SoulsFormats.GPARAM;

namespace StudioCore.GraphicsParamEditorNS;

public static class GparamUtils
{
    /// <summary>
    /// Search for valid duplicate name for a GPARAM file
    /// </summary>
    public static string CreateDuplicateFileName(string fileName)
    {
        Match mapMatch = Regex.Match(fileName, @"[0-9]{4}");

        if (mapMatch.Success)
        {
            var res = mapMatch.Groups[0].Value;

            int slot = 0;
            string slotStr = "";

            try
            {
                int number;
                int.TryParse(res, out number);

                slot = number + 1;
            }
            catch { }

            if (slot >= 100 && slot < 999)
            {
                slotStr = "0";
            }
            if (slot >= 10 && slot < 99)
            {
                slotStr = "00";
            }
            if (slot >= 0 && slot < 9)
            {
                slotStr = "000";
            }

            var finalSlotStr = $"{slotStr}{slot}";
            var final = fileName.Replace(res, finalSlotStr);

            return final;
        }
        else
        {
            Match dupeMatch = Regex.Match(fileName, @"__[0-9]{1}");

            if (dupeMatch.Success)
            {
                var res = dupeMatch.Groups[0].Value;

                Match numMatch = Regex.Match(res, @"[0-9]{1}");

                var num = numMatch.Groups[0].Value;
                try
                {
                    int number;
                    int.TryParse(res, out number);

                    number = number + 1;

                    return $"{fileName}__{number}";
                }
                catch
                {
                    return $"{fileName}__1";
                }
            }
            else
            {
                return $"{fileName}__1";
            }
        }
    }

    public static string GetReadableObjectTypeName(IField field)
    {
        string typeName = "Unknown";

        if (field is GPARAM.IntField)
        {
            typeName = "Signed Integer";
        }
        if (field is GPARAM.UintField)
        {
            typeName = "Unsigned Integer";
        }
        if (field is GPARAM.ShortField)
        {
            typeName = "Signed Short";
        }
        if (field is GPARAM.SbyteField)
        {
            typeName = "Signed Byte";
        }
        if (field is GPARAM.ByteField)
        {
            typeName = "Byte";
        }
        if (field is GPARAM.FloatField)
        {
            typeName = "Float";
        }
        if (field is GPARAM.Vector2Field)
        {
            typeName = "Vector2";
        }
        if (field is GPARAM.Vector3Field)
        {
            typeName = "Vector3";
        }
        if (field is GPARAM.Vector4Field)
        {
            typeName = "Vector4";
        }
        if (field is GPARAM.BoolField)
        {
            typeName = "Boolean";
        }
        if (field is GPARAM.ColorField)
        {
            typeName = "Color";
        }

        return typeName;
    }
}
