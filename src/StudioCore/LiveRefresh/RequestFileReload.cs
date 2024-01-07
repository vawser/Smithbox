using System;
using System.Text;

namespace StudioCore.LiveRefresh;

public class RequestFileReload
{
    public enum ReloadType
    {
        Parts,
        Chr,
        Object
    }

    public static GameType Type;

    internal static long GetReloadPtr()
    {
        if (Type == GameType.DarkSoulsIII)
        {
            var GetReloadPtr_ = IntPtr.Add(Memory.BaseAddress, 0x4768E78);
            GetReloadPtr_ = new IntPtr(Memory.ReadInt64(GetReloadPtr_));
            return GetReloadPtr_;
        }

        return 0;
    }

    public static void RequestReloadParts()
    {
        var PartsPtr = (IntPtr)GetReloadPtr();

        if (Type == GameType.DarkSoulsIII)
        {
            try
            {
                Memory.AttachProc("DarkSoulsIII");

                Memory.WriteFloat(PartsPtr + 0x3048, 10);
                Memory.WriteBoolean(PartsPtr + 0x3044, true);
            }
            finally
            {
                Memory.CloseHandle();
            }
        }
    }

    public static void RequestReload(ReloadType type, string name)
    {
        if (type == ReloadType.Chr)
        {
            RequestReloadChr(name);
        }
        else if (type == ReloadType.Object)
        {
            RequestReloadObj(name);
        }
        else if (type == ReloadType.Parts)
        {
            RequestReloadParts();
        }
    }

    private static void RequestReloadChr(string chrName)
    {
        var chrNameBytes = Encoding.Unicode.GetBytes(chrName);

        if (Type == GameType.DarkSoulsIII)
        {
            try
            {
                Memory.AttachProc("DarkSoulsIII");

                Memory.WriteBoolean(Memory.BaseAddress + 0x4768F7F, true);

                byte[] buffer =
                {
                    0x48, 0xBA, 0, 0, 0, 0, 0, 0, 0, 0, //mov rdx,Alloc
                    0x48, 0xA1, 0x78, 0x8E, 0x76, 0x44, 0x01, 0x00, 0x00, 0x00, //mov rax,[144768E78]
                    0x48, 0x8B, 0xC8, //mov rcx,rax
                    0x49, 0xBE, 0x10, 0x1E, 0x8D, 0x40, 0x01, 0x00, 0x00, 0x00, //mov r14,00000001408D1E10
                    0x48, 0x83, 0xEC, 0x28, //sub rsp,28
                    0x41, 0xFF, 0xD6, //call r14
                    0x48, 0x83, 0xC4, 0x28, //add rsp,28
                    0xC3 //ret
                };

                Memory.ExecuteBufferFunction(buffer, chrNameBytes);
            }
            finally
            {
                Memory.CloseHandle();
            }
        }
    }

    private static void RequestReloadObj(string objName)
    {
        var objNameBytes = Encoding.Unicode.GetBytes(objName);

        if (Type == GameType.DarkSoulsIII)
        {
            try
            {
                Memory.AttachProc("DarkSoulsIII");

                byte[] buffer =
                {
                    0x48, 0xBA, 0, 0, 0, 0, 0, 0, 0, 0, //mov rdx,Alloc
                    0x48, 0xA1, 0xC8, 0x51, 0x74, 0x44, 0x01, 0x00, 0x00, 0x00, //mov rax,[1447451C8]
                    0x48, 0x8B, 0xC8, //mov rcx,rax
                    0x49, 0xBE, 0x10, 0x1E, 0x8D, 0x40, 0x01, 0x00, 0x00, 0x00, //mov r14,000000014067FFF0
                    0x48, 0x83, 0xEC, 0x28, //sub rsp,28
                    0x41, 0xFF, 0xD6, //call r14
                    0x48, 0x83, 0xC4, 0x28, //add rsp,28
                    0xC3 //ret
                };

                Memory.ExecuteBufferFunction(buffer, objNameBytes);
            }
            finally
            {
                Memory.CloseHandle();
            }
        }
    }
}
