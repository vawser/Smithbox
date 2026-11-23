namespace SoulsFormats
{
    public partial class FXR1
    {
        public class DS1RExtraNodes
        {
            public FXField DS1R_ColorModR;
            public FXField DS1R_ColorModG;
            public FXField DS1R_ColorModB;
            public FXField DS1R_ColorModA;
            public FXField DS1R_ColorModMult;
            public float Unk6;
            public int Unk7;
            public int Unk8;
            public int Unk9;
            public int Unk10;
            public int Unk11;

            private void ReadInner(BinaryReaderEx br, FxrEnvironment env)
            {
                DS1R_ColorModR = FXField.Read(br, env);
                DS1R_ColorModG = FXField.Read(br, env);
                DS1R_ColorModB = FXField.Read(br, env);
                DS1R_ColorModA = FXField.Read(br, env);
                DS1R_ColorModMult = FXField.Read(br, env);
                Unk6 = br.ReadSingle();
                Unk7 = br.ReadInt32();
                Unk8 = br.ReadInt32();
                Unk9 = br.ReadInt32();
                Unk10 = br.ReadInt32();
                Unk11 = br.ReadInt32();
            }

            internal void Write(BinaryWriterEx bw, FXActionData beh)
            {
                beh.WriteField(DS1R_ColorModR);
                beh.WriteField(DS1R_ColorModG);
                beh.WriteField(DS1R_ColorModB);
                beh.WriteField(DS1R_ColorModA);
                beh.WriteField(DS1R_ColorModMult);
                bw.WriteSingle(Unk6);
                bw.WriteInt32(Unk7);
                bw.WriteInt32(Unk8);
                bw.WriteInt32(Unk9);
                bw.WriteInt32(Unk10);
                bw.WriteInt32(Unk11);
            }

            internal static DS1RExtraNodes Read(BinaryReaderEx br, FxrEnvironment env)
            {
                var p = new DS1RExtraNodes();
                p.ReadInner(br, env);
                return p;
            }
        }
    }
    
}
