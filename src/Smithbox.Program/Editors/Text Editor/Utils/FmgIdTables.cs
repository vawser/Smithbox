using StudioCore.Application;
using System;
using System.Collections.Generic;

namespace StudioCore.Editors.TextEditor;

/// <summary>The kinds of text container (msgbnd) the FMG naming tells apart, by their path</summary>
public enum FmgContainerKind
{
    Item,
    Menu,
    Sample,
    NgWord,
    SellRegion
}

/// <summary>
///     Which enum names the FMGs (by BND ID) of each kind of text container, per game. The enums'
///     <see cref="System.ComponentModel.DataAnnotations.DisplayAttribute"/>s hold the names: Name is the display name,
///     ShortName the grouping and Description the DLC grouping; the member name is the internal name.
/// </summary>
public static class FmgIdTables
{
    /// <param name="LogName">The game as the "missing enum" log messages name it</param>
    /// <param name="Tables">In the order the containers are matched: a path matching an earlier kind wins</param>
    private sealed record GameTables(string LogName, (FmgContainerKind Kind, Type Enum)[] Tables);

    private static readonly Dictionary<ProjectType, GameTables> Games = new()
    {
        [ProjectType.DES] = new("DES",
        [
            (FmgContainerKind.Item, typeof(Item_MsgBndID_DES)),
            (FmgContainerKind.Menu, typeof(Menu_MsgBndID_DES)),
            (FmgContainerKind.Sample, typeof(Sample_MsgBndID_DES)),
        ]),
        [ProjectType.DS1] = new("DS1",
        [
            (FmgContainerKind.Item, typeof(Item_MsgBndID_DS1)),
            (FmgContainerKind.Menu, typeof(Menu_MsgBndID_DS1)),
        ]),
        // Remastered has the same text containers as Prepare to Die
        [ProjectType.DS1R] = new("DS1",
        [
            (FmgContainerKind.Item, typeof(Item_MsgBndID_DS1)),
            (FmgContainerKind.Menu, typeof(Menu_MsgBndID_DS1)),
        ]),
        [ProjectType.BB] = new("BB",
        [
            (FmgContainerKind.Item, typeof(Item_MsgBndID_BB)),
            (FmgContainerKind.Menu, typeof(Menu_MsgBndID_BB)),
        ]),
        [ProjectType.DS3] = new("DS3",
        [
            (FmgContainerKind.Item, typeof(Item_MsgBndID_DS3)),
            (FmgContainerKind.Menu, typeof(Menu_MsgBndID_DS3)),
            (FmgContainerKind.NgWord, typeof(NgWord_MsgBndID_DS3)),
        ]),
        [ProjectType.SDT] = new("SDT",
        [
            (FmgContainerKind.Item, typeof(Item_MsgBndID_SDT)),
            (FmgContainerKind.Menu, typeof(Menu_MsgBndID_SDT)),
            (FmgContainerKind.SellRegion, typeof(SellRegion_MsgBndID_SDT)),
        ]),
        [ProjectType.ER] = new("ER",
        [
            (FmgContainerKind.Item, typeof(Item_MsgBndID_ER)),
            (FmgContainerKind.Menu, typeof(Menu_MsgBndID_ER)),
            (FmgContainerKind.NgWord, typeof(NgWord_MsgBndID_ER)),
            (FmgContainerKind.SellRegion, typeof(SellRegion_MsgBndID_ER)),
        ]),
        [ProjectType.AC6] = new("AC6",
        [
            (FmgContainerKind.Item, typeof(Item_MsgBndID_AC6)),
            (FmgContainerKind.Menu, typeof(Menu_MsgBndID_AC6)),
            (FmgContainerKind.NgWord, typeof(NgWord_MsgBndID_AC6)),
        ]),
        [ProjectType.NR] = new("NR",
        [
            (FmgContainerKind.Item, typeof(Item_MsgBndID_NR)),
            (FmgContainerKind.Menu, typeof(Menu_MsgBndID_NR)),
            (FmgContainerKind.NgWord, typeof(NgWord_MsgBndID_NR)),
        ]),
        // The network test's messages have always named it DB
        [ProjectType.DB_NT] = new("DB",
        [
            (FmgContainerKind.Item, typeof(Item_MsgBndID_DB_NT)),
            (FmgContainerKind.Menu, typeof(Menu_MsgBndID_DB_NT)),
        ]),
        // The full release starts from the network test's tables (as its FMG Descriptor Registry starts from the
        // network test's), until its own text is surveyed. Its own enums (Item_/Menu_MsgBndID_DB) are empty.
        [ProjectType.DB] = new("DB",
        [
            (FmgContainerKind.Item, typeof(Item_MsgBndID_DB_NT)),
            (FmgContainerKind.Menu, typeof(Menu_MsgBndID_DB_NT)),
        ]),

        // DS2 and DS2S name their FMGs by file (no BND IDs)
    };

    /// <summary>
    ///     The enum naming the container's FMGs: the game's first table whose kind the container's path matches
    /// </summary>
    /// <returns>False if the game has no table for the container</returns>
    public static bool TryFind(ProjectType game, TextContainerWrapper info, out FmgContainerKind kind, out Type enumType,
        out string logName)
    {
        kind = default;
        enumType = null;
        logName = null;

        if (!Games.TryGetValue(game, out var tables))
            return false;

        foreach (var table in tables.Tables)
        {
            if (Matches(table.Kind, info))
            {
                kind = table.Kind;
                enumType = table.Enum;
                logName = tables.LogName;
                return true;
            }
        }

        return false;
    }

    private static bool Matches(FmgContainerKind kind, TextContainerWrapper info) => kind switch
    {
        FmgContainerKind.Item => TextUtils.IsItemContainer(info),
        FmgContainerKind.Menu => TextUtils.IsMenuContainer(info),
        FmgContainerKind.Sample => TextUtils.IsSampleContainer(info),
        FmgContainerKind.NgWord => TextUtils.IsNgWordContainer(info),
        FmgContainerKind.SellRegion => TextUtils.IsSellRegionContainer(info),
        _ => false
    };
}
