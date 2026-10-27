using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SoulsFormats;

public partial class MSB_DB_NT
{
    internal enum ModelType : uint
    {
        MapPiece = 0,
        Enemy = 2,
        Player = 4,
        Collision = 5,
        DummyEnemy = 8,
        DummyAsset = 9,
        Asset = 10, // AEG
        PrefabAsset = 11 // AEP
    }
    public class ModelParam : Param<Model>, IMsbParam<IMsbModel>
    {
        public List<Model.MapPiece> MapPieces { get; set; }
        public List<Model.Enemy> Enemies { get; set; }
        public List<Model.Player> Players { get; set; }
        public List<Model.Collision> Collisions { get; set; }

        public List<Model.DummyEnemy> DummyEnemies { get; set; }
        public List<Model.DummyAsset> DummyAssets { get; set; }

        public List<Model.Asset> Assets { get; set; }
        public List<Model.PrefabAsset> PrefabAssets { get; set; }

        public ModelParam() : base(66, "MODEL_PARAM_ST")
        {
            MapPieces = new List<Model.MapPiece>();
            Enemies = new List<Model.Enemy>();
            Players = new List<Model.Player>();
            Collisions = new List<Model.Collision>();
            DummyEnemies = new List<Model.DummyEnemy>();
            DummyAssets = new List<Model.DummyAsset>();
            Assets = new List<Model.Asset>();
            PrefabAssets = new List<Model.PrefabAsset>();
        }

        public Model Add(Model model)
        {
            switch (model)
            {
                case Model.MapPiece m: MapPieces.Add(m); break;
                case Model.Enemy m: Enemies.Add(m); break;
                case Model.Player m: Players.Add(m); break;
                case Model.Collision m: Collisions.Add(m); break;
                case Model.DummyEnemy m: DummyEnemies.Add(m); break;
                case Model.DummyAsset m: DummyAssets.Add(m); break;
                case Model.Asset m: Assets.Add(m); break;
                case Model.PrefabAsset m: PrefabAssets.Add(m); break;

                default:
                    throw new ArgumentException($"Unrecognized type {model.GetType()}.", nameof(model));
            }
            return model;
        }
        IMsbModel IMsbParam<IMsbModel>.Add(IMsbModel item) => Add((Model)item);

        public override List<Model> GetEntries()
        {
            return SFUtil.ConcatAll<Model>(
                MapPieces, 
                Enemies, 
                Players, 
                Collisions, 
                DummyEnemies, 
                DummyAssets, 
                Assets, 
                PrefabAssets);
        }
        IReadOnlyList<IMsbModel> IMsbParam<IMsbModel>.GetEntries() => GetEntries();

        internal override Model ReadEntry(BinaryReaderEx br, int version)
        {
            ModelType type = br.GetEnum32<ModelType>(br.Position + 8);
            switch (type)
            {
                case ModelType.MapPiece:
                    return MapPieces.EchoAdd(new Model.MapPiece(br));

                case ModelType.Enemy:
                    return Enemies.EchoAdd(new Model.Enemy(br));

                case ModelType.Player:
                    return Players.EchoAdd(new Model.Player(br));

                case ModelType.Collision:
                    return Collisions.EchoAdd(new Model.Collision(br));

                case ModelType.DummyEnemy:
                    return DummyEnemies.EchoAdd(new Model.DummyEnemy(br));

                case ModelType.DummyAsset:
                    return DummyAssets.EchoAdd(new Model.DummyAsset(br));

                case ModelType.Asset:
                    return Assets.EchoAdd(new Model.Asset(br));

                case ModelType.PrefabAsset:
                    return PrefabAssets.EchoAdd(new Model.PrefabAsset(br));

                default:
                    throw new NotImplementedException($"Unimplemented model type: {type}");
            }
        }
    }

    public abstract class Model : Entry, IMsbModel
    {
        private protected abstract ModelType Type { get; }
        private protected abstract bool HasTypeData { get; }

        public string SibPath { get; set; }

        private int InstanceCount;

        public int Unk1C { get; set; }

        private protected Model(string name)
        {
            Name = name;
            SibPath = "";
        }

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
            Unk1C = br.ReadInt32();
            long typeDataOffset = br.ReadInt64();


            if (!BinaryReaderEx.IgnoreAsserts)
            {
                if (nameOffset == 0)
                    throw new InvalidDataException($"{nameof(nameOffset)} must not be 0 in type {GetType()}.");
                if (sibOffset == 0)
                    throw new InvalidDataException($"{nameof(sibOffset)} must not be 0 in type {GetType()}.");
                if (HasTypeData ^ typeDataOffset != 0)
                    throw new InvalidDataException($"Unexpected {nameof(typeDataOffset)} 0x{typeDataOffset:X} in type {GetType()}.");
            }

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
            bw.WriteInt32(Unk1C);
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

        public override string ToString()
        {
            return $"{Type} {Name}";
        }

        public class MapPiece : Model
        {
            private protected override ModelType Type => ModelType.MapPiece;
            private protected override bool HasTypeData => false;

            public MapPiece() : base("mXXXXXX") { }

            internal MapPiece(BinaryReaderEx br) : base(br) { }
        }

        public class Enemy : Model
        {
            private protected override ModelType Type => ModelType.Enemy;
            private protected override bool HasTypeData => false;

            public Enemy() : base("cXXXX") { }

            internal Enemy(BinaryReaderEx br) : base(br) { }
        }

        public class Player : Model
        {
            private protected override ModelType Type => ModelType.Player;
            private protected override bool HasTypeData => false;

            public Player() : base("c0000") { }

            internal Player(BinaryReaderEx br) : base(br) { }
        }

        public class Collision : Model
        {
            private protected override ModelType Type => ModelType.Collision;
            private protected override bool HasTypeData => false;

            public Collision() : base("hXXXXXX") { }

            internal Collision(BinaryReaderEx br) : base(br) { }
        }

        public class DummyEnemy : Model
        {
            private protected override ModelType Type => ModelType.DummyEnemy;
            private protected override bool HasTypeData => false;

            public DummyEnemy() : base("cXXXX") { }

            internal DummyEnemy(BinaryReaderEx br) : base(br) { }
        }

        public class DummyAsset : Model
        {
            private protected override ModelType Type => ModelType.DummyAsset;
            private protected override bool HasTypeData => false;

            public DummyAsset() : base("AEGxxx_xxx") { }

            internal DummyAsset(BinaryReaderEx br) : base(br) { }
        }

        public class Asset : Model
        {
            private protected override ModelType Type => ModelType.Asset;
            private protected override bool HasTypeData => false;

            public Asset() : base("AEGxxx_xxx") { }

            internal Asset(BinaryReaderEx br) : base(br) { }
        }

        public class PrefabAsset : Model
        {
            private protected override ModelType Type => ModelType.Asset;
            private protected override bool HasTypeData => false;

            public PrefabAsset() : base("AEGxxx_xxx") { }

            internal PrefabAsset(BinaryReaderEx br) : base(br) { }
        }
    }
}