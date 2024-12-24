using System.Diagnostics;
using System.Text.Json;
using Model;

namespace GamingMode;

internal static class Program
{
    static async Task Main(string[] args)
    {
        if (File.Exists(BluetoothSettings.JsonFile) == false)
        {
            await BluetoothSettings.Configure();
        }

        if (File.Exists(DisplaySettings.JsonFile) == false)
        {
            DisplaySettings.Configure();
        }
        
        var deviceJson = await File.ReadAllTextAsync(BluetoothSettings.JsonFile);
        var deviceInfo = JsonSerializer.Deserialize<BluetoothDeviceInfo>(deviceJson);
        var displayJson = await File.ReadAllTextAsync(DisplaySettings.JsonFile);
        var displaySettings = JsonSerializer.Deserialize<DisplayInfo>(displayJson);
        Console.WriteLine("Looking for Bluetooth device...");
        while (true)
        {
            if (await BluetoothSettings.IsBluetoothDeviceConnected(deviceInfo))
            {
                Console.WriteLine("Device connected! Setting display...");
                if (DisplaySettings.SetDisplaySettings(displaySettings))
                {
                    Console.WriteLine("Display settings applied successfully.");
                }
                else
                {
                    Console.WriteLine("Failed to apply display settings.");
                }

                Console.WriteLine("Minimalize all to desktop...");
                DisplaySettings.MinimalizeAll();

                Console.WriteLine("Launching Steam in Big Picture mode...");
                LaunchSteamBigPicture();

                break;
            }

            Thread.Sleep(5000);
        }
    }
    
    static void LaunchSteamBigPicture()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "steam://open/bigpicture",
                UseShellExecute = true,
            };
            Process.Start(psi);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to launch Steam: {ex.Message}");
        }
    }
}