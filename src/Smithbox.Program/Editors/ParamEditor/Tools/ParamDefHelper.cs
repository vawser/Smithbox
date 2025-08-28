using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor.Tools;

public static class ParamDefHelper
{
    public static List<string> WrittenParamTypes = new();

    public static void GenerateBaseParamDefFile(string fileName, string paramType)
    {
        if (!WrittenParamTypes.Contains(paramType))
        {
            // TEMP: used to create all def files for new game
            var xmlTemplate = $"<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<PARAMDEF XmlVersion=\"1\">\r\n  <ParamType>{paramType}</ParamType>\r\n  <DataVersion>1</DataVersion>\r\n  <BigEndian>False</BigEndian>\r\n  <Unicode>True</Unicode>\r\n  <FormatVersion>203</FormatVersion>\r\n  <Fields>\r\n    <Field Def=\"u8 XXXXX\" />\r\n  </Fields>\r\n</PARAMDEF>";

            var writePath = $@"{CFG.Current.SmithboxBuildFolder}\src\Smithbox.Data\Assets\PARAM\NR\Defs\{fileName}.xml";

            File.WriteAllText(writePath, xmlTemplate);

            WrittenParamTypes.Add(paramType);
        }
    }
}
