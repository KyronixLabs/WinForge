using System.IO;
using System.Text.Json;
using Microsoft.Win32;
using WinForge.Models;

namespace WinForge.Services;

public sealed class StartupService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public IReadOnlyList<StartupItem> LoadItems()
    {
        AppPaths.Ensure();
        var items = new Dictionary<string, StartupItem>(StringComparer.OrdinalIgnoreCase);
        var saved = LoadState().ToDictionary(x => x.Key, StringComparer.OrdinalIgnoreCase);

        foreach (var item in ReadRegistryItems(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Run", "Current user Run"))
        {
            item.IsEnabled = true;
            item.OriginalIsEnabled = true;
            items[item.Key] = item;
        }

        foreach (var item in ReadRegistryItems(Registry.LocalMachine, @"Software\Microsoft\Windows\CurrentVersion\Run", "All users Run"))
        {
            item.IsEnabled = true;
            item.OriginalIsEnabled = true;
            items[item.Key] = item;
        }

        foreach (var item in ReadStartupFolderItems(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "Current user Startup folder"))
        {
            item.IsEnabled = true;
            item.OriginalIsEnabled = true;
            items[item.Key] = item;
        }

        foreach (var item in ReadStartupFolderItems(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup), "All users Startup folder"))
        {
            item.IsEnabled = true;
            item.OriginalIsEnabled = true;
            items[item.Key] = item;
        }

        foreach (var savedItem in saved.Values)
        {
            if (!items.ContainsKey(savedItem.Key))
            {
                savedItem.IsEnabled = false;
                savedItem.OriginalIsEnabled = false;
                items[savedItem.Key] = savedItem;
            }
        }

        return items.Values
            .OrderBy(x => x.IsEnabled ? 0 : 1)
            .ThenBy(x => x.Source)
            .ThenBy(x => x.Name)
            .ToList();
    }

    public string ApplyChanges(IEnumerable<StartupItem> items)
    {
        AppPaths.Ensure();
        var state = LoadState();
        var stateMap = state.ToDictionary(x => x.Key, StringComparer.OrdinalIgnoreCase);
        var lines = new List<string>();

        foreach (var item in items)
        {
            if (item.IsEnabled == item.OriginalIsEnabled)
            {
                continue;
            }

            if (!item.IsEnabled)
            {
                DisableItem(item, stateMap, lines);
            }
            else
            {
                EnableItem(item, stateMap, lines);
            }
        }

        SaveState(stateMap.Values.OrderBy(x => x.Source).ThenBy(x => x.Name).ToList());
        return lines.Count == 0 ? "No startup changes were needed." : string.Join(Environment.NewLine, lines);
    }

    public string BuildReport(IEnumerable<StartupItem> items)
    {
        var lines = new List<string>
        {
            $"Captured: {DateTime.Now:g}",
            string.Empty
        };

        foreach (var item in items.OrderBy(x => x.Source).ThenBy(x => x.Name))
        {
            lines.Add($"{item.Name}");
            lines.Add($"  Enabled: {item.IsEnabled}");
            lines.Add($"  Source: {item.Source}");
            lines.Add($"  Location: {item.Location}");
            if (!string.IsNullOrWhiteSpace(item.Command))
            {
                lines.Add($"  Command: {item.Command}");
            }
            lines.Add(string.Empty);
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static IEnumerable<StartupItem> ReadRegistryItems(RegistryKey hive, string path, string source)
    {
        using var key = hive.OpenSubKey(path, writable: false);
        if (key == null)
        {
            yield break;
        }

        var hiveName = ReferenceEquals(hive, Registry.CurrentUser) ? "HKCU" : "HKLM";

        foreach (var valueName in key.GetValueNames())
        {
            var value = Convert.ToString(key.GetValue(valueName));
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            yield return new StartupItem
            {
                Key = $"REG|{hiveName}|{path}|{valueName}",
                Name = valueName,
                Source = source,
                Location = $"{hiveName}\\{path}",
                Command = value,
                Kind = "Registry",
                Hive = hiveName,
                RegistryPath = path,
                ValueName = valueName
            };
        }
    }

    private static IEnumerable<StartupItem> ReadStartupFolderItems(string folderPath, string source)
    {
        if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
        {
            yield break;
        }

        foreach (var file in Directory.GetFiles(folderPath))
        {
            var name = Path.GetFileNameWithoutExtension(file);
            yield return new StartupItem
            {
                Key = $"FILE|{folderPath}|{Path.GetFileName(file)}",
                Name = name,
                Source = source,
                Location = folderPath,
                Command = file,
                Kind = "StartupFolder",
                OriginalPath = file
            };
        }
    }

    private static void DisableItem(StartupItem item, Dictionary<string, StartupItem> stateMap, List<string> lines)
    {
        if (item.Kind == "Registry")
        {
            using var key = OpenRegistryKey(item.Hive, item.RegistryPath, writable: true);
            if (key == null)
            {
                lines.Add($"Could not open {item.Location} for {item.Name}.");
                return;
            }

            var currentValue = Convert.ToString(key.GetValue(item.ValueName));
            if (string.IsNullOrWhiteSpace(currentValue))
            {
                lines.Add($"{item.Name} was already disabled.");
                return;
            }

            item.Command = currentValue;
            key.DeleteValue(item.ValueName, false);
            stateMap[item.Key] = Clone(item, false);
            lines.Add($"Disabled startup entry: {item.Name}");
            return;
        }

        if (item.Kind == "StartupFolder")
        {
            var originalPath = item.OriginalPath;
            if (string.IsNullOrWhiteSpace(originalPath) || !File.Exists(originalPath))
            {
                lines.Add($"Startup file was not found for {item.Name}.");
                return;
            }

            var disabledFolder = Path.Combine(AppPaths.StartupDisabled, MakeSafeFolderName(item.Source));
            Directory.CreateDirectory(disabledFolder);
            var disabledPath = Path.Combine(disabledFolder, Path.GetFileName(originalPath));
            if (File.Exists(disabledPath))
            {
                disabledPath = Path.Combine(disabledFolder, $"{DateTime.Now:yyyyMMddHHmmss}_{Path.GetFileName(originalPath)}");
            }

            File.Move(originalPath, disabledPath);
            item.DisabledPath = disabledPath;
            stateMap[item.Key] = Clone(item, false);
            lines.Add($"Disabled startup file: {item.Name}");
        }
    }

    private static void EnableItem(StartupItem item, Dictionary<string, StartupItem> stateMap, List<string> lines)
    {
        if (!stateMap.TryGetValue(item.Key, out var saved))
        {
            lines.Add($"No saved startup state was found for {item.Name}.");
            return;
        }

        if (saved.Kind == "Registry")
        {
            using var key = OpenRegistryKey(saved.Hive, saved.RegistryPath, writable: true);
            if (key == null)
            {
                lines.Add($"Could not open {saved.Location} for {saved.Name}.");
                return;
            }

            key.SetValue(saved.ValueName, saved.Command);
            stateMap.Remove(saved.Key);
            lines.Add($"Enabled startup entry: {saved.Name}");
            return;
        }

        if (saved.Kind == "StartupFolder")
        {
            if (string.IsNullOrWhiteSpace(saved.DisabledPath) || !File.Exists(saved.DisabledPath))
            {
                lines.Add($"Saved startup file could not be found for {saved.Name}.");
                return;
            }

            var destination = saved.OriginalPath;
            if (string.IsNullOrWhiteSpace(destination))
            {
                lines.Add($"Original startup location is missing for {saved.Name}.");
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            if (File.Exists(destination))
            {
                File.Delete(destination);
            }

            File.Move(saved.DisabledPath, destination);
            stateMap.Remove(saved.Key);
            lines.Add($"Enabled startup file: {saved.Name}");
        }
    }

    private static RegistryKey? OpenRegistryKey(string hive, string path, bool writable)
    {
        return hive.Equals("HKCU", StringComparison.OrdinalIgnoreCase)
            ? Registry.CurrentUser.OpenSubKey(path, writable)
            : Registry.LocalMachine.OpenSubKey(path, writable);
    }

    private static string MakeSafeFolderName(string input)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var chars = input.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray();
        return new string(chars);
    }

    private static StartupItem Clone(StartupItem source, bool enabled)
    {
        return new StartupItem
        {
            Key = source.Key,
            Name = source.Name,
            Source = source.Source,
            Location = source.Location,
            Command = source.Command,
            Kind = source.Kind,
            Hive = source.Hive,
            RegistryPath = source.RegistryPath,
            ValueName = source.ValueName,
            OriginalPath = source.OriginalPath,
            DisabledPath = source.DisabledPath,
            IsEnabled = enabled,
            OriginalIsEnabled = enabled
        };
    }

    private static List<StartupItem> LoadState()
    {
        try
        {
            if (File.Exists(AppPaths.StartupStateFile))
            {
                var json = File.ReadAllText(AppPaths.StartupStateFile);
                return JsonSerializer.Deserialize<List<StartupItem>>(json, JsonOptions) ?? new List<StartupItem>();
            }
        }
        catch
        {
        }

        return new List<StartupItem>();
    }

    private static void SaveState(List<StartupItem> items)
    {
        var json = JsonSerializer.Serialize(items, JsonOptions);
        File.WriteAllText(AppPaths.StartupStateFile, json);
    }
}
