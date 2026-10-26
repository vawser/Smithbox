using SoulsFormats;
using System.Text;

namespace StudioCore.Application;

// For reference: actual usage is in SoulsFormats Param.cs
public static class ParamDefHelper
{
    public static void GenerateEmptyDefFile(string filename, string paramType)
    {
        StringBuilder fileText = new();

        fileText.AppendLine("<PARAMDEF XmlVersion=\"2\">");
        fileText.AppendLine($"  <ParamType>{paramType}</ParamType>");
        fileText.AppendLine($"  <DataVersion>0</DataVersion>");
        fileText.AppendLine($"  <BigEndian>False</BigEndian>");
        fileText.AppendLine($"  <Unicode>True</Unicode>");
        fileText.AppendLine($"  <FormatVersion>0</FormatVersion>");
        fileText.AppendLine($"  <Fields>");

        fileText.AppendLine($"  </Fields>");
        fileText.AppendLine($"</PARAMDEF>");

        var outputPath = $"C:\\Users\\benja\\Programming\\C#\\Smithbox\\src\\Smithbox.Data\\Assets\\PARAM\\DB\\Defs\\{filename}.xml";
        File.WriteAllText(outputPath, fileText.ToString());
    }

    private static List<string> WrittenParamTypes = new();

    public static void GenerateBaseParamDefFile(PARAMDEF def, int rowSize, string fileName)
    {
        var paramType = $"{def.ParamType}";

        if (WrittenParamTypes.Contains(paramType))
            return;

        StringBuilder fileText = new();

        fileText.AppendLine("<PARAMDEF XmlVersion=\"2\">");
        fileText.AppendLine($"  <ParamType>{paramType}</ParamType>");
        fileText.AppendLine($"  <DataVersion>0</DataVersion>");
        fileText.AppendLine($"  <BigEndian>False</BigEndian>");
        fileText.AppendLine($"  <Unicode>True</Unicode>");
        fileText.AppendLine($"  <FormatVersion>0</FormatVersion>");
        fileText.AppendLine($"  <Fields>");

        // Fields
        var offset = 0;
        var fieldIndex = 0;
        var dataLength = rowSize;

        while (offset + 4 <= dataLength)
        {
            WriteField_Int(fileText, $"unk{fieldIndex:X2}");
            offset += 4;
            fieldIndex++;
        }

        if (offset + 2 <= dataLength)
        {
            WriteField_Short(fileText, $"unk{fieldIndex:X2}");
            offset += 2;
            fieldIndex++;
        }

        if (offset < dataLength)
        {
            WriteField_Byte(fileText, $"unk{fieldIndex:X2}");
        }

        fileText.AppendLine($"  </Fields>");
        fileText.AppendLine($"</PARAMDEF>");

        var outputPath = $"C:\\Users\\benja\\Programming\\C#\\Smithbox\\src\\Smithbox.Data\\Assets\\PARAM\\DB\\Defs\\{fileName}.xml";
        File.WriteAllText(outputPath, fileText.ToString());

        WrittenParamTypes.Add(paramType);
    }

    private static void WriteField_Int(StringBuilder fileText, string fieldName)
    {
        var type = "s32";
        fileText.AppendLine($"    <Field Def=\"{type} {fieldName}\" />");
    }
    private static void WriteField_Short(StringBuilder fileText, string fieldName)
    {
        var type = "u16";
        fileText.AppendLine($"    <Field Def=\"{type} {fieldName}\" />");
    }
    private static void WriteField_Byte(StringBuilder fileText, string fieldName)
    {
        var type = "u8";
        fileText.AppendLine($"    <Field Def=\"{type} {fieldName}\" />");
    }
}

