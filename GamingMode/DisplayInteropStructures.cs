using System.Runtime.InteropServices;

namespace GamingMode;

[StructLayout(LayoutKind.Sequential)]
struct DISPLAY_DEVICE
{
    public int cb;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string DeviceName;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string DeviceString;

    public int StateFlags;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string DeviceID;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string DeviceKey;

    public DISPLAY_DEVICE(int flags)
    {
        cb = 0;
        StateFlags = flags;
        DeviceName = new string((char)32, 32);
        DeviceString = new string((char)32, 128);
        DeviceID = new string((char)32, 128);
        DeviceKey = new string((char)32, 128);
        cb = Marshal.SizeOf(this);
    }
}


enum DISP_CHANGE
{
    SUCCESSFUL = 0,
    RESTART = 1,
    FAILED = -1
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
struct DEVMODE
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string dmDeviceName;

    public short dmSpecVersion;
    public short dmDriverVersion;
    public short dmSize;
    public short dmDriverExtra;
    public int dmFields;
    public int dmPositionX;
    public int dmPositionY;
    public int dmDisplayOrientation;
    public int dmDisplayFixedOutput;
    public short dmColor;
    public short dmDuplex;
    public short dmYResolution;
    public short dmTTOption;
    public short dmCollate;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string dmFormName;

    public short dmLogPixels;
    public int dmBitsPerPel;
    public int dmPelsWidth;
    public int dmPelsHeight;
    public int dmDisplayFlags;
    public int dmDisplayFrequency;
    public int dmICMMethod;
    public int dmICMIntent;
    public int dmMediaType;
    public int dmDitherType;
    public int dmReserved1;
    public int dmReserved2;
    public int dmPanningWidth;
    public int dmPanningHeight;
}

struct DM
{
    public const int PELSWIDTH = 0x80000;
    public const int PELSHEIGHT = 0x100000;
    public const int DISPLAYFREQUENCY = 0x400000;
}