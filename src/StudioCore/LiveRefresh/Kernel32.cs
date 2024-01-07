using System;
using System.Runtime.InteropServices;

namespace StudioCore.LiveRefresh;

public class Kernel32
{
    [DllImport("kernel32.dll")]
    public static extern IntPtr VirtualAlloc(IntPtr lpAddress, int dwSize, int flAllocationType, int flProtect);

    [DllImport("kernel32.dll")]
    public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, int flAllocationType,
        int flProtect);

    [DllImport("kernel32.dll")]
    public static extern bool VirtualFree(IntPtr lpAddress, int dwSize, int dwFreeType);

    [DllImport("kernel32.dll")]
    public static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, int dwFreeType);

    [DllImport("kernel32.dll")]
    public static extern int WaitForSingleObject(IntPtr hObject, uint dwMilliseconds);

    [DllImport("kernel32")]
    public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize,
        IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out uint lpThreadId);

    [DllImport("kernel32.dll")]
    public static extern bool OpenProcessToken(IntPtr hProcess, uint DesiredAccess, ref IntPtr TokenHandle);

    [DllImport("kernel32.dll")]
    public static extern int GetProcessId(IntPtr hProcess);

    [DllImport("kernel32.dll")]
    public static extern bool DuplicateHandle(IntPtr hSourceProcessHandle, IntPtr hSourceHandle,
        IntPtr TargetProcessHandle, ref IntPtr lpTargetHandle, int dwDesiredAccess, bool bInherithandle,
        int dwOptions);

    [DllImport("kernel32.dll")]
    public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

    [DllImport("kernel32.dll")]
    public static extern IntPtr GetModuleHandleA(string lpModuleName);

    [DllImport("kernel32.dll")]
    public static extern void DeleteProcThreadAttributeList(IntPtr lpAttributeList);

    [DllImport("kernel32.dll")]
    public static extern bool InitializeProcThreadAttributeList(IntPtr lpAttributeList, int dwAttributeCount,
        int dwFlags, ref IntPtr lpSize);

    [DllImport("kernel32.dll")]
    public static extern bool UpdateProcThreadAttribute(IntPtr lpAttributeList, uint dwFlags, IntPtr Attribute,
        ref IntPtr lpValue, IntPtr cbSize, IntPtr lpPreviousValue, IntPtr lpReturnSize);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer,
        UIntPtr nSize, [Out] UIntPtr lpNumberOfBytesRead);
    //public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, UIntPtr nSize, [Out] UIntPtr lpNumberOfBytesRead);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer,
        UIntPtr nSize, [Out] UIntPtr lpNumberOfBytesWritten);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool CloseHandle(IntPtr handle);
}
