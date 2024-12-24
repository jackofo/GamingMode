using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows.Forms;
using Model;

namespace GamingMode;

internal static class DisplaySettings
{
    public const string JsonFile = "displaySettings.json";
    const int DM_DISPLAYFREQUENCY = 0x00400000;
    const uint CDS_UPDATEREGISTRY = 0x01;
    const int WM_COMMAND = 0x111;
    const int MIN_ALL = 419;

    [DllImport("user32.dll")]
    static extern bool EnumDisplaySettings(string lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode);

    [DllImport("user32.dll")]
    static extern bool EnumDisplayDevices(string lpDevice, int iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, int dwFlags);

    [DllImport("user32.dll")]
    static extern DISP_CHANGE ChangeDisplaySettingsEx(string lpszDeviceName, ref DEVMODE lpDevMode, IntPtr hwnd, uint dwFlags, IntPtr lParam);
    
    [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
    static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
    [DllImport("user32.dll", EntryPoint = "SendMessage", SetLastError = true)]
    static extern IntPtr SendMessage(IntPtr hWnd, Int32 Msg, IntPtr wParam, IntPtr lParam);

    public static void MinimalizeAll() {
        IntPtr lHwnd = FindWindow("Shell_TrayWnd", null);
        SendMessage(lHwnd, WM_COMMAND, (IntPtr)MIN_ALL, IntPtr.Zero);
    }

    static List<(int Width, int Height)> GetAvailableResolutions(string deviceName)
    {
        var resolutions = new HashSet<(int, int)>();
        var devMode = new DEVMODE
        {
            dmSize = (short)Marshal.SizeOf(typeof(DEVMODE))
        };
        var modeNum = 0;
        while (EnumDisplaySettings(deviceName, modeNum++, ref devMode))
        {
            resolutions.Add((devMode.dmPelsWidth, devMode.dmPelsHeight));
        }

        var sortedResolutions = new List<(int, int)>(resolutions);
        sortedResolutions.Sort((a, b) => b.Item1.CompareTo(a.Item1));
        return sortedResolutions;
    }

    static List<int> GetAvailableRefreshRates(string deviceName, int width, int height)
    {
        var refreshRates = new HashSet<int>();
        var devMode = new DEVMODE
        {
            dmSize = (short)Marshal.SizeOf(typeof(DEVMODE))
        };
        var modeNum = 0;
        while (EnumDisplaySettings(deviceName, modeNum++, ref devMode))
        {
            if (devMode.dmPelsWidth == width && devMode.dmPelsHeight == height)
            {
                refreshRates.Add(devMode.dmDisplayFrequency);
            }
        }

        var sortedRefreshRates = new List<int>(refreshRates);
        sortedRefreshRates.Sort();
        return sortedRefreshRates;
    }
    
    public static bool SetDisplaySettings(DisplayInfo info)
    {
        var devMode = new DEVMODE
        {
            dmSize = (short)Marshal.SizeOf(typeof(DEVMODE)),
            dmDeviceName = info.Display,
            dmPelsWidth = info.Resolution.Width,
            dmPelsHeight = info.Resolution.Height,
            dmDisplayFrequency = info.RefreshRate,
            dmFields = DM.PELSWIDTH | DM.PELSHEIGHT | DM.DISPLAYFREQUENCY
        };
        
        var device = new DISPLAY_DEVICE(0);
        var adapters = Screen.AllScreens.Select(s => s.DeviceName).ToArray();
        foreach (var adapter in adapters)
        {
            var x = 0;
            while (EnumDisplayDevices(adapter, x, ref device, 0))
            {
                if (device.DeviceID == info.Display)
                {
                    devMode.dmDeviceName = device.DeviceName;
                    return ChangeDisplaySettingsEx(adapter, ref devMode, IntPtr.Zero, CDS_UPDATEREGISTRY, IntPtr.Zero) == DISP_CHANGE.SUCCESSFUL;
                }
                x++;
            }
        }

        return false;

    }

    public static void Configure()
    {
        Console.WriteLine("Connected Displays:");
        var displays = new List<(DISPLAY_DEVICE device, string adapter)>();
        var device = new DISPLAY_DEVICE(0);
        var adapters = Screen.AllScreens.Select(s => s.DeviceName).ToArray();
        for (var i = 0; i < adapters.Length; ++i)
        {
            var x = 0;
            while (EnumDisplayDevices(adapters[i], x, ref device, 0))
            {
                Console.WriteLine($"{x + i + 1}. {device.DeviceString}");
                displays.Add((device, adapters[i]));
                x++;
            }
        }

        Console.Write("Enter the number of the display you want to configure: ");
        var displayChoice = int.Parse(Console.ReadLine() ?? string.Empty) - 1;
        if (displayChoice < 0 || displayChoice >= displays.Count)
        {
            Console.WriteLine("Invalid display choice.");
            return;
        }

        var selectedScreen = displays[displayChoice];
        Console.WriteLine($"You selected: {selectedScreen.device.DeviceString}");
        var resolutions = GetAvailableResolutions(selectedScreen.adapter);
        Console.WriteLine("Available Resolutions:");
        for (var i = 0; i < resolutions.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {resolutions[i].Width}x{resolutions[i].Height}");
        }

        Console.Write("Enter the number of the resolution you want to select: ");
        var resolutionChoice = int.Parse(Console.ReadLine() ?? string.Empty) - 1;
        if (resolutionChoice < 0 || resolutionChoice >= resolutions.Count)
        {
            Console.WriteLine("Invalid resolution choice.");
            return;
        }

        var selectedResolution = resolutions[resolutionChoice];
        Console.WriteLine($"You selected resolution: {selectedResolution.Width}x{selectedResolution.Height}");
        var refreshRates = GetAvailableRefreshRates(selectedScreen.adapter, selectedResolution.Width, selectedResolution.Height);
        Console.WriteLine("Available Refresh Rates:");
        for (var i = 0; i < refreshRates.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {refreshRates[i]} Hz");
        }

        Console.Write("Enter the number of the refresh rate you want to select: ");
        var refreshRateChoice = int.Parse(Console.ReadLine() ?? string.Empty) - 1;
        if (refreshRateChoice < 0 || refreshRateChoice >= refreshRates.Count)
        {
            Console.WriteLine("Invalid refresh rate choice.");
            return;
        }

        var selectedRefreshRate = refreshRates[refreshRateChoice];
        Console.WriteLine($"You selected refresh rate: {selectedRefreshRate} Hz");
        var result = new DisplayInfo
        {
            Display = selectedScreen.device.DeviceID, 
            Resolution = new Resolution
            {
                Width = selectedResolution.Width, 
                Height = selectedResolution.Height,
            }, 
            RefreshRate = selectedRefreshRate,
        };
        
        var json = JsonSerializer.Serialize(result);
        File.WriteAllText(JsonFile, json);
        Console.WriteLine("Settings saved to displaySettings.json.");
    }
}