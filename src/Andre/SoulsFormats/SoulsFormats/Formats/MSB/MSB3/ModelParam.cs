using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SoulsFormats
{
    public partial class MSB3
    {
        internal enum ModelType : uint
        {
            MapPiece = 0,
            Object = 1,
            Enemy = 2,
            Item = 3,
            Player = 4,
            Collision = 5,
            Navmesh = 6,
            DummyObject = 7,
            DummyEnemy = 8,
            Other = 0xFFFFFFFF
        }

        /// <summary>
        /// A section containing all the models available to parts in this map.
        /// </summary>
        public class ModelParam : Param<Model>, IMsbParam<IMsbModel>
        {
            internal override int Version => 3;
            internal override string Type => "MODEL_PARAM_ST";

            /// <summary>
            /// Map piece models in this section.
            /// </summary>
            public List<Model.MapPiece> MapPieces { get; set; }

            /// <summary>
            /// Object models in this section.
            /// </summary>
            public List<Model.Object> Objects { get; set; }

            /// <summary>
            /// Enemy models in this section.
            /// </summary>
            public List<Model.Enemy> Enemies { get; set; }

            /// <summary>
            /// Player models in this section.
            /// </summary>
            public List<Model.Player> Players { get; set; }

            /// <summary>
            /// Collision models in this section.
            /// </summary>
            public List<Model.Collision> Collisions { get; set; }

            /// <summary>
            /// Other models in this section.
            /// </summary>
            public List<Model.Other> Others { get; set; }

            /// <summary>
            /// Creates a new ModelParam with no models.
            /// </summary>
            public ModelParam()
            {
                MapPieces = new List<Model.MapPiece>();
                Objects = new List<Model.Object>();
                Enemies = new List<Model.Enemy>();
                Players = new List<Model.Player>();
                Collisions = new List<Model.Collision>();
                Others = new List<Model.Other>();
            }

            /// <summary>
            /// Adds a model to the appropriate list for its type; returns the model.
            /// </summary>
            public Model Add(Model model)
            {
                switch (model)
                {
                    case Model.MapPiece m: MapPieces.Add(m); break;
                    case Model.Object m: Objects.Add(m); break;
                    case Model.Enemy m: Enemies.Add(m); break;
                    case Model.Player m: Players.Add(m); break;
                    case Model.Collision m: Collisions.Add(m); break;
                    case Model.Other m: Others.Add(m); break;

                    default:
                        throw new ArgumentException($"Unrecognized type {model.GetType()}.", nameof(model));
                }
                return model;
            }
            IMsbModel IMsbParam<IMsbModel>.Add(IMsbModel item) => Add((Model)item);

            /// <summary>
            /// Returns every model in the order they will be written.
            /// </summary>
            public override List<Model> GetEntries()
            {
                return SFUtil.ConcatAll<Model>(
                    MapPieces, Objects, Enemies, Players, Collisions,
                    Others);
            }
            IReadOnlyList<IMsbModel> IMsbParam<IMsbModel>.GetEntries() => GetEntries();

            internal override Model ReadEntry(BinaryReaderEx br)
            {
                ModelType type = br.GetEnum32<ModelType>(br.Position + 8);
                switch (type)
                {
                    case ModelType.MapPiece:
                        return MapPieces.EchoAdd(new Model.MapPiece(br));

                    case ModelType.Object:
                        return Objects.EchoAdd(new Model.Object(br));

                    case ModelType.Enemy:
                        return Enemies.EchoAdd(new Model.Enemy(br));

                    case ModelType.Player:
                        return Players.EchoAdd(new Model.Player(br));

                    case ModelType.Collision:
                        return Collisions.EchoAdd(new Model.Collision(br));

                    case ModelType.Other:
                        return Others.EchoAdd(new Model.Other(br));

                    default:
                        throw new NotImplementedException($"Unsupported model type: {type}");
                }
            }
        }

        /// <summary>
        /// A model available for use by parts in this map.
        /// </summary>
        public abstract class Model : NamedEntry, IMsbModel
        {
            private protected abstract ModelType Type { get; }
            private protected abstract bool HasTypeData { get; }

            /// <summary>
            /// The name of this model.
            /// </summary>
            public override string Name { get; set; }

            /// <summary>
            /// Unknown network path to a .sib file.
            /// </summary>
            public string SibPath { get; set; }

            private int InstanceCount;

            private protected Model(string name)
            {
                Name = name;
                SibPath = "";
            }

            /// <summary>
            /// Creates a deep copy of the model.
            /// </summary>
            public Model DeepCopy()
            {
                return (Model)MemberwiseClone();
            }
            IMsbModel IMsbModel.DeepCopy() => DeepCopy();

            private protected Model(BinaryReaderEx br)
            {
                long start = br.Position;

                long nameOffset = br.ReadInt64();
                br.AssertUInt32((uint)Type);
                br.ReadInt32(); // ID
                long sibOffset = br.ReadInt64();
                InstanceCount = br.ReadInt32();
                br.AssertInt32(0);
                long typeDataOffset = br.ReadInt64();

                if (nameOffset == 0)
                    throw new InvalidDataException($"{nameof(nameOffset)} must not be 0 in type {GetType()}.");
                if (sibOffset == 0)
                    throw new InvalidDataException($"{nameof(sibOffset)} must not be 0 in type {GetType()}.");
                if (HasTypeData ^ typeDataOffset != 0)
                    throw new InvalidDataException($"Unexpected {nameof(typeDataOffset)} 0x{typeDataOffset:X} in type {GetType()}.");

                br.Position = start + nameOffset;
                Name = br.ReadUTF16();

                br.Position = start + sibOffset;
                SibPath = br.ReadUTF16();

                if (HasTypeData)
                {
                    br.Position = start + typeDataOffset;
                    ReadTypeData(br);
                }
            }

            private protected virtual void ReadTypeData(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadTypeData)}.");

            internal override void Write(BinaryWriterEx bw, int id)
            {
                long start = bw.Position;

                bw.ReserveInt64("NameOffset");
                bw.WriteUInt32((uint)Type);
                bw.WriteInt32(id);
                bw.ReserveInt64("SibOffset");
                bw.WriteInt32(InstanceCount);
                bw.WriteInt32(0);
                bw.ReserveInt64("TypeDataOffset");

                bw.FillInt64("NameOffset", bw.Position - start);
                bw.WriteUTF16(MSB.ReambiguateName(Name), true);
                bw.FillInt64("SibOffset", bw.Position - start);
                bw.WriteUTF16(SibPath, true);
                bw.Pad(8);

                if (HasTypeData)
                {
                    bw.FillInt64("TypeDataOffset", bw.Position - start);
                    WriteTypeData(bw);
                }
                else
                {
                    bw.FillInt64("TypeDataOffset", 0);
                }
            }

            private protected virtual void WriteTypeData(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadTypeData)}.");

            internal void CountInstances(List<Part> parts)
            {
                InstanceCount = parts.Count(p => p.ModelName == Name);
            }

            /// <summary>
            /// Returns the type and name of this model.
            /// </summary>
            public override string ToString()
            {
                return $"{Type} : {Name}";
            }

            /// <summary>
            /// A fixed part of the level geometry.
            /// </summary>
            public class MapPiece : Model
            {
                private protected override ModelType Type => ModelType.MapPiece;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT01 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool UnkT02 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool UnkT03 { get; set; }

                /// <summary>
                /// Creates a MapPiece with default values.
                /// </summary>
                public MapPiece() : base("mXXXXXX")
                {
                    UnkT02 = true;
                    UnkT03 = true;
                }

                internal MapPiece(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadByte();
                    UnkT01 = br.ReadByte();
                    UnkT02 = br.ReadBoolean();
                    UnkT03 = br.ReadBoolean();

                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteByte(UnkT00);
                    bw.WriteByte(UnkT01);
                    bw.WriteBoolean(UnkT02);
                    bw.WriteBoolean(UnkT03);

                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// A dynamic or interactible entity.
            /// </summary>
            public class Object : Model
            {
                private protected override ModelType Type => ModelType.Object;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT01 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool UnkT02 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool UnkT03 { get; set; }

                /// <summary>
                /// Creates an Object with default values.
                /// </summary>
                public Object() : base("oXXXXXX")
                {
                    UnkT02 = true;
                    UnkT03 = true;
                }

                internal Object(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadByte();
                    UnkT01 = br.ReadByte();
                    UnkT02 = br.ReadBoolean();
                    UnkT03 = br.ReadBoolean();

                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteByte(UnkT00);
                    bw.WriteByte(UnkT01);
                    bw.WriteBoolean(UnkT02);
                    bw.WriteBoolean(UnkT03);

                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Any character in the map that is not the player.
            /// </summary>
            public class Enemy : Model
            {
                private protected override ModelType Type => ModelType.Enemy;
                private protected override bool HasTypeData => false;

                /// <summary>
                /// Creates an Enemy with default values.
                /// </summary>
                public Enemy() : base("cXXXX") { }

                internal Enemy(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// The player character.
            /// </summary>
            public class Player : Model
            {
                private protected override ModelType Type => ModelType.Player;
                private protected override bool HasTypeData => false;

                /// <summary>
                /// Creates a Player with default values.
                /// </summary>
                public Player() : base("c0000") { }

                internal Player(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// The invisible physical surface of the map.
            /// </summary>
            public class Collision : Model
            {
                private protected override ModelType Type => ModelType.Collision;
                private protected override bool HasTypeData => false;

                /// <summary>
                /// Creates a Collision with default values.
                /// </summary>
                public Collision() : base("hXXXXXX") { }

                internal Collision(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class Other : Model
            {
                private protected override ModelType Type => ModelType.Other;
                private protected override bool HasTypeData => false;

                /// <summary>
                /// Creates an Other with default values.
                /// </summary>
                public Other() : base("lXXXXXX") { }

                internal Other(BinaryReaderEx br) : base(br) { }
            }
        }
    }
}
