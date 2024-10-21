using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SoulsFormats
{
    public partial class MSBFA
    {
        /// <summary>
        /// The type of the Model entry.
        /// </summary>
        internal enum ModelType : int
        {
            /// <summary>
            /// A model that is a piece of the map, such as the ground.
            /// </summary>
            MapPiece = 0,

            /// <summary>
            /// A model that is dynamic or interactable such as a building or car that can be destroyed.
            /// </summary>
            Object = 1,

            /// <summary>
            /// The model of a minor living entity, such as most non-AC enemies.
            /// </summary>
            Enemy = 2,

            /// <summary>
            /// The model of a major living entity, such as an AC.
            /// </summary>
            Dummy = 4
        }

        #region Param

        /// <summary>
        /// Model files that are available for parts to use.
        /// </summary>
        public class ModelParam : Param<Model>, IMsbParam<IMsbModel>
        {
            /// <summary>
            /// Models for fixed terrain and scenery.
            /// </summary>
            public List<Model.MapPiece> MapPieces { get; set; }

            /// <summary>
            /// Models for dynamic or interactable objects.
            /// </summary>
            public List<Model.Object> Objects { get; set; }

            /// <summary>
            /// Models for minor living entities.
            /// </summary>
            public List<Model.Enemy> Enemies { get; set; }

            /// <summary>
            /// Models for major living entities.
            /// </summary>
            public List<Model.Dummy> Dummies { get; set; }

            /// <summary>
            /// Creates an empty ModelParam with the default version.
            /// </summary>
            public ModelParam() : base(10001002, "MODEL_PARAM_ST")
            {
                MapPieces = new List<Model.MapPiece>();
                Objects = new List<Model.Object>();
                Enemies = new List<Model.Enemy>();
                Dummies = new List<Model.Dummy>();
            }

            /// <summary>
            /// Adds a model to the appropriate list for its type; returns the model.
            /// </summary>
            public Model Add(Model model)
            {
                switch (model)
                {
                    case Model.MapPiece m:
                        MapPieces.Add(m);
                        break;
                    case Model.Object m:
                        Objects.Add(m);
                        break;
                    case Model.Enemy m:
                        Enemies.Add(m);
                        break;
                    case Model.Dummy m:
                        Dummies.Add(m);
                        break;
                    default:
                        throw new ArgumentException($"Unrecognized type {model.GetType()}.", nameof(model));
                }
                return model;
            }
            IMsbModel IMsbParam<IMsbModel>.Add(IMsbModel item) => Add((Model)item);

            /// <summary>
            /// Returns every Model in the order they will be written.
            /// </summary>
            public override List<Model> GetEntries() => SFUtil.ConcatAll<Model>(MapPieces, Objects, Enemies, Dummies);
            IReadOnlyList<IMsbModel> IMsbParam<IMsbModel>.GetEntries() => GetEntries();

            internal override Model ReadEntry(BinaryReaderEx br)
            {
                ModelType type = br.GetEnum32<ModelType>(br.Position + 4);
                switch (type)
                {
                    case ModelType.MapPiece:
                        return MapPieces.EchoAdd(new Model.MapPiece(br));
                    case ModelType.Object:
                        return Objects.EchoAdd(new Model.Object(br));
                    case ModelType.Enemy:
                        return Enemies.EchoAdd(new Model.Enemy(br));
                    case ModelType.Dummy:
                        return Dummies.EchoAdd(new Model.Dummy(br));
                    default:
                        throw new NotImplementedException($"Unimplemented model type: {type}");
                }
            }
        }

        #endregion

        #region Entry

        public abstract class Model : ParamEntry, IMsbModel
        {
            /// <summary>
            /// The type of an overridden type.
            /// </summary>
            private protected abstract ModelType Type { get; }

            /// <summary>
            /// Whether or not an overridden type has a row reference.
            /// </summary>
            private protected abstract bool HasRowReference { get; }

            /// <summary>
            /// The path to a resource that presumably used to contain this model during development. Usually a path to an SIB file.
            /// </summary>
            public string ResourcePath { get; set; }

            /// <summary>
            /// The number of parts that may reference this model.
            /// </summary>
            private int InstanceCount;

            /// <summary>
            /// Unknown; Seems to have bytes that affect shadows and fog?
            /// </summary>
            public RenderConfig Render { get; set; }

            /// <summary>
            /// Creates a new Model entry with default values. This is to be used as a base for overriding types.
            /// </summary>
            /// <param name="name"></param>
            private protected Model(string name)
            {
                Name = name;
                ResourcePath = "";
                Render = new RenderConfig();
            }

            /// <summary>
            /// Creates a deep copy of the model.
            /// </summary>
            public Model DeepCopy()
            {
                return (Model)MemberwiseClone();
            }
            IMsbModel IMsbModel.DeepCopy() => DeepCopy();

            /// <summary>
            /// Reads a Model entry from a stream.
            /// </summary>
            private protected Model(BinaryReaderEx br)
            {
                long start = br.Position;
                int nameOffset = br.ReadInt32();
                br.AssertUInt32((uint)Type);
                br.ReadInt32(); // ID
                int resourceOffset = br.ReadInt32();
                InstanceCount = br.ReadInt32();
                int offsetRender = br.ReadInt32();
                int rowReferenceOffset = br.ReadInt32();

                br.Position = start + nameOffset;
                Name = br.ReadShiftJIS();

                br.Position = start + resourceOffset;
                ResourcePath = br.ReadShiftJIS();

                br.Position = start + offsetRender;
                Render = new RenderConfig(br);

                if (HasRowReference)
                {
                    br.Position = start + rowReferenceOffset;
                    ReadRowReferenceConfig(br);
                }
            }

            private protected virtual void ReadRowReferenceConfig(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadRowReferenceConfig)}.");

            internal override void Write(BinaryWriterEx bw, int id)
            {
                long start = bw.Position;
                bw.ReserveInt32("NameOffset");
                bw.WriteInt32((int)Type);
                bw.WriteInt32(id);
                bw.ReserveInt32("ResourceOffset");
                bw.WriteInt32(InstanceCount);
                bw.ReserveInt32("OffsetRender");
                bw.ReserveInt32("TypeDataOffset");

                bw.FillInt32("NameOffset", (int)(bw.Position - start));
                bw.WriteShiftJIS(MSB.ReambiguateName(Name), true);
                bw.FillInt32("ResourceOffset", (int)(bw.Position - start));
                bw.WriteASCII(ResourcePath, true);
                bw.Pad(4);

                bw.FillInt32("OffsetRender", (int)(bw.Position - start));
                Render.Write(bw);

                if (HasRowReference)
                {
                    bw.FillInt32("TypeDataOffset", (int)(bw.Position - start));
                    WriteRowReferenceConfig(bw);
                }
                else
                {
                    bw.FillInt32("TypeDataOffset", 0);
                }
            }

            private protected virtual void WriteRowReferenceConfig(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteRowReferenceConfig)}.");

            /// <summary>
            /// Sets the count of part instances that references this model from a list of parts.
            /// </summary>
            /// <param name="parts">A list of all parts.</param>
            internal void CountInstances(List<Part> parts)
            {
                InstanceCount = parts.Count(p => p.ModelName == Name);
            }

            /// <summary>
            /// Returns the type and name of the model as a string.
            /// </summary>
            public override string ToString()
            {
                return $"{Type} {Name}";
            }

            #region Model Sub Structs

            /// <summary>
            /// Unknown.
            /// </summary>
            public class RenderConfig
            {
                public byte Unk00 { get; set; }
                public byte Unk01 { get; set; }
                public byte Unk02 { get; set; }
                public byte Unk03 { get; set; }

                public RenderConfig()
                {
                    Unk00 = 0;
                    Unk01 = 0;
                    Unk02 = 0;
                    Unk03 = 0;
                }

                internal RenderConfig(BinaryReaderEx br)
                {
                    Unk00 = br.ReadByte();
                    Unk01 = br.ReadByte();
                    Unk02 = br.ReadByte();
                    Unk03 = br.ReadByte();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteByte(Unk00);
                    bw.WriteByte(Unk01);
                    bw.WriteByte(Unk02);
                    bw.WriteByte(Unk03);
                }
            }

            #endregion

            #region ModelType Structs

            /// <summary>
            /// A fixed part of the level geometry.
            /// </summary>
            public class MapPiece : Model
            {
                private protected override ModelType Type => ModelType.MapPiece;
                private protected override bool HasRowReference => false;

                /// <summary>
                /// Creates a <see cref="MapPiece"/> with default values.
                /// </summary>
                public MapPiece() : base("mXXXX") { }

                /// <summary>
                /// Reads a <see cref="MapPiece"/> from a stream.
                /// </summary>
                internal MapPiece(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// A dynamic or interactible entity.
            /// </summary>
            public class Object : Model
            {
                private protected override ModelType Type => ModelType.Object;
                private protected override bool HasRowReference => true;

                /// <summary>
                /// The destroy ID of this object for the DestroyAP param.
                /// </summary>
                [MSBParamReference(ParamName = "DestroyAP")]
                public short DestroyAPID { get; set; }

                /// <summary>
                /// Unknown. 0 on objects.
                /// </summary>
                public short UnkT02 { get; set; }

                /// <summary>
                /// Creates an <see cref="Object"/> with default values.
                /// </summary>
                public Object() : base("oXXXX") { }

                /// <summary>
                /// Reads an <see cref="Object"/> from a stream.
                /// </summary>
                internal Object(BinaryReaderEx br) : base(br) { }

                private protected override void ReadRowReferenceConfig(BinaryReaderEx br)
                {
                    DestroyAPID = br.ReadInt16();
                    UnkT02 = br.ReadInt16();
                }

                private protected override void WriteRowReferenceConfig(BinaryWriterEx bw)
                {
                    bw.WriteInt16(DestroyAPID);
                    bw.WriteInt16(UnkT02);
                }
            }

            /// <summary>
            /// Most minor living entities on the map.
            /// </summary>
            public class Enemy : Model
            {
                private protected override ModelType Type => ModelType.Enemy;
                private protected override bool HasRowReference => false;

                /// <summary>
                /// Creates an <see cref="Enemy"/> with default values.
                /// </summary>
                public Enemy() : base("eXXXX") { }

                /// <summary>
                /// Reads an <see cref="Enemy"/> from a stream.
                /// </summary>
                internal Enemy(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// Most major living entities on the map.
            /// </summary>
            public class Dummy : Model
            {
                private protected override ModelType Type => ModelType.Dummy;
                private protected override bool HasRowReference => true;

                /// <summary>
                /// The design ID of this dummy for the AcAssemblyDrawing param.
                /// </summary>
                [MSBParamReference(ParamName = "AcAssemblyDrawing")]
                public short AcAssemblyDrawingID { get; set; }

                /// <summary>
                /// Unknown. 100 on dummies.
                /// </summary>
                public short UnkT02 { get; set; }

                /// <summary>
                /// Creates a <see cref="Dummy"/> with default values.
                /// </summary>
                public Dummy() : base("aXXXX") { }

                /// <summary>
                /// Reads a <see cref="Dummy"/> from a stream.
                /// </summary>
                internal Dummy(BinaryReaderEx br) : base(br) { }

                private protected override void ReadRowReferenceConfig(BinaryReaderEx br)
                {
                    AcAssemblyDrawingID = br.ReadInt16();
                    UnkT02 = br.ReadInt16();
                }

                private protected override void WriteRowReferenceConfig(BinaryWriterEx bw)
                {
                    bw.WriteInt16(AcAssemblyDrawingID);
                    bw.WriteInt16(UnkT02);
                }
            }

            #endregion
        }

        #endregion
    }
}
