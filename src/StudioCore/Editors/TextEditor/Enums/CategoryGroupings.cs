using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor.Enums;

public static class CategoryGroupings
{
    public static List<TextContainerCategory> DES_Languages = new()
    {
        TextContainerCategory.Japanese,
        TextContainerCategory.English
    };

    public static List<TextContainerCategory> DS1_Languages = new()
    {
        TextContainerCategory.English,
        TextContainerCategory.French,
        TextContainerCategory.German,
        TextContainerCategory.Italian,
        TextContainerCategory.Japanese,
        TextContainerCategory.Korean,
        TextContainerCategory.Polish,
        TextContainerCategory.Russian,
        TextContainerCategory.English,
        TextContainerCategory.Spanish,
        TextContainerCategory.TraditionalChinese
    };

    public static List<TextContainerCategory> DS2_Languages = new()
    {
        TextContainerCategory.English,
        TextContainerCategory.French,
        TextContainerCategory.German,
        TextContainerCategory.Italian,
        TextContainerCategory.Japanese,
        TextContainerCategory.Korean,
        TextContainerCategory.Polish,
        TextContainerCategory.Russian,
        TextContainerCategory.English,
        TextContainerCategory.Spanish,
        TextContainerCategory.TraditionalChinese,
        TextContainerCategory.SpanishNeutral,
        TextContainerCategory.Portuguese,
    };

    public static List<TextContainerCategory> BB_Languages = new()
    {
        TextContainerCategory.English,
        TextContainerCategory.French,
        TextContainerCategory.German,
        TextContainerCategory.Italian,
        TextContainerCategory.Japanese,
        TextContainerCategory.Korean,
        TextContainerCategory.Polish,
        TextContainerCategory.Russian,
        TextContainerCategory.English,
        TextContainerCategory.Spanish,
        TextContainerCategory.TraditionalChinese,
        TextContainerCategory.Portuguese,
        TextContainerCategory.SpanishLatin,
        TextContainerCategory.SimplifiedChinese,
        TextContainerCategory.Danish,
        TextContainerCategory.EnglishUK,
        TextContainerCategory.Finnish,
        TextContainerCategory.Dutch,
        TextContainerCategory.Norwegian,
        TextContainerCategory.PortugueseLatin,
        TextContainerCategory.Swedish,
        TextContainerCategory.Turkish
    };

    public static List<TextContainerCategory> DS3_Languages = new()
    {
        TextContainerCategory.English,
        TextContainerCategory.French,
        TextContainerCategory.German,
        TextContainerCategory.Italian,
        TextContainerCategory.Japanese,
        TextContainerCategory.Korean,
        TextContainerCategory.Polish,
        TextContainerCategory.Russian,
        TextContainerCategory.English,
        TextContainerCategory.Spanish,
        TextContainerCategory.SpanishLatin,
        TextContainerCategory.TraditionalChinese,
        TextContainerCategory.SimplifiedChinese,
        TextContainerCategory.Portuguese,
    };


    public static List<TextContainerCategory> SDT_Languages = new()
    {
        TextContainerCategory.English,
        TextContainerCategory.French,
        TextContainerCategory.German,
        TextContainerCategory.Italian,
        TextContainerCategory.Japanese,
        TextContainerCategory.Korean,
        TextContainerCategory.Polish,
        TextContainerCategory.Russian,
        TextContainerCategory.English,
        TextContainerCategory.Spanish,
        TextContainerCategory.SpanishLatin,
        TextContainerCategory.TraditionalChinese,
        TextContainerCategory.SimplifiedChinese,
        TextContainerCategory.Thai,
        TextContainerCategory.Portuguese,
    };

    public static List<TextContainerCategory> ER_Languages = new()
    {
        TextContainerCategory.English,
        TextContainerCategory.French,
        TextContainerCategory.German,
        TextContainerCategory.Italian,
        TextContainerCategory.Japanese,
        TextContainerCategory.Korean,
        TextContainerCategory.Polish,
        TextContainerCategory.Russian,
        TextContainerCategory.English,
        TextContainerCategory.Spanish,
        TextContainerCategory.SpanishLatin,
        TextContainerCategory.TraditionalChinese,
        TextContainerCategory.SimplifiedChinese,
        TextContainerCategory.Thai,
        TextContainerCategory.Portuguese,
        TextContainerCategory.Arabic,
    };

    public static List<TextContainerCategory> AC6_Languages = new()
    {
        TextContainerCategory.English,
        TextContainerCategory.French,
        TextContainerCategory.German,
        TextContainerCategory.Italian,
        TextContainerCategory.Japanese,
        TextContainerCategory.Korean,
        TextContainerCategory.Polish,
        TextContainerCategory.Russian,
        TextContainerCategory.English,
        TextContainerCategory.Spanish,
        TextContainerCategory.SpanishLatin,
        TextContainerCategory.TraditionalChinese,
        TextContainerCategory.SimplifiedChinese,
        TextContainerCategory.Thai,
        TextContainerCategory.Portuguese,
        TextContainerCategory.Arabic,
    };

    public static List<TextContainerCategory> NonLanguages = new()
    {
        TextContainerCategory.None,
        TextContainerCategory.Common,
        TextContainerCategory.SellRegion
    };
}
