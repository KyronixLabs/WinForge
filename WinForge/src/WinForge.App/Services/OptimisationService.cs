using System.IO;
using System.Text.Json;
using WinForge.Models;

namespace WinForge.Services;

public sealed class OptimisationService
{
    private readonly CommandService _commands = new();
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public List<OptimisationOption> Options { get; } = OptimisationCatalog.CreateDefaultOptions();

    public async Task<string> ApplySelectedAsync(CancellationToken cancellationToken = default)
    {
        AppPaths.Ensure();

        var selected = Options.Where(option => option.IsSelected).ToList();
        if (selected.Count == 0)
        {
            return "No optimisation options were selected.";
        }

        var log = new List<string>
        {
            $"WinForge run started at {DateTime.Now:g}",
            $"Selected options: {selected.Count}",
            string.Empty
        };

        await TryCreateRestorePointAsync(log, cancellationToken);

        var run = new OptimisationRun();

        foreach (var option in selected)
        {
            log.Add($"Applying: {option.Title}");
            log.Add(option.Summary);

            try
            {
                var output = await _commands.RunPowerShellAsync(option.ApplyScript, cancellationToken);
                log.Add(output.Trim());
                run.AppliedOptions.Add(new AppliedOption
                {
                    Id = option.Id,
                    Title = option.Title,
                    RevertScript = option.RevertScript
                });
            }
            catch (Exception ex)
            {
                log.Add($"Failed: {ex.Message}");
            }

            log.Add(string.Empty);
        }

        var snapshotPath = SaveRun(run);
        log.Add($"Snapshot saved: {snapshotPath}");

        var text = string.Join(Environment.NewLine, log);
        SaveLog(text, "optimise");
        return text;
    }

    public async Task<string> RevertLastAsync(CancellationToken cancellationToken = default)
    {
        AppPaths.Ensure();

        var snapshot = GetLatestSnapshot();
        if (snapshot is null)
        {
            return "No WinForge snapshot was found to revert.";
        }

        var run = JsonSerializer.Deserialize<OptimisationRun>(await File.ReadAllTextAsync(snapshot, cancellationToken));
        if (run is null || run.AppliedOptions.Count == 0)
        {
            return "The latest snapshot does not contain any applied options.";
        }

        var log = new List<string>
        {
            $"WinForge revert started at {DateTime.Now:g}",
            $"Snapshot: {snapshot}",
            string.Empty
        };

        foreach (var option in run.AppliedOptions.AsEnumerable().Reverse())
        {
            if (string.IsNullOrWhiteSpace(option.RevertScript))
            {
                log.Add($"Skipped: {option.Title}");
                log.Add("This action does not have an automatic revert script.");
                log.Add(string.Empty);
                continue;
            }

            log.Add($"Reverting: {option.Title}");

            try
            {
                var output = await _commands.RunPowerShellAsync(option.RevertScript, cancellationToken);
                log.Add(output.Trim());
            }
            catch (Exception ex)
            {
                log.Add($"Failed: {ex.Message}");
            }

            log.Add(string.Empty);
        }

        var text = string.Join(Environment.NewLine, log);
        SaveLog(text, "revert");
        return text;
    }


    public IReadOnlyList<SnapshotInfo> GetSnapshots()
    {
        AppPaths.Ensure();
        return Directory.GetFiles(AppPaths.Snapshots, "winforge-snapshot-*.json")
            .OrderByDescending(File.GetLastWriteTime)
            .Select(path =>
            {
                var count = 0;
                try
                {
                    var json = File.ReadAllText(path);
                    count = JsonSerializer.Deserialize<OptimisationRun>(json, JsonOptions)?.AppliedOptions.Count ?? 0;
                }
                catch
                {
                }

                return new SnapshotInfo
                {
                    Name = Path.GetFileName(path),
                    Path = path,
                    CreatedAt = File.GetLastWriteTime(path),
                    AppliedCount = count
                };
            })
            .ToList();
    }

    public async Task<string> RevertSnapshotAsync(string snapshot, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(snapshot) || !File.Exists(snapshot))
        {
            return "Snapshot was not found.";
        }

        var run = JsonSerializer.Deserialize<OptimisationRun>(await File.ReadAllTextAsync(snapshot, cancellationToken));
        if (run is null || run.AppliedOptions.Count == 0)
        {
            return "The selected snapshot does not contain any applied options.";
        }

        var log = new List<string>
        {
            $"WinForge revert started at {DateTime.Now:g}",
            $"Snapshot: {snapshot}",
            string.Empty
        };

        foreach (var option in run.AppliedOptions.AsEnumerable().Reverse())
        {
            if (string.IsNullOrWhiteSpace(option.RevertScript))
            {
                log.Add($"Skipped: {option.Title}");
                log.Add("This action does not have an automatic revert script.");
                log.Add(string.Empty);
                continue;
            }

            log.Add($"Reverting: {option.Title}");

            try
            {
                var output = await _commands.RunPowerShellAsync(option.RevertScript, cancellationToken);
                log.Add(output.Trim());
            }
            catch (Exception ex)
            {
                log.Add($"Failed: {ex.Message}");
            }

            log.Add(string.Empty);
        }

        var text = string.Join(Environment.NewLine, log);
        SaveLog(text, "revert");
        return text;
    }

    public void SelectProfile(string profile)
    {
        foreach (var option in Options)
        {
            option.IsSelected = profile switch
            {
                "Safe" => option.Id is "cleanup-user-temp" or "cleanup-windows-temp" or "network-flush-dns" or "security-enable-firewall" or "security-defender-realtime",
                "Gaming" => option.Category is "Gaming" || option.Id is "cleanup-user-temp" or "network-flush-dns" or "power-disable-usb-suspend" or "power-disable-pcie-saving",
                "LowEnd" => option.Id is "cleanup-user-temp" or "cleanup-windows-temp" or "visual-disable-transparency" or "network-flush-dns" or "security-enable-firewall",
                "Server" => option.Id is "power-disable-usb-suspend" or "power-disable-pcie-saving" or "network-private-profile" or "security-enable-firewall" or "security-defender-realtime",
                "Privacy" => option.Category is "Privacy" || option.Id is "security-enable-firewall" or "security-defender-realtime",
                _ => option.IsSelected
            };
        }
    }


    public string SaveCurrentProfile(string name)
    {
        AppPaths.Ensure();
        var safeName = string.Join("_", name.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries)).Trim();
        if (string.IsNullOrWhiteSpace(safeName))
        {
            safeName = "CustomProfile";
        }

        var path = Path.Combine(AppPaths.Profiles, safeName + ".json");
        var selected = Options.Where(option => option.IsSelected).Select(option => option.Id).ToList();
        File.WriteAllText(path, JsonSerializer.Serialize(selected, JsonOptions));
        return path;
    }

    public IReadOnlyList<string> GetSavedProfiles()
    {
        AppPaths.Ensure();
        return Directory.GetFiles(AppPaths.Profiles, "*.json")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name!)
            .OrderBy(name => name)
            .ToList();
    }

    public bool LoadSavedProfile(string name)
    {
        AppPaths.Ensure();
        var path = Path.Combine(AppPaths.Profiles, name + ".json");
        if (!File.Exists(path))
        {
            return false;
        }

        var ids = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(path), JsonOptions) ?? new List<string>();
        var selected = ids.ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var option in Options)
        {
            option.IsSelected = selected.Contains(option.Id);
        }

        return true;
    }

    private async Task TryCreateRestorePointAsync(List<string> log, CancellationToken cancellationToken)
    {
        const string script = """
$ErrorActionPreference = 'Continue'
Checkpoint-Computer -Description 'WinForge before optimisation' -RestorePointType 'MODIFY_SETTINGS'
Write-Output 'System restore point request completed.'
""";

        try
        {
            var output = await _commands.RunPowerShellAsync(script, cancellationToken);
            log.Add(output.Trim());
        }
        catch (Exception ex)
        {
            log.Add($"Restore point request failed: {ex.Message}");
        }

        log.Add(string.Empty);
    }

    private static string SaveRun(OptimisationRun run)
    {
        var path = Path.Combine(AppPaths.Snapshots, $"winforge-snapshot-{DateTime.Now:yyyyMMdd-HHmmss}.json");
        var json = JsonSerializer.Serialize(run, JsonOptions);
        File.WriteAllText(path, json);
        return path;
    }

    private static string? GetLatestSnapshot()
    {
        return Directory.GetFiles(AppPaths.Snapshots, "winforge-snapshot-*.json")
            .OrderByDescending(File.GetLastWriteTime)
            .FirstOrDefault();
    }

    private static void SaveLog(string text, string prefix)
    {
        var path = Path.Combine(AppPaths.Logs, $"{prefix}-{DateTime.Now:yyyyMMdd-HHmmss}.log");
        File.WriteAllText(path, text);
    }
}
