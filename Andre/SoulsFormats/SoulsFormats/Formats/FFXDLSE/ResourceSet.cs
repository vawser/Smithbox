using System.Collections.Generic;

namespace SoulsFormats
{
    public partial class FFXDLSE
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public class ResourceSet : FXSerializable
        {
            internal override string ClassName => "FXResourceSet";

            internal override int Version => 1;

            public List<int> Vector1 { get; set; }

            public List<int> Vector2 { get; set; }

            public List<int> Vector3 { get; set; }

            public List<int> Vector4 { get; set; }

            public List<int> Vector5 { get; set; }

            public ResourceSet()
            {
                Vector1 = new List<int>();
                Vector2 = new List<int>();
                Vector3 = new List<int>();
                Vector4 = new List<int>();
                Vector5 = new List<int>();
            }

            internal ResourceSet(BinaryReaderEx br, List<string> classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, List<string> classNames)
            {
                Vector1 = DLVector.Read(br, classNames);
                Vector2 = DLVector.Read(br, classNames);
                Vector3 = DLVector.Read(br, classNames);
                Vector4 = DLVector.Read(br, classNames);
                Vector5 = DLVector.Read(br, classNames);
            }

            internal override void AddClassNames(List<string> classNames)
            {
                base.AddClassNames(classNames);
                DLVector.AddClassNames(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, List<string> classNames)
            {
                DLVector.Write(bw, classNames, Vector1);
                DLVector.Write(bw, classNames, Vector2);
                DLVector.Write(bw, classNames, Vector3);
                DLVector.Write(bw, classNames, Vector4);
                DLVector.Write(bw, classNames, Vector5);
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
