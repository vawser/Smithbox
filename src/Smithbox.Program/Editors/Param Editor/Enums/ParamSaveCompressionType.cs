using SoulsFormats;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public enum ParamSaveCompressionType
{
    [Display(Name = "Default")]
    Default,
    [Display(Name = "None")]
    None,
    [Display(Name = "ZLIB")]
    Zlib,
    [Display(Name = "DCP-EDGE")]
    DCP_EDGE,
    [Display(Name = "DCP-DEFLATE")]
    DCP_DFLT,
    [Display(Name = "ZSTD [ER:SOTE]")]
    DCX_ZSTD,
    [Display(Name = "EDGE [DES]")]
    DCX_EDGE,
    [Display(Name = "DEFLATE (10000.24.9) [DS1/DS2]")]
    DCX_DFLT_10000_24_9,
    [Display(Name = "DEFLATE (10000.44.9) [BB/DS3]")]
    DCX_DFLT_10000_44_9,
    [Display(Name = "DEFLATE (11000.44.8) [DS3]")]
    DCX_DFLT_11000_44_8,
    [Display(Name = "DEFLATE (11000.44.9) [SDT]")]
    DCX_DFLT_11000_44_9,
    [Display(Name = "DEFLATE (11000.44.9.15) [ER]")]
    DCX_DFLT_11000_44_9_15,
    [Display(Name = "KRAK")]
    DCX_KRAK,
    [Display(Name = "KRAK-MAX")]
    DCX_KRAK_MAX
}

public static class ParamSaveCompression
{
    public static DCX.Type GetCurrentOverride()
    {
        var compressionOverride = CFG.Current.ParamEditor_CompressionOverride;

        switch(compressionOverride)
        {
            case ParamSaveCompressionType.Default: return DCX.Type.None;
            case ParamSaveCompressionType.None: return DCX.Type.None;
            case ParamSaveCompressionType.Zlib: return DCX.Type.Zlib;
            case ParamSaveCompressionType.DCP_EDGE: return DCX.Type.DCP_EDGE;
            case ParamSaveCompressionType.DCP_DFLT: return DCX.Type.DCP_DFLT;
            case ParamSaveCompressionType.DCX_ZSTD: return DCX.Type.DCX_ZSTD;
            case ParamSaveCompressionType.DCX_EDGE: return DCX.Type.DCX_EDGE;
            case ParamSaveCompressionType.DCX_DFLT_10000_24_9: return DCX.Type.DCX_DFLT_10000_24_9;
            case ParamSaveCompressionType.DCX_DFLT_10000_44_9: return DCX.Type.DCX_DFLT_10000_44_9;
            case ParamSaveCompressionType.DCX_DFLT_11000_44_8: return DCX.Type.DCX_DFLT_11000_44_8;
            case ParamSaveCompressionType.DCX_DFLT_11000_44_9: return DCX.Type.DCX_DFLT_11000_44_9;
            case ParamSaveCompressionType.DCX_DFLT_11000_44_9_15: return DCX.Type.DCX_DFLT_11000_44_9_15;
            case ParamSaveCompressionType.DCX_KRAK: return DCX.Type.DCX_KRAK;
            case ParamSaveCompressionType.DCX_KRAK_MAX: return DCX.Type.DCX_KRAK_MAX;
        }

        return DCX.Type.None;
    }

}
