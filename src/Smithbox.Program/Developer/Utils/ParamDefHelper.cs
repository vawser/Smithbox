using SoulsFormats;
using System.Text;
using static SoulsFormats.PARAMDEF;

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

    public static void ConvertPARAMDEFtoXML()
    {
        var srcDir = @"C:\Users\benja\Programming\Dump\er_switch\paramdef\paramdef-paramdefbnd-dcx";
        var outputDir = @"C:\Users\benja\Programming\Dump\er_switch\paramdef\output";
        
        foreach(var file in Directory.EnumerateFiles(srcDir))
        {
            var filename = Path.GetFileNameWithoutExtension(file);

            try
            {
                var paramdef = PARAMDEF.Read(file);

                StringBuilder fileText = new();

                fileText.AppendLine($"<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                fileText.AppendLine($"<PARAMDEF XmlVersion=\"2\">");
                fileText.AppendLine($"  <ParamType>{paramdef.ParamType}</ParamType>");
                fileText.AppendLine($"  <Index>65</Index>");
                fileText.AppendLine($"  <DataVersion>{paramdef.DataVersion}</DataVersion>");
                fileText.AppendLine($"  <BigEndian>{paramdef.BigEndian}</BigEndian>");
                fileText.AppendLine($"  <Unicode>{paramdef.Unicode}</Unicode>");
                fileText.AppendLine($"  <FormatVersion>{paramdef.FormatVersion}</FormatVersion>");
                fileText.AppendLine($"  <Fields>");

                foreach (var field in paramdef.Fields)
                {
                    WriteFieldXml(fileText, field);
                }

                fileText.AppendLine($"  </Fields>");
                fileText.AppendLine($"</PARAMDEF>");

                var outputPath = Path.Join(outputDir, $"{filename}.xml");
                File.WriteAllText(outputPath, fileText.ToString());
            }
            catch(Exception)
            {

            }
        }
    }

    private static List<string> primitives = new List<string>()
    {
        "s8",
        "u8",
        "s16",
        "u16",
        "s32",
        "u32",
        "b32",
        "f32",
        "angle32",
        "f64",
        "dummy8",
        "fixstr",
        "fixstrW"
    };

    private static void WriteFieldXml(StringBuilder fileText, PARAMDEF.Field field)
    {
        var defaultVal = "";
        if(field.Default != null)
        {
            defaultVal = field.Default.ToString();
            if (defaultVal != "0")
            {
                fileText.AppendLine($"    <Field Def=\"{field.ToString()} = {defaultVal}\">");
            }
            else
            {
                fileText.AppendLine($"    <Field Def=\"{field.ToString()}\">");
            }
        }
        else
        {
            fileText.AppendLine($"    <Field Def=\"{field.ToString()}\">");
        }


        if (field.DisplayName != null)
        {
            fileText.AppendLine($"      <DisplayName>{field.DisplayName}</DisplayName>");
        }

        var hasEnum = false;
        if (!primitives.Contains(field.InternalType))
        {
            hasEnum = true;
            fileText.AppendLine($"      <Enum>{field.InternalType}</Enum>");
        }

        if (field.Description != null)
        {
            fileText.AppendLine($"      <Description>{field.Description}</Description>");
        }

        if (field.DisplayType != DefType.dummy8)
        {
            if (field.EditFlags == EditFlags.None)
            {
                fileText.AppendLine($"      <EditFlags>{field.EditFlags}</EditFlags>");
            }
        }

        if (!hasEnum)
        {
            if (field.Minimum != null)
            {
                fileText.AppendLine($"      <Minimum>{field.Minimum}</Minimum>");
            }
        }

        if (field.Maximum != null)
        {
            fileText.AppendLine($"      <Maximum>{field.Maximum}</Maximum>");
        }

        if (!hasEnum)
        {
            if (field.Increment != null && field.Increment.ToString() != "1")
            {
                fileText.AppendLine($"      <Increment>{field.Increment}</Increment>");
            }
        }

        if (field.SortID != 0)
        {
            fileText.AppendLine($"      <SortID>{field.SortID}</SortID>");
        }

        if (field.UnkB8 != null)
        {
            fileText.AppendLine($"      <UnkB8>{field.UnkB8}</UnkB8>");
        }

        if (field.UnkC0 != null)
        {
            fileText.AppendLine($"      <UnkC0>{field.UnkC0}</UnkC0>");
        }

        if (field.UnkC8 != null)
        {
            fileText.AppendLine($"      <UnkC8>{field.UnkC8}</UnkC8>");
        }

        fileText.AppendLine($"    </Field>");
    }
}

