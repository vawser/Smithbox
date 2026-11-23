using System;
using System.Collections.Generic;
using System.Linq;

namespace SoulsFormats
{
    public partial class FXR1
    {
        internal class FxrEnvironment
        {
            public List<long> DEBUG_PointerTable = new List<long>();
            public List<long> DEBUG_PointerTable_Unused = new List<long>();

            public void DEBUG_PointerTableCheck(BinaryReaderEx br)
            {
                //if (DEBUG_PointerTable.Contains(br.Position))
                //{
                //    if (DEBUG_PointerTable_Unused.Contains(br.Position))
                //        DEBUG_PointerTable_Unused.Remove(br.Position);
                //}
                //else
                //{
                //    Console.WriteLine($"WARNING: OFFSET 0x{br.Position:X} NOT IN POINTER TABLE.");
                //}
            }

            public BinaryWriterEx bw;

            public FXR1 fxr;

            public List<FXNode> XmlFXContainer = new List<FXNode>();

            private List<long> PointerOffsets = new List<long>();
            private List<long> FXNodeOffets = new List<long>();

            public Dictionary<long, object> ObjectsByOffset = new Dictionary<long, object>();
            public Dictionary<object, long> OffsetsByObject = new Dictionary<object, long>();

            private List<object> ThingsToWrite = new List<object>();

            /// <summary>
            /// A pointer slot whose value the object writes itself (ActionData's own offsets): while writing, the value
            /// is relative to the object's unit and is moved with it.
            /// </summary>
            public void RegisterPointerOffset(long offset)
            {
                if (current != null)
                {
                    current.RelativeSlots.Add((int)offset);
                    return;
                }
                if (!PointerOffsets.Contains(offset))
                    PointerOffsets.Add(offset);
            }

            public void AddThingToWrite(object thingToWrite)
            {
                if (!ThingsToWrite.Contains(thingToWrite))
                    ThingsToWrite.Add(thingToWrite);
            }

            public void RegisterOffset(long offset, object thingThere)
            {
                // Writing: an object inside the unit being written (its own start, list entries, inline containers)
                if (current != null)
                {
                    current.Members.Add(((int)offset, thingThere));
                    if (thingThere is FXState)
                        currentMember = current.Members.Count(m => m.Obj is FXState) - 1;
                    return;
                }
                if (!ObjectsByOffset.ContainsKey(offset))
                    ObjectsByOffset.Add(offset, thingThere);

                if (!OffsetsByObject.ContainsKey(thingThere))
                    OffsetsByObject.Add(thingThere, offset);
            }

            public FXNodePointer GetFXNodePointer(BinaryReaderEx br, long offset)
            {
                if (offset == 0)
                    return null;

                if (offset != br.Position)
                    DEBUG_PointerTableCheck(br);

                if (ObjectsByOffset.ContainsKey(offset))
                {
                    if (ObjectsByOffset[offset] is FXNodePointer v)
                        return v;
                    else 
                        throw new InvalidOperationException();
                }
                else
                {
                    var newVal = new FXNodePointer();
                    RegisterOffset(offset, newVal);
                    br.StepIn(offset);
                    newVal.Read(br, this);
                    br.StepOut();
                    
                    return newVal;
                }
            }

            public FXActionData GetFXActionData(BinaryReaderEx br, long offset)
            {
                if (offset == 0)
                    return null;

                if (offset != br.Position)
                    DEBUG_PointerTableCheck(br);

                if (ObjectsByOffset.ContainsKey(offset))
                {
                    if (ObjectsByOffset[offset] is FXActionData v)
                        return v;
                    else
                        throw new InvalidOperationException();
                }
                else
                {
                    br.StepIn(offset);
                    var newVal = FXActionData.Read(br, this);
                    RegisterOffset(offset, newVal);
                    newVal.XID = $"0x{offset:X}";
                    br.StepOut();

                    return newVal;
                }
            }

            public FXRecord GetFXRecord(BinaryReaderEx br, long offset)
            {
                if (offset == 0)
                    return null;
                if (ObjectsByOffset.TryGetValue(offset, out var existing))
                    return existing as FXRecord ?? throw new InvalidOperationException();
                var newVal = new FXRecord();
                RegisterOffset(offset, newVal);
                newVal.XID = $"0x{offset:X}";
                br.StepIn(offset);
                newVal.Read(br);
                br.StepOut();
                return newVal;
            }

            public FXModifier GetFXModifier(BinaryReaderEx br, long offset)
            {
                if (offset == 0)
                    return null;

                if (offset != br.Position)
                    DEBUG_PointerTableCheck(br);

                if (ObjectsByOffset.ContainsKey(offset))
                {
                    if (ObjectsByOffset[offset] is FXModifier v)
                        return v;
                    else
                        throw new InvalidOperationException();
                }
                else
                {
                    br.StepIn(offset);
                    var newVal = FXModifier.GetProperType(br, this);
                    RegisterOffset(offset, newVal);
                    newVal.XID = $"0x{offset:X}";
                    newVal.Read(br, this);
                    br.StepOut();
                    
                    return newVal;
                }
            }

            public FXNode GetFXNode(BinaryReaderEx br, long offset)
            {
                if (offset == 0)
                    return null;

                if (offset != br.Position)
                    DEBUG_PointerTableCheck(br);

                if (ObjectsByOffset.ContainsKey(offset))
                {
                    if (ObjectsByOffset[offset] is FXNode v)
                        return v;
                    else
                        throw new InvalidOperationException();
                }
                else
                {
                    br.StepIn(offset);
                    var newVal = FXNode.GetProperFXNodeType(br, this);
                    RegisterOffset(offset, newVal);
                    newVal.XID = $"0x{offset:X}";
                    XmlFXContainer.Add(newVal);
                    newVal.Read(br, this);
                    br.StepOut();
                    
                    return newVal;
                }
            }

            public FXContainer GetFXContainer(BinaryReaderEx br, long offset)
            {
                if (offset == 0)
                    return null;

                if (offset != br.Position)
                    DEBUG_PointerTableCheck(br);

                if (ObjectsByOffset.ContainsKey(offset))
                {
                    if (ObjectsByOffset[offset] is FXContainer v)
                        return v;
                    else
                        throw new InvalidOperationException();
                }
                else
                {
                    var newVal = new FXContainer();
                    RegisterOffset(offset, newVal);
                    br.StepIn(offset);
                    newVal.Read(br, this);
                    br.StepOut();
                    
                    return newVal;
                }
            }

            public FXState GetFXState(BinaryReaderEx br, long offset)
            {
                if (offset == 0)
                    return null;

                if (offset != br.Position)
                    DEBUG_PointerTableCheck(br);

                if (ObjectsByOffset.ContainsKey(offset))
                {
                    if (ObjectsByOffset[offset] is FXState v)
                        return v;
                    else
                        throw new InvalidOperationException();
                }
                else
                {
                    var newVal = new FXState();
                    RegisterOffset(offset, newVal);
                    newVal.XID = $"0x{offset:X}";
                    br.StepIn(offset);
                    newVal.Read(br, this);
                    br.StepOut();
                    
                    return newVal;
                }
            }

            public FXTransition GetFXTransition(BinaryReaderEx br, long offset)
            {
                if (offset == 0)
                    return null;

                if (offset != br.Position)
                    DEBUG_PointerTableCheck(br);

                if (ObjectsByOffset.ContainsKey(offset))
                {
                    if (ObjectsByOffset[offset] is FXTransition v)
                        return v;
                    else
                        throw new InvalidOperationException();
                }
                else
                {
                    var newVal = new FXTransition();
                    RegisterOffset(offset, newVal);
                    newVal.XID = $"0x{offset:X}";
                    br.StepIn(offset);
                    newVal.Read(br, this);
                    br.StepOut();

                    return newVal;
                }
            }

            public FXAction GetFXAction(BinaryReaderEx br, long offset)
            {
                if (offset == 0)
                    return null;

                if (offset != br.Position)
                    DEBUG_PointerTableCheck(br);

                if (ObjectsByOffset.ContainsKey(offset))
                {
                    if (ObjectsByOffset[offset] is FXAction v)
                        return v;
                    else
                        throw new InvalidOperationException();
                }
                else
                {   
                    var newVal = new FXAction();
                    RegisterOffset(offset, newVal);
                    newVal.XID = $"0x{offset:X}";
                    br.StepIn(offset);
                    newVal.Read(br, this);
                    br.StepOut();
                    return newVal;
                }
            }

            // ================================================================ writing (FFX Older Games Plan Â§17, F4)
            //
            // The original tool's layout, found from every vanilla DS1 effect (FfxSurvey --fxr1diff / --fxr1order):
            // objects are grouped by kind into sections, in a fixed order, and within a section they come in the order
            // a depth-first walk from the root reaches them (pointer fields in order; a state's actions before its
            // transitions; a transition's target state is not followed). Here each object (a "unit": a node, a
            // container, an ActionData, a modifier, or a list) is written once into its own buffer, recording the
            // pointers it holds and the objects inside it; the buffers are then laid out by section and relocated.

            internal sealed class Unit
            {
                public object Obj;
                public Section Section;
                /// <summary>FXR v2: which root (133) the unit was written under; the packed tail goes root by root.</summary>
                public int RootIndex;
                public int Order;
                public byte[] Bytes;
                public long Base;
                /// <summary>Pointer slots to fill with another object's offset.</summary>
                public List<(int At, object Target)> Fixups = new List<(int, object)>();
                /// <summary>Pointer slots holding an offset inside this unit (ActionData's own), moved with it.</summary>
                public List<int> RelativeSlots = new List<int>();
                public List<int> NodeOffsets = new List<int>();
                public List<int> GarbageSlots = new List<int>();
                /// <summary>Objects inside this unit at their offsets (its own start, list entries, inline containers).</summary>
                public List<(int At, object Obj)> Members = new List<(int, object)>();
                /// <summary>Units this one points to, in field order, with the list entry (state) they came from.</summary>
                public List<(object Target, int Member)> Children = new List<(object, int)>();
            }

            /// <summary>Section order, as vanilla lays them out.</summary>
            internal enum Section
            {
                Root, Container, State, Transition, Action, Data, Node, NodePointer, Late, ActionData,
                /// <summary>FXR v2 container records (placement learned from Bloodborne, FFX plan §19).</summary>
                Record,
                /// <summary>Nodes a transition's condition starts at (DS1: at the end of the nodes; DS1R: before the node pointers).</summary>
                Evaluator,
                /// <summary>FXR v2: the first root's node 141's node list (the header points at it), after the nodes.</summary>
                V2RootList,
                /// <summary>FXR v2: the packed nodes' data lists (the first count2 nodes of a container: FFX plan §19).</summary>
                PackedData,
                /// <summary>FXR v2: the packed nodes, block by block.</summary>
                PackedNode,
                /// <summary>FXR v2: the roots' node 141, the last object.</summary>
                V2Root141,
                /// <summary>FXR v2: the packed colour nodes (15, 83), after the packed vector lists.</summary>
                PackedColor,
                /// <summary>FXR v2: the packed nodes' 16-byte vector lists (colour values), after 141.</summary>
                PackedLate,
            }

            /// <summary>Where a section goes: DS1R puts the node pointer lists (after the evaluators) before the other nodes.</summary>
            private int Rank(Section s)
            {
                Section[] narrow = { Section.Root, Section.Container, Section.State, Section.Transition, Section.Action, Section.Data, Section.Node, Section.Evaluator, Section.NodePointer, Section.Late, Section.ActionData, Section.Record };
                Section[] wide = { Section.Root, Section.Container, Section.State, Section.Transition, Section.Action, Section.Data, Section.Evaluator, Section.NodePointer, Section.Node, Section.Late, Section.ActionData, Section.Record };
                Section[] v2 = { Section.Root, Section.Container, Section.State, Section.Transition, Section.Action, Section.Data, Section.Evaluator, Section.NodePointer, Section.Node, Section.Late, Section.Record, Section.V2RootList, Section.PackedData, Section.PackedNode, Section.V2Root141, Section.PackedLate, Section.PackedColor };
                return Array.IndexOf(fxr.Version == 2 ? v2 : fxr.Wide ? wide : narrow, s);
            }

            // FXR v2 layout state: the packed nodes (the first count2 nodes of every container whose count2 is set), the
            // roots' 141 nodes (written after everything else), and whether the unit being written belongs to a packed node
            private Dictionary<FXContainer, int> packedCounts;
            private readonly Dictionary<object, FXContainer> listOwners = new Dictionary<object, FXContainer>(ReferenceEqualityComparer.Instance);

            /// <summary>FXR v2: the node pointer list a container just made, so its entries can be told packed or not.</summary>
            internal void NoteContainerList(FXContainer c, object list) => listOwners[list] = c;
            private HashSet<object> v2Roots141;
            private readonly List<object> deferredV2 = new List<object>();
            private bool writingPacked;
            private int rootIndex = -1;

            /// <summary>FXR v2: sections laid out root by root (each root's packed nodes, then its 141); the roots' 141 lists and packed data come together before them.</summary>
            private static bool PerRoot(Section s) => s is Section.PackedNode or Section.V2Root141;

            /// <summary>Sort key of a unit's section: v2 per-root sections come after the shared ones, root by root.</summary>
            private int SectionKey(Unit u)
            {
                int rank = Rank(u.Section);
                if (fxr.Version != 2)
                    return rank;
                int firstTail = Rank(Section.PackedNode);
                if (rank < firstTail)
                    return rank;
                if (PerRoot(u.Section))
                    return 100 + Math.Max(0, u.RootIndex) * 100 + rank;
                return 1000000 + rank;
            }
            private bool writingV2Tree;

            /// <summary>
            /// FXR v2: the action types whose values are packed (laid out in the tail, FFX plan §19): DS1's ActionData
            /// types plus 10000–10600, 20, 45, 60, 64 and 65, from every vanilla Bloodborne action call (FfxSurvey --bbwalk).
            /// </summary>
            internal static readonly HashSet<int> PackedActionTypes = new HashSet<int>
            {
                20, 27, 28, 29, 30, 31, 32, 40, 43, 45, 55, 59, 60, 61, 64, 65, 66, 70, 71, 84, 105, 107, 108, 117,
                10000, 10003, 10008, 10200, 10300, 10301, 10600,
            };

            private void PrepareV2()
            {
                packedCounts = new Dictionary<FXContainer, int>(ReferenceEqualityComparer.Instance);
                v2Roots141 = new HashSet<object>(ReferenceEqualityComparer.Instance);
                var seen = new HashSet<object>(ReferenceEqualityComparer.Instance);
                void Container(FXContainer c, int actionType = -1)
                {
                    if (c == null || !seen.Add(c)) return;
                    // A packed action type's container: its first count2 nodes (all when count2 is the count) are packed, if
                    // they are first written from it (a node shared with an unpacked place written first stays unpacked)
                    if (PackedActionTypes.Contains(actionType))
                        packedCounts[c] = c.V2PackedRead >= 0 ? c.V2PackedRead : c.V2Count2 >= 0 ? c.V2Count2 : c.FXNodes.Count;
                    else if (c.V2PackedRead > 0)
                        packedCounts[c] = c.V2PackedRead;
                    foreach (var n in c.FXNodes)
                        Node(n);
                }
                void Node(FXNode n)
                {
                    if (n == null || !seen.Add(n)) return;
                    switch (n)
                    {
                        case FXNode.EffectCreateNode133 r:
                            Container(r.Container1);
                            if (r.V2Node != null) v2Roots141.Add(r.V2Node);
                            Node(r.V2Node);
                            foreach (var s in r.States)
                                foreach (var a in s.Actions)
                                    Container(a.Container, a.ActionType);
                            break;
                        case FXNode.EffectCreateNode134 m: foreach (var x in m.Nodes) Node(x); break;
                        case FXNode.ContainerNode cn: Container(cn.Container); break;
                        case FXNode.ActionCallNode ac: Container(ac.Container, ac.ActionType); break;
                        case FXNode.EffectCallNode ec: Container(ec.Container); break;
                    }
                }
                Node(fxr.RootFXNode);
            }

            private bool writingTransitions;

            private readonly Dictionary<object, Unit> units = new Dictionary<object, Unit>(ReferenceEqualityComparer.Instance);
            private readonly List<Unit> unitOrder = new List<Unit>();
            private readonly List<(long At, object Target)> headerFixups = new List<(long, object)>();
            private readonly List<object> roots = new List<object>();
            private readonly List<object> deferredTransitions = new List<object>();
            private Unit current;
            private int currentMember = -1;

            private int GetListObjectCount(object obj)
            {
                switch (obj)
                {
                    case List<int> intList: return intList.Count;
                    case List<float> floatList: return floatList.Count;
                    case List<FXState> fxStateList: return fxStateList.Count;
                    case List<FXTransition> fxTransitionList: return fxTransitionList.Count;
                    case List<FXAction> fxActionList: return fxActionList.Count;
                    case List<FXNode> nodeList: return nodeList.Count;
                    // An empty node pointer list is a null pointer in vanilla (the reference wrote a pointer to nothing)
                    case List<FXNodePointer> pointerList: return pointerList.Count;
                    default: return -1;
                }
            }

            /// <summary>Objects written as units of their own; others (states, transitions, actions, node pointers) live in their list's unit.</summary>
            private static bool IsUnit(object o) =>
                o is FXNode || o is FXContainer || o is FXActionData || o is FXModifier || o is FXRecord || o is System.Collections.IList;

            internal static Section SectionOf(object o, bool root) => o switch
            {
                // A multi-root effect: the 134 root and every 133 under it sit together at the start
                FXNode when root => Section.Root,
                FXNode.EffectCreateNode133 => Section.Root,
                FXNode => Section.Node,
                List<FXNode> => Section.NodePointer,
                FXContainer => Section.Container,
                List<FXState> => Section.State,
                List<FXTransition> => Section.Transition,
                List<FXAction> => Section.Action,
                List<FXNodePointer> => Section.NodePointer,
                FXActionData => Section.ActionData,
                // FXR v2: 48-byte records (kind 1) have a 16-aligned section; 8-byte ones (kind 0) sit with the data lists
                FXRecord r => r.Kind == 0 ? Section.Data : Section.Late,
                FXModifier1_MoveEffect => Section.Late,
                VectorList => Section.Late,
                FXModifier => Section.Data,
                _ => Section.Data,
            };

            /// <summary>Alignment of a unit's start; the first unit of a section may need more (see <see cref="SectionAlign"/>).</summary>
            internal int UnitAlign(Unit u) => u.Section switch
            {
                Section.Late or Section.ActionData or Section.PackedLate or Section.Record => 16,
                // DS1R's 64-bit pointers: nodes and node pointer lists start on 8 bytes
                _ when fxr.Wide && (u.Obj is FXNode || u.Obj is List<FXNodePointer> || u.Obj is List<FXNode>) => 8,
                _ => 4,
            };

            internal static int SectionAlign(Section s) => s switch
            {
                Section.Late or Section.ActionData or Section.PackedLate or Section.Record or Section.V2RootList => 16,
                _ => 4,
            };

            public void RegisterPointer(object pointToObject,
                bool useExistingPointerOnly = false,
                bool assertNotNull = false)
            {
                if (pointToObject == null || GetListObjectCount(pointToObject) == 0)
                {
                    if (assertNotNull)
                        throw new InvalidOperationException("Assertion that pointer is not null failed.");
                    WriteFXR1Varint(bw, 0);
                    return;
                }

                if (current == null)
                {
                    // The header's pointer to the root node
                    headerFixups.Add((bw.Position, pointToObject));
                    roots.Add(pointToObject);
                }
                else
                {
                    current.Fixups.Add(((int)bw.Position, pointToObject));
                    // ActionData's pointer back to its container, and a transition's target state, point at objects
                    // reached another way
                    if (!useExistingPointerOnly && IsUnit(pointToObject) && !units.ContainsKey(pointToObject))
                        current.Children.Add((pointToObject, currentMember));
                }
                bw.WriteUInt32(0xEEEEEEEE);
                WriteFXR1Garbage(bw);
            }

            /// <summary>Writes one unit into its own buffer, then (depth first) the units it points to.</summary>
            private void WriteUnit(object obj, bool isRoot, object parent = null)
            {
                var section = SectionOf(obj, isRoot);
                // DS1R: a transition's condition (every node first reached through the deferred transitions) has its own
                // section; DS1 leaves those nodes at the end of the node section, where the deferral puts them
                if (fxr.Wide && writingTransitions && section == Section.Node)
                    section = Section.Evaluator;

                // FXR v2: packed nodes and everything they hold go to the packed tail; a root's 141 and its own list
                // to their sections
                bool wasPacked = writingPacked;
                if (fxr.Version == 2)
                {
                    if (obj is FXNode && parent is List<FXNodePointer> list && listOwners.TryGetValue(list, out var owner)
                        && packedCounts.TryGetValue(owner, out int packed) && list.FindIndex(p => ReferenceEquals(p.Node, obj)) is int idx && idx >= 0 && idx < packed)
                        writingPacked = true;
                    if (writingPacked)
                        section = obj switch
                        {
                            // A colour constant counts as vector data
                            FXNode.ColorNode15 or FXNode.Color2Node83 or FXNode.ColorWrapNode84 => Section.PackedColor,
                            FXNode => Section.PackedNode,
                            VectorList => Section.PackedLate,
                            System.Collections.IList when obj is not List<FXNodePointer> => Section.PackedData,
                            _ => section,
                        };
                    else if (v2Roots141.Contains(obj))
                        section = Section.V2Root141;
                    else if (obj is List<FXNodePointer> && parent != null && v2Roots141.Contains(parent))
                        section = Section.V2RootList;
                }
                if (fxr.Version == 2 && obj is FXNode.EffectCreateNode133)
                    rootIndex++;
                var u = new Unit { Obj = obj, Section = section, RootIndex = rootIndex };
                units[obj] = u;

                var (prevUnit, prevWriter, prevMember) = (current, bw, currentMember);
                var w = new BinaryWriterEx(fxr.BigEndian) { VarintLong = fxr.Wide };
                bw = w;
                current = u;
                currentMember = -1;
                DoDataWrite(obj);
                u.Bytes = w.FinishBytes();
                (current, bw, currentMember) = (prevUnit, prevWriter, prevMember);

                u.Order = unitOrder.Count;
                unitOrder.Add(u);

                // Transition lists wait until the whole walk is done (every root's actions come first, then every
                // transition, in state order: FFX plan §17)
                foreach (var (target, _) in u.Children.ToList())
                {
                    if (units.ContainsKey(target))
                        continue;
                    if (target is List<FXTransition>)
                        deferredTransitions.Add(target);
                    // FXR v2: a root's 141 subtree comes after everything else
                    else if (fxr.Version == 2 && v2Roots141.Contains(target))
                        deferredV2.Add(target);
                    else
                        WriteUnit(target, false, obj);
                }
                writingPacked = wasPacked;

                // FXR v2: a root's 141 subtree follows that root (multi-root files go root by root)
                if (fxr.Version == 2 && obj is FXNode.EffectCreateNode133 root && root.V2Node != null && !units.ContainsKey(root.V2Node))
                    WriteUnit(root.V2Node, false, obj);
            }

            private void DoDataWrite(object data)
            {
                switch (data)
                {
                    case FXNodePointer asEffectFXNode: asEffectFXNode.Write(bw, this); break;
                    case FXActionData asActionData: asActionData.Write(bw, this); break;
                    case FXModifier asModifier: asModifier.Write(bw, this); break;
                    case FXRecord asRecord: asRecord.Write(bw, this); break;
                    case FXContainer asEffect: asEffect.Write(bw, this); break;
                    case FXAction asFXAction: asFXAction.Write(bw, this); break;
                    case FXTransition asTransition: asTransition.Write(bw, this); break;
                    case FXState asState: asState.Write(bw, this); break;
                    case FXNode asFXNode: asFXNode.Write(bw, this); break;
                    case List<FXNode> asFXContainer:
                        foreach (var v in asFXContainer)
                            RegisterPointer(v);
                        break;
                    case List<FXTransition> asTransitionList:
                        foreach (var v in asTransitionList)
                        {
                            RegisterOffset(bw.Position, v);
                            v.Write(bw, this);
                        }
                        break;
                    case List<FXAction> asFXActionList:
                        foreach (var v in asFXActionList)
                        {
                            RegisterOffset(bw.Position, v);
                            v.Write(bw, this);
                        }
                        break;
                    case List<FXState> asStateList:
                        foreach (var v in asStateList)
                        {
                            RegisterOffset(bw.Position, v);
                            v.Write(bw, this);
                        }
                        break;
                    case List<FXNodePointer> asEffectFXContainer:
                        foreach (var v in asEffectFXContainer)
                        {
                            RegisterOffset(bw.Position, v);
                            v.Write(bw, this);
                        }
                        break;
                    case List<float> asFloatList:
                        foreach (var v in asFloatList)
                            bw.WriteSingle(v);
                        break;
                    case List<int> asIntList:
                        foreach (var v in asIntList)
                            bw.WriteInt32(v);
                        break;
                    default: throw new NotImplementedException($"FxrEnvironment write not implemented for '{data.GetType()}'");
                }
            }

            /// <summary>Writes every unit, laid out by section, and fills in every pointer. Called with bw at the data start.</summary>
            public void FinishRecursiveWrite()
            {
                var main = bw;
                if (fxr.Version == 2)
                    PrepareV2();
                foreach (var root in roots.ToList())
                    if (!units.ContainsKey(root))
                        WriteUnit(root, true);
                // The transitions put off by the walk, in the order met (a transition's evaluator can hold more)
                writingTransitions = true;
                for (int i = 0; i < deferredTransitions.Count; i++)
                    if (!units.ContainsKey(deferredTransitions[i]))
                        WriteUnit(deferredTransitions[i], false);
                writingTransitions = false;
                // FXR v2: each root's 141 subtree, last
                for (int i = 0; i < deferredV2.Count; i++)
                    if (!units.ContainsKey(deferredV2[i]))
                        WriteUnit(deferredV2[i], false);

                Section? previous = null;
                foreach (var u in unitOrder.OrderBy(SectionKey).ThenBy(u => u.Order))
                {
                    if (previous != u.Section)
                        main.Pad(SectionAlign(u.Section));
                    previous = u.Section;
                    main.Pad(UnitAlign(u));

                    u.Base = main.Position;
                    main.WriteBytes(u.Bytes);
                    OffsetsByObject[u.Obj] = u.Base;
                    foreach (var (at, obj) in u.Members)
                        if (!OffsetsByObject.ContainsKey(obj) || ReferenceEquals(obj, u.Obj))
                            OffsetsByObject[obj] = u.Base + at;
                    foreach (var at in u.NodeOffsets)
                        FXNodeOffets.Add(u.Base + at);
                }
                long end = main.Position;

                // Pointers: offsets inside a unit move with it; pointers to other objects get their offsets
                foreach (var u in unitOrder)
                {
                    foreach (var at in u.RelativeSlots)
                    {
                        long slot = u.Base + at;
                        main.StepIn(slot);
                        int value = BitConverter.ToInt32(u.Bytes, at);
                        if (fxr.BigEndian) value = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(value);
                        main.WriteInt32((int)(value + u.Base));
                        main.StepOut();
                        PointerOffsets.Add(slot);
                    }
                    foreach (var (at, target) in u.Fixups)
                    {
                        long slot = u.Base + at;
                        main.StepIn(slot);
                        main.WriteInt32((int)OffsetsByObject[target]);
                        main.StepOut();
                        PointerOffsets.Add(slot);
                    }
                }
                // The header's pointer to the root is in the pointer table too
                foreach (var (at, target) in headerFixups)
                {
                    main.StepIn(at);
                    main.WriteInt32((int)OffsetsByObject[target]);
                    main.StepOut();
                    PointerOffsets.Add(at);
                }
                main.Position = end;
            }

            // DS1R garbage words: where each was written (unit-relative while a unit is written, else absolute)
            private readonly List<long> garbageSlots = new List<long>();

            internal void RecordGarbage(long position)
            {
                if (current != null)
                    current.GarbageSlots.Add((int)position);
                else
                    garbageSlots.Add(position);
            }

            /// <summary>Puts each garbage word read at an offset back at that offset (unedited layouts are the vanilla ones).</summary>
            internal void ApplyGarbage()
            {
                long end = bw.Position;
                IEnumerable<long> all = garbageSlots.Concat(unitOrder.SelectMany(u => u.GarbageSlots.Select(at => u.Base + at)));
                foreach (var slot in all)
                {
                    if (!fxr.Garbage.TryGetValue(slot, out uint value))
                        continue;
                    bw.StepIn(slot);
                    bw.WriteUInt32(value);
                    bw.StepOut();
                }
                bw.Position = end;
            }

            public void RegisterFXNodeOffsetHere()
            {
                if (current != null)
                {
                    current.NodeOffsets.Add((int)bw.Position);
                    return;
                }
                if (!FXNodeOffets.Contains(bw.Position))
                    FXNodeOffets.Add(bw.Position);
            }

            internal void WriteFXNodeTable(string tableCountFillLabel)
            {
                foreach (var location in FXNodeOffets.Distinct().OrderBy(x => x))
                {
                    WriteFXR1Varint(bw, (int)location);
                }
                bw.FillInt32(tableCountFillLabel, FXNodeOffets.Distinct().Count());
            }

            internal void WritePointerTable(string tableCountFillLabel)
            {
                foreach (var offset in PointerOffsets.Distinct().OrderBy(x => x))
                {
                    WriteFXR1Varint(bw, (int)offset);
                }
                bw.FillInt32(tableCountFillLabel, PointerOffsets.Distinct().Count());
            }

            internal void ReadPointerTable(BinaryReaderEx br, int count)
            {
                DEBUG_PointerTable = new List<long>(count);
                DEBUG_PointerTable_Unused = new List<long>(count);
                for (int i = 0; i < count; i++)
                {
                    int next = ReadFXR1Varint(br);
                    DEBUG_PointerTable.Add(next);
                    DEBUG_PointerTable_Unused.Add(next);
                }
            }
        }
    }
}
