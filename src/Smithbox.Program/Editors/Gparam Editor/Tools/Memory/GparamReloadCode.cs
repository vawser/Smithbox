namespace StudioCore.Editors.GparamEditor;

/// <summary>
/// Builds the thread code the Gparam Reloader runs inside the game. Kept free of Smithbox types so
/// Claude/Tools/GparamReloadAob can compile it and disassemble the result.
///
/// The thread turns the GPARAM's IDs off, runs Init again on the same ResCap (a new GX handle), and only then replays
/// the release half of the CSGparamResCap destructor on the old handle and bytes, then turns the same IDs back on, so
/// the game's own state (the ResCap, its ID lists, the lerper's view of it) stays as it was. Parsing before releasing
/// keeps anything both copies use (textures) loaded throughout.
/// It returns 1 when reloaded, 2 when Init failed to parse (the ResCap is left with no data), 0 when the ResCap
/// changed since Smithbox read it (nothing done).
///
/// The bytes must be a plain (uncompressed) GPARAM. Init copies them, so the buffer can be freed once the thread returns.
/// </summary>
public static class GparamReloadCode
{
    /// <summary>Game addresses (absolute, in the running game) and ResCap field offsets</summary>
    public record Addresses(
        long RepositoryPointer, long Find, long Init, long GxGlobal, long Activate, long Deactivate, long Release,
        long GetAllocator, int AllocatorFree, int GxGparamOffset, int GxManagerOffset,
        int HandleOffset, int ActiveOffset, int SlotOffset, int ActivateParamsOffset, int BytesOffset);

    /// <summary>What Smithbox read from the ResCap just before the reload</summary>
    public record Snapshot(long ResCap, long Handle, bool Active, int Slot, int MainId, IReadOnlyList<int> ListIds);

    /// <param name="name">UTF-16 name buffer in the game, for the Find check</param>
    /// <param name="data">GPARAM bytes buffer in the game</param>
    public static byte[] Build(Addresses a, Snapshot s, long name, long data, int size)
    {
        var e = new Emitter();

        // Prologue: keep the non-volatile registers used; rsp is 16-aligned after 5 pushes and sub 40h
        e.B(0x53);                                    // push rbx
        e.B(0x41, 0x54);                              // push r12
        e.B(0x41, 0x55);                              // push r13
        e.B(0x41, 0x56);                              // push r14
        e.B(0x41, 0x57);                              // push r15
        e.B(0x48, 0x83, 0xEC, 0x40);                  // sub rsp,40h

        // rc = Find([repo], name); it must still be the ResCap that was read
        e.MovRaxImm(a.RepositoryPointer);
        e.B(0x48, 0x8B, 0x08);                        // mov rcx,[rax]
        e.B(0x48, 0x85, 0xC9);                        // test rcx,rcx
        e.Jcc(0x84, "fail");                          // je fail
        e.B(0x48, 0xBA); e.I64(name);                 // mov rdx,name
        e.CallAbs(a.Find);
        e.B(0x48, 0xBB); e.I64(s.ResCap);             // mov rbx,rc
        e.B(0x48, 0x39, 0xD8);                        // cmp rax,rbx
        e.Jcc(0x85, "fail");                          // jne fail

        // r13 = the handle; it must still be the one read
        e.B(0x4C, 0x8B, 0xAB); e.I32(a.HandleOffset); // mov r13,[rbx+handle]
        e.MovRaxImm(s.Handle);
        e.B(0x49, 0x39, 0xC5);                        // cmp r13,rax
        e.Jcc(0x85, "fail");                          // jne fail

        // r12 = [gx] + gparam offset (first argument of Activate / Deactivate)
        e.MovRaxImm(a.GxGlobal);
        e.B(0x4C, 0x8B, 0x20);                        // mov r12,[rax]
        e.B(0x49, 0x81, 0xC4); e.I32(a.GxGparamOffset); // add r12,gxGparamOffset

        // Turn off every active ID on the old handle, at once (fade 0), as the destructor does
        foreach (var id in s.ListIds)
            Deactivate(e, a, s, id);

        if (s.Active)
            Deactivate(e, a, s, s.MainId);

        // r14 = the old kept copy of the bytes. Detach it and the old handle from the ResCap, and set "active" so
        // Init doesn't turn the GPARAM on by itself: the IDs are restored below
        e.B(0x4C, 0x8B, 0xB3); e.I32(a.BytesOffset);  // mov r14,[rbx+bytes]
        e.B(0x48, 0xC7, 0x83); e.I32(a.BytesOffset); e.I32(0);   // mov qword [rbx+bytes],0
        e.B(0x48, 0xC7, 0x83); e.I32(a.HandleOffset); e.I32(-1); // mov qword [rbx+handle],-1
        e.B(0xC6, 0x83); e.I32(a.ActiveOffset); e.B(1);          // mov byte [rbx+active],1

        // Init(rc, data, size): a new handle (and a new kept copy) while the old one is still alive
        e.B(0x48, 0x89, 0xD9);                        // mov rcx,rbx
        e.B(0x48, 0xBA); e.I64(data);                 // mov rdx,data
        e.B(0x49, 0xB8); e.I64(size);                 // mov r8,size
        e.CallAbs(a.Init);

        // Put back the slot and active flag the game had; r15 = the new handle (-1 when the bytes didn't parse)
        e.B(0xC7, 0x83); e.I32(a.SlotOffset); e.I32(s.Slot);           // mov dword [rbx+slot],slot
        e.B(0xC6, 0x83); e.I32(a.ActiveOffset); e.B(s.Active ? (byte)1 : (byte)0); // mov byte [rbx+active],active
        e.B(0x4C, 0x8B, 0xBB); e.I32(a.HandleOffset); // mov r15,[rbx+handle]

        // Release(manager, oldHandle)
        e.MovRaxImm(a.GxGlobal);
        e.B(0x48, 0x8B, 0x00);                        // mov rax,[rax]
        e.B(0x48, 0x8B, 0x88); e.I32(a.GxManagerOffset); // mov rcx,[rax+manager]
        e.B(0x4C, 0x89, 0xEA);                        // mov rdx,r13
        e.CallAbs(a.Release);

        // Free the old kept copy of the bytes: allocator = GetAllocator(p); allocator->Free(p)
        e.B(0x4D, 0x85, 0xF6);                        // test r14,r14
        e.Jcc(0x84, "noBytes");                       // je noBytes
        e.B(0x4C, 0x89, 0xF1);                        // mov rcx,r14
        e.CallAbs(a.GetAllocator);
        e.B(0x48, 0x89, 0xC1);                        // mov rcx,rax
        e.B(0x4C, 0x89, 0xF2);                        // mov rdx,r14
        e.B(0x4C, 0x8B, 0x00);                        // mov r8,[rax]
        e.B(0x41, 0xFF, 0x90); e.I32(a.AllocatorFree); // call [r8+free]
        e.Label("noBytes");

        e.B(0x49, 0x83, 0xFF, 0xFF);                  // cmp r15,-1
        e.Jcc(0x84, "initFailed");                    // je initFailed

        // Turn the same IDs back on, on the new handle, at once
        e.B(0x4D, 0x89, 0xFD);                        // mov r13,r15
        if (s.Active)
            Activate(e, a, s, s.MainId);

        foreach (var id in s.ListIds)
            Activate(e, a, s, id);

        e.B(0xB8); e.I32(1);                          // mov eax,1
        e.Jmp("done");

        e.Label("initFailed");
        e.B(0xB8); e.I32(2);                          // mov eax,2
        e.Jmp("done");

        e.Label("fail");
        e.B(0x31, 0xC0);                              // xor eax,eax

        e.Label("done");
        e.B(0x48, 0x83, 0xC4, 0x40);                  // add rsp,40h
        e.B(0x41, 0x5F);                              // pop r15
        e.B(0x41, 0x5E);                              // pop r14
        e.B(0x41, 0x5D);                              // pop r13
        e.B(0x41, 0x5C);                              // pop r12
        e.B(0x5B);                                    // pop rbx
        e.B(0xC3);                                    // ret

        return e.Finish();
    }

    /// <summary>Deactivate(r12, r13, id, slot, fade 0.0)</summary>
    private static void Deactivate(Emitter e, Addresses a, Snapshot s, int id)
    {
        e.B(0x4C, 0x89, 0xE1);                        // mov rcx,r12
        e.B(0x4C, 0x89, 0xEA);                        // mov rdx,r13
        e.B(0x41, 0xB8); e.I32(id);                   // mov r8d,id
        e.B(0x41, 0xB9); e.I32(s.Slot);               // mov r9d,slot
        e.B(0xC7, 0x44, 0x24, 0x20); e.I32(0);        // mov dword [rsp+20h],0 (0.0f)
        e.CallAbs(a.Deactivate);
    }

    /// <summary>Activate(r12, r13, id, slot, rc + params, fade 0.0, instant)</summary>
    private static void Activate(Emitter e, Addresses a, Snapshot s, int id)
    {
        e.B(0x48, 0x8D, 0x83); e.I32(a.ActivateParamsOffset); // lea rax,[rbx+params]
        e.B(0x48, 0x89, 0x44, 0x24, 0x20);            // mov [rsp+20h],rax
        e.B(0xC7, 0x44, 0x24, 0x28); e.I32(0);        // mov dword [rsp+28h],0 (0.0f)
        e.B(0xC6, 0x44, 0x24, 0x30, 0x01);            // mov byte [rsp+30h],1
        e.B(0x4C, 0x89, 0xE1);                        // mov rcx,r12
        e.B(0x4C, 0x89, 0xEA);                        // mov rdx,r13
        e.B(0x41, 0xB8); e.I32(id);                   // mov r8d,id
        e.B(0x41, 0xB9); e.I32(s.Slot);               // mov r9d,slot
        e.CallAbs(a.Activate);
    }

    /// <summary>A byte list with rel32 jumps to labels, patched at the end</summary>
    private class Emitter
    {
        private readonly List<byte> Code = new();
        private readonly Dictionary<string, int> Labels = new();
        private readonly List<(int At, string Label)> Fixups = new();

        public void B(params byte[] b) => Code.AddRange(b);
        public void I32(int v) => Code.AddRange(BitConverter.GetBytes(v));
        public void I64(long v) => Code.AddRange(BitConverter.GetBytes(v));

        public void MovRaxImm(long v) { B(0x48, 0xB8); I64(v); }
        public void CallAbs(long target) { MovRaxImm(target); B(0xFF, 0xD0); } // mov rax,target; call rax

        public void Jcc(byte cc, string label) { B(0x0F, cc); Fixups.Add((Code.Count, label)); I32(0); }
        public void Jmp(string label) { B(0xE9); Fixups.Add((Code.Count, label)); I32(0); }
        public void Label(string label) => Labels[label] = Code.Count;

        public byte[] Finish()
        {
            var code = Code.ToArray();

            foreach (var (at, label) in Fixups)
                BitConverter.GetBytes(Labels[label] - (at + 4)).CopyTo(code, at);

            return code;
        }
    }
}
