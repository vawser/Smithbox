using SoulsFormats;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// An edit of the Navigation tool's NVM mode (DeS, DS1, DS1R): swaps the edited NVMs, and the MCP and MCG when they
/// change, for the copies the edit made (NvmEdit), and back on undo. The map's save writes them (MapFileType.Navmesh).
/// </summary>
public class NavigationNvmEditAction : ViewportAction
{
    public sealed record FileChange(string Name, NVM Before, NVM After);

    private readonly MapContainer Map;
    private readonly NvmBank Bank;
    private readonly IReadOnlyList<FileChange> Files;
    private readonly MCP McpBefore, McpAfter;
    private readonly MCG McgBefore, McgAfter;
    private readonly Action Changed;

    /// <param name="mcpAfter">The new MCP, or the same object as mcpBefore when it doesn't change (likewise the MCG).</param>
    /// <param name="changed">Called after each apply, e.g. to redraw the overlay.</param>
    public NavigationNvmEditAction(MapContainer map, NvmBank bank, IReadOnlyList<FileChange> files,
        MCP mcpBefore, MCP mcpAfter, MCG mcgBefore, MCG mcgAfter, Action changed)
    {
        Map = map;
        Bank = bank;
        Files = files ?? Array.Empty<FileChange>();
        McpBefore = mcpBefore;
        McpAfter = mcpAfter;
        McgBefore = mcgBefore;
        McgAfter = mcgAfter;
        Changed = changed;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        foreach (var file in Files)
            Bank.SetFile(Map, file.Name, file.After);

        if (!ReferenceEquals(McpBefore, McpAfter))
            Bank.SetMcp(Map, McpAfter);

        if (!ReferenceEquals(McgBefore, McgAfter))
            Bank.SetMcg(Map, McgAfter);

        Changed?.Invoke();
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        foreach (var file in Files)
            Bank.SetFile(Map, file.Name, file.Before);

        if (!ReferenceEquals(McpBefore, McpAfter))
            Bank.SetMcp(Map, McpBefore);

        if (!ReferenceEquals(McgBefore, McgAfter))
            Bank.SetMcg(Map, McgBefore);

        Changed?.Invoke();
        return ActionEvent.NoEvent;
    }
}
