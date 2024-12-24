using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using InTheHand.Bluetooth;
using Model;

namespace GamingMode;

internal static class BluetoothSettings
{
    public const string JsonFile = "device.json";

    [RequiresUnreferencedCode("Calls BluetoothSettings.Program.SaveDeviceToJson(BluetoothDeviceInfo)")]
    [RequiresDynamicCode("Calls BluetoothSettings.Program.SaveDeviceToJson(BluetoothDeviceInfo)")]
    public static async Task Configure()
    {
        Console.WriteLine("Searching for Bluetooth devices...");
        var devices = await GetBluetoothDevices();
        if (devices.Count == 0)
        {
            Console.WriteLine("No Bluetooth devices found.");
            return;
        }

        Console.WriteLine("Available Bluetooth devices:");
        for (var i = 0; i < devices.Count; i++)
        {
            Console.WriteLine($"[{i}] {devices[i].Name} - {devices[i].Address}");
        }

        Console.Write("Select a device by entering its number: ");
        if (int.TryParse(Console.ReadLine(), out var selectedIndex) && selectedIndex >= 0 && selectedIndex < devices.Count)
        {
            var selectedDevice = devices[selectedIndex];
            SaveDeviceToJson(selectedDevice);
            Console.WriteLine("Device details saved to device.json.");
        }
        else
        {
            Console.WriteLine("Invalid selection.");
        }
    }

    static async Task<List<BluetoothDeviceInfo>> GetBluetoothDevices() => (await Bluetooth.GetPairedDevicesAsync())
        .Select(device => new BluetoothDeviceInfo { Name = device.Name, Address = device.Id.ToString() })
        .ToList();
    
    public static async Task<bool> IsBluetoothDeviceConnected(BluetoothDeviceInfo deviceInfo) => (await Bluetooth.GetPairedDevicesAsync())
        .Any(device => device.Id == deviceInfo.Address && device.Gatt.IsConnected);

    [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.Serialize<TValue>(TValue, JsonSerializerOptions)")]
    [RequiresDynamicCode("Calls System.Text.Json.JsonSerializer.Serialize<TValue>(TValue, JsonSerializerOptions)")]
    static void SaveDeviceToJson(BluetoothDeviceInfo device)
    {
        var json = JsonSerializer.Serialize(device, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(JsonFile, json);
    }
}