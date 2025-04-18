using System;
using System.Collections.Generic;

namespace SoulsFormats
{
    /// <summary>
    /// A description format for ESDs, published only for DS2. It is not read by the game.
    /// </summary>
    public class EDD : SoulsFile<EDD>
    {
        /// <summary>
        /// Whether the EDD is in 64-bit or 32-bit format.
        /// </summary>
        public bool LongFormat { get; set; }

        /// <summary>
        /// Descriptions of built-in functions which can be used in the ESD file.
        /// </summary>
        public List<FunctionSpec> FunctionSpecs { get; set; }

        /// <summary>
        /// Descriptions of built-in commands which can be used in the ESD file.
        /// </summary>
        public List<CommandSpec> CommandSpecs { get; set; }

        /// <summary>
        /// Descriptions of machines and states defined in the ESD file.
        /// </summary>
        public List<MachineDesc> Machines { get; set; }

        /// <summary>
        /// Unknown.
        /// </summary>
        public int Unk80 { get; set; }

        /// <summary>
        /// Unknown.
        /// </summary>
        public int[] UnkB0 { get; private set; }

        /// <summary>
        /// Creates a new EDD with no data.
        /// </summary>
        public EDD()
        {
            LongFormat = false;
            FunctionSpecs = new List<FunctionSpec>();
            CommandSpecs = new List<CommandSpec>();
            Machines = new List<MachineDesc>();
            UnkB0 = new int[4];
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;

            string magic = br.AssertASCII(["fSSL", "fsSL"]);
            LongFormat = magic == "fsSL";
            br.VarintLong = LongFormat;

            br.AssertInt32(1);
            br.AssertInt32(1);
            br.AssertInt32(1);
            br.AssertInt32(0x7C);
            int dataSize = br.ReadInt32();
            br.AssertInt32(11);

            br.AssertInt32(LongFormat ? 0x58 : 0x34);
            br.AssertInt32(1);
            br.AssertInt32(LongFormat ? 0x10 : 8);
            int stringCount = br.ReadInt32();
            br.AssertInt32(4);
            br.AssertInt32(0);
            br.AssertInt32(8);
            int functionSpecCount = br.ReadInt32();
            int conditionSize = br.AssertInt32(LongFormat ? 0x10 : 8);
            int conditionCount = br.ReadInt32();
            br.AssertInt32(LongFormat ? 0x10 : 8);
            br.AssertInt32(0);
            br.AssertInt32(LongFormat ? 0x18 : 0x10);
            int commandSpecCount = br.ReadInt32();
            int commandSize = br.AssertInt32(4);
            int commandCount = br.ReadInt32();
            int passCommandSize = br.AssertInt32(LongFormat ? 0x10 : 8);
            int passCommandCount = br.ReadInt32();
            int stateSize = br.AssertInt32(LongFormat ? 0x78 : 0x3C);
            int stateCount = br.ReadInt32();
            br.AssertInt32(LongFormat ? 0x48 : 0x30);
            int machineCount = br.ReadInt32();

            int stringsOffset = br.ReadInt32();
            br.AssertInt32(0);
            br.AssertInt32(stringsOffset);
            Unk80 = br.ReadInt32();
            br.AssertInt32(dataSize);
            br.AssertInt32(0);
            br.AssertInt32(dataSize);
            br.AssertInt32(0);

            long dataStart = br.Position;
            br.AssertVarint(0);
            long commandSpecOffset = br.ReadVarint();
            br.AssertVarint(commandSpecCount);
            long functionSpecOffset = br.ReadVarint();
            br.AssertVarint(functionSpecCount);
            long machineOffset = br.ReadVarint();
            br.AssertInt32(machineCount);
            UnkB0 = br.ReadInt32s(4);
            if (LongFormat)
            {
                br.AssertInt32(0);
            }
            br.AssertVarint(LongFormat ? 0x58 : 0x34);
            br.AssertVarint(stringCount);

            List<string> strings = new List<string>();
            for (int i = 0; i < stringCount; i++)
            {
                long stringOffset = br.ReadVarint();
                // Char count not needed as all strings are null-terminated
                br.ReadVarint();
                string str = br.GetUTF16(dataStart + stringOffset);
                strings.Add(str);
            }

            FunctionSpecs = new List<FunctionSpec>();
            for (int i = 0; i < functionSpecCount; i++)
            {
                FunctionSpecs.Add(new FunctionSpec(br, strings));
            }

            Dictionary<long, ConditionDesc> conditions = new Dictionary<long, ConditionDesc>();
            for (int i = 0; i < conditionCount; i++)
            {
                long offset = br.Position - dataStart;
                conditions[offset] = new ConditionDesc(br);
            }

            CommandSpecs = new List<CommandSpec>();
            for (int i = 0; i < commandSpecCount; i++)
            {
                CommandSpecs.Add(new CommandSpec(br, strings));
            }

            Dictionary<long, CommandDesc> commands = new Dictionary<long, CommandDesc>();
            for (int i = 0; i < commandCount; i++)
            {
                long offset = br.Position - dataStart;
                commands[offset] = new CommandDesc(br, strings);
            }
            if (LongFormat)
            {
                // Data-start-aligned padding.
                long offset = br.Position - dataStart;
                if (offset % 8 > 0)
                {
                    br.Skip(8 - (int)(offset % 8));
                }
            }

            Dictionary<long, PassCommandDesc> passCommands = new Dictionary<long, PassCommandDesc>();
            for (int i = 0; i < passCommandCount; i++)
            {
                long offset = br.Position - dataStart;
                passCommands[offset] = new PassCommandDesc(br, commands, commandSize);
            }

            Dictionary<long, StateDesc> states = new Dictionary<long, StateDesc>();
            for (int i = 0; i < stateCount; i++)
            {
                long offset = br.Position - dataStart;
                states[offset] = new StateDesc(br, strings, dataStart, conditions, conditionSize, commands, commandSize, passCommands, passCommandSize);
            }

            Machines = new List<MachineDesc>();
            for (int i = 0; i < machineCount; i++)
            {
                Machines.Add(new MachineDesc(br, strings, states, stateSize));
            }

            if (conditions.Count > 0 || commands.Count > 0 || passCommands.Count > 0 || states.Count > 0)
            {
                throw new FormatException("Orphaned ESD descriptions found");
            }
        }

        /// <summary>
        /// A description of a built-in function in this type of ESD.
        /// </summary>
        public class FunctionSpec
        {
            /// <summary>
            /// ID used in ESD to call the function.
            /// </summary>
            public int ID { get; set; }

            /// <summary>
            /// Description of the function.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte Unk06 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte Unk07 { get; set; }

            /// <summary>
            /// Creates a function spec with the given function ID and description, or defaults if not provided.
            /// </summary>
            public FunctionSpec(int id = 0, string name = null)
            {
                ID = id;
                Name = name;
            }

            internal FunctionSpec(BinaryReaderEx br, List<string> strings)
            {
                ID = br.ReadInt32();
                short nameIndex = br.ReadInt16();
                Unk06 = br.ReadByte();
                Unk07 = br.ReadByte();

                Name = strings[nameIndex];
            }
        }

        /// <summary>
        /// A data structure associated with conditions. It has no data in DS2.
        /// </summary>
        public class ConditionDesc
        {
            /// <summary>
            /// Creates a condition with no data.
            /// </summary>
            public ConditionDesc() { }

            internal ConditionDesc(BinaryReaderEx br)
            {
                br.AssertVarint(-1);
                br.AssertVarint(0);
            }
        }

        /// <summary>
        /// A description of a built-in command in this type of ESD.
        /// </summary>
        public class CommandSpec
        {
            /// <summary>
            /// ID used in ESD to call the command.
            /// </summary>
            public long ID { get; set; }

            /// <summary>
            /// Description of the command.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public short Unk0E { get; set; }

            /// <summary>
            /// Creates a command spec with the given command ID and description, or defaults if not provided.
            /// </summary>
            public CommandSpec(long id = 0, string name = null)
            {
                ID = id;
                Name = name;
            }

            internal CommandSpec(BinaryReaderEx br, List<string> strings)
            {
                ID = br.ReadVarint();
                br.AssertVarint(-1);
                br.AssertInt32(0);
                short nameIndex = br.ReadInt16();
                Unk0E = br.ReadInt16();

                Name = strings[nameIndex];
            }
        }

        /// <summary>
        /// A description of a command used in a state of the ESD.
        /// </summary>
        public class CommandDesc
        {
            /// <summary>
            /// Description text. This often matches the command specification text, but is sometimes overridden.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Creates a command description with the given name, or default if not provided.
            /// </summary>
            public CommandDesc(string name = null)
            {
                Name = name;
            }

            internal CommandDesc(BinaryReaderEx br, List<string> strings)
            {
                short nameIndex = br.ReadInt16();
                br.AssertByte(1);
                br.AssertByte(0xFF);

                Name = strings[nameIndex];
            }
        }

        /// <summary>
        /// A description of commands in the pass command block of a condition. This appears to
        /// ignore the pass block if only contains the 'return' command, bank 7 id -1, so this
        /// annotation is uncommon in DS2.
        /// </summary>
        public class PassCommandDesc
        {
            /// <summary>
            /// Descriptions for the commands in the pass block.
            /// </summary>
            public List<CommandDesc> PassCommands { get; set; }

            /// <summary>
            /// Creates a new empty pass command description.
            /// </summary>
            public PassCommandDesc()
            {
                PassCommands = new List<CommandDesc>();
            }

            internal PassCommandDesc(BinaryReaderEx br, Dictionary<long, CommandDesc> commands, int commandSize)
            {
                int commandOffset = br.ReadInt32();
                int commandCount = br.ReadInt32();
                PassCommands = GetUniqueOffsetList(commandOffset, commandCount, commands, commandSize);
            }
        }

        /// <summary>
        /// A description of a state defined in the ESD.
        /// </summary>
        public class StateDesc
        {
            /// <summary>
            /// ID of the state.
            /// </summary>
            public long ID { get; set; }

            /// <summary>
            /// Description text.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Descriptions for commands in the entry block.
            /// </summary>
            public List<CommandDesc> EntryCommands { get; set; }

            /// <summary>
            /// Descriptions for commands in the exit block.
            /// </summary>
            public List<CommandDesc> ExitCommands { get; set; }

            /// <summary>
            /// Descriptions for commands in the while block.
            /// </summary>
            public List<CommandDesc> WhileCommands { get; set; }

            /// <summary>
            /// Descriptions for commands in conditions' pass blocks when nontrivial.
            /// </summary>
            public List<PassCommandDesc> PassCommands { get; set; }

            /// <summary>
            /// Descriptions for conditions. Doesn't contain anything interesting.
            /// </summary>
            public List<ConditionDesc> Conditions { get; set; }

            /// <summary>
            /// Creates a new state description with the given id and name, or defaults if not provided.
            /// </summary>
            public StateDesc(long id = 0, string name = null)
            {
                ID = id;
                Name = name;
                EntryCommands = new List<CommandDesc>();
                ExitCommands = new List<CommandDesc>();
                WhileCommands = new List<CommandDesc>();
                PassCommands = new List<PassCommandDesc>();
                Conditions = new List<ConditionDesc>();
            }

            internal StateDesc(
                BinaryReaderEx br, List<string> strings, long dataStart,
                Dictionary<long, ConditionDesc> conditions, int conditionSize,
                Dictionary<long, CommandDesc> commands, int commandSize,
                Dictionary<long, PassCommandDesc> passCommands, int passCommandSize)
            {
                ID = br.ReadVarint();
                long nameIndexOffset = br.ReadVarint();
                br.AssertVarint(1);
                long entryCommandOffset = br.ReadVarint();
                long entryCommandCount = br.ReadVarint();
                long exitCommandOffset = br.ReadVarint();
                long exitCommandCount = br.ReadVarint();
                long whileCommandOffset = br.ReadVarint();
                long whileCommandCount = br.ReadVarint();
                long passCommandOffset = br.ReadVarint();
                long passCommandCount = br.ReadVarint();
                long conditionOffset = br.ReadVarint();
                long conditionCount = br.ReadVarint();
                br.AssertVarint(-1);
                br.AssertVarint(0);

                short nameIndex = br.GetInt16(dataStart + nameIndexOffset);
                Name = strings[nameIndex];
                EntryCommands = GetUniqueOffsetList(entryCommandOffset, entryCommandCount, commands, commandSize);
                ExitCommands = GetUniqueOffsetList(exitCommandOffset, exitCommandCount, commands, commandSize);
                WhileCommands = GetUniqueOffsetList(whileCommandOffset, whileCommandCount, commands, commandSize);
                PassCommands = GetUniqueOffsetList(passCommandOffset, passCommandCount, passCommands, passCommandSize);
                Conditions = GetUniqueOffsetList(conditionOffset, conditionCount, conditions, conditionSize);
            }
        }

        /// <summary>
        /// A description of a machine defined in the ESD.
        /// </summary>
        public class MachineDesc
        {
            /// <summary>
            /// ID of the machine.
            /// </summary>
            public int ID { get; set; }

            /// <summary>
            /// Text description.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public short Unk06 { get; set; }

            /// <summary>
            /// Text description of params to the machine, when it is callable by other machines.
            /// </summary>
            public string[] ParamNames { get; private set; }

            /// <summary>
            /// Descriptions of the machine's states.
            /// </summary>
            public List<StateDesc> States { get; set; }

            /// <summary>
            /// Creates a new machine description with the given id and name, or defaults if not provided.
            /// </summary>
            public MachineDesc(int id = 0, string name = null)
            {
                ID = id;
                Name = name;
                ParamNames = new string[8];
                States = new List<StateDesc>();
            }

            internal MachineDesc(BinaryReaderEx br, List<string> strings, Dictionary<long, StateDesc> states, int stateSize)
            {
                ID = br.ReadInt32();
                short nameIndex = br.ReadInt16();
                Unk06 = br.ReadInt16();
                short[] paramIndices = br.ReadInt16s(8);
                br.AssertVarint(-1);
                br.AssertVarint(0);
                br.AssertVarint(-1);
                br.AssertVarint(0);
                long stateOffset = br.ReadVarint();
                long stateCount = br.ReadVarint();
                States = GetUniqueOffsetList(stateOffset, stateCount, states, stateSize);

                Name = strings[nameIndex];
                ParamNames = new string[8];
                for (int i = 0; i < 8; i++)
                {
                    if (paramIndices[i] >= 0)
                    {
                        ParamNames[i] = strings[paramIndices[i]];
                    }
                }
            }
        }

        private static List<T> GetUniqueOffsetList<T>(long offset, long count, Dictionary<long, T> offsets, int objSize)
        {
            List<T> objs = new List<T>();
            for (int i = 0; i < count; i++)
            {
                if (!offsets.ContainsKey(offset))
                {
                    throw new FormatException($"Nonexistent or reused {typeof(T)} at index {i}/{count}, offset {offset}");
                }
                objs.Add(offsets[offset]);
                offsets.Remove(offset);
                offset += objSize;
            }
            return objs;
        }
    }
}
