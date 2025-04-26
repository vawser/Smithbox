using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace SoulsFormats
{
    /// <summary>
    /// Rainbow Stone FXR format parser class.
    ///
    /// An SFX definition file used in DS3 and Sekiro. Extension: .fxr
    /// Initial work by TKGP, Meowmaritus and NamelessHoodie.
    ///
    /// Maintained by ivi for the Rainbow Stone FXR tool.
    /// </summary>
    [XmlType(TypeName = "FXR3")]
    public class FXR3 : SoulsFile<FXR3>
    {
        public FXRVersion Version { get; set; }

        public int Id { get; set; }

        [XmlElement("StateMap")]
        public StateMap RootStateMap { get; set; }

        [XmlElement("Container")]
        public Container RootContainer { get; set; }

        public HashSet<int> ReferenceList { get; set; }

        public HashSet<int> ExternalValueList { get; set; }

        public List<int> UnkBloodEnabler { get; set; }

        // public List<int> UnkEmpty { get; set; }

        public FXR3()
        {
            Version = FXRVersion.DarkSouls3;
            RootStateMap = new StateMap();
            RootContainer = new Container();
            ReferenceList = new HashSet<int>();
            ExternalValueList = new HashSet<int>();
            UnkBloodEnabler = new List<int>();
            // UnkEmpty = new List<int>();
        }

        public FXR3(FXR3 fxr)
        {
            Id = fxr.Id;
            Version = fxr.Version;
            RootStateMap = new StateMap(fxr.RootStateMap);
            RootContainer = new Container(fxr.RootContainer);
            ReferenceList = new HashSet<int>(fxr.ReferenceList);
            ExternalValueList = new HashSet<int>(fxr.ExternalValueList);
            UnkBloodEnabler = new List<int>(fxr.UnkBloodEnabler);
            // UnkEmpty = new List<int>(fxr.UnkEmpty);
        }

        protected override bool Is(BinaryReaderEx br)
        {
            if (br.Length < 8L)
                return false;
            string ascii = br.GetASCII(0L, 4);
            short int16 = br.GetInt16(6L);
            return ascii == "FXR\0" && (int16 == 4 || int16 == 5);
        }

        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;

            br.AssertASCII("FXR\0");
            int num1 = br.AssertInt16(0);
            Version = br.ReadEnum16<FXRVersion>();
            br.AssertInt32(1);
            Id = br.ReadInt32();
            int stateMapOffset = br.ReadInt32();
            br.AssertInt32(1); // Section 1 count
            br.ReadInt32(); // Section 2 offset
            br.ReadInt32(); // Section 2 count
            br.ReadInt32(); // Section 3 offset
            br.ReadInt32(); // Section 3 count
            int containerOffset = br.ReadInt32();
            br.ReadInt32(); // Section 4 count
            br.ReadInt32(); // Section 5 offset
            br.ReadInt32(); // Section 5 count
            br.ReadInt32(); // Section 6 offset
            br.ReadInt32(); // Section 6 count
            br.ReadInt32(); // Section 7 offset
            br.ReadInt32(); // Section 7 count
            br.ReadInt32(); // Section 8 offset
            br.ReadInt32(); // Section 8 count
            br.ReadInt32(); // Section 9 offset
            br.ReadInt32(); // Section 9 count
            br.ReadInt32(); // Section 10 offset
            br.ReadInt32(); // Section 10 count
            br.ReadInt32(); // Section 11 offset
            br.ReadInt32(); // Section 11 count
            br.AssertInt32(1);
            br.AssertInt32(0);

            if (Version == FXRVersion.Sekiro)
            {
                int referenceOffset = br.ReadInt32();
                int referenceCount = br.ReadInt32();
                int externalValueOffset = br.ReadInt32();
                int externalValueCount = br.ReadInt32();
                int unkBloodEnablerOffset = br.ReadInt32();
                int unkBloodEnablerCount = br.ReadInt32();
                // int section15Offset = br.ReadInt32();
                // int section15Count = br.ReadInt32();
                br.ReadInt32(); // Section 15 offset
                br.AssertInt32(0); // Section 15 count

                ReferenceList = new HashSet<int>(br.GetInt32s(referenceOffset, referenceCount));
                ExternalValueList = new HashSet<int>(br.GetInt32s(externalValueOffset, externalValueCount));
                UnkBloodEnabler = new List<int>(br.GetInt32s(unkBloodEnablerOffset, unkBloodEnablerCount));
                // UnkEmpty = new List<int>(br.GetInt32s(section15Offset, section15Count));
            }
            else
            {
                ReferenceList = new HashSet<int>();
                ExternalValueList = new HashSet<int>();
                UnkBloodEnabler = new List<int>();
                // UnkEmpty = new List<int>();
            }

            br.Position = stateMapOffset;
            RootStateMap = new StateMap(br);

            br.Position = containerOffset;
            RootContainer = new Container(br);
        }

        protected override void Write(BinaryWriterEx bw)
        {
            bw.WriteASCII("FXR\0");
            bw.WriteInt16(0);
            bw.WriteUInt16((ushort)Version);
            bw.WriteInt32(1);
            bw.WriteInt32(Id);
            bw.ReserveInt32("StateMapOffset");
            bw.WriteInt32(1);
            bw.ReserveInt32("StateOffset");
            bw.WriteInt32(RootStateMap.States.Count);
            bw.ReserveInt32("TransitionOffset");
            bw.ReserveInt32("TransitionCount");
            bw.ReserveInt32("ContainerOffset");
            bw.ReserveInt32("ContainerCount");
            bw.ReserveInt32("EffectOffset");
            bw.ReserveInt32("EffectCount");
            bw.ReserveInt32("ActionOffset");
            bw.ReserveInt32("ActionCount");
            bw.ReserveInt32("PropertyOffset");
            bw.ReserveInt32("PropertyCount");
            bw.ReserveInt32("ModifierOffset");
            bw.ReserveInt32("ModifierCount");
            bw.ReserveInt32("ConditionalPropertyOffset");
            bw.ReserveInt32("ConditionalPropertyCount");
            bw.ReserveInt32("UnkFieldListOffset");
            bw.ReserveInt32("UnkFieldListCount");
            bw.ReserveInt32("FieldOffset");
            bw.ReserveInt32("FieldCount");
            bw.WriteInt32(1);
            bw.WriteInt32(0);

            if (Version == FXRVersion.Sekiro)
            {
                bw.ReserveInt32("ReferenceOffset");
                bw.WriteInt32(ReferenceList.Count);
                bw.ReserveInt32("ExternalValueOffset");
                bw.WriteInt32(ExternalValueList.Count);
                bw.ReserveInt32("UnkBloodEnablerOffset");
                bw.WriteInt32(UnkBloodEnabler.Count);
                bw.ReserveInt32("UnkEmptyOffset");
                bw.WriteInt32(0); // UnkEmpty always 0
                // bw.WriteInt32(UnkEmpty.Count);
                // bw.WriteInt32(0);
                // bw.WriteInt32(0);
            }

            bw.FillInt32("StateMapOffset", (int)bw.Position);
            RootStateMap.Write(bw);
            bw.Pad(16);
            bw.FillInt32("StateOffset", (int)bw.Position);
            RootStateMap.WriteStates(bw);
            bw.Pad(16);
            bw.FillInt32("TransitionOffset", (int)bw.Position);
            List<State> states = RootStateMap.States;
            List<StateCondition> transitions = new List<StateCondition>();
            for (int index = 0; index < states.Count; ++index)
                states[index].WriteTransitions(bw, index, transitions);
            bw.FillInt32("TransitionCount", transitions.Count);
            bw.Pad(16);
            bw.FillInt32("ContainerOffset", (int)bw.Position);
            List<Container> Containers = new List<Container>();
            RootContainer.Write(bw, Containers);
            RootContainer.WriteContainers(bw, Containers);
            bw.FillInt32("ContainerCount", Containers.Count);
            bw.Pad(16);
            bw.FillInt32("EffectOffset", (int)bw.Position);
            int EffectCount = 0;
            for (int index = 0; index < Containers.Count; ++index)
                Containers[index].WriteEffects(bw, index, ref EffectCount);
            bw.FillInt32("EffectCount", EffectCount);
            bw.Pad(16);
            bw.FillInt32("ActionOffset", (int)bw.Position);
            EffectCount = 0;
            List<Action> actions = new List<Action>();
            for (int index = 0; index < Containers.Count; ++index)
                Containers[index].WriteActions(bw, index, ref EffectCount, actions);
            bw.FillInt32("ActionCount", actions.Count);
            bw.Pad(16);
            bw.FillInt32("PropertyOffset", (int)bw.Position);
            List<Property> properties = new List<Property>();
            for (int index = 0; index < actions.Count; ++index)
                actions[index].WriteProperties(bw, index, properties);
            bw.FillInt32("PropertyCount", properties.Count);
            bw.Pad(16);
            bw.FillInt32("ModifierOffset", (int)bw.Position);
            List<PropertyModifier> modifiers = new List<PropertyModifier>();
            for (int index = 0; index < properties.Count; ++index)
                properties[index].WriteModifiers(bw, index, modifiers);
            bw.FillInt32("ModifierCount", modifiers.Count);
            bw.Pad(16);
            bw.FillInt32("ConditionalPropertyOffset", (int)bw.Position);
            List<Property> conditionalProperties = new List<Property>();
            for (int index = 0; index < modifiers.Count; ++index)
                modifiers[index].WriteProperties(bw, index, conditionalProperties);
            bw.FillInt32("ConditionalPropertyCount", conditionalProperties.Count);
            bw.Pad(16);
            bw.FillInt32("UnkFieldListOffset", (int)bw.Position);
            List<UnkFieldList> fieldLists = new List<UnkFieldList>();
            for (int index = 0; index < actions.Count; ++index)
                actions[index].WriteUnkFieldLists(bw, index, fieldLists);
            bw.FillInt32("UnkFieldListCount", fieldLists.Count);
            bw.Pad(16);
            bw.FillInt32("FieldOffset", (int)bw.Position);
            int fieldCount = 0;
            for (int index = 0; index < transitions.Count; ++index)
                transitions[index].WriteFields(bw, index, ref fieldCount);
            for (int index = 0; index < actions.Count; ++index)
                actions[index].WriteFields(bw, index, ref fieldCount);
            for (int index = 0; index < properties.Count; ++index)
                properties[index].WriteFields(bw, index, ref fieldCount, false);
            for (int index = 0; index < modifiers.Count; ++index)
                modifiers[index].WriteFields(bw, index, ref fieldCount);
            for (int index = 0; index < conditionalProperties.Count; ++index)
                conditionalProperties[index].WriteFields(bw, index, ref fieldCount, true);
            for (int index = 0; index < fieldLists.Count; ++index)
                fieldLists[index].WriteFields(bw, index, ref fieldCount);
            bw.FillInt32("FieldCount", fieldCount);
            bw.Pad(16);

            if (Version != FXRVersion.Sekiro)
                return;

            bw.FillInt32("ReferenceOffset", (int)bw.Position);
            bw.WriteInt32s(ReferenceList.ToList());
            bw.Pad(16);

            bw.FillInt32("ExternalValueOffset", (int)bw.Position);
            bw.WriteInt32s(ExternalValueList.ToList());
            bw.Pad(16);

            bw.FillInt32("UnkBloodEnablerOffset", (int)bw.Position);
            bw.WriteInt32s(UnkBloodEnabler);
            bw.Pad(16);

            // if (UnkEmpty.Count > 0)
            // {
            //     bw.FillInt32("UnkEmptyOffset", (int)bw.Position);
            //     bw.WriteInt32s(UnkEmpty);
            //     bw.Pad(16);
            // }
            // else
            // {
            bw.FillInt32("UnkEmptyOffset", 0);
            // }
        }

        public enum FXRVersion : ushort
        {
            DarkSouls3 = 4,
            Sekiro = 5,
        }

        public class StateMap
        {
            [XmlElement(nameof(State))] public List<State> States { get; set; }

            public StateMap() => States = new List<State>();

            internal StateMap(BinaryReaderEx br)
            {
                br.AssertInt32(0);
                int capacity = br.ReadInt32();
                int num = br.ReadInt32();
                br.AssertInt32(0);
                br.StepIn(num);
                States = new List<State>(capacity);
                for (int index = 0; index < capacity; ++index)
                    States.Add(new State(br));
                br.StepOut();
            }

            internal StateMap(StateMap stateMap)
            {
                States = stateMap.States.Select(state => new State(state)).ToList();
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteInt32(0);
                bw.WriteInt32(States.Count);
                bw.ReserveInt32("StateMapStatesOffset");
                bw.WriteInt32(0);
            }

            internal void WriteStates(BinaryWriterEx bw)
            {
                bw.FillInt32("StateMapStatesOffset", (int)bw.Position);
                for (int index = 0; index < States.Count; ++index)
                    States[index].Write(bw, index);
            }
        }

        public class State
        {
            [XmlElement("StayCondition")] public List<StateCondition> Conditions { get; set; }

            public State() => Conditions = new List<StateCondition>();

            internal State(BinaryReaderEx br)
            {
                br.AssertInt32(0);
                int capacity = br.ReadInt32();
                int num = br.ReadInt32();
                br.AssertInt32(0);
                br.StepIn(num);
                Conditions = new List<StateCondition>(capacity);
                for (int index = 0; index < capacity; ++index)
                    Conditions.Add(new StateCondition(br));
                br.StepOut();
            }

            internal State(State state)
            {
                Conditions = state.Conditions.Select(transition => new StateCondition(transition)).ToList();
            }

            internal void Write(BinaryWriterEx bw, int index)
            {
                bw.WriteInt32(0);
                bw.WriteInt32(Conditions.Count);
                bw.ReserveInt32(string.Format("StateTransitionsOffset[{0}]", index));
                bw.WriteInt32(0);
            }

            internal void WriteTransitions(
                BinaryWriterEx bw,
                int index,
                List<StateCondition> transitions)
            {
                bw.FillInt32(string.Format("StateTransitionsOffset[{0}]", index), (int)bw.Position);
                foreach (StateCondition transition in Conditions)
                    transition.Write(bw, transitions);
            }
        }

        public class StateCondition
        {
            public enum OperatorType
            {
                NotEqual = 0,
                Equal = 1,
                GreaterThanOrEqual = 2,
                GreaterThan = 3,
                LessThanOrEqual = 4, // Not present in format
                LessThan = 5 // Not present in format
            }

            public enum OperandType
            {
                Literal = -4,
                External = -3,
                TimeOfDay = -2,
                StateTime = -1
            }

            [XmlInclude(typeof(ConditionOperandLiteral)),
             XmlInclude(typeof(ConditionOperandExternal)),
             XmlInclude(typeof(ConditionOperandStateTime)),
             XmlInclude(typeof(ConditionOperandUnkMinus2))]
            public abstract class ConditionOperand
            {
                [XmlIgnore] public abstract OperandType Type { get; }

                [XmlIgnore]
                public object Value
                {
                    get
                    {
                        switch (this)
                        {
                            case ConditionOperandExternal intOp:
                                return intOp.Value;
                            case ConditionOperandLiteral floatOp:
                                return floatOp.Value;
                            default:
                                return null;
                        }
                    }
                }

                public Field ToField()
                {
                    switch (this)
                    {
                        case ConditionOperandExternal intOp:
                            return new Field.FieldInt(intOp.Value);
                        case ConditionOperandLiteral floatOp:
                            return new Field.FieldFloat(floatOp.Value);
                        default:
                            return null;
                    }
                }

                public static ConditionOperand Create(OperandType type, object value)
                {
                    switch (type)
                    {
                        case OperandType.Literal:
                            return new ConditionOperandLiteral()
                            {
                                Value = Convert.ToSingle(value)
                            };
                        case OperandType.External:
                            return new ConditionOperandExternal()
                            {
                                Value = Convert.ToInt32(value)
                            };
                        case OperandType.StateTime:
                            return new ConditionOperandStateTime();
                        case OperandType.TimeOfDay:
                            return new ConditionOperandUnkMinus2();
                    }

                    throw new ArgumentException("Invalid operand type");
                }

                public static ConditionOperand Create(OperandType type, Field field)
                {
                    if (type == OperandType.External && field is Field.FieldInt fieldInt)
                    {
                        return new ConditionOperandExternal
                        {
                            Value = fieldInt.Value
                        };
                    }

                    if (type == OperandType.Literal && field is Field.FieldFloat fieldFloat)
                    {
                        return new ConditionOperandLiteral
                        {
                            Value = fieldFloat.Value
                        };
                    }

                    if (type is OperandType.StateTime && field is null)
                        return new ConditionOperandStateTime();

                    if (type is OperandType.TimeOfDay && field is null)
                        return new ConditionOperandUnkMinus2();

                    throw new ArgumentException("Field does not match operand type");
                }

                public static ConditionOperand Create(ConditionOperand operand)
                {
                    switch (operand.GetType().Name)
                    {
                        case "ConditionOperandLiteral":
                            var litOp = (ConditionOperandLiteral)operand;
                            return new ConditionOperandLiteral
                            {
                                Value = litOp.Value
                            };
                        case "ConditionOperandExternal":
                            var extOp = (ConditionOperandExternal)operand;
                            return new ConditionOperandExternal
                            {
                                Value = extOp.Value
                            };
                        case "ConditionOperandStateTime":
                            return new ConditionOperandStateTime();
                        case "ConditionOperandUnkMinus2":
                            return new ConditionOperandUnkMinus2();
                    }

                    throw new ArgumentException("Unknown operand type");
                }
            }

            [XmlType("External")]
            public class ConditionOperandExternal : ConditionOperand
            {
                public override OperandType Type => OperandType.External;
                [XmlAttribute] public new int Value;
            }

            [XmlType("Literal")]
            public class ConditionOperandLiteral : ConditionOperand
            {
                public override OperandType Type => OperandType.Literal;
                [XmlAttribute] public new float Value;
            }

            [XmlType("StateTime")]
            public class ConditionOperandStateTime : ConditionOperand
            {
                public override OperandType Type => OperandType.StateTime;
            }

            [XmlType("UnkMinus2")]
            public class ConditionOperandUnkMinus2 : ConditionOperand
            {
                public override OperandType Type => OperandType.TimeOfDay;
            }

            public class ConditionOperator
            {
                [XmlText] public OperatorType Type;
                [XmlAttribute("Unk")] public byte UnkModifier;

                public ConditionOperator(ConditionOperator op)
                {
                    Type = op.Type;
                    UnkModifier = op.UnkModifier;
                }
                public ConditionOperator() {}
            }

            public ConditionOperand LeftOperand { get; set; }

            [XmlElement("Op")] public ConditionOperator Operator { get; set; }

            public ConditionOperand RightOperand { get; set; }

            [XmlAttribute] public int ElseMoveToStateIndex { get; set; }

            public StateCondition()
            {
            }

            internal StateCondition(BinaryReaderEx br)
            {
                var op = br.ReadInt16();
                var opType = (OperatorType)(op & 0b0011);
                var unkOperatorModifier = (byte)(op >> 2 & 0b0011);
                br.AssertByte(0);
                br.AssertByte(1);
                br.AssertInt32(0);
                ElseMoveToStateIndex = br.ReadInt32();
                br.AssertInt32(0);
                var field1Source = br.AssertInt16(-4, -3, -2, -1);
                var leftOperandType = (OperandType)field1Source;
                br.AssertByte(0);
                br.AssertByte(1);
                br.AssertInt32(0);
                bool hasField1 = br.AssertInt32(0, 1) == 1; // Has Field1 or not
                br.AssertInt32(0);
                int fieldOffset1 = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                var field2Source = br.AssertInt16(-4, -3, -2, -1);
                var rightOperandType = (OperandType)field2Source;
                br.AssertByte(0);
                br.AssertByte(1);
                br.AssertInt32(0);
                bool hasField2 = br.AssertInt32(0, 1) == 1; // Has Field2 or not
                br.AssertInt32(0);
                int fieldOffset2 = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                Operator = new ConditionOperator()
                {
                    Type = opType,
                    UnkModifier = unkOperatorModifier
                };
                LeftOperand = ConditionOperand.Create(leftOperandType,
                    hasField1 ? Field.ReadAt(br, fieldOffset1, leftOperandType, 0) : null);
                RightOperand = ConditionOperand.Create(rightOperandType,
                    hasField2 ? Field.ReadAt(br, fieldOffset2, rightOperandType, 0) : null);

                if (LeftOperand.Type == OperandType.Literal && RightOperand.Type != OperandType.Literal)
                {
                    // Switcheroo
                    var formerlyRight = RightOperand;
                    var formerlyLeft = LeftOperand;
                    LeftOperand = formerlyRight;
                    RightOperand = formerlyLeft;
                    switch (Operator.Type)
                    {
                        case OperatorType.GreaterThan:
                            Operator.Type = OperatorType.LessThan;
                            break;
                        case OperatorType.GreaterThanOrEqual:
                            Operator.Type = OperatorType.LessThanOrEqual;
                            break;
                    }
                }
            }

            internal StateCondition(StateCondition stateCondition)
            {
                Operator = new ConditionOperator()
                {
                    Type = stateCondition.Operator.Type,
                    UnkModifier = stateCondition.Operator.UnkModifier,
                };
                ElseMoveToStateIndex = stateCondition.ElseMoveToStateIndex;
                LeftOperand = ConditionOperand.Create(stateCondition.LeftOperand);
                RightOperand = ConditionOperand.Create(stateCondition.RightOperand);
            }

            internal void Write(BinaryWriterEx bw, List<StateCondition> transitions)
            {
                var leftOperand = LeftOperand;
                var rightOperand = RightOperand;
                var operatorType = Operator.Type;
                // Handle invalid operator type
                if (operatorType >= OperatorType.LessThanOrEqual)
                {
                    leftOperand = RightOperand;
                    rightOperand = LeftOperand;
                    switch (Operator.Type)
                    {
                        case OperatorType.LessThanOrEqual:
                            operatorType = OperatorType.GreaterThanOrEqual;
                            break;
                        case OperatorType.LessThan:
                            operatorType = OperatorType.GreaterThan;
                            break;
                    }
                }
                int count = transitions.Count;
                short op = (short)((byte)operatorType | ((Operator.UnkModifier << 2) & 0b1100));
                bw.WriteInt16(op);
                bw.WriteByte(0);
                bw.WriteByte(1);
                bw.WriteInt32(0);
                bw.WriteInt32(ElseMoveToStateIndex);
                bw.WriteInt32(0);
                bw.WriteInt16((short)leftOperand.Type);
                bw.WriteByte(0);
                bw.WriteByte(1);
                bw.WriteInt32(0);
                bw.WriteInt32(leftOperand.Value != null ? 1 : 0);
                bw.WriteInt32(0);
                bw.ReserveInt32(string.Format("TransitionFieldOffset1[{0}]", count));
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt16((short)rightOperand.Type);
                bw.WriteByte(0);
                bw.WriteByte(1);
                bw.WriteInt32(0);
                bw.WriteInt32(rightOperand.Value != null ? 1 : 0);
                bw.WriteInt32(0);
                bw.ReserveInt32(string.Format("TransitionFieldOffset2[{0}]", count));
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                transitions.Add(this);
            }

            internal void WriteFields(BinaryWriterEx bw, int index, ref int fieldCount)
            {
                var leftOperand = LeftOperand;
                var rightOperand = RightOperand;
                // Handle invalid operators
                if (Operator.Type >= OperatorType.LessThanOrEqual)
                {
                    leftOperand = RightOperand;
                    rightOperand = LeftOperand;
                }
                int fieldPos1 = leftOperand.Value != null ? (int)bw.Position : 0;
                bw.FillInt32(string.Format("TransitionFieldOffset1[{0}]", index), fieldPos1);
                if (leftOperand.Value != null)
                {
                    leftOperand.ToField().Write(bw);
                    fieldCount++;
                }

                int fieldPos2 = rightOperand.Value != null ? (int)bw.Position : 0;
                bw.FillInt32(string.Format("TransitionFieldOffset2[{0}]", index), fieldPos2);
                if (rightOperand.Value != null)
                {
                    rightOperand.ToField().Write(bw);
                    fieldCount++;
                }
            }
        }

        public class Container
        {
            [XmlAttribute] public short Id { get; set; }

            public List<Action> Actions { get; set; }
            public List<Effect> Effects { get; set; }
            public List<Container> Containers { get; set; }

            public Container()
            {
                Containers = new List<Container>();
                Effects = new List<Effect>();
                Actions = new List<Action>();
            }

            internal Container(BinaryReaderEx br)
            {
                Id = br.ReadInt16();
                int num1 = br.AssertByte(0);
                int num2 = br.AssertByte(1);
                br.AssertInt32(0);
                int EffectCount = br.ReadInt32();
                int ActionCount = br.ReadInt32();
                int ContainerCount = br.ReadInt32();
                br.AssertInt32(0);
                int EffectOffset = br.ReadInt32();
                br.AssertInt32(0);
                int ActionOffset = br.ReadInt32();
                br.AssertInt32(0);
                int ContainerOffset = br.ReadInt32();
                br.AssertInt32(0);
                br.StepIn(ContainerOffset);
                Containers = new List<Container>(ContainerCount);
                for (int index = 0; index < ContainerCount; ++index)
                    Containers.Add(new Container(br));
                br.StepOut();
                br.StepIn(EffectOffset);
                Effects = new List<Effect>(EffectCount);
                for (int index = 0; index < EffectCount; ++index)
                    Effects.Add(new Effect(br));
                br.StepOut();
                br.StepIn(ActionOffset);
                Actions = new List<Action>(ActionCount);
                for (int index = 0; index < ActionCount; ++index)
                    Actions.Add(new Action(br));
                br.StepOut();
            }

            internal Container(Container cont)
            {
                Id = cont.Id;
                Containers = cont.Containers.Select(c => new Container(c)).ToList();
                Effects = cont.Effects.Select(effect => new Effect(effect)).ToList();
                Actions = cont.Actions.Select(action => new Action(action)).ToList();
            }

            internal void Write(BinaryWriterEx bw, List<Container> containers)
            {
                int count = containers.Count;
                bw.WriteInt16(Id);
                bw.WriteByte(0);
                bw.WriteByte(1);
                bw.WriteInt32(0);
                bw.WriteInt32(Effects.Count);
                bw.WriteInt32(Actions.Count);
                bw.WriteInt32(Containers.Count);
                bw.WriteInt32(0);
                bw.ReserveInt32(string.Format("ContainerEffectsOffset[{0}]", count));
                bw.WriteInt32(0);
                bw.ReserveInt32(string.Format("ContainerActionsOffset[{0}]", count));
                bw.WriteInt32(0);
                bw.ReserveInt32(string.Format("ContainerChildContainersOffset[{0}]", count));
                bw.WriteInt32(0);
                containers.Add(this);
            }

            internal void WriteContainers(BinaryWriterEx bw, List<Container> containers)
            {
                int num = containers.IndexOf(this);
                if (Containers.Count == 0)
                {
                    bw.FillInt32(string.Format("ContainerChildContainersOffset[{0}]", num), 0);
                }
                else
                {
                    bw.FillInt32(string.Format("ContainerChildContainersOffset[{0}]", num), (int)bw.Position);
                    foreach (Container container in Containers)
                        container.Write(bw, containers);
                    foreach (Container container in Containers)
                        container.WriteContainers(bw, containers);
                }
            }

            internal void WriteEffects(BinaryWriterEx bw, int index, ref int effectCount)
            {
                if (Effects.Count == 0)
                {
                    bw.FillInt32(string.Format("ContainerEffectsOffset[{0}]", index), 0);
                }
                else
                {
                    bw.FillInt32(string.Format("ContainerEffectsOffset[{0}]", index), (int)bw.Position);
                    for (int index1 = 0; index1 < Effects.Count; ++index1)
                        Effects[index1].Write(bw, effectCount + index1);
                    effectCount += Effects.Count;
                }
            }

            internal void WriteActions(
                BinaryWriterEx bw,
                int index,
                ref int effectCount,
                List<Action> actions)
            {
                bw.FillInt32(string.Format("ContainerActionsOffset[{0}]", index), (int)bw.Position);
                foreach (Action action in Actions)
                    action.Write(bw, actions);
                for (int index1 = 0; index1 < Effects.Count; ++index1)
                    Effects[index1].WriteActions(bw, effectCount + index1, actions);
                effectCount += Effects.Count;
            }
        }

        public class Effect
        {
            [XmlAttribute] public short Id { get; set; }

            public List<Action> Actions { get; set; }

            public Effect() => Actions = new List<Action>();

            internal Effect(BinaryReaderEx br)
            {
                Id = br.ReadInt16();
                int num1 = br.AssertByte(0);
                int num2 = br.AssertByte(1);
                br.AssertInt32(0);
                br.AssertInt32(0);
                int capacity = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                int num3 = br.ReadInt32();
                br.AssertInt32(0);
                br.StepIn(num3);
                Actions = new List<Action>(capacity);
                for (int index = 0; index < capacity; ++index)
                    Actions.Add(new Action(br));
                br.StepOut();
            }

            internal Effect(Effect effect)
            {
                Id = effect.Id;
                Actions = effect.Actions.Select(action => new Action(action)).ToList();
            }

            internal void Write(BinaryWriterEx bw, int index)
            {
                bw.WriteInt16(Id);
                bw.WriteByte(0);
                bw.WriteByte(1);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(Actions.Count);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.ReserveInt32(string.Format("EffectActionsOffset[{0}]", index));
                bw.WriteInt32(0);
            }

            internal void WriteActions(
                BinaryWriterEx bw,
                int index,
                List<Action> actions)
            {
                bw.FillInt32(string.Format("EffectActionsOffset[{0}]", index), (int)bw.Position);
                foreach (Action action in Actions)
                    action.Write(bw, actions);
            }
        }

        public class Action
        {
            [XmlAttribute] public short Id { get; set; }

            public bool Unk02 { get; set; }

            public bool Unk03 { get; set; }

            public int Unk04 { get; set; }

            public List<UnkFieldList> UnkFieldLists { get; set; }

            [XmlArrayItem(Type = typeof(Field.FieldInt), ElementName = "Int"),
             XmlArrayItem(Type = typeof(Field.FieldFloat), ElementName = "Float")]
            public List<Field> Fields1 { get; set; }

            [XmlArrayItem(Type = typeof(Field.FieldInt), ElementName = "Int"),
             XmlArrayItem(Type = typeof(Field.FieldFloat), ElementName = "Float")]
            public List<Field> Fields2 { get; set; }

            public List<Property> Properties1 { get; set; }

            public List<Property> Properties2 { get; set; }

            public Action()
            {
                Properties1 = new List<Property>();
                Properties2 = new List<Property>();
                UnkFieldLists = new List<UnkFieldList>();
                Fields1 = new List<Field>();
                Fields2 = new List<Field>();
            }

            internal Action(BinaryReaderEx br)
            {
                Id = br.ReadInt16(); // 0
                Unk02 = br.ReadBoolean(); // 2
                Unk03 = br.ReadBoolean(); // 3
                Unk04 = br.ReadInt32(); // 4
                int fieldCount1 = br.ReadInt32(); // 8
                int unkFieldListCount = br.ReadInt32(); // 12
                int propertyCount1 = br.ReadInt32(); // 16
                int fieldCount2 = br.ReadInt32(); // 20
                br.AssertInt32(0);
                int propertyCount2 = br.ReadInt32();
                int fieldOffset = br.ReadInt32();
                // Console.WriteLine($"fieldOffset: {fieldOffset}");
                br.AssertInt32(0);
                int unkFieldListOffset = br.ReadInt32();
                br.AssertInt32(0);
                int propertyOffset = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);

                br.StepIn(propertyOffset);
                {
                    Properties1 = new List<Property>(propertyCount1);
                    for (int index = 0; index < propertyCount1; ++index)
                        Properties1.Add(new Property(br, false));

                    Properties2 = new List<Property>(propertyCount2);
                    for (int index = 0; index < propertyCount2; ++index)
                        Properties2.Add(new Property(br, false));
                }
                br.StepOut();

                br.StepIn(unkFieldListOffset);
                {
                    UnkFieldLists = new List<UnkFieldList>(unkFieldListCount);
                    for (int index = 0; index < unkFieldListCount; ++index)
                        UnkFieldLists.Add(new UnkFieldList(br));
                }
                br.StepOut();

                br.StepIn(fieldOffset);
                {
                    Fields1 = Field.ReadMany(br, fieldCount1, this);
                    Fields2 = Field.ReadMany(br, fieldCount2, this);
                }
                br.StepOut();
            }

            internal Action(Action action)
            {
                Id = action.Id;
                Unk02 = action.Unk02;
                Unk03 = action.Unk03;
                Unk04 = action.Unk04;
                Properties1 = action.Properties1.Select(prop => new Property(prop)).ToList();
                Properties2 = action.Properties2.Select(prop => new Property(prop)).ToList();
                UnkFieldLists = action.UnkFieldLists.Select(section => new UnkFieldList(section)).ToList();
                Fields1 = action.Fields1.Select(Field.Create).ToList();
                Fields2 = action.Fields2.Select(Field.Create).ToList();
            }

            internal void Write(BinaryWriterEx bw, List<Action> Actions)
            {
                int count = Actions.Count;
                bw.WriteInt16(Id);
                bw.WriteBoolean(Unk02);
                bw.WriteBoolean(Unk03);
                bw.WriteInt32(Unk04);
                bw.WriteInt32(Fields1.Count);
                bw.WriteInt32(UnkFieldLists.Count);
                bw.WriteInt32(Properties1.Count);
                bw.WriteInt32(Fields2.Count);
                bw.WriteInt32(0);
                bw.WriteInt32(Properties2.Count);
                bw.ReserveInt32($"ActionFieldsOffset[{count}]");
                bw.WriteInt32(0);
                bw.ReserveInt32(string.Format("ActionUnkFieldListsOffset[{0}]", count));
                bw.WriteInt32(0);
                bw.ReserveInt32(string.Format("ActionPropertiesOffset[{0}]", count));
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                Actions.Add(this);
            }

            internal void WriteProperties(BinaryWriterEx bw, int index, List<Property> properties)
            {
                bw.FillInt32(string.Format("ActionPropertiesOffset[{0}]", index), (int)bw.Position);
                foreach (Property property in Properties1)
                    property.Write(bw, properties, false);
                foreach (Property property in Properties2)
                    property.Write(bw, properties, false);
            }

            internal void WriteUnkFieldLists(BinaryWriterEx bw, int index, List<UnkFieldList> unkFieldLists)
            {
                bw.FillInt32(string.Format("ActionUnkFieldListsOffset[{0}]", index), (int)bw.Position);
                foreach (UnkFieldList unkFieldList in UnkFieldLists)
                    unkFieldList.Write(bw, unkFieldLists);
            }

            internal void WriteFields(BinaryWriterEx bw, int index, ref int fieldCount)
            {
                if (Fields1.Count == 0 && Fields2.Count == 0)
                {
                    bw.FillInt32(string.Format("ActionFieldsOffset[{0}]", index), 0);
                }
                else
                {
                    bw.FillInt32(string.Format("ActionFieldsOffset[{0}]", index), (int)bw.Position);
                    foreach (Field field in Fields1)
                        field.Write(bw);
                    foreach (Field field in Fields2)
                        field.Write(bw);
                    fieldCount += Fields1.Count + Fields2.Count;
                }
            }
        }

        public enum FieldType
        {
            Int,
            Float
        }

        [XmlInclude(typeof(FieldFloat))]
        [XmlInclude(typeof(FieldInt))]
        public abstract class Field
        {
            [XmlIgnore] public virtual FieldType Type { get; }

            public static Field Create(FieldType type)
            {
                switch (type)
                {
                    case FieldType.Int:
                        return new FieldInt();
                    case FieldType.Float:
                        return new FieldFloat();
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            }

            public static Field Create(float value)
            {
                return new FieldFloat(value);
            }

            public static Field Create(int value)
            {
                return new FieldInt(value);
            }

            public static Field Create(Field field)
            {
                if (field.GetType() == typeof(FieldFloat))
                {
                    return new FieldFloat(((FieldFloat)field).Value);
                }

                if (field.GetType() == typeof(FieldInt))
                {
                    return new FieldInt(((FieldInt)field).Value);
                }

                throw new InvalidOperationException("Field passed for creation was neither Float nor Int");
            }

            public static FieldFloat Create(FieldFloat field)
            {
                return new FieldFloat(field.Value);
            }

            public static FieldInt Create(FieldInt field)
            {
                return new FieldInt(field.Value);
            }

            public static Field Read(BinaryReaderEx br, object context = null, int? index = null)
            {
                Field field = null;
                bool isInt = false;
                // First value of interpolated properties is int (stop count), rest are floats.
                if (context is Property property)
                {
                    // Unk AC6 InterpolationType
                    if (property.InterpolationType == PropertyInterpolationType.UnkAc6)
                    {
                        if (index > 0 && index <= (int)property.PropertyType + 1)
                            isInt = true;
                    }
                    else if (property.InterpolationType != PropertyInterpolationType.Constant)
                    {
                        if (index == 0)
                            isInt = true;
                    }
                }
                else if (context is StateCondition.OperandType)
                {
                    isInt = context is StateCondition.OperandType.External;
                }
                else
                {
                    // TODO: Replace heuristic with field def
                    float single = br.GetSingle(br.Position);
                    if (single >= 9.99999974737875E-05 && single < 1000000.0 ||
                        single <= -9.99999974737875E-05 && single > -1000000.0)
                        field = new FieldFloat(single);
                    else
                        isInt = true;
                }

                if (field == null)
                {
                    if (isInt)
                        field = new FieldInt(br.GetInt32(br.Position));
                    else
                        field = new FieldFloat(br.GetSingle(br.Position));
                }

                br.Position += 4L;
                return field;
            }

            public static Field ReadAt(
                BinaryReaderEx br,
                int offset,
                object context = null,
                int? index = null)
            {
                br.StepIn(offset);
                Field field = Read(br, context, index);
                br.StepOut();
                return field;
            }

            public static List<Field> ReadMany(BinaryReaderEx br, int count, object context = null)
            {
                List<Field> fieldList = new List<Field>();
                for (int index = 0; index < count; ++index)
                    fieldList.Add(Read(br, context, index));
                return fieldList;
            }

            public static List<Field> ReadManyAt(
                BinaryReaderEx br,
                int offset,
                int count,
                object context = null)
            {
                br.StepIn(offset);
                List<Field> fieldList = ReadMany(br, count, context);
                br.StepOut();
                return fieldList;
            }

            public abstract void Write(BinaryWriterEx bw);

            [XmlType("Float")]
            public class FieldFloat : Field
            {
                public override FieldType Type => FieldType.Float;
                [XmlAttribute] public float Value;

                public override void Write(BinaryWriterEx bw) => bw.WriteSingle(Value);

                public FieldFloat(float value)
                {
                    Value = value;
                }

                public FieldFloat()
                {
                }
            }

            [XmlType("Int")]
            public class FieldInt : Field
            {
                public override FieldType Type => FieldType.Int;
                [XmlAttribute] public int Value;

                public override void Write(BinaryWriterEx bw) => bw.WriteInt32(Value);

                public FieldInt(int value)
                {
                    Value = value;
                }

                public FieldInt()
                {
                }
            }
        }

        public enum PropertyType
        {
            Scalar = 0,
            Vector2 = 1,
            Vector3 = 2,
            Color = 3
        }

        public enum PropertyInterpolationType
        {
            Zero = 0,
            One = 1,
            Constant = 2,
            Stepped = 3,
            Linear = 4,
            Curve1 = 5,
            Curve2 = 6,
            UnkAc6 = 7
        }

        public class Property
        {
            [XmlAttribute] public PropertyType PropertyType { get; set; }

            [XmlAttribute] public PropertyInterpolationType InterpolationType { get; set; }

            [XmlAttribute] public bool IsLoop { get; set; }

            [XmlArrayItem(Type = typeof(Field.FieldInt), ElementName = "Int"),
             XmlArrayItem(Type = typeof(Field.FieldFloat), ElementName = "Float")]
            public List<Field> Fields { get; set; }

            public List<PropertyModifier> Modifiers { get; set; }

            public Property()
            {
                Modifiers = new List<PropertyModifier>();
                Fields = new List<Field>();
            }

            internal Property(BinaryReaderEx br, bool conditional)
            {
                var typeEnumA = br.ReadInt16();
                br.AssertByte(0);
                br.AssertByte(1);
                PropertyType = (PropertyType)(typeEnumA & 0b00000000_00000011);
                InterpolationType = (PropertyInterpolationType)((typeEnumA & 0b00000000_11110000) >> 4);
                IsLoop = Convert.ToBoolean((typeEnumA & 0b00010000_00000000) >> 12);
                br.ReadInt32(); // TypeEnumB
                int count = br.ReadInt32();
                br.AssertInt32(0);
                int offset = br.ReadInt32();
                br.AssertInt32(0);
                if (!conditional)
                {
                    int num3 = br.ReadInt32();
                    br.AssertInt32(0);
                    int capacity = br.ReadInt32();
                    br.AssertInt32(0);
                    br.StepIn(num3);
                    Modifiers = new List<PropertyModifier>(capacity);
                    for (int index = 0; index < capacity; ++index)
                        Modifiers.Add(new PropertyModifier(br));
                    br.StepOut();
                }

                Fields = Field.ReadManyAt(br, offset, count, this);
            }

            internal Property(Property prop)
            {
                PropertyType = prop.PropertyType;
                InterpolationType = prop.InterpolationType;
                IsLoop = prop.IsLoop;
                Modifiers = prop.Modifiers.Select(section => new PropertyModifier(section)).ToList();
                Fields = prop.Fields.Select(Field.Create).ToList();
            }

            internal void Write(BinaryWriterEx bw, List<Property> properties, bool conditional)
            {
                int count = properties.Count;
                int typeEnumA = (int)PropertyType | (int)InterpolationType << 4 | Convert.ToInt32(IsLoop) << 12;
                int typeEnumB = ((int)PropertyType | (int)InterpolationType << 2) + (Convert.ToInt32(IsLoop) << 4);
                bw.WriteInt16(Convert.ToInt16(typeEnumA));
                bw.WriteByte(0);
                bw.WriteByte(1);
                bw.WriteInt32(typeEnumB);
                bw.WriteInt32(Fields.Count);
                bw.WriteInt32(0);
                var offsetName = (conditional ? "ConditionalProperty" : "Property") + "FieldsOffset[{0}]";
                bw.ReserveInt32(string.Format(offsetName, count));
                bw.WriteInt32(0);
                if (!conditional)
                {
                    bw.ReserveInt32(string.Format("PropertyModifiersOffset[{0}]", count));
                    bw.WriteInt32(0);
                    bw.WriteInt32(Modifiers.Count);
                    bw.WriteInt32(0);
                }

                properties.Add(this);
            }

            internal void WriteModifiers(BinaryWriterEx bw, int index, List<PropertyModifier> modifiers)
            {
                bw.FillInt32(string.Format("PropertyModifiersOffset[{0}]", index), (int)bw.Position);
                foreach (PropertyModifier modifier in Modifiers)
                    modifier.Write(bw, modifiers);
            }

            internal void WriteFields(BinaryWriterEx bw, int index, ref int fieldCount, bool conditional)
            {
                var offsetName = (conditional ? "ConditionalProperty" : "Property") + "FieldsOffset[{0}]";
                if (Fields.Count == 0)
                {
                    bw.FillInt32(string.Format(offsetName, index), 0);
                }
                else
                {
                    bw.FillInt32(string.Format(offsetName, index), (int)bw.Position);
                    foreach (Field field in Fields)
                        field.Write(bw);
                    fieldCount += Fields.Count;
                }
            }
        }

        public class PropertyModifier
        {
            [XmlAttribute] public ushort TypeEnumA { get; set; }

            [XmlAttribute] public uint TypeEnumB { get; set; }

            [XmlArrayItem(Type = typeof(Field.FieldInt), ElementName = "Int"),
             XmlArrayItem(Type = typeof(Field.FieldFloat), ElementName = "Float")]
            public List<Field> Fields { get; set; }

            public List<Property> Properties { get; set; }

            public PropertyModifier()
            {
                Properties = new List<Property>();
                Fields = new List<Field>();
            }

            internal PropertyModifier(BinaryReaderEx br)
            {
                TypeEnumA = br.ReadUInt16();
                br.AssertByte(0);
                br.AssertByte(1);
                TypeEnumB = br.ReadUInt32();
                int fieldCount = br.ReadInt32();
                int propertyCount = br.ReadInt32();
                int fieldOffset = br.ReadInt32();
                br.AssertInt32(0);
                int propertyOffset = br.ReadInt32();
                br.AssertInt32(0);
                br.StepIn(propertyOffset);
                Properties = new List<Property>(propertyCount);
                for (int index = 0; index < propertyCount; ++index)
                    Properties.Add(new Property(br, true));
                br.StepOut();
                Fields = Field.ReadManyAt(br, fieldOffset, fieldCount, this);
            }

            internal PropertyModifier(PropertyModifier property)
            {
                TypeEnumA = property.TypeEnumA;
                TypeEnumB = property.TypeEnumB;
                Properties = property.Properties.Select(prop => new Property(prop)).ToList();
                Fields = property.Fields.Select(Field.Create).ToList();
            }

            internal void Write(BinaryWriterEx bw, List<PropertyModifier> modifiers)
            {
                int count = modifiers.Count;
                bw.WriteUInt16(TypeEnumA);
                bw.WriteByte(0);
                bw.WriteByte(1);
                bw.WriteUInt32(TypeEnumB);
                bw.WriteInt32(Fields.Count);
                bw.WriteInt32(Properties.Count);
                bw.ReserveInt32(string.Format("ModifierFieldsOffset[{0}]", count));
                bw.WriteInt32(0);
                bw.ReserveInt32(string.Format("ModifierConditionalPropertysOffset[{0}]", count));
                bw.WriteInt32(0);
                modifiers.Add(this);
            }

            internal void WriteProperties(BinaryWriterEx bw, int index, List<Property> properties)
            {
                bw.FillInt32(string.Format("ModifierConditionalPropertysOffset[{0}]", index), (int)bw.Position);
                foreach (Property property in Properties)
                    property.Write(bw, properties, true);
            }

            internal void WriteFields(BinaryWriterEx bw, int index, ref int fieldCount)
            {
                bw.FillInt32(string.Format("ModifierFieldsOffset[{0}]", index), (int)bw.Position);
                foreach (Field field in Fields)
                    field.Write(bw);
                fieldCount += Fields.Count;
            }
        }

        public class UnkFieldList
        {
            [XmlArrayItem(Type = typeof(Field.FieldInt), ElementName = "Int"),
             XmlArrayItem(Type = typeof(Field.FieldFloat), ElementName = "Float")]
            public List<Field> Fields { get; set; }

            public UnkFieldList() => Fields = new List<Field>();

            internal UnkFieldList(BinaryReaderEx br)
            {
                int offset = br.ReadInt32();
                br.AssertInt32(0);
                int count = br.ReadInt32();
                br.AssertInt32(0);
                Fields = Field.ReadManyAt(br, offset, count, this);
            }

            internal UnkFieldList(UnkFieldList fieldList)
            {
                Fields = fieldList.Fields.Select(Field.Create).ToList();
            }

            internal void Write(BinaryWriterEx bw, List<UnkFieldList> fieldLists)
            {
                int count = fieldLists.Count;
                bw.ReserveInt32(string.Format("UnkFieldListFieldsOffset[{0}]", count));
                bw.WriteInt32(0);
                bw.WriteInt32(Fields.Count);
                bw.WriteInt32(0);
                fieldLists.Add(this);
            }

            internal void WriteFields(BinaryWriterEx bw, int index, ref int fieldCount)
            {
                bw.FillInt32(string.Format("UnkFieldListFieldsOffset[{0}]", index), (int)bw.Position);
                foreach (Field field in Fields)
                    field.Write(bw);
                fieldCount += Fields.Count;
            }
        }
    }

    public class FXR3EnhancedSerialization
    {
        public static FXR3 XMLToFXR3(XDocument XML)
        {
            XmlSerializer test = new XmlSerializer(typeof(FXR3));
            XmlReader xmlReader = XML.CreateReader();

            return (FXR3)test.Deserialize(xmlReader);
        }

        public static XDocument FXR3ToXML(FXR3 fxr)
        {
            XDocument XDoc = new XDocument();

            using (var xmlWriter = XDoc.CreateWriter())
            {
                var thing = new XmlSerializer(typeof(FXR3));
                thing.Serialize(xmlWriter, fxr);
            }

            return XDoc;
        }
    }
}