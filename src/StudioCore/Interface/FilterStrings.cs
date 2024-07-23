using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Interface;
public static class FilterStrings
{
    // These patterns are meant to be passed directly into PlatformUtils.
    // Everything about their handling should be done there.

    // Game Executable (.EXE, EBOOT.BIN)|*.EXE*;*EBOOT.BIN*
    // Windows executable (*.EXE)|*.EXE*
    // Playstation executable (*.BIN)|*.BIN*
    public static string GameExecutableFilter = "exe,bin";

    // Project file (project.json)|PROJECT.JSON
    public static string ProjectJsonFilter = "json";

    // Regulation file (regulation.bin)|REGULATION.BIN
    public static string RegulationBinFilter = "bin";

    // Data file (Data0.bdt)|DATA0.BDT
    public static string Data0Filter = "bdt";

    // ParamBndDcx (gameparam.parambnd.dcx)|GAMEPARAM.PARAMBND.DCX
    public static string ParamBndDcxFilter = "parambnd.dcx";

    // ParamBnd (gameparam.parambnd)|GAMEPARAM.PARAMBND
    public static string ParamBndFilter = "parambnd";

    // Enc_RegBndDcx (enc_regulation.bnd.dcx)|ENC_REGULATION.BND.DCX
    public static string EncRegulationFilter = "bnd.dcx";

    // Loose Param file (*.Param)|*.Param
    public static string ParamLooseFilter = "param";

    // CSV file (*.csv)|*.csv
    public static string CsvFilter = "csv";

    // Text file (*.txt)|*.txt
    public static string TxtFilter = "txt";

    // Exported FMGs (*.fmg.json)|*.fmg.json
    public static string FmgJsonFilter = "fmg.json";

    // Exported FMGs (*.fmg.json)|*.fmg.json
    public static string FmgMergeJsonFilter = "fmgmerge.json";


    // All file filter is implicitly added by NFD. Ideally this is used explicitly.
    // All files|*.*
}
