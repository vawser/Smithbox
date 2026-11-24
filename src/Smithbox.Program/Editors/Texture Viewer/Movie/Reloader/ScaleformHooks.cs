using Iced.Intel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Iced.Intel.AssemblerRegisters;

namespace StudioCore.Editors.TextureViewer;

/// <summary>
/// The Scaleform Reloader's code in the game (a native port of gfx-reloader's detours, MIT, lugia19), and its
/// control block, which Smithbox reads and writes from outside. One allocation near the exe:
///   page 0: header (magic, control block address), three trampolines, the hook code (assembled with Iced);
///   page 1: the control block.
/// The hooks are never taken out; the Enabled flag turns them off. See Claude/Documentation/Scaleform.md §5 Scaleform Reloader.
/// </summary>
public static class ScaleformHooks
{
    public const ulong Magic = 0x314C525846474253; // "SBGFXRL1"

    // Page 0
    public const int HeaderMagic = 0x00;
    public const int HeaderControl = 0x08;
    private const int TrampolineStep = 0x40;
    private const int TrampolineOpen = 0x80;
    private const int TrampolineTime = 0xC0;
    private const int CodeStart = 0x100;
    public const int AllocationSize = 0x2000;
    public const int ControlOffset = 0x1000;

    // Control block
    public const int Enabled = 0x00;          // u32: 0 makes every hook pass through
    public const int Heartbeat = 0x04;        // u32: +1 per window step
    public const int Round = 0x08;            // u64: added (<< 32) to mod files' modify time
    public const int ReleaseFlag = 0x10;      // u32: set by Smithbox, cleared once the list is released (game thread)
    public const int ReleaseCount = 0x14;     // u32
    public const int ReleaseList = 0x18;      // u64[32]: addresses of cache node values
    public const int TargetCount = 0x118;     // u32
    public const int TargetList = 0x120;      // u64[32]: windows to reload; a reloaded entry is set to 0
    public const int SurveyFlag = 0x220;      // u32: record every stepping window
    public const int SurveyIndex = 0x224;     // u32
    public const int SurveyList = 0x228;      // u64[256] ring
    public const int ReloadedCount = 0xA28;   // u32
    public const int ReloadedList = 0xA30;    // u64[32]
    public const int MaxList = 32;
    public const int SurveySize = 256;

    public sealed class Install
    {
        public ulong Allocation;
        public ulong Control;
        public bool Reused;
    }

    /// <summary>
    /// Finds hooks a previous Smithbox session installed (the window step starts with a jmp into a page with the magic),
    /// or installs them. Every function's prologue is checked first (a jmp from another tool is accepted).
    /// </summary>
    public static Install InstallOrFind(ScaleformProcess process, ulong imageBase, ulong imageSize, ScaleformReloadOffsets offsets)
    {
        var stepSite = imageBase + offsets.WindowStep.RvaValue;

        var existing = FindExisting(process, stepSite);
        if (existing != 0)
            return new Install { Allocation = existing, Control = process.U64(existing + HeaderControl), Reused = true };

        foreach (var (name, code) in offsets.Functions())
        {
            var expected = code.PrologueBytes;
            var actual = process.Read(imageBase + code.RvaValue, expected.Length);
            if (actual[0] != 0xE9 && !actual.AsSpan().SequenceEqual(expected))
                throw new InvalidDataException($"{name} at {imageBase + code.RvaValue:X} is not the expected code (not {offsets.Game}?)");
        }

        var allocation = process.AllocateNear(imageBase, imageSize, AllocationSize);
        var (page, patches) = Build(process.Read, imageBase, offsets, allocation);

        // Page 0 and the control block, before anything jumps there
        process.Write(allocation, page);
        var block = new byte[0x1000];
        BitConverter.GetBytes(1u).CopyTo(block, Enabled);
        process.Write(allocation + ControlOffset, block);

        process.PatchWithThreadsSuspended(patches);

        return new Install { Allocation = allocation, Control = allocation + ControlOffset, Reused = false };
    }

    /// <summary>
    /// Page 0 (header, trampolines, hook code) for an allocation, and the jumps to write at the three sites. Reads the
    /// sites' code with 'read' (the game, or an exe image for an offline check: GfxReloaderCheck).
    /// </summary>
    public static (byte[] Page, List<(ulong Address, byte[] Bytes)> Patches) Build(Func<ulong, int, byte[]> read, ulong imageBase,
        ScaleformReloadOffsets offsets, ulong allocation)
    {
        var control = allocation + ControlOffset;
        var stepSite = imageBase + offsets.WindowStep.RvaValue;
        var openSite = imageBase + offsets.FileOpen.RvaValue;
        var timeSite = imageBase + offsets.ModifyTime.RvaValue;

        var (stepTramp, stepStolen) = Trampoline(read, stepSite, allocation + TrampolineStep);
        var (openTramp, openStolen) = Trampoline(read, openSite, allocation + TrampolineOpen);
        var (timeTramp, timeStolen) = Trampoline(read, timeSite, allocation + TrampolineTime);

        var code = Assemble(allocation + CodeStart, control, offsets, imageBase,
            allocation + TrampolineStep, allocation + TrampolineOpen, allocation + TrampolineTime,
            out var stepEntry, out var openEntry, out var timeEntry);

        if (CodeStart + code.Length > ControlOffset)
            throw new InvalidOperationException("The hook code does not fit its page");

        var page = new byte[CodeStart + code.Length];
        BitConverter.GetBytes(Magic).CopyTo(page, HeaderMagic);
        BitConverter.GetBytes(control).CopyTo(page, HeaderControl);
        stepTramp.CopyTo(page, TrampolineStep);
        openTramp.CopyTo(page, TrampolineOpen);
        timeTramp.CopyTo(page, TrampolineTime);
        code.CopyTo(page, CodeStart);

        return (page,
        [
            (stepSite, Jump(stepSite, stepEntry, stepStolen)),
            (openSite, Jump(openSite, openEntry, openStolen)),
            (timeSite, Jump(timeSite, timeEntry, timeStolen)),
        ]);
    }

    /// <summary>The offsets of the parts of page 0, for the offline check.</summary>
    public static (int StepTrampoline, int OpenTrampoline, int TimeTrampoline, int Code) PageLayout => (TrampolineStep, TrampolineOpen, TrampolineTime, CodeStart);

    private static ulong FindExisting(ScaleformProcess process, ulong site)
    {
        var bytes = process.Read(site, 5);
        if (bytes[0] != 0xE9)
            return 0;

        var target = (ulong)((long)site + 5 + BitConverter.ToInt32(bytes, 1));
        var page = target & ~0xFFFUL;
        return process.TryU64(page + HeaderMagic) == Magic ? page : 0;
    }

    /// <summary>jmp rel32 to the hook, padded with nops to the stolen length.</summary>
    private static byte[] Jump(ulong site, ulong target, int stolen)
    {
        var bytes = Enumerable.Repeat((byte)0x90, stolen).ToArray();
        bytes[0] = 0xE9;
        BitConverter.GetBytes((int)((long)target - (long)(site + 5))).CopyTo(bytes, 1);
        return bytes;
    }

    /// <summary>
    /// The site's first whole instructions covering 5 bytes, relocated to rip (RIP-relative operands and branches are
    /// fixed by Iced), then a jmp back after them.
    /// </summary>
    private static (byte[] Code, int Stolen) Trampoline(Func<ulong, int, byte[]> read, ulong site, ulong rip)
    {
        var bytes = read(site, 32);
        var decoder = Iced.Intel.Decoder.Create(64, new ByteArrayCodeReader(bytes), site);
        var instructions = new List<Instruction>();
        int stolen = 0;
        while (stolen < 5)
        {
            var ins = decoder.Decode();
            if (ins.IsInvalid)
                throw new InvalidDataException($"Undecodable code at {ins.IP:X}");
            instructions.Add(ins);
            stolen += ins.Length;
        }
        instructions.Add(Instruction.CreateBranch(Code.Jmp_rel32_64, site + (ulong)stolen));

        var writer = new CodeWriterImpl();
        if (!BlockEncoder.TryEncode(64, new InstructionBlock(writer, instructions, rip), out var error, out _))
            throw new InvalidOperationException($"Trampoline for {site:X}: {error}");

        var code = writer.ToArray();
        if (code.Length > 0x40)
            throw new InvalidOperationException($"Trampoline for {site:X} is too long");
        return (code, stolen);
    }

    private sealed class CodeWriterImpl : CodeWriter
    {
        private readonly List<byte> Bytes = new();
        public override void WriteByte(byte value) => Bytes.Add(value);
        public byte[] ToArray() => Bytes.ToArray();
    }

    /// <summary>
    /// The three hooks. Arguments are the Windows x64 ones (rcx, rdx, r8, r9); each hook keeps them for the original.
    /// </summary>
    private static byte[] Assemble(ulong rip, ulong control, ScaleformReloadOffsets o, ulong imageBase,
        ulong trampStep, ulong trampOpen, ulong trampTime, out ulong stepEntry, out ulong openEntry, out ulong timeEntry)
    {
        var windowClose = imageBase + o.WindowClose.RvaValue;
        var defaultOpen = imageBase + o.DefaultOpen.RvaValue;
        var controller = (int)ScaleformReloadOffsets.Hex(o.WindowController);
        var factory = (int)ScaleformReloadOffsets.Hex(o.WindowFactory);
        var closeCheck = (int)ScaleformReloadOffsets.Hex(o.WindowCloseCheck);

        var a = new Assembler(64);

        var step = a.CreateLabel();
        var open = a.CreateLabel();
        var time = a.CreateLabel();
        var isArchive = a.CreateLabel();

        #region Window step (window, out, p3, p4): game thread, every live window, every frame
        var stepOriginal = a.CreateLabel();
        var stepNoSurvey = a.CreateLabel();
        var stepTargets = a.CreateLabel();
        var stepLoop = a.CreateLabel();
        var stepFound = a.CreateLabel();
        var stepRelease = a.CreateLabel();
        var releaseLoop = a.CreateLabel();
        var releaseNext = a.CreateLabel();
        var releaseDone = a.CreateLabel();
        var skipRecord = a.CreateLabel();

        a.Label(ref step);
        a.mov(rax, control);
        a.inc(__dword_ptr[rax + Heartbeat]);
        a.cmp(__dword_ptr[rax + Enabled], 0);
        a.je(stepOriginal);

        // Survey: record the window in the ring
        a.cmp(__dword_ptr[rax + SurveyFlag], 0);
        a.je(stepNoSurvey);
        a.mov(r10d, __dword_ptr[rax + SurveyIndex]);
        a.and(r10d, SurveySize - 1);
        a.mov(__qword_ptr[rax + r10 * 8 + SurveyList], rcx);
        a.inc(__dword_ptr[rax + SurveyIndex]);
        a.Label(ref stepNoSurvey);

        // Release the listed cached movies first (once)
        a.cmp(__dword_ptr[rax + ReleaseFlag], 0);
        a.jne(stepRelease);

        // A window to reload?
        a.Label(ref stepTargets);
        a.mov(r10d, __dword_ptr[rax + TargetCount]);
        a.test(r10d, r10d);
        a.jz(stepOriginal);
        a.xor(r11d, r11d);
        a.Label(ref stepLoop);
        a.cmp(__qword_ptr[rax + r11 * 8 + TargetList], rcx);
        a.je(stepFound);
        a.inc(r11d);
        a.cmp(r11d, r10d);
        a.jb(stepLoop);
        a.jmp(stepOriginal);

        a.Label(ref stepFound);
        a.mov(__qword_ptr[rax + r11 * 8 + TargetList], 0);
        // Only a live window (controller and factory set) is reopened by the step
        a.cmp(__qword_ptr[rcx + controller], 0);
        a.je(stepOriginal);
        a.cmp(__qword_ptr[rcx + factory], 0);
        a.je(stepOriginal);

        a.push(rbx); a.push(rsi); a.push(rdi); a.push(r12); a.push(r13); a.push(r14);
        a.sub(rsp, 0x28);
        a.mov(rbx, rcx); a.mov(rsi, rdx); a.mov(rdi, r8); a.mov(r12, r9);
        // The close destroys the close check, which only the creator sets: keep it out of the close
        a.mov(r13, __qword_ptr[rbx + closeCheck]);
        a.mov(__qword_ptr[rbx + closeCheck], 0);
        a.mov(rcx, rbx); a.xor(edx, edx); a.xor(r8d, r8d); a.xor(r9d, r9d);
        a.mov(rax, windowClose);
        a.call(rax);
        a.mov(__qword_ptr[rbx + closeCheck], r13);
        a.mov(rax, control);
        a.mov(r10d, __dword_ptr[rax + ReloadedCount]);
        a.cmp(r10d, MaxList);
        a.jae(skipRecord);
        a.mov(__qword_ptr[rax + r10 * 8 + ReloadedList], rbx);
        a.inc(__dword_ptr[rax + ReloadedCount]);
        a.Label(ref skipRecord);
        // The original step reopens it (no controller + factory = open)
        a.mov(rcx, rbx); a.mov(rdx, rsi); a.mov(r8, rdi); a.mov(r9, r12);
        a.mov(rax, trampStep);
        a.call(rax);
        a.add(rsp, 0x28);
        a.pop(r14); a.pop(r13); a.pop(r12); a.pop(rdi); a.pop(rsi); a.pop(rbx);
        a.ret();

        a.Label(ref stepRelease);
        a.push(rbx); a.push(rsi); a.push(rdi); a.push(r12); a.push(r13); a.push(r14);
        a.sub(rsp, 0x28);
        a.mov(rbx, rcx); a.mov(rsi, rdx); a.mov(rdi, r8); a.mov(r12, r9);
        a.mov(r13, control);
        a.xor(r14d, r14d);
        a.Label(ref releaseLoop);
        a.cmp(r14d, __dword_ptr[r13 + ReleaseCount]);
        a.jae(releaseDone);
        a.mov(rax, __qword_ptr[r13 + r14 * 8 + ReleaseList]);
        a.test(rax, rax);
        a.jz(releaseNext);
        a.mov(rcx, __qword_ptr[rax]);
        a.test(rcx, rcx);
        a.jz(releaseNext);
        // Take the cache's reference: Scaleform frees the movie when the last one goes
        a.mov(__qword_ptr[rax], 0);
        a.@lock.dec(__dword_ptr[rcx + 8]);
        a.jnz(releaseNext);
        a.mov(rax, __qword_ptr[rcx]);
        a.call(__qword_ptr[rax]);
        a.Label(ref releaseNext);
        a.inc(r14d);
        a.jmp(releaseLoop);
        a.Label(ref releaseDone);
        a.mov(__dword_ptr[r13 + ReleaseFlag], 0);
        a.mov(rcx, rbx); a.mov(rdx, rsi); a.mov(r8, rdi); a.mov(r9, r12);
        a.add(rsp, 0x28);
        a.pop(r14); a.pop(r13); a.pop(r12); a.pop(rdi); a.pop(rsi); a.pop(rbx);
        a.mov(rax, control);
        a.jmp(stepTargets);

        a.Label(ref stepOriginal);
        a.mov(rax, trampStep);
        a.jmp(rax);
        #endregion

        #region File open (opener, utf-8 url, flags, mode): mod files from disk
        var openOriginal = a.CreateLabel();
        var openFallback = a.CreateLabel();

        a.Label(ref open);
        a.mov(rax, control);
        a.cmp(__dword_ptr[rax + Enabled], 0);
        a.je(openOriginal);
        a.test(rdx, rdx);
        a.jz(openOriginal);
        a.call(isArchive);
        a.test(eax, eax);
        a.jnz(openOriginal);
        a.push(rbx); a.push(rsi); a.push(rdi); a.push(r12); a.push(r13);
        a.sub(rsp, 0x20);
        a.mov(rbx, rcx); a.mov(rsi, rdx); a.mov(rdi, r8); a.mov(r12, r9);
        a.mov(rax, defaultOpen);
        a.call(rax);
        a.test(rax, rax);
        a.jz(openFallback);
        a.add(rsp, 0x20);
        a.pop(r13); a.pop(r12); a.pop(rdi); a.pop(rsi); a.pop(rbx);
        a.ret();
        // Not readable from disk: the game's own (boot copy)
        a.Label(ref openFallback);
        a.mov(rcx, rbx); a.mov(rdx, rsi); a.mov(r8, rdi); a.mov(r9, r12);
        a.add(rsp, 0x20);
        a.pop(r13); a.pop(r12); a.pop(rdi); a.pop(rsi); a.pop(rbx);
        a.Label(ref openOriginal);
        a.mov(rax, trampOpen);
        a.jmp(rax);
        #endregion

        #region Modify time (opener, utf-8 url) -> time: mod files get the round added
        var timeReturn = a.CreateLabel();

        a.Label(ref time);
        a.push(rbx); a.push(rsi);
        a.sub(rsp, 0x28);
        a.mov(rbx, rdx);
        a.mov(rax, trampTime);
        a.call(rax);
        a.mov(rsi, rax);
        a.test(rbx, rbx);
        a.jz(timeReturn);
        a.mov(rax, control);
        a.cmp(__dword_ptr[rax + Enabled], 0);
        a.je(timeReturn);
        a.mov(rdx, rbx);
        a.call(isArchive);
        a.test(eax, eax);
        a.jnz(timeReturn);
        a.mov(rax, control);
        a.mov(rax, __qword_ptr[rax + Round]);
        a.shl(rax, 32);
        a.add(rsi, rax);
        a.Label(ref timeReturn);
        a.mov(rax, rsi);
        a.add(rsp, 0x28);
        a.pop(rsi); a.pop(rbx);
        a.ret();
        #endregion

        #region isArchive(rdx = utf-8 path) -> eax: "root:/..." with a root of 2+ letters or digits (data0:/menu/..)
        // me3 renames overridden files (\\me3??..) and a drive has one letter: those are not archive paths.
        // Clobbers rax, r10, r11.
        var scan = a.CreateLabel();
        var colon = a.CreateLabel();
        var alnum = a.CreateLabel();
        var notArchive = a.CreateLabel();

        a.Label(ref isArchive);
        a.xor(eax, eax);
        a.Label(ref scan);
        a.movzx(r10d, __byte_ptr[rdx + rax]);
        a.cmp(r10d, ':');
        a.je(colon);
        a.test(r10d, r10d);
        a.jz(notArchive);
        a.lea(r11d, __[r10 - '0']);
        a.cmp(r11d, 9);
        a.jbe(alnum);
        a.or(r10d, 0x20);
        a.lea(r11d, __[r10 - 'a']);
        a.cmp(r11d, 25);
        a.jbe(alnum);
        a.jmp(notArchive);
        a.Label(ref alnum);
        a.inc(eax);
        a.cmp(eax, 32);
        a.jb(scan);
        a.jmp(notArchive);
        a.Label(ref colon);
        a.cmp(eax, 2);
        a.jb(notArchive);
        a.cmp(__byte_ptr[rdx + rax + 1], '/');
        a.jne(notArchive);
        a.mov(eax, 1);
        a.ret();
        a.Label(ref notArchive);
        a.xor(eax, eax);
        a.ret();
        #endregion

        var writer = new CodeWriterImpl();
        var result = a.Assemble(writer, rip, BlockEncoderOptions.ReturnNewInstructionOffsets);
        stepEntry = result.GetLabelRIP(step);
        openEntry = result.GetLabelRIP(open);
        timeEntry = result.GetLabelRIP(time);
        return writer.ToArray();
    }
}
