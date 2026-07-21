using System.Runtime.InteropServices;

namespace RenderNorth.DisplaySwitcher.Native;

internal static class NativeMethods
{
    internal const uint QDC_ONLY_ACTIVE_PATHS = 0x2;
    internal const uint QDC_VIRTUAL_MODE_AWARE = 0x10;
    internal const uint SDC_USE_SUPPLIED_DISPLAY_CONFIG = 0x20;
    internal const uint SDC_APPLY = 0x80;
    internal const uint SDC_SAVE_TO_DATABASE = 0x200;
    internal const uint SDC_ALLOW_CHANGES = 0x400;
    internal const uint SDC_VIRTUAL_MODE_AWARE = 0x8000;
    internal const int ERROR_INSUFFICIENT_BUFFER = 122;
    internal const int ERROR_SUCCESS = 0;

    [DllImport("user32.dll")]
    internal static extern int GetDisplayConfigBufferSizes(uint flags, out uint pathCount, out uint modeCount);

    [DllImport("user32.dll")]
    internal static extern int QueryDisplayConfig(uint flags, ref uint pathCount,
        [Out] DisplayConfigPathInfo[] paths, ref uint modeCount, [Out] DisplayConfigModeInfo[] modes,
        IntPtr currentTopologyId);

    [DllImport("user32.dll")]
    internal static extern int SetDisplayConfig(uint pathCount, [In] DisplayConfigPathInfo[] paths,
        uint modeCount, [In] DisplayConfigModeInfo[] modes, uint flags);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    internal static extern int DisplayConfigGetDeviceInfo(ref DisplayConfigTargetDeviceName requestPacket);
}

[StructLayout(LayoutKind.Sequential)]
internal struct Luid { public uint LowPart; public int HighPart; public readonly long Value => ((long)HighPart << 32) | LowPart; }

[StructLayout(LayoutKind.Sequential)]
internal struct DisplayConfigRational { public uint Numerator; public uint Denominator; }

internal enum DisplayConfigModeInfoType : uint { Source = 1, Target = 2, DesktopImage = 3 }
internal enum DisplayConfigRotation : uint { Identity = 1, Rotate90, Rotate180, Rotate270 }
internal enum DisplayConfigScaling : uint { Identity = 1, Centered, Stretched, AspectRatioCenteredMax, Custom, Preferred }
internal enum DisplayConfigScanLineOrdering : uint { Unspecified, Progressive, Interlaced, InterlacedUpperFieldFirst, InterlacedLowerFieldFirst }
internal enum DisplayConfigVideoOutputTechnology : uint { Other = 0xFFFFFFFF, Hd15 = 0, SVideo, CompositeVideo, ComponentVideo, Dvi, Hdmi, Lvds, Djpn, Sdi, DisplayPortExternal, DisplayPortEmbedded, UdiExternal, UdiEmbedded, SdtvDongle, Miracast, IndirectWired, IndirectVirtual }

[StructLayout(LayoutKind.Sequential)]
internal struct DisplayConfigPathSourceInfo
{
    public Luid AdapterId; public uint Id; public uint ModeInfoIdx; public uint StatusFlags;
}

[StructLayout(LayoutKind.Sequential)]
internal struct DisplayConfigPathTargetInfo
{
    public Luid AdapterId; public uint Id; public uint ModeInfoIdx; public DisplayConfigVideoOutputTechnology OutputTechnology;
    public DisplayConfigRotation Rotation; public DisplayConfigScaling Scaling; public DisplayConfigRational RefreshRate;
    public DisplayConfigScanLineOrdering ScanLineOrdering;
    [MarshalAs(UnmanagedType.Bool)] public bool TargetAvailable;
    public uint StatusFlags;
}

[StructLayout(LayoutKind.Sequential)]
internal struct DisplayConfigPathInfo
{
    public DisplayConfigPathSourceInfo SourceInfo; public DisplayConfigPathTargetInfo TargetInfo; public uint Flags;
}

[StructLayout(LayoutKind.Sequential)]
internal struct PointL { public int X; public int Y; }

[StructLayout(LayoutKind.Sequential)]
internal struct DisplayConfig2DRegion { public uint Cx; public uint Cy; }

[StructLayout(LayoutKind.Sequential)]
internal struct DisplayConfigVideoSignalInfo
{
    public ulong PixelRate; public DisplayConfigRational HSyncFreq; public DisplayConfigRational VSyncFreq;
    public DisplayConfig2DRegion ActiveSize; public DisplayConfig2DRegion TotalSize; public uint VideoStandard; public DisplayConfigScanLineOrdering ScanLineOrdering;
}

[StructLayout(LayoutKind.Sequential)]
internal struct DisplayConfigTargetMode { public DisplayConfigVideoSignalInfo TargetVideoSignalInfo; }

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal struct DisplayConfigSourceMode
{
    public uint Width; public uint Height; public uint PixelFormat; public PointL Position;
}

[StructLayout(LayoutKind.Sequential)]
internal struct RectL { public int Left; public int Top; public int Right; public int Bottom; }

[StructLayout(LayoutKind.Sequential)]
internal struct DisplayConfigDesktopImageInfo { public PointL PathSourceSize; public RectL DesktopImageRegion; public RectL DesktopImageClip; }

[StructLayout(LayoutKind.Explicit)]
internal struct DisplayConfigModeUnion
{
    [FieldOffset(0)] public DisplayConfigTargetMode TargetMode;
    [FieldOffset(0)] public DisplayConfigSourceMode SourceMode;
    [FieldOffset(0)] public DisplayConfigDesktopImageInfo DesktopImageInfo;
}

[StructLayout(LayoutKind.Sequential)]
internal struct DisplayConfigModeInfo
{
    public DisplayConfigModeInfoType InfoType; public uint Id; public Luid AdapterId; public DisplayConfigModeUnion Mode;
}

[StructLayout(LayoutKind.Sequential)]
internal struct DisplayConfigDeviceInfoHeader
{
    public uint Type; public uint Size; public Luid AdapterId; public uint Id;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal struct DisplayConfigTargetDeviceName
{
    public DisplayConfigDeviceInfoHeader Header; public uint Flags; public DisplayConfigVideoOutputTechnology OutputTechnology;
    public ushort EdidManufactureId; public ushort EdidProductCodeId; public uint ConnectorInstance;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)] public string MonitorFriendlyDeviceName;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string MonitorDevicePath;
}
