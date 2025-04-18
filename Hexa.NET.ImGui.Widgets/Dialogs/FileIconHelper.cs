namespace Hexa.NET.ImGui.Widgets.Dialogs
{
    using Hexa.NET.ImGui.Widgets.Imaging;
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;

    public unsafe class FileIconHelper
    {
        public static byte* GetIcon(string file, int size, bool large)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return GetFileIconPixelDataWin32(file, size, large);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return GetFileIconPixelDataOSX(file, size);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return GetFileIconPixelDataLinux(file, size);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        #region WIN32

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, SHFILEINFO* psfi, uint cbFileInfo, uint uFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("user32.dll", EntryPoint = "GetDC")]
        private static extern nint GetDC(nint hWnd);

        [DllImport("user32.dll", EntryPoint = "ReleaseDC")]
        private static extern int ReleaseDC(nint hWnd, nint hDC);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool GetIconInfo(IntPtr hIcon, ICONINFO* piconinfo);

        [DllImport("gdi32.dll")]
        private static extern int GetDIBits(IntPtr hdc, IntPtr hbmp, uint uStartScan, uint cScanLines, byte* lpvBits, BITMAPINFO* lpbi, uint uUsage);

        [DllImport("gdi32.dll")]
        private static extern int GetObject(nint hObject, int nCount, void* lpObject);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHFILEINFO
        {
            public nint hIcon;
            public int iIcon;
            public uint dwAttributes;
            public fixed byte szDisplayName[260];
            public fixed byte szTypeName[80];
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ICONINFO
        {
            public bool fIcon;
            public int xHotspot;
            public int yHotspot;
            public nint hbmMask;
            public nint hbmColor;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BITMAP
        {
            public int bmType;
            public int bmWidth;
            public int bmHeight;
            public int bmWidthBytes;
            public ushort bmPlanes;
            public ushort bmBitsPixel;
            public IntPtr bmBits;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BITMAPINFOHEADER
        {
            public uint biSize;
            public int biWidth;
            public int biHeight;
            public ushort biPlanes;
            public ushort biBitCount;
            public uint biCompression;
            public uint biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public uint biClrUsed;
            public uint biClrImportant;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BITMAPINFO
        {
            public BITMAPINFOHEADER bmiHeader;
            public uint bmiColors;
        }

        private const uint SHGFI_ICON = 0x100;
        private const uint SHGFI_LARGEICON = 0x0; // Large icon
        private const uint SHGFI_SMALLICON = 0x1; // Small icon
        private const uint DIB_RGB_COLORS = 0;

        private static byte* GetFileIconPixelDataWin32(string filePath, int size, bool largeIcon)
        {
            SHFILEINFO shinfo;
            uint flags = SHGFI_ICON | (largeIcon ? SHGFI_LARGEICON : SHGFI_SMALLICON);
            SHGetFileInfo(filePath, 0, &shinfo, (uint)sizeof(SHFILEINFO), flags);

            nint hIcon = shinfo.hIcon;
            if (hIcon == IntPtr.Zero)
            {
                return null;
            }

            ICONINFO iconInfo;
            GetIconInfo(hIcon, &iconInfo);

            BITMAP bitmap;
            int result = GetObject(iconInfo.hbmColor, sizeof(BITMAP), &bitmap);
            if (result == 0)
            {
                Marshal.ThrowExceptionForHR(result);
            }

            BITMAPINFO info;
            info.bmiHeader.biSize = (uint)sizeof(BITMAPINFOHEADER);

            nint hdc = GetDC(0);

            result = GetDIBits(hdc, iconInfo.hbmColor, 0, (uint)bitmap.bmHeight, null, &info, DIB_RGB_COLORS);
            if (result == 0)
            {
                Marshal.ThrowExceptionForHR(result);
            }

            var header = info.bmiHeader;
            int width = header.biWidth;
            int height = header.biHeight;
            int bytesPerPixel = (header.biBitCount + 7) / 8;
            int bytesPerRow = width * bytesPerPixel;

            byte* pixelData = AllocT<byte>(bytesPerRow * height);

            info.bmiHeader.biHeight = -info.bmiHeader.biHeight;
            info.bmiHeader.biCompression = 0x0000;
            info.bmiHeader.biBitCount = 32;
            result = GetDIBits(hdc, iconInfo.hbmColor, 0, (uint)height, pixelData, &info, DIB_RGB_COLORS);
            if (result != 0)
            {
                Marshal.ThrowExceptionForHR(result);
            }

            ReleaseDC(0, hdc);

            HexaImage image = new();
            image.Width = width;
            image.Height = height;
            image.Pixels = pixelData;
            image.Format = Convert(bitmap);

            image.ConvertFormat(PixelFormat.RGBA8UNorm);
            //image.Resize(size, size);

            DeleteObject(iconInfo.hbmColor);
            DeleteObject(iconInfo.hbmMask);
            DestroyIcon(hIcon);

            return pixelData;
        }

        private static PixelFormat Convert(BITMAP bitmap)
        {
            switch (bitmap.bmBitsPixel)
            {
                case 32:
                    return PixelFormat.BGRA8UNorm;

                case 24:
                    return PixelFormat.BGR8UNorm;

                default:
                    throw new NotSupportedException();
            }
        }

        #endregion WIN32

        #region OSX

        [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_getClass")]
        private static extern nint objc_getClass(byte* className);// wants a CChar*, CChar is a byte

        private static nint ObjCGetClass(ReadOnlySpan<byte> className)
        {
            fixed (byte* pClassName = className)
            {
                return objc_getClass(pClassName);
            }
        }

        [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "sel_registerName")]
        private static extern nint SelRegisterName(byte* selectorName); // wants a CChar*, CChar is a byte

        private static nint SelRegisterName(ReadOnlySpan<byte> selectorName)
        {
            fixed (byte* pSelectorName = selectorName)
            {
                return SelRegisterName(pSelectorName);
            }
        }

        [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
        private static extern nint ObjCMsgSend(nint receiver, nint selector, nint arg1);

        [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
        private static extern nint ObjCMsgSend(nint receiver, nint selector);

        [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
        private static extern nint CGImageGetWidth(nint image);

        [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
        private static extern nint CGImageGetHeight(nint image);

        [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
        private static extern nint CGColorSpaceCreateDeviceRGB();

        [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
        private static extern nint CGBitmapContextCreate(byte* data, int width, int height, int bitsPerComponent, int bytesPerRow, nint colorSpace, CGImageAlphaInfo bitmapInfo);

        [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
        private static extern void CGContextDrawImage(nint context, CGRect rect, nint image);

        [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
        private static extern void CGContextRelease(nint context);

        [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
        private static extern void CGColorSpaceRelease(nint colorSpace);

        private enum CGImageAlphaInfo : uint
        {
            kCGImageAlphaPremultipliedLast = 1
        }

        private struct CGRect
        {
            public double x;
            public double y;
            public double width;
            public double height;

            public CGRect(double x, double y, double width, double height)
            {
                this.x = x;
                this.y = y;
                this.width = width;
                this.height = height;
            }
        }

        private static byte* GetFileIconPixelDataOSX(string filePath, int size)
        {
            char* pFile = filePath.ToUTF16Ptr();  // NSString is a UTF-16 string

            nint workspace = ObjCMsgSend(ObjCGetClass("NSWorkspace"u8), SelRegisterName("sharedWorkspace"u8));
            nint icon = ObjCMsgSend(workspace, SelRegisterName("iconForFile:"u8), (nint)pFile);
            nint cgImage = ObjCMsgSend(icon, SelRegisterName("CGImage"u8));

            Free(pFile);

            var result = GetCGImagePixelData(cgImage, size);

            ObjCMsgSend(icon, SelRegisterName("release"u8));

            return result;
        }

        private static byte* GetCGImagePixelData(IntPtr cgImage, int size)
        {
            int width = (int)CGImageGetWidth(cgImage);
            int height = (int)CGImageGetHeight(cgImage);
            int bytesPerPixel = 4;
            int bytesPerRow = width * bytesPerPixel;

            byte* pixelData = AllocT<byte>(height * bytesPerRow);
            nint colorSpace = CGColorSpaceCreateDeviceRGB();
            nint context = CGBitmapContextCreate(pixelData, width, height, 8, bytesPerRow, colorSpace, CGImageAlphaInfo.kCGImageAlphaPremultipliedLast);

            CGContextDrawImage(context, new CGRect(0, 0, size, size), cgImage);

            CGContextRelease(context);
            CGColorSpaceRelease(colorSpace);

            return pixelData;
        }

        #endregion OSX

        #region Linux

        [DllImport("libgtk-3.so.0")]
        private static extern nint gtk_icon_theme_get_default();

        [DllImport("libgtk-3.so.0")]
        private static extern nint gtk_icon_theme_lookup_icon(nint icon_theme, nint icon_name, int size, int flags);

        [DllImport("libgtk-3.so.0")]
        private static extern nint gtk_icon_info_load_icon(nint icon_info, nint error);

        [DllImport("libgtk-3.so.0")]
        private static extern void g_object_unref(nint obj);

        [DllImport("libgio-2.0.so.0")]
        private static extern nint g_file_new_for_path(byte* path);

        [DllImport("libgio-2.0.so.0")]
        private static extern nint g_file_query_info(nint gfile, byte* attributes, int flags, nint cancellable, nint error);

        private static nint g_file_query_info(nint gfile, ReadOnlySpan<byte> attributes, int flags, nint cancellable, nint error)
        {
            fixed (byte* pAttributes = attributes)
            {
                return g_file_query_info(gfile, pAttributes, flags, cancellable, error);
            }
        }

        [DllImport("libgio-2.0.so.0")]
        private static extern nint g_file_info_get_icon(nint gfileinfo);

        [DllImport("libgio-2.0.so.0")]
        private static extern nint g_icon_to_string(nint icon);

        [DllImport("libgdk-3.so.0")]
        private static extern int gdk_pixbuf_get_width(nint pixbuf);

        [DllImport("libgdk-3.so.0")]
        private static extern int gdk_pixbuf_get_height(nint pixbuf);

        [DllImport("libgdk-3.so.0")]
        private static extern int gdk_pixbuf_get_rowstride(nint pixbuf);

        [DllImport("libgdk-3.so.0")]
        private static extern byte* gdk_pixbuf_get_pixels(nint pixbuf);

        [DllImport("libgdk-3.so.0")]
        private static extern int gdk_pixbuf_get_n_channels(nint pixbuf);

        private static byte* GetFileIconPixelDataLinux(string filePath, int size)
        {
            byte* pFile = filePath.ToUTF8Ptr();
            nint iconTheme = gtk_icon_theme_get_default();
            nint gFile = g_file_new_for_path(pFile);
            nint gFileInfo = g_file_query_info(gFile, "standard::icon"u8, 0, 0, 0);
            nint gIcon = g_file_info_get_icon(gFileInfo);
            nint iconName = g_icon_to_string(gIcon);
            nint iconInfo = gtk_icon_theme_lookup_icon(iconTheme, iconName, size, 0);
            nint pixbuf = gtk_icon_info_load_icon(iconInfo, 0);

            Free(pFile);

            byte* result = GetPixbufPixelData(pixbuf);

            // Release the allocated resources to avoid memory leaks
            g_object_unref(gFileInfo);
            g_object_unref(gIcon);
            g_object_unref(iconInfo);
            g_object_unref(pixbuf);

            return result;
        }

        private static byte* GetPixbufPixelData(IntPtr pixbuf)
        {
            int width = gdk_pixbuf_get_width(pixbuf);
            int height = gdk_pixbuf_get_height(pixbuf);
            int rowstride = gdk_pixbuf_get_rowstride(pixbuf);
            byte* pixels = gdk_pixbuf_get_pixels(pixbuf);
            int nChannels = gdk_pixbuf_get_n_channels(pixbuf);
            int size = height * rowstride;
            byte* pixelData = AllocT<byte>(size);
            Memcpy(pixels, pixelData, size);
            return pixelData;
        }

        #endregion Linux
    }
}