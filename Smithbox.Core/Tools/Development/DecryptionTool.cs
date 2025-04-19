using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Tools.Development;

public static class DecryptionTool
{
    public static void DecryptRegulation()
    {
        var path = "C:\\Users\\benja\\Modding\\Elden Ring\\Projects\\ER-SOTE\\Mod\\regulation.bin";
        var writePath = "C:\\Users\\benja\\Modding\\Elden Ring\\Projects\\ER-SOTE\\Mod\\decrypted_regulation.bin";

        byte[] bytes = File.ReadAllBytes(path);
        bytes = SFUtil.DecryptByteArray(SFUtil.erRegulationKey, bytes);
        File.WriteAllBytes(writePath, bytes);
    }
}
