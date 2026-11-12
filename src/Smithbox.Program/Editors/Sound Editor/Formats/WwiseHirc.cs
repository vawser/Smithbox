using System;
using System.Collections.Generic;

namespace StudioCore.Editors.SoundEditor;

/// <summary>
/// The parts of a Wwise bank's HIRC object graph the Sound Editor needs to go from an event to the media it can play:
/// Event → Actions → target node → descendants (found by inverting each node's DirectParentID) → Sound / MusicTrack
/// sources. Full container bodies (playlists, switch tables, RTPCs) are not parsed: each node is read only up to its
/// DirectParentID. Layouts follow wwiser's v135 (ER/NR/AC6) / v150 (DB_NT) readers and are checked against every game
/// bank by SoundSurvey hirc (every action and parent ID must resolve).
/// <para>Kept free of Smithbox dependencies: Claude/Tools/SoundSurvey links this file.</para>
/// </summary>
public sealed class WwiseHirc
{
    public const byte TypeSound = 2, TypeAction = 3, TypeEvent = 4, TypeRanSeq = 5, TypeSwitch = 6, TypeActorMixer = 7,
        TypeLayer = 9, TypeMusicSegment = 10, TypeMusicTrack = 11, TypeMusicSwitch = 12, TypeMusicRanSeq = 13;

    public sealed class Node
    {
        public uint Id;
        public byte Type;
        public uint ParentId;
        public bool ParentRead;

        /// <summary>Where NodeBaseParams starts and the object ends (for layout checks).</summary>
        public int BaseParamsPos, End;

        /// <summary>Sound: its one source. MusicTrack: each clip's source.</summary>
        public List<uint> SourceIds = new();
        public List<Node> Children = new();
    }

    public sealed class Action
    {
        public uint Id;
        public ushort ActionType;
        public uint TargetId;

        /// <summary>0x04xx: Play</summary>
        public bool IsPlay => (ActionType >> 8) == 0x04;

        /// <summary>0x01xx: Stop</summary>
        public bool IsStop => (ActionType >> 8) == 0x01;
    }

    public sealed class Event
    {
        public uint Id;
        public List<uint> ActionIds = new();
    }

    public uint Version;
    public Dictionary<uint, Node> Nodes = new();
    public Dictionary<uint, Action> Actions = new();
    public List<Event> Events = new();

    /// <summary>Objects whose layout read failed or ran past their size (a layout check for the survey).</summary>
    public int ParseErrors;

    public static WwiseHirc Read(byte[] d, int hircBody, uint version)
    {
        var h = new WwiseHirc { Version = version };
        int count = BitConverter.ToInt32(d, hircBody);
        int pos = hircBody + 4;
        for (int i = 0; i < count; i++)
        {
            byte type = d[pos];
            int size = BitConverter.ToInt32(d, pos + 1);
            int body = pos + 5, end = body + size;
            try { h.ReadObject(d, type, body, end); }
            catch { h.ParseErrors++; }
            pos = end;
        }

        // children by inverting parents
        foreach (var n in h.Nodes.Values)
            if (n.ParentRead && n.ParentId != 0 && h.Nodes.TryGetValue(n.ParentId, out var p))
                p.Children.Add(n);

        return h;
    }

    private void ReadObject(byte[] d, byte type, int p, int end)
    {
        uint id = BitConverter.ToUInt32(d, p);
        switch (type)
        {
            case TypeEvent:
                {
                    var e = new Event { Id = id };
                    int q = p + 4;
                    uint n = Version <= 122 ? ReadU32(d, ref q) : ReadVar(d, ref q);
                    for (uint k = 0; k < n; k++) e.ActionIds.Add(ReadU32(d, ref q));
                    if (q > end) throw new FormatException("event");
                    Events.Add(e);
                    break;
                }
            case TypeAction:
                Actions[id] = new Action { Id = id, ActionType = BitConverter.ToUInt16(d, p + 4), TargetId = BitConverter.ToUInt32(d, p + 6) };
                break;
            case TypeSound:
                {
                    var node = new Node { Id = id, Type = type };
                    int q = p + 4;
                    node.SourceIds.Add(ReadSource(d, ref q));
                    ReadParent(d, q, end, node);
                    Nodes[id] = node;
                    break;
                }
            case TypeRanSeq:
            case TypeSwitch:
            case TypeActorMixer:
            case TypeLayer:
                {
                    var node = new Node { Id = id, Type = type };
                    ReadParent(d, p + 4, end, node);
                    Nodes[id] = node;
                    break;
                }
            case TypeMusicSegment:
            case TypeMusicSwitch:
            case TypeMusicRanSeq:
                {
                    var node = new Node { Id = id, Type = type };
                    ReadParent(d, p + 4 + 1, end, node); // MusicNodeParams: uFlags u8, then NodeBaseParams
                    Nodes[id] = node;
                    break;
                }
            case TypeMusicTrack:
                {
                    var node = new Node { Id = id, Type = type };
                    int q = p + 4;
                    q += 1; // uFlags
                    uint numSources = ReadU32(d, ref q);
                    for (uint k = 0; k < numSources; k++) node.SourceIds.Add(ReadSource(d, ref q));
                    uint numItems = ReadU32(d, ref q);
                    q += (int)numItems * PlaylistItemSize;
                    if (numItems > 0) q += 4; // numSubTrack
                    uint numAuto = ReadU32(d, ref q);
                    for (uint k = 0; k < numAuto; k++)
                    {
                        q += 8; // uClipIndex, eAutoType
                        uint points = ReadU32(d, ref q);
                        q += (int)points * 12;
                    }
                    ReadParent(d, q, end, node);
                    Nodes[id] = node;
                    break;
                }
        }
    }

    // AkTrackSrcInfo: trackID, sourceID, eventID (v>=?), fPlayAt, fBeginTrimOffset, fEndTrimOffset, fSrcDuration (f64)
    private int PlaylistItemSize => 4 + 4 + 4 + 8 * 4;

    /// <summary>AkBankSourceData: plugin ID, stream type, source ID, in-memory size, source bits, [plugin params]. Returns the source ID.</summary>
    private static uint ReadSource(byte[] d, ref int q)
    {
        uint plugin = ReadU32(d, ref q);
        q += 1; // StreamType
        uint source = ReadU32(d, ref q);
        q += 4; // uInMemoryMediaSize
        q += 1; // uSourceBits
        if ((plugin & 0x0F) == 2) // source plug-ins (sine, silence, tone) carry a parameter block
        {
            uint size = ReadU32(d, ref q);
            q += (int)size;
        }
        return source;
    }

    /// <summary>NodeBaseParams up to DirectParentID: FX params, [attachment override], OverrideBusId, DirectParentID.</summary>
    private void ReadParent(byte[] d, int q, int end, Node node)
    {
        node.BaseParamsPos = q;
        node.End = end;
        // Layouts found by SoundSurvey hirc's offset probe (parent = a node of the same bank):
        //   v135: bIsOverrideParentFX, uNumFx, [bitsFxBypass, uNumFx x 7], bOverrideAttachmentParams, OverrideBusId, DirectParentID
        //         (no FX: parent at +7; 1 FX: +15; 2 FX: +22)
        //   v150: bIsOverrideParentFX, uNumFx, [uNumFx x 7] (no bypass byte), bIsOverrideParentMetadata, uNumFxMetadata,
        //         [uNumFxMetadata x 6], OverrideBusId, DirectParentID (no FX: +8; 1 FX: +15)
        q += 1; // bIsOverrideParentFX
        int numFx = d[q++];
        if (Version <= 136)
        {
            if (numFx > 0) q += 1 + numFx * 7; // bitsFxBypass, then per FX: index u8, fxID u32, bIsShareSet u8, bIsRendered u8
            q += 1; // bOverrideAttachmentParams
        }
        else
        {
            q += numFx * 7;
            q += 1; // bIsOverrideParentMetadata
            int numMeta = d[q++];
            q += numMeta * 6; // index u8, fxID u32, bIsShareSet u8
        }
        q += 4; // OverrideBusId
        if (q + 4 > end) throw new FormatException("node");
        node.ParentId = BitConverter.ToUInt32(d, q);
        node.ParentRead = true;
    }

    /// <summary>The media a node can play: its own sources and all its descendants'.</summary>
    public void CollectMedia(Node n, HashSet<uint> into, int depth = 0)
    {
        if (depth > 64) return;
        foreach (var s in n.SourceIds) into.Add(s);
        foreach (var c in n.Children) CollectMedia(c, into, depth + 1);
    }

    /// <summary>The media an event's Play actions can reach (targets in this bank only).</summary>
    public HashSet<uint> EventMedia(Event e)
    {
        var media = new HashSet<uint>();
        foreach (var aid in e.ActionIds)
            if (Actions.TryGetValue(aid, out var a) && a.IsPlay && Nodes.TryGetValue(a.TargetId, out var target))
                CollectMedia(target, media);
        return media;
    }

    private static uint ReadU32(byte[] d, ref int q) { uint v = BitConverter.ToUInt32(d, q); q += 4; return v; }

    /// <summary>AkVarUInt: 7 bits per byte, high bit = more.</summary>
    private static uint ReadVar(byte[] d, ref int q)
    {
        uint v = 0;
        for (int i = 0; i < 5; i++)
        {
            byte b = d[q++];
            v = (v << 7) | (uint)(b & 0x7F);
            if ((b & 0x80) == 0) break;
        }
        return v;
    }
}
