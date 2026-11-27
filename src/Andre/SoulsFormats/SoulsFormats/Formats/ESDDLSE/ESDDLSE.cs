using System;
using System.Collections.Generic;
using System.Linq;

namespace SoulsFormats
{
    /// <summary>
    /// A Demon's Souls ESD: an EzState project written by the DL serializer ("DLSE" stream 2.3, big-endian).
    /// Extension: .esd (loose or in talk/chr/menu esdbnd binders). Other games' ESDs are <see cref="ESD"/>.
    /// <para>The stream is a tagged object graph. An object is a u16 class (1-based index into the file's class table)
    /// and a u32 class version, then its fields. A pointer is a u16 class and a u32 id (0 = null): the first time an
    /// id appears its object follows inline, later it's a reference. Ids count up from 1 in definition order. A vector
    /// is the DLVector class, a u32 count and the elements; a buffer is the buffer class, a u32 length and the bytes.</para>
    /// <para>Transition and host event targets aren't stored in those objects: after the maps comes an 8-byte value,
    /// then one target pointer per transition and host event, in id order.</para>
    /// <para>The older stream versions 2.0 / 2.1 (loose files the game doesn't load) aren't supported.</para>
    /// </summary>
    public class ESDDLSE : SoulsFile<ESDDLSE>
    {
        private const string ClassProject = "EzStateProject";
        private const string ClassVector = "DLVector";
        private const string ClassMap = "EzStateMap";
        private const string ClassState = "EzStateMapState";
        private const string ClassTransition = "EzStateTransition";
        private const string ClassBuffer = "buffer";
        private const string ClassHostEvent = "EzStateTransitionHostEvent";
        private const string ClassEvent = "EzStateExternalEventT<ES_EVENT_PARAM_NUM_6>";

        private static readonly Dictionary<string, int> Versions = new()
        {
            [ClassProject] = 2, [ClassMap] = 2, [ClassState] = 2, [ClassTransition] = 3, [ClassHostEvent] = 1, [ClassEvent] = 2,
        };

        /// <summary>
        /// The most arguments an event can have (the event class is a template on 6 parameters).
        /// </summary>
        public const int MaxEventArgs = 6;

        /// <summary>
        /// Unknown header value, roughly 1.27 times the file size (perhaps an in-memory size). Written back as read.
        /// </summary>
        public uint HeaderA { get; set; }

        /// <summary>
        /// Unknown 8-byte value after the maps. Not a hash of the content: copies of a file that are otherwise
        /// byte-identical carry different values. Written back as read.
        /// </summary>
        public byte[] TailValue { get; set; } = new byte[8];

        /// <summary>
        /// The state machines.
        /// </summary>
        public List<Map> Maps { get; set; } = new();

        /// <summary>
        /// Something a transition or host event can lead to: a state, or a transition (a subcondition).
        /// </summary>
        public interface ITarget { }

        /// <summary>
        /// An item of a state's entry / exit list or a transition's pass list.
        /// </summary>
        public abstract class Event { }

        /// <summary>
        /// A command call: an id and up to six argument expressions (bytecode).
        /// </summary>
        public class ExternalEvent : Event
        {
            /// <summary>The command id (no bank in DES).</summary>
            public int CommandId { get; set; }

            /// <summary>The argument expressions, at most <see cref="MaxEventArgs"/>.</summary>
            public List<byte[]> Args { get; set; } = new();
        }

        /// <summary>
        /// A subcondition link. A transition whose pass list holds these has subconditions: its own target is the
        /// first, each host event's target the next, and the last host event has a null target.
        /// </summary>
        public class HostEvent : Event
        {
            /// <summary>The next subcondition, or null at the end of the list.</summary>
            public ITarget Target { get; set; }
        }

        /// <summary>
        /// A state machine.
        /// </summary>
        public class Map
        {
            /// <summary>The machine id.</summary>
            public int Id { get; set; }

            /// <summary>The initial state (in vanilla files always also the first of <see cref="States"/>).</summary>
            public MapState Initial { get; set; }

            /// <summary>The states.</summary>
            public List<MapState> States { get; set; } = new();

            /// <summary>Every transition of the machine: the ones its states list, then the subconditions.</summary>
            public List<Transition> Transitions { get; set; } = new();
        }

        /// <summary>
        /// A state.
        /// </summary>
        public class MapState : ITarget
        {
            /// <summary>The state id, unique in its machine.</summary>
            public int Id { get; set; }

            /// <summary>The transitions checked while in this state, in order.</summary>
            public List<Transition> Transitions { get; set; } = new();

            /// <summary>Commands run on entering the state.</summary>
            public List<Event> EntryEvents { get; set; } = new();

            /// <summary>Commands run on leaving the state.</summary>
            public List<Event> ExitEvents { get; set; } = new();
        }

        /// <summary>
        /// A transition: a condition, commands run when it passes, and where it leads.
        /// </summary>
        public class Transition : ITarget
        {
            /// <summary>The condition expression (bytecode).</summary>
            public byte[] Evaluator { get; set; } = Array.Empty<byte>();

            /// <summary>Commands run when the condition passes, or the host events of a subcondition list.</summary>
            public List<Event> PassEvents { get; set; } = new();

            /// <summary>The state to enter, the first subcondition, or null.</summary>
            public ITarget Target { get; set; }
        }

        /// <summary>
        /// Checks whether the data appears to be a file of this format (stream 2.3).
        /// </summary>
        protected override bool Is(BinaryReaderEx br)
        {
            if (br.Length < 8)
                return false;
            return br.GetASCII(0, 4) == "DLSE" && br.GetBytes(4, 4) is [0, 2, 0, 3];
        }

        #region Read

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = true;
            br.AssertASCII("DLSE");
            br.AssertUInt16(2);
            ushort minor = br.ReadUInt16();
            if (minor != 3)
                throw new NotSupportedException($"DLSE ESD stream version 2.{minor} isn't supported (only 2.3).");
            HeaderA = br.ReadUInt32();
            br.ReadUInt32(); // transitions + host events, recomputed on write
            br.ReadUInt32(); // id count, recomputed on write
            int classCount = br.ReadUInt16();
            var classes = new List<string>(classCount);
            for (int i = 0; i < classCount; i++)
                classes.Add(br.ReadASCII(br.ReadInt32()));

            var reader = new DlseReader(br, classes);
            reader.ExpectObject(ClassProject);
            Maps = reader.ReadVector(() => reader.ReadPointer<Map>(ClassMap));
            TailValue = br.ReadBytes(8);
            // In id order; reading a target may define a new transition, which then takes its turn by its id
            uint last = 0;
            while (reader.Owners.Keys.Where(k => k > last).DefaultIfEmpty(0u).Min() is uint next && next != 0)
            {
                last = next;
                object owner = reader.Owners[next];
                var target = reader.ReadPointer<ITarget>(ClassState, ClassTransition);
                if (owner is Transition t) t.Target = target;
                else ((HostEvent)owner).Target = target;
            }
            if (br.Position != br.Length)
                throw new FormatException($"DLSE ESD: {br.Length - br.Position} bytes left over at 0x{br.Position:X}.");
        }

        private sealed class DlseReader
        {
            private readonly BinaryReaderEx br;
            private readonly List<string> classes;
            private readonly Dictionary<uint, object> objects = new();
            public readonly Dictionary<uint, object> Owners = new();

            public DlseReader(BinaryReaderEx br, List<string> classes)
            {
                this.br = br;
                this.classes = classes;
            }

            private FormatException Fail(string what) => new FormatException($"DLSE ESD: {what} at 0x{br.Position:X}.");

            private string Tag()
            {
                int index = br.ReadUInt16();
                if (index < 1 || index > classes.Count)
                    throw Fail($"class index {index}");
                return classes[index - 1];
            }

            private void Expect(string cls)
            {
                string tag = Tag();
                if (tag != cls)
                    throw Fail($"expected {cls}, found {tag}");
            }

            public void ExpectObject(string cls)
            {
                Expect(cls);
                int version = br.ReadInt32();
                if (version != Versions[cls])
                    throw Fail($"{cls} version {version} (expected {Versions[cls]})");
            }

            public List<T> ReadVector<T>(Func<T> element)
            {
                Expect(ClassVector);
                uint count = br.ReadUInt32();
                var list = new List<T>((int)Math.Min(count, 0x10000));
                for (uint i = 0; i < count; i++)
                    list.Add(element());
                return list;
            }

            private byte[] ReadBuffer()
            {
                Expect(ClassBuffer);
                return br.ReadBytes(br.ReadInt32());
            }

            public T ReadPointer<T>(params string[] allowed) where T : class
            {
                string cls = Tag();
                if (!allowed.Contains(cls))
                    throw Fail($"pointer to {cls} where {string.Join(" or ", allowed)} expected");
                uint id = br.ReadUInt32();
                if (id == 0)
                    return null;
                if (objects.TryGetValue(id, out object known))
                    return known as T ?? throw Fail($"object {id} is a {known.GetType().Name}, not a {typeof(T).Name}");

                ExpectObject(cls);
                object obj = cls switch
                {
                    ClassMap => new Map(),
                    ClassState => new MapState(),
                    ClassTransition => new Transition(),
                    ClassHostEvent => new HostEvent(),
                    ClassEvent => new ExternalEvent(),
                    _ => throw Fail($"no object of class {cls} expected"),
                };
                objects[id] = obj;
                if (obj is Transition or HostEvent)
                    Owners[id] = obj;
                ReadFields(obj);
                return obj as T ?? throw Fail($"{cls} where a {typeof(T).Name} was expected");
            }

            private Event ReadEvent() => ReadPointer<Event>(ClassEvent, ClassHostEvent);

            private void ReadFields(object obj)
            {
                switch (obj)
                {
                    case Map map:
                        map.Id = br.ReadInt32();
                        map.Initial = ReadPointer<MapState>(ClassState);
                        map.States = ReadVector(() => ReadPointer<MapState>(ClassState));
                        map.Transitions = ReadVector(() => ReadPointer<Transition>(ClassTransition));
                        break;
                    case MapState state:
                        state.Id = br.ReadInt32();
                        state.Transitions = ReadVector(() => ReadPointer<Transition>(ClassTransition));
                        state.EntryEvents = ReadVector(ReadEvent);
                        state.ExitEvents = ReadVector(ReadEvent);
                        break;
                    case Transition transition:
                        transition.Evaluator = ReadBuffer();
                        transition.PassEvents = ReadVector(ReadEvent);
                        break;
                    case HostEvent:
                        break;
                    case ExternalEvent ev:
                        ev.CommandId = br.ReadInt32();
                        int countPlusOne = br.ReadInt32();
                        if (countPlusOne < 1 || countPlusOne > MaxEventArgs + 1)
                            throw Fail($"event argument count {countPlusOne - 1}");
                        ev.Args = new List<byte[]>(countPlusOne - 1);
                        for (int i = 0; i < countPlusOne - 1; i++)
                            ev.Args.Add(ReadBuffer());
                        break;
                }
            }
        }

        #endregion

        #region Write

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = true;
            bool hasHostEvents = AllEvents().Any(e => e is HostEvent);
            var classes = new List<string> { ClassProject, ClassVector, ClassMap, ClassState, ClassTransition, ClassBuffer };
            if (hasHostEvents)
                classes.Add(ClassHostEvent);
            classes.Add(ClassEvent);

            bw.WriteASCII("DLSE");
            bw.WriteUInt16(2);
            bw.WriteUInt16(3);
            bw.WriteUInt32(HeaderA);
            bw.ReserveUInt32("OwnerCount");
            bw.ReserveUInt32("IdCount");
            bw.WriteUInt16((ushort)classes.Count);
            foreach (string cls in classes)
            {
                bw.WriteInt32(cls.Length);
                bw.WriteASCII(cls);
            }

            var writer = new DlseWriter(bw, classes);
            writer.WriteObjectHeader(ClassProject);
            writer.WriteVector(Maps, m => writer.WritePointer(ClassMap, m));
            if (TailValue == null || TailValue.Length != 8)
                throw new InvalidOperationException("DLSE ESD: the tail value must be 8 bytes.");
            bw.WriteBytes(TailValue);
            // Targets of every transition and host event, in definition order; writing one may define a new object,
            // which then gets its own target in turn
            for (int i = 0; i < writer.Owners.Count; i++)
            {
                ITarget target = writer.Owners[i] is Transition t ? t.Target : ((HostEvent)writer.Owners[i]).Target;
                writer.WriteTarget(target);
            }
            bw.FillUInt32("OwnerCount", (uint)writer.Owners.Count);
            // The id count is the highest id + 1, and one more in files that use host events
            bw.FillUInt32("IdCount", writer.NextId + (hasHostEvents ? 1u : 0u));
        }

        private IEnumerable<Event> AllEvents()
        {
            var seenTransitions = new HashSet<Transition>(ReferenceEqualityComparer.Instance);
            IEnumerable<Event> FromTransition(Transition t)
            {
                if (t == null || !seenTransitions.Add(t))
                    yield break;
                foreach (Event e in t.PassEvents)
                {
                    yield return e;
                    if (e is HostEvent h && h.Target is Transition ht)
                        foreach (Event sub in FromTransition(ht)) yield return sub;
                }
                if (t.Target is Transition tt)
                    foreach (Event sub in FromTransition(tt)) yield return sub;
            }
            foreach (Map map in Maps)
            {
                foreach (MapState state in map.States.Append(map.Initial).Where(s => s != null))
                {
                    foreach (Event e in state.EntryEvents.Concat(state.ExitEvents)) yield return e;
                    foreach (Transition t in state.Transitions)
                        foreach (Event e in FromTransition(t)) yield return e;
                }
                foreach (Transition t in map.Transitions)
                    foreach (Event e in FromTransition(t)) yield return e;
            }
        }

        private sealed class DlseWriter
        {
            private readonly BinaryWriterEx bw;
            private readonly Dictionary<string, ushort> classIndex;
            private readonly Dictionary<object, uint> ids = new(ReferenceEqualityComparer.Instance);
            public readonly List<object> Owners = new();
            public uint NextId = 1;

            public DlseWriter(BinaryWriterEx bw, List<string> classes)
            {
                this.bw = bw;
                classIndex = classes.Select((c, i) => (c, i)).ToDictionary(x => x.c, x => (ushort)(x.i + 1));
            }

            private void Tag(string cls) => bw.WriteUInt16(classIndex[cls]);

            public void WriteObjectHeader(string cls)
            {
                Tag(cls);
                bw.WriteInt32(Versions[cls]);
            }

            public void WriteVector<T>(IList<T> items, Action<T> element)
            {
                Tag(ClassVector);
                bw.WriteUInt32((uint)items.Count);
                foreach (T item in items)
                    element(item);
            }

            private void WriteBuffer(byte[] bytes)
            {
                Tag(ClassBuffer);
                bw.WriteInt32(bytes.Length);
                bw.WriteBytes(bytes);
            }

            public void WritePointer(string cls, object obj)
            {
                Tag(cls);
                if (obj == null)
                {
                    bw.WriteUInt32(0);
                    return;
                }
                if (ids.TryGetValue(obj, out uint known))
                {
                    bw.WriteUInt32(known);
                    return;
                }
                uint id = NextId++;
                ids[obj] = id;
                if (obj is Transition or HostEvent)
                    Owners.Add(obj);
                bw.WriteUInt32(id);
                WriteObjectHeader(cls);
                WriteFields(obj);
            }

            // A null target is tagged as a state, as in vanilla files
            public void WriteTarget(ITarget target) => WritePointer(target is Transition ? ClassTransition : ClassState, target);

            private void WriteEvent(Event e) => WritePointer(e is HostEvent ? ClassHostEvent : ClassEvent, e);

            private void WriteFields(object obj)
            {
                switch (obj)
                {
                    case Map map:
                        bw.WriteInt32(map.Id);
                        WritePointer(ClassState, map.Initial);
                        WriteVector(map.States, s => WritePointer(ClassState, s));
                        WriteVector(map.Transitions, t => WritePointer(ClassTransition, t));
                        break;
                    case MapState state:
                        bw.WriteInt32(state.Id);
                        WriteVector(state.Transitions, t => WritePointer(ClassTransition, t));
                        WriteVector(state.EntryEvents, WriteEvent);
                        WriteVector(state.ExitEvents, WriteEvent);
                        break;
                    case Transition transition:
                        WriteBuffer(transition.Evaluator ?? Array.Empty<byte>());
                        WriteVector(transition.PassEvents, WriteEvent);
                        break;
                    case HostEvent:
                        break;
                    case ExternalEvent ev:
                        if (ev.Args.Count > MaxEventArgs)
                            throw new InvalidOperationException($"DLSE ESD: command {ev.CommandId} has {ev.Args.Count} arguments; at most {MaxEventArgs} fit.");
                        bw.WriteInt32(ev.CommandId);
                        bw.WriteInt32(ev.Args.Count + 1);
                        foreach (byte[] arg in ev.Args)
                            WriteBuffer(arg);
                        break;
                    default:
                        throw new InvalidOperationException($"DLSE ESD: can't write a {obj.GetType().Name}.");
                }
            }
        }

        #endregion
    }
}
